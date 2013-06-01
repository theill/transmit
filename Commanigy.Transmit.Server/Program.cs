#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel; 
#endregion

namespace Commanigy.Transmit.Server {
	class Program {
		public static void Main(string[] args) {
			// Create a ServiceHost for the StreamingService type
			using (ServiceHost serviceHost = new ServiceHost(typeof(TransferService))) {
				// Open the ServiceHostBase to create listeners and start listening for messages.
				serviceHost.Open();

				// The service can now be accessed.
				Console.WriteLine("Transmit transfer service is ready.");
				Console.WriteLine("Press <ENTER> to terminate.");
				Console.WriteLine();
				Console.ReadLine();
			}
		}
	}
}