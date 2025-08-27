using BestStoreMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace BestStoreMVC.Services.Repository
{
    /// <summary>
    /// Unit of Work 實作類別
    /// 管理所有 Repository 和交易
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        // 資料庫上下文
        private readonly ApplicationDbContext _context;
        
        // 交易物件
        private IDbContextTransaction? _transaction;
        
        // 產品 Repository 實例
        private IProductRepository? _productRepository;
        
        // 使用者 Repository 實例
        private IUserRepository? _userRepository;
        
        // 訂單 Repository 實例
        private IOrderRepository? _orderRepository;
        
        // 使用者管理器
        private readonly UserManager<ApplicationUser> _userManager;
        
        // 角色管理器
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="context">資料庫上下文</param>
        /// <param name="userManager">使用者管理器</param>
        /// <param name="roleManager">角色管理器</param>
        public UnitOfWork(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        /// <summary>
        /// 資料庫上下文屬性
        /// </summary>
        public ApplicationDbContext Context => _context;

        /// <summary>
        /// 產品 Repository 屬性
        /// </summary>
        public IProductRepository Products => _productRepository ??= new ProductRepository(_context);

        /// <summary>
        /// 使用者 Repository 屬性
        /// </summary>
        public IUserRepository Users => _userRepository ??= new UserRepository(_userManager, _roleManager, _context);

        /// <summary>
        /// 訂單 Repository 屬性
        /// </summary>
        public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);

        /// <summary>
        /// 儲存變更
        /// </summary>
        /// <returns>受影響的記錄數</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 開始交易
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// 提交交易
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 回滾交易
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
