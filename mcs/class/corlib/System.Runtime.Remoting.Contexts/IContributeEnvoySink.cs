//
// System.Runtime.Remoting.Contexts.IContributeEnvoySink.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Contexts {

	public interface IContributeEnvoySink
	{
		IMessageSink GetEnvoySink (MarshalByRefObject obj, IMessageSink nextSink);
	}
}
