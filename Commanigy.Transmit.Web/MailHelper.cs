#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using log4net;
using System.Configuration;
using System.Web;
using System.Web.Hosting; 
#endregion

namespace Commanigy.Transmit.Web {
	public class MailHelper {
		private static ILog log = LogManager.GetLogger("web");

		public static void Send(string subject, string body, string bodyHtml, Dictionary<string, string> tokens) {
			if (!string.IsNullOrEmpty(body)) {
				body = body.Replace("\\n", "\n");
			}

			SmtpClient client = new SmtpClient();
			client.EnableSsl = Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.MailSecure;

			MailAddress from = null, replyTo = null, to = null;
			try {
				string fromAddress = tokens["Sender.Mail"];
				from = new MailAddress(fromAddress, tokens["Sender.DisplayName"]);
				to = new MailAddress(tokens["Recipient.Mail"].Replace(';', ','), tokens["Recipient.DisplayName"]);

				string defaultReplyTo = Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.MailReplyTo;
				replyTo = (!string.IsNullOrEmpty(defaultReplyTo)) ? new MailAddress(defaultReplyTo) : from;
			}
			catch (ArgumentException) {
				log.ErrorFormat("Failed to dispatch email from '{0}' to '{1}'", tokens["Sender.Mail"], tokens["Recipient.Mail"]);
				return;
			}
			catch (FormatException) {
				log.ErrorFormat("Failed to dispatch email from '{0}' to '{1}'", tokens["Sender.Mail"], tokens["Recipient.Mail"]);
				return;
			}

			MailMessage msg = new MailMessage(from, to);
			msg.ReplyTo = replyTo;
			msg.Subject = ReplaceTokens(subject, tokens);
			msg.BodyEncoding = Encoding.UTF8;

			if (string.IsNullOrEmpty(bodyHtml)) {
				msg.IsBodyHtml = false;
				msg.Body = ReplaceTokens(body, tokens);
			}
			else {
				AlternateView plainView = AlternateView.CreateAlternateViewFromString(ReplaceTokens(body, tokens), null, "text/plain");
				plainView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;
				msg.AlternateViews.Add(plainView);

				AlternateView htmlView = AlternateView.CreateAlternateViewFromString(ReplaceTokens(bodyHtml, tokens), null, "text/html");
				htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

				if (bodyHtml.Contains("cid:download")) {
					LinkedResource downloadButton = new LinkedResource(HostingEnvironment.MapPath("~/images/download-files.png"), "image/png");
					downloadButton.ContentId = "download";
					htmlView.LinkedResources.Add(downloadButton);
				}
				
				if (bodyHtml.Contains("cid:upload")) {
					LinkedResource btn = new LinkedResource(HostingEnvironment.MapPath("~/images/upload-files.png"), "image/png");
					btn.ContentId = "upload";
					htmlView.LinkedResources.Add(btn);
				}

				if (bodyHtml.Contains("cid:companylogo")) {
					string companyLogo;
					if (string.IsNullOrEmpty(Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.CompanyLogo)) {
						companyLogo = "~/images/transmit-logo.png";
					}
					else {
						companyLogo = Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.CompanyLogo;
					}

					LinkedResource btn = new LinkedResource(HostingEnvironment.MapPath(companyLogo), "image/png");
					btn.ContentId = "companylogo";
					htmlView.LinkedResources.Add(btn);
				}

				msg.AlternateViews.Add(htmlView);
			}

			try {
				client.Send(msg);
			}
			catch (Exception x) {
				log.Error(string.Format("Failed to dispatch email to {0}", to.Address), x);
			}
		}

		private static string ReplaceTokens(string v, Dictionary<string, string> tokens) {
			foreach (KeyValuePair<string, string> item in tokens) {
				v = v.Replace("[h " + item.Key + "]", HttpUtility.HtmlEncode(item.Value).Replace(System.Environment.NewLine, "<br />"));
				v = v.Replace("[" + item.Key + "]", item.Value);
			}
			return v;
		}
	}
}
