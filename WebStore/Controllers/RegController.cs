using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebStore.Models;

namespace WebStore.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RegController
    {
        //[HttpGet]
        //[Route("privacy")]
        //public IActionResult Privacy()
        //{
        //    return View();
        //}
    }
}
