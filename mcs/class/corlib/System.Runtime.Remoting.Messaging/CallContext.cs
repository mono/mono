// 
// System.Runtime.Remoting.Messaging.CallContext.cs
//
// Authors: Jaime Anguiano Olarra (jaime@gnome.org)
//         Lluis Sanchez Gual (lluis@ximian.com)
//          Marek Safar (marek.safar@gmail.com)
//
// (c) 2002, Jaime Anguiano Olarra
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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
using System.Collections.Generic;

namespace System.Runtime.Remoting.Messaging 
{	
	[Serializable]
	[System.Runtime.InteropServices.ComVisible (true)]
	public sealed class CallContext 
	{
		[ThreadStatic] static Header [] Headers;
		[ThreadStatic] static object hostContext;
		
		private CallContext ()
		{
		}

		public static object HostContext {
			get { return hostContext; }
			set { hostContext = value; }
		}

		public static void FreeNamedDataSlot (string name)
		{
			ExecutionContext.FreeNamedDataSlot (name);
		}

		public static object GetData (string name) 
		{
			var value = LogicalGetData (name);
			if (value == null)
				Datastore.TryGetValue (name, out value);

			return value;
		}
		
		public static void SetData (string name, object data) 
		{
			if (data is ILogicalThreadAffinative) {
				LogicalSetData (name, data);
			} else {
				LogicalContext.FreeNamedDataSlot (name);
				Datastore [name] = data;
			}
		}
		
		public static object LogicalGetData (string name) 
		{
			return LogicalContext.GetData (name);
		}

		public static void LogicalSetData (string name, object data) 
		{
			Datastore.Remove (name);
			LogicalContext.SetData (name, data);
		}

		public static Header[] GetHeaders () 
		{
			return Headers;
		}
		
		public static void SetHeaders (Header[] headers) 
		{
			Headers = headers;
		}

		internal static object SetCurrentCallContext (LogicalCallContext ctx)
		{
			object oldData = new object[] { Datastore, LogicalContext.Datastore };

			Hashtable logicalDatastore;
			if (ctx != null && ctx.HasInfo)
				logicalDatastore = (Hashtable) ctx.Datastore.Clone ();
			else
				logicalDatastore = null;
				
			LogicalContext.Datastore = logicalDatastore;
			return oldData;
		}

		internal static void UpdateCurrentLogicalCallContext (LogicalCallContext ctx)
		{
			Hashtable data = ctx.Datastore;
			if (data == null)
				return;

			foreach (DictionaryEntry entry in data)
				LogicalSetData ((string)entry.Key, entry.Value);
		}
		
		internal static void RestoreCallContext (object oldContext)
		{
			object[] contextArray = (object[])oldContext;
			ExecutionContext.DataStore = (Dictionary<string, object>)contextArray [0];
			LogicalContext.Datastore = (Hashtable)contextArray [1];
		}

		static Dictionary<string, object> Datastore {
			get {
				return ExecutionContext.DataStore;
			}
		}

		static LogicalCallContext LogicalContext {
			get {
				return ExecutionContext.LogicalCallContext;
			}
		}

		static ExecutionContext ExecutionContext {
			get {
				return ExecutionContext.Current;
			}
		}
	}

	[System.Runtime.InteropServices.ComVisible (true)]
	public interface ILogicalThreadAffinative
	{
	}
}
