using BestStoreMVC.Models;
using System.Text.Json.Nodes;

namespace BestStoreMVC.Services
{
    /// <summary>
    /// 結帳業務邏輯介面
    /// 定義所有與結帳相關的業務邏輯操作
    /// </summary>
    public interface ICheckoutService
    {
        /// <summary>
        /// 取得結帳頁面資料
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <returns>結帳頁面資料</returns>
        (List<OrderItem> CartItems, decimal Total, string PaypalClientId) GetCheckoutData(HttpRequest request, HttpResponse response, string deliveryAddress);

        /// <summary>
        /// 建立 PayPal 訂單
        /// </summary>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <returns>PayPal 訂單 ID</returns>
        Task<string> CreatePayPalOrderAsync(HttpRequest request, HttpResponse response);

        /// <summary>
        /// 完成 PayPal 付款
        /// </summary>
        /// <param name="orderId">PayPal 訂單 ID</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <returns>付款完成結果</returns>
        Task<bool> CompletePayPalPaymentAsync(string orderId, string deliveryAddress, HttpRequest request, HttpResponse response);

        /// <summary>
        /// 儲存 PayPal 訂單到資料庫
        /// </summary>
        /// <param name="paypalResponse">PayPal 回應資料</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <param name="request">HTTP 請求</param>
        /// <param name="response">HTTP 回應</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>儲存結果</returns>
        Task<bool> SavePayPalOrderAsync(string paypalResponse, string deliveryAddress, HttpRequest request, HttpResponse response, string userId);

        /// <summary>
        /// 取得 PayPal 存取權杖
        /// </summary>
        /// <returns>存取權杖</returns>
        Task<string> GetPayPalAccessTokenAsync();

        /// <summary>
        /// 驗證 PayPal 訂單資料
        /// </summary>
        /// <param name="orderId">PayPal 訂單 ID</param>
        /// <param name="deliveryAddress">送貨地址</param>
        /// <returns>驗證結果</returns>
        (bool IsValid, string? ErrorMessage) ValidatePayPalOrderData(string orderId, string deliveryAddress);
    }
}








