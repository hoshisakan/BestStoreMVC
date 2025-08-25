using BestStoreMVC.Models;
using BestStoreMVC.Services;
using BestStoreMVC.Services.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace BestStoreMVC.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private string PaypalClientId { get; set; } = "";
        private string PayPalSecret { get; set; } = "";
        private string PaypalUrl { get; set; } = "";

        private readonly decimal shippingFee;
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;


        public CheckoutController(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            PaypalClientId = configuration["PaypalSettings:ClientId"]!;
            PayPalSecret = configuration["PaypalSettings:Secret"]!;
            PaypalUrl = configuration["PaypalSettings:Url"]!;
            shippingFee = configuration.GetValue<decimal>("CartSetting:ShippingFee");
            this.context = context;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal total = CartHelper.GetSubtotal(cartItems);

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            TempData.Keep();

            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.Total = total;
            ViewBag.PaypalClientId = PaypalClientId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal totalAmount = CartHelper.GetSubtotal(cartItems) + shippingFee;

            // 創建請求正文
            JsonObject createOrderRequest = new JsonObject();
            createOrderRequest["intent"] = "CAPTURE";

            JsonObject amount = new JsonObject();
            amount["currency_code"] = "USD";
            amount["value"] = totalAmount;

            JsonObject purchaseUnit1 = new JsonObject();
            purchaseUnit1["amount"] = amount;

            JsonArray purchaseUnits = new JsonArray();
            purchaseUnits.Add(purchaseUnit1);

            createOrderRequest["purchase_units"] = purchaseUnits;

            // 取得 Access Token
            string accessToken = await GetPayPalAccessToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                return new JsonResult(new { Id = "" });
            }

            string url = PaypalUrl + "/v2/checkout/orders";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToJsonString(), null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse != null)
                    {
                        var paypalOrderId = jsonResponse["id"]?.ToString() ?? "";
                        return new JsonResult(new { Id = paypalOrderId });
                    }
                }
            }

            return new JsonResult(new { Id = "" });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder([FromBody] JsonObject data)
        {
            var orderId = data["orderID"]?.ToString();
            var deliveryAddress = data?["deliveryAddress"]?.ToString();

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(deliveryAddress))
            {
                return new JsonResult("error");
            }

            // 取得 Access Token
            string accessToken = await GetPayPalAccessToken();

            if (string.IsNullOrEmpty(accessToken))
            {
                return new JsonResult("error");
            }

            string url = PaypalUrl + $"/v2/checkout/orders/{orderId}/capture";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        var paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
                        if (paypalOrderStatus == "COMPLETED")
                        {
                            // 建立訂單，並存入資料庫
                            await SaveOrder(jsonResponse.ToJsonString(), deliveryAddress);

                            return new JsonResult("success");
                        }
                    }
                }
            }

            return new JsonResult("error");
        }

        private async Task SaveOrder(string paypalResponse, string deliveryAddress)
        {
            // 取得購物車內容
            var cartItems = CartHelper.GetCartItems(Request, Response, context);

            // 取得目前使用者
            var appUser = await userManager.GetUserAsync(User);

            if (appUser == null)
            {
                return;
            }

            // 建立訂單
            var order = new Order
            {
                ClientId = appUser.Id,
                Items = cartItems,
                ShippingFee = shippingFee,
                DeliveryAddress = deliveryAddress,
                PaymentMethod = "paypal",
                PaymentStatus = "accepted",
                PaymentDetails = paypalResponse,
                OrderStatus = "pending",
                CreatedAt = DateTime.Now
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // 清空購物車
            Response.Cookies.Delete("shopping_cart");
        }

        /// <summary>
        /// 測試取得 Access Token
        /// </summary>
        /// <returns></returns>
        //public async Task<string> Token()
        //{
        //    return await GetPayPalAccessToken();
        //}

        private async Task<string> GetPayPalAccessToken()
        {
            string accessToken = "";

            string url = PaypalUrl + "/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{PaypalClientId}:{PayPalSecret}"));
                client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials64}");

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", null, "application/x-www-form-urlencoded");

                var httpResponse = await client.SendAsync(requestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();

                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse != null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }
            }

            return accessToken;
        }
    }
}
