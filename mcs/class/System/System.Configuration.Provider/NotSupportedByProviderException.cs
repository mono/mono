//
// System.Configuration.Provider.NotSupportedByProviderException
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Runtime.Serialization;

namespace System.Configuration.Provider {
	[Serializable]
	public class NotSupportedByProviderException : Exception {
		public NotSupportedByProviderException () : base () {}
		protected NotSupportedByProviderException (SerializationInfo info, StreamingContext context)  : base (info, context) {}
		public NotSupportedByProviderException (string message) : base (message) {}
		public NotSupportedByProviderException (string message, Exception innerException)  : base (message, innerException) {}
	}
}
#endif