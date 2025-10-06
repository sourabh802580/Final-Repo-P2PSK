using P2PLibray.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace P2PERP.Controllers
{
    public class AdminController : Controller
    {

        // GET: InventoryDashBoard
        public ActionResult InventoryDashBoard()
        {
            return View();
        }

        public ActionResult Index()
        {
            if (Session["StaffCode"] == null || string.IsNullOrWhiteSpace(Session["StaffCode"].ToString()))
            {
                return RedirectToAction("MainLogin", "Account");
            }
            return View();
        }

        #region Pranav Mane
        public async Task<ActionResult> UserProfile()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null)
                return loginCheck;

            BALAccount account = new BALAccount();
            var acc = await account.UserProfileDetails(Session["StaffCode"].ToString());
            return View(acc);
        }

        public ActionResult CheckLogin()
        {
            if (Session["StaffCode"] == null || string.IsNullOrWhiteSpace(Session["StaffCode"].ToString()))
            {
                return RedirectToAction("MainLogin", "Account");
            }
            return null;
        }

        public ActionResult Calender()
        {
            if (Session["StaffCode"] == null || string.IsNullOrWhiteSpace(Session["StaffCode"].ToString()))
            {
                return RedirectToAction("MainLogin", "Account");
            }
            return View();
        }
        #endregion


    }
}