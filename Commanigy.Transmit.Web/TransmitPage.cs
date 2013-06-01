#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using Commanigy.Transmit.Data;
using System.Web;
using System.Reflection;
using System.Resources;
using System.IO;
using log4net;
#endregion

namespace Commanigy.Transmit.Web {
	public class TransmitPage : System.Web.UI.Page {
		private ILog log = LogManager.GetLogger("web");

		private AuthenticatedUser user = UserHelper.Current;

		public AuthenticatedUser CurrentUser {
			get {
				return user;
			}
		}

		protected string _(string v, params object[] a) {
			return Localizer.t(v, a);
		}
	}
}
