//
// System.Runtime.Remoting.Channels.TransportHeaders.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
        [Serializable]
	public class TransportHeaders : ITransportHeaders
	{
		Hashtable hash_table;
		
		public TransportHeaders ()
		{
			hash_table = new Hashtable ();
		}

		public object this [object key]
	        {
			get {
				return  hash_table [key];
			}
			
			set {
				hash_table [key] = value;
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return  hash_table.GetEnumerator ();
		}
	}
}
