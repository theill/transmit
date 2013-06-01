// notice, you are -not- able to include this namespace because the custom generator tool will fail to run then
// using System.Configuration;

namespace Commanigy.Transmit.Data {
	partial class DataClassesDataContext {
		partial void OnCreated() {
			System.Configuration.ConnectionStringSettings settings = System.Configuration.ConfigurationManager.ConnectionStrings["transmitConnectionString"];
			if (settings != null) {
				this.Connection.ConnectionString = settings.ConnectionString;
			}
		}
	}
}