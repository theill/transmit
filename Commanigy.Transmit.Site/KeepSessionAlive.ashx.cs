#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// Keeps a session alive by updating a cookie variable each time it's pinged.
	/// </summary>
	public class KeepSessionAlive : IHttpHandler, IRequiresSessionState {

		public void ProcessRequest(HttpContext context) {
			context.Session["KeepSessionAlive"] = DateTime.UtcNow;
		}

		public bool IsReusable {
			get {
				return false;
			}
		}
	}
}