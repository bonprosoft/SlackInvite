using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlackInvite.Models;

namespace SlackInvite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ServiceConfiguration _configuration;

        public HomeController(IOptions<ServiceConfiguration> config)
        {
            _configuration = config.Value;
        }

        [ActionName("Index")]
        public IActionResult Index()
        {
            ViewBag.TeamName = _configuration.SlackTeamName;
            return View();
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Index(InvitationRequestViewModel model)
        {
            ViewBag.TeamName = _configuration.SlackTeamName;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var client = new HttpClient())
                {
                    var endpointUri = $"https://{_configuration.SlackTeamName}.slack.com/api/users.admin.invite";
                    var content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "email", model.Email },
                        { "token", _configuration.SlackToken },
                        { "set_active", "true" }
                    });
                    var result = await client.PostAsync(endpointUri, content);
                    if (!result.IsSuccessStatusCode) 
                        throw new Exception();
                }
                ViewBag.MessageTitle = "Done.";
                ViewBag.Message = $"We sent invitation for {_configuration.SlackTeamName} slack team to {model.Email}.";
                ViewBag.IsSuccess = true;
            }
            catch (Exception e)
            {
                ViewBag.MessageTitle = "Oops.";
                ViewBag.Message = "Invalid e-mail address or something went wrong :(";
                ViewBag.IsSuccess = false;
            }
            return View(new InvitationRequestViewModel());
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
