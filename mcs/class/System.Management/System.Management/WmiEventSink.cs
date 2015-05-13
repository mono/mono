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
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Management
{
	internal class WmiEventSink : IWmiEventSource
	{
		private static int s_hash;

		private int hash;

		private ManagementOperationObserver watcher;

		private object context;

		private ManagementScope scope;

		private object stub;

		private ManagementPath path;

		private string className;

		private bool isLocal;

		private static ManagementOperationObserver watcherParameter;

		private static object contextParameter;

		private static ManagementScope scopeParameter;

		private static string pathParameter;

		private static string classNameParameter;

		private static WmiEventSink wmiEventSinkNew;

		public IWbemObjectSink Stub
		{
			get
			{
				IWbemObjectSink wbemObjectSink;
				IWbemObjectSink wbemObjectSink1;
				try
				{
					if (this.stub != null)
					{
						wbemObjectSink1 = (IWbemObjectSink)this.stub;
					}
					else
					{
						wbemObjectSink1 = null;
					}
					wbemObjectSink = wbemObjectSink1;
				}
				catch
				{
					wbemObjectSink = null;
				}
				return wbemObjectSink;
			}
		}

		static WmiEventSink()
		{
			WmiEventSink.s_hash = 0;
		}

		protected WmiEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, string path, string className)
		{
			try
			{
				this.context = context;
				this.watcher = watcher;
				this.className = className;
				this.isLocal = false;
				if (path != null)
				{
					this.path = new ManagementPath(path);
					if (string.Compare(this.path.Server, ".", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(this.path.Server, Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						this.isLocal = true;
					}
				}
				if (scope != null)
				{
					this.scope = scope.Clone();
					if (path == null && (string.Compare(this.scope.Path.Server, ".", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(this.scope.Path.Server, Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0))
					{
						this.isLocal = true;
					}
				}
				WmiNetUtilsHelper.GetDemultiplexedStub_f(this, this.isLocal, out this.stub);
				this.hash = Interlocked.Increment(ref WmiEventSink.s_hash);
			}
			catch
			{
			}
		}

		internal void Cancel()
		{
			try
			{
				this.scope.GetIWbemServices().CancelAsyncCall_((IWbemObjectSink)this.stub);
			}
			catch
			{
			}
		}

		public override int GetHashCode()
		{
			return this.hash;
		}

		internal static WmiEventSink GetWmiEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, string path, string className)
		{
			if (!MTAHelper.IsNoContextMTA())
			{
				WmiEventSink.watcherParameter = watcher;
				WmiEventSink.contextParameter = context;
				WmiEventSink.scopeParameter = scope;
				WmiEventSink.pathParameter = path;
				WmiEventSink.classNameParameter = className;
				ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethod(WmiEventSink.HackToCreateWmiEventSink));
				threadDispatch.Start();
				return WmiEventSink.wmiEventSinkNew;
			}
			else
			{
				return new WmiEventSink(watcher, context, scope, path, className);
			}
		}

		private static void HackToCreateWmiEventSink()
		{
			WmiEventSink.wmiEventSinkNew = new WmiEventSink(WmiEventSink.watcherParameter, WmiEventSink.contextParameter, WmiEventSink.scopeParameter, WmiEventSink.pathParameter, WmiEventSink.classNameParameter);
		}

		public virtual void Indicate(IntPtr pIWbemClassObject)
		{
			Marshal.AddRef(pIWbemClassObject);
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = new IWbemClassObjectFreeThreaded(pIWbemClassObject);
			try
			{
				ObjectReadyEventArgs objectReadyEventArg = new ObjectReadyEventArgs(this.context, ManagementBaseObject.GetBaseObject(wbemClassObjectFreeThreaded, this.scope));
				this.watcher.FireObjectReady(objectReadyEventArg);
			}
			catch
			{
			}
		}

		internal void ReleaseStub()
		{
			try
			{
				if (this.stub != null)
				{
					Marshal.ReleaseComObject(this.stub);
					this.stub = null;
				}
			}
			catch
			{
			}
		}

		public void SetStatus(int flags, int hResult, string message, IntPtr pErrorObj)
		{
			CompletedEventArgs completedEventArg;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			if (pErrorObj != IntPtr.Zero)
			{
				Marshal.AddRef(pErrorObj);
				wbemClassObjectFreeThreaded = new IWbemClassObjectFreeThreaded(pErrorObj);
			}
			try
			{
				if (flags != 0)
				{
					if ((flags & 2) != 0)
					{
						ProgressEventArgs progressEventArg = new ProgressEventArgs(this.context, (hResult & -65536) >> 16, hResult & 0xffff, message);
						this.watcher.FireProgress(progressEventArg);
					}
				}
				else
				{
					if (this.path != null)
					{
						if (this.className != null)
						{
							this.path.RelativePath = this.className;
						}
						else
						{
							this.path.RelativePath = message;
						}
						if (this.InternalObjectPut != null)
						{
							try
							{
								InternalObjectPutEventArgs internalObjectPutEventArg = new InternalObjectPutEventArgs(this.path);
								this.InternalObjectPut(this, internalObjectPutEventArg);
							}
							catch
							{
							}
						}
						ObjectPutEventArgs objectPutEventArg = new ObjectPutEventArgs(this.context, this.path);
						this.watcher.FireObjectPut(objectPutEventArg);
					}
					if (wbemClassObjectFreeThreaded == null)
					{
						completedEventArg = new CompletedEventArgs(this.context, hResult, null);
					}
					else
					{
						completedEventArg = new CompletedEventArgs(this.context, hResult, new ManagementBaseObject(wbemClassObjectFreeThreaded));
					}
					this.watcher.FireCompleted(completedEventArg);
					this.watcher.RemoveSink(this);
				}
			}
			catch
			{
			}
		}

		internal event InternalObjectPutEventHandler InternalObjectPut;
	}
}