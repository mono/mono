// 
// System.Runtime.Remoting.Messaging.CallContext.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// (c) 2002, Jaime Anguiano Olarra
//
///<summary>
///Provides several properties that come with the execution code path.
///This class is sealed.
///</summary>

using System;
using System.Threading;
using System.Collections;

namespace System.Runtime.Remoting.Messaging 
{
	
	[Serializable]
	public sealed class CallContext 
	{
		// public methods
		public static void FreeNamedDataSlot (string name) 
		{
			Datastore.Remove (name);
		}

		public static object GetData (string name) 
		{
			return Datastore [name];
		}

		public static Header[] GetHeaders () 
		{
			return (Header[]) Datastore ["__Headers"];
		}

		public static void SetData (string name, object data) 
		{
			Datastore [name] = data;
		}

		public static void SetHeaders (Header[] headers) 
		{
			Datastore ["__Headers"] = headers;
		}

		internal static LogicalCallContext CreateLogicalCallContext ()
		{
			LocalDataStoreSlot ds = Thread.GetNamedDataSlot ("__CallContext");
			Hashtable res = (Hashtable) Thread.GetData (ds);

			LogicalCallContext ctx = new LogicalCallContext();
			if (res == null) return ctx;

			foreach (DictionaryEntry entry in res)
				if (entry.Value is ILogicalThreadAffinative)
					ctx.SetData ((string)entry.Key, entry.Value);

			return ctx;
		}

		internal static void SetCurrentCallContext (LogicalCallContext ctx)
		{
			Hashtable data = ctx.Datastore;
			if (data == null) return;

			foreach (DictionaryEntry entry in data)
				SetData ((string)entry.Key, entry.Value);
		}

		internal static void ResetCurrentCallContext ()
		{
			LocalDataStoreSlot ds = Thread.GetNamedDataSlot ("__CallContext");
			Thread.SetData (ds, null);
		}

		private static Hashtable Datastore
		{
			get {
				LocalDataStoreSlot ds = Thread.GetNamedDataSlot ("__CallContext");
				Hashtable res = (Hashtable) Thread.GetData (ds);
				if (res == null) {
					res = new Hashtable ();
					Thread.SetData (ds, res);
				}
				return res;
			}
		}
	}

	public interface ILogicalThreadAffinative
	{
	}
}
