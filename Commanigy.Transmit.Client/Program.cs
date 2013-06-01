#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Commanigy.Transmit.Client.Properties; 
#endregion

namespace Commanigy.Transmit.Client {
	/// <summary>
	/// 
	/// </summary>
	static class Program {
		//private static NotifyIcon icon;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//icon = new NotifyIcon();
			//icon.Text = "Transmit";
			//icon.Icon = Resources.Upload;
			//icon.Visible = true;
			//icon.DoubleClick += new EventHandler(ShowStatusBalloon);

			Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
			Application.Run(new Transmit());
		}

		static void Application_ApplicationExit(object sender, EventArgs e) {
			//icon.Dispose();
			//icon = null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//public static void ShowStatusBalloon(object sender, EventArgs e) {
		//    icon.ShowBalloonTip(5000, "Transmit is uploading files", "Transmit is currently uploading your selected files.", ToolTipIcon.Info);
		//}
	}
}