#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Commanigy.Transmit.Client;
using System.IO;
#endregion

namespace Commanigy.Transmit.Client {
	public class Transmitter {
		public event TransmitterCompletedHandler Completed;

		private List<string> files;
		private Dictionary<string, string> options;

		public Transmitter(List<string> files, Dictionary<string, string> options) {
			this.files = files;
			this.options = options;
		}


		public void Start() {
			new Thread(Run).Start();
		}


		void Run() {
			string token = Guid.NewGuid().ToString("N");
			Upload(token, Pack());

			string response = SharePackage(token);

			if (this.Completed != null) {
				TransmitCompletedResult result = new TransmitCompletedResult();
				result.Token = token;
				result.Response = response;

				this.Completed(this, result);
			}
		}


		private string SharePackage(string token) {
			string[] fileTokens = files.ToArray();
			for (int i = 0; i < fileTokens.Length; i++) {
				fileTokens[i] = Path.GetFileName(fileTokens[i]);
			}

			using (TransferClient transfer = new TransferClient("BasicHttpBinding_ITransfer")) {
				string result = transfer.SharePackage(options["SenderMail"], new string[0], token, fileTokens, options["Subject"], options["Body"]);
				transfer.Close();
				return result;
			}
		}


		private string Pack() {
			string zippedSourceFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(zippedSourceFolder);
			foreach (var f in files) {
				File.Copy(f, Path.Combine(zippedSourceFolder, Path.GetFileName(f)), true);
			}

			string zippedArchive = Path.GetTempFileName();
			ICSharpCode.SharpZipLib.Zip.FastZip fz = new ICSharpCode.SharpZipLib.Zip.FastZip();
			fz.CreateZip(zippedArchive, zippedSourceFolder, true, "", "");
			return zippedArchive;
		}


		/// <summary>
		/// Performs actual upload by streaming entire (zipped) file.
		/// </summary>
		/// <param name="token"></param>
		/// <param name="transferFileName"></param>
		/// <returns></returns>
		private string Upload(string token, string transferFileName) {
			using (TransferClient transfer = new TransferClient("BasicHttpBinding_ITransfer")) {
				using (FileStream stream = File.OpenRead(transferFileName)) {
					schemas.commanigy.com.UploadMeta meta = new schemas.commanigy.com.UploadMeta();
					meta.Code = token;
					transfer.Upload(meta, stream);
					stream.Close();
				}

				transfer.Close();
			}

			return token;
		}

		public delegate void TransmitterCompletedHandler(object sender, TransmitCompletedResult result);
	}
}