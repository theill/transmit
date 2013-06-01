#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using Commanigy.Transmit.Data;
using System.Web;
using System.Configuration;
using log4net;
using System.Security.Principal;
using System.Runtime.InteropServices; 
#endregion

namespace Commanigy.Transmit.Web {
	/// <summary>
	/// 
	/// </summary>
	public class UserHelper {
		private static ILog log = LogManager.GetLogger("site");

		/// <summary>
		/// 
		/// </summary>
		public static AuthenticatedUser Current {
			get {
				//WindowsIdentity identity = WindowsIdentity.GetCurrent();
				//log.DebugFormat("WindowsIdentity");
				//log.DebugFormat(" .IsAnonymous => {0}", identity.IsAnonymous);
				//log.DebugFormat(" .IsGuest => {0}", identity.IsGuest);
				//log.DebugFormat(" .IsSystem => {0}", identity.IsSystem);
				//log.DebugFormat(" .IsAuthenticated => {0}", identity.IsAuthenticated);
				//log.DebugFormat(" .AuthenticationType => {0}", identity.AuthenticationType);
				//log.DebugFormat(" .Name => {0}", identity.Name);
				//log.DebugFormat(" .Token => {0}", identity.Token);
				//log.DebugFormat(" .User => {0}", identity.User);
				//log.DebugFormat("Current.User.Identity");
				//log.DebugFormat(" .AuthenticationType => {0}", HttpContext.Current.User.Identity.AuthenticationType);
				//log.DebugFormat(" .IsAuthenticated => {0}", HttpContext.Current.User.Identity.IsAuthenticated);
				//log.DebugFormat(" .Name => {0}", HttpContext.Current.User.Identity.Name);

				string loginName = HttpContext.Current.User.Identity.Name.Substring(HttpContext.Current.User.Identity.Name.IndexOf('\\') + 1);
				if (!string.IsNullOrEmpty(loginName)) {
					return Locator.FindByLogin(loginName);
				}

				return null;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static string GetSiteUrl(AuthenticatedUser user) {
			string url = (user != null && user.Type == AuthenticatedUser.PersonType.User) ? TransmitSettings.Instance.Setting.InternalUrl : TransmitSettings.Instance.Setting.ExternalUrl;
			
			// trim ending slash if available
			if (url.EndsWith("/")) {
				url = url.Substring(0, url.Length - 1);
			}

			return url;
		}

		public const int LOGON32_LOGON_INTERACTIVE = 2;
		public const int LOGON32_PROVIDER_DEFAULT = 0;

		WindowsImpersonationContext impersonationContext;

		[DllImport("advapi32.dll")]
		public static extern int LogonUserA(String lpszUserName,
			String lpszDomain,
			String lpszPassword,
			int dwLogonType,
			int dwLogonProvider,
			ref IntPtr phToken);
		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int DuplicateToken(IntPtr hToken,
			int impersonationLevel,
			ref IntPtr hNewToken);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool RevertToSelf();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool CloseHandle(IntPtr handle);

		public bool impersonateValidUser(String userName, String domain, String password) {
			WindowsIdentity tempWindowsIdentity;
			IntPtr token = IntPtr.Zero;
			IntPtr tokenDuplicate = IntPtr.Zero;

			if (RevertToSelf()) {
				if (LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE,
					LOGON32_PROVIDER_DEFAULT, ref token) != 0) {
					if (DuplicateToken(token, 2, ref tokenDuplicate) != 0) {
						tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
						impersonationContext = tempWindowsIdentity.Impersonate();
						if (impersonationContext != null) {
							CloseHandle(token);
							CloseHandle(tokenDuplicate);
							return true;
						}
					}
				}
			}
			if (token != IntPtr.Zero)
				CloseHandle(token);
			if (tokenDuplicate != IntPtr.Zero)
				CloseHandle(tokenDuplicate);
			return false;
		}

		public void undoImpersonation() {
			impersonationContext.Undo();
		}
	}
}