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
		}

		public static object GetData (string name) 
		{
			return Datastore [name];
		}
		
		public static void SetData (string name, object data) 
		{
			Datastore [name] = data;
		}

		[MonoTODO]
		public static object LogicalGetData (string name) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static void LogicalSetData (string name, object data) 
		{
			throw new NotImplementedException ();
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
			if (datastore != null) {
				foreach (DictionaryEntry entry in datastore)
					if (entry.Value is ILogicalThreadAffinative) {
						if (ctx == null) ctx = new LogicalCallContext ();
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
			object oldData = datastore;

			if (ctx != null && ctx.HasInfo)
				datastore = (Hashtable) ctx.Datastore.Clone ();
			else
				datastore = null;
				
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
			datastore = (Hashtable) oldContext;
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
	}

	[System.Runtime.InteropServices.ComVisible (true)]
	public interface ILogicalThreadAffinative
	{
	}
}
