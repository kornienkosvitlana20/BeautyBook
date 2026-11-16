using Microsoft.AspNetCore.Mvc;
namespace BeautyBook.Controllers;
public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Error() => View();
}
