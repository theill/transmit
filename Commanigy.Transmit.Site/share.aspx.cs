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
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using log4net;

using Commanigy.Transmit.Data;
using Commanigy.Transmit.Web;
using System.Globalization;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// Shares a file to one or more recipients.
	/// </summary>
	public partial class SharePage : TransmitPage {
		private ILog log = LogManager.GetLogger("site");

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e) {
			if (this.CurrentUser == null) {
				// TODO: consider using an auth filter
				Response.Redirect("~/authenticate.aspx");
				Response.End();
				return;
			}

			//log.DebugFormat("User {0} is requesting page '{1}' at {2}", this.CurrentUser, Page.Title, Page.Request.Path);

			if (!Page.IsPostBack) {
				LnkShare.Text = _("Share files");
				TbxMessage.Text = TransmitSettings.Instance.Setting.ShareDefaultMessage;
				HfToken.Value = Guid.NewGuid().ToString("N");
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnShare_Click(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(Request["Recipients[]"])) {
				ValidationError.Display(_("No recipients specified"));
			}

			if (string.IsNullOrEmpty(Request["Files[]"])) {
				ValidationError.Display(_("No files uploaded"));
			}

			if (!Page.IsValid) {
				ValidationSummary.HeaderText = _("Please correct the following:");
				return;
			}

			using (DataClassesDataContext db = new DataClassesDataContext()) {
				Package p = db.Packages.SingleOrDefault(pkg => pkg.Code == HfToken.Value);
				if (p == null) {
					// FIXME: this "if" section should be removed once Flash code has been updated to files immediately as well
					p = new Package() {
						Code = HfToken.Value,
						SenderMail = this.CurrentUser.Mail,
						Message = TbxMessage.Text,
						Status = (char)PackageStatus.Open,
						Scanned = false,
						ExpiresAt = DateTime.Now.AddDays(14),
						CreatedAt = DateTime.UtcNow
					};

					string[] fileHashes = Request["Files[]"].Split(',');

					foreach (string fileHash in fileHashes) {
						db.Files.InsertOnSubmit(new File() {
							CreatedAt = DateTime.UtcNow,
							FileHash = fileHash,
							FileSize = Storage.GetFileSize(p.Code, fileHash),
							Package = p,
						});
					}
				}
				else {
					p.SenderMail = this.CurrentUser.Mail;
					p.Message = TbxMessage.Text;
				}

				db.SubmitChanges();
			}

			// get fresh copy of package
			Package package = Package.FindByCode(HfToken.Value);

			string[] recipients = Request["Recipients[]"].Split(',');

			log.DebugFormat("Packing files into ZIP archive {0} for faster (initial) downloads by clients", package.Code);
			new Thread(new ParameterizedThreadStart(delegate(object data) {
				Package uploadedPackage = data as Package;
				Storage.Pack(uploadedPackage.Code);

				ScanPackage(uploadedPackage);

				NotifyRecipients(uploadedPackage, recipients);
			})).Start(package);

			log.DebugFormat("User {0} completed sharing", this.CurrentUser);
			this.Response.Redirect("~/share.success.aspx", true);
		}


		/// <summary>
		/// Notify all recipients about new, shared downloadable package.
		/// </summary>
		/// <param name="package">Recently uploaded package</param>
		/// <param name="recipients">An array of recipient emails</param>
		private void NotifyRecipients(Package package, string[] recipients) {
			log.DebugFormat("User {0} is dispatching share emails to {1} recipients", this.CurrentUser, recipients.Length);
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				foreach (string recipientEmail in recipients) {
					NotifyRecipient(package, this.CurrentUser.Mail, this.CurrentUser.DisplayName, recipientEmail, TbxMessage.Text);

					Transfer t = new Transfer() {
						CreatedAt = DateTime.UtcNow,
						PackageID = package.ID,
						RecipientMail = recipientEmail
					};
					db.Transfers.InsertOnSubmit(t);
				}
				db.SubmitChanges();
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fromAddress"></param>
		/// <param name="fromDisplayName"></param>
		/// <param name="toAddress"></param>
		private void NotifyRecipient(Package package, string fromAddress, string fromDisplayName, string toAddress, string message) {
			if (string.IsNullOrEmpty(message)) {
				message = string.Empty;
			}

			log.DebugFormat("Sending notification from {0} ({1}) to {2}", fromAddress, fromDisplayName, toAddress);

			// try to look up recipient in local store (LDAP server)
			AuthenticatedUser recipientUser = Locator.FindByMail(toAddress);

			Dictionary<string, string> tokens = new Dictionary<string, string>();
			tokens.Add("CompanyName", TransmitSettings.Instance.Setting.CompanyName);
			tokens.Add("Sender.Mail", fromAddress);
			tokens.Add("Sender.DisplayName", fromDisplayName);
			tokens.Add("Recipient.Mail", toAddress);
			tokens.Add("Recipient.DisplayName", (recipientUser != null) ? recipientUser.DisplayName : toAddress);
			tokens.Add("Package.Code", package.Code);
			tokens.Add("Package.Files", PackageHelper.FilesToMailHtml(package.Files.ToList()));
			tokens.Add("Package.Files.Count", package.Files.Count.ToString());
			tokens.Add("Mail.Message", message);
			tokens.Add("File.Location", string.Format("{0}/receive.aspx?h={1}", UserHelper.GetSiteUrl(recipientUser), package.Code));

			MailHelper.Send(TransmitSettings.Instance.Setting.ShareMailSubject, TransmitSettings.Instance.Setting.ShareMailBodyPlain, TransmitSettings.Instance.Setting.ShareMailBodyHtml, tokens);
		}

		private void ScanPackage(Package package) {
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				package.Scanned = PackageHelper.Scan(package);
				db.SubmitChanges();
			}
		}
	}
}