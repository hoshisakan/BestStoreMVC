// 使用 System.Text.Json 來做 JSON 反序列化
using BestStoreMVC.Models;
using System.Text.Json;

namespace BestStoreMVC.Services.Helper
{
    public class CartHelper
    {
        // 取得購物車字典：key = ProductId(int), value = 數量(int)
        public static Dictionary<int, int> GetCartDictionary(HttpRequest request, HttpResponse response)
        {
            // 從 Cookie 讀取名為 "shopping_cart" 的值，若不存在則給空字串
            string cookieValue = request.Cookies["shopping_cart"] ?? "";

            try
            {
                // 將 Base64 編碼的 cookie 內容解碼成 UTF8 字串（預期為 JSON）
                var cart = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));

                // 記錄除錯訊息：顯示原始 cookie 與解碼後內容
                Console.WriteLine("[CartHelper] cart=" + cookieValue + " -> " + cart);

                // 反序列化為 Dictionary<int, int>（商品 Id -> 數量）
                var dictionary = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);

                // 如果成功反序列化，直接回傳字典
                if (dictionary != null)
                {
                    return dictionary;
                }
            }
            catch (Exception)
            {
            }

            // 若 cookie 有值但解析失敗，視為壞資料：刪除該 cookie
            if (cookieValue.Length > 0)
            {
                // cookie 內容錯誤，刪除 cookie
                response.Cookies.Delete("shopping_cart");
            }

            // 回傳空的購物車字典
            return new Dictionary<int, int>();
        }

        // 取得購物車內商品總數量（累加所有品項的數量）
        public static int GetCartSize(HttpRequest request, HttpResponse response)
        {
            // 初始化總數量為 0
            int cartSize = 0;

            // 先取得購物車字典
            var cartDictionary = GetCartDictionary(request, response);

            // 逐一加總每個品項的數量
            foreach (var keyValuePair in cartDictionary)
            {
                cartSize += keyValuePair.Value;
            }

            // 回傳總數量
            return cartSize;
        }

        public static List<OrderItem> GetCartItems(HttpRequest request, HttpResponse response, ApplicationDbContext context)
        {
            var cartItems = new List<OrderItem>();
            var cartDictionary = GetCartDictionary(request, response);
            
            foreach (var pair in cartDictionary)
            {
                var productId = pair.Key;
                var quantity = pair.Value;
                var product = context.Products.Find(productId);
                if (product != null && quantity > 0)
                {
                    var orderItem = new OrderItem()
                    {
                        Product = product,
                        Quantity = quantity,
                        UnitPrice = product.Price
                    };
                    cartItems.Add(orderItem);
                }
            }
            return cartItems;
        }

        public static decimal GetSubtotal(List<OrderItem> cartItems)
        {
            decimal subtotal = 0;

            foreach (var item in cartItems)
            {
                subtotal += item.UnitPrice * item.Quantity;
            }

            return subtotal;
        }
    }
}
