using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 8;

        public StoreController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            IQueryable<Product> query = context.Products;

            // search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            // filter functionality
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand.Contains(brand));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Contains(category));
            }

            // sort functionality
            if (sort == "price_asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (sort == "price_desc")
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else
            {
                // newest products first
                query = query.OrderByDescending(p => p.Id);
            }

            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal totalCount = query.Count();
            int totalPages = (int)Math.Ceiling(totalCount / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };

            return View(storeSearchModel);
        }

        public IActionResult Details(int id)
        {
            var proudct = context.Products.Find(id);

            if (proudct == null)
            {
                return RedirectToAction("Index", "Store");
            }

            return View(proudct);
        }
    }
}
