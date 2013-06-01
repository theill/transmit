#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using log4net.Config;
using log4net; 
#endregion

namespace Commanigy.Transmit.SiteSupport {
	public class Global : System.Web.HttpApplication {
		private ILog log = LogManager.GetLogger("support");

		protected void Application_Start(object sender, EventArgs e) {
			XmlConfigurator.Configure();
			log.Info("Transmit (support) has been started up");

		}

		protected void Session_Start(object sender, EventArgs e) {

		}

		protected void Application_BeginRequest(object sender, EventArgs e) {

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e) {

		}

		protected void Application_Error(object sender, EventArgs e) {

		}

		protected void Session_End(object sender, EventArgs e) {

		}

		protected void Application_End(object sender, EventArgs e) {

		}
	}
}