//
// Microsoft.Web.Services.Messaging.SoapChannelCollection.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Collections;

namespace Microsoft.Web.Services.Messaging
{
	public class SoapChannelCollection
	{

		private Hashtable _channels = new Hashtable ();

		public void Add (string to, SoapChannel channel)
		{
			if(to == null || to.Length == 0) {
				throw new ArgumentNullException ("to");
			}
			if(channel == null) {
				throw new ArgumentNullException ("channel");
			}

			_channels[to] = channel;
		}

		public void Clear ()
		{
			_channels.Clear ();
		}

		public bool Contains (string to)
		{
			if(to == null || to.Length == 0) {
				throw new ArgumentNullException ("to");
			}
			return _channels.Contains (to);
		}

		public IEnumerator GetEnumerator ()
		{
			return _channels.Values.GetEnumerator ();
		}

		public void Remove (string to)
		{
			if(to == null || to.Length == 0) {
				throw new ArgumentNullException ("to");
			}
			_channels.Remove (to);
		}

		public int Count {
			get { return _channels.Values.Count; }
		}

		public SoapChannel this [string index] {
			get { return (SoapChannel) _channels[index]; }
		}

		public ICollection Keys {
			get { return _channels.Keys; }
		}

		public object SyncRoot {
			get { return _channels.SyncRoot; }
		}

		public ICollection Values {
			get { return _channels.Values; }
		}
	}
}
