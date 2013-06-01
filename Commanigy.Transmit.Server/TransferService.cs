#region Using directives
using System;
using System.Configuration;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Xml.Serialization;
using Commanigy.Transmit.Data;
using log4net;
#endregion

namespace Commanigy.Transmit.Server {
	/// <summary>
	/// 
	/// </summary>
	public class TransferService : ITransfer {
		private ILog log = LogManager.GetLogger("server");

		/// <summary>
		/// 
		/// </summary>
		/// <param name="msg"></param>
		public void Upload(UploadMessage msg) {
			DateTime startTime = DateTime.Now;

			string filePath = System.IO.Path.Combine(TransmitSettings.Instance.Setting.StorageLocation, msg.Meta.Code + ".zip");
			try {
				Console.WriteLine("Saving to file {0}", filePath);
				log.DebugFormat("Receiving \"{0}\"", filePath);

				System.IO.FileStream outstream = System.IO.File.Open(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				//read from the input stream in 4K chunks
				//and save to output stream
				const int bufferLen = 4096;
				byte[] buffer = new byte[bufferLen];
				int count = 0;
				while ((count = msg.Content.Read(buffer, 0, bufferLen)) > 0) {
					Console.Write(".");
					outstream.Write(buffer, 0, count);
				}
				outstream.Close();
				msg.Content.Close();

				TimeSpan duration = DateTime.Now - startTime;
				Console.WriteLine();
				Console.WriteLine("File {0} saved in {1} seconds", filePath, duration.Milliseconds);
				log.DebugFormat("Receiving file \"{0}\" in {1} seconds", filePath, duration.Milliseconds);
			}
			catch (System.IO.IOException ex) {
				Console.WriteLine(string.Format("An exception was thrown while opening or writing to file {0}", filePath));
				Console.WriteLine("Exception is: ");
				Console.WriteLine(ex.ToString());
				throw ex;
			}
		}


		public string SharePackage(string sender, string[] recipients, string packageCode, string[] tokens, string subject, string message) {
			Console.WriteLine("Sharing package from " + sender + " for package " + packageCode + " containing " + tokens.Length + " files");
			log.InfoFormat("Sharing package from \"{0}\"", sender);

			Package package = new Package() {
				Code = packageCode,
				SenderMail = sender,
				Message = subject + "\n" + message,
				Status = (char)PackageStatus.Open,
				Scanned = false,
				ExpiresAt = DateTime.UtcNow.AddDays(14),
				CreatedAt = DateTime.UtcNow
			};

			using (DataClassesDataContext db = new DataClassesDataContext()) {
				foreach (string fileHash in tokens) {
					File f = new File() {
						CreatedAt = DateTime.UtcNow,
						FileHash = fileHash,
						FileSize = 0/*Storage.GetFileSize(package.Code, fileHash)*/,
						Package = package,
					};
					db.Files.InsertOnSubmit(f);
				}

				db.SubmitChanges();
			}

			string response = TransmitSettings.Instance.Setting.OutlookInjectedHtml ?? "Download: [TransmitUrl]/receive.aspx?h=[Package.Code]";
			response = response.Replace("[Package.Code]", packageCode);
			response = response.Replace("[TransmitUrl]", TransmitSettings.Instance.Setting.ExternalUrl);
			return response;
		}
	}
}