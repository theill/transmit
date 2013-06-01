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
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using log4net;

using Commanigy.Transmit.Data;
using Commanigy.Transmit.Web;
using System.Threading;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class UploadPage : TransmitPage {
		private ILog log = log4net.LogManager.GetLogger("site");

		//private Invitation invitation;
		public Invitation Invitation { get; set;  }

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsInvitationValid(Request["h"])) {
				Response.Redirect("~/authenticate.aspx");
				Response.End();
				return;
			}

			if (!Page.IsPostBack) {
				LnkShare.Text = _("Done! Upload files");
				LblInvitationMessage.Text = this.Invitation.Message.Replace("\n", "<br />");

				HfToken.Value = Guid.NewGuid().ToString("N");

				TbxMessage.Text = TransmitSettings.Instance.Setting.UploadDefaultMessage;
			}
		}

		protected string FormatProfilePicture() {
			AuthenticatedUser inviter = Locator.FindByMail(this.Invitation.SenderMail);
			return (inviter != null) ? "Pictures.ashx?p=images/no-user-picture.png&d=64&u=" + Server.UrlEncode(inviter.Url) : "images/no-user-picture.png";
//			return "Pictures.ashx?p=images/no-user-picture.png&d=64&m=" + Server.UrlEncode(this.Invitation.SenderMail);
		}

		private bool IsInvitationValid(string invitationCode) {
			return !string.IsNullOrEmpty(invitationCode) && (this.Invitation = LoadInvitation(invitationCode)) != null;
		}

		private Invitation LoadInvitation(string invitationCode) {
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				return db.Invitations.SingleOrDefault(invitation => invitation.Code == invitationCode);
			}
		}

		protected void BtnShare_Click(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(Request["Files[]"])) {
				ValidationError.Display(_("No files uploaded"));
			}

			if (!Page.IsValid) {
				(this.Master as SiteMasterPage).Flash(_("You did not provide files to share"));
				return;
			}

			string[] fileHashes = Request["Files[]"].Split(',');

			using (DataClassesDataContext db = new DataClassesDataContext()) {
				Package p = db.Packages.SingleOrDefault(pkg => pkg.Code == HfToken.Value);
				if (p == null) {
					// FIXME: this "if" section should be removed once Flash code has been updated to files immediately as well
					p = new Package() {
						Code = HfToken.Value,
						SenderMail = this.Invitation.RecipientMail,
						Message = TbxMessage.Text,
						Status = (char)PackageStatus.Open,
						Scanned = false,
						ExpiresAt = DateTime.Now.AddDays(14),
						CreatedAt = DateTime.UtcNow
					};

					foreach (string fileHash in fileHashes) {
						Commanigy.Transmit.Data.File f = new Commanigy.Transmit.Data.File() {
							CreatedAt = DateTime.UtcNow,
							FileHash = fileHash,
							FileSize = Storage.GetFileSize(p.Code, fileHash),
							Package = p,
						};
						db.Files.InsertOnSubmit(f);
					}
				}
				else {
					p.SenderMail = this.Invitation.RecipientMail;
					p.Message = TbxMessage.Text;
				}

				db.SubmitChanges();
			}

			// get fresh copy of package
			Package package = Package.FindByCode(HfToken.Value);

			log.Debug("Packing files into ZIP archive for faster (initial) downloads by clients");
			new Thread(new ParameterizedThreadStart(delegate(object data) {
				Package uploadedPackage = data as Package;
				Storage.Pack(uploadedPackage.Code);
			})).Start(package);

			// generate unique token allowing recipient (requester) to download file
			string uniqueHash = package.Code;

			// look up user requesting files (this should always resolve to a user) unless that 
			// user has been disabled before file has been uploaded
			AuthenticatedUser recipientUser = Locator.FindByMail(this.Invitation.SenderMail);

			Dictionary<string, string> tokens = new Dictionary<string, string>();
			tokens.Add("CompanyName", TransmitSettings.Instance.Setting.CompanyName);
			tokens.Add("Sender.Mail", this.Invitation.RecipientMail);
			tokens.Add("Sender.DisplayName", this.Invitation.RecipientDisplayName);
			tokens.Add("Recipient.Mail", this.Invitation.SenderMail);
			tokens.Add("Recipient.DisplayName", (recipientUser != null) ? recipientUser.DisplayName : this.Invitation.SenderDisplayName);
			tokens.Add("Mail.Message", TbxMessage.Text ?? string.Empty);
			tokens.Add("Package.Code", package.Code);
			tokens.Add("Package.Files", PackageHelper.FilesToMailHtml(package.Files.ToList()));
			tokens.Add("File.Location", string.Format("{0}/receive.aspx?h={1}", UserHelper.GetSiteUrl(recipientUser), uniqueHash));

			MailHelper.Send(TransmitSettings.Instance.Setting.UploadMailSubject, TransmitSettings.Instance.Setting.UploadMailBodyPlain, TransmitSettings.Instance.Setting.UploadMailBodyHtml, tokens);

			this.Response.Redirect("~/upload.success.aspx", true);
		}
	}
}