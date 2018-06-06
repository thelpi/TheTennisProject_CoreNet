using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TheTennisProject_CoreNet.Models;

namespace TheTennisProject_CoreNet.Controllers
{
    /// <summary>
    /// Todo
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Todo
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <returns></returns>
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <returns></returns>
        public IActionResult AtpRanking(DateTime Date)
        {
            ViewData["Message"] = "ATP ranking.";

            return View(new AtpRankingModel(Date));
        }

        /// <summary>
        /// Todo
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
