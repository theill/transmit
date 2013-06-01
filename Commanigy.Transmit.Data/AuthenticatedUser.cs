#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Principal;
using System.DirectoryServices;
#endregion

namespace Commanigy.Transmit.Data {
	[Serializable()]
	public class AuthenticatedUser {
		public enum PersonType {
			User,
			Contact,
			Group
		}

		public string DisplayName { get; set; }
		public string CommonName { get; set; }
		public string GivenName { get; set; }
		public string Mail { get; set; }
		public string Department { get; set; }
		public string AccountName { get; set; }
		public string Country { get; set; }
		public string Location { get; set; }
		public string Url { get; set; }
		public string Company { get; set; }
		public string Title { get; set; }
		public PersonType Type { get; set; }

		public AuthenticatedUser() {

		}

		public AuthenticatedUser(ResultPropertyCollection resultProperties) {
			foreach (string key in resultProperties.PropertyNames) {
				// key is (apparently) always lowercased

				foreach (object values in resultProperties[key]) {
					switch (key) {
						case "objectclass":
							if (values.ToString() == "user") {
								this.Type = PersonType.User;
							}
							else if (values.ToString() == "group") {
								this.Type = PersonType.Group;
							}
							else {
								this.Type = PersonType.Contact;
							}
							break;

						case "displayname":
							this.DisplayName = Sanitize(values.ToString());
							break;

						case "mail":
							this.Mail = Sanitize(values.ToString());
							break;

						case "cn": // commonname
							this.CommonName = Sanitize(values.ToString());
							break;

						case "department":
							this.Department = Sanitize(values.ToString());
							break;

						case "givenname":
							this.GivenName = Sanitize(values.ToString());
							break;

						case "samaccountname":
							this.AccountName = Sanitize(values.ToString());
							break;

						case "c": // country
							this.Country = Sanitize(values.ToString());
							break;

						case "l": // location
							this.Location = Sanitize(values.ToString());
							break;

						case "url": // url (could be used for profile pictures)
							this.Url = Sanitize(values.ToString());
							break;

						case "company":
							this.Company = Sanitize(values.ToString());
							break;

						case "title":
							this.Title = Sanitize(values.ToString());
							break;

						/*
					case "sn": // surname
						break;
					case "name":
						break;
					case "distinguishedname":
						break;
					case "member":
						break;
					case "initials":
						break;
					case "postalcode":
						break;
					case "l": // location
						break;
					case "c":
						break;
					case "mobile":
						break;
					case "homephone":
						break;
					case "title":
						break;
					case "co":
						break;
					case "st": // state
						break;
					case "password":
						break;
					case "memberof":
						break;
					case "uid":
						break;
					case "description":
						break;
						 */
					}
				}
			}
		}

		/// <summary>
		/// Sanitize value from LDAP by removing white spaces, filtering common 
		/// abbreviations, etc.
		/// </summary>
		/// <param name="ldapValue">Original value from LDAP</param>
		/// <returns>Sanitized value</returns>
		private string Sanitize(string ldapValue) {
			// never return empty values
			if (ldapValue == null) {
				return string.Empty;
			}

			// remove pre- and post-fixed whitespaces
			ldapValue = ldapValue.Trim();

			// a "n/a" value is converted to an empty string
			if (ldapValue.ToLower().Equals("n/a")) {
				return string.Empty;
			}

			return ldapValue;
		}

		private static string ReadProperty(SearchResult result, string propertyName) {
			if (result.Properties.Contains(propertyName)) {
				ResultPropertyValueCollection values = result.Properties[propertyName];
				if (values != null && values.Count > 0) {
					return values[0].ToString();
				}
			}

			return string.Empty;
		}

		public override string ToString() {
			return string.Format("{0} {1}", this.Mail, string.IsNullOrEmpty(this.CommonName) ? "" : "(" + this.CommonName + ")");
		}

		/*
		public static AuthenticatedUser Current {
			get {
				WindowsIdentity identity = WindowsIdentity.GetCurrent();

				// only assume users are properly authenticated when using Kerberos
//				if (identity.IsAuthenticated && identity.AuthenticationType == "Kerberos") {
				if (identity.IsAuthenticated) {
					string loginName = identity.Name.Substring(identity.Name.IndexOf('\\') + 1);

//					Locator.UserByLogin(loginName);

					DirectorySearcher mySearcher = new DirectorySearcher("sAMAccountName=" + loginName);
					try {
						SearchResult result = mySearcher.FindOne();

						AuthenticatedUser a = new AuthenticatedUser();
						a.CommonName = ReadProperty(result, "cn");
						a.DisplayName = ReadProperty(result, "displayName");
						a.GivenName = ReadProperty(result, "givenName");
						a.Mail = ReadProperty(result, "mail");
						a.Department = ReadProperty(result, "department");
						return a;
					}
					catch (Exception) {

					}
				}

				return null;
			}
		}
		*/
	}
}