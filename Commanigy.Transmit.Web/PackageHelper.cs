#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using Commanigy.Transmit.Data;
using System.Diagnostics; 
#endregion

namespace Commanigy.Transmit.Web {
	public class PackageHelper {
		public static string FilesToMailHtml(List<File> entitySet) {
			string files = "<ul>";
			foreach (var f in entitySet) {
				files += "<li>" + f.FileHash + " <em style=\"color: #999999\">" + NumberHelper.NumberToHumanSize(f.FileSize) + "</em></li>";
			}
			files += "</ul>";

			return files;
		}

		public static bool Scan(Package package) {
			//Process si = new Process();
			////si.StartInfo.WorkingDirectory = "c:\\";
			//si.StartInfo.UseShellExecute = false;
			//si.StartInfo.FileName = "cmd.exe";
			//si.StartInfo.Arguments = "/C dir";
			//si.StartInfo.CreateNoWindow = true;
			//si.StartInfo.RedirectStandardInput = true;
			//si.StartInfo.RedirectStandardOutput = true;
			//si.StartInfo.RedirectStandardError = true;
			//si.Start();
			//string output = si.StandardOutput.ReadToEnd();
			//si.Close();

			//return output;
			return true;
		}

	}
}