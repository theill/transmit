#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.ServiceModel.Channels;
using System.Runtime.Serialization;
#endregion

namespace Commanigy.Transmit.Server {
	[ServiceContract(Namespace = "http://schemas.commanigy.com")]
	public interface ITransfer {
		[OperationContract]
		void Upload(UploadMessage request);

		[OperationContract]
		string SharePackage(string sender, string[] recipients, string packageCode, string[] tokens, string subject, string message);
	}

	[DataContract(Namespace = "http://schemas.commanigy.com")]
	public class UploadMeta {
		[DataMember]
		public string Code;
	}
}