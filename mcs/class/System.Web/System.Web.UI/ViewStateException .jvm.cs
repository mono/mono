#if NET_2_0
using System.Runtime.Serialization;

namespace System.Web.UI
{
	[SerializableAttribute]
	public sealed class ViewStateException : Exception, ISerializable
	{
		public ViewStateException () {
			throw new NotImplementedException ();
		}
		public bool IsConnected {
			get {
				throw new NotImplementedException ();
			}
		}
		public override string Message {
			get {
				throw new NotImplementedException ();
			}
		}
		public string Path {
			get {
				throw new NotImplementedException ();
			}
		}
		public string PersistedState {
			get {
				throw new NotImplementedException ();
			}
		}
		public string Referer {
			get {
				throw new NotImplementedException ();
			}
		}
		public string RemoteAddress {
			get {
				throw new NotImplementedException ();
			}
		}
		public string RemotePort {
			get {
				throw new NotImplementedException ();
			}
		}
		public string UserAgent {
			get {
				throw new NotImplementedException ();
			}
		}
		public override void GetObjectData (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}
	}
}
#endif
