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
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Management.Instrumentation
{
	public class Instrumentation
	{
		private static string processIdentity;

		private static Hashtable instrumentedAssemblies;

		internal static string ProcessIdentity
		{
			get
			{
				lock (typeof(Instrumentation))
				{
					if (Instrumentation.processIdentity == null)
					{
						Guid guid = Guid.NewGuid();
						Instrumentation.processIdentity = guid.ToString().ToLower(CultureInfo.InvariantCulture);
					}
				}
				return Instrumentation.processIdentity;
			}
		}

		static Instrumentation()
		{
			Instrumentation.processIdentity = null;
			Instrumentation.instrumentedAssemblies = new Hashtable();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public Instrumentation()
		{
		}

		public static void Fire(object eventData)
		{
			IEvent @event = eventData as IEvent;
			if (@event == null)
			{
				Instrumentation.GetFireFunction(eventData.GetType())(eventData);
				return;
			}
			else
			{
				@event.Fire();
				return;
			}
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		private static extern int GetCurrentProcessId();

		internal static ProvisionFunction GetFireFunction(Type type)
		{
			return new ProvisionFunction(Instrumentation.GetInstrumentedAssembly(type.Assembly).Fire);
		}

		private static InstrumentedAssembly GetInstrumentedAssembly(Assembly assembly)
		{
			InstrumentedAssembly item;
			lock (Instrumentation.instrumentedAssemblies)
			{
				if (!Instrumentation.instrumentedAssemblies.ContainsKey(assembly))
				{
					Instrumentation.Initialize(assembly);
				}
				item = (InstrumentedAssembly)Instrumentation.instrumentedAssemblies[assembly];
			}
			return item;
		}

		internal static ProvisionFunction GetPublishFunction(Type type)
		{
			return new ProvisionFunction(Instrumentation.GetInstrumentedAssembly(type.Assembly).Publish);
		}

		internal static ProvisionFunction GetRevokeFunction(Type type)
		{
			return new ProvisionFunction(Instrumentation.GetInstrumentedAssembly(type.Assembly).Revoke);
		}

		private static void Initialize(Assembly assembly)
		{
			lock (Instrumentation.instrumentedAssemblies)
			{
				if (!Instrumentation.instrumentedAssemblies.ContainsKey(assembly))
				{
					SchemaNaming schemaNaming = SchemaNaming.GetSchemaNaming(assembly);
					if (schemaNaming != null)
					{
						if (!schemaNaming.IsAssemblyRegistered())
						{
							if (WMICapabilities.IsUserAdmin())
							{
								schemaNaming.DecoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyFullVersion(assembly);
								schemaNaming.RegisterNonAssemblySpecificSchema(null);
								schemaNaming.RegisterAssemblySpecificSchema();
							}
							else
							{
								throw new Exception(RC.GetString("ASSEMBLY_NOT_REGISTERED"));
							}
						}
						InstrumentedAssembly instrumentedAssembly = new InstrumentedAssembly(assembly, schemaNaming);
						Instrumentation.instrumentedAssemblies.Add(assembly, instrumentedAssembly);
					}
				}
			}
		}

		public static bool IsAssemblyRegistered(Assembly assemblyToRegister)
		{
			bool flag;
			if (null != assemblyToRegister)
			{
				lock (Instrumentation.instrumentedAssemblies)
				{
					if (!Instrumentation.instrumentedAssemblies.ContainsKey(assemblyToRegister))
					{
						goto Label0;
					}
					else
					{
						flag = true;
					}
				}
				return flag;
			}
			else
			{
				throw new ArgumentNullException("assemblyToRegister");
			}
		Label0:
			SchemaNaming schemaNaming = SchemaNaming.GetSchemaNaming(assemblyToRegister);
			if (schemaNaming != null)
			{
				return schemaNaming.IsAssemblyRegistered();
			}
			else
			{
				return false;
			}
		}

		public static void Publish(object instanceData)
		{
			Type type = instanceData as Type;
			Assembly assembly = instanceData as Assembly;
			IInstance instance = instanceData as IInstance;
			if (type == null)
			{
				if (assembly == null)
				{
					if (instance == null)
					{
						Instrumentation.GetPublishFunction(instanceData.GetType())(instanceData);
						return;
					}
					else
					{
						instance.Published = true;
						return;
					}
				}
				else
				{
					Instrumentation.GetInstrumentedAssembly(assembly);
					return;
				}
			}
			else
			{
				Instrumentation.GetInstrumentedAssembly(type.Assembly);
				return;
			}
		}

		public static void RegisterAssembly(Assembly assemblyToRegister)
		{
			if (null != assemblyToRegister)
			{
				Instrumentation.GetInstrumentedAssembly(assemblyToRegister);
				return;
			}
			else
			{
				throw new ArgumentNullException("assemblyToRegister");
			}
		}

		public static void Revoke(object instanceData)
		{
			IInstance instance = instanceData as IInstance;
			if (instance == null)
			{
				Instrumentation.GetRevokeFunction(instanceData.GetType())(instanceData);
				return;
			}
			else
			{
				instance.Published = false;
				return;
			}
		}

		public static void SetBatchSize(Type instrumentationClass, int batchSize)
		{
			Instrumentation.GetInstrumentedAssembly(instrumentationClass.Assembly).SetBatchSize(instrumentationClass, batchSize);
		}
	}
}