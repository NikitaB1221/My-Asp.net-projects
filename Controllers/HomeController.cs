using ASP111.Models;
using ASP111.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ASP_111.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Декларируем зависимости
        private readonly DateService _dateService;
        private readonly TimeService _timeService;
        private readonly DateTimeService _dateTimeService;
        //  Признак readonly говорит о том что данные должны инициализироваться конструктором
        public HomeController(ILogger<HomeController> logger, DateService dateService, TimeService timeService, DateTimeService dateTimeService)  //  параметр в конструкторе требует передать ссылку на обьект, иначе обьект конструктор не может быть создан - это являеться зависимостью
        {
            _logger = logger;
            _dateService = dateService;
            _timeService = timeService;
            _dateTimeService = dateTimeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }

        public IActionResult Razor()
        {
            return View();
        }
        public ViewResult Services()
        {
            ViewData["date"] = _dateService.GetDate();
            ViewData["time"] = _timeService.GetTime();
            ViewData["now"] = _dateTimeService.GetNow();
            ViewData["date-hash"] = _dateService.GetHashCode();
            ViewData["time-hash"] = _timeService.GetHashCode();
            ViewData["now-hash"] = _dateTimeService.GetHashCode();
            ViewData["ctrl-hash"] = this.GetHashCode();
            return View();
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