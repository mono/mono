// 
// System.Runtime.Remoting.Messaging.CallContext.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//
// (c) 2002, Jaime Anguiano Olarra
//
// FIXME: This is just a skeleton for practical purposes.
///<summary>
///Provides several properties that come with the execution code path.
///This class is sealed.
///</summary>

using System;

namespace System.Runtime.Remoting.Messaging 
{
	
	[Serializable]
	public sealed class CallContext 
	{
		// public methods
		[MonoTODO]
		public static void FreeNamedDataSlot (string name) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetData (string name) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Header[] GetHeaders () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetData (string name, object data) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetHeaders (Header[] headers) 
		{
			throw new NotImplementedException ();
		}
	}
}
