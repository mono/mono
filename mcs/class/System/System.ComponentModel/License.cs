//
// System.ComponentModel.License.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	public abstract class License : IDisposable
	{

		protected License()
		{
			// Intentionally empty
		}

		public abstract string LicenseKey { get; }
		public abstract void Dispose();
	}
}
