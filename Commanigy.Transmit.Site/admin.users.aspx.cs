#region Using directives
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;

using log4net; 

using Commanigy.Transmit.Data;
using Commanigy.Transmit.Web;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class AdminUsersPage : TransmitPage {
		private ILog log = LogManager.GetLogger("site");

		class LocalUser {
			public string Name { get; set; }
			public string FullName { get; set; }
			public string Description { get; set; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e) {
			if (this.CurrentUser == null || !this.User.IsInRole(TransmitSettings.Instance.Setting.RestrictSettingsToGroup)) {
				log.DebugFormat("Blocking access for user {0} since user isn't in group {1}", this.CurrentUser, TransmitSettings.Instance.Setting.RestrictSettingsToGroup);
				Response.Redirect("~/authenticate.aspx");
				Response.End();
				return;
			}

			if (!Page.IsPostBack) {
				LookupUsers();
			}
		}

		protected void LookupUsers() {
			List<LocalUser> users = new List<LocalUser>();
			DirectoryEntry hostMachineDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName);
			DirectoryEntries entries = hostMachineDirectory.Children;

			foreach (DirectoryEntry entry in entries) {
				if (entry.SchemaClassName == "User") {
					users.Add(new LocalUser { Name = entry.Name,
						FullName = (entry.Properties["FullName"] as PropertyValueCollection).Value as string,
						Description = (entry.Properties["Description"] as PropertyValueCollection).Value as string });
				}
			}

			DlLocalUsers.DataSource = users;
			DlLocalUsers.DataBind();
		}

		protected void BtnCreateUser_Click(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(TbxName.Text) || string.IsNullOrEmpty(TbxPassword.Text)) {
				ValidationError.Display("You need to specify both login name and password");
			}

			if (!Page.IsValid) {
				return;
			}

			if (CreateLocalUser(TbxName.Text, TbxPassword.Text, TbxFullName.Text)) {
				CreateUser(TbxName.Text, TbxFullName.Text);

				(this.Master as SiteMasterPage).Flash("User created");
			}

			LookupUsers();
		}

		private void CreateUser(string name, string fullName) {
			Data.User user = new User() {
				AccountName = name,
				CommonName = name,
				Company = TbxCompany.Text,
				Country = TbxCountry.Text,
				CreatedAt = DateTime.UtcNow,
				Department = TbxDepartment.Text,
				DisplayName = fullName,
				GivenName = name,
				Location = TbxLocation.Text,
				Mail = TbxMail.Text,
				Title = TbxTitle.Text,
				Url = TbxUrl.Text
			};

			using (Data.DataClassesDataContext db = new Data.DataClassesDataContext()) {
				db.Users.InsertOnSubmit(user);
				db.SubmitChanges();
			}
		}

		protected bool CreateLocalUser(string name, string password, string fullName) {
			DirectoryEntry hostMachineDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName);
			DirectoryEntries entries = hostMachineDirectory.Children;
			
			bool exists = false;
			try {
				entries.Find(name);
				exists = true;
			}
			catch (System.Runtime.InteropServices.COMException) {
				// user doesn't exist
			}

			if (!exists) {
				DirectoryEntry obUser = entries.Add(name, "User");
				obUser.Properties["FullName"].Add(fullName);
				obUser.Properties["Description"].Add("Transmit basic user account");
				obUser.Invoke("SetPassword", password);
				obUser.Invoke("Put", new object[] { "UserFlags", 0x10000 + 0x0040 });

				// HACK: this should be implemented in a nicer way
				UserHelper uh = new UserHelper();
				if (uh.impersonateValidUser(ConfigurationManager.AppSettings["Commanigy.Transmit.Site.Admin.UserName"], ConfigurationManager.AppSettings["Commanigy.Transmit.Site.Admin.Domain"], ConfigurationManager.AppSettings["Commanigy.Transmit.Site.Admin.Password"])) {
					log.DebugFormat("Creating user {0} on local machine", name);
					obUser.CommitChanges();
					uh.undoImpersonation();
					return true;
				}
				else {
					log.ErrorFormat("Unable to impersonate user \"{0}\" having rights to create local user. User \"{1}\" NOT created", ConfigurationManager.AppSettings["Commanigy.Transmit.Site.Admin.UserName"], name);
				}
			}

			return false;
		}
	}
}