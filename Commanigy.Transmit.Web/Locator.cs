#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Commanigy.Transmit.Data;
using log4net;
using System.Configuration;
using System.DirectoryServices;
using System.Web.Caching;
using System.Linq;
#endregion

namespace Commanigy.Transmit.Web {
	/// <summary>
	/// 
	/// </summary>
	public class Locator {
		private static ILog log = LogManager.GetLogger("site");

		/// <summary>
		/// Returns all users matching display name. Names are split into groups and each group are searched to 
		/// avoid finding users with characters inside search e.g. searching for "Asper" should return users with 
		/// a name, middle or last name "Asper" but should not return "Casper".
		/// 
		/// This is similar to the way Facebook looks up contacts.
		/// </summary>
		/// <param name="displayName"></param>
		/// <returns></returns>
		public static List<AuthenticatedUser> FindByDisplayName(string displayName) {
			string displayNameLowered = displayName.ToLower();

			string filter = string.Format(TransmitSettings.Instance.Setting.LdapFilterName, StripForLdapQuery(displayName));
			return FilterActiveDirectoryUsers(filter).FindAll(delegate(AuthenticatedUser a) {
				// initially do a full search
				string fullName = (a.DisplayName ?? a.CommonName).ToLower();
				if (fullName.StartsWith(displayNameLowered)) {
					return true;
				}

				// split name parts and do partial word search
				string[] names = fullName.Split(' ');
				foreach (string name in names) {
					if (name.StartsWith(displayNameLowered)) {
						return true;
					}
				}

				return false;
			});
		}

		public static AuthenticatedUser FindByMail(string mail) {
			string cacheKey = string.Format("AuthenticatedUser.FindByMail({0})", mail);

			if (HttpContext.Current != null && HttpContext.Current.Cache[cacheKey] != null) {
				return HttpContext.Current.Cache[cacheKey] as AuthenticatedUser;
			}

			string filter = string.Format(TransmitSettings.Instance.Setting.LdapFilterMail, StripForLdapQuery(mail));

			List<AuthenticatedUser> users = FilterActiveDirectoryUsers(filter);
			if (users.Count > 0) {
				AuthenticatedUser user = users.Find(m => m.Mail == mail) ?? users[0];
				log.DebugFormat("Looked up {1} users by mail \"{0}\" and returning {2}", mail, users.Count, user.DisplayName);

				if (HttpContext.Current != null) {
					HttpContext.Current.Cache[cacheKey] = user;
				}

				return user as AuthenticatedUser;
			}
			else {
				log.DebugFormat("No user by mail \"{0}\"", mail);
			}

			return null;
		}


		/// <summary>
		/// Queries AD for a single user by users login name (sAMAccountName).
		/// </summary>
		/// <param name="login">Name to lookup e.g. "admintpt"</param>
		/// <returns>User if found; null otherwise</returns>
		public static AuthenticatedUser FindByLogin(string login) {
			string cacheKey = string.Format("AuthenticatedUser.FindByLogin({0})", login);

			AuthenticatedUser user = (HttpContext.Current != null ? HttpContext.Current.Cache[cacheKey] : null) as AuthenticatedUser;
			if (user != null) {
				return user;
			}

			string filter = string.Format(TransmitSettings.Instance.Setting.LdapFilterLogin, StripForLdapQuery(login));

			List<AuthenticatedUser> users = FilterActiveDirectoryUsers(filter);
			if (users.Count > 0) {
				user = users.Find(m => m.AccountName == login) ?? users[0];
				log.DebugFormat("Looked up {1} users by login \"{0}\" and returning \"{2}\"", login, users.Count, user.DisplayName);

				if (HttpContext.Current != null) {
					HttpContext.Current.Cache[cacheKey] = user;
				}

				return user as AuthenticatedUser;
			}
			else {
				log.DebugFormat("No user by login \"{0}\"", login);
			}

			return null;
		}

		private static string StripForLdapQuery(string query) {
			/*
Almost any characters can be used in Distinguished Names. However, some must be escaped with the backslash "\" escape character. Active Directory requires that the following characters be escaped:

The comma	,
The backslash character	\
The pound sign character	#
The plus sign	+
The less than symbol	<
The greater than symbol	>
The semicolon	;
The double quote character	"
The equal sign	=
Leading or trailing spaces	 
The space character must be escaped only if it is the leading or trailing character in a component name, such as a Common Name. Embedded spaces should not be escaped.
			 */

			StringBuilder buffer = new StringBuilder(query);
			buffer.Replace(",", "\\,");
			buffer.Replace("\\", "\\\\");
			buffer.Replace("#", "\\#");
			buffer.Replace("+", "\\+");
			buffer.Replace("<", "\\<");
			buffer.Replace(">", "\\>");
			buffer.Replace(";", "\\;");
			buffer.Replace("\"", "\\\"");
			buffer.Replace("=", "\\=");
			return buffer.ToString().Trim();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private static List<AuthenticatedUser> FilterActiveDirectoryUsers(string filter) {
			int sizeLimit = TransmitSettings.Instance.Setting.LdapSizeLimit;

			/*
			 * (&
			 *  (|
			 *   (&
			 *	  (|
			 *	   (givenName=kristoffer*)
			 *	   (sn=kristoffer*)
			 *	   (displayName=kristoffer*)
			 *	   (cn=kristoffer*)
			 *	   )
			 *	  (|
			 *	   (objectClass=user)
			 *	   (objectClass=contact)
			 *	   )
			 *	  )
			 *	 (&
			 *	  (|
			 *	   (givenName=kristoffer*)
			 *	   (sn=kristoffer*)
			 *	   (displayName=kristoffer*)
			 *	   (cn=kristoffer*)
			 *	   )
			 *	  (mail=*)
			 *	  (objectClass=group)
			 *	  )
			 *	 )
			 *	(!(userAccountControl=514))
			 *	(mail=*)
			 *	(!(lockoutTime>=1))
			 *	)
			 *	
			 * (&(objectClass=person)(!(objectClass=computer))(|(givenName={0}*)(sn={0}*)(displayName={0}*)(cn={0}*))(|(objectClass=user)(objectClass=contact))(!(userAccountControl=514))(mail=*)(!(lockoutTime>=1)))
			 * 
			 * (&(|(&(|(givenName={0}*)(sn={0}*)(displayName={0}*)(cn={0}*))(|(objectClass=user)(objectClass=contact)))(&(|(givenName={0}*)(sn={0}*)(displayName={0}*)(cn={0}*))(mail=*)(objectClass=group)))(!(userAccountControl=514))(mail=*)(!(lockoutTime>=1)))
			 */
			DirectorySearcher mySearcher = new DirectorySearcher(filter, new string[] { "displayName", "mail", "cn", "sn", "department", "givenName", "sAMAccountName", "c", "l", "url", "company", "title", "objectClass" });
			mySearcher.SizeLimit = sizeLimit;

			// trick to get 'size limit' and not falling back to server defaults
			// see http://richarddingwall.name/2008/04/29/the-finer-points-of-net-directoryservices/
			mySearcher.PageSize = sizeLimit / 2;

			List<AuthenticatedUser> users;

			if (ConfigurationManager.AppSettings["Commanigy.Transmit.Site.Debug"] == "true") {
				//users = LoadDebugUsers();
				users = new List<AuthenticatedUser>();

				var staticUsers = from u in new DataClassesDataContext().Users
								  select u;

				foreach (User u in staticUsers) {
					users.Add(new AuthenticatedUser() {
						AccountName = u.AccountName,
						CommonName = u.CommonName,
						Company = u.Company,
						Country = u.Country,
						Department = u.Department,
						DisplayName = u.DisplayName,
						GivenName = u.GivenName,
						Location = u.Location,
						Mail = u.Mail,
						Title = u.Title,
						Type = (AuthenticatedUser.PersonType)Enum.Parse(typeof(AuthenticatedUser.PersonType), u.Type),
						Url = u.Url
					});
				}

				//if (filter.ToLower().Contains("(samaccountname=demo)") || filter.ToLower().Contains("(mail=john@doe.com)")) {
				//    users.RemoveAll(delegate(AuthenticatedUser u) { return u.DisplayName != "John Doe"; });
				//}
				//else if (filter.ToLower().Contains("theill")) {
				//    users.RemoveAll(delegate(AuthenticatedUser u) { return u.DisplayName != "Peter Theill"; });
				//}
				//else if (filter.ToLower().Contains("peter@commanigy.com")) {
				//    users.RemoveAll(delegate(AuthenticatedUser u) { return u.Mail != "peter@commanigy.com"; });
				//}
			}
			else {
				users = new List<AuthenticatedUser>();
				try {
					SearchResultCollection results = mySearcher.FindAll();

					log.DebugFormat("Loaded {0} users from Active Directory using filter {1}", results.Count, filter);
					foreach (SearchResult result in results) {
						users.Add(new AuthenticatedUser(result.Properties));
					}
				}
				catch (System.Runtime.InteropServices.COMException comx) {
					if (comx.Message.Contains("The specified domain either does not exist or could not be contacted.")) {
						log.ErrorFormat("Unable to query Active Directory. Error: {0}", comx.Message.Trim());
					}
					else {
						log.Error("Unable to query users from Active Directory. Please consult your manual for details. Technical error: ", comx);
					}
				}
				catch (Exception x) {
					log.Debug("Failed to load users", x);
				}
			}

			// sort by display name
			users.Sort(new Comparison<AuthenticatedUser>(delegate(AuthenticatedUser a1, AuthenticatedUser a2) { return (a1.DisplayName ?? string.Empty).CompareTo((a2.DisplayName ?? string.Empty)); }));

			return users;
		}

		private static List<AuthenticatedUser> LoadDebugUsers() {
			List<AuthenticatedUser> users = new List<AuthenticatedUser>();
			#region inserting debugging users
			users.Add(new AuthenticatedUser() { DisplayName = "John Doe", Mail = "john@doe.com", Department = "Marketing", Title = "Founder", Company = "Doe Shoes Inc.", Location = "Copenhagen", Country = "Denmark", Url = "images/users/eduardop.jpg", AccountName = "demo", Type = AuthenticatedUser.PersonType.User });
			users.Add(new AuthenticatedUser() { DisplayName = "Peter Theill", Mail = "peter@commanigy.com", Department = "Software Development", Title = "Software Developer", Company = "Commanigy", Location = "Copenhagen", Country = "Denmark", Url = "images/users/peter-theill.jpg", AccountName = "theill", Type = AuthenticatedUser.PersonType.User });
			users.Add(new AuthenticatedUser() { DisplayName = "Bjørn E. Andersen", Mail = "bjoern@commanigy.com", Department = "Marketing", Title = "Marketing Manager", Company = "Commanigy", Location = "Copenhagen", Country = "Denmark", Url = "images/users/morten-sanderhus.jpg", AccountName = "bjoern", Type = AuthenticatedUser.PersonType.User });
			users.Add(new AuthenticatedUser() { DisplayName = "Kristoffer Jepsen", Mail = "kris.nrj@gmail.local", Department = "HQ ICT Services", Title = "IT Architect", Company = "UNOPS", Location = "Copenhagen", Country = "Denmark", Url = "images/users/kristoffer-jepsen.jpg", Type = AuthenticatedUser.PersonType.User });
			users.Add(new AuthenticatedUser() { DisplayName = "Casper Fabricius", Mail = "me@casperfabricius.local", Department = "Development", Url = "images/users/eduardop.jpg", Type = AuthenticatedUser.PersonType.User });
			users.Add(new AuthenticatedUser() { DisplayName = "Sidse Skipper Holmberg Nielsen", Mail = "sidse@theill.local", Url = "images/users/sidse-nielsen.jpg", Type = AuthenticatedUser.PersonType.User });
			users.Add(new AuthenticatedUser() { DisplayName = "Lars Jungsberg", Mail = "lars@jungsberg.local", Url = "http://localhost/Transmit.Internal/images/users/lars-jungsberg.jpg", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "David Abigt", Mail = "dabigt@austin.rr.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Charles Abraham", Mail = "charly@jam.rr.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Jerold Abrams", Mail = "jabrams@pacbell.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Len Abrams", Mail = "len.abrams@waterpolicy.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Andrew Ackerman", Mail = "ackerman76@hotmail.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "William Addison", Mail = "ad@fcs.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Moshe Har Adir", Mail = "attia_moshe@hotmail.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Angra Aeroportos", Mail = "fagner@imagemhost.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Karsten Agler", Mail = "scotis_man@yahoo.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "Rachel Ahlum", Mail = "rachel.ahlum@gmail.local", Type = AuthenticatedUser.PersonType.Contact });
			users.Add(new AuthenticatedUser() { DisplayName = "René Ahuir", Mail = "rene.ahuir@free.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Commanigy Users", Mail = "all@commanigy.com", Type = AuthenticatedUser.PersonType.Group });
			users.Add(new AuthenticatedUser() { DisplayName = "Ron Akanowicz", Mail = "rakanowicz@attbi.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kim Andre Akeroe", Mail = "kimandre@kaa.no" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ahmed Al Nasser", Mail = "ahmad-876@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "James Alabiso", Mail = "jim@starlogic.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Circuito Albacete", Mail = "webmaster@civab.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jonathan Albers", Mail = "nospam@pandaguy.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brent Alberts", Mail = "brent@brentalberts.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Philipp Albig", Mail = "philipp@albigs.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "William Albright", Mail = "dbright@abcpro.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Michel Albuisson", Mail = "michel@albuisson.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Mario Alcoser", Mail = "alcoser@granderiver.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Robert Aleck", Mail = "robert.aleck@cynexia.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kevin Alexander", Mail = "kevalex@comcast.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Nathan Allan", Mail = "nathan@netbloke.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "donald allen", Mail = "hrybldguy@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Erik Allen", Mail = "erik.allen@mchsi.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ethan Allen", Mail = "ethanwa@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Peter Allison", Mail = "peter.allison@ic24.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Dominic Allkins", Mail = "dominic@allkins.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Dave Alm", Mail = "dave@dcwi.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Olavi Alm", Mail = "olavi.alm@surffi.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "AlmondJoy AlmondJoy", Mail = "almondjjoy@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "aloneone.local aloneone.local", Mail = "tomasino@aloneone.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Dominic Alston", Mail = "nik@nikalston.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Alberto Alvarez", Mail = "pulpi69@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Reuel Alvarez", Mail = "block1of4@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Gloria Amer", Mail = "ajinora@rogers.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Peter Ammann", Mail = "pammann@optobyte.ch" });
			users.Add(new AuthenticatedUser() { DisplayName = "Paul Amonson", Mail = "paul@amonson.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Cristina Amor", Mail = "princesa.7@gmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Allan Hoby Andersen", Mail = "allanhoby50@abmerkur.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jesper Andersen", Mail = "jesper.andersen@greyscale.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kurt Moskjær Andersen", Mail = "info@moskjaer.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Poul Andersen", Mail = "poul@hvid-schaefer.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "tony anderson", Mail = "tonyingvl@bellsouth.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Albert Andrascik", Mail = "albertandrascik@optonline.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Lars Andreasson", Mail = "lars@andreasson.pp.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Alexei Andreev", Mail = "postmaster@netfort.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brendan Andrews", Mail = "charybdis@pandemicstudios.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brendan Andrews", Mail = "charybdis@pandemicstudios.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Harold Andrews", Mail = "harold@haroldandrews.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Harold Andrews", Mail = "me@haroldandrews.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Marc Andrews", Mail = "marcsabinman@blueyonder.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "B Andries", Mail = "bevan@ecr.co.za" });
			users.Add(new AuthenticatedUser() { DisplayName = "Charles Angel", Mail = "charles@charlesangel.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joseph Angerson", Mail = "sales@digisquid.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "blue anthem", Mail = "blueanthem@yahoo.local.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brian Antoine", Mail = "briana@circuit.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Font Antoni", Mail = "toni@rosesnet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Fred Antoon", Mail = "shep44@adelphia.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Bob Anundson", Mail = "bob@anundson.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joe Anzalone", Mail = "Joe@captinshmit.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kristian Apodaca", Mail = "payments@klaserv.local", Location = "Copenhagen" });
			users.Add(new AuthenticatedUser() { DisplayName = "Derek Archer", Mail = "derekarcher@myrealbox.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "James Arlet", Mail = "jarlet@telra.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "David Armitage", Mail = "dha@btinternet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Scott Armitage", Mail = "sarmitage@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Philip Armour", Mail = "philiparmour@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ralph Arnett", Mail = "ralph@ticnet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "howard h arnold", Mail = "harnold@ec.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Howard H. Arnold", Mail = "harnold@triad.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jonathan Arnold", Mail = "owners@ldscafe.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "james arseneault", Mail = "pet.draveurs@lino.sympatico" });
			users.Add(new AuthenticatedUser() { DisplayName = "larry arthurs", Mail = "myoldestore@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Daniel Artusi", Mail = "lartusi@austin.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Philip Aschauer", Mail = "aschauer@robertsteiner" });
			users.Add(new AuthenticatedUser() { DisplayName = "Lynn Ashley", Mail = "la-orders@ashleys.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Carly Ashton", Mail = "deathofseason@gmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Justin Ashworth", Mail = "justinashworth@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "H N Asper", Mail = "hendrik@asper.maruanaja" });
			users.Add(new AuthenticatedUser() { DisplayName = "Guy Asselin", Mail = "techno.qc@videotron" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jean Astie", Mail = "jean@gabys.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Arriel Atienza", Mail = "aatienza@westernu.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Atmosphere Design", Mail = "james@atmosphered.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Shay Auctions", Mail = "coreyshay001@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kam Aujla", Mail = "kam.aujla@vancouver.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Victor Auld", Mail = "auld@auldfamily.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Dennis J Austin", Mail = "djaustin@sbcglobal.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Matthew Austin", Mail = "mattraustin@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Marcello Avolio", Mail = "mangoes_27@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ron R Axelson", Mail = "rraxelson@verizon.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Daniel Ayars", Mail = "dayars@nbbj.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "SARVE AZITA", Mail = "sazita@numericable.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Domenico Azzolina", Mail = "d.azzolina@tin.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Stephen Babb Jr", Mail = "salent@air4u.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brenda Babin", Mail = "bbabin@lumcon.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kenneth Babington", Mail = "kenneth.babington@broadpark.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Mark Backhouse", Mail = "mark.backhouse@ntlworld.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Maren Backstrøm", Mail = "maren@inngangen.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "James Bacon", Mail = "jwbacon@comcast.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "John Bagley", Mail = "john@jcbagley.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ward Bahner", Mail = "wbahner@livejournal.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Clifford Bahr", Mail = "poppops_netandgames@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Bryan Charles Bailey", Mail = "kd5yov@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Christopher Bailey", Mail = "cbailey@palmerpaint.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Dan Bailey", Mail = "dan@iowapcservices.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Daniel Bailey", Mail = "baileygoat@gmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jarrett Bailey", Mail = "jarrettabailey@sbcglobal.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Richard Bailey", Mail = "r_a_bailey@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Pierre Baillargeon", Mail = "nova1@mediaone.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Michael Bairos", Mail = "mbairos@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Patrick Baize", Mail = "pbaize@satx.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Will Bakali", Mail = "will.bakali@willster.localeeserve.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Adam Baker", Mail = "acbaker@ucdavis.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Justin Baker", Mail = "justin.baker@csiro.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kelly Baker", Mail = "girl@subsomatic.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kelly Baker", Mail = "paypal@subsomatic.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Matthew Baker", Mail = "matthew_a_baker@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Tom Baker", Mail = "bakersix@hotpop.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Raymond Bakken", Mail = "quidnano@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "lance balch", Mail = "lance@kwic.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Alan balding", Mail = "alan@flower-studio.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Trevor Baldwin", Mail = "trevor@glenlee.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Danielle Balsamo", Mail = "dbalsamo@gmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kimberley Banoczi", Mail = "kym@dancysoft.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Aaron Baranoski", Mail = "aaron@tananasports.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Bruce Baranski", Mail = "bruce@brucebaranski.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Vance Barbee", Mail = "vance@rhythmexpress.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ian Barber", Mail = "gonzo@raegunne.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Nathalie BARBILLAT", Mail = "nathalie.barbillat@devoteam.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "BargainGoodies.local BargainGoodies.local", Mail = "bargaingoodies@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Sid Barkel", Mail = "Sid.Barkel@BTConnect.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Leon Barker", Mail = "support@itkaos.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Sharon Barley", Mail = "needz@softhome.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Chad Barnes", Mail = "hiawathaboy@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Paul Barney", Mail = "peb39321@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jennifer Baron", Mail = "webmaster@girl138.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Barons Self Storage Barons Self Storage", Mail = "info@baronsselfstorage.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Rob Barr", Mail = "kuririn@ninja-monkey.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Emmanuel Barral-Godet", Mail = "koxmail@wanadoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "jason barrett", Mail = "xero@xerosix.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "DeeAnn Barricklow", Mail = "predictable806@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Paul Bartholomew", Mail = "paulbart@comcast.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Rhys Bartle", Mail = "Rhys@opotiki.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Gary Bartolacci", Mail = "garybarto@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jennifer Bartolome", Mail = "thecheapshack@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Christopher Bartow", Mail = "chris@thewangshow.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Robert Bascom", Mail = "rbascom@ultrasw.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Aldo Basili", Mail = "aldo.basili@gmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Greg Bassett", Mail = "greg@aknobforbrightness.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Josh Bassinger", Mail = "carolinaboy02@earthlink.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Greg Basso", Mail = "hotinto25@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Mark Batchelour", Mail = "mark@batchelour.localeeserve.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Robbie Bates", Mail = "robbieb@rainbow.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Erica Batte", Mail = "ebatte@gmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Francesco Battilani", Mail = "tattoos@tin.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Patricia Bau", Mail = "zorbs@leafqueen.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "German Bauer", Mail = "design4use@acm.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joe Bauer", Mail = "joebauer@pacbell.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Thomas Baye", Mail = "thomas@baye.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jacob Bayless", Mail = "JBayless@ci.santa-rosa.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kevin Beaird", Mail = "wonker@wonker.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jonathan Beamer", Mail = "jlbeamer@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "David Bear", Mail = "kb1jfz@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brian Beard", Mail = "jasontrauer@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Lyn Beardsall", Mail = "katie.cullinan@classicpictures.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Marvin Beatty", Mail = "beatty@shaw.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Robert Beatty", Mail = "rebeatty@shaw.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Bruce Beauchamp", Mail = "ebayrelated@comcast.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Stuart Beavers", Mail = "sholland@safestore.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Richard Beavis", Mail = "paypal@beavo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Glenn Bechtold", Mail = "gbechtold@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Carl Beck", Mail = "carl.localck@insightbb.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Henrik Beck", Mail = "webmaster@henrikbeck.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Mark Beckemeyer", Mail = "bmeyers2@ktis.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Rohan Beckett", Mail = "rohan@beckett.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Aimee Beckwith", Mail = "aimee@scoopa.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Paul Beech", Mail = "i@guru.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Paul Beech", Mail = "guru@boreworms.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Goran Begic", Mail = "gbegic@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Rene Beier", Mail = "beier@sporty.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Marco Beijen", Mail = "marco@beijen.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Behnam Beikzadeh", Mail = "B_Beikzadeh@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brian Beitzel", Mail = "brian@beitzel.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "David Belcher", Mail = "davidbelcher@bellsouth.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Raymond Belford III", Mail = "butchie_b45@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Gordon Bell", Mail = "resort@beauview.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Nancy Bell", Mail = "info@beauviewresort.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Phillip Bellamy", Mail = "phil@psbellamy.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Michael Beller", Mail = "mbeller@mills.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "David Bell-Feins", Mail = "belfazar@bellatlantic.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ferdinando Belliere", Mail = "calderinometeo@libero.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Anthony Bellissimo", Mail = "twoncam@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joshua Bello", Mail = "josh@osd.bsdi.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "john belofi", Mail = "jbelofi@usd.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Vic Beltz", Mail = "v5i7cit6@wedgewoodinn.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Nicole Benard", Mail = "info@michaud.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jorge Bencomo", Mail = "Blyth26@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Tine Bengtsson", Mail = "info@aboutbalance.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Grant Bennett", Mail = "woode@drivingassist.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Michael Bennett", Mail = "vampyre99@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Neil Bennett", Mail = "neilb@kvewtv.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Paul Bennett", Mail = "paul.localnnett@autobag.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "gunter bens", Mail = "guenter@iol.ie" });
			users.Add(new AuthenticatedUser() { DisplayName = "Christoph Bensegger", Mail = "christoph@bensegger.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ernest Benson", Mail = "erniebenson@xplornet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Michael Benz", Mail = "mvbenz@mvbenz.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Thomas Benzie", Mail = "tbenzie@gndflor.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Peter van den Berg", Mail = "peter@vd-berg.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Sarah Berg", Mail = "sarah@asphericash.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Tone Kristine Berg", Mail = "espen@aaserod.local", Department = "Software Development" });
			users.Add(new AuthenticatedUser() { DisplayName = "Livar Bergheim", Mail = "livarbe@online.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brian Bergin", Mail = "conquercam@terabyte.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "James L. Bergin", Mail = "jimbergin@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Thor-Björn Bergman", Mail = "info@peak24.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Børge Berg-Olsen", Mail = "azoth@dod.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Mattias Bergsten", Mail = "fnord@fnord.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Fredrik Bergström", Mail = "fred@ko2000.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Raymond Berlanger", Mail = "raymond@deklimop.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Stephane Bernard", Mail = "sbernard@ismeca.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Heiko Bernhoerster", Mail = "Heiko@Bernhoerster.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jose Bernier", Mail = "scohen@doce.ufl.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Norbert G. Bernigau", Mail = "home@bernigau.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Scott Berres", Mail = "berresboy@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Steve Berry", Mail = "steve@berry.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Walker Berry", Mail = "themadweaz@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ralf Bethke", Mail = "bethkeralf@web.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Wolfgang Betz", Mail = "dr.-ing.w.localtz@t-online.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Eric Beumer", Mail = "webmaster@leeghwaterlaan.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Marc Bevaart", Mail = "marc.localvaart@cirrus.localmon.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Heinz Beyer", Mail = "hb@bti-net.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joanne Bianco", Mail = "webmaster@ravenrain.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Richard Bibby", Mail = "bibby_flygel117@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Scott Bideau", Mail = "kusellout69@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Larry Bignami Jr.", Mail = "RxFlow@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "F.C. Bij", Mail = "conquercam@superfly.localmon.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Marko Bimek", Mail = "ichmarko@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Alberto Biraghi", Mail = "alberto@biraghi.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Toni Birrer", Mail = "toni@dexter.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Toni Birrer", Mail = "toni@dexter.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jason Bishop", Mail = "jasonbishop@gsinet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Lougan Bishop", Mail = "lougan@comcast.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Lougan Bishop", Mail = "leb2e@mtsu.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Marco Bisi", Mail = "marco@binergy.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joanne Bisso", Mail = "ambiome@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jan Bister", Mail = "jbister@neo.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Alan Bixby", Mail = "abixby@whidbey.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Gail Bjork", Mail = "gail@gtbdesign.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Magnus Björk", Mail = "magnus.bjork@mbox306.swipnet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "James Blades", Mail = "jim.blades@virgin.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Martin Blaha", Mail = "mb1974@gmx.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "James Blair", Mail = "lukeblair@msn.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Steve Blake", Mail = "timh@rushmore.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Terri Blakely", Mail = "luzah@cyberius.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "William Blakely", Mail = "wblakely@pacbell.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Steve Blakemore", Mail = "steveblakemore@openlink.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Douglas Blanton", Mail = "Doug.Blanton@gmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Martin Blaszczyk", Mail = "martin@photo-gallery.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Carl Blatchley", Mail = "carl@blatchley.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Keimpe Bleeker", Mail = "bleekerk@xs4all.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Charles Blevins", Mail = "aidstasticwow@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Sarah Bleviss", Mail = "hyper@wondergirl.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Poulus Bliek", Mail = "poulus@webgoed.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Richard Blisard", Mail = "rblisard@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Heather Bliss", Mail = "playforregret@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Stephen Bliss", Mail = "bliss1940-paypal@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joey Bloomfield", Mail = "paypal@marinecreek.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Robert Blue", Mail = "orders@robblue.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Peter Blumenstein", Mail = "erwinvonwien@earthlink.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Thomas Blättner", Mail = "thbl@onlinehome.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Paul Bocca", Mail = "boyi@orange.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Don Boccio", Mail = "djboccio@optonline.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Christoph Bock", Mail = "Christoph.Bock@ePost.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Olav Bodelier", Mail = "obodelier@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Stephan Bogaert", Mail = "stephan.bogaert@advalvas.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "William Bohn", Mail = "ntense@tupelomfg.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Larissa Boiko", Mail = "admin@100mb.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Sarah Boinske", Mail = "jjamssmotorsports@yahoo.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Kirk Boman", Mail = "kd0j@arrl.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Perry bond", Mail = "cash@wooden.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Perry bond", Mail = "bath@wooden.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jess Bonde", Mail = "jess@rubiks.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "S. Bono", Mail = "bonosa@comcast.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Andrew Bonventre", Mail = "ajb23@po.cwru.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "John Bonzey", Mail = "jbonzey2@charter.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "charles booher", Mail = "jeffbooher714@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Neil Booth", Mail = "neil@duckhole.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Nicholas Booth", Mail = "nicholas@booth.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Stephanie Boothroyd", Mail = "stephjb26@aol.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Bordignon Grigno", Mail = "bordignon.grigno@bordignon.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Matteo Borri", Mail = "mkb@libero.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ken Borton", Mail = "ken@michigansnowcams.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Ken Borton", Mail = "kbborton@avci.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Robert Boscarelli", Mail = "bosco@boscarelli.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Dik Bots", Mail = "dbots@home.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Neil Boughton", Mail = "wickedmotors@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Devon Boulden", Mail = "boldbear@4dv.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jordan Bouvier", Mail = "jordanb@rochester.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Russell Bowen", Mail = "rbowen2@rochester.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Michael Bowes", Mail = "zap@tig.local.au" });
			users.Add(new AuthenticatedUser() { DisplayName = "Christopher Bowles", Mail = "chris@audaciously.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jason Bowman", Mail = "jbowman90@woh.rr.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "David Boyce", Mail = "dboyce@attbi.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "meghan boyle", Mail = "boymegh1@lycoming.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joern Brabandt", Mail = "joern@brabandt.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joern Brabandt", Mail = "Joern@Brabandt.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Jason Bradley", Mail = "jrbradley@charter.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "S J J Bradley", Mail = "steve@smallbluefish.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Scott Bradley", Mail = "sbradley@bhpres.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "William Bradley", Mail = "sbradley@bhpres.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Philippe Braem", Mail = "pbrm@telenet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Casey Bragg", Mail = "caseybragg@excite.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "William Braidic", Mail = "Domain_Owner@braidic.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Trevor Bramble", Mail = "trevor@killthe.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Howard Bramlett", Mail = "krisknight@clearchannel.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Andrew Branch", Mail = "awbranch@frontiernet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Berth Brandell", Mail = "berth.brandell@home.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Brian Brandt", Mail = "Webmaster@geez.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Wilhelm Brandt", Mail = "wilhelm_brandt@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Katya Braun Valle", Mail = "katya@ufm.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Aliza Braverman", Mail = "jasonb@braverman.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Donald Bray", Mail = "dlbray@blueriver.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Annette Breckling", Mail = "kknafel@ace-syscom.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Bruno Bredenberg", Mail = "bruno@surfnet.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Randy Breeser", Mail = "randy.breeser@noaa.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Joshua Brehm", Mail = "captainhaddock@mindspring.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Terry Brekko", Mail = "tyler@brekko.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Alex Brem", Mail = "alex@brem.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Maarten Bremer", Mail = "maarten@xolphin.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "William Brendel", Mail = "utmb78@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Armin Breneis", Mail = "armin.breneis@gmx.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Chris Brenes", Mail = "chrissreef@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Dave Brennan", Mail = "davebrennan@hotmail.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "Leonardo Bresani", Mail = "leonardo@acaconstrutora.local" });
			users.Add(new AuthenticatedUser() { DisplayName = "David Breuss", Mail = "dcbreuss@worldnet.att.local" });
			#endregion

			return users;
		}
	}
}