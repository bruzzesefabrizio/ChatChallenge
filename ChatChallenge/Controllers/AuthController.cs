using ChatChallenge.Models;
using PusherServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;

namespace ChatChallenge.Controllers
{
	public class AuthController : Controller
	{
		private readonly Pusher pusher;

		//class constructor
		public AuthController()
		{

			var options = new PusherOptions
			{
				Cluster = ConfigurationManager.AppSettings["PUSHER_APP_CLUSTER"],
				Encrypted = true
			};
			pusher = new Pusher(
			   ConfigurationManager.AppSettings["PUSHER_APP_ID"],
			   ConfigurationManager.AppSettings["PUSHER_APP_KEY"],
			   ConfigurationManager.AppSettings["PUSHER_APP_SECRET"],
			   options
			);
		}
		[HttpPost]
		public ActionResult Login()
		{

			string user_name = Request.Form["username"];

			if (user_name.Trim() == "")
			{
				return Redirect("/");
			}


			using (var db = new ChatContext())
			{

				User user = db.Users.FirstOrDefault(u => u.Name == user_name);

				if (user == null)
				{
					user = new User { 
						Name = user_name,
						Created_at = DateTime.Now
					};

					db.Users.Add(user);
					db.SaveChanges();
				}

				Session["user"] = user;
			}

			return Redirect("/chat");
		}

		public JsonResult AuthForChannel(string channel_name, string socket_id)
		{
			if (Session["user"] == null)
			{
				return Json(new { status = "error", message = "User is not logged in" });
			}

			var currentUser = (User)Session["user"];

			if (channel_name.IndexOf("presence") >= 0)
			{

				var channelData = new PresenceChannelData()
				{
					user_id = currentUser.Id.ToString(),
					user_info = new
					{
						id = currentUser.Id,
						name = currentUser.Name
					},
				};

				var presenceAuth = pusher.Authenticate(channel_name, socket_id, channelData);

				return Json(presenceAuth);

			}

			if (channel_name.IndexOf(currentUser.Id.ToString()) == -1)
			{
				return Json(new { status = "error", message = "User cannot join channel" });
			}

			var auth = pusher.Authenticate(channel_name, socket_id);

			return Json(auth);
		}
	}
}