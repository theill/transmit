using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Commanigy.Transmit.Web {
	/// <summary>
	/// Stolen from https://mvcactionpack.svn.codeplex.com/svn/ActionView/Helpers/NumberHelper.cs
	/// </summary>
	public class NumberHelper {
		private static readonly Regex witharea = new Regex("([0-9]{1,3})([0-9]{3})([0-9]{4}$)", RegexOptions.Compiled);
		private static readonly Regex withoutarea = new Regex("([0-9]{1,3})([0-9]{3})([0-9]{4})$", RegexOptions.Compiled);
		private static readonly Regex numberwithdelims = new Regex(@"(\d)(?=(\d\d\d)+(?!\d))", RegexOptions.Compiled);
		public static string NumberToPhone(string number, string countryCode, string extension, string delimiter, bool includeAreaCode) {
			StringBuilder sb = new StringBuilder(20);
			if (!string.IsNullOrEmpty(countryCode)) sb.Append("+").Append(countryCode).Append(delimiter);
			if (includeAreaCode)
				sb.Append(witharea.Replace(number, "($1) $2" + delimiter + "$3"));
			else
				sb.Append(withoutarea.Replace(number, "$1" + delimiter + "$2" + delimiter + "$3"));

			if (!string.IsNullOrEmpty(extension)) sb.Append(" x ").Append(extension);

			return sb.ToString();
		}

		public static string NumberWithDelimiter(int number) {
			return NumberWithDelimiter(number.ToString());
		}
		public static string NumberWithDelimiter(double number) {
			return NumberWithDelimiter(number.ToString());
		}
		public static string NumberWithDelimiter(string number) {
			return NumberWithDelimiter(number, ",", ".");
		}
		public static string NumberWithDelimiter(string number, string delimiter, string separator) {
			string[] parts = number.Split('.');
			parts[0] = numberwithdelims.Replace(parts[0], "$1" + delimiter);
			return string.Join(separator, parts);
		}

		public static string NumberToHumanSize(long size) {
			return NumberToHumanSize(size, 1);
		}

		public static string NumberToHumanSize(long size, int precision) {
			//IEEE definitions
			if (size == 1) {
				return "1 Byte";
			}
			else if (size < 1000) {
				return String.Format("{0:F" + precision + "}", size) + " Bytes";
			}
			else if (size < 1000000) {
				return String.Format("{0:F" + precision + "}", size / 1000.0) + " KB";
			}
			else if (size < 1000000000) {
				return String.Format("{0:F" + precision + "}", size / 1000000.0) + " MB";
			}
			else if (size < 1000000000000) {
				return String.Format("{0:F" + precision + "}", size / 1000000000.0) + " GB";
			}
			else {
				return String.Format("{0:F" + precision + "}", size / 1000000000000.0) + " TB";
			}
		}
	}
}
