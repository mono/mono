//
// System.Runtime.Remoting.Messaging.HeaderHandler.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright Ximian, Inc. 2002
//

namespace System.Runtime.Remoting.Messaging {

	[Serializable]
	public delegate object HeaderHandler (Header[] headers);
}
