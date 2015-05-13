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
using System.Collections.Specialized;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.Management.Instrumentation
{
	internal class SchemaNaming
	{
		private const string Win32ProviderClassName = "__Win32Provider";

		private const string EventProviderRegistrationClassName = "__EventProviderRegistration";

		private const string InstanceProviderRegistrationClassName = "__InstanceProviderRegistration";

		private const string DecoupledProviderClassName = "MSFT_DecoupledProvider";

		private const string ProviderClassName = "WMINET_ManagedAssemblyProvider";

		private const string InstrumentationClassName = "WMINET_Instrumentation";

		private const string InstrumentedAssembliesClassName = "WMINET_InstrumentedAssembly";

		private const string DecoupledProviderCLSID = "{54D8502C-527D-43f7-A506-A9DA075E229C}";

		private const string GlobalWmiNetNamespace = "root\\MicrosoftWmiNet";

		private const string InstrumentedNamespacesClassName = "WMINET_InstrumentedNamespaces";

		private const string NamingClassName = "WMINET_Naming";

		private const string iwoaDef = "class IWOA\n{\nprotected const string DllName = \"wminet_utils.dll\";\nprotected const string EntryPointName = \"UFunc\";\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyHandle\")] public static extern int GetPropertyHandle_f27(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszPropertyName, [Out] out Int32 pType, [Out] out Int32 plHandle);\n//[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte aData);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadPropertyValue\")] public static extern int ReadPropertyValue_f29(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lBufferSize, [Out] out Int32 plNumBytes, [Out] out Byte aData);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadDWORD\")] public static extern int ReadDWORD_f30(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt32 pdw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt32 dw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadQWORD\")] public static extern int ReadQWORD_f32(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt64 pqw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt64 pw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyInfoByHandle\")] public static extern int GetPropertyInfoByHandle_f34(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out][MarshalAs(UnmanagedType.BStr)]  out string   pstrName, [Out] out Int32 pType);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Lock\")] public static extern int Lock_f35(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Unlock\")] public static extern int Unlock_f36(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\n\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Put\")] public static extern int Put_f5(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszName, [In] Int32 lFlags, [In] ref object pVal, [In] Int32 Type);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In][MarshalAs(UnmanagedType.LPWStr)] string str);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref SByte n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Int16 n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref UInt16 n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\", CharSet=CharSet.Unicode)] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Char c);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 dw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteSingle\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Single dw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int64 pw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDouble\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Double pw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Clone\")] public static extern int Clone_f(int vFunc, IntPtr pWbemClassObject, [Out] out IntPtr ppCopy);\n}\ninterface IWmiConverter\n{\n    void ToWMI(object obj);\n    ManagementObject GetInstance();\n}\nclass SafeAssign\n{\n    public static UInt16 boolTrue = 0xffff;\n    public static UInt16 boolFalse = 0;\n    static Hashtable validTypes = new Hashtable();\n    static SafeAssign()\n    {\n        validTypes.Add(typeof(SByte), null);\n        validTypes.Add(typeof(Byte), null);\n        validTypes.Add(typeof(Int16), null);\n        validTypes.Add(typeof(UInt16), null);\n        validTypes.Add(typeof(Int32), null);\n        validTypes.Add(typeof(UInt32), null);\n        validTypes.Add(typeof(Int64), null);\n        validTypes.Add(typeof(UInt64), null);\n        validTypes.Add(typeof(Single), null);\n        validTypes.Add(typeof(Double), null);\n        validTypes.Add(typeof(Boolean), null);\n        validTypes.Add(typeof(String), null);\n        validTypes.Add(typeof(Char), null);\n        validTypes.Add(typeof(DateTime), null);\n        validTypes.Add(typeof(TimeSpan), null);\n        validTypes.Add(typeof(ManagementObject), null);\n        nullClass.SystemProperties [\"__CLASS\"].Value = \"nullInstance\";\n    }\n    public static object GetInstance(object o)\n    {\n        if(o is ManagementObject)\n            return o;\n        return null;\n    }\n    static ManagementClass nullClass = new ManagementClass(";

		private const string iwoaDefEnd = ");\n    \n    public static ManagementObject GetManagementObject(object o)\n    {\n        if(o != null && o is ManagementObject)\n            return o as ManagementObject;\n        // Must return empty instance\n        return nullClass.CreateInstance();\n    }\n    public static object GetValue(object o)\n    {\n        Type t = o.GetType();\n        if(t.IsArray)\n            t = t.GetElementType();\n        if(validTypes.Contains(t))\n            return o;\n        return null;\n    }\n    public static string WMITimeToString(DateTime dt)\n    {\n        TimeSpan ts = dt.Subtract(dt.ToUniversalTime());\n        int diffUTC = (ts.Minutes + ts.Hours * 60);\n        if(diffUTC >= 0)\n            return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000+{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, diffUTC);\n        return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000-{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, -diffUTC);\n    }\n    public static string WMITimeToString(TimeSpan ts)\n    {\n        return String.Format(\"{0:D8}{1:D2}{2:D2}{3:D2}.{4:D3}000:000\", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);\n    }\n    public static string[] WMITimeArrayToStringArray(DateTime[] dates)\n    {\n        string[] strings = new string[dates.Length];\n        for(int i=0;i<dates.Length;i++)\n            strings[i] = WMITimeToString(dates[i]);\n        return strings;\n    }\n    public static string[] WMITimeArrayToStringArray(TimeSpan[] timeSpans)\n    {\n        string[] strings = new string[timeSpans.Length];\n        for(int i=0;i<timeSpans.Length;i++)\n            strings[i] = WMITimeToString(timeSpans[i]);\n        return strings;\n    }\n}\n";

		private Assembly assembly;

		private SchemaNaming.AssemblySpecificNaming assemblyInfo;

		private ManagementObject registrationInstance;

		private string AssemblyName
		{
			get
			{
				return this.assemblyInfo.AssemblyName;
			}
		}

		private string AssemblyPath
		{
			get
			{
				return this.assemblyInfo.AssemblyPath;
			}
		}

		private string AssemblyUniqueIdentifier
		{
			get
			{
				return this.assemblyInfo.AssemblyUniqueIdentifier;
			}
		}

		public string Code
		{
			get
			{
				string end;
				using (StreamReader streamReader = new StreamReader(this.CodePath))
				{
					end = streamReader.ReadToEnd();
				}
				return end;
			}
		}

		private string CodePath
		{
			get
			{
				return Path.Combine(this.DataDirectory, string.Concat(this.DecoupledProviderInstanceName, ".cs"));
			}
		}

		private string DataDirectory
		{
			get
			{
				return Path.Combine(WMICapabilities.FrameworkDirectory, this.NamespaceName);
			}
		}

		private string DecoupledProviderClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath(this.assemblyInfo.NamespaceName, "MSFT_DecoupledProvider");
			}
		}

		public string DecoupledProviderInstanceName
		{
			get
			{
				return this.assemblyInfo.DecoupledProviderInstanceName;
			}
			set
			{
				this.assemblyInfo.DecoupledProviderInstanceName = value;
			}
		}

		private string EventProviderRegistrationClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath(this.assemblyInfo.NamespaceName, "__EventProviderRegistration");
			}
		}

		private string EventProviderRegistrationPath
		{
			get
			{
				return SchemaNaming.AppendProperty(this.EventProviderRegistrationClassPath, "provider", string.Concat("\\\\\\\\.\\\\", this.ProviderPath.Replace("\\", "\\\\").Replace("\"", "\\\"")));
			}
		}

		private string GlobalInstrumentationClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath("root\\MicrosoftWmiNet", "WMINET_Instrumentation");
			}
		}

		private string GlobalNamingClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath("root\\MicrosoftWmiNet", "WMINET_Naming");
			}
		}

		private string GlobalRegistrationClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath("root\\MicrosoftWmiNet", "WMINET_InstrumentedNamespaces");
			}
		}

		private string GlobalRegistrationNamespace
		{
			get
			{
				return "root\\MicrosoftWmiNet";
			}
		}

		private string GlobalRegistrationPath
		{
			get
			{
				return SchemaNaming.AppendProperty(this.GlobalRegistrationClassPath, "NamespaceName", this.assemblyInfo.NamespaceName.Replace("\\", "\\\\"));
			}
		}

		private string InstanceProviderRegistrationClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath(this.assemblyInfo.NamespaceName, "__InstanceProviderRegistration");
			}
		}

		private string InstanceProviderRegistrationPath
		{
			get
			{
				return SchemaNaming.AppendProperty(this.InstanceProviderRegistrationClassPath, "provider", string.Concat("\\\\\\\\.\\\\", this.ProviderPath.Replace("\\", "\\\\").Replace("\"", "\\\"")));
			}
		}

		private string InstrumentationClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath(this.assemblyInfo.NamespaceName, "WMINET_Instrumentation");
			}
		}

		public string Mof
		{
			get
			{
				string end;
				using (StreamReader streamReader = new StreamReader(this.MofPath))
				{
					end = streamReader.ReadToEnd();
				}
				return end;
			}
		}

		private string MofPath
		{
			get
			{
				return Path.Combine(this.DataDirectory, string.Concat(this.DecoupledProviderInstanceName, ".mof"));
			}
		}

		public string NamespaceName
		{
			get
			{
				return this.assemblyInfo.NamespaceName;
			}
		}

		public Assembly PrecompiledAssembly
		{
			get
			{
				if (!File.Exists(this.PrecompiledAssemblyPath))
				{
					return null;
				}
				else
				{
					return Assembly.LoadFrom(this.PrecompiledAssemblyPath);
				}
			}
		}

		private string PrecompiledAssemblyPath
		{
			get
			{
				return Path.Combine(this.DataDirectory, string.Concat(this.DecoupledProviderInstanceName, ".dll"));
			}
		}

		private string ProviderClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath(this.assemblyInfo.NamespaceName, "WMINET_ManagedAssemblyProvider");
			}
		}

		private string ProviderPath
		{
			get
			{
				return SchemaNaming.AppendProperty(this.ProviderClassPath, "Name", this.assemblyInfo.DecoupledProviderInstanceName);
			}
		}

		private string RegistrationClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath(this.assemblyInfo.NamespaceName, "WMINET_InstrumentedAssembly");
			}
		}

		private ManagementObject RegistrationInstance
		{
			get
			{
				if (this.registrationInstance == null)
				{
					this.registrationInstance = new ManagementObject(this.RegistrationPath);
				}
				return this.registrationInstance;
			}
		}

		private string RegistrationPath
		{
			get
			{
				return SchemaNaming.AppendProperty(this.RegistrationClassPath, "Name", this.assemblyInfo.DecoupledProviderInstanceName);
			}
		}

		public string SecurityDescriptor
		{
			get
			{
				return this.assemblyInfo.SecurityDescriptor;
			}
		}

		private string Win32ProviderClassPath
		{
			get
			{
				return SchemaNaming.MakeClassPath(this.assemblyInfo.NamespaceName, "__Win32Provider");
			}
		}

		private SchemaNaming(string namespaceName, string securityDescriptor, Assembly assembly)
		{
			this.assembly = assembly;
			this.assemblyInfo = new SchemaNaming.AssemblySpecificNaming(namespaceName, securityDescriptor, assembly);
			if (!SchemaNaming.DoesInstanceExist(this.RegistrationPath))
			{
				this.assemblyInfo.DecoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyMinorVersion(assembly);
			}
		}

		private static string AppendProperty(string classPath, string propertyName, string propertyValue)
		{
			object[] objArray = new object[6];
			objArray[0] = classPath;
			objArray[1] = (char)46;
			objArray[2] = propertyName;
			objArray[3] = "=\"";
			objArray[4] = propertyValue;
			objArray[5] = (char)34;
			return string.Concat(objArray);
		}

		private static bool DoesClassExist(string objectPath)
		{
			bool flag = false;
			try
			{
				ManagementObject managementClass = new ManagementClass(objectPath);
				managementClass.Get();
				flag = true;
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				if (ManagementStatus.InvalidNamespace != managementException.ErrorCode && ManagementStatus.InvalidClass != managementException.ErrorCode && ManagementStatus.NotFound != managementException.ErrorCode)
				{
					throw managementException;
				}
			}
			return flag;
		}

		private static bool DoesInstanceExist(string objectPath)
		{
			bool flag = false;
			try
			{
				ManagementObject managementObject = new ManagementObject(objectPath);
				managementObject.Get();
				flag = true;
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				if (ManagementStatus.InvalidNamespace != managementException.ErrorCode && ManagementStatus.InvalidClass != managementException.ErrorCode && ManagementStatus.NotFound != managementException.ErrorCode)
				{
					throw managementException;
				}
			}
			return flag;
		}

		private static void EnsureClassExists(SchemaNaming.InstallLogWrapper context, string classPath, SchemaNaming.ClassMaker classMakerFunction)
		{
			try
			{
				context.LogMessage(string.Concat(RC.GetString("CLASS_ENSURE"), " ", classPath));
				ManagementClass managementClass = new ManagementClass(classPath);
				managementClass.Get();
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				if (managementException.ErrorCode != ManagementStatus.NotFound)
				{
					throw managementException;
				}
				else
				{
					context.LogMessage(string.Concat(RC.GetString("CLASS_ENSURECREATE"), " ", classPath));
					ManagementClass managementClass1 = classMakerFunction();
					managementClass1.Put();
				}
			}
		}

		private static void EnsureNamespace(string baseNamespace, string childNamespaceName)
		{
			if (!SchemaNaming.DoesInstanceExist(string.Concat(baseNamespace, ":__NAMESPACE.Name=\"", childNamespaceName, "\"")))
			{
				ManagementClass managementClass = new ManagementClass(string.Concat(baseNamespace, ":__NAMESPACE"));
				ManagementObject managementObject = managementClass.CreateInstance();
				managementObject["Name"] = childNamespaceName;
				managementObject.Put();
			}
		}

		private static void EnsureNamespace(SchemaNaming.InstallLogWrapper context, string namespaceName)
		{
			context.LogMessage(string.Concat(RC.GetString("NAMESPACE_ENSURE"), " ", namespaceName));
			string str = null;
			char[] chrArray = new char[1];
			chrArray[0] = '\\';
			string[] strArrays = namespaceName.Split(chrArray);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str1 = strArrays[i];
				if (str != null)
				{
					SchemaNaming.EnsureNamespace(str, str1);
					str = string.Concat(str, "\\", str1);
				}
				else
				{
					str = str1;
				}
			}
		}

		private static string EnsureNamespaceInMof(string baseNamespace, string childNamespaceName)
		{
			return string.Format("{0}instance of __Namespace\n{{\n  Name = \"{1}\";\n}};\n\n", SchemaNaming.PragmaNamespace(baseNamespace), childNamespaceName);
		}

		private static string EnsureNamespaceInMof(string namespaceName)
		{
			string str = "";
			string str1 = null;
			char[] chrArray = new char[1];
			chrArray[0] = '\\';
			string[] strArrays = namespaceName.Split(chrArray);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str2 = strArrays[i];
				if (str1 != null)
				{
					str = string.Concat(str, SchemaNaming.EnsureNamespaceInMof(str1, str2));
					str1 = string.Concat(str1, "\\", str2);
				}
				else
				{
					str1 = str2;
				}
			}
			return str;
		}

		private string GenerateMof(string[] mofs)
		{
			string[] mofFormat = new string[22];
			mofFormat[0] = "//**************************************************************************\n";
			mofFormat[1] = string.Format("//* {0}\n", this.DecoupledProviderInstanceName);
			mofFormat[2] = string.Format("//* {0}\n", this.AssemblyUniqueIdentifier);
			mofFormat[3] = "//**************************************************************************\n";
			mofFormat[4] = "#pragma autorecover\n";
			mofFormat[5] = SchemaNaming.EnsureNamespaceInMof(this.GlobalRegistrationNamespace);
			mofFormat[6] = SchemaNaming.EnsureNamespaceInMof(this.NamespaceName);
			mofFormat[7] = SchemaNaming.PragmaNamespace(this.GlobalRegistrationNamespace);
			mofFormat[8] = SchemaNaming.GetMofFormat(new ManagementClass(this.GlobalInstrumentationClassPath));
			mofFormat[9] = SchemaNaming.GetMofFormat(new ManagementClass(this.GlobalRegistrationClassPath));
			mofFormat[10] = SchemaNaming.GetMofFormat(new ManagementClass(this.GlobalNamingClassPath));
			mofFormat[11] = SchemaNaming.GetMofFormat(new ManagementObject(this.GlobalRegistrationPath));
			mofFormat[12] = SchemaNaming.PragmaNamespace(this.NamespaceName);
			mofFormat[13] = SchemaNaming.GetMofFormat(new ManagementClass(this.InstrumentationClassPath));
			mofFormat[14] = SchemaNaming.GetMofFormat(new ManagementClass(this.RegistrationClassPath));
			mofFormat[15] = SchemaNaming.GetMofFormat(new ManagementClass(this.DecoupledProviderClassPath));
			mofFormat[16] = SchemaNaming.GetMofFormat(new ManagementClass(this.ProviderClassPath));
			mofFormat[17] = SchemaNaming.GetMofFormat(new ManagementObject(this.ProviderPath));
			mofFormat[18] = SchemaNaming.GetMofFormat(new ManagementObject(this.EventProviderRegistrationPath));
			mofFormat[19] = SchemaNaming.GetMofFormat(new ManagementObject(this.InstanceProviderRegistrationPath));
			mofFormat[20] = string.Concat(mofs);
			mofFormat[21] = SchemaNaming.GetMofFormat(new ManagementObject(this.RegistrationPath));
			return string.Concat(mofFormat);
		}

		private static string GetMofFormat(ManagementObject obj)
		{
			return string.Concat(obj.GetText(TextFormat.Mof).Replace("\n", "\n"), "\n");
		}

		public static SchemaNaming GetSchemaNaming(Assembly assembly)
		{
			InstrumentedAttribute attribute = InstrumentedAttribute.GetAttribute(assembly);
			if (attribute != null)
			{
				return new SchemaNaming(attribute.NamespaceName, attribute.SecurityDescriptor, assembly);
			}
			else
			{
				return null;
			}
		}

		public bool IsAssemblyRegistered()
		{
			if (!SchemaNaming.DoesInstanceExist(this.RegistrationPath))
			{
				return false;
			}
			else
			{
				ManagementObject managementObject = new ManagementObject(this.RegistrationPath);
				return 0 == string.Compare(this.AssemblyUniqueIdentifier, managementObject["RegisteredBuild"].ToString(), StringComparison.OrdinalIgnoreCase);
			}
		}

		private bool IsClassAlreadyPresentInRepository(ManagementObject obj)
		{
			bool flag = false;
			string str = SchemaNaming.MakeClassPath(this.NamespaceName, (string)obj.SystemProperties["__CLASS"].Value);
			if (SchemaNaming.DoesClassExist(str))
			{
				ManagementObject managementClass = new ManagementClass(str);
				flag = managementClass.CompareTo(obj, ComparisonSettings.IgnoreObjectSource | ComparisonSettings.IgnoreCase);
			}
			return flag;
		}

		private bool IsSchemaToBeCompared()
		{
			bool flag = false;
			if (SchemaNaming.DoesInstanceExist(this.RegistrationPath))
			{
				ManagementObject managementObject = new ManagementObject(this.RegistrationPath);
				flag = 0 != string.Compare(this.AssemblyUniqueIdentifier, managementObject["RegisteredBuild"].ToString(), StringComparison.OrdinalIgnoreCase);
			}
			return flag;
		}

		private static string MakeClassPath(string namespaceName, string className)
		{
			return string.Concat(namespaceName, ":", className);
		}

		private ManagementClass MakeDecoupledProviderClass()
		{
			ManagementClass managementClass = new ManagementClass(this.Win32ProviderClassPath);
			ManagementClass managementClass1 = managementClass.Derive("MSFT_DecoupledProvider");
			PropertyDataCollection properties = managementClass1.Properties;
			properties.Add("HostingModel", "Decoupled:Com", CimType.String);
			properties.Add("SecurityDescriptor", CimType.String, false);
			properties.Add("Version", 1, CimType.UInt32);
			properties["CLSID"].Value = "{54D8502C-527D-43f7-A506-A9DA075E229C}";
			return managementClass1;
		}

		private ManagementClass MakeGlobalInstrumentationClass()
		{
			ManagementClass managementClass = new ManagementClass("root\\MicrosoftWmiNet", "", null);
			managementClass.SystemProperties["__CLASS"].Value = "WMINET_Instrumentation";
			managementClass.Qualifiers.Add("abstract", true);
			return managementClass;
		}

		private ManagementClass MakeInstrumentationClass()
		{
			ManagementClass managementClass = new ManagementClass(this.NamespaceName, "", null);
			managementClass.SystemProperties["__CLASS"].Value = "WMINET_Instrumentation";
			managementClass.Qualifiers.Add("abstract", true);
			return managementClass;
		}

		private ManagementClass MakeNamespaceRegistrationClass()
		{
			ManagementClass managementClass = new ManagementClass(this.GlobalInstrumentationClassPath);
			ManagementClass managementClass1 = managementClass.Derive("WMINET_InstrumentedNamespaces");
			PropertyDataCollection properties = managementClass1.Properties;
			properties.Add("NamespaceName", CimType.String, false);
			PropertyData item = properties["NamespaceName"];
			item.Qualifiers.Add("key", true);
			return managementClass1;
		}

		private ManagementClass MakeNamingClass()
		{
			ManagementClass managementClass = new ManagementClass(this.GlobalInstrumentationClassPath);
			ManagementClass managementClass1 = managementClass.Derive("WMINET_Naming");
			managementClass1.Qualifiers.Add("abstract", true);
			PropertyDataCollection properties = managementClass1.Properties;
			properties.Add("InstrumentedAssembliesClassName", "WMINET_InstrumentedAssembly", CimType.String);
			return managementClass1;
		}

		private ManagementClass MakeProviderClass()
		{
			ManagementClass managementClass = new ManagementClass(this.DecoupledProviderClassPath);
			ManagementClass managementClass1 = managementClass.Derive("WMINET_ManagedAssemblyProvider");
			PropertyDataCollection properties = managementClass1.Properties;
			properties.Add("Assembly", CimType.String, false);
			return managementClass1;
		}

		private ManagementClass MakeRegistrationClass()
		{
			ManagementClass managementClass = new ManagementClass(this.InstrumentationClassPath);
			ManagementClass managementClass1 = managementClass.Derive("WMINET_InstrumentedAssembly");
			PropertyDataCollection properties = managementClass1.Properties;
			properties.Add("Name", CimType.String, false);
			PropertyData item = properties["Name"];
			item.Qualifiers.Add("key", true);
			properties.Add("RegisteredBuild", CimType.String, false);
			properties.Add("FullName", CimType.String, false);
			properties.Add("PathToAssembly", CimType.String, false);
			properties.Add("Code", CimType.String, false);
			properties.Add("Mof", CimType.String, false);
			return managementClass1;
		}

		private static string PragmaNamespace(string namespaceName)
		{
			return string.Format("#pragma namespace(\"\\\\\\\\.\\\\{0}\")\n\n", namespaceName.Replace("\\", "\\\\"));
		}

		private void RegisterAssemblyAsInstrumented()
		{
			ManagementClass managementClass = new ManagementClass(this.RegistrationClassPath);
			ManagementObject decoupledProviderInstanceName = managementClass.CreateInstance();
			decoupledProviderInstanceName["Name"] = this.DecoupledProviderInstanceName;
			decoupledProviderInstanceName["RegisteredBuild"] = this.AssemblyUniqueIdentifier;
			decoupledProviderInstanceName["FullName"] = this.AssemblyName;
			decoupledProviderInstanceName["PathToAssembly"] = this.AssemblyPath;
			decoupledProviderInstanceName["Code"] = "";
			decoupledProviderInstanceName["Mof"] = "";
			decoupledProviderInstanceName.Put();
		}

		private void RegisterAssemblySpecificDecoupledProviderInstance()
		{
			ManagementClass managementClass = new ManagementClass(this.ProviderClassPath);
			ManagementObject decoupledProviderInstanceName = managementClass.CreateInstance();
			decoupledProviderInstanceName["Name"] = this.DecoupledProviderInstanceName;
			decoupledProviderInstanceName["HostingModel"] = "Decoupled:Com";
			if (this.SecurityDescriptor != null)
			{
				decoupledProviderInstanceName["SecurityDescriptor"] = this.SecurityDescriptor;
			}
			decoupledProviderInstanceName.Put();
		}

		public void RegisterAssemblySpecificSchema()
		{
			SecurityHelper.UnmanagedCode.Demand();
			Type[] instrumentedTypes = InstrumentedAttribute.GetInstrumentedTypes(this.assembly);
			StringCollection stringCollections = new StringCollection();
			StringCollection stringCollections1 = new StringCollection();
			StringCollection stringCollections2 = new StringCollection();
			string[] mofFormat = new string[(int)instrumentedTypes.Length];
			CodeWriter codeWriter = new CodeWriter();
			ReferencesCollection referencesCollection = new ReferencesCollection();
			codeWriter.AddChild(referencesCollection.UsingCode);
			referencesCollection.Add(typeof(object));
			referencesCollection.Add(typeof(ManagementClass));
			referencesCollection.Add(typeof(Marshal));
			referencesCollection.Add(typeof(SuppressUnmanagedCodeSecurityAttribute));
			referencesCollection.Add(typeof(FieldInfo));
			referencesCollection.Add(typeof(Hashtable));
			codeWriter.Line();
			CodeWriter codeWriter1 = codeWriter.AddChild("public class WMINET_Converter");
			codeWriter1.Line("public static Hashtable mapTypeToConverter = new Hashtable();");
			CodeWriter codeWriter2 = codeWriter1.AddChild("static WMINET_Converter()");
			Hashtable hashtables = new Hashtable();
			for (int i = 0; i < (int)instrumentedTypes.Length; i++)
			{
				hashtables[instrumentedTypes[i]] = string.Concat("ConvertClass_", i);
			}
			bool beCompared = this.IsSchemaToBeCompared();
			bool flag = false;
			if (!beCompared)
			{
				flag = !this.IsAssemblyRegistered();
			}
			for (int j = 0; j < (int)instrumentedTypes.Length; j++)
			{
				SchemaMapping schemaMapping = new SchemaMapping(instrumentedTypes[j], this, hashtables);
				codeWriter2.Line(string.Format("mapTypeToConverter[typeof({0})] = typeof({1});", schemaMapping.ClassType.FullName.Replace('+', '.'), schemaMapping.CodeClassName));
				if (beCompared && !this.IsClassAlreadyPresentInRepository(schemaMapping.NewClass))
				{
					flag = true;
				}
				SchemaNaming.ReplaceClassIfNecessary(schemaMapping.ClassPath, schemaMapping.NewClass);
				mofFormat[j] = SchemaNaming.GetMofFormat(schemaMapping.NewClass);
				codeWriter.AddChild(schemaMapping.Code);
				InstrumentationType instrumentationType = schemaMapping.InstrumentationType;
				switch (instrumentationType)
				{
					case InstrumentationType.Instance:
					{
						stringCollections1.Add(schemaMapping.ClassName);
						break;
					}
					case InstrumentationType.Event:
					{
						stringCollections.Add(schemaMapping.ClassName);
						break;
					}
					case InstrumentationType.Abstract:
					{
						stringCollections2.Add(schemaMapping.ClassName);
						break;
					}
				}
			}
			this.RegisterAssemblySpecificDecoupledProviderInstance();
			this.RegisterProviderAsEventProvider(stringCollections);
			this.RegisterProviderAsInstanceProvider();
			this.RegisterAssemblyAsInstrumented();
			Directory.CreateDirectory(this.DataDirectory);
			using (StreamWriter streamWriter = new StreamWriter(this.CodePath, false, Encoding.Unicode))
			{
				streamWriter.WriteLine(codeWriter);
				streamWriter.WriteLine(string.Concat("class IWOA\n{\nprotected const string DllName = \"wminet_utils.dll\";\nprotected const string EntryPointName = \"UFunc\";\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyHandle\")] public static extern int GetPropertyHandle_f27(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszPropertyName, [Out] out Int32 pType, [Out] out Int32 plHandle);\n//[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte aData);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadPropertyValue\")] public static extern int ReadPropertyValue_f29(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lBufferSize, [Out] out Int32 plNumBytes, [Out] out Byte aData);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadDWORD\")] public static extern int ReadDWORD_f30(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt32 pdw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt32 dw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"ReadQWORD\")] public static extern int ReadQWORD_f32(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out] out UInt64 pqw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] UInt64 pw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"GetPropertyInfoByHandle\")] public static extern int GetPropertyInfoByHandle_f34(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [Out][MarshalAs(UnmanagedType.BStr)]  out string   pstrName, [Out] out Int32 pType);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Lock\")] public static extern int Lock_f35(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Unlock\")] public static extern int Unlock_f36(int vFunc, IntPtr pWbemClassObject, [In] Int32 lFlags);\n\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Put\")] public static extern int Put_f5(int vFunc, IntPtr pWbemClassObject, [In][MarshalAs(UnmanagedType.LPWStr)]  string   wszName, [In] Int32 lFlags, [In] ref object pVal, [In] Int32 Type);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In][MarshalAs(UnmanagedType.LPWStr)] string str);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Byte n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref SByte n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Int16 n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\")] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref UInt16 n);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WritePropertyValue\", CharSet=CharSet.Unicode)] public static extern int WritePropertyValue_f28(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 lNumBytes, [In] ref Char c);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDWORD\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int32 dw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteSingle\")] public static extern int WriteDWORD_f31(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Single dw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteQWORD\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Int64 pw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"WriteDouble\")] public static extern int WriteQWORD_f33(int vFunc, IntPtr pWbemClassObject, [In] Int32 lHandle, [In] Double pw);\n[SuppressUnmanagedCodeSecurity, DllImport(DllName, EntryPoint=\"Clone\")] public static extern int Clone_f(int vFunc, IntPtr pWbemClassObject, [Out] out IntPtr ppCopy);\n}\ninterface IWmiConverter\n{\n    void ToWMI(object obj);\n    ManagementObject GetInstance();\n}\nclass SafeAssign\n{\n    public static UInt16 boolTrue = 0xffff;\n    public static UInt16 boolFalse = 0;\n    static Hashtable validTypes = new Hashtable();\n    static SafeAssign()\n    {\n        validTypes.Add(typeof(SByte), null);\n        validTypes.Add(typeof(Byte), null);\n        validTypes.Add(typeof(Int16), null);\n        validTypes.Add(typeof(UInt16), null);\n        validTypes.Add(typeof(Int32), null);\n        validTypes.Add(typeof(UInt32), null);\n        validTypes.Add(typeof(Int64), null);\n        validTypes.Add(typeof(UInt64), null);\n        validTypes.Add(typeof(Single), null);\n        validTypes.Add(typeof(Double), null);\n        validTypes.Add(typeof(Boolean), null);\n        validTypes.Add(typeof(String), null);\n        validTypes.Add(typeof(Char), null);\n        validTypes.Add(typeof(DateTime), null);\n        validTypes.Add(typeof(TimeSpan), null);\n        validTypes.Add(typeof(ManagementObject), null);\n        nullClass.SystemProperties [\"__CLASS\"].Value = \"nullInstance\";\n    }\n    public static object GetInstance(object o)\n    {\n        if(o is ManagementObject)\n            return o;\n        return null;\n    }\n    static ManagementClass nullClass = new ManagementClass(new ManagementPath(@\"", this.NamespaceName, "\"));\n    \n    public static ManagementObject GetManagementObject(object o)\n    {\n        if(o != null && o is ManagementObject)\n            return o as ManagementObject;\n        // Must return empty instance\n        return nullClass.CreateInstance();\n    }\n    public static object GetValue(object o)\n    {\n        Type t = o.GetType();\n        if(t.IsArray)\n            t = t.GetElementType();\n        if(validTypes.Contains(t))\n            return o;\n        return null;\n    }\n    public static string WMITimeToString(DateTime dt)\n    {\n        TimeSpan ts = dt.Subtract(dt.ToUniversalTime());\n        int diffUTC = (ts.Minutes + ts.Hours * 60);\n        if(diffUTC >= 0)\n            return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000+{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, diffUTC);\n        return String.Format(\"{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}.{6:D3}000-{7:D3}\", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, -diffUTC);\n    }\n    public static string WMITimeToString(TimeSpan ts)\n    {\n        return String.Format(\"{0:D8}{1:D2}{2:D2}{3:D2}.{4:D3}000:000\", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);\n    }\n    public static string[] WMITimeArrayToStringArray(DateTime[] dates)\n    {\n        string[] strings = new string[dates.Length];\n        for(int i=0;i<dates.Length;i++)\n            strings[i] = WMITimeToString(dates[i]);\n        return strings;\n    }\n    public static string[] WMITimeArrayToStringArray(TimeSpan[] timeSpans)\n    {\n        string[] strings = new string[timeSpans.Length];\n        for(int i=0;i<timeSpans.Length;i++)\n            strings[i] = WMITimeToString(timeSpans[i]);\n        return strings;\n    }\n}\n"));
			}
			using (StreamWriter streamWriter1 = new StreamWriter(this.MofPath, false, Encoding.Unicode))
			{
				streamWriter1.WriteLine(this.GenerateMof(mofFormat));
			}
			if (flag)
			{
				SchemaNaming.RegisterSchemaUsingMofcomp(this.MofPath);
			}
		}

		private void RegisterNamespaceAsInstrumented()
		{
			ManagementClass managementClass = new ManagementClass(this.GlobalRegistrationClassPath);
			ManagementObject namespaceName = managementClass.CreateInstance();
			namespaceName["NamespaceName"] = this.NamespaceName;
			namespaceName.Put();
		}

		public void RegisterNonAssemblySpecificSchema(InstallContext installContext)
		{
			SecurityHelper.UnmanagedCode.Demand();
			WmiNetUtilsHelper.VerifyClientKey_f();
			SchemaNaming.InstallLogWrapper installLogWrapper = new SchemaNaming.InstallLogWrapper(installContext);
			SchemaNaming.EnsureNamespace(installLogWrapper, this.GlobalRegistrationNamespace);
			SchemaNaming.EnsureClassExists(installLogWrapper, this.GlobalInstrumentationClassPath, new SchemaNaming.ClassMaker(this.MakeGlobalInstrumentationClass));
			SchemaNaming.EnsureClassExists(installLogWrapper, this.GlobalRegistrationClassPath, new SchemaNaming.ClassMaker(this.MakeNamespaceRegistrationClass));
			SchemaNaming.EnsureClassExists(installLogWrapper, this.GlobalNamingClassPath, new SchemaNaming.ClassMaker(this.MakeNamingClass));
			SchemaNaming.EnsureNamespace(installLogWrapper, this.NamespaceName);
			SchemaNaming.EnsureClassExists(installLogWrapper, this.InstrumentationClassPath, new SchemaNaming.ClassMaker(this.MakeInstrumentationClass));
			SchemaNaming.EnsureClassExists(installLogWrapper, this.RegistrationClassPath, new SchemaNaming.ClassMaker(this.MakeRegistrationClass));
			try
			{
				ManagementClass managementClass = new ManagementClass(this.DecoupledProviderClassPath);
				if (managementClass["HostingModel"].ToString() != "Decoupled:Com")
				{
					managementClass.Delete();
				}
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				if (managementException.ErrorCode != ManagementStatus.NotFound)
				{
					throw managementException;
				}
			}
			SchemaNaming.EnsureClassExists(installLogWrapper, this.DecoupledProviderClassPath, new SchemaNaming.ClassMaker(this.MakeDecoupledProviderClass));
			SchemaNaming.EnsureClassExists(installLogWrapper, this.ProviderClassPath, new SchemaNaming.ClassMaker(this.MakeProviderClass));
			if (!SchemaNaming.DoesInstanceExist(this.GlobalRegistrationPath))
			{
				this.RegisterNamespaceAsInstrumented();
			}
		}

		private string RegisterProviderAsEventProvider(StringCollection events)
		{
			ManagementClass managementClass = new ManagementClass(this.EventProviderRegistrationClassPath);
			ManagementObject managementObject = managementClass.CreateInstance();
			managementObject["provider"] = string.Concat("\\\\.\\", this.ProviderPath);
			string[] strArrays = new string[events.Count];
			int num = 0;
			foreach (string @event in events)
			{
				int num1 = num;
				num = num1 + 1;
				strArrays[num1] = string.Concat("select * from ", @event);
			}
			managementObject["EventQueryList"] = strArrays;
			return managementObject.Put().Path;
		}

		private string RegisterProviderAsInstanceProvider()
		{
			ManagementClass managementClass = new ManagementClass(this.InstanceProviderRegistrationClassPath);
			ManagementObject managementObject = managementClass.CreateInstance();
			managementObject["provider"] = string.Concat("\\\\.\\", this.ProviderPath);
			managementObject["SupportsGet"] = true;
			managementObject["SupportsEnumeration"] = true;
			return managementObject.Put().Path;
		}

		private static void RegisterSchemaUsingMofcomp(string mofPath)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.Arguments = mofPath;
			processStartInfo.FileName = string.Concat(WMICapabilities.InstallationDirectory, "\\mofcomp.exe");
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.CreateNoWindow = true;
			Process process = Process.Start(processStartInfo);
			process.WaitForExit();
		}

		private static void ReplaceClassIfNecessary(string classPath, ManagementClass newClass)
		{
			try
			{
				ManagementClass managementClass = SchemaNaming.SafeGetClass(classPath);
				if (managementClass != null)
				{
					if (newClass.GetText(TextFormat.Mof) != managementClass.GetText(TextFormat.Mof))
					{
						managementClass.Delete();
						newClass.Put();
					}
				}
				else
				{
					newClass.Put();
				}
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				string str = string.Concat(RC.GetString("CLASS_NOTREPLACED_EXCEPT"), "\n{0}\n{1}");
				throw new ArgumentException(string.Format(str, classPath, newClass.GetText(TextFormat.Mof)), managementException);
			}
		}

		private static ManagementClass SafeGetClass(string classPath)
		{
			ManagementClass managementClass = null;
			try
			{
				ManagementClass managementClass1 = new ManagementClass(classPath);
				managementClass1.Get();
				managementClass = managementClass1;
			}
			catch (ManagementException managementException1)
			{
				ManagementException managementException = managementException1;
				if (managementException.ErrorCode != ManagementStatus.NotFound)
				{
					throw managementException;
				}
			}
			return managementClass;
		}

		private class AssemblySpecificNaming
		{
			private string namespaceName;

			private string securityDescriptor;

			private string decoupledProviderInstanceName;

			private string assemblyUniqueIdentifier;

			private string assemblyName;

			private string assemblyPath;

			public string AssemblyName
			{
				get
				{
					return this.assemblyName;
				}
			}

			public string AssemblyPath
			{
				get
				{
					return this.assemblyPath;
				}
			}

			public string AssemblyUniqueIdentifier
			{
				get
				{
					return this.assemblyUniqueIdentifier;
				}
			}

			public string DecoupledProviderInstanceName
			{
				get
				{
					return this.decoupledProviderInstanceName;
				}
				set
				{
					this.decoupledProviderInstanceName = value;
				}
			}

			public string NamespaceName
			{
				get
				{
					return this.namespaceName;
				}
			}

			public string SecurityDescriptor
			{
				get
				{
					return this.securityDescriptor;
				}
			}

			public AssemblySpecificNaming(string namespaceName, string securityDescriptor, Assembly assembly)
			{
				this.namespaceName = namespaceName;
				this.securityDescriptor = securityDescriptor;
				this.decoupledProviderInstanceName = AssemblyNameUtility.UniqueToAssemblyFullVersion(assembly);
				this.assemblyUniqueIdentifier = AssemblyNameUtility.UniqueToAssemblyBuild(assembly);
				this.assemblyName = assembly.FullName;
				this.assemblyPath = assembly.Location;
			}
		}

		private delegate ManagementClass ClassMaker();

		private class InstallLogWrapper
		{
			private InstallContext context;

			public InstallLogWrapper(InstallContext context)
			{
				this.context = context;
			}

			public void LogMessage(string str)
			{
				if (this.context != null)
				{
					this.context.LogMessage(str);
				}
			}
		}
	}
}