#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;

using Commanigy.Transmit.Web;
using Commanigy.Transmit.Data;
using System.IO;
#endregion

namespace Commanigy.Transmit.SiteSupport {
	/// <summary>
	/// Summary description for Transmit
	/// </summary>
	[WebService(Namespace = "http://transmit.commanigy.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	// [System.Web.Script.Services.ScriptService]
	public class Transmit : System.Web.Services.WebService {
		[WebMethod()]
		public string[] Upload(List<string> files) {
			return new string[] {"fjong", "bong"};
		}

		[WebMethod()]
		public void Clean() {
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				// get all old packages
				var oldPackages = from p in db.Packages
								  where p.CreatedAt < DateTime.UtcNow.AddDays(-14)
								  select p;

				// delete all obsoleted packages
				oldPackages.ToList().ForEach(delegate(Package p) {
					if (Storage.DeletePath(p.Code)) {
						db.Packages.DeleteOnSubmit(p);
					}
				});

				db.SubmitChanges();
			}
		}

		//[WebMethod]
		//public void Request(AuthenticatedUser currentUser, string recipientEmail, string message) {
		//    // try to lookup user in AD based on recipient mail
		//    AuthenticatedUser recipientUser = Locator.FindByMail(recipientEmail);

		//    Invitation invitation = new Invitation {
		//        SenderMail = currentUser.Mail,
		//        SenderDisplayName = currentUser.DisplayName,
		//        RecipientMail = recipientEmail,
		//        RecipientDisplayName = (recipientUser != null) ? recipientUser.DisplayName : recipientEmail,
		//        Message = message,
		//        Code = Guid.NewGuid().ToString("N"),
		//        CreatedAt = DateTime.UtcNow
		//    };

		//    using (DataClassesDataContext db = new DataClassesDataContext()) {
		//        db.Invitations.InsertOnSubmit(invitation);
		//        db.SubmitChanges();
		//    }

		//    Dictionary<string, string> tokens = new Dictionary<string, string>();
		//    tokens.Add("Sender.Mail", currentUser.Mail);
		//    tokens.Add("Sender.DisplayName", currentUser.DisplayName);
		//    tokens.Add("Recipient.Mail", invitation.RecipientMail);
		//    tokens.Add("Recipient.DisplayName", invitation.RecipientDisplayName);
		//    tokens.Add("Mail.Message", invitation.Message);
		//    tokens.Add("Mail.InvitationCode", invitation.Code);
		//    tokens.Add("Url.Location", string.Format("{0}/upload.aspx?h={1}", UserHelper.GetSiteUrl(recipientUser), invitation.Code));

		//    string mailBody = System.IO.File.ReadAllText(Context.Server.MapPath("App_Data/mails/requesting-files.txt"));
		//    MailHelper.Send(ConfigurationManager.AppSettings["Commanigy.Transmit.Site.Request.MailSubject"], mailBody.Replace("\\n", "\n"), tokens);
		//}
	}
}