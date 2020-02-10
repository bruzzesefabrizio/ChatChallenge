using ChatChallenge.Models;
using PusherServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace ChatChallenge.Controllers
{
    public class ChatController : Controller
    {
        private readonly Pusher pusher;
        public ChatController()
        {
            var options = new PusherOptions
            {
                Cluster = ConfigurationManager.AppSettings["PUSHER_APP_CLUSTER"],
                Encrypted = true
            };
            pusher = new Pusher(ConfigurationManager.AppSettings["PUSHER_APP_ID"],
                                ConfigurationManager.AppSettings["PUSHER_APP_KEY"],
                                ConfigurationManager.AppSettings["PUSHER_APP_SECRET"], options);
        }
        public ActionResult Index()
        {
            if (Session["user"] == null)
            {
                return Redirect("/");
            }
            var currentUser = (User)Session["user"];
            using (var db = new ChatContext())
            {
                ViewBag.allUsers = db.Users.Where(u => u.Name != currentUser.Name)
                                 .ToList();
            }
            ViewBag.currentUser = currentUser;
            return View();
        }

        public JsonResult ConversationWithContact(int contact)
        {
            if (Session["user"] == null)
            {
                return Json(new { status = "error", message = "User is not logged in" });
            }
            var currentUser = (User)Session["user"];
            var conversations = new List<Conversation>();
            using (var db = new ChatContext())
            {
                conversations = db.Conversations.
                                  Where(c => (c.Receiver_id == currentUser.Id && c.Sender_id == contact) || (c.Receiver_id == contact && c.Sender_id == currentUser.Id))
                                  .OrderBy(c => c.Created_at)
                                  .ToList();
            }
            return Json(new { status = "success", data = conversations }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SendMessage()
        {
            if (Session["user"] == null)
            {
                return Json(new { status = "error", message = "User is not logged in" });
            }
            var currentUser = (User)Session["user"];
            var contact = Convert.ToInt32(Request.Form["contact"]);
            string socket_id = Request.Form["socket_id"];
            Conversation convo = new Conversation
            {
                Sender_id = currentUser.Id,
                Message = Request.Form["message"],
                Receiver_id = contact,
                Created_at = DateTime.Now
            };
            using (var db = new ChatContext())
            {
                db.Conversations.Add(convo);
                db.SaveChanges();
            }
            var conversationChannel = GetConvoChannel(currentUser.Id, contact);
            pusher.TriggerAsync(conversationChannel,
                                "new_message",
                                convo,
                                new TriggerOptions() { SocketId = socket_id });
            return Json(convo);
        }
        [HttpPost]
        public JsonResult MessageDelivered(int message_id)
        {
            Conversation convo = null;
            using (var db = new ChatContext())
            {
                convo = db.Conversations.FirstOrDefault(c => c.Id == message_id);
                if (convo != null)
                {
                    convo.Status = Conversation.MessageStatus.Delivered;
                    db.Entry(convo).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }
            string socket_id = Request.Form["socket_id"];
            var conversationChannel = GetConvoChannel(convo.Sender_id, convo.Receiver_id);
            pusher.TriggerAsync(
              conversationChannel,
              "message_delivered",
              convo,
              new TriggerOptions() { SocketId = socket_id });
            return Json(convo);
        }
        private string GetConvoChannel(int user_id, int contact_id)
        {
            if (user_id > contact_id)
            {
                return "private-chat-" + contact_id + "-" + user_id;
            }
            return "private-chat-" + user_id + "-" + contact_id;
        }
    }
}