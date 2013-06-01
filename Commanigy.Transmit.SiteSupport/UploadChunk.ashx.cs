#region Using directives
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.IO;
using Commanigy.Transmit.Web;
using log4net;
using Commanigy.Transmit.Data; 
#endregion

namespace Commanigy.Transmit.SiteSupport {
	/// <summary>
	/// Handles a single upload at a time.
	/// </summary>
	public class UploadChunk : IHttpHandler {
		private ILog log = LogManager.GetLogger("support");

		public void ProcessRequest(HttpContext context) {
			try {
				//log.DebugFormat("Start processing request from {0} with a content-length of {1}", HttpContext.Current.Request.UserHostAddress, context.Request.Headers["Content-Length"]);

				JumploaderWrapper.FileSystemFileSaver save = new JumploaderWrapper.FileSystemFileSaver(context, Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.StorageLocation);
				JumploaderWrapper.MultUploadHandler handle = new JumploaderWrapper.MultUploadHandler(save);
				handle.FileSaved += new JumploaderWrapper.MFileEventHandler(delegate(object sender, JumploaderWrapper.FileSavedEventArgs args) {
					log.DebugFormat("Client {0} successfully stored \"{3}\" ({1}kB) for token \"{2}\"", HttpContext.Current.Request.UserHostAddress, Math.Ceiling(int.Parse(context.Request.Headers["Content-Length"]) / 1024f), context.Request["token"], args.FileName);

					using (DataClassesDataContext db = new DataClassesDataContext()) {
						Package package = db.Packages.SingleOrDefault(p => p.Code == context.Request["token"]);
						if (package == null) {
							// create package if it doesn't already exists
							package = new Package() {
								Code = context.Request["token"],
								SenderMail = "Unspecified",
								Status = (char)PackageStatus.Open,
								Scanned = false,
								ExpiresAt = DateTime.Now.AddDays(14),
								CreatedAt = DateTime.UtcNow
							};
						}

						Commanigy.Transmit.Data.File file = package.Files.Where(f => f.FileHash == args.FileName).FirstOrDefault();
						if (file == null) {
							db.Files.InsertOnSubmit(new Commanigy.Transmit.Data.File() {
								CreatedAt = DateTime.UtcNow,
								FileHash = args.FileName,
								FileSize = int.Parse(context.Request.Headers["Content-Length"]),
								Package = package
							});
						}
						else {
							file.FileSize += int.Parse(context.Request.Headers["Content-Length"]);
						}

						db.SubmitChanges();
					}
				});
				handle.ProcessRequest(context);
			}
			catch (Exception x) {
				log.Error("Failed to store received file", x);
				throw x;
			}
		}

		public bool IsReusable {
			get { return true; }
		}
	}
}