using System.Web.Mvc;

namespace ChatChallenge.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["user"] != null)
            {
                return Redirect("/chat");
            }

            return View();
        }

    }
}