//
// System.Runtime.Remoting.EnvoyInfo.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting
{
	[Serializable]
	internal class EnvoyInfo: IEnvoyInfo
	{
		IMessageSink envoySinks;

		public EnvoyInfo (IMessageSink sinks)
		{
			envoySinks = sinks;
		}

		public IMessageSink EnvoySinks 
		{ 
			get { return envoySinks; }
			set { envoySinks = value; }
		}
	}
}
