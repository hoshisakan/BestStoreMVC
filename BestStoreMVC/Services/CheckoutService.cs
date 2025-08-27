using BestStoreMVC.Models;
using BestStoreMVC.Services.Helper;
using BestStoreMVC.Services.Repository;
using System.Text.Json.Nodes;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 結帳業務邏輯實作類別
    /// 實作所有與結帳相關的業務邏輯操作
    /// </summary>
    public class CheckoutService : ICheckoutService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;
        
        // PayPal 設定
        private readonly string _paypalClientId;
        private readonly string _paypalSecret;
        private readonly string _paypalUrl;
        
        // 運費設定
        private readonly decimal _shippingFee;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        /// <param name="configuration">設定檔</param>
        public CheckoutService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            
            // 從設定檔取得 PayPal 設定
            _paypalClientId = configuration["PaypalSettings:ClientId"] ?? "";
            _paypalSecret = configuration["PaypalSettings:Secret"] ?? "";
            _paypalUrl = configuration["PaypalSettings:Url"] ?? "https://api-m.sandbox.paypal.com";
            
            // 從設定檔取得運費
            _shippingFee = configuration.GetValue<decimal>("CartSetting:ShippingFee");
        }

        /// <summary>
        /// 取得結帳頁面資料
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <returns>結帳頁面資料</returns>
        public (List<OrderItem> CartItems, decimal Total, string PaypalClientId) GetCheckoutData(HttpRequest request, HttpResponse response, string deliveryAddress)
        {
            // 透過 CartHelper 取得購物車項目清單
            var cartItems = CartHelper.GetCartItems(request, response, _unitOfWork.Context);
            
            // 計算總金額（小計 + 運費）
            var total = CartHelper.GetSubtotal(cartItems) + _shippingFee;

            // 回傳結帳頁面資料
            return (cartItems, total, _paypalClientId);
        }

        /// <summary>
        /// 建立 PayPal 訂單
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <returns>PayPal 訂單 ID</returns>
        public async Task<string> CreatePayPalOrderAsync(HttpRequest request, HttpResponse response)
        {
            // 取得購物車項目清單
            var cartItems = CartHelper.GetCartItems(request, response, _unitOfWork.Context);
            
            // 計算總金額（小計 + 運費）
            var totalAmount = CartHelper.GetSubtotal(cartItems) + _shippingFee;

            // 建立 PayPal 訂單請求
            var createOrderRequest = CreatePayPalOrderRequest(totalAmount);

            // 取得 PayPal 存取權杖
            var accessToken = await GetPayPalAccessTokenAsync();

            // 如果無法取得存取權杖，回傳空字串
            if (string.IsNullOrEmpty(accessToken))
            {
                return "";
            }

            // 建立 PayPal 訂單
            var paypalOrderId = await CreatePayPalOrderWithTokenAsync(createOrderRequest, accessToken);

            // 回傳 PayPal 訂單 ID
            return paypalOrderId;
        }

        /// <summary>
        /// 完成 PayPal 付款
        /// </summary>
        /// <param name="orderId">PayPal 訂單 ID</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <returns>付款完成結果</returns>
        public async Task<bool> CompletePayPalPaymentAsync(string orderId, string deliveryAddress, HttpRequest request, HttpResponse response)
        {
            try
            {
                // 驗證 PayPal 訂單資料
                var (isValid, errorMessage) = ValidatePayPalOrderData(orderId, deliveryAddress);
                if (!isValid)
                {
                    return false;
                }

                // 取得 PayPal 存取權杖
                var accessToken = await GetPayPalAccessTokenAsync();

                // 如果無法取得存取權杖，回傳失敗
                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                // 完成 PayPal 付款
                var paymentResult = await CapturePayPalPaymentAsync(orderId, accessToken);

                // 如果付款成功，儲存訂單到資料庫
                if (paymentResult)
                {
                    // 取得 PayPal 回應資料（這裡簡化處理，實際應該從 PayPal 回應中取得）
                    var paypalResponse = $"{{\"orderId\":\"{orderId}\",\"status\":\"COMPLETED\"}}";
                    
                    // 從請求中取得使用者 ID（這裡需要從 HTTP 上下文中取得）
                    var userId = GetUserIdFromRequest(request);
                    
                    // 儲存訂單到資料庫
                    var saveResult = await SavePayPalOrderAsync(paypalResponse, deliveryAddress, request, response, userId);
                    
                    return saveResult;
                }

                return false;
            }
            catch (Exception ex)
            {
                // 記錄錯誤（在實際應用中應該使用 ILogger）
                Console.WriteLine($"Error completing PayPal payment: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 儲存 PayPal 訂單到資料庫
        /// </summary>
        /// <param name="paypalResponse">PayPal 回應資料</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>儲存結果</returns>
        public async Task<bool> SavePayPalOrderAsync(string paypalResponse, string deliveryAddress, HttpRequest request, HttpResponse response, string userId)
        {
            try
            {
                // 取得購物車內容
                var cartItems = CartHelper.GetCartItems(request, response, _unitOfWork.Context);

                // 建立訂單
                var order = new Order
                {
                    ClientId = userId,
                    Items = cartItems,
                    ShippingFee = _shippingFee,
                    DeliveryAddress = deliveryAddress,
                    PaymentMethod = "paypal",
                    PaymentStatus = "accepted",
                    PaymentDetails = paypalResponse,
                    OrderStatus = "pending",
                    CreatedAt = DateTime.Now
                };

                // 透過 Repository 新增訂單
                await _unitOfWork.Orders.AddAsync(order);
                
                // 儲存變更
                await _unitOfWork.SaveChangesAsync();

                // 清空購物車
                response.Cookies.Delete("shopping_cart");

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得 PayPal 存取權杖
        /// </summary>
        /// <returns>存取權杖</returns>
        public async Task<string> GetPayPalAccessTokenAsync()
        {
            try
            {
                var url = _paypalUrl + "/v1/oauth2/token";

                using (var client = new HttpClient())
                {
                    // 建立基本認證標頭
                    var credentials64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_paypalClientId}:{_paypalSecret}"));
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials64}");

                    // 建立請求訊息
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMessage.Content = new StringContent("grant_type=client_credentials", null, "application/x-www-form-urlencoded");

                    // 發送請求
                    var httpResponse = await client.SendAsync(requestMessage);

                    // 檢查回應狀態
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var strResponse = await httpResponse.Content.ReadAsStringAsync();
                        var jsonResponse = JsonNode.Parse(strResponse);

                        if (jsonResponse != null)
                        {
                            return jsonResponse["access_token"]?.ToString() ?? "";
                        }
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 驗證 PayPal 訂單資料
        /// </summary>
        /// <param name="orderId">PayPal 訂單 ID</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidatePayPalOrderData(string orderId, string deliveryAddress)
        {
            // 檢查 PayPal 訂單 ID 是否為空
            if (string.IsNullOrEmpty(orderId))
            {
                return (false, "PayPal order ID is required");
            }

            // 檢查送貨地址是否為空
            if (string.IsNullOrEmpty(deliveryAddress))
            {
                return (false, "Delivery address is required");
            }

            // 驗證通過
            return (true, null);
        }

        /// <summary>
        /// 建立 PayPal 訂單請求
        /// </summary>
        /// <param name="totalAmount">總金額</param>
        /// <returns>PayPal 訂單請求</returns>
        private JsonObject CreatePayPalOrderRequest(decimal totalAmount)
        {
            // 建立請求正文
            var createOrderRequest = new JsonObject();
            createOrderRequest["intent"] = "CAPTURE";

            // 建立金額物件
            var amount = new JsonObject();
            amount["currency_code"] = "USD";
            amount["value"] = totalAmount.ToString("F2");

            // 建立購買單位物件
            var purchaseUnit = new JsonObject();
            purchaseUnit["amount"] = amount;

            // 建立購買單位陣列
            var purchaseUnits = new JsonArray();
            purchaseUnits.Add(purchaseUnit);

            // 設定購買單位
            createOrderRequest["purchase_units"] = purchaseUnits;

            return createOrderRequest;
        }

        /// <summary>
        /// 使用存取權杖建立 PayPal 訂單
        /// </summary>
        /// <param name="createOrderRequest">建立訂單請求</param>
        /// <param name="accessToken">存取權杖</param>
        /// <returns>PayPal 訂單 ID</returns>
        private async Task<string> CreatePayPalOrderWithTokenAsync(JsonObject createOrderRequest, string accessToken)
        {
            try
            {
                var url = _paypalUrl + "/v2/checkout/orders";

                using (var client = new HttpClient())
                {
                    // 設定授權標頭
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                    // 建立請求訊息
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMessage.Content = new StringContent(createOrderRequest.ToJsonString(), null, "application/json");

                    // 發送請求
                    var httpResponse = await client.SendAsync(requestMessage);

                    // 檢查回應狀態
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var strResponse = await httpResponse.Content.ReadAsStringAsync();
                        var jsonResponse = JsonNode.Parse(strResponse);

                        if (jsonResponse != null)
                        {
                            return jsonResponse["id"]?.ToString() ?? "";
                        }
                    }
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 從 HTTP 請求中取得使用者 ID
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <returns>使用者 ID</returns>
        private string GetUserIdFromRequest(HttpRequest request)
        {
            try
            {
                // 從請求標頭中取得使用者 ID（如果有的話）
                if (request.Headers.ContainsKey("X-User-ID"))
                {
                    return request.Headers["X-User-ID"].ToString();
                }

                // 如果沒有，回傳預設值或空字串
                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 完成 PayPal 付款
        /// </summary>
        /// <param name="orderId">PayPal 訂單 ID</param>
        /// <param name="accessToken">存取權杖</param>
        /// <returns>付款完成結果</returns>
        private async Task<bool> CapturePayPalPaymentAsync(string orderId, string accessToken)
        {
            try
            {
                var url = _paypalUrl + $"/v2/checkout/orders/{orderId}/capture";

                using (var client = new HttpClient())
                {
                    // 設定授權標頭
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                    
                    // 建立請求訊息
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                    requestMessage.Content = new StringContent("", null, "application/json");

                    // 發送請求
                    var httpResponse = await client.SendAsync(requestMessage);
                    
                    // 檢查回應狀態
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var strResponse = await httpResponse.Content.ReadAsStringAsync();
                        var jsonResponse = JsonNode.Parse(strResponse);
                        
                        if (jsonResponse != null)
                        {
                            var paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
                            return paypalOrderStatus == "COMPLETED";
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
