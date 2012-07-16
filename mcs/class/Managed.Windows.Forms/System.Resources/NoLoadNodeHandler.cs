using System;
using System.Resources;

namespace System.Resources {
	internal abstract class NoLoadNodeHandler : ResXDataNodeHandler {
		public NoLoadNodeHandler ()
		{
		}		

		public abstract string OutputXml ();

	}
}

