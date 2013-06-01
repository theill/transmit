#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
#endregion

namespace Commanigy.Transmit.Server {
	[MessageContract]
	public class UploadMessage {
		[MessageHeader(MustUnderstand = true)]
		public UploadMeta Meta { get; set; }

		[MessageBodyMember(Order = 1)]
		public Stream Content { get; set; }
	}
}
