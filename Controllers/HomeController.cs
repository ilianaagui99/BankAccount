using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using BankAccount.Models;

namespace BankAccount.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            _context = context;
        }

        //Home page
        [HttpGet("")]
        public IActionResult Index()
        {
            //ViewBag.AllUsers = _context.Users;
            return View();
        }

        //Post to User database (register)
        [HttpPost("/register")]
        public IActionResult Registration(User user)
            {
            // Check initial ModelState
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    // You may consider returning to the View at this point
                    return View("Index");
                }
                // if everything is okay save the user Users db
                // Initializing a PasswordHasher object, providing our User class as its type
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user, user.Password);
                _context.Add(user);
                _context.SaveChanges();
                HttpContext.Session.SetInt32("userId", user.UserId);
                return RedirectToAction("Dashboard",new { userId = user.UserId });
            }
            // otherwise
            return View("Index");
            }

        //login page
        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View();
        }

        //request to login 
        [HttpPost("/login")]
        public IActionResult LoginPost(Login userSubmission)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                User userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<Login>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                     ModelState.AddModelError("Password", "Passwords don't match");
                    return View("Login");

                }

                // If everything is okay, redirect to dashboard page 
                HttpContext.Session.SetInt32("userId", userInDb.UserId);
                return RedirectToAction("Dashboard", new { userId = userInDb.UserId });
            }
            // go back to login if any errors
            return View("Login");
        }   

         //request to dashboard page   
        [HttpGet("dashboard/{userId}")]
            public IActionResult Dashboard()
                {
                    int? userId = HttpContext.Session.GetInt32("userId");
                    if(userId == null)
                    {
                        return View("Index");
                    }
                    ViewBag.User = _context.Users.FirstOrDefault(u => u.UserId == userId);
                    ViewBag.Transactions = _context.Transactions.Include(t => t.AccountOwner)
                                                            .Where(t => t.UserId == userId)
                                                            .OrderByDescending(t => t.CreatedAt);
                    return View("Dashboard");
                }
        [HttpPost("dashboard/{userId}/transaction")]
        public IActionResult NewTransaction(Transaction newTransaction)
        {
            int? userId = HttpContext.Session.GetInt32("userId");
            if(userId == null)
            {
                return RedirectToAction("Index");
            }
            User user = _context.Users.FirstOrDefault(i => i.UserId == userId);
            ViewBag.Transactions = _context.Transactions.Include(t => t.AccountOwner)
                                .Where(t => t.UserId == userId)
                                .OrderByDescending(t => t.CreatedAt);
            ViewBag.User = user;
            if(ModelState.IsValid)
            {
                if(user.balance + newTransaction.Amount < 0)
                {
                    ModelState.AddModelError("Amount", "You're too broke right now");
                    return View("Account");
                }
                
                newTransaction.UserId = (int) userId;
                _context.Transactions.Add(newTransaction);
                user.balance += newTransaction.Amount;
                _context.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("Dashboard");
        }
       
    
    public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}