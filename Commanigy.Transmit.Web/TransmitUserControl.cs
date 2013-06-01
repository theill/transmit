#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
#endregion

namespace Commanigy.Transmit.Web {
	public class TransmitUserControl : System.Web.UI.UserControl {
		protected string _(string v, params object[] a) {
			return Localizer.t(v, a);
		}
	}
}
