using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Final.Models;
using MongoDB.Bson;

namespace Final.Pages.Account
{
    public class DetailsModel : PageModel
    {
        private readonly Final.Models.MyDataContext _context;

        public DetailsModel(Final.Models.MyDataContext context)
        {
            _context = context;
        }

		public Models.Account Account { get; set; } = default!; 
        public String NameAgent { get; set; }
        public async Task<IActionResult> OnGetAsync(ObjectId id)
        {
            try
            {
                if (id == null || _context.Accounts == null)
                {
                    return NotFound();
                }

                var account = await _context.Accounts.FirstOrDefaultAsync(m => m.Id.ToString() == id.ToString());
                if (account.AgentID == null)
                {
                    // Handle the case where the AgentID is null
                    // This might involve returning a different result or setting a default Agent, depending on your requirements.
                }
                else
                {
                    // Now you can safely execute the query to get the agent
                    var agent = await _context.Agents.FirstOrDefaultAsync(o => o.Id.ToString() == account.AgentID.ToString());

                    if (agent == null)
                    {
                        return NotFound(); // Handle the case where the agent is not found
                    }
                   
                    NameAgent = agent?.Name;
                 

                    // Do something with the agent...
                }
            
               

                if (account == null)
                {
                    return NotFound();
                }
                else
                {
                    Account = account;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error at Details =>" + ex);
            }
           
            return Page();
        }
    }
}

