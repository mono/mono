//
// System.Runtime.Remoting.Channels.SinkProviderData.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public class SinkProviderData
	{
		string sinkName;
		ArrayList children;
		Hashtable properties;
		
		public SinkProviderData (string name)
		{
			sinkName = name;
			children = new ArrayList ();
			properties = new Hashtable ();
		}

		public IList Children
		{
			get {
				return children;
			}
		}

		public string Name
	        {
			get {
				return sinkName;
			}
		}

		public IDictionary Properties
		{
			get {
				return properties;
			}
		}
	}
}
