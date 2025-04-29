using InventoryMangementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InventoryMangementSystem.Controllers
{
    /// <summary>
    /// Controller responsible for handling the home page and sign-in functionality.
    /// </summary>
    public class HomeController : Controller
    {
        // Dependency injection for logging.
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class with a logger instance.
        /// </summary>
        /// <param name="logger">Logger for tracking and logging information.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger; // Assigns the injected logger to the private field.
        }

        /// <summary>
        /// Returns the sign-in page view.
        /// </summary>
        /// <returns>The view for the sign-in page.</returns>
        public IActionResult SignInPage()
        {
            return View(); // Renders the SignInPage view.
        }

        /// <summary>
        /// Displays error information when an error occurs in the application.
        /// </summary>
        /// <returns>The error view with request details.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Creates an ErrorViewModel object with the request ID and returns the error view.
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
