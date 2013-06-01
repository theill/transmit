#region Using directives
using System;
using System.Collections.Generic;
using System.Text; 
#endregion

namespace JumploaderWrapper {
	public interface IFileSaver {
		bool SaveFile(string fileName, byte[] data);
	}
}