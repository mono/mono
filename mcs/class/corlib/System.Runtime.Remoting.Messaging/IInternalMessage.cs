using System;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Messaging
{
	internal interface IInternalMessage
	{
		Identity TargetIdentity { get; set; }
		string Uri { get; set; }
	}
}
