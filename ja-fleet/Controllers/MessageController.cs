using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using Microsoft.Extensions.Configuration;
using jafleet.Util;

namespace jafleet.Controllers
{
    public class MessageController : Controller
    {

        private readonly jafleetContext _context;
        private readonly IConfiguration _configuration;

        public MessageController(jafleetContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Send(MessageModel model)
        {
            LineUtil.PushMe("【JA-Fleet from web】\n"+
                $"名前：{model.Name}\n" +
                $"返信先：{model.Replay}\n" +
                $"{model.Message}");
            return Content("OK");
        }

    }
}
