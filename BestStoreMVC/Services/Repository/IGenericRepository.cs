namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 通用 Repository 介面
    /// 定義所有實體通用的資料庫操作
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// 取得所有實體
        /// </summary>
        /// <returns>實體清單</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// 根據 ID 取得實體
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>實體物件，如果找不到則回傳 null</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// 新增實體
        /// </summary>
        /// <param name="entity">實體物件</param>
        /// <returns>新增的實體物件</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// 更新實體
        /// </summary>
        /// <param name="entity">實體物件</param>
        /// <returns>更新後的實體物件</returns>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// 刪除實體
        /// </summary>
        /// <param name="id">實體 ID</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// 檢查實體是否存在
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>實體是否存在</returns>
        Task<bool> ExistsAsync(int id);
    }
}
