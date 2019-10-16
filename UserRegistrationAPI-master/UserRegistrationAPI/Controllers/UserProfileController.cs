using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserRegistrationAPI.Models;

namespace UserRegistrationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        public UserProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        //GET: /api/UserProfile 
        public async Task<object> GetUserProfile()
        {
            string userId = User.Claims.First(c => c.Type == "UserId").Value;
            var user = await _userManager.FindByIdAsync(userId);
            return new {
                 user.FullName,
                 user.Email,
                 user.UserName
            };
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        [Route("ForAdmin")]
        public string GetForAdmin()
        {
            return "web method for Admin";
        }

        [HttpGet]
        [Authorize(Roles ="Customer")]
        [Route("ForCustomer")]
        public string GetForCustomer()
        {
            return "web method for Customer";
        }

        [HttpGet]
        [Authorize(Roles ="Admin,Customer")] //chaining of roles 
        [Route("ForUsersOrCustomer")]
        public string GetForAdminOrCustomer()
        {
            return "web method for Admin or customer";
        }
    }
}