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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Threading;
using System.Collections;

namespace System.Runtime.Remoting.Messaging 
{
	
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public sealed class CallContext 
	{
		[ThreadStatic] static Header [] Headers;
		[ThreadStatic] static Hashtable logicalDatastore;
		[ThreadStatic] static Hashtable datastore;
		[ThreadStatic] static object hostContext;
		
		private CallContext ()
		{
		}

		public static object HostContext {
			get { return hostContext; }
			set { hostContext = value; }
		}

		// public methods
		public static void FreeNamedDataSlot (string name) 
		{
			Datastore.Remove (name);
			LogicalDatastore.Remove (name);
		}

		public static object GetData (string name) 
		{
			if (LogicalDatastore.ContainsKey (name)) {
				return LogicalDatastore [name];
			} else {
				return Datastore [name];
			}
		}
		
		public static void SetData (string name, object data) 
		{
			if (data is ILogicalThreadAffinative) {
				LogicalSetData (name, data);
			} else {
				LogicalDatastore.Remove (name);
				Datastore [name] = data;
			}
		}
		
		public static object LogicalGetData (string name) 
		{
			return LogicalDatastore [name];
		}

		public static void LogicalSetData (string name, object data) 
		{
			Datastore.Remove (name);
			LogicalDatastore [name] = data;
		}

		public static Header[] GetHeaders () 
		{
			return Headers;
		}
		
		public static void SetHeaders (Header[] headers) 
		{
			Headers = headers;
		}

		internal static LogicalCallContext CreateLogicalCallContext (bool createEmpty)
		{
			LogicalCallContext ctx = null;
			if (logicalDatastore != null) {
				ctx = new LogicalCallContext ();
				foreach (DictionaryEntry entry in logicalDatastore) {
					ctx.SetData ((string)entry.Key, entry.Value);
				}
			}

			if (ctx == null && createEmpty)
				return new LogicalCallContext ();
			else
				return ctx;
		}

		internal static object SetCurrentCallContext (LogicalCallContext ctx)
		{
			object oldData = new object[] { datastore, logicalDatastore };

			if (ctx != null && ctx.HasInfo)
				logicalDatastore = (Hashtable) ctx.Datastore.Clone ();
			else
				logicalDatastore = null;
				
			return oldData;
		}
		
		internal static void UpdateCurrentLogicalCallContext (LogicalCallContext ctx)
		{
			Hashtable data = ctx.Datastore;
			foreach (DictionaryEntry entry in data)
				LogicalSetData ((string)entry.Key, entry.Value);
		}
		
		internal static void RestoreCallContext (object oldContext)
		{
			object[] contextArray = (object[])oldContext;
			datastore = (Hashtable)contextArray [0];
			logicalDatastore = (Hashtable)contextArray [1];
		}

		private static Hashtable Datastore
		{
			get {
				Hashtable r = datastore;
				if (r == null)
					return datastore = new Hashtable ();
				return r;
			}
		}

		private static Hashtable LogicalDatastore
		{
			get {
				Hashtable r = logicalDatastore;
				if (r == null)
					return logicalDatastore = new Hashtable ();
				return r;
			}
		}
	}

	[System.Runtime.InteropServices.ComVisible (true)]
	public interface ILogicalThreadAffinative
	{
	}
}
