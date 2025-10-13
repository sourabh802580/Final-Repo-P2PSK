using Newtonsoft.Json;
using P2PLibray.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace P2PERP.Controllers
{
    public class AccountController : Controller
    {
        BALAccount bal = new BALAccount();

        /// <summary>
        /// Displays the login page and clears any existing session.
        /// </summary>
        /// <returns>The login view.</returns>
        [HttpGet]
        public ActionResult MainLogin()
        {
            Session.Clear();
            FormsAuthentication.SignOut();

            return View();
        }

        /// <summary>
        /// Logs out the current user, clears the session, and prevents cached access.
        /// </summary>
        /// <returns>Redirects to the login page.</returns>
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();

            Response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return RedirectToAction("MainLogin", "Account");
        }

        /// <summary>
        /// Authenticates a user with the provided account credentials.
        /// </summary>
        /// <param name="acc">The account object containing email and password.</param>
        /// <returns>
        /// A JSON result indicating success or failure. 
        /// On success, includes the department ID.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> MainLogin(Account acc)
        {
            Session["StaffCode"] = null;
            Account acc1 = await bal.Login(acc);

            if (acc1?.StaffCode == string.Empty || acc1?.StaffCode == null)
            {
                return Json(new { success = false, message = "Invalid login credentials" });
            }

            Session["StaffCode"] = acc1.StaffCode;
            Session["DepartmentId"] = acc1.DepartmentId;
            Session["RoleId"] = acc1.RoleId;
            return Json(new { success = true, departmentId = acc1.DepartmentId });
        }

        /// <summary>
        /// Displays the forgot password page.
        /// </summary>
        /// <returns>The forgot password view.</returns>
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Handles a user's request to reset their password.
        /// Verifies if the provided email exists in the system, 
        /// and if valid, stores relevant session information for the password reset process.
        /// </summary>
        /// <param name="acc">An <see cref="Account"/> object containing the user's email address.</param>
        /// <returns>
        /// Returns a JSON result:
        /// - If the email is valid: { success = true } and stores the staff code and email in session for further steps.
        /// - If the email is invalid: { success = false, message = "Invalid Email" }.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(Account acc)
        {
            string str = await bal.CheckEmail(acc);

            if (string.IsNullOrEmpty(str))
            {
                return Json(new { success = false, message = "Invalid Email" });
            }

            Session["StaffCodeForForgotPassword"] = str;
            Session["ForgetPasswordEmail"] = acc.EmailAddress;

            return Json(new { success = true });
        }

        /// <summary>
        /// Checks whether the current user session contains a valid StaffCode.
        /// </summary>
        /// <returns>
        /// Returns a JSON object with:
        /// - success = true, if Session["StaffCode"] exists.
        /// - success = false, if Session["StaffCode"] is null.
        /// </returns>
        [HttpGet]
        public JsonResult CheckSession()
        {
            bool isValid = Session["StaffCode"] != null;
            return Json(new { success = isValid }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Displays the verification code entry page.
        /// </summary>
        /// <returns>The verification code view.</returns>
        [HttpGet]
        public ActionResult VerifyCode()
        {
            return View();
        }

        /// <summary>
        /// Verifies the reset code entered by the user.
        /// </summary>
        /// <param name="acc">The account object containing the entered code.</param>
        /// <returns>
        /// A JSON result indicating whether the verification code matches.
        /// </returns>
        [HttpPost]
        public ActionResult VerifyCode(Account acc)
        {
            if (acc.Code != Session["VerificationCode"].ToString())
                return Json(new { success = false, message = "Code Dose Not Match" });
            return Json(new { success = true });
        }

        /// <summary>
        /// Displays the change password page.
        /// </summary>
        /// <returns>The change password view.</returns>
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Updates the password for the staff account after verification.
        /// </summary>
        /// <param name="acc">The account object containing new and confirm password.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<ActionResult> ChangePassword(Account acc)
        {
            if (acc.Password != acc.ConfirmPassword)
                return Json(new { success = false, message = "Passwords Dose Not Match" });

            acc.StaffCode = Session["StaffCodeForForgotPassword"].ToString();

            await bal.ChangePassword(acc);

            return Json(new { success = true });
        }

        /// <summary>
        /// Retrieves user information for the currently logged-in staff.
        /// </summary>
        /// <returns>
        /// A JSON result containing user name, department, role, and profile photo,
        /// or an error if the session has expired.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> GetUserInfo()
        {
            var staffCode = Session["StaffCode"]?.ToString();
            if (string.IsNullOrEmpty(staffCode))
                return Json(new { success = false, message = "Session expired" }, JsonRequestBehavior.AllowGet);

            Account acc = await bal.UserProfileData(staffCode);

            return Json(new
            {
                success = true,
                userName = acc.UserName,
                department = acc.Department,
                role = acc.RoleName,
                profilePhoto = acc.ProfilePhoto
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the user profile with the provided account information.
        /// </summary>
        /// <param name="acc">The account object containing updated profile info.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<ActionResult> UpdateUserProfile(Account acc)
        {
            if (String.IsNullOrEmpty(acc.AlternamteNumber))
                return Json(new { success = false, message = "Alternate Number Is Null" });

            await bal.UpdateUserProfile(acc);

            return Json(new { success = true });
        }

        /// <summary>
        /// Sends an email with optional CC, BCC, and attachments to the specified recipients.
        /// </summary>
        /// <param name="email">The Email object containing recipients, subject, body, and attachments.</param>
        /// <returns>
        /// A JSON result indicating the outcome:
        /// - success = true, message = "Email sent successfully." if the email was sent successfully.
        /// - success = false, message = error details if sending failed.
        /// </returns>
        [HttpPost]
        public JsonResult SendEmail(Email email)
        {
            try
            {
                var smtpClient = new System.Net.Mail.SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential(
                        System.Web.Configuration.WebConfigurationManager.AppSettings["MainEmail"],
                        System.Web.Configuration.WebConfigurationManager.AppSettings["AppPassword"]
                    ),
                    EnableSsl = true,
                };

                var mail = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(System.Web.Configuration.WebConfigurationManager.AppSettings["MainEmail"]),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = email.IsBodyHtml
                };

                // Add recipients
                email.ToEmails?.ForEach(x => mail.To.Add(x));
                email.CcEmails?.ForEach(x => mail.CC.Add(x));
                email.BccEmails?.ForEach(x => mail.Bcc.Add(x));

                // Add attachments (if any)
                if (email.AttachmentPaths != null)
                {
                    foreach (var path in email.AttachmentPaths)
                    {
                        if (System.IO.File.Exists(path))
                            mail.Attachments.Add(new System.Net.Mail.Attachment(path));
                    }
                }

                smtpClient.Send(mail);

                return Json(new { success = true, message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all permissions assigned to the currently logged-in staff.
        /// </summary>
        /// <returns>
        /// A JSON result containing permission type and name,
        /// or an error if the session has expired.
        /// </returns>
        public async Task<ActionResult> GetReadPermissions()
        {
            var staffCode = Session["StaffCode"]?.ToString();
            if (string.IsNullOrEmpty(staffCode))
                return Json(new { success = false, message = "Session expired" }, JsonRequestBehavior.AllowGet);

            var permissions = await bal.GetAllPermissions(staffCode);

            var allPermission = permissions.Select(p => new { p.PermissionType, p.PermissionName }).ToList();

            return Json(new { success = true, permissions = allPermission }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves country details from the CountryStateCity API.
        /// </summary>
        /// <param name="id">
        /// The ISO2 country code.  
        /// If null or empty, all countries are returned;  
        /// otherwise, details of the specified country are returned.
        /// </param>
        /// <returns>
        /// A JSON result containing either a list of <see cref="CountryDto"/> objects (all countries)  
        /// or a single <see cref="CountryDto"/> object (one country).
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetCountries(string id = null)
        {
            var code = id;
            var apiKey = WebConfigurationManager.AppSettings["X-CSCAPI-KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key not found in Web.config!");
            }

            using (var client = new HttpClient())
            {
                string url = string.IsNullOrEmpty(code)
                    ? "https://api.countrystatecity.in/v1/countries"
                    : $"https://api.countrystatecity.in/v1/countries/{code}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url)
                };
                request.Headers.Add("X-CSCAPI-KEY", apiKey);

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(code))
                    {
                        // All countries → array
                        var allCountries = JsonConvert.DeserializeObject<List<CountryDto>>(body);
                        return Json(allCountries, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // Single country → object
                        var country = JsonConvert.DeserializeObject<CountryDto>(body);
                        return Json(country, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves state details for a given country from the CountryStateCity API.
        /// </summary>
        /// <param name="CountryCode">The ISO2 country code.</param>
        /// <param name="StateCode">
        /// (Optional) The ISO2 state code.  
        /// If provided, the result is filtered to the matching state only.
        /// </param>
        /// <returns>
        /// A JSON result containing a list of <see cref="StateDto"/> objects,  
        /// filtered by state code if specified.
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetStates(string CountryCode, string StateCode = null)
        {
            var apiKey = WebConfigurationManager.AppSettings["X-CSCAPI-KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key not found in Web.config!");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", apiKey);

                // Correct API endpoint per country
                string url = $"https://api.countrystatecity.in/v1/countries/{CountryCode}/states";

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var states = JsonConvert.DeserializeObject<List<StateDto>>(body);

                // If StateCode provided, filter
                if (!string.IsNullOrEmpty(StateCode))
                {
                    states = states
                        .Where(s => string.Equals(s.Iso2, StateCode, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return Json(states, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Retrieves city details for a given country and state from the CountryStateCity API.
        /// </summary>
        /// <param name="countryCode">The ISO2 country code.</param>
        /// <param name="stateCode">The ISO2 state code.</param>
        /// <param name="CityId">
        /// (Optional) The numeric city ID.  
        /// If greater than 0, returns details of a single city including mapped country and state names;  
        /// otherwise, returns all cities in the state.
        /// </param>
        /// <returns>
        /// A JSON result containing either a list of <see cref="CityDto"/> objects (all cities in the state)  
        /// or a single <see cref="CityDto"/> object enriched with country and state names (one city).
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetCities(string countryCode, string stateCode, int CityId = 0)
        {
            var apiKey = WebConfigurationManager.AppSettings["X-CSCAPI-KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key not found in Web.config!");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", apiKey);

                // Cities endpoint
                var url = $"https://api.countrystatecity.in/v1/countries/{countryCode}/states/{stateCode}/cities";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var cities = JsonConvert.DeserializeObject<List<CityDto>>(body);

                if (CityId > 0)
                {
                    var city = cities.FirstOrDefault(c => c.Id == CityId);
                    if (city != null)
                    {
                        // still use ISO2 code here
                        var countryUrl = $"https://api.countrystatecity.in/v1/countries/{countryCode}";
                        var countryRes = await client.GetStringAsync(countryUrl);
                        var country = JsonConvert.DeserializeObject<CountryDto>(countryRes);

                        var stateUrl = $"https://api.countrystatecity.in/v1/countries/{countryCode}/states/{stateCode}";
                        var stateRes = await client.GetStringAsync(stateUrl);
                        var state = JsonConvert.DeserializeObject<StateDto>(stateRes);

                        // Map codes → names
                        city.CountryName = country?.Name;
                        city.StateName = state?.Name;
                    }

                    return Json(city, JsonRequestBehavior.AllowGet);
                }

                return Json(cities, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Retrieves calendar events for the logged-in staff member based on their permissions.
        /// </summary>
        /// <remarks>
        /// - Verifies if the session has a valid staff code.  
        /// - Gets user permissions from the business layer.  
        /// - For each permission, loads the corresponding event data (PR, RFQ, Quotation, PO, GRN, Goods Return, QC).  
        /// - Events are returned in a JSON format ready to bind with calendar components like FullCalendar.
        /// </remarks>
        /// <returns>
        /// A JSON result containing a list of calendar events.  
        /// Each event includes:
        /// <list type="bullet">
        ///   <item><description><c>id</c> – Unique event identifier</description></item>
        ///   <item><description><c>title</c> – Event title</description></item>
        ///   <item><description><c>start</c>/<c>end</c> – Event dates</description></item>
        ///   <item><description><c>className</c>, <c>color</c>, <c>textColor</c> – Event styling</description></item>
        ///   <item><description><c>extendedProps</c> – Extra details depending on the module</description></item>
        /// </list>
        /// If the session has expired, returns <c>{ success = false, message = "Session expired" }</c>.
        /// </returns>
        public async Task<JsonResult> GetEvents()
        {
            var events = new List<object>();

            // Get logged-in staff code from session
            var staffCode = Session["StaffCode"]?.ToString();
            if (string.IsNullOrEmpty(staffCode))
                return Json(new { success = false, message = "Session expired" }, JsonRequestBehavior.AllowGet);

            // Fetch user permissions
            var permissions = await bal.GetReadPermissions(staffCode);

            // Loop through each permission and load events accordingly
            foreach (var perm in permissions)
            {
                switch (perm.PermissionName)
                {
                    case "PurchaseRequisition":
                        // Load Purchase Requisition events
                        var PRList = await bal.PRListPCM();
                        foreach (var pr in PRList)
                        {
                            var PRDetails = await bal.PRDetails(pr.IdCode);
                            events.Add(new
                            {
                                id = pr.IdCode,
                                title = $"Purchase Requisition Is Added By {pr.AddedBy}",
                                start = pr.AddedDate.ToString("yyyy/MM/ddTHH:mm:ss"),
                                color = "#007bff",

                                extendedProps = new
                                {
                                    module = "PurchaseRequisition",
                                    PRCode = PRDetails.PRCode,
                                    RequiredDate = PRDetails.RequiredDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    StatusName = PRDetails.StatusName,
                                    Description = PRDetails.Description,
                                    AddedBy = PRDetails.AddedBy,
                                    AddedDate = PRDetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    ApprovedBy = PRDetails.ApprovedBy,
                                    ApprovedDate = PRDetails.ApprovedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    PriorityName = PRDetails.PriorityName,

                                    Items = PRDetails.Items
                                }
                            });
                        }
                        break;
                    case "RequestForQuotation":
                        // Load RFQ events
                        var RFQList = await bal.RFQListPCM();
                        foreach (var pr in RFQList)
                        {
                            var RFQDetails = await bal.RFQDetails(pr.IdCode);
                            events.Add(new
                            {
                                id = pr.IdCode,
                                title = $"Request For Quotation Is Added By {pr.AddedBy}",
                                start = pr.AddedDate.ToString("yyyy-MM-dd"),
                                end = ((pr.EndDate.Date.AddDays(1) - pr.AddedDate.Date).TotalDays > 7 ? pr.AddedDate.Date.AddDays(7) : pr.EndDate.Date.AddDays(-2)).ToString("yyyy-MM-dd"),
                                color = "#17a2b8",

                                extendedProps = new
                                {
                                    module = "RequestForQuotation",
                                    RFQCode = RFQDetails.RFQCode,
                                    PRCode = RFQDetails.PRCode,
                                    ExpectedDate = RFQDetails.ExpectedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    Description = RFQDetails.Description,
                                    AddedBy = RFQDetails.AddedBy,
                                    AddedDate = RFQDetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    AccountantName = RFQDetails.AccountantName,
                                    AccountantEmail = RFQDetails.AccountantEmail,
                                    DeliveryAddress = RFQDetails.DeliveryAddress,

                                    Items = RFQDetails.Items
                                }
                            });
                        }
                        break;
                    case "RegisterQuotation":
                        // Load Register Quotation events (grouped by date)
                        var RQList = await bal.RQListPCM();
                        foreach (var pr in RQList)
                        {
                            var RQDetails = await bal.RQDetails(pr.AddedDate.ToString("yyyy-MM-dd"));

                            var items = RQDetails.Select(i => new {
                                i.RegisterQuotationCode,
                                i.RFQCode,
                                i.VendorName,
                                i.StatusName,
                                i.AddedBy,
                                DeliveryDate = i.DeliveryDate.HasValue ? i.DeliveryDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                                AddedDate = i.AddedDate.HasValue ? i.AddedDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                                i.ApprovedBy,
                                ApprovedDate = i.ApprovedDate.HasValue ? i.ApprovedDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                                i.ShippingCharges
                            });

                            events.Add(new
                            {
                                id = $"RQ-{pr.AddedDate:yyyyMMdd}",
                                title = $"{pr.Count} Quotations are Registerd By {pr.AddedBy}",
                                start = pr.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                color = "#6f42c1",

                                extendedProps = new
                                {
                                    module = "RegisterQuotation",

                                    Items = items
                                }
                            });
                        }
                        break;
                    case "PurchaseOrder":
                        // Load Purchase Order events
                        var POList = await bal.POListPCM();
                        foreach (var po in POList)
                        {
                            var PODetails = await bal.GetPODetails(po.IdCode);

                            events.Add(new
                            {
                                id = po.IdCode,
                                title = $"Purchase Order Is Added By {po.AddedBy}",
                                start = po.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                color = "#fd7e14",

                                extendedProps = new
                                {
                                    module = "PurchaseOrder",

                                    POCode = PODetails.POCode,
                                    StatusName = PODetails.StatusName,
                                    AddedDate = PODetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    ApprovedDate = PODetails.ApprovedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    TotalAmount = PODetails.TotalAmount,
                                    BillingAddress = PODetails.BillingAddress,
                                    VendorName = PODetails.VendorName,
                                    AddedBy = PODetails.AddedBy,
                                    ApprovedBy = PODetails.ApprovedBy,
                                    AccountantName = PODetails.AccountantName,
                                    ShippingCharges = PODetails.ShippingCharges,

                                    Items = PODetails.Items,

                                    TermConditions = PODetails.TermConditions ?? new List<string>()
                                }
                            });
                        }
                        break;
                    case "GRNInfo":
                        // Load GRN (Goods Receipt Note) events
                        var GRNList = await bal.GRNListPCM();
                        foreach (var grn in GRNList)
                        {
                            var GRNDetails = await bal.GRNDetails(grn.IdCode);

                            var items = GRNDetails.Items.Select(g => new
                            {
                                g.GRNCode,
                                g.GRNItemCode,
                                g.ItemCode,
                                g.ItemName,
                                g.Quantity,
                                g.CostPerUnit,
                                g.Discount,
                                g.TaxRate,
                                g.FinalAmount
                            }).ToList();

                            events.Add(new
                            {
                                id = grn.IdCode,
                                title = $"GRN Is Added By {grn.AddedBy}",
                                start = grn.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                color = "#28a745",

                                extendedProps = new
                                {
                                    module = "GRNInfo",

                                    POCode = GRNDetails.POCode,
                                    GRNCode = GRNDetails.GRNCode,
                                    PODate = GRNDetails.PODate?.ToString("dd/MM/yyyy").Replace("-","/"),
                                    GRNDate = GRNDetails.GRNDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    InvoiceDate = GRNDetails.InvoiceDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    VendorName = GRNDetails.VendorName,
                                    InvoiceCode = GRNDetails.InvoiceCode,
                                    CompanyAddress = GRNDetails.CompanyAddress,
                                    BillingAddress = GRNDetails.BillingAddress,
                                    StatusName = GRNDetails.StatusName,
                                    TotalAmount = GRNDetails.TotalAmount,
                                    ShippingCharges = Convert.ToDecimal(GRNDetails.ShippingCharges),

                                    Items = items,
                                }
                            });
                        }
                        break;
                    case "GoodsReturnInfo":
                        // Load Goods Return events
                        var goodsReturnList = await bal.GRListPCM();
                        foreach (var gr in goodsReturnList)
                        {
                            var GRDetails = await bal.GRDetails(gr.IdCode);
                            events.Add(new
                            {
                                id = gr.IdCode,
                                title = $"Goods Return Entry Is Added By {gr.AddedBy}",
                                start = gr.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                color = "ffc107",

                                extendedProps = new
                                {
                                    module = "GoodsReturnInfo",

                                    GoodsReturnCode = GRDetails.GoodsReturnCode,
                                    GRNCode = GRDetails.GRNCode,
                                    TransporterName = GRDetails.TransporterName,
                                    TransportContactNo = GRDetails.TransportContactNo,
                                    VehicleNo = GRDetails.VehicleNo,
                                    VehicleType = GRDetails.VehicleType,
                                    Reason = GRDetails.Reason,
                                    AddedBy = GRDetails.AddedBy,
                                    AddedDate = GRDetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                                    Status = GRDetails.StatusName,

                                    Items = GRDetails.Items,
                                }
                            });
                        }
                        break;
                    case "QualityCheckInfo":
                        // Load Quality Check events
                        var QCList = await bal.QCListPCM();
                        foreach (var qc in QCList)
                        {
                            var QCDetails = await bal.QCDetails(qc.AddedDate.ToString("yyyy-MM-dd"), qc.Status);

                            var items = QCDetails.Select(i => new {
                                i.QualityCheckCode,
                                i.StatusName,
                                i.GRNItemsCode,
                                i.ItemCode,
                                i.ItemName,
                                i.Quantity,
                                i.InspectionFrequency,
                                i.SampleQualityChecked,
                                i.SampleTestFailed,
                                i.QCAddedBy,
                                QCAddedDate = i.QCAddedDate.HasValue ? i.QCAddedDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                                i.QCFailedAddedBy,
                                QCFailedDate = i.QCFailedDate.HasValue ? i.QCFailedDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                                i.Reason
                            });

                            events.Add(new
                            {
                                id = $"QC-{qc.AddedDate:yyyyMMdd}",
                                title = $"{qc.Count} Items Has {(qc.Status == "Confirmed" ? "Passed" : "Failed")} Quality Check",
                                start = qc.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                color = "#dc3545",

                                extendedProps = new
                                {
                                    module = "QualityCheckInfo",

                                    Items = items,
                                }
                            });
                        }
                        break;
                }
            }

            // Return final events as JSON
            return Json(events, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves the list of unread notifications for the currently logged-in staff member.
        /// </summary>
        /// <returns>
        /// A JSON result containing a list of unread notifications for the user.
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            string staffCode = Session["StaffCode"]?.ToString();
            var data = await bal.GetUnreadNotifications(staffCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves the complete list of notifications (read and unread) 
        /// for the currently logged-in staff member.
        /// </summary>
        /// <returns>
        /// A JSON result containing all notifications for the user.
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetAllNotifications()
        {
            string staffCode = Session["StaffCode"]?.ToString();
            var data = await bal.GetAllNotifications(staffCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Marks a specific notification as read for the currently logged-in staff member.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to mark as read.</param>
        /// <returns>
        /// A JSON result indicating success.
        /// </returns>
        [HttpPost]
        public async Task<JsonResult> MarkAsRead(int id)
        {
            string staffCode = Session["StaffCode"]?.ToString();
            await bal.MarkAsRead(id, staffCode);
            return Json(new { success = true });
        }

        /// <summary>
        /// Marks all unread notifications as read for the currently logged-in staff member.
        /// </summary>
        /// <returns>
        /// A JSON result indicating success.
        /// </returns>
        [HttpPost]
        public async Task<JsonResult> MarkAllAsRead()
        {
            string staffCode = Session["StaffCode"]?.ToString();
            await bal.MarkAllAsRead(staffCode);
            return Json(new { success = true });
        }
    }
}