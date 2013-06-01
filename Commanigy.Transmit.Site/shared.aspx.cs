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
	public partial class SharedPage : TransmitPage {
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
				throw new HttpException(404, "Package hash not specified in request");
			}

			this.package = LoadPackage(uniqueHash);
			if (this.package == null || this.package.SenderMail != this.CurrentUser.Mail) {
				throw new HttpException(404, "Package '" + uniqueHash + "' not found or you do not have access to it.");
			}

			if (!Page.IsPostBack) {
				LblMessage.Text = package.Message.Replace("\n", "<br />");

//				List<FileInfo> files = Storage.GetFiles(package.Code).ToList<FileInfo>();

				DlFiles.DataSource = package.Files;
				DlFiles.DataBind();

				SharedTo.DataSource = package.Transfers;
				SharedTo.DataBind();
			}
		}

		private Package LoadPackage(string code) {
			Package package;

			using (DataClassesDataContext db = new DataClassesDataContext()) {
				package = db.Packages.SingleOrDefault(p => p.Code == code);
				if (package == null) {
					return null;
				}

				package.Files.Load();
				package.Transfers.Load();
			}

			return package;
		}


		/// <summary>
		/// Download entire package to client computer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnDownload_Click(object sender, EventArgs e) {
			AuthenticatedUser senderUser = Locator.FindByMail(package.SenderMail);

			log.DebugFormat("User {0} downloaded package {1}", this.CurrentUser, package.Code);

			using (DataClassesDataContext db = new DataClassesDataContext()) {
				Package updatingPackage = db.Packages.SingleOrDefault(p => p.Code == package.Code);
				foreach (var f in updatingPackage.Files) {
					f.DownloadCount += 1;
				}

				db.SubmitChanges();
			}

			string archiveFileName;
			if (package.Files.Count == 1 && package.Files[0].FileHash.ToLower().EndsWith(".zip")) {
				archiveFileName = package.Files[0].FileHash;
			}
			else {
				archiveFileName = string.Format("files-from-{0}.zip", (senderUser != null) ? senderUser.DisplayName : package.SenderMail).Replace(" ", "");
			}

			// strip invalid characters from filename
			string invalidCharacters = Regex.Escape(new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars()));
			archiveFileName = Regex.Replace(archiveFileName, "[" + invalidCharacters + "]", "");

			Response.ContentType = "application/x-zip-compressed";
			Response.AppendHeader("Content-Disposition", string.Format("attachment; filename={0}", archiveFileName));

			string filename = Storage.GetFullDirectoryPath(package.Code) + ".zip";

			// check if we already have a .zip archive
			if (System.IO.File.Exists(filename)) {
				//Response.WriteFile(Storage.GetFullDirectoryPath(package.Code) + ".zip");
				WriteFileToOutputStream(filename);
			}
			else {
				FastZip fz = new FastZip();
				fz.CreateZip(Response.OutputStream, Storage.GetFullDirectoryPath(package.Code), true, "", "");
			}

			Response.End();
		}

		private void WriteFileToOutputStream(string filename) {
			Stream outstream = null;
			try {
				outstream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

				// buffer to read 50K bytes in chunk:
				byte[] buffer = new byte[50000];

				long dataToRead = outstream.Length;

				while (dataToRead > 0) {
					// verify that client is still connected
					if (Response.IsClientConnected) {
						int length = outstream.Read(buffer, 0, buffer.Length);

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
			catch (Exception x) {
				log.Error("Failed to fully stream full file to user", x);
			}
			finally {
				if (outstream != null) {
					outstream.Close();
				}
			}
		}


		/// <summary>
		/// Reshare entire package with other users
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnReshare_Click(object sender, EventArgs e) {
			// FIXME: implement this function
		}
	}
}