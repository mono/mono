//
// System.Runtime.Remoting.Messaging.ISerializationRootObject.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging
{
	internal interface ISerializationRootObject
	{
		void RootSetObjectData (SerializationInfo info, StreamingContext context);
	}
}
