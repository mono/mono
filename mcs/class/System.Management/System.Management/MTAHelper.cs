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
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Management
{
	internal class MTAHelper
	{
		private static ArrayList reqList;

		private static object critSec;

		private static AutoResetEvent evtGo;

		private static bool workerThreadInitialized;

		private static Guid IID_IObjectContext;

		private static Guid IID_IComThreadingInfo;

		private static bool CanCallCoGetObjectContext;

		static MTAHelper()
		{
			MTAHelper.reqList = new ArrayList(3);
			MTAHelper.critSec = new object();
			MTAHelper.evtGo = new AutoResetEvent(false);
			MTAHelper.workerThreadInitialized = false;
			MTAHelper.IID_IObjectContext = new Guid("51372AE0-CAE7-11CF-BE81-00AA00A2FA25");
			MTAHelper.IID_IComThreadingInfo = new Guid("000001ce-0000-0000-C000-000000000046");
			MTAHelper.CanCallCoGetObjectContext = MTAHelper.IsWindows2000OrHigher();
		}

		public MTAHelper()
		{
		}

		/*
		[DllImport("ole32.dll", CharSet=CharSet.None)]
		[SuppressUnmanagedCodeSecurity]
		private static extern int CoGetObjectContext(ref Guid riid, out IntPtr pUnk);
		*/

		private static int CoGetObjectContext (ref Guid riid, out IntPtr pUnk)
		{
			pUnk = IntPtr.Zero;
			return 0;
		}


		public static object CreateInMTA(Type type)
		{
			if (!MTAHelper.IsNoContextMTA())
			{
				MTAHelper.MTARequest mTARequest = new MTAHelper.MTARequest(type);
				lock (MTAHelper.critSec)
				{
					if (!MTAHelper.workerThreadInitialized)
					{
						MTAHelper.InitWorkerThread();
						MTAHelper.workerThreadInitialized = true;
					}
					int num = MTAHelper.reqList.Add(mTARequest);
					if (!MTAHelper.evtGo.Set())
					{
						MTAHelper.reqList.RemoveAt(num);
						throw new ManagementException(RC.GetString("WORKER_THREAD_WAKEUP_FAILED"));
					}
				}
				mTARequest.evtDone.WaitOne();
				if (mTARequest.exception == null)
				{
					return mTARequest.createdObject;
				}
				else
				{
					throw mTARequest.exception;
				}
			}
			else
			{
				return Activator.CreateInstance(type);
			}
		}

		private static void InitWorkerThread()
		{
			Thread thread = new Thread(new ThreadStart(MTAHelper.WorkerThread));
			thread.SetApartmentState(ApartmentState.MTA);
			thread.IsBackground = true;
			thread.Start();
		}

		public static bool IsNoContextMTA()
		{
			WmiNetUtilsHelper.APTTYPE aPTTYPE = WmiNetUtilsHelper.APTTYPE.APTTYPE_STA;
			bool flag;
			if (Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA)
			{
				if (MTAHelper.CanCallCoGetObjectContext)
				{
					IntPtr zero = IntPtr.Zero;
					IntPtr intPtr = IntPtr.Zero;
					try
					{
						if (MTAHelper.CoGetObjectContext(ref MTAHelper.IID_IComThreadingInfo, out zero) == 0)
						{
							if (WmiNetUtilsHelper.GetCurrentApartmentType_f(3, zero, out aPTTYPE) == 0)
							{
								if (aPTTYPE == WmiNetUtilsHelper.APTTYPE.APTTYPE_MTA)
								{
									if (Marshal.QueryInterface(zero, ref MTAHelper.IID_IObjectContext, out intPtr) != 0)
									{
										return true;
									}
									else
									{
										flag = false;
									}
								}
								else
								{
									flag = false;
								}
							}
							else
							{
								flag = false;
							}
						}
						else
						{
							flag = false;
						}
					}
					finally
					{
						if (zero != IntPtr.Zero)
						{
							Marshal.Release(zero);
						}
						if (intPtr != IntPtr.Zero)
						{
							Marshal.Release(intPtr);
						}
					}
					return flag;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		private static bool IsWindows2000OrHigher()
		{
			OperatingSystem oSVersion = Environment.OSVersion;
			if (oSVersion.Platform != PlatformID.Win32NT || !(oSVersion.Version >= new Version(5, 0)))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		private static void WorkerThread()
		{
		Label0:
			MTAHelper.evtGo.WaitOne();
			while (true)
			{
				MTAHelper.MTARequest item = null;
				lock (MTAHelper.critSec)
				{
					if (MTAHelper.reqList.Count <= 0)
					{
						goto Label0;
					}
					else
					{
						item = (MTAHelper.MTARequest)MTAHelper.reqList[0];
						MTAHelper.reqList.RemoveAt(0);
					}
				}
				try
				{
					try
					{
						item.createdObject = Activator.CreateInstance(item.typeToCreate);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						item.exception = exception;
					}
				}
				finally
				{
					item.evtDone.Set();
				}
			}
		}

		private class MTARequest
		{
			public AutoResetEvent evtDone;

			public Type typeToCreate;

			public object createdObject;

			public Exception exception;

			public MTARequest(Type typeToCreate)
			{
				this.evtDone = new AutoResetEvent(false);
				this.typeToCreate = typeToCreate;
			}
		}
	}
}