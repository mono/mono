//
// System.Runtime.Remoting.Messaging.OneWayAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;

namespace System.Runtime.Remoting.Messaging {

	[AttributeUsage (AttributeTargets.Method)]
	public class OneWayAttribute : Attribute
	{
		public OneWayAttribute ()
		{
		}
	}
}
