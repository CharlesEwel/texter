﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Texter.Models;
using Texter.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;


namespace Texter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetMessages()
        {
            var allMessages = Message.GetMessages();
            return View(allMessages);
        }
        public IActionResult Inbox()
        {
            var UserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var thisUser = _db.Users.FirstOrDefault(u => u.Id == UserId);
            var responses = Message.GetSMSResponses(thisUser.PhoneNumber);
            return View(responses);
        }
    public IActionResult SendMessage()
        {
            var UserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.User = _db.Users.Include(u => u.Contacts).FirstOrDefault(u=>u.Id == UserId);
            return View();
        }
        [HttpPost]
        public IActionResult SendMessage(SendMessageViewModel newMessage)
        {
            foreach(string recipient in newMessage.Recipients)
            {
                Message individualMessage = new Models.Message { To = recipient, From = newMessage.From, Body = newMessage.Body, Status = newMessage.Status };
                individualMessage.Send();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, PhoneNumber = model.PhoneNumber};
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LogInViewModel model)
        {
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: true, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
        public IActionResult AddContact()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddContact(Contact newContact)
        {
            var UserId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            newContact.ApplicationUserId = UserId;
            _db.Contacts.Add(newContact);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
