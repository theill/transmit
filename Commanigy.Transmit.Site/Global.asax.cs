#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;

using log4net.Config;
using log4net;

using Commanigy.Transmit.Web;
using Commanigy.Transmit.Data;
using System.Web;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// Site initialization
	/// </summary>
	public class Global : System.Web.HttpApplication {
		private ILog log = LogManager.GetLogger("site");

		protected void Application_Start(object sender, EventArgs e) {
			XmlConfigurator.Configure();
			log.Info("Transmit has been started up");
		}

		protected void Session_Start(object sender, EventArgs e) {

		}

		protected void Application_BeginRequest(object sender, EventArgs e) {

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {

		}

		protected void Application_Error(object sender, EventArgs e) {
			Exception x = HttpContext.Current.Server.GetLastError();
			log.Error("General exception \"" + (x.InnerException != null ? x.InnerException.Message : x.Message) + "\" was thrown", x);
		}

		protected void Session_End(object sender, EventArgs e) {

		}

		protected void Application_End(object sender, EventArgs e) {

		}
	}
}