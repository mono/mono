//
// System.Runtime.Remoting.IEnvoyInfo.cs
//
// AUthor: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting {

	public interface IEnvoyInfo
	{
		IMessageSink EnvoySinks { get; set; }
	}
}
