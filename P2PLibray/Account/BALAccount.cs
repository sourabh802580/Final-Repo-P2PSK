using P2PHelper;
using P2PLibray.Quality;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLibray.Account
{
    public class BALAccount
    {
        MSSQL sql = new MSSQL();

        /// <summary>
        /// Attempts to log in a user by validating the provided email and password
        /// against the database.
        /// </summary>
        /// <param name="acc">The account object containing the email address and password for login.</param>
        /// <returns>
        /// An <see cref="Account"/> object with StaffCode and DepartmentId populated
        /// if the login is successful; otherwise, returns the same object with
        /// empty/zero values.
        /// </returns>
        public async Task<Account> Login(Account acc)
        {
            Dictionary<string,string> param = new Dictionary<string,string>();
            param.Add("@Flag","Login");
            param.Add("@Email",acc.EmailAddress);
            param.Add("@Password",acc.Password);

            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param);
            if (dr.Read())
            {
                acc.StaffCode = dr.IsDBNull(dr.GetOrdinal("StaffCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("StaffCode"));
                acc.DepartmentId = dr.IsDBNull(dr.GetOrdinal("DepartmentId")) ? 0 : dr.GetInt32(dr.GetOrdinal("DepartmentId"));
                acc.RoleId = dr.IsDBNull(dr.GetOrdinal("RoleId")) ? 0 : dr.GetInt32(dr.GetOrdinal("RoleId"));

            }

            return acc;
        }

        /// <summary>
        /// Checks whether an email exists in the system and retrieves the corresponding staff code.
        /// </summary>
        /// <param name="acc">The account object containing the email address to check.</param>
        /// <returns>The staff code associated with the email if found; otherwise, an empty string.</returns>
        public async Task<string> CheckEmail(Account acc)
        {
            string code = string.Empty;
            
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "CheckEmail");
            param.Add("@Email",acc.EmailAddress);

            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param);
            if (dr.Read())
            {
                code = dr.IsDBNull(dr.GetOrdinal("StaffCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("StaffCode"));
            }

            return code;
        }

        /// <summary>
        /// Changes the password of a staff member.
        /// </summary>
        /// <param name="acc">The account object containing StaffCode and new Password.</param>
        public async Task ChangePassword(Account acc)
        {
            Dictionary<string,string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ChangePassword");
            param.Add("@Password", acc.Password);
            param.Add("@StaffCode", acc.StaffCode);
            await sql.ExecuteStoredProcedure("AccountProcedure", param);
        }

        /// <summary>
        /// Retrieves complete user profile details for a given staff member,
        /// including personal information, contact details, and assigned permissions.
        /// </summary>
        /// <param name="StaffCode">The unique staff code used to identify the user.</param>
        /// <returns>
        /// An <see cref="Account"/> object containing profile information such as
        /// name, department, role, contact details, addresses, and permissions.
        /// </returns>

        public async Task<Account> UserProfileDetails(string StaffCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UserProfilePCM");
            param.Add("@StaffCode", StaffCode);

            var acc = new Account();

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    acc.UserName = dr.IsDBNull(dr.GetOrdinal("FullName")) ? string.Empty : dr.GetString(dr.GetOrdinal("FullName"));
                    acc.StaffCode = dr.IsDBNull(dr.GetOrdinal("StaffCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("StaffCode"));
                    acc.Department = dr.IsDBNull(dr.GetOrdinal("DepartmentName")) ? string.Empty : dr.GetString(dr.GetOrdinal("DepartmentName"));
                    acc.RoleName = dr.IsDBNull(dr.GetOrdinal("RoleName")) ? string.Empty : dr.GetString(dr.GetOrdinal("RoleName"));
                    acc.ProfilePhoto = dr.IsDBNull(dr.GetOrdinal("Location")) ? string.Empty : dr.GetString(dr.GetOrdinal("Location"));
                    acc.JoiningDate = dr.IsDBNull(dr.GetOrdinal("JoiningDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("JoiningDate"));
                    acc.Gender = dr.IsDBNull(dr.GetOrdinal("Gender")) ? string.Empty : dr.GetString(dr.GetOrdinal("Gender"));
                    acc.DateOfBirth = dr.IsDBNull(dr.GetOrdinal("DOB")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("DOB"));
                    acc.BloodGroup = dr.IsDBNull(dr.GetOrdinal("BloodGroup")) ? string.Empty : dr.GetString(dr.GetOrdinal("BloodGroup"));
                    acc.PhoneNumber = dr.IsDBNull(dr.GetOrdinal("ContactNo")) ? string.Empty : dr.GetInt64(dr.GetOrdinal("ContactNo")).ToString();
                    acc.AlternamteNumber = dr.IsDBNull(dr.GetOrdinal("AlternameNumber")) ? string.Empty : dr.GetInt64(dr.GetOrdinal("AlternameNumber")).ToString();
                    acc.EmailAddress = dr.IsDBNull(dr.GetOrdinal("EmailAddress")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmailAddress"));
                    acc.MotherName = dr.IsDBNull(dr.GetOrdinal("MotherName")) ? string.Empty : dr.GetString(dr.GetOrdinal("MotherName"));
                    acc.SameLocation = dr.IsDBNull(dr.GetOrdinal("SameLocation")) ? false : dr.GetBoolean(dr.GetOrdinal("SameLocation"));
                    acc.LocalLocation = dr.IsDBNull(dr.GetOrdinal("LocalLocation")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalLocation"));
                    acc.LocalLandmark = dr.IsDBNull(dr.GetOrdinal("LocalLandmark")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalLandmark"));
                    acc.LocalPincode = dr.IsDBNull(dr.GetOrdinal("LocalPincode")) ? 0 : dr.GetInt32(dr.GetOrdinal("LocalPincode"));
                    acc.ParmanentLocation = dr.IsDBNull(dr.GetOrdinal("ParmanentLocation")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentLocation"));
                    acc.ParmanentLandmark = dr.IsDBNull(dr.GetOrdinal("ParmanentLandmark")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentLandmark"));
                    acc.ParmanentPincode = dr.IsDBNull(dr.GetOrdinal("ParmanentPincode")) ? 0 : dr.GetInt32(dr.GetOrdinal("ParmanentPincode"));
                    acc.CountryCode = dr.IsDBNull(dr.GetOrdinal("LocalCountryCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalCountryCode"));
                    acc.StateCode = dr.IsDBNull(dr.GetOrdinal("LocalStateCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalStateCode"));
                    acc.CityId = dr.IsDBNull(dr.GetOrdinal("LocalCityId")) ? 0 : dr.GetInt32(dr.GetOrdinal("LocalCityId"));
                    acc.ExtraCountryCode = dr.IsDBNull(dr.GetOrdinal("ParmanentCountryCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentCountryCode"));
                    acc.ExtraStateCode = dr.IsDBNull(dr.GetOrdinal("ParmanentStateCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentStateCode"));
                    acc.ExtraCityId = dr.IsDBNull(dr.GetOrdinal("ParmanentCityId")) ? 0 : dr.GetInt32(dr.GetOrdinal("ParmanentCityId"));
                }
                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        acc.PermissionsData.Add(new Permissions
                        {
                            PermissionName = dr.IsDBNull(dr.GetOrdinal("PermissionName")) ? string.Empty : dr.GetString(dr.GetOrdinal("PermissionName")),
                            HasPermission = dr.IsDBNull(dr.GetOrdinal("HasPermission")) ? 0 : dr.GetInt32(dr.GetOrdinal("HasPermission"))
                        });
                    }
                }
            }
            return acc;
        }

        /// <summary>
        /// Retrieves basic user profile data (summary) for a staff member.
        /// </summary>
        /// <param name="StaffCode">The staff code of the user.</param>
        /// <returns>An Account object with basic user profile data.</returns>
        public async Task<Account> UserProfileData(string StaffCode)
        {
            Account acc = new Account();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UserDataPCM");
            param.Add("@StaffCode", StaffCode);

            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param);
            if (dr.Read())
            {
                acc.UserName = dr.IsDBNull(dr.GetOrdinal("FullName")) ? string.Empty : dr.GetString(dr.GetOrdinal("FullName"));
                acc.Department = dr.IsDBNull(dr.GetOrdinal("DepartmentName")) ? string.Empty : dr.GetString(dr.GetOrdinal("DepartmentName"));
                acc.RoleName = dr.IsDBNull(dr.GetOrdinal("RoleName")) ? string.Empty : dr.GetString(dr.GetOrdinal("RoleName"));
                acc.ProfilePhoto = dr.IsDBNull(dr.GetOrdinal("Location")) ? string.Empty : dr.GetString(dr.GetOrdinal("Location"));
            }

            return acc;
        }

        /// <summary>
        /// Updates the profile information of a staff member.
        /// </summary>
        /// <param name="acc">The account object containing updated profile information.</param>
        public async Task UpdateUserProfile(Account acc)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UpdateUserProfilePCM");
            param.Add("@AlternateNumber", acc.AlternamteNumber);
            param.Add("@StaffCode", acc.StaffCode);
            await sql.ExecuteStoredProcedure("AccountProcedure", param);
        }

        /// <summary>
        /// Retrieves the list of read-only permissions assigned to a staff member.
        /// </summary>
        /// <param name="StaffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="Permissions"/> objects containing the names of
        /// read permissions granted to the user.
        /// </returns>
        public async Task<List<Permissions>> GetReadPermissions(string StaffCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetReadPermissionsPCM");
            param.Add("@StaffCode", StaffCode);

            var acc = new List<Permissions>();

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Permissions
                    {
                        PermissionName = dr.IsDBNull(dr.GetOrdinal("Permissions")) ? string.Empty : dr.GetString(dr.GetOrdinal("Permissions"))
                    });
                }
            }
            return acc;
        }

        /// <summary>
        /// Retrieves all notifications (both read and unread) for the specified staff member.
        /// </summary>
        /// <param name="staffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="NotificationProperty"/> objects containing notification details.
        /// </returns>
        public async Task<List<NotificationProperty>> GetAllNotifications(string staffCode)
        {
            var result = new List<NotificationProperty>();

            var parameters = new Dictionary<string, string>
            {
                { "@Flag","GetAllNotifications" },
                { "@StaffCode", staffCode }
            };

            DataSet ds = await sql.ExecuteStoredProcedureReturnDS("AccountProcedure", parameters);

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    result.Add(new NotificationProperty
                    {
                        NotificationId = Convert.ToInt32(row["NotificationId"]),
                        StaffCode = row["StaffCode"].ToString(),
                        NotificationMessage = row["NotificationMessage"].ToString(),
                        IsRead = Convert.ToBoolean(row["IsRead"])
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves only unread notifications for the specified staff member.
        /// </summary>
        /// <param name="staffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="NotificationProperty"/> objects containing unread notification details.
        /// </returns>
        public async Task<List<NotificationProperty>> GetUnreadNotifications(string staffCode)
        {
            var result = new List<NotificationProperty>();

            var parameters = new Dictionary<string, string>
            {
                { "@Flag","GetUnreadNotifications" },
                { "@StaffCode", staffCode }
            };

            DataSet ds = await sql.ExecuteStoredProcedureReturnDS("AccountProcedure", parameters);

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    result.Add(new NotificationProperty
                    {
                        NotificationId = Convert.ToInt32(row["NotificationId"]),
                        StaffCode = row["StaffCode"].ToString(),
                        NotificationMessage = row["NotificationMessage"].ToString(),
                        IsRead = Convert.ToBoolean(row["IsRead"])
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Marks a single notification as read for the specified staff member.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to mark as read.</param>
        /// <param name="staffCode">The staff code of the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MarkAsRead(int id, string staffCode)
        {
            var parameters = new Dictionary<string, string>
            {
                { "@Flag","MarkSingleRead" },
                { "@NotificationId", id.ToString() },
                { "@StaffCode", staffCode }
            };

            await sql.ExecuteStoredProcedure("AccountProcedure", parameters);
        }

        /// <summary>
        /// Marks all notifications as read for the specified staff member.
        /// </summary>
        /// <param name="staffCode">The staff code of the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MarkAllAsRead(string staffCode)
        {
            var parameters = new Dictionary<string, string>
            {
                { "@Flag","MarkAllRead" },
                { "@StaffCode", staffCode }
            };

            await sql.ExecuteStoredProcedure("AccountProcedure", parameters);
        }

        /// <summary>
        /// Retrieves the complete list of permissions assigned to a staff member,
        /// including both permission types and their names.
        /// </summary>
        /// <param name="StaffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="Permissions"/> objects containing both the type
        /// of permission and its corresponding name.
        /// </returns>
        public async Task<List<Permissions>> GetAllPermissions(string StaffCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetAllPermissionsPcm");
            param.Add("@StaffCode", StaffCode);

            var acc = new List<Permissions>();

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Permissions
                    {
                        PermissionType = dr.IsDBNull(dr.GetOrdinal("Permissions")) ? string.Empty : dr.GetString(dr.GetOrdinal("Permissions")),
                        PermissionName = dr.IsDBNull(dr.GetOrdinal("PermissionName")) ? string.Empty : dr.GetString(dr.GetOrdinal("PermissionName"))
                    });
                }
            }
            return acc;
        }

        /// <summary>
        /// Retrieves a list of Purchase Requisitions (PRs) for calendar display.
        /// </summary>
        /// <returns>A list of Account objects containing PR information.</returns>
        public async Task<List<Account>> PRListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowPRsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("PRCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("PRCode")),
                        Status = dr.IsDBNull(dr.GetOrdinal("StatusName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StatusName")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed information for a specific Purchase Requisition (PR),
        /// including items and metadata.
        /// </summary>
        /// <param name="code">The PR code.</param>
        /// <returns>A CalendarEventData object with PR details and items.</returns>
        public async Task<CalendarEventData> PRDetails(string code)
        {
            var prDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "PRDetailsPCM");
            param.Add("@Code",code);
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    prDetails.PRCode = dr["PRCode"].ToString(); 
                    prDetails.RequiredDate = dr["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(dr["RequiredDate"]) : (DateTime?)null; 
                    prDetails.StatusName = dr["StatusName"].ToString(); 
                    prDetails.Description = dr["Description"].ToString(); 
                    prDetails.AddedBy = dr["AddedBy"].ToString(); 
                    prDetails.AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null; 
                    prDetails.ApprovedBy = dr["ApprovedBy"]?.ToString(); 
                    prDetails.ApprovedDate = dr["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(dr["ApprovedDate"]) : (DateTime?)null; 
                    prDetails.PriorityName = dr["PriorityName"]?.ToString();
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        prDetails.Items.Add(new ItemData
                        {
                            PRCode = dr["PRCode"].ToString(),
                            PRItemCode = dr["PRItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            RequiredQuantity = dr["RequiredQuantity"] != DBNull.Value ? Convert.ToInt32(dr["RequiredQuantity"]) : 0
                        });
                    }
                }
            }

            return prDetails;
        }

        /// <summary>
        /// Retrieves a list of RFQs (Request for Quotation) to display on the calendar.
        /// </summary>
        /// <returns>A list of Account objects containing RFQ details.</returns>
        public async Task<List<Account>> RFQListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowRFQsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("RFQCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("RFQCode")),
                        Status = dr.IsDBNull(dr.GetOrdinal("StatusName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StatusName")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        EndDate = dr.IsDBNull(dr.GetOrdinal("ExpectedDate")) ? DateTime.Today : dr.GetDateTime(dr.GetOrdinal("ExpectedDate"))
                    });
                }
            }

            return acc;
        }

        //// <summary>
        /// Retrieves detailed information about a specific RFQ (Request for Quotation),
        /// including related items.
        /// </summary>
        /// <param name="code">The RFQ code.</param>
        /// <returns>A CalendarEventData object with RFQ details and items.</returns>
        public async Task<CalendarEventData> RFQDetails(string code)
        {
            var rfqDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "RFQDetailsPCM");
            param.Add("@Code", code);
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    rfqDetails.RFQCode = dr["RFQCode"].ToString();
                    rfqDetails.PRCode = dr["PRCode"].ToString();
                    rfqDetails.AddedBy = dr["AddedBy"].ToString();
                    rfqDetails.AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null;
                    rfqDetails.ExpectedDate = dr["ExpectedDate"] != DBNull.Value ? Convert.ToDateTime(dr["ExpectedDate"]) : (DateTime?)null;
                    rfqDetails.Description = dr["Description"].ToString();
                    rfqDetails.AccountantName = dr["AccountantName"].ToString();
                    rfqDetails.AccountantEmail = dr["EmailAddress"].ToString();
                    rfqDetails.DeliveryAddress = dr["DeliveryAddress"].ToString();
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        rfqDetails.Items.Add(new ItemData
                        {
                            RFQCode = dr["RFQCode"]?.ToString(),
                            PRItemCode = dr["PRItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            RequiredQuantity = dr["RequiredQuantity"] != DBNull.Value ? Convert.ToInt32(dr["RequiredQuantity"]) : 0
                        });
                    }
                }
            }

            return rfqDetails;
        }

        /// <summary>
        /// Retrieves a list of RQs (Register Quotations) aggregated by date to display on the calendar.
        /// </summary>
        /// <returns>A list of Account objects containing RQ counts and added details.</returns>
        public async Task<List<Account>> RQListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowRQsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        Count = dr.IsDBNull(dr.GetOrdinal("EntryCount")) ? 0 : dr.GetInt32(dr.GetOrdinal("EntryCount")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed RQ (Register Quotation) information for a given date.
        /// </summary>
        /// <param name="date">The date for which RQ details are retrieved.</param>
        /// <returns>A list of CalendarEventData objects with RQ details.</returns>
        public async Task<List<CalendarEventData>> RQDetails(string date)
        {
            var RqDetails = new List<CalendarEventData>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "RQDetailsPCM");
            param.Add("@Date", date);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    RqDetails.Add(new CalendarEventData
                    {
                        RegisterQuotationCode = dr["RegisterQuotationCode"].ToString(),
                        RFQCode = dr["RFQCode"].ToString(),
                        VendorName = dr["VenderName"].ToString(),
                        DeliveryDate = dr.IsDBNull(dr.GetOrdinal("DeliveryDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("DeliveryDate")),
                        StatusName = dr["StatusName"].ToString(),
                        AddedBy = dr["AddedBy"].ToString(),
                        ApprovedBy = dr["ApprovedBy"].ToString(),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        ApprovedDate = dr.IsDBNull(dr.GetOrdinal("ApprovedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("ApprovedDate")),
                        ShippingCharges = dr.IsDBNull(dr.GetOrdinal("ShippingCharges")) ? 0 : dr.GetDecimal(dr.GetOrdinal("ShippingCharges")),
                    });
                }
            }

            return RqDetails;
        }

        /// <summary>
        /// Retrieves a list of POs (Purchase Orders) to display on the calendar.
        /// </summary>
        /// <returns>A list of Account objects containing PO details.</returns>
        public async Task<List<Account>> POListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowPOsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("POCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("POCode")),
                        Status = dr.IsDBNull(dr.GetOrdinal("StatusName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StatusName")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed information about a specific Purchase Order (PO),
        /// including items and terms/conditions.
        /// </summary>
        /// <param name="code">The PO code.</param>
        /// <returns>A CalendarEventData object with PO details, items, and terms.</returns>
        public async Task<CalendarEventData> GetPODetails(string code)
        {
            var podetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "PODetailsPCM");
            param.Add("@Code", code);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    podetails.POCode = dr["POCode"]?.ToString();
                    podetails.StatusName = dr["StatusName"]?.ToString();
                    podetails.AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null;
                    podetails.ApprovedDate = dr["ApprovedRejectedDate"] != DBNull.Value ? Convert.ToDateTime(dr["ApprovedRejectedDate"]) : (DateTime?)null;
                    podetails.TotalAmount = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0;
                    podetails.BillingAddress = dr["BillingAddress"]?.ToString();
                    podetails.VendorName = dr["VenderName"]?.ToString();
                    podetails.AddedBy = dr["AddedBy"]?.ToString();
                    podetails.ApprovedBy = dr["ApprovedBy"]?.ToString();
                    podetails.ShippingCharges = dr["ShippingCharges"] != DBNull.Value ? Convert.ToDecimal(dr["ShippingCharges"]) : 0;
                    podetails.AccountantName = dr["AccountantName"]?.ToString();
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        podetails.Items.Add(new ItemData
                        {
                            POCode = dr["POCode"]?.ToString(),
                            POItemCode = dr["POItemCode"]?.ToString(),
                            RQItemCode = dr["RQItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            CostPerUnit = dr["CostPerUnit"] != DBNull.Value ? Convert.ToDecimal(dr["CostPerUnit"]) : (decimal?)null,
                            Discount = dr.IsDBNull(dr.GetOrdinal("Discount")) ? 0 : dr.GetInt32(dr.GetOrdinal("Discount")),
                            Quantity = dr["Quantity"] != DBNull.Value ? Convert.ToInt64(dr["Quantity"]) : (long?)null,
                            StatusName = dr["StatusName"].ToString()
                        });
                    }
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        var term = dr["TermConditionName"]?.ToString();
                        if (!string.IsNullOrEmpty(term))
                            podetails.TermConditions.Add(term);
                    }
                }
            }

            return podetails;
        }

        /// <summary>
        /// Retrieves a list of GRNs (Goods Receipt Notes) to display on the calendar.
        /// </summary>
        /// <returns>A list of Account objects containing GRN details.</returns>
        public async Task<List<Account>> GRNListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowGRNsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("GRNCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("GRNCode")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed information about a specific GRN (Goods Receipt Note),
        /// including items received.
        /// </summary>
        /// <param name="code">The GRN code.</param>
        /// <returns>A CalendarEventData object with GRN details and items.</returns>
        public async Task<CalendarEventData> GRNDetails(string code)
        {
            var grnDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNDetailsPCM");
            param.Add("@Code", code);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    grnDetails.POCode = dr["POCode"].ToString();
                    grnDetails.GRNCode = dr["GRNCode"].ToString();
                    grnDetails.PODate = dr.IsDBNull(dr.GetOrdinal("PODate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("PODate"));
                    grnDetails.GRNDate = dr.IsDBNull(dr.GetOrdinal("GRNDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("GRNDate"));
                    grnDetails.InvoiceDate = dr.IsDBNull(dr.GetOrdinal("InvoiceDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("InvoiceDate"));
                    grnDetails.VendorName = dr["VenderName"].ToString();
                    grnDetails.InvoiceCode = dr["InvoiceNo"].ToString();
                    grnDetails.CompanyAddress = dr["CompanyAddress"].ToString();
                    grnDetails.BillingAddress = dr["BillingAddress"].ToString();
                    grnDetails.TotalAmount = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0;
                    grnDetails.ShippingCharges = dr["ShippingCharges"] != DBNull.Value ? Convert.ToDecimal(dr["ShippingCharges"]) : 0;
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        grnDetails.Items.Add(new ItemData
                        {
                            GRNCode = dr.IsDBNull(dr.GetOrdinal("GRNCode")) ? null : dr.GetString(dr.GetOrdinal("GRNCode")),
                            GRNItemCode = dr.IsDBNull(dr.GetOrdinal("GRNItemCode")) ? null : dr.GetString(dr.GetOrdinal("GRNItemCode")),
                            ItemCode = dr.IsDBNull(dr.GetOrdinal("ItemCode")) ? null : dr.GetString(dr.GetOrdinal("ItemCode")),
                            ItemName = dr.IsDBNull(dr.GetOrdinal("ItemName")) ? "-" : dr.GetString(dr.GetOrdinal("ItemName")),
                            Quantity = dr.IsDBNull(dr.GetOrdinal("UnitQuantity")) ? 0 : dr.GetInt64(dr.GetOrdinal("UnitQuantity")),
                            CostPerUnit = dr.IsDBNull(dr.GetOrdinal("CostPerUnit")) ? 0 : dr.GetDecimal(dr.GetOrdinal("CostPerUnit")),
                            Discount = dr.IsDBNull(dr.GetOrdinal("Discount")) ? 0 : dr.GetInt32(dr.GetOrdinal("Discount")),
                            TaxRate = dr.IsDBNull(dr.GetOrdinal("TaxRate")) ? "-" : dr.GetString(dr.GetOrdinal("TaxRate")),
                            FinalAmount = dr.IsDBNull(dr.GetOrdinal("FinalAmount")) ? 0 : dr.GetDecimal(dr.GetOrdinal("FinalAmount"))
                        });
                    }
                }
            }

            return grnDetails;
        }

        /// <summary>
        /// Retrieves a list of GRs (Goods Returns) to display on the calendar.
        /// </summary>
        /// <returns>A list of Account objects containing Goods Return details.</returns>
        public async Task<List<Account>> GRListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowGRsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("GoodReturnCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("GoodReturnCode")),
                        Status = dr.IsDBNull(dr.GetOrdinal("StatusName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StatusName")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed information about a specific Goods Return,
        /// including returned items.
        /// </summary>
        /// <param name="code">The Goods Return code.</param>
        /// <returns>A CalendarEventData object with Goods Return details and items.</returns>
        public async Task<CalendarEventData> GRDetails(string code)
        {
            var grDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRDetailsPCM");
            param.Add("@Code", code);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    grDetails.GoodsReturnCode = dr["GoodReturnCode"].ToString();
                    grDetails.GRNCode = dr["GRNCode"].ToString();
                    grDetails.TransporterName = dr["TransporterName"].ToString();
                    grDetails.TransportContactNo = dr["TransportContactNo"].ToString();
                    grDetails.VehicleNo = dr["VehicleNo"].ToString();
                    grDetails.VehicleType = dr["VehicleType"].ToString();
                    grDetails.Reason = dr["ReasonOfRejection"].ToString();
                    grDetails.AddedBy = dr["AddedBy"].ToString();
                    grDetails.AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"));
                    grDetails.StatusName = dr["StatusName"].ToString();

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            grDetails.Items.Add(new ItemData
                            {
                                GRItemCode = dr["GRItemCode"].ToString(),
                                ItemCode = dr["ItemCode"].ToString(),
                                ItemName = dr["ItemName"].ToString(),
                                Reason = dr["Reason"].ToString()
                            });
                        }
                    }
                }
            }

            return grDetails;
        }

        /// <summary>
        /// Retrieves a list of QCs (Quality Checks) aggregated by date/status for calendar display.
        /// </summary>
        /// <returns>A list of Account objects containing QC counts and related details.</returns>
        public async Task<List<Account>> QCListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowQCsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        Count = dr.IsDBNull(dr.GetOrdinal("EntryCount")) ? 0 : dr.GetInt32(dr.GetOrdinal("EntryCount")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        Status = dr.IsDBNull(dr.GetOrdinal("StatusName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StatusName")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed QC (Quality Check) information for a given date and status.
        /// </summary>
        /// <param name="date">The date for which QC details are retrieved.</param>
        /// <param name="status">The QC status (e.g., Passed, Failed).</param>
        /// <returns>A list of CalendarEventData objects with QC details.</returns>
        public async Task<List<CalendarEventData>> QCDetails(string date,string status)
        {
            var qcdetails = new List<CalendarEventData>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "QCDetailsPCM");
            param.Add("@Date", date);
            param.Add("@StatusName", status);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    try
                    {
                        qcdetails.Add(new CalendarEventData
                        {
                            QualityCheckCode = dr["QualityCheckCode"]?.ToString(),
                            StatusName = dr["StatusName"]?.ToString(),
                            GRNItemsCode = dr["GRNItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            QCAddedBy = dr["QCAddedBy"]?.ToString(),
                            QCFailedAddedBy = dr["QCFailedAddedBy"]?.ToString(),
                            Reason = dr["Reason"]?.ToString(),
                            InspectionFrequency = dr["InspectionFrequency"] == DBNull.Value ? 0 : Convert.ToInt32(dr["InspectionFrequency"]),
                            SampleQualityChecked = dr["SampleQualityChecked"] == DBNull.Value ? 0 : Convert.ToInt64(dr["SampleQualityChecked"]),
                            SampleTestFailed = dr["SampleTestFailed"] == DBNull.Value ? 0 : Convert.ToInt64(dr["SampleTestFailed"]),
                            QCAddedDate = dr["QCAddedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["QCAddedDate"]),
                            QCFailedDate = dr["QCFailedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["QCFailedDate"]),
                            Quantity = dr["Quantity"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Quantity"]),
                        });

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading QC record: " + ex.Message);
                        throw;
                    }

                }
            }

            return qcdetails;
        }
    }
}
