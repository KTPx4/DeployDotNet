using Final.Models;
using Final.util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using System.Security.Policy;

namespace Final.Pages.Transaction_Page
{


    public class TransactionModel : PageModel
    {
        public static List<OrderDetail> StaticOrderDetailList { get; set; }
        private readonly Final.Models.MyDataContext _context;


        [BindProperty]
        public Customer Customer { get; set; } = new Customer();


        [BindProperty]
        public Order Order { get; set; } = new Order();

        public int Number { get; set; }


        public TransactionModel(Final.Models.MyDataContext context)
        {
            _context = context;
        }
        public void OnGet(List<string> list)
        {
            foreach (var orderDetailString in list)
            {
                string[] components = orderDetailString.Split(',');

                string barCodeID = components[0];
                int quantity = int.Parse(components[1]);
                int totalPrice = int.Parse(components[2]);

                // Create an OrderDetail object and add it to the list
                var orderDetail = new OrderDetail
                {
                    BarCodeID = barCodeID,
                    Quantity = quantity,
                    TotalPrice = totalPrice
                };

                // Initialize the list if it's null
                StaticOrderDetailList ??= new List<OrderDetail>();

                // Add the orderDetail to the StaticOrderDetailList
                StaticOrderDetailList.Add(orderDetail);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool isRedirected = false;

            // Check if the phone number exists in the database
            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Phone == Customer.Phone);


            Order.ToltalPayment = (int) SumTotalPrices();
            if (existingCustomer != null)
            {
                // Phone number exists, update the customer with desired values
                Customer.fullName = existingCustomer.fullName; // Update with desired value
                Customer.Address = existingCustomer.Address;   // Update with desired value

            }
            else if (!string.IsNullOrEmpty(Customer.fullName) && !string.IsNullOrEmpty(Customer.Phone))
            {
                // Phone number doesn't exist, create a new customer and add to the database
                _context.Customers.Add(Customer);
                await _context.SaveChangesAsync();
            }
            if (Order.ToltalPayment > Order.MoneyGiven)
            {
                TempData["error"] = "Total price cannot be greater than the money given.";
                return Page();
            }
            else if (Order.MoneyExchange == 0 && (Order.MoneyGiven - Order.ToltalPayment) != 0)
            {
                Order.MoneyExchange = Order.MoneyGiven - Order.ToltalPayment;

                // Save the Order to the database

                return Page();
            }

          



            Order.StaffEmail = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            Order.CreateAt = DateTime.Now;
            Order.CustomerPhone = Customer.Phone;
            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();


            // Retrieve the Order using the CreateAt attribute
            Order = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.CreateAt == Order.CreateAt);

            // Set OrderID for each OrderDetail in StaticOrderDetailList
            foreach (var orderDetail in StaticOrderDetailList)
            {
                orderDetail.OrderID = Order.Id;
            }

            // Save each OrderDetail to the database
            foreach (var orderDetail in StaticOrderDetailList)
            {
                // Check if the entity is already being tracked
                var existingOrderDetail = _context.ChangeTracker.Entries<OrderDetail>()
                    .FirstOrDefault(e => e.Entity.Id == orderDetail.Id);

                if (existingOrderDetail != null)
                {
                    // Detach the existing tracked entity
                    existingOrderDetail.State = EntityState.Detached;
                }

                if (_context.OrderDetails.Local.Any(e => e.Id == orderDetail.Id))
                {
                    // Cập nhật thông tin của thực thể đã tồn tại
                    _context.Update(orderDetail);
                }
                else
                {
                    // Thêm mới nếu thực thể không tồn tại
                    _context.OrderDetails.Add(orderDetail);
                }

                await _context.SaveChangesAsync();
            }


            // Save each OrderDetail to the database


            return RedirectToPage("DetailOrder", new { id = Order.Id });
            //return RedirectToPage("DetailOrder", new { id = Order.Id });

        }


        // util 
        public decimal SumTotalPrices()
        {
            return StaticOrderDetailList?.Sum(orderDetail => orderDetail.TotalPrice) ?? 0;
        }

    }
}
