using BestStoreMVC.Services;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// Unit of Work 介面
    /// 管理所有 Repository 和交易
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 資料庫上下文
        /// </summary>
        ApplicationDbContext Context { get; }

        /// <summary>
        /// 產品 Repository
        /// </summary>
        IProductRepository Products { get; }

        /// <summary>
        /// 使用者 Repository
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// 訂單 Repository
        /// </summary>
        IOrderRepository Orders { get; }

        /// <summary>
        /// 儲存變更
        /// </summary>
        /// <returns>受影響的記錄數</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 開始交易
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// 提交交易
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// 回滾交易
        /// </summary>
        Task RollbackTransactionAsync();
    }
}
