//
// AssemblyRef
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
using System.Globalization;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace System.Management.Instrumentation
{
	internal sealed class EventSource : IWbemProviderInit, IWbemEventProvider, IWbemEventProviderQuerySink, IWbemEventProviderSecurity, IWbemServices_Old
	{
		private IWbemDecoupledRegistrar registrar;

		private static ArrayList eventSources;

		private InstrumentedAssembly instrumentedAssembly;

		private static int shutdownInProgress;

		private static ReaderWriterLock preventShutdownLock;

		private IWbemServices pNamespaceNA;

		private IWbemObjectSink pSinkNA;

		private IWbemServices pNamespaceMTA;

		private IWbemObjectSink pSinkMTA;

		private ArrayList reqList;

		private object critSec;

		private AutoResetEvent doIndicate;

		private bool workerThreadInitialized;

		private bool alive;

		private Hashtable mapQueryIdToQuery;

		static EventSource()
		{
			EventSource.eventSources = new ArrayList();
			EventSource.shutdownInProgress = 0;
			EventSource.preventShutdownLock = new ReaderWriterLock();
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(EventSource.ProcessExit);
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(EventSource.ProcessExit);
		}

		public EventSource(string namespaceName, string appName, InstrumentedAssembly instrumentedAssembly)
		{
			this.registrar = (IWbemDecoupledRegistrar)(new WbemDecoupledRegistrar());
			this.reqList = new ArrayList(3);
			this.critSec = new object();
			this.doIndicate = new AutoResetEvent(false);
			this.alive = true;
			this.mapQueryIdToQuery = new Hashtable();
			lock (EventSource.eventSources)
			{
				if (EventSource.shutdownInProgress == 0)
				{
					this.instrumentedAssembly = instrumentedAssembly;
					int num = this.registrar.Register_(0, null, null, null, namespaceName, appName, this);
					if (num != 0)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					EventSource.eventSources.Add(this);
				}
			}
		}

		public bool Any()
		{
			if (this.pSinkMTA == null)
			{
				return true;
			}
			else
			{
				return this.mapQueryIdToQuery.Count == 0;
			}
		}

		~EventSource()
		{
			try
			{
				this.UnRegister();
			}
			finally
			{
				//this.Finalize();
			}
		}

		public void IndicateEvents(int length, IntPtr[] objects)
		{
			if (this.pSinkMTA != null)
			{
				if (!MTAHelper.IsNoContextMTA())
				{
					EventSource.MTARequest mTARequest = new EventSource.MTARequest(length, objects);
					lock (this.critSec)
					{
						if (!this.workerThreadInitialized)
						{
							Thread thread = new Thread(new ThreadStart(this.MTAWorkerThread2));
							thread.IsBackground = true;
							thread.SetApartmentState(ApartmentState.MTA);
							thread.Start();
							this.workerThreadInitialized = true;
						}
						int num = this.reqList.Add(mTARequest);
						if (!this.doIndicate.Set())
						{
							this.reqList.RemoveAt(num);
							throw new ManagementException(RC.GetString("WORKER_THREAD_WAKEUP_FAILED"));
						}
					}
					mTARequest.doneIndicate.WaitOne();
					if (mTARequest.exception != null)
					{
						throw mTARequest.exception;
					}
				}
				else
				{
					int num1 = this.pSinkMTA.Indicate_(length, objects);
					if (num1 < 0)
					{
						if (((long)num1 & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num1);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
						}
					}
				}
				GC.KeepAlive(this);
				return;
			}
			else
			{
				return;
			}
		}

		public void MTAWorkerThread2()
		{
		Label0:
			this.doIndicate.WaitOne();
			if (this.alive)
			{
				while (true)
				{
					EventSource.MTARequest item = null;
					lock (this.critSec)
					{
						if (this.reqList.Count <= 0)
						{
							goto Label0;
						}
						else
						{
							item = (EventSource.MTARequest)this.reqList[0];
							this.reqList.RemoveAt(0);
						}
					}
					try
					{
						try
						{
							if (this.pSinkMTA != null)
							{
								int num = this.pSinkMTA.Indicate_(item.lengthFromSTA, item.objectsFromSTA);
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
						catch (Exception exception1)
						{
							Exception exception = exception1;
							item.exception = exception;
						}
					}
					finally
					{
						item.doneIndicate.Set();
						GC.KeepAlive(this);
					}
				}
			}
			else
			{
				return;
			}
		}

		private static void ProcessExit(object o, EventArgs args)
		{
			if (EventSource.shutdownInProgress == 0)
			{
				Interlocked.Increment(ref EventSource.shutdownInProgress);
				try
				{
					EventSource.preventShutdownLock.AcquireWriterLock(-1);
					lock (EventSource.eventSources)
					{
						foreach (EventSource eventSource in EventSource.eventSources)
						{
							eventSource.UnRegister();
						}
					}
				}
				finally
				{
					EventSource.preventShutdownLock.ReleaseWriterLock();
					Thread.Sleep(50);
					EventSource.preventShutdownLock.AcquireWriterLock(-1);
					EventSource.preventShutdownLock.ReleaseWriterLock();
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void RelocateNamespaceRCWToMTA()
		{
			ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.RelocateNamespaceRCWToMTA_ThreadFuncion));
			threadDispatch.Parameter = this;
			threadDispatch.Start();
		}

		private void RelocateNamespaceRCWToMTA_ThreadFuncion(object param)
		{
			EventSource currentApartment = (EventSource)param;
			currentApartment.pNamespaceMTA = (IWbemServices)EventSource.RelocateRCWToCurrentApartment(currentApartment.pNamespaceNA);
			currentApartment.pNamespaceNA = null;
		}

		private static object RelocateRCWToCurrentApartment(object comObject)
		{
			if (comObject != null)
			{
				IntPtr unknownForObject = Marshal.GetIUnknownForObject(comObject);
				int num = Marshal.ReleaseComObject(comObject);
				if (num == 0)
				{
					comObject = Marshal.GetObjectForIUnknown(unknownForObject);
					Marshal.Release(unknownForObject);
					return comObject;
				}
				else
				{
					throw new Exception();
				}
			}
			else
			{
				return null;
			}
		}

		private void RelocateSinkRCWToMTA()
		{
			ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.RelocateSinkRCWToMTA_ThreadFuncion));
			threadDispatch.Parameter = this;
			threadDispatch.Start();
		}

		private void RelocateSinkRCWToMTA_ThreadFuncion(object param)
		{
			EventSource currentApartment = (EventSource)param;
			currentApartment.pSinkMTA = (IWbemObjectSink)EventSource.RelocateRCWToCurrentApartment(currentApartment.pSinkNA);
			currentApartment.pSinkNA = null;
		}

		int System.Management.IWbemEventProvider.ProvideEvents_(IWbemObjectSink pSink, int lFlags)
		{
			this.pSinkNA = pSink;
			this.RelocateSinkRCWToMTA();
			return 0;
		}

		int System.Management.IWbemEventProviderQuerySink.CancelQuery_(uint dwId)
		{
			lock (this.mapQueryIdToQuery)
			{
				this.mapQueryIdToQuery.Remove(dwId);
			}
			return 0;
		}

		int System.Management.IWbemEventProviderQuerySink.NewQuery_(uint dwId, string wszQueryLanguage, string wszQuery)
		{
			lock (this.mapQueryIdToQuery)
			{
				if (this.mapQueryIdToQuery.ContainsKey(dwId))
				{
					this.mapQueryIdToQuery.Remove(dwId);
				}
				this.mapQueryIdToQuery.Add(dwId, wszQuery);
			}
			return 0;
		}

		int System.Management.IWbemEventProviderSecurity.AccessCheck_(string wszQueryLanguage, string wszQuery, int lSidLength, ref byte pSid)
		{
			return 0;
		}

		int System.Management.IWbemProviderInit.Initialize_(string wszUser, int lFlags, string wszNamespace, string wszLocale, IWbemServices pNamespace, IWbemContext pCtx, IWbemProviderInitSink pInitSink)
		{
			this.pNamespaceNA = pNamespace;
			this.RelocateNamespaceRCWToMTA();
			this.pSinkNA = null;
			this.pSinkMTA = null;
			lock (this.mapQueryIdToQuery)
			{
				this.mapQueryIdToQuery.Clear();
			}
			pInitSink.SetStatus_(0, 0);
			Marshal.ReleaseComObject(pInitSink);
			return 0;
		}

		int System.Management.IWbemServices_Old.CancelAsyncCall_(IWbemObjectSink pSink)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.CreateClassEnum_(string strSuperclass, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = null;
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.CreateClassEnumAsync_(string strSuperclass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.CreateInstanceEnum_(string strFilter, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = null;
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.CreateInstanceEnumAsync_(string strFilter, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			Type type = null;
			int num;
			try
			{
				EventSource.preventShutdownLock.AcquireReaderLock(-1);
				if (EventSource.shutdownInProgress == 0)
				{
					int tickCount = Environment.TickCount + 100;
					foreach (Type key in this.instrumentedAssembly.mapTypeToConverter.Keys)
					{
						if (string.Compare(ManagedNameAttribute.GetMemberName(key), strFilter, StringComparison.Ordinal) != 0)
						{
							continue;
						}
						type = key;
						break;
					}
					if (null != type)
					{
						int num1 = 64;
						IntPtr[] value = new IntPtr[num1];
						IntPtr[] intPtrArray = new IntPtr[num1];
						ConvertToWMI[] convertToWMIArray = new ConvertToWMI[num1];
						IWbemClassObjectFreeThreaded[] wbemClassObjectFreeThreaded = new IWbemClassObjectFreeThreaded[num1];
						int num2 = 0;
						int V_8 = 0;
						object processIdentity = Instrumentation.ProcessIdentity;
						try
						{
							InstrumentedAssembly.readerWriterLock.AcquireReaderLock(-1);
							foreach (DictionaryEntry dictionaryEntry in InstrumentedAssembly.mapIDToPublishedObject)
							{
								if (EventSource.shutdownInProgress == 0)
								{
									if (type != dictionaryEntry.Value.GetType())
									{
										continue;
									}
									if (convertToWMIArray[num2] != null)
									{
										lock (dictionaryEntry.Value)
										{
											convertToWMIArray[num2](dictionaryEntry.Value);
										}
										value[num2] = (IntPtr)convertToWMIArray[num2].Target.GetType().GetField("instWbemObjectAccessIP").GetValue(convertToWMIArray[num2].Target);
										Marshal.AddRef(value[num2]);
										wbemClassObjectFreeThreaded[num2] = new IWbemClassObjectFreeThreaded(value[num2]);
										wbemClassObjectFreeThreaded[num2].Put_("ProcessId", 0, ref processIdentity, 0);
										if (num2 == 0)
										{
											int V_15;
											WmiNetUtilsHelper.GetPropertyHandle_f27(27, wbemClassObjectFreeThreaded[num2], "InstanceId", out V_15, out V_8);
										}
									}
									else
									{
										object obj = Activator.CreateInstance((Type)this.instrumentedAssembly.mapTypeToConverter[type]);
										convertToWMIArray[num2] = (ConvertToWMI)Delegate.CreateDelegate(typeof(ConvertToWMI), obj, "ToWMI");
										lock (dictionaryEntry.Value)
										{
											convertToWMIArray[num2](dictionaryEntry.Value);
										}
										value[num2] = (IntPtr)obj.GetType().GetField("instWbemObjectAccessIP").GetValue(obj);
										Marshal.AddRef(value[num2]);
										wbemClassObjectFreeThreaded[num2] = new IWbemClassObjectFreeThreaded(value[num2]);
										wbemClassObjectFreeThreaded[num2].Put_("ProcessId", 0, ref processIdentity, 0);
										if (num2 == 0)
										{
											int V_13;
											WmiNetUtilsHelper.GetPropertyHandle_f27(27, wbemClassObjectFreeThreaded[num2], "InstanceId", out V_13, out V_8);
										}
									}
									string str = (string)dictionaryEntry.Key;
									WmiNetUtilsHelper.WritePropertyValue_f28(28, wbemClassObjectFreeThreaded[num2], V_8, (str.Length + 1) * 2, str);
									num2++;
									if (num2 != num1 && Environment.TickCount < tickCount)
									{
										continue;
									}
									for (int i = 0; i < num2; i++)
									{
										WmiNetUtilsHelper.Clone_f(12, value[i], out intPtrArray[i]);
									}
									int num3 = pResponseHandler.Indicate_(num2, intPtrArray);
									for (int j = 0; j < num2; j++)
									{
										Marshal.Release(intPtrArray[j]);
									}
									if (num3 == 0)
									{
										num2 = 0;
										tickCount = Environment.TickCount + 100;
									}
									else
									{
										num = 0;
										return num;
									}
								}
								else
								{
									num = 0;
									return num;
								}
							}
						}
						finally
						{
							InstrumentedAssembly.readerWriterLock.ReleaseReaderLock();
						}
						if (num2 > 0)
						{
							for (int k = 0; k < num2; k++)
							{
								WmiNetUtilsHelper.Clone_f(12, value[k], out intPtrArray[k]);
							}
							pResponseHandler.Indicate_(num2, intPtrArray);
							for (int l = 0; l < num2; l++)
							{
								Marshal.Release(intPtrArray[l]);
							}
						}
						return 0;
					}
					else
					{
						num = 0;
					}
				}
				else
				{
					num = 0;
				}
			}
			finally
			{
				pResponseHandler.SetStatus_(0, 0, null, IntPtr.Zero);
				Marshal.ReleaseComObject(pResponseHandler);
				EventSource.preventShutdownLock.ReleaseReaderLock();
			}
			return num;
		}

		int System.Management.IWbemServices_Old.DeleteClass_(string strClass, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.DeleteClassAsync_(string strClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.DeleteInstance_(string strObjectPath, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.DeleteInstanceAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.ExecMethod_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObject_DoNotMarshal pInParams, out IWbemClassObject_DoNotMarshal ppOutParams, IntPtr ppCallResult)
		{
			ppOutParams = null;
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.ExecMethodAsync_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObject_DoNotMarshal pInParams, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.ExecNotificationQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = null;
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.ExecNotificationQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.ExecQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = null;
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.ExecQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.GetObject_(string strObjectPath, int lFlags, IWbemContext pCtx, out IWbemClassObject_DoNotMarshal ppObject, IntPtr ppCallResult)
		{
			ppObject = null;
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.GetObjectAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			Match match = Regex.Match(strObjectPath.ToLower(CultureInfo.InvariantCulture), "(.*?)\\.instanceid=\"(.*?)\",processid=\"(.*?)\"");
			if (match.Success)
			{
				//match.Groups[1].Value;
				string value = match.Groups[2].Value;
				string str = match.Groups[3].Value;
				if (Instrumentation.ProcessIdentity == str)
				{
					int num = ((IConvertible)value).ToInt32((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int)));
					object item = null;
					try
					{
						InstrumentedAssembly.readerWriterLock.AcquireReaderLock(-1);
						item = InstrumentedAssembly.mapIDToPublishedObject[num.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int)))];
					}
					finally
					{
						InstrumentedAssembly.readerWriterLock.ReleaseReaderLock();
					}
					if (item != null)
					{
						Type type = (Type)this.instrumentedAssembly.mapTypeToConverter[item.GetType()];
						if (type != null)
						{
							object obj = Activator.CreateInstance(type);
							ConvertToWMI convertToWMI = (ConvertToWMI)Delegate.CreateDelegate(typeof(ConvertToWMI), obj, "ToWMI");
							lock (item)
							{
								convertToWMI(item);
							}
							IntPtr[] intPtrArray = new IntPtr[1];
							intPtrArray[0] = (IntPtr)obj.GetType().GetField("instWbemObjectAccessIP").GetValue(obj);
							IntPtr[] intPtrArray1 = intPtrArray;
							Marshal.AddRef(intPtrArray1[0]);
							IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = new IWbemClassObjectFreeThreaded(intPtrArray1[0]);
							object processIdentity = num;
							wbemClassObjectFreeThreaded.Put_("InstanceId", 0, ref processIdentity, 0);
							processIdentity = Instrumentation.ProcessIdentity;
							wbemClassObjectFreeThreaded.Put_("ProcessId", 0, ref processIdentity, 0);
							pResponseHandler.Indicate_(1, intPtrArray1);
							pResponseHandler.SetStatus_(0, 0, null, IntPtr.Zero);
							Marshal.ReleaseComObject(pResponseHandler);
							return 0;
						}
					}
					pResponseHandler.SetStatus_(0, -2147217406, null, IntPtr.Zero);
					Marshal.ReleaseComObject(pResponseHandler);
					return -2147217406;
				}
				else
				{
					pResponseHandler.SetStatus_(0, -2147217406, null, IntPtr.Zero);
					Marshal.ReleaseComObject(pResponseHandler);
					return -2147217406;
				}
			}
			else
			{
				pResponseHandler.SetStatus_(0, -2147217406, null, IntPtr.Zero);
				Marshal.ReleaseComObject(pResponseHandler);
				return -2147217406;
			}
		}

		int System.Management.IWbemServices_Old.OpenNamespace_(string strNamespace, int lFlags, IWbemContext pCtx, out IWbemServices ppWorkingNamespace, IntPtr ppCallResult)
		{
			ppWorkingNamespace  = null;
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.PutClass_(IWbemClassObject_DoNotMarshal pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.PutClassAsync_(IWbemClassObject_DoNotMarshal pObject, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.PutInstance_(IWbemClassObject_DoNotMarshal pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.PutInstanceAsync_(IWbemClassObject_DoNotMarshal pInst, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return -2147217396;
		}

		int System.Management.IWbemServices_Old.QueryObjectSink_(int lFlags, out IWbemObjectSink ppResponseHandler)
		{
			ppResponseHandler = null;
			return -2147217396;
		}

		private void UnRegister()
		{
			lock (this)
			{
				if (this.registrar != null)
				{
					if (this.workerThreadInitialized)
					{
						this.alive = false;
						this.doIndicate.Set();
						GC.KeepAlive(this);
						this.workerThreadInitialized = false;
					}
					this.registrar.UnRegister_();
					this.registrar = null;
				}
			}
		}

		private class MTARequest
		{
			public AutoResetEvent doneIndicate;

			public Exception exception;

			public int lengthFromSTA;

			public IntPtr[] objectsFromSTA;

			public MTARequest(int length, IntPtr[] objects)
			{
				this.doneIndicate = new AutoResetEvent(false);
				this.lengthFromSTA = -1;
				this.lengthFromSTA = length;
				this.objectsFromSTA = objects;
			}
		}
	}
}