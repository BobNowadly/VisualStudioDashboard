using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public partial class LoginController : Controller
    {
        [AllowAnonymous]
        public virtual ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public virtual ActionResult Index(LoginViewModel model)
        {
            if (model.UserName == ConfigurationManager.AppSettings["UserName"] &&
                model.Password == ConfigurationManager.AppSettings["Password"])
            {
                FormsAuthentication.SetAuthCookie(model.UserName, true);

                return RedirectToAction(MVC.Burnup.Index());
            }

            return View(model);
        }
    }
}