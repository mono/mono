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
		internal const string HeadersKey = "__Headers";
		internal const string ContextKey = "__CallContext";
		
		private CallContext ()
		{
		}

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
			return (Header[]) Datastore [HeadersKey];
		}

		public static void SetData (string name, object data) 
		{
			Datastore [name] = data;
		}

		public static void SetHeaders (Header[] headers) 
		{
			Datastore [HeadersKey] = headers;
		}

		internal static LogicalCallContext CreateLogicalCallContext ()
		{
			LocalDataStoreSlot ds = Thread.GetNamedDataSlot (ContextKey);
			Hashtable res = (Hashtable) Thread.GetData (ds);

			LogicalCallContext ctx = new LogicalCallContext();
			if (res == null) return ctx;

			foreach (DictionaryEntry entry in res)
				if (entry.Value is ILogicalThreadAffinative)
					ctx.SetData ((string)entry.Key, entry.Value);

			return ctx;
		}

		internal static object SetCurrentCallContext (LogicalCallContext ctx)
		{
			LocalDataStoreSlot ds = Thread.GetNamedDataSlot (ContextKey);
			object oldData = Thread.GetData (ds);

			if (ctx != null && ctx.HasInfo)
			{
				Hashtable newData = new Hashtable();
				Hashtable data = ctx.Datastore;
				
				foreach (DictionaryEntry entry in data)
					newData [(string)entry.Key] = entry.Value;
	
				Thread.SetData (ds, newData);
			}
			else
				Thread.SetData (ds, null);
				
			return oldData;
		}
		
		internal static void UpdateCurrentCallContext (LogicalCallContext ctx)
		{
			Hashtable data = ctx.Datastore;
			foreach (DictionaryEntry entry in data)
				SetData ((string)entry.Key, entry.Value);
		}
		
		internal static void RestoreCallContext (object oldContext)
		{
			LocalDataStoreSlot ds = Thread.GetNamedDataSlot (ContextKey);
			Thread.SetData (ds, oldContext);
		}

		private static Hashtable Datastore
		{
			get 
			{
				LocalDataStoreSlot ds = Thread.GetNamedDataSlot (ContextKey);
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
