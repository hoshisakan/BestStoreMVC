using Microsoft.EntityFrameworkCore;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// 通用 Repository 實作類別
    /// 提供所有實體通用的資料庫操作實作
    /// </summary>
    /// <typeparam name="T">實體類型</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // 資料庫上下文，用於存取資料庫
        protected readonly ApplicationDbContext _context;
        
        // 實體的 DbSet，用於操作特定實體的資料
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// 建構函式，注入資料庫上下文
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); // 取得實體對應的 DbSet
        }

        /// <summary>
        /// 取得所有實體
        /// </summary>
        /// <returns>實體清單</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            // 將 DbSet 轉換為清單並回傳
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// 根據 ID 取得實體
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>實體物件，如果找不到則回傳 null</returns>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            // 根據 ID 查找實體
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// 新增實體
        /// </summary>
        /// <param name="entity">實體物件</param>
        /// <returns>新增的實體物件</returns>
        public virtual async Task<T> AddAsync(T entity)
        {
            // 將實體加入 DbSet
            _dbSet.Add(entity);
            
            // 儲存變更到資料庫
            await _context.SaveChangesAsync();
            
            // 回傳新增的實體
            return entity;
        }

        /// <summary>
        /// 更新實體
        /// </summary>
        /// <param name="entity">實體物件</param>
        /// <returns>更新後的實體物件</returns>
        public virtual async Task<T> UpdateAsync(T entity)
        {
            // 標記實體為已修改狀態
            _dbSet.Update(entity);
            
            // 儲存變更到資料庫
            await _context.SaveChangesAsync();
            
            // 回傳更新後的實體
            return entity;
        }

        /// <summary>
        /// 刪除實體
        /// </summary>
        /// <param name="id">實體 ID</param>
        public virtual async Task DeleteAsync(int id)
        {
            // 根據 ID 查找實體
            var entity = await _dbSet.FindAsync(id);
            
            // 如果找到實體，則刪除
            if (entity != null)
            {
                // 從 DbSet 中移除實體
                _dbSet.Remove(entity);
                
                // 儲存變更到資料庫
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 檢查實體是否存在
        /// </summary>
        /// <param name="id">實體 ID</param>
        /// <returns>實體是否存在</returns>
        public virtual async Task<bool> ExistsAsync(int id)
        {
            // 根據 ID 查找實體，如果找到則回傳 true，否則回傳 false
            return await _dbSet.FindAsync(id) != null;
        }
    }
}
