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

namespace System.Management
{
	internal class WmiGetEventSink : WmiEventSink
	{
		private ManagementObject managementObject;

		private static ManagementOperationObserver watcherParameter;

		private static object contextParameter;

		private static ManagementScope scopeParameter;

		private static ManagementObject managementObjectParameter;

		private static WmiGetEventSink wmiGetEventSinkNew;

		private WmiGetEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, ManagementObject managementObject) : base(watcher, context, scope, null, null)
		{
			this.managementObject = managementObject;
		}

		internal static WmiGetEventSink GetWmiGetEventSink(ManagementOperationObserver watcher, object context, ManagementScope scope, ManagementObject managementObject)
		{
			if (!MTAHelper.IsNoContextMTA())
			{
				WmiGetEventSink.watcherParameter = watcher;
				WmiGetEventSink.contextParameter = context;
				WmiGetEventSink.scopeParameter = scope;
				WmiGetEventSink.managementObjectParameter = managementObject;
				ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethod(WmiGetEventSink.HackToCreateWmiGetEventSink));
				threadDispatch.Start();
				return WmiGetEventSink.wmiGetEventSinkNew;
			}
			else
			{
				return new WmiGetEventSink(watcher, context, scope, managementObject);
			}
		}

		private static void HackToCreateWmiGetEventSink()
		{
			WmiGetEventSink.wmiGetEventSinkNew = new WmiGetEventSink(WmiGetEventSink.watcherParameter, WmiGetEventSink.contextParameter, WmiGetEventSink.scopeParameter, WmiGetEventSink.managementObjectParameter);
		}

		public override void Indicate(IntPtr pIWbemClassObject)
		{
			Marshal.AddRef(pIWbemClassObject);
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = new IWbemClassObjectFreeThreaded(pIWbemClassObject);
			if (this.managementObject != null)
			{
				try
				{
					this.managementObject.wbemObject = wbemClassObjectFreeThreaded;
				}
				catch
				{
				}
			}
		}
	}
}