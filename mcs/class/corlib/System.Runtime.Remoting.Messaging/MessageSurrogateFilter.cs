//
// System.Runtime.Remoting.Messaging.MessageSurrogateFilter.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) 2002, Copyright. Ximian, Inc.
//

namespace System.Runtime.Remoting.Messaging {

	[Serializable]
	public delegate bool MessageSurrogateFilter (string key, object value);
}
