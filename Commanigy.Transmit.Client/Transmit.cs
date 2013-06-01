#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms; 
#endregion

namespace Commanigy.Transmit.Client {
	public partial class Transmit : Form {
		public Transmit() {
			InitializeComponent();
		}

		private void TbxRecipients_TextChanged(object sender, EventArgs e) {
			using (UsersRef.UsersServiceClient a = new UsersRef.UsersServiceClient("WebHttpBinding_UsersService")) {
				UsersRef.Person[] people = a.Query(TbxRecipients.Text);
				listBox1.Items.Clear();
				foreach (var item in people) {
					listBox1.Items.Add(item.displayName);
				}

				a.Close();
			}
		}
	}
}