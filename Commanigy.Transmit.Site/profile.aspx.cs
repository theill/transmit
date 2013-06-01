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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using Commanigy.Transmit.Data;
using Commanigy.Transmit.Web;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// 
	/// </summary>
	public partial class ProfilePage : TransmitPage {
		public AuthenticatedUser ProfileUser {
			get {
				if (string.IsNullOrEmpty(Request["id"])) {
					return this.CurrentUser;
				}

				return Locator.FindByMail(Request["id"]);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Page_Load(object sender, EventArgs e) {
			this.Title = (this.ProfileUser != null) ? this.ProfileUser.DisplayName : string.Empty;

			if (!Page.IsPostBack) {
				if (this.ProfileUser == null) {
					throw new HttpException(404, "Profile not found");
				}

				if (this.ProfileUser == this.CurrentUser) {
					using (DataClassesDataContext db = new DataClassesDataContext()) {
						var shared = (from transfer in db.Transfers
									  where transfer.Package.SenderMail == this.CurrentUser.Mail
									  orderby transfer.CreatedAt descending
									  select transfer).Take(10);

						DlSharedByUser.DataSource = shared;
						DlSharedByUser.DataBind();

						var received = (from transfer in db.Transfers
										where transfer.RecipientMail == this.CurrentUser.Mail
										orderby transfer.CreatedAt descending
										select transfer).Take(10);

						//var requestedFromUser = from i in db.Invitations
						//                        where i.RecipientMail == this.CurrentUser.Mail
						//                        orderby i.CreatedAt descending
						//                        select i;

						DlRequestedFiles.DataSource = received;
						DlRequestedFiles.DataBind();

						var invited = (from invitation in db.Invitations
									   where invitation.SenderMail == this.CurrentUser.Mail
									   orderby invitation.CreatedAt descending
									   select invitation).Take(10);

						DlInvitedUsers.DataSource = invited;
						DlInvitedUsers.DataBind();
					}
				}
			}
		}

		protected string FormatProfilePicture() {
			return "Pictures.ashx?p=images/no-user-picture.png&d=96&m=" + Server.UrlEncode(this.ProfileUser.Mail);
		}


		protected string FormatFiles(object f) {
			System.Data.Linq.EntitySet<Commanigy.Transmit.Data.File> files = f as System.Data.Linq.EntitySet<Commanigy.Transmit.Data.File>;

			if (Convert.ToInt32(files.Count) == 1) {
				return files[0].FileHash;
			}

			return Commanigy.Transmit.Web.StringHelper.Pluralize(Convert.ToInt32(files.Count), _("file"), _("files"));
		}

		/// <summary>
		/// Query LDAP for a user and if found takes their givenname or, if not available
		/// the first part of their displayname.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		protected string TrimmedDisplayName(string email) {
			AuthenticatedUser a = Locator.FindByMail(email);
			if (a != null) {
				if (!string.IsNullOrEmpty(a.GivenName)) {
					return a.GivenName;
				}

				return a.DisplayName.Split(' ')[0];
			}

			return email;
		}
	}
}