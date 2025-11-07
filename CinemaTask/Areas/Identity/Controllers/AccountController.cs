
using CinemaTask.Models;
using CinemaTask.Repositories.IRepositories;
using CinemaTask.ViewModels;
using CinemaTask.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Threading.Tasks;

namespace CinemaTask.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                // Log or show the validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

                // Optional: display them in ViewData for debugging
                ViewData["Errors"] = errors;

                return View(registerVM);
            }

            var user = new ApplicationUser()
            {
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(registerVM);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account",
                new { area = "Identity", token, userId = user.Id }, Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(registerVM.Email,
                    "Cinema Task - Confirm Your Email!",
                    $"<h1>Please Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email send failed: " + ex.Message);
            }

            TempData["success-notification"] = "Registration successful! Please confirm your email.";
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                TempData["error-notification"] = "Invalid email confirmation request.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["error-notification"] = "User not found.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                TempData["success-notification"] = "Email confirmed successfully. You can now log in.";
                return RedirectToAction("Login");
            }

            TempData["error-notification"] = "Email confirmation failed or token is invalid.";
            return RedirectToAction("Login");
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByNameAsync(loginVM.UserNameOREmail) ?? await _userManager.FindByEmailAsync(loginVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email OR Password");
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Too many attemps, try again after 5 min");

                else if (result.IsNotAllowed)
                    ModelState.AddModelError(string.Empty, "Please Confirm Your Email First!!");
                else
                    ModelState.AddModelError(string.Empty, "Invalid User Name / Email OR Password");

                return View(loginVM);
            }

            TempData["success-notification"] = "Login Successfully";

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOREmail) ?? await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(resendEmailConfirmationVM);
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Already Confirmed!!!");
                return View(resendEmailConfirmationVM);
            }

            // Send Mail Confirmation
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token = token, user.Id }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!
                , "Ecommerce 518 - Resend Confirm Your Email!"
                , $"<h1>Please Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            TempData["success-notification"] = "Send Email Successfully";
            return RedirectToAction("Login");
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var user = await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOREmail) ?? await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOREmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid User Name / Email");
                return View(forgetPasswordVM);
            }

            var otp = new Random().Next(1000, 9999).ToString();

            var userOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id);

            var totalCount = userOTPs.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);

            if (totalCount > 5)
            {
                ModelState.AddModelError(string.Empty, "Pleas Try Again Later. Too Many Attemps");
                return View(forgetPasswordVM);
            }
            else
            {
                await _applicationUserOTPRepository.AddAsync(new()
                {
                    ApplicationUserId = user.Id,
                    CreateAt = DateTime.UtcNow,
                    IsValid = true,
                    Id = Guid.NewGuid().ToString(),
                    OTP = otp,
                    ValidTo = DateTime.UtcNow.AddMinutes(30)
                }, cancellationToken: cancellationToken);
                await _applicationUserOTPRepository.CommitAsync(cancellationToken);

                await _emailSender.SendEmailAsync(user.Email!
                    , "Cinema - Forget Password!"
                    , $"<h1>Use this OTP: {otp} To Validate Your Account. Don't share it.</h1>");

                TempData["success-notification"] = "Send OTP Your Email";
            }

            TempData["From-ForgetPassword"] = Guid.NewGuid().ToString();
            return RedirectToAction("ValidateOTP", new { userId = user.Id });
        }

        public IActionResult ValidateOTP(string userId)
        {
            if (TempData["From-ForgetPassword"] is null)
                return NotFound();

            return View(new ValidateOTPVM()
            {
                ApplicationUserId = userId
            });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTPVM validateOTP)
        {
            if (!ModelState.IsValid)
                return View(validateOTP);


            var validOTP = await _applicationUserOTPRepository.GetOneAsync(e => e.ApplicationUserId == validateOTP.ApplicationUserId && e.IsValid && e.ValidTo > DateTime.UtcNow);

            if (validOTP is null)
            {
                TempData["error-notification"] = "Invalid OTP";
                return RedirectToAction(nameof(ValidateOTP), new { userId = validateOTP.ApplicationUserId });
            }

            TempData["From-ValidateOTP"] = Guid.NewGuid().ToString();

            return RedirectToAction("NewPassword", new { userId = validateOTP.ApplicationUserId });
        }

        public IActionResult NewPassword(string userId)
        {
            if (TempData["From-ValidateOTP"] is null)
                return NotFound();

            return View(new NewPasswordVM()
            {
                ApplicationUserId = userId
            });
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(newPasswordVM);

            var user = await _userManager.FindByIdAsync(newPasswordVM.ApplicationUserId);

            if (user is null)
            {
                TempData["error-notification"] = "User Not Found";
                return RedirectToAction(nameof(NewPassword), new { userId = newPasswordVM.ApplicationUserId });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);

            TempData["success-notification"] = "Change Password Successfully";

            return RedirectToAction("Login");
        }
    }
}
