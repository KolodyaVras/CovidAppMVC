    using CovidAppMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CovidAppMVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(IdInp inpId)
        {
            if (ModelState.IsValid)
            {
                // поиск пользователя в бд
                Сотрудники user = null;
                using (Covid19Entities db = new Covid19Entities())
                {
                    user = db.Сотрудники.FirstOrDefault(u => u.Код_сотрудника == inpId.id);
                }
                if (user != null)
                {
                    return RedirectToAction("Index", "PersonalSertificate", new { id = user.Код_сотрудника});
                }
                else
                {
                    ModelState.AddModelError("", "Пользователя с таким табельным номером нет");
                }
            }

            return View(inpId);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}