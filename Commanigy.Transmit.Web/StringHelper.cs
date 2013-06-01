using System;
using System.Collections.Generic;
using System.Text;

namespace Commanigy.Transmit.Web {
	public class StringHelper {
		public static string Truncate(string s, int length) {
			if (s.Length > length && length > 3) {
				return s.Substring(0, length - 3) + "...";
			}

			return s.Substring(0, Math.Min(length, s.Length));
		}

		public static string Pluralize(int v, string singular, string plural) {
			return string.Format("{0} {1}", v, (v != 1) ? plural : singular);
		}

		public static string IfEmpty(string v, string defaultString) {
			return string.IsNullOrEmpty(v) ? defaultString : v;
		}
	}
}
