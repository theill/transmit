#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace Commanigy.Transmit.Data {
	public class TransmitSettings {
		private static TransmitSettings instance = new TransmitSettings();

		public static TransmitSettings Instance {
			get {
				return instance;
			}
		}

		private Setting setting;

		public Setting Setting {
			get {
				return setting;
			}
		}

		public string ApplicationName {
			get {
				return "Transmit";
			}
		}

		public string ApplicationUrl {
			get {
				return "http://transmit.commanigy.com";
			}
		}

		public string UploadUrl {
			get {
				return this.Setting.UploadUrl + (this.Setting.UploadChunked ? "/UploadChunk.ashx" : "/Upload.ashx");
			}
		}

		public string UploadAppletArchiveUrl {
			get {
				return this.Setting.UploadUrl + "/jl_core_z.jar";
			}
		}

		public string UsersLookupUrl {
			get {
				return this.Setting.UploadUrl + "/UsersService.svc/Search";
			}
		}



		public TransmitSettings() {
			Reload();
		}

		public void Reload() {
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				setting = db.Settings.First();
			}
		}
	}
}
