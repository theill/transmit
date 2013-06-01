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
#endregion

namespace Commanigy.Transmit.SiteSupport {
	/// <summary>
	/// Handles a single upload at a time.
	/// </summary>
	public class Upload : IHttpHandler {
		private ILog log = LogManager.GetLogger("support");

		public void ProcessRequest(HttpContext context) {
			try {
				string token = context.Request["token"];
				if (string.IsNullOrEmpty(token)) {
					log.Warn("Upload request missing token - file cannot be stored");
					return;
				}

				log.DebugFormat("Start processing request for token \"{2}\" from {0} with a content-length of {1}", HttpContext.Current.Request.UserHostAddress, context.Request.Headers["Content-Length"], token);

				// this is a blocking call since it will wait for entire upload to complete
				HttpPostedFile uploadedFile = context.Request.Files["payload"];

				if (uploadedFile == null) {
					log.DebugFormat("Client {0} did not finish file", HttpContext.Current.Request.UserHostAddress);
					foreach (var key in context.Request.Headers.AllKeys) {
						log.DebugFormat("Got {0} => {1}", key, context.Request.Headers[key]);
					}

					if (!context.Response.IsClientConnected) {
						log.InfoFormat("Client {0} on {3} disconnected prematurely after uploading {1} out of {2} bytes", HttpContext.Current.Request.UserHostAddress, context.Request.TotalBytes, context.Request.Headers["Content-Length"], HttpContext.Current.Request.UserAgent);
					}
					return;
				}
				log.DebugFormat("Client {0} sent {1} bytes as file {2}", HttpContext.Current.Request.UserHostAddress, uploadedFile.ContentLength, uploadedFile.FileName);

				string uniqueFileName = Storage.SaveFile(token, uploadedFile);
				log.DebugFormat("Client {0} successfully stored \"{1}\" with filename \"{2}\"", HttpContext.Current.Request.UserHostAddress, uploadedFile.FileName, uniqueFileName);
				
				context.Response.StatusCode = 200;
				context.Response.Write(uniqueFileName);
				context.Response.Flush();
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
