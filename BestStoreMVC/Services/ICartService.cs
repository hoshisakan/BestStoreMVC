using BestStoreMVC.Models;
using BestStoreMVC.Models.ViewModel;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 購物車業務邏輯介面
    /// 定義所有與購物車相關的業務邏輯操作
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// 取得購物車項目清單
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <returns>購物車項目清單</returns>
        List<OrderItem> GetCartItems(HttpRequest request, HttpResponse response);

        /// <summary>
        /// 計算購物車小計
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>小計金額</returns>
        decimal GetSubtotal(List<OrderItem> cartItems);

        /// <summary>
        /// 取得運費
        /// </summary>
        /// <returns>運費金額</returns>
        decimal GetShippingFee();

        /// <summary>
        /// 計算購物車總計（含運費）
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>總計金額</returns>
        decimal GetTotal(List<OrderItem> cartItems);

        /// <summary>
        /// 計算購物車項目總數量
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>項目總數量</returns>
        int GetCartSize(List<OrderItem> cartItems);

        /// <summary>
        /// 驗證結帳資料
        /// </summary>
        /// <param name="model">結帳資料模型</param>
        /// <param name="cartItems">購物車項目清單</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string? ErrorMessage) ValidateCheckout(CheckoutDto model, List<OrderItem> cartItems);

        /// <summary>
        /// 建立訂單
        /// </summary>
        /// <param name="cartItems">購物車項目清單</param>
        /// <param name="clientId">客戶 ID</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <param name="paymentMethod">付款方式</param>
        /// <returns>建立的訂單</returns>
        Task<Order> CreateOrderAsync(List<OrderItem> cartItems, string clientId, string deliveryAddress, string paymentMethod);

        /// <summary>
        /// 清除購物車 Cookie
        /// </summary>
        /// <param name="response">HTTP 回應</param>
        void ClearCart(HttpResponse response);
    }
}









