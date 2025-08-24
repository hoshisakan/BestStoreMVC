using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BestStoreMVC.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/Orders/{action=Index}/{id?}")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 5;


        public AdminOrdersController(ApplicationDbContext context)
        {
            this.context = context;
        }


        public IActionResult Index(int pageIndex)
        {
            IQueryable<Order> query = context.Orders.
                Include(o => o.Client).
                Include(o => o.Items).
                OrderByDescending(o => o.Id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var orders = query.ToList();

            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }

        public IActionResult Details(int id)
        {
            var order = context.Orders.
                Include(o => o.Client).
                Include(o => o.Items).
                ThenInclude(oi => oi.Product).
                FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.NumsOrder = context.Orders.Where(o => o.ClientId == order.ClientId).Count();

            return View(order);
        }

        public async Task<IActionResult> Edit(int id, string? payment_status, string? order_status)
        {
            var order = context.Orders.Find(id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(payment_status) && string.IsNullOrEmpty(order_status))
            {
                return RedirectToAction("Details", new { id = id });
            }

            if (!string.IsNullOrEmpty(payment_status))
            {
                order.PaymentStatus = payment_status;
            }

            if (!string.IsNullOrEmpty(order_status))
            {
                order.OrderStatus = order_status;
            }

            await context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = id });
        }
    }
}
