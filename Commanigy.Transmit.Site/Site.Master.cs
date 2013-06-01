#region Using directives
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Security.Principal;
using System.Threading;
using Commanigy.Transmit.Web;
using Commanigy.Transmit.Data;
using log4net;
using System.Globalization;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class SiteMasterPage : System.Web.UI.MasterPage {
		private ILog log = LogManager.GetLogger("site");

		public bool IsAdministrator {
			get {
				return Page.User.IsInRole(TransmitSettings.Instance.Setting.RestrictSettingsToGroup);
			}
		}

		public bool IsAuthenticated {
			get {
				TransmitPage p = (Page as Commanigy.Transmit.Web.TransmitPage);
				return p != null && p.CurrentUser != null;
			}
		}

		public string MenuLiTag(string pageUrl, string pageTitle, string additionalCssClassNames) {
			return string.Format("<li class=\"{0}\"><a href=\"{1}\">{2}</a></li>", additionalCssClassNames + (IsOnPage("/" + pageUrl) ? " selected" : ""), pageUrl, pageTitle);
		}

		public bool IsOnPage(string pageUrl) {
			return Request.Path.EndsWith(pageUrl);
		}

		protected string BodyCssClasses { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e) {
			this.BodyCssClasses = "hide-help";

			SetCurrentCulture();
		}

		private void SetCurrentCulture() {
			CultureInfo culture = null;

			if (Request["language"] != null) {
				culture = CultureInfo.CreateSpecificCulture(Request["language"]);
			}
			else {
				culture = Session["culture"] as CultureInfo;
			}

			if (culture == null) {
				// set default culture to English in case no culture is set in request nor session
				culture = CultureInfo.CreateSpecificCulture("en-US");
			}

			Session["culture"] = culture;

			Thread.CurrentThread.CurrentCulture = culture;
		}

		protected string _(string v, params object[] a) {
			return Localizer.t(v, a);
		}

		protected string InsertCompanyLogoCss() {
			if (string.IsNullOrEmpty(Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.CompanyLogo)) {
				return string.Empty;
			}

			return @"<style type=""text/css"">
				#n .menu-navigation li.application-name {
					background: transparent url(" + Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.CompanyLogo.Replace("~/", "") + @") no-repeat scroll 0 0;
				}
				</style>";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="v"></param>
		public void Flash(string v) {
			Flash(v, "info");
		}

		public void Flash(string msg, string clazz) {
			LblFlash.Text = string.Format("<div class=\"flash-{0}\">{1}</div>", clazz, msg);
		}
	}
}