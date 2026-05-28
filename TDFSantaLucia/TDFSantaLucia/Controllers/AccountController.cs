using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly ICuentaService _cuentaService;

        public AccountController(ICuentaService cuentaService)
        {
            _cuentaService = cuentaService;
        }

        [HttpGet("Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var (succeeded, errorMessage) = await _cuentaService.LoginAsync(
                model.Correo, model.Password, model.RememberMe);

            if (succeeded)
                return LocalRedirect(returnUrl ?? "/");

            ModelState.AddModelError(string.Empty, errorMessage!);
            return View(model);
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _cuentaService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("AccesoDenegado")]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}