//
// System.Runtime.Remoting.Channels.SinkProviderData.cs
//
// Author: Rodrigo Moya (rodrigo@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels
{
	public class SinkProviderData
	{
		private string sinkName;
		
		public SinkProviderData (string name)
		{
			sinkName = name;
		}

		public IList Children
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
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
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
	}
}
