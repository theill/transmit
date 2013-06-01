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

using log4net; 

using Commanigy.Transmit.Data;
using Commanigy.Transmit.Web;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class AdminCleanupPage : TransmitPage {
		private ILog log = LogManager.GetLogger("site");

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
		}

		private IQueryable<Package> ExpiredPackages(DataClassesDataContext db) {
			return from p in db.Packages
				   where p.ExpiresAt < DateTime.UtcNow &&
				   p.Status == (char)PackageStatus.Open
				   select p;
		}

		private IQueryable<string> OpenPackageCodes(DataClassesDataContext db) {
			return from p in db.Packages
				   where p.ExpiresAt >= DateTime.UtcNow &&
				   p.Status == (char)PackageStatus.Open
				   select p.Code;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_LoadComplete(object sender, EventArgs e) {
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				// get all expired packages after page has loaded and 
				// "cleanup" method has been invoked
				var expiredPackages = ExpiredPackages(db);

				DlExpiredPackages.DataSource = expiredPackages;
				DlExpiredPackages.DataBind();

				DlAvailableDirectories.DataSource = GetAvailableDirectories(db);
				DlAvailableDirectories.DataBind();
			}
		}

		private string[] GetAvailableDirectories(DataClassesDataContext db) {
			var openPackages = OpenPackageCodes(db);
			string[] availableDirectories = Storage.GetDirectories();
			return Array.FindAll(availableDirectories, delegate(string v) {
				return !openPackages.Contains(v);
			});
		}

		protected void BtnCleanup_Click(object sender, EventArgs e) {
			int cleanedPackages = 0;
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				// get all expired packages
				var expiredPackages = ExpiredPackages(db);

				log.DebugFormat("Cleaning out {0} packages which are too old", expiredPackages.Count());
				
				// delete packages from file size and from database
				expiredPackages.ToList().ForEach(delegate(Package p) {
					if (Storage.DeletePath(p.Code)) {
						cleanedPackages++;
						p.Status = (char)PackageStatus.Expired;
						//db.Packages.DeleteOnSubmit(p);
					}
				});

				db.SubmitChanges();
			}

			(this.Master as SiteMasterPage).Flash(string.Format("Cleaned up {0} packages", cleanedPackages));
		}

		protected void BtnDeleteUnattachedPackages_Click(object sender, EventArgs e) {
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				Storage.PurgeDirectories(GetAvailableDirectories(db));
			}
		}
	}
}