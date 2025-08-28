using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;
using BestStoreMVC.Services.Helper;
using BestStoreMVC.Services.Repository;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 購物車業務邏輯實作類別
    /// 實作所有與購物車相關的業務邏輯操作
    /// </summary>
    public class CartService : ICartService
    {
        // Unit of Work 實例，用於存取 Repository
        private readonly IUnitOfWork _unitOfWork;
        
        // 運費設定
        private readonly decimal _shippingFee;

        /// <summary>
        /// 建構函式，注入必要的依賴
        /// </summary>
        /// <param name="unitOfWork">Unit of Work 實例</param>
        /// <param name="configuration">設定檔</param>
        public CartService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            // 從設定檔取得運費
            _shippingFee = configuration.GetValue<decimal>("CartSetting:ShippingFee");
        }

        /// <summary>
        /// 取得購物車項目清單
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <returns>購物車項目清單</returns>
        public List<OrderItem> GetCartItems(HttpRequest request, HttpResponse response)
        {
            // 透過 CartHelper 取得購物車項目清單
            return CartHelper.GetCartItems(request, response, _unitOfWork.Context);
        }

        /// <summary>
        /// 計算購物車小計
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>小計金額</returns>
        public decimal GetSubtotal(List<OrderItem> cartItems)
        {
            // 透過 CartHelper 計算小計
            return CartHelper.GetSubtotal(cartItems);
        }

        /// <summary>
        /// 計算購物車總計（含運費）
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>總計金額</returns>
        public decimal GetTotal(List<OrderItem> cartItems)
        {
            // 計算小計加上運費
            var subtotal = GetSubtotal(cartItems);
            return subtotal + _shippingFee;
        }

        /// <summary>
        /// 計算購物車項目總數量
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>項目總數量</returns>
        public int GetCartSize(List<OrderItem> cartItems)
        {
            // 計算所有項目的數量總和
            return cartItems.Sum(item => item.Quantity);
        }

        /// <summary>
        /// 驗證結帳資料
        /// </summary>
        /// <param name="model">結帳資料模型</param>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateCheckout(CheckoutDto model, List<OrderItem> cartItems)
        {
            // 檢查購物車是否為空
            if (cartItems.Count == 0)
            {
                return (false, "Your cart is empty");
            }

            // 檢查送貨地址是否為空
            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
            {
                return (false, "Delivery address is required");
            }

            // 檢查付款方式是否為空
            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
            {
                return (false, "Payment method is required");
            }

            // 驗證通過
            return (true, null);
        }

        /// <summary>
        /// 建立訂單
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <param name="clientId">客戶 ID</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <param name="paymentMethod">付款方式</param>
        /// <returns>建立的訂單</returns>
        public async Task<Order> CreateOrderAsync(List<OrderItem> cartItems, string clientId, string deliveryAddress, string paymentMethod)
        {
            // 建立新的訂單物件
            var order = new Order
            {
                ClientId = clientId,
                Items = cartItems,
                ShippingFee = _shippingFee,
                DeliveryAddress = deliveryAddress,
                PaymentMethod = paymentMethod,
                PaymentStatus = "pending",
                PaymentDetails = "",
                OrderStatus = "created",
                CreatedAt = DateTime.Now
            };

            // 透過 Repository 新增訂單
            var createdOrder = await _unitOfWork.Orders.AddAsync(order);
            
            // 儲存變更
            await _unitOfWork.SaveChangesAsync();

            // 回傳建立的訂單
            return createdOrder;
        }

        /// <summary>
        /// 清除購物車 Cookie
        /// </summary>
        /// <param name="response">HTTP 回應</param>
        public void ClearCart(HttpResponse response)
        {
            // 刪除購物車 Cookie
            response.Cookies.Delete("shopping_cart");
        }
    }
}








