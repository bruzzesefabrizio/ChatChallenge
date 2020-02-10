using ChatChallenge.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ChatChallenge.Controllers
{
    public class ConversationController : Controller
    {
        public JsonResult WithContact(int contact)
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

            return Json(new { status = "success", data = conversations });
        }
    }
}