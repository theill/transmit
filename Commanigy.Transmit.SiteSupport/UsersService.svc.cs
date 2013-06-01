#region Using directives
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Commanigy.Transmit.Web;
using Commanigy.Transmit.Data;
using System.Web;
using System.Collections.Generic; 
#endregion

namespace Commanigy.Transmit.SiteSupport {
	[DataContract]
	public class Person {
		[DataMember]
		internal string displayName;

		[DataMember]
		internal string mail;

		[DataMember]
		internal string department;

		[DataMember]
		internal string country;

		[DataMember]
		internal string location;

		[DataMember]
		internal string title;

		[DataMember]
		internal string company;

		[DataMember]
		internal string url;

		[DataMember]
		internal string type;
	}

	[ServiceContract(Namespace = "http://commanigy.com/")]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class UsersService {
		[OperationContract]
		[WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
		public Person[] Search() {
			return this.Query(HttpContext.Current.Request["q"]);
		}

		[OperationContract]
		[WebGet(BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
		public Person[] Query(string query) {
			List<AuthenticatedUser> foundUsers = Locator.FindByDisplayName(query);
			return foundUsers.ConvertAll<Person>(new Converter<AuthenticatedUser, Person>(delegate(AuthenticatedUser user) {
				Person p = new Person();
				p.displayName = user.DisplayName ?? user.CommonName;
				p.mail = user.Mail;
				p.department = user.Department;
				p.country = user.Country;
				p.location = user.Location;
				p.title = user.Title;
				p.company = user.Company;
				p.url = user.Url;
				p.type = Enum.GetName(typeof(AuthenticatedUser.PersonType), user.Type);
				return p;
			})).ToArray();
		}
	}
}