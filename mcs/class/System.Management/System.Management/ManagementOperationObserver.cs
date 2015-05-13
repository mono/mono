//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
using System.Collections;

namespace System.Management
{
	public class ManagementOperationObserver
	{
		private Hashtable m_sinkCollection;

		private WmiDelegateInvoker delegateInvoker;

		internal bool HaveListenersForProgress
		{
			get
			{
				bool length = false;
				try
				{
					if (this.Progress != null)
					{
						length = (int)this.Progress.GetInvocationList().Length > 0;
					}
				}
				catch
				{
				}
				return length;
			}
		}

		public ManagementOperationObserver()
		{
			this.m_sinkCollection = new Hashtable();
			this.delegateInvoker = new WmiDelegateInvoker(this);
		}

		public void Cancel()
		{
			Hashtable hashtables = new Hashtable();
			lock (this.m_sinkCollection)
			{
				IDictionaryEnumerator enumerator = this.m_sinkCollection.GetEnumerator();
				try
				{
					enumerator.Reset();
					while (enumerator.MoveNext())
					{
						DictionaryEntry current = (DictionaryEntry)enumerator.Current;
						hashtables.Add(current.Key, current.Value);
					}
				}
				catch
				{
				}
			}
			try
			{
				IDictionaryEnumerator dictionaryEnumerator = hashtables.GetEnumerator();
				dictionaryEnumerator.Reset();
				while (dictionaryEnumerator.MoveNext())
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)dictionaryEnumerator.Current;
					WmiEventSink value = (WmiEventSink)dictionaryEntry.Value;
					try
					{
						value.Cancel();
					}
					catch
					{
					}
				}
			}
			catch
			{
			}
		}

		internal void FireCompleted(CompletedEventArgs args)
		{
			try
			{
				this.delegateInvoker.FireEventToDelegates(this.Completed, args);
			}
			catch
			{
			}
		}

		internal void FireObjectPut(ObjectPutEventArgs args)
		{
			try
			{
				this.delegateInvoker.FireEventToDelegates(this.ObjectPut, args);
			}
			catch
			{
			}
		}

		internal void FireObjectReady(ObjectReadyEventArgs args)
		{
			try
			{
				this.delegateInvoker.FireEventToDelegates(this.ObjectReady, args);
			}
			catch
			{
			}
		}

		internal void FireProgress(ProgressEventArgs args)
		{
			try
			{
				this.delegateInvoker.FireEventToDelegates(this.Progress, args);
			}
			catch
			{
			}
		}

		internal WmiGetEventSink GetNewGetSink(ManagementScope scope, object context, ManagementObject managementObject)
		{
			WmiGetEventSink wmiGetEventSink;
			try
			{
				WmiGetEventSink wmiGetEventSink1 = WmiGetEventSink.GetWmiGetEventSink(this, context, scope, managementObject);
				lock (this.m_sinkCollection)
				{
					this.m_sinkCollection.Add(wmiGetEventSink1.GetHashCode(), wmiGetEventSink1);
				}
				wmiGetEventSink = wmiGetEventSink1;
			}
			catch
			{
				wmiGetEventSink = null;
			}
			return wmiGetEventSink;
		}

		internal WmiEventSink GetNewPutSink(ManagementScope scope, object context, string path, string className)
		{
			WmiEventSink wmiEventSink;
			try
			{
				WmiEventSink wmiEventSink1 = WmiEventSink.GetWmiEventSink(this, context, scope, path, className);
				lock (this.m_sinkCollection)
				{
					this.m_sinkCollection.Add(wmiEventSink1.GetHashCode(), wmiEventSink1);
				}
				wmiEventSink = wmiEventSink1;
			}
			catch
			{
				wmiEventSink = null;
			}
			return wmiEventSink;
		}

		internal WmiEventSink GetNewSink(ManagementScope scope, object context)
		{
			WmiEventSink wmiEventSink;
			try
			{
				WmiEventSink wmiEventSink1 = WmiEventSink.GetWmiEventSink(this, context, scope, null, null);
				lock (this.m_sinkCollection)
				{
					this.m_sinkCollection.Add(wmiEventSink1.GetHashCode(), wmiEventSink1);
				}
				wmiEventSink = wmiEventSink1;
			}
			catch
			{
				wmiEventSink = null;
			}
			return wmiEventSink;
		}

		internal void RemoveSink(WmiEventSink eventSink)
		{
			try
			{
				lock (this.m_sinkCollection)
				{
					this.m_sinkCollection.Remove(eventSink.GetHashCode());
				}
				eventSink.ReleaseStub();
			}
			catch
			{
			}
		}

		public event CompletedEventHandler Completed;
		public event ObjectPutEventHandler ObjectPut;
		public event ObjectReadyEventHandler ObjectReady;
		public event ProgressEventHandler Progress;
	}
}