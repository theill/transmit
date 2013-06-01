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
using System.Data.Linq;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class AdminPackagesPage : TransmitPage {
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_LoadComplete(object sender, EventArgs e) {
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				var packages = (from p in db.Packages
								where p.Status == (char)PackageStatus.Open
								  orderby p.CreatedAt descending
								  select p).Take(100);

				LvPackages.DataSource = packages;
				LvPackages.DataBind();
			}
		}

		protected string FilesList(object f) {
			EntitySet<File> files = f as EntitySet<File>;
			return string.Join(", ", files.Select<File, string>(a => a.FileHash).Take(10).ToArray());
		}

		protected string FormatLevel(string level) {
			return string.Format("<div class=\"level {0}\" title=\"{0}\">{1}</div>", level.ToLower(), level.Substring(0, 1));
		}

		protected string FormatToggleImage(object exception) {
			if (string.IsNullOrEmpty(exception as string)) {
				return "<img src=\"images/information-white.png\" width=\"16\" height=\"16\" />";
			}

			return "<a href=\"#\" class=\"toggle-exception\"><img src=\"images/plus-white.png\" width=\"16\" height=\"16\" /></a>";
		}
	}
}