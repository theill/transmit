#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Commanigy.Transmit.Client.Properties;
#endregion

namespace Commanigy.Transmit.Client {
	public partial class TransferForm : Form {
		private NotifyIcon icon = new NotifyIcon();
		private Transmitter transmitter;

		public TransferForm() {
			InitializeComponent();
		}

		public TransferForm(List<string> files, Dictionary<string, string> options, Transmitter.TransmitterCompletedHandler onTransmitDone) {
			InitializeComponent();

			InitializeStatusBalloon();

			transmitter = new Transmitter(files, options);
			if (onTransmitDone != null) {
				transmitter.Completed += new Transmitter.TransmitterCompletedHandler(onTransmitDone);
			}

			ShowStatusBalloon(this, null);
		}

		private void InitializeStatusBalloon() {
			icon.Text = "Transmit";
			icon.Icon = Resources.Upload;
			icon.Visible = true;
			icon.DoubleClick += new EventHandler(ShowStatusBalloon);
		}

		public void ShowStatusBalloon(object sender, EventArgs e) {
			icon.ShowBalloonTip(5000, "Transmit is uploading files", "Transmit is currently uploading your selected files.", ToolTipIcon.Info);
		}

		public void StartTransfer() {
			transmitter.Start();

			//ThreadStart start = new ThreadStart(this.transport.Send);
			//new Thread(start).Start();
			//this.timer.Enabled = true;
			//this.timer.Interval = 200;
			//this.timer.Tick += new EventHandler(this.Timer_Tick);
			//base.Visible = true;
		}

		private void button1_Click(object sender, EventArgs e) {
			using (TransferClient transfer = new TransferClient("BasicHttpBinding_ITransfer")) {
				using (FileStream stream = File.OpenRead("Commanigy.Transmit.Client.pdb")) {
					schemas.commanigy.com.UploadMeta md = new schemas.commanigy.com.UploadMeta();
					md.Code = Guid.NewGuid().ToString("N");
					transfer.Upload(md, stream);
					stream.Close();
				}

				transfer.Close();
			}
		}

		private void TransferForm_FormClosed(object sender, FormClosedEventArgs e) {
			this.icon.Dispose();
			this.icon = null;
		}
	}
}