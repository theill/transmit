#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web; 
#endregion

namespace Commanigy.Transmit.Web {
	public class ValidationError : IValidator {
		private ValidationError(string message) {
			ErrorMessage = message;
			IsValid = false;
		}

		public string ErrorMessage { get; set; }

		public bool IsValid { get; set; }

		public void Validate() {
			// no action required
		}

		public static void Display(string message) {
			Page currentPage = HttpContext.Current.Handler as Page;
			currentPage.Validators.Add(new ValidationError(message));
		}
	}
}
