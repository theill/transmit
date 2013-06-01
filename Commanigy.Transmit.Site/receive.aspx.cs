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
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using log4net;

using ICSharpCode.SharpZipLib.Zip;

using Commanigy.Transmit.Web;
using Commanigy.Transmit.Data;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class ReceivePage : TransmitPage {
		private ILog log = LogManager.GetLogger("site");

		private Package package;
		public Package Package {
			get {
				return package;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e) {
			//log.DebugFormat("User {0} is requesting page '{1}' at {2}", this.CurrentUser, Page.Title, Page.Request.Path);

			string uniqueHash = Request["h"]; // package.code
			if (string.IsNullOrEmpty(uniqueHash)) {
				throw new HttpException(404, "Package hash not specified");
			}

			package = Package.FindByCode(uniqueHash);
			if (package == null) {
				throw new HttpException(404, "Package " + uniqueHash + " not found");
			}

			if (!Page.IsPostBack) {
				ImgSharedBy.Attributes["onerror"] = "null;this.src='images/no-user-picture.png'";
				ImgSharedBy.Attributes["onload"] = "this.style.backgroundImage='none'";

				AuthenticatedUser senderUser = Locator.FindByMail(package.SenderMail);
				if (senderUser != null) {
					ImgSharedBy.ImageUrl = "Pictures.ashx?p=images/no-user-picture.png&u=" + Server.UrlEncode(senderUser.Url);
					ImgSharedBy.ToolTip = string.Format("Avatar for {0}", senderUser.DisplayName);
					LblSender.Text = senderUser.DisplayName;
				}
				else {
					ImgSharedBy.ImageUrl = "images/no-user-picture.png";
					ImgSharedBy.ToolTip = "";
					LblSender.Text = package.SenderMail;
				}
				LblMessage.Text = package.Message.Replace("\n", "<br />");

				DlFiles.DataSource = package.Files;
				DlFiles.DataBind();
			}
		}

		/// <summary>
		/// Download entire package to client computer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnDownload_Click(object sender, EventArgs e) {
			List<FileInfo> files = new List<FileInfo>();
			foreach (Commanigy.Transmit.Data.File f in package.Files) {
				files.AddRange(Storage.GetFiles(package.Code));
			}

			if (files.Count == 1 && files[0].Extension.ToLower() == ".zip") {
				Response.ContentType = "application/x-zip-compressed";
				Response.AppendHeader("Content-Disposition", string.Format("attachment;filename={0}", files[0].Name.ToLower()));
				Response.TransmitFile(files[0].FullName);
				Response.End();
				return;
			}

			AuthenticatedUser senderUser = Locator.FindByMail(package.SenderMail);

			string archiveFileName = string.Format("files-from-{0}.zip", (senderUser != null) ? senderUser.DisplayName : package.SenderMail).Replace(" ", "");

			string invalidString = Regex.Escape(new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars()));
			archiveFileName = Regex.Replace(archiveFileName, "[" + invalidString + "]", "");

			Response.ContentType = "application/x-zip-compressed";
			Response.AppendHeader("Content-Disposition", string.Format("attachment; filename={0}", archiveFileName.ToLower()));

			string filename = Storage.GetFullDirectoryPath(package.Code) + ".zip";

			// check if we already have a .zip archive
			if (System.IO.File.Exists(filename)) {
				//Response.WriteFile(Storage.GetFullDirectoryPath(package.Code) + ".zip");

				Stream iStream = null;
				try {
					iStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

					// buffer to read 10K bytes in chunk:
					byte[] buffer = new byte[10000];

					long dataToRead = iStream.Length;

					while (dataToRead > 0) {
						// Verify that the client is connected.
						if (Response.IsClientConnected) {
							int length = iStream.Read(buffer, 0, buffer.Length);

							Response.OutputStream.Write(buffer, 0, length);
							Response.Flush();

							buffer = new byte[buffer.Length];
							dataToRead = dataToRead - length;
						}
						else {
							// prevent infinite loop if user disconnects
							dataToRead = -1;
						}
					}
				}
				catch (Exception ex) {
					log.Error("Failed to fully stream file to user", ex);
				}
				finally {
					if (iStream != null) {
						iStream.Close();
					}
				}
			}
			else {
				FastZip fz = new FastZip();
				fz.CreateZip(Response.OutputStream, Storage.GetFullDirectoryPath(package.Code), true, "", "");
			}

			Response.End();
		}
	}
}