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
using Commanigy.Transmit.Data;
using System.Collections.Generic;
using Commanigy.Transmit.Web;
using log4net; 
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class AdminSettingsPage : TransmitPage {
		private ILog log = LogManager.GetLogger("site");

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e) {
			if (this.CurrentUser == null || !this.User.IsInRole(TransmitSettings.Instance.Setting.RestrictSettingsToGroup)) {
				log.DebugFormat("Blocking access for user '{0}' since user isn't in group '{1}'", this.CurrentUser, TransmitSettings.Instance.Setting.RestrictSettingsToGroup);
				Response.Redirect("~/authenticate.aspx");
				Response.End();
				return;
			}

			//log.DebugFormat("User {0} is requesting page '{1}' at {2}", this.CurrentUser, Page.Title, Page.Request.Path);

			if (!Page.IsPostBack) {
				LdsSettings.DataBind();
			}
		}

		protected void Settings_Updating(object sender, LinqDataSourceUpdateEventArgs e) {
			Setting a = (e.NewObject as Setting);
			a.Login = HttpContext.Current.User.Identity.Name;
			a.CreatedAt = DateTime.UtcNow;
		}

		protected void Settings_Updated(object sender, LinqDataSourceStatusEventArgs e) {
			TransmitSettings.Instance.Reload();

			(this.Master as SiteMasterPage).Flash("Settings saved");
		}
	}
}