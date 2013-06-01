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
using System.Collections.Generic;
using System.Linq;

using log4net;

using Commanigy.Transmit.Data;
using Commanigy.Transmit.Web; 
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// Requests a file from either an internal or external user
	/// </summary>
	public partial class RequestPage : TransmitPage {
		private ILog log = LogManager.GetLogger("site");

		protected void Page_Load(object sender, EventArgs e) {
			if (this.CurrentUser == null) {
				// TODO: consider using an auth filter
				Response.Redirect("~/authenticate.aspx");
				Response.End();
				return;
			}

			//log.DebugFormat("User {0} is requesting page '{1}' at {2}", this.CurrentUser, Page.Title, Page.Request.Path);

			if (!Page.IsPostBack) {
				LnkRequest.Text = _("Request files");
				TbxMessage.Text = TransmitSettings.Instance.Setting.RequestDefaultMessage;
			}
		}

		/// <summary>
		/// Requests a file from one or more (internal- or external) users.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BtnRequest_Click(object sender, EventArgs e) {
			if (string.IsNullOrEmpty(Request["Recipients[]"])) {
				ValidationError.Display(_("No recipients specified"));
			}

			if (!Page.IsValid) {
				ValidationSummary.HeaderText = _("Please correct the following:");
				//(this.Master as SiteMasterPage).Flash("You did not provide a file to share");
				return;
			}

			string[] recipients = Request["Recipients[]"].ToLower().Split(',');
			log.DebugFormat("User {0} is dispatching request emails to {1} recipients", this.CurrentUser, recipients.Length);
			foreach (string recipientEmail in recipients) {
				// try to lookup user in AD based on recipient mail
				AuthenticatedUser recipientUser = Locator.FindByMail(recipientEmail);

				Invitation invitation = new Invitation {
					SenderMail = this.CurrentUser.Mail,
					SenderDisplayName = this.CurrentUser.DisplayName,
					RecipientMail = recipientEmail,
					RecipientDisplayName = (recipientUser != null) ? recipientUser.DisplayName : recipientEmail,
					Message = TbxMessage.Text,
					Code = Guid.NewGuid().ToString("N"),
					CreatedAt = DateTime.UtcNow
				};

				using (DataClassesDataContext db = new DataClassesDataContext()) {
					db.Invitations.InsertOnSubmit(invitation);
					db.SubmitChanges();
				}

				Dictionary<string, string> tokens = new Dictionary<string, string>();
				tokens.Add("CompanyName", TransmitSettings.Instance.Setting.CompanyName);
				tokens.Add("Sender.Mail", this.CurrentUser.Mail);
				tokens.Add("Sender.DisplayName", this.CurrentUser.DisplayName);
				tokens.Add("Recipient.Mail", invitation.RecipientMail);
				tokens.Add("Recipient.DisplayName", invitation.RecipientDisplayName);
				tokens.Add("Mail.Message", invitation.Message);
				tokens.Add("Mail.InvitationCode", invitation.Code);
				tokens.Add("Url.Location", string.Format("{0}/upload.aspx?h={1}", UserHelper.GetSiteUrl(recipientUser), invitation.Code));

				MailHelper.Send(TransmitSettings.Instance.Setting.RequestMailSubject, TransmitSettings.Instance.Setting.RequestMailBodyPlain, TransmitSettings.Instance.Setting.RequestMailBodyHtml, tokens);
			}

			this.Response.Redirect("~/request.success.aspx", true);
		}
	}
}