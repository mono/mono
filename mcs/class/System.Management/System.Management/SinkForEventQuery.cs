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
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Management
{
	internal class SinkForEventQuery : IWmiEventSource
	{
		private ManagementEventWatcher eventWatcher;

		private object context;

		private IWbemServices services;

		private IWbemObjectSink stub;

		private int status;

		private bool isLocal;

		public int Status
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.status;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.status = value;
			}
		}

		internal IWbemObjectSink Stub
		{
			get
			{
				return this.stub;
			}
		}

		public SinkForEventQuery(ManagementEventWatcher eventWatcher, object context, IWbemServices services)
		{
			this.services = services;
			this.context = context;
			this.eventWatcher = eventWatcher;
			this.status = 0;
			this.isLocal = false;
			if (string.Compare(eventWatcher.Scope.Path.Server, ".", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(eventWatcher.Scope.Path.Server, Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				this.isLocal = true;
			}
			if (!MTAHelper.IsNoContextMTA())
			{
				ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.HackToCreateStubInMTA));
				threadDispatch.Parameter = this;
				threadDispatch.Start();
				return;
			}
			else
			{
				this.HackToCreateStubInMTA(this);
				return;
			}
		}

		internal void Cancel()
		{
			if (this.stub != null)
			{
				lock (this)
				{
					if (this.stub != null)
					{
						int num = this.services.CancelAsyncCall_(this.stub);
						this.ReleaseStub();
						if (num < 0)
						{
							if (((long)num & (long)-4096) != (long)-2147217408)
							{
								Marshal.ThrowExceptionForHR(num);
							}
							else
							{
								ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
							}
						}
					}
				}
			}
		}

		private void Cancel2(object o)
		{
			try
			{
				this.Cancel();
			}
			catch
			{
			}
		}

		private void HackToCreateStubInMTA(object param)
		{
			SinkForEventQuery getDemultiplexedStubF = (SinkForEventQuery)param;
			object obj = null;
			getDemultiplexedStubF.Status = WmiNetUtilsHelper.GetDemultiplexedStub_f(getDemultiplexedStubF, getDemultiplexedStubF.isLocal, out obj);
			getDemultiplexedStubF.stub = (IWbemObjectSink)obj;
		}

		public void Indicate(IntPtr pWbemClassObject)
		{
			Marshal.AddRef(pWbemClassObject);
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = new IWbemClassObjectFreeThreaded(pWbemClassObject);
			try
			{
				EventArrivedEventArgs eventArrivedEventArg = new EventArrivedEventArgs(this.context, new ManagementBaseObject(wbemClassObjectFreeThreaded));
				this.eventWatcher.FireEventArrived(eventArrivedEventArg);
			}
			catch
			{
			}
		}

		internal void ReleaseStub()
		{
			if (this.stub != null)
			{
				lock (this)
				{
					if (this.stub != null)
					{
						try
						{
							Marshal.ReleaseComObject(this.stub);
							this.stub = null;
						}
						catch
						{
						}
					}
				}
			}
		}

		public void SetStatus(int flags, int hResult, string message, IntPtr pErrObj)
		{
			try
			{
				this.eventWatcher.FireStopped(new StoppedEventArgs(this.context, hResult));
				if (hResult != -2147217358 && hResult != 0x40006)
				{
					ThreadPool.QueueUserWorkItem(new WaitCallback(this.Cancel2));
				}
			}
			catch
			{
			}
		}
	}
}