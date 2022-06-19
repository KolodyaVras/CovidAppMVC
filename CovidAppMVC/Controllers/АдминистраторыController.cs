using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CovidAppMVC.Models;

namespace CovidAppMVC.Controllers
{
    public class АдминистраторыController : Controller
    {
        private Covid19Entities db = new Covid19Entities();

        // GET: Администраторы
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                // поиск пользователя в бд
                Администраторы user = null;
                using (Covid19Entities db = new Covid19Entities())
                {
                    user = db.Администраторы.FirstOrDefault(u => u.Логин_администратора == model.Name && u.Пароль_администратора == model.Password);

                }
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(model.Name, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Пользователя с таким логином и паролем нет");
                }
            }

            return View(model);
        }
    }
}
