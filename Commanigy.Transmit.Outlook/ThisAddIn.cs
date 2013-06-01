#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Office = Microsoft.Office.Core;
using System.IO;
using System.Runtime.InteropServices;
using Commanigy.Transmit.Client;
using System.Configuration; 
#endregion

namespace Commanigy.Transmit.OutlookAddin {
	/// <summary>
	/// 
	/// </summary>
	public partial class ThisAddIn {
		private int fileSizeLimit = 250 * 1024; // only transfer file if above 250kb
		private int signatureLogoSizeLimit = 100 * 1024;

		private Microsoft.Office.Interop.Outlook.MailItem mailItem;
		private string entryID;
		private TransferForm transferForm;

		private readonly string SUBJECT_TRANSMITTED = Guid.NewGuid().ToString("N");

		private List<string> filesToTransmit;

		private Dictionary<string, string> options = new Dictionary<string, string>();

		private void ThisAddIn_Startup(object sender, System.EventArgs e) {
			this.Application.ItemSend += new Microsoft.Office.Interop.Outlook.ApplicationEvents_11_ItemSendEventHandler(Application_ItemSend);
		}

		void Application_ItemSend(object item, ref bool Cancel) {
			mailItem = (Microsoft.Office.Interop.Outlook.MailItem)item;

			if (string.IsNullOrEmpty(mailItem.Subject) || !mailItem.Subject.StartsWith(SUBJECT_TRANSMITTED)) {
				// new mail needed to be processed
				this.filesToTransmit = new List<string>();

				CopyAttachedFiles(FindAttachedFiles());

				bool saved = false;
				try {
					this.mailItem.Save();
					saved = true;
				}
				catch (COMException x) {
					System.Windows.Forms.MessageBox.Show(x.Message);
					//this.RestoreFilesToSend(this.mailItem);
					//this.RestoreFoldersToSend(this.mailItem);
				}

				if (saved && filesToTransmit.Count > 0) {
					Transfer();

					// cancel normal send (place in Draft) so it's possible to lookup later
					Cancel = true;
				}
			}
			else {
				mailItem.Subject = mailItem.Subject.Substring(SUBJECT_TRANSMITTED.Length);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		private List<string> FindAttachedFiles() {
			List<string> attachments = new List<string>();
			for (int i = 0; i < this.mailItem.Attachments.Count; i++) {
				Microsoft.Office.Interop.Outlook.Attachment attachment = this.mailItem.Attachments[i + 1];
				if ((attachment.Type == Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue) || (attachment.Type == Microsoft.Office.Interop.Outlook.OlAttachmentType.olByReference)) {
					string attachmentFileName = attachment.FileName.ToLower();
					if (!attachmentFileName.EndsWith(".transmit")) {
						string tempFileName = Path.GetTempFileName();
						try {
							try {
								attachment.SaveAsFile(tempFileName);

								if (!IsSignature(tempFileName, attachmentFileName)) {
									attachments.Add(attachment.FileName);
								}
							}
							catch (COMException) {

							}
						}
						finally {
							File.Delete(tempFileName);
						}
					}
				}
			}

			return attachments;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="tempFileName"></param>
		/// <param name="attachmentFileName"></param>
		/// <returns></returns>
		private bool IsSignature(string tempFileName, string attachmentFileName) {
			long length = new FileInfo(tempFileName).Length;

			// do not remove any signature files from mail. Figure this out simply based on filesize
			bool signatureLogo = ((length < signatureLogoSizeLimit) && ((attachmentFileName.EndsWith(".gif") || attachmentFileName.EndsWith(".png")) || attachmentFileName.EndsWith(".jpg")));
			return signatureLogo || (length < fileSizeLimit);
		}


		/// <summary>
		/// 
		/// </summary>
		private void Transfer() {
			this.entryID = this.mailItem.EntryID;
			//this.addin.CloseInspector();
			options.Add("SenderMail", this.mailItem.SendUsingAccount.SmtpAddress ?? "Unknown");
			options.Add("Subject", this.mailItem.Subject ?? "");
			options.Add("Body", this.mailItem.Body);
			string[] strArray = new string[this.mailItem.Recipients.Count];
			for (int n = 0; n < this.mailItem.Recipients.Count; n++) {
				strArray[n] = this.mailItem.Recipients[n + 1].Address;
			}
			options.Add("Recipients", string.Join(";", strArray));
			
			transferForm = new TransferForm(filesToTransmit, options, this.TransmitDone);
			transferForm.StartTransfer();

			//Progress progress = new Progress(loginManager, this.completeFilesToSend, this.foldersToSend, parameters);
			//progress.Transport.OnBluewhaleDone += new Transport.BluewhaleDoneHandler(this.BluewhaleDoneHandler);
			//progress.Send();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		private void CopyAttachedFiles(List<string> files) {
			string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			new DirectoryInfo(tempDirectory).Create();

			foreach (string attachedFile in files) {
				for (int m = 0; m < this.mailItem.Attachments.Count; m++) {
					Microsoft.Office.Interop.Outlook.Attachment attachment = this.mailItem.Attachments[m + 1];
					if (((attachment.Type == Microsoft.Office.Interop.Outlook.OlAttachmentType.olByValue) || (attachment.Type == Microsoft.Office.Interop.Outlook.OlAttachmentType.olByReference)) && (attachment.FileName == Path.GetFileName(attachedFile))) {
						string item = tempDirectory + @"\" + attachment.FileName;
						this.filesToTransmit.Add(item);
						attachment.SaveAsFile(item);
						attachment.Delete();
					}
				}
			}
		}


		/// <summary>
		/// Files have been successfully transferred and we have a transmit token.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="transmitToken"></param>
		void TransmitDone(object sender, TransmitCompletedResult result) {
			// we might need to have this as a local variable
			Microsoft.Office.Interop.Outlook.Application applicationObject = this.Application;

			Microsoft.Office.Interop.Outlook.MailItem mailItem = null;
			try {
				object storeID = applicationObject.Session.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderDrafts).StoreID;
				mailItem = (Microsoft.Office.Interop.Outlook.MailItem)applicationObject.Session.GetItemFromID(this.entryID, storeID);
				if (!string.IsNullOrEmpty(result.Response)) {
					mailItem.Subject = SUBJECT_TRANSMITTED + mailItem.Subject;
					if (mailItem.HTMLBody != null) {
						string linkHtml = result.Response;

						string str2 = "<img border=\"0\" width=\"120\" height=\"35\" src=\"cid:reply.gif\"/>";
						int index = linkHtml.IndexOf(str2);
						if (index > -1) {
							linkHtml = linkHtml.Substring(0, index - 1) + linkHtml.Substring(index + str2.Length);
						}
						int length = -1;
						length = mailItem.HTMLBody.ToUpper().LastIndexOf("</BODY>");
						if (length != -1) {
							mailItem.HTMLBody = mailItem.HTMLBody.Substring(0, length) + linkHtml + mailItem.HTMLBody.Substring(length, mailItem.HTMLBody.Length - length);
						}
						else {
							mailItem.HTMLBody += "\n" + linkHtml;
						}
					}
					else {
						mailItem.Body = mailItem.Body + result.Response;
					}

					// do actual sending of email (move from Draft to Outbox)
					(mailItem as Microsoft.Office.Interop.Outlook._MailItem).Send();
				}
				else {
					mailItem.Display(false);
					//this.RestoreFilesToSend(mailItem);
					//this.RestoreFoldersToSend(mailItem);
				}
				try {
					foreach (string str3 in this.filesToTransmit) {
						File.Delete(str3);
					}
					if (this.filesToTransmit.Count > 0) {
						new DirectoryInfo(Path.GetDirectoryName(this.filesToTransmit[0])).Delete();
					}
				}
				catch (Exception exception) {
					System.Windows.Forms.MessageBox.Show(exception.Message);
					//MessageBox.Show(ForegroundWindow.Instance, Messages.Instance.Get("ErrorCleanup"), Messages.Instance.Get("MessageTitle"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					//ErrorLogger.Log("ErrorCleanup", exception);
				}
			}
			catch (Exception exception2) {
				System.Windows.Forms.MessageBox.Show(exception2.Message);
				//MessageBox.Show(ForegroundWindow.Instance, Messages.Instance.Get("ErrorSending"), Messages.Instance.Get("MessageTitle"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
				//ErrorLogger.Log("ErrorSending", exception2);
			}
			finally {
				if (mailItem != null) {
					Marshal.ReleaseComObject(mailItem);
				}
				mailItem = null;
				if (applicationObject != null) {
					Marshal.ReleaseComObject(applicationObject);
				}
				applicationObject = null;
				transferForm.Close();
				transferForm = null;
			}
		}

		private void ThisAddIn_Shutdown(object sender, System.EventArgs e) {

		}

		#region VSTO generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InternalStartup() {
			this.Startup += new System.EventHandler(ThisAddIn_Startup);
			this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
		}

		#endregion
	}
}