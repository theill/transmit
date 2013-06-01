#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace Commanigy.Transmit.Data {
	public partial class Package {
		public static Package FindByCode(string code) {
			Package package;
			using (DataClassesDataContext db = new DataClassesDataContext()) {
				package = db.Packages.SingleOrDefault(p => p.Code == code);
				if (package == null) {
					return null;
				}

				package.Files.Load();
				package.Transfers.Load();
			}
			return package;
		}
	}

	public enum PackageStatus {
		Open = 'O',
		Expired = 'E'
	}
}
