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
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Reflection;
using System.Threading;

namespace System.Management.Instrumentation
{
	internal class InstrumentedAssembly
	{
		private SchemaNaming naming;

		public EventSource source;

		public Hashtable mapTypeToConverter;

		public static ReaderWriterLock readerWriterLock;

		public static Hashtable mapIDToPublishedObject;

		private static Hashtable mapPublishedObjectToID;

		private static int upcountId;

		private Hashtable mapTypeToTypeInfo;

		private InstrumentedAssembly.TypeInfo lastTypeInfo;

		private Type lastType;

		static InstrumentedAssembly()
		{
			InstrumentedAssembly.readerWriterLock = new ReaderWriterLock();
			InstrumentedAssembly.mapIDToPublishedObject = new Hashtable();
			InstrumentedAssembly.mapPublishedObjectToID = new Hashtable();
			InstrumentedAssembly.upcountId = 0xeff;
		}

		public InstrumentedAssembly(Assembly assembly, SchemaNaming naming)
		{
			this.mapTypeToTypeInfo = new Hashtable();
			SecurityHelper.UnmanagedCode.Demand();
			this.naming = naming;
			Assembly precompiledAssembly = naming.PrecompiledAssembly;
			if (null == precompiledAssembly)
			{
				CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
				CompilerParameters compilerParameter = new CompilerParameters();
				compilerParameter.GenerateInMemory = true;
				compilerParameter.ReferencedAssemblies.Add(assembly.Location);
				compilerParameter.ReferencedAssemblies.Add(typeof(BaseEvent).Assembly.Location);
				compilerParameter.ReferencedAssemblies.Add(typeof(Component).Assembly.Location);
				Type[] types = assembly.GetTypes();
				for (int i = 0; i < (int)types.Length; i++)
				{
					Type type = types[i];
					if (this.IsInstrumentedType(type))
					{
						this.FindReferences(type, compilerParameter);
					}
				}
				string[] code = new string[1];
				code[0] = naming.Code;
				CompilerResults compilerResult = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameter, code);
				foreach (CompilerError error in compilerResult.Errors)
				{
					Console.WriteLine(error.ToString());
				}
				if (!compilerResult.Errors.HasErrors)
				{
					precompiledAssembly = compilerResult.CompiledAssembly;
				}
				else
				{
					throw new Exception(RC.GetString("FAILED_TO_BUILD_GENERATED_ASSEMBLY"));
				}
			}
			Type type1 = precompiledAssembly.GetType("WMINET_Converter");
			this.mapTypeToConverter = (Hashtable)type1.GetField("mapTypeToConverter").GetValue(null);
			if (MTAHelper.IsNoContextMTA())
			{
				this.InitEventSource(this);
				return;
			}
			else
			{
				ThreadDispatch threadDispatch = new ThreadDispatch(new ThreadDispatch.ThreadWorkerMethodWithParam(this.InitEventSource));
				threadDispatch.Parameter = this;
				threadDispatch.Start();
				return;
			}
		}

		public void FindReferences(Type type, CompilerParameters parameters)
		{
			if (!parameters.ReferencedAssemblies.Contains(type.Assembly.Location))
			{
				parameters.ReferencedAssemblies.Add(type.Assembly.Location);
			}
			if (type.BaseType != null)
			{
				this.FindReferences(type.BaseType, parameters);
			}
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < (int)interfaces.Length; i++)
			{
				Type type1 = interfaces[i];
				if (type1.Assembly != type.Assembly)
				{
					this.FindReferences(type1, parameters);
				}
			}
		}

		public void Fire(object o)
		{
			SecurityHelper.UnmanagedCode.Demand();
			this.Fire(o.GetType(), o);
		}

		public void Fire(Type t, object o)
		{
			InstrumentedAssembly.TypeInfo typeInfo = this.GetTypeInfo(t);
			typeInfo.Fire(o);
		}

		private InstrumentedAssembly.TypeInfo GetTypeInfo(Type t)
		{
			InstrumentedAssembly.TypeInfo typeInfo;
			lock (this.mapTypeToTypeInfo)
			{
				if (this.lastType != t)
				{
					this.lastType = t;
					InstrumentedAssembly.TypeInfo item = (InstrumentedAssembly.TypeInfo)this.mapTypeToTypeInfo[t];
					if (item == null)
					{
						item = new InstrumentedAssembly.TypeInfo(this.source, this.naming, (Type)this.mapTypeToConverter[t]);
						this.mapTypeToTypeInfo.Add(t, item);
					}
					this.lastTypeInfo = item;
					typeInfo = item;
				}
				else
				{
					typeInfo = this.lastTypeInfo;
				}
			}
			return typeInfo;
		}

		private void InitEventSource(object param)
		{
			InstrumentedAssembly eventSource = (InstrumentedAssembly)param;
			eventSource.source = new EventSource(eventSource.naming.NamespaceName, eventSource.naming.DecoupledProviderInstanceName, this);
		}

		public bool IsInstrumentedType(Type type)
		{
			if (null != type.GetInterface("System.Management.Instrumentation.IEvent", false) || null != type.GetInterface("System.Management.Instrumentation.IInstance", false))
			{
				return true;
			}
			else
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(InstrumentationClassAttribute), true);
				if (customAttributes == null || (int)customAttributes.Length == 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public void Publish(object o)
		{
			SecurityHelper.UnmanagedCode.Demand();
			try
			{
				InstrumentedAssembly.readerWriterLock.AcquireWriterLock(-1);
				if (!InstrumentedAssembly.mapPublishedObjectToID.ContainsKey(o))
				{
					InstrumentedAssembly.mapIDToPublishedObject.Add(InstrumentedAssembly.upcountId.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))), o);
					InstrumentedAssembly.mapPublishedObjectToID.Add(o, InstrumentedAssembly.upcountId);
					InstrumentedAssembly.upcountId = InstrumentedAssembly.upcountId + 1;
				}
			}
			finally
			{
				InstrumentedAssembly.readerWriterLock.ReleaseWriterLock();
			}
		}

		public void Revoke(object o)
		{
			SecurityHelper.UnmanagedCode.Demand();
			try
			{
				InstrumentedAssembly.readerWriterLock.AcquireWriterLock(-1);
				object item = InstrumentedAssembly.mapPublishedObjectToID[o];
				if (item != null)
				{
					int num = (int)item;
					InstrumentedAssembly.mapPublishedObjectToID.Remove(o);
					InstrumentedAssembly.mapIDToPublishedObject.Remove(num.ToString((IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int))));
				}
			}
			finally
			{
				InstrumentedAssembly.readerWriterLock.ReleaseWriterLock();
			}
		}

		public void SetBatchSize(Type t, int batchSize)
		{
			this.GetTypeInfo(t).SetBatchSize(batchSize);
		}

		private class TypeInfo
		{
			private FieldInfo fieldInfo;

			private int batchSize;

			private bool batchEvents;

			private ConvertToWMI[] convertFunctionsBatch;

			private ConvertToWMI convertFunctionNoBatch;

			private IntPtr[] wbemObjects;

			private Type converterType;

			private int currentIndex;

			public EventSource source;

			public int lastFire;

			public Thread cleanupThread;

			public TypeInfo(EventSource source, SchemaNaming naming, Type converterType)
			{
				this.batchSize = 64;
				this.batchEvents = true;
				this.converterType = converterType;
				this.source = source;
				object obj = Activator.CreateInstance(converterType);
				this.convertFunctionNoBatch = (ConvertToWMI)Delegate.CreateDelegate(typeof(ConvertToWMI), obj, "ToWMI");
				this.SetBatchSize(this.batchSize);
			}

			public void Cleanup()
			{
				int num = 0;
				while (num < 20)
				{
					Thread.Sleep(100);
					if (this.currentIndex != 0)
					{
						num = 0;
						if (Environment.TickCount - this.lastFire < 100)
						{
							continue;
						}
						lock (this)
						{
							if (this.currentIndex > 0)
							{
								this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
								this.currentIndex = 0;
								this.lastFire = Environment.TickCount;
							}
						}
					}
					else
					{
						num++;
					}
				}
				this.cleanupThread = null;
			}

			public IntPtr ExtractIntPtr(object o)
			{
				return (IntPtr)o.GetType().GetField("instWbemObjectAccessIP").GetValue(o);
			}

			public void Fire(object o)
			{
				if (!this.source.Any())
				{
					if (this.batchEvents)
					{
						lock (this)
						{
							InstrumentedAssembly.TypeInfo typeInfo = this;
							int num = typeInfo.currentIndex;
							int num1 = num;
							typeInfo.currentIndex = num + 1;
							this.convertFunctionsBatch[num1](o);
							this.wbemObjects[this.currentIndex - 1] = (IntPtr)this.fieldInfo.GetValue(this.convertFunctionsBatch[this.currentIndex - 1].Target);
							if (this.cleanupThread != null)
							{
								if (this.currentIndex == this.batchSize)
								{
									this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
									this.currentIndex = 0;
									this.lastFire = Environment.TickCount;
								}
							}
							else
							{
								int tickCount = Environment.TickCount;
								if (tickCount - this.lastFire >= 0x3e8)
								{
									this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
									this.currentIndex = 0;
									this.lastFire = tickCount;
								}
								else
								{
									this.lastFire = Environment.TickCount;
									this.cleanupThread = new Thread(new ThreadStart(this.Cleanup));
									this.cleanupThread.SetApartmentState(ApartmentState.MTA);
									this.cleanupThread.Start();
								}
							}
						}
					}
					else
					{
						lock (this)
						{
							this.convertFunctionNoBatch(o);
							this.wbemObjects[0] = (IntPtr)this.fieldInfo.GetValue(this.convertFunctionNoBatch.Target);
							this.source.IndicateEvents(1, this.wbemObjects);
						}
					}
					return;
				}
				else
				{
					return;
				}
			}

			public void SetBatchSize(int batchSize)
			{
				if (batchSize > 0)
				{
					if (!WMICapabilities.MultiIndicateSupported)
					{
						batchSize = 1;
					}
					lock (this)
					{
						if (this.currentIndex > 0)
						{
							this.source.IndicateEvents(this.currentIndex, this.wbemObjects);
							this.currentIndex = 0;
							this.lastFire = Environment.TickCount;
						}
						this.wbemObjects = new IntPtr[batchSize];
						if (batchSize <= 1)
						{
							this.fieldInfo = this.convertFunctionNoBatch.Target.GetType().GetField("instWbemObjectAccessIP");
							this.wbemObjects[0] = this.ExtractIntPtr(this.convertFunctionNoBatch.Target);
							this.batchEvents = false;
						}
						else
						{
							this.batchEvents = true;
							this.batchSize = batchSize;
							this.convertFunctionsBatch = new ConvertToWMI[batchSize];
							for (int i = 0; i < batchSize; i++)
							{
								object obj = Activator.CreateInstance(this.converterType);
								this.convertFunctionsBatch[i] = (ConvertToWMI)Delegate.CreateDelegate(typeof(ConvertToWMI), obj, "ToWMI");
								this.wbemObjects[i] = this.ExtractIntPtr(obj);
							}
							this.fieldInfo = this.convertFunctionsBatch[0].Target.GetType().GetField("instWbemObjectAccessIP");
						}
					}
					return;
				}
				else
				{
					throw new ArgumentOutOfRangeException("batchSize");
				}
			}
		}
	}
}