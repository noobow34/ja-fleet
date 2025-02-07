﻿using Microsoft.AspNetCore.Mvc;
using jafleet.Models;
using jafleet.Commons.EF;
using Noobow.Commons.Utils;
using Noobow.Commons.Constants;
using EnumStringValues;

namespace jafleet.Controllers
{
    public class MessageController : Controller
    {

        private readonly JafleetContext _context;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _services;

        public MessageController(JafleetContext context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _services = serviceScopeFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SendAsync(MessageModel model)
        {
            await SlackUtil.PostAsync(SlackChannelEnum.jafleet.GetStringValue(), "【JA-Fleet from web】\n" +
                $"名前：{model.Name}\n" +
                $"返信先：{model.Replay}\n" +
                $"{model.Message}");
            _ = Task.Run(() =>
            {
                using var serviceScope = _services.CreateScope();
                using var context = serviceScope.ServiceProvider.GetService<JafleetContext>()!;
                var m = new Message
                {
                    Sender = model.Name,
                    MessageDetail = model.Message,
                    ReplayTo = model.Replay,
                    MessageType = Commons.Constants.MessageType.WEB,
                    RecieveDate = DateTime.Now
                };
                context.Messagess.Add(m);
                context.SaveChanges();
            });
            return Content("OK");
        }

    }
}
