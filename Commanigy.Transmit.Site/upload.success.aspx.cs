﻿#region Using directives
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
	/// </summary>
	public partial class UploadSuccessPage : TransmitPage {
		private ILog log = LogManager.GetLogger("site");

		/// <summary>
		/// User currently logged on to system
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e) {
			//log.DebugFormat("User {0} is requesting page '{1}' at {2}", this.CurrentUser, Page.Title, Page.Request.Path);
		}
	}
}
