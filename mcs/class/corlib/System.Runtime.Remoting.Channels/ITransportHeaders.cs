//
// System.Runtime.Remoting.Channels.ITransportHeaders.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;

namespace System.Runtime.Remoting.Channels {

	public interface ITransportHeaders
	{
		object this [object key] { get; set; }

		IEnumerator GetEnumerator();
	}
}
