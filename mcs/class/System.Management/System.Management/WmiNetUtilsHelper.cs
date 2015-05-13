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
using System.Security;

namespace System.Management
{
	internal static class WmiNetUtilsHelper
	{
		internal static string myDllPath;

		internal static WmiNetUtilsHelper.ResetSecurity ResetSecurity_f;

		internal static WmiNetUtilsHelper.SetSecurity SetSecurity_f;

		internal static WmiNetUtilsHelper.BlessIWbemServices BlessIWbemServices_f;

		internal static WmiNetUtilsHelper.BlessIWbemServicesObject BlessIWbemServicesObject_f;

		internal static WmiNetUtilsHelper.GetPropertyHandle GetPropertyHandle_f27;

		internal static WmiNetUtilsHelper.WritePropertyValue WritePropertyValue_f28;

		internal static WmiNetUtilsHelper.GetQualifierSet GetQualifierSet_f;

		internal static WmiNetUtilsHelper.Get Get_f;

		internal static WmiNetUtilsHelper.Put Put_f;

		internal static WmiNetUtilsHelper.Delete Delete_f;

		internal static WmiNetUtilsHelper.GetNames GetNames_f;

		internal static WmiNetUtilsHelper.BeginEnumeration BeginEnumeration_f;

		internal static WmiNetUtilsHelper.Next Next_f;

		internal static WmiNetUtilsHelper.EndEnumeration EndEnumeration_f;

		internal static WmiNetUtilsHelper.GetPropertyQualifierSet GetPropertyQualifierSet_f;

		internal static WmiNetUtilsHelper.Clone Clone_f;

		internal static WmiNetUtilsHelper.GetObjectText GetObjectText_f;

		internal static WmiNetUtilsHelper.SpawnDerivedClass SpawnDerivedClass_f;

		internal static WmiNetUtilsHelper.SpawnInstance SpawnInstance_f;

		internal static WmiNetUtilsHelper.CompareTo CompareTo_f;

		internal static WmiNetUtilsHelper.GetPropertyOrigin GetPropertyOrigin_f;

		internal static WmiNetUtilsHelper.InheritsFrom InheritsFrom_f;

		internal static WmiNetUtilsHelper.GetMethod GetMethod_f;

		internal static WmiNetUtilsHelper.PutMethod PutMethod_f;

		internal static WmiNetUtilsHelper.DeleteMethod DeleteMethod_f;

		internal static WmiNetUtilsHelper.BeginMethodEnumeration BeginMethodEnumeration_f;

		internal static WmiNetUtilsHelper.NextMethod NextMethod_f;

		internal static WmiNetUtilsHelper.EndMethodEnumeration EndMethodEnumeration_f;

		internal static WmiNetUtilsHelper.GetMethodQualifierSet GetMethodQualifierSet_f;

		internal static WmiNetUtilsHelper.GetMethodOrigin GetMethodOrigin_f;

		internal static WmiNetUtilsHelper.QualifierSet_Get QualifierGet_f;

		internal static WmiNetUtilsHelper.QualifierSet_Put QualifierPut_f;

		internal static WmiNetUtilsHelper.QualifierSet_Delete QualifierDelete_f;

		internal static WmiNetUtilsHelper.QualifierSet_GetNames QualifierGetNames_f;

		internal static WmiNetUtilsHelper.QualifierSet_BeginEnumeration QualifierBeginEnumeration_f;

		internal static WmiNetUtilsHelper.QualifierSet_Next QualifierNext_f;

		internal static WmiNetUtilsHelper.QualifierSet_EndEnumeration QualifierEndEnumeration_f;

		internal static WmiNetUtilsHelper.GetCurrentApartmentType GetCurrentApartmentType_f;

		internal static WmiNetUtilsHelper.VerifyClientKey VerifyClientKey_f;

		internal static WmiNetUtilsHelper.Clone Clone_f12;

		internal static WmiNetUtilsHelper.GetDemultiplexedStub GetDemultiplexedStub_f;

		internal static WmiNetUtilsHelper.CreateInstanceEnumWmi CreateInstanceEnumWmi_f;

		internal static WmiNetUtilsHelper.CreateClassEnumWmi CreateClassEnumWmi_f;

		internal static WmiNetUtilsHelper.ExecQueryWmi ExecQueryWmi_f;

		internal static WmiNetUtilsHelper.ExecNotificationQueryWmi ExecNotificationQueryWmi_f;

		internal static WmiNetUtilsHelper.PutInstanceWmi PutInstanceWmi_f;

		internal static WmiNetUtilsHelper.PutClassWmi PutClassWmi_f;

		internal static WmiNetUtilsHelper.CloneEnumWbemClassObject CloneEnumWbemClassObject_f;

		internal static WmiNetUtilsHelper.ConnectServerWmi ConnectServerWmi_f;

		static WmiNetUtilsHelper ()
		{
			if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix) {
				WmiNetUtilsHelper.ResetSecurity_f = UnixWmiNetUtils.ResetSecurity;

				WmiNetUtilsHelper.SetSecurity_f = UnixWmiNetUtils.SetSecurity;

				WmiNetUtilsHelper.BlessIWbemServices_f = UnixWmiNetUtils.BlessIWbemServices;

				WmiNetUtilsHelper.BlessIWbemServicesObject_f = UnixWmiNetUtils.BlessIWbemServicesObject;

				WmiNetUtilsHelper.GetPropertyHandle_f27 = UnixWmiNetUtils.GetPropertyHandle;

				WmiNetUtilsHelper.WritePropertyValue_f28 = UnixWmiNetUtils.WritePropertyValue;

				WmiNetUtilsHelper.Clone_f12 = UnixWmiNetUtils.Clone;

				WmiNetUtilsHelper.VerifyClientKey_f = UnixWmiNetUtils.VerifyClientKey;

				WmiNetUtilsHelper.GetQualifierSet_f = UnixWmiNetUtils.GetQualifierSet;

				WmiNetUtilsHelper.Get_f = UnixWmiNetUtils.Get;

				WmiNetUtilsHelper.Put_f = UnixWmiNetUtils.Put;

				WmiNetUtilsHelper.Delete_f = UnixWmiNetUtils.Delete;

				WmiNetUtilsHelper.GetNames_f = UnixWmiNetUtils.GetNames;

				WmiNetUtilsHelper.BeginEnumeration_f = UnixWmiNetUtils.BeginEnumeration;

				WmiNetUtilsHelper.Next_f = UnixWmiNetUtils.Next;

				WmiNetUtilsHelper.EndEnumeration_f = UnixWmiNetUtils.EndEnumeration;

				WmiNetUtilsHelper.GetPropertyQualifierSet_f = UnixWmiNetUtils.GetPropertyQualifierSet;

				WmiNetUtilsHelper.Clone_f = UnixWmiNetUtils.Clone;

				WmiNetUtilsHelper.GetObjectText_f = UnixWmiNetUtils.GetObjectText;

				WmiNetUtilsHelper.SpawnDerivedClass_f = UnixWmiNetUtils.SpawnDerivedClass;

				WmiNetUtilsHelper.SpawnInstance_f = UnixWmiNetUtils.SpawnInstance;

				WmiNetUtilsHelper.CompareTo_f = UnixWmiNetUtils.CompareTo;

				WmiNetUtilsHelper.GetPropertyOrigin_f = UnixWmiNetUtils.GetPropertyOrigin;

				WmiNetUtilsHelper.InheritsFrom_f = UnixWmiNetUtils.InheritsFrom;

				WmiNetUtilsHelper.GetMethod_f = UnixWmiNetUtils.GetMethod;

				WmiNetUtilsHelper.PutMethod_f = UnixWmiNetUtils.PutMethod;

				WmiNetUtilsHelper.DeleteMethod_f = UnixWmiNetUtils.DeleteMethod;

				WmiNetUtilsHelper.BeginMethodEnumeration_f = UnixWmiNetUtils.BeginMethodEnumeration;

				WmiNetUtilsHelper.NextMethod_f = UnixWmiNetUtils.NextMethod;

				WmiNetUtilsHelper.EndMethodEnumeration_f = UnixWmiNetUtils.EndMethodEnumeration;

				WmiNetUtilsHelper.GetMethodQualifierSet_f = UnixWmiNetUtils.GetMethodQualifierSet;

				WmiNetUtilsHelper.GetMethodOrigin_f = UnixWmiNetUtils.GetMethodOrigin;

				WmiNetUtilsHelper.QualifierGet_f = UnixWmiNetUtils.QualifierSet_Get;

				WmiNetUtilsHelper.QualifierPut_f = UnixWmiNetUtils.QualifierSet_Put;

				WmiNetUtilsHelper.QualifierDelete_f = UnixWmiNetUtils.QualifierSet_Delete;

				WmiNetUtilsHelper.QualifierGetNames_f = UnixWmiNetUtils.QualifierSet_GetNames;

				WmiNetUtilsHelper.QualifierBeginEnumeration_f = UnixWmiNetUtils.QualifierSet_BeginEnumeration;

				WmiNetUtilsHelper.QualifierNext_f = UnixWmiNetUtils.QualifierSet_Next;

				WmiNetUtilsHelper.QualifierEndEnumeration_f = UnixWmiNetUtils.QualifierSet_EndEnumeration;

				WmiNetUtilsHelper.GetCurrentApartmentType_f = UnixWmiNetUtils.GetCurrentApartmentType;

				WmiNetUtilsHelper.GetDemultiplexedStub_f = UnixWmiNetUtils.GetDemultiplexedStub;

				WmiNetUtilsHelper.CreateInstanceEnumWmi_f = UnixWmiNetUtils.CreateInstanceEnumWmi;

				WmiNetUtilsHelper.CreateClassEnumWmi_f = UnixWmiNetUtils.CreateClassEnumWmi;

				WmiNetUtilsHelper.ExecQueryWmi_f = UnixWmiNetUtils.ExecQueryWmi;

				WmiNetUtilsHelper.ExecNotificationQueryWmi_f = UnixWmiNetUtils.ExecNotificationQueryWmi;

				WmiNetUtilsHelper.PutInstanceWmi_f = UnixWmiNetUtils.PutInstanceWmi;

				WmiNetUtilsHelper.PutClassWmi_f = UnixWmiNetUtils.PutClassWmi;

				WmiNetUtilsHelper.CloneEnumWbemClassObject_f = UnixWmiNetUtils.CloneEnumWbemClassObject;

				WmiNetUtilsHelper.ConnectServerWmi_f = UnixWmiNetUtils.ConnectServerWmi;

			} else {
				WmiNetUtilsHelper.myDllPath = string.Concat (RuntimeEnvironment.GetRuntimeDirectory (), "\\wminet_utils.dll");
				IntPtr intPtr = WmiNetUtilsHelper.LoadLibrary (WmiNetUtilsHelper.myDllPath);
				if (intPtr != IntPtr.Zero) {
					IntPtr procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "ResetSecurity");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.ResetSecurity_f = (WmiNetUtilsHelper.ResetSecurity)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.ResetSecurity));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "SetSecurity");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.SetSecurity_f = (WmiNetUtilsHelper.SetSecurity)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.SetSecurity));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "BlessIWbemServices");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.BlessIWbemServices_f = (WmiNetUtilsHelper.BlessIWbemServices)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.BlessIWbemServices));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "BlessIWbemServicesObject");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.BlessIWbemServicesObject_f = (WmiNetUtilsHelper.BlessIWbemServicesObject)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.BlessIWbemServicesObject));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetPropertyHandle");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetPropertyHandle_f27 = (WmiNetUtilsHelper.GetPropertyHandle)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetPropertyHandle));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "WritePropertyValue");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.WritePropertyValue_f28 = (WmiNetUtilsHelper.WritePropertyValue)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.WritePropertyValue));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "Clone");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.Clone_f12 = (WmiNetUtilsHelper.Clone)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.Clone));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "VerifyClientKey");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.VerifyClientKey_f = (WmiNetUtilsHelper.VerifyClientKey)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.VerifyClientKey));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetQualifierSet");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetQualifierSet_f = (WmiNetUtilsHelper.GetQualifierSet)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetQualifierSet));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "Get");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.Get_f = (WmiNetUtilsHelper.Get)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.Get));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "Put");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.Put_f = (WmiNetUtilsHelper.Put)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.Put));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "Delete");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.Delete_f = (WmiNetUtilsHelper.Delete)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.Delete));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetNames");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetNames_f = (WmiNetUtilsHelper.GetNames)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetNames));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "BeginEnumeration");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.BeginEnumeration_f = (WmiNetUtilsHelper.BeginEnumeration)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.BeginEnumeration));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "Next");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.Next_f = (WmiNetUtilsHelper.Next)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.Next));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "EndEnumeration");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.EndEnumeration_f = (WmiNetUtilsHelper.EndEnumeration)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.EndEnumeration));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetPropertyQualifierSet");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetPropertyQualifierSet_f = (WmiNetUtilsHelper.GetPropertyQualifierSet)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetPropertyQualifierSet));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "Clone");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.Clone_f = (WmiNetUtilsHelper.Clone)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.Clone));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetObjectText");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetObjectText_f = (WmiNetUtilsHelper.GetObjectText)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetObjectText));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "SpawnDerivedClass");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.SpawnDerivedClass_f = (WmiNetUtilsHelper.SpawnDerivedClass)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.SpawnDerivedClass));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "SpawnInstance");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.SpawnInstance_f = (WmiNetUtilsHelper.SpawnInstance)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.SpawnInstance));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "CompareTo");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.CompareTo_f = (WmiNetUtilsHelper.CompareTo)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.CompareTo));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetPropertyOrigin");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetPropertyOrigin_f = (WmiNetUtilsHelper.GetPropertyOrigin)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetPropertyOrigin));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "InheritsFrom");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.InheritsFrom_f = (WmiNetUtilsHelper.InheritsFrom)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.InheritsFrom));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetMethod");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetMethod_f = (WmiNetUtilsHelper.GetMethod)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetMethod));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "PutMethod");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.PutMethod_f = (WmiNetUtilsHelper.PutMethod)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.PutMethod));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "DeleteMethod");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.DeleteMethod_f = (WmiNetUtilsHelper.DeleteMethod)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.DeleteMethod));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "BeginMethodEnumeration");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.BeginMethodEnumeration_f = (WmiNetUtilsHelper.BeginMethodEnumeration)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.BeginMethodEnumeration));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "NextMethod");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.NextMethod_f = (WmiNetUtilsHelper.NextMethod)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.NextMethod));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "EndMethodEnumeration");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.EndMethodEnumeration_f = (WmiNetUtilsHelper.EndMethodEnumeration)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.EndMethodEnumeration));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetMethodQualifierSet");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetMethodQualifierSet_f = (WmiNetUtilsHelper.GetMethodQualifierSet)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetMethodQualifierSet));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetMethodOrigin");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetMethodOrigin_f = (WmiNetUtilsHelper.GetMethodOrigin)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetMethodOrigin));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "QualifierSet_Get");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.QualifierGet_f = (WmiNetUtilsHelper.QualifierSet_Get)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.QualifierSet_Get));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "QualifierSet_Put");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.QualifierPut_f = (WmiNetUtilsHelper.QualifierSet_Put)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.QualifierSet_Put));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "QualifierSet_Delete");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.QualifierDelete_f = (WmiNetUtilsHelper.QualifierSet_Delete)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.QualifierSet_Delete));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "QualifierSet_GetNames");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.QualifierGetNames_f = (WmiNetUtilsHelper.QualifierSet_GetNames)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.QualifierSet_GetNames));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "QualifierSet_BeginEnumeration");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.QualifierBeginEnumeration_f = (WmiNetUtilsHelper.QualifierSet_BeginEnumeration)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.QualifierSet_BeginEnumeration));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "QualifierSet_Next");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.QualifierNext_f = (WmiNetUtilsHelper.QualifierSet_Next)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.QualifierSet_Next));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "QualifierSet_EndEnumeration");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.QualifierEndEnumeration_f = (WmiNetUtilsHelper.QualifierSet_EndEnumeration)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.QualifierSet_EndEnumeration));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetCurrentApartmentType");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetCurrentApartmentType_f = (WmiNetUtilsHelper.GetCurrentApartmentType)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetCurrentApartmentType));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "GetDemultiplexedStub");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.GetDemultiplexedStub_f = (WmiNetUtilsHelper.GetDemultiplexedStub)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.GetDemultiplexedStub));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "CreateInstanceEnumWmi");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.CreateInstanceEnumWmi_f = (WmiNetUtilsHelper.CreateInstanceEnumWmi)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.CreateInstanceEnumWmi));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "CreateClassEnumWmi");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.CreateClassEnumWmi_f = (WmiNetUtilsHelper.CreateClassEnumWmi)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.CreateClassEnumWmi));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "ExecQueryWmi");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.ExecQueryWmi_f = (WmiNetUtilsHelper.ExecQueryWmi)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.ExecQueryWmi));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "ExecNotificationQueryWmi");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.ExecNotificationQueryWmi_f = (WmiNetUtilsHelper.ExecNotificationQueryWmi)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.ExecNotificationQueryWmi));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "PutInstanceWmi");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.PutInstanceWmi_f = (WmiNetUtilsHelper.PutInstanceWmi)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.PutInstanceWmi));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "PutClassWmi");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.PutClassWmi_f = (WmiNetUtilsHelper.PutClassWmi)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.PutClassWmi));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "CloneEnumWbemClassObject");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.CloneEnumWbemClassObject_f = (WmiNetUtilsHelper.CloneEnumWbemClassObject)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.CloneEnumWbemClassObject));
					}
					procAddress = WmiNetUtilsHelper.GetProcAddress (intPtr, "ConnectServerWmi");
					if (procAddress != IntPtr.Zero) {
						WmiNetUtilsHelper.ConnectServerWmi_f = (WmiNetUtilsHelper.ConnectServerWmi)Marshal.GetDelegateForFunctionPointer (procAddress, typeof(WmiNetUtilsHelper.ConnectServerWmi));
					}
				}
			}
		}

		/*
		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SuppressUnmanagedCodeSecurity]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, string procname);
		
		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SuppressUnmanagedCodeSecurity]
		internal static extern IntPtr LoadLibrary(string fileName);
		*/

		[SuppressUnmanagedCodeSecurity]
		internal static IntPtr GetProcAddress(IntPtr hModule, string procname)
		{
			return IntPtr.Zero;
		}
		
		[SuppressUnmanagedCodeSecurity]
		internal static IntPtr LoadLibrary(string fileName)
		{
			return IntPtr.Zero;
		}

		internal enum APTTYPE
		{
			APTTYPE_CURRENT = -1,
			APTTYPE_STA = 0,
			APTTYPE_MTA = 1,
			APTTYPE_NA = 2,
			APTTYPE_MAINSTA = 3
		}

		internal delegate int BeginEnumeration(int vFunc, IntPtr pWbemClassObject, int lEnumFlags);

		internal delegate int BeginMethodEnumeration(int vFunc, IntPtr pWbemClassObject, int lEnumFlags);

		internal delegate int BlessIWbemServices(IWbemServices pIUnknown, string strUser, IntPtr password, string strAuthority, int impersonationLevel, int authenticationLevel);

		internal delegate int BlessIWbemServicesObject(object pIUnknown, string strUser, IntPtr password, string strAuthority, int impersonationLevel, int authenticationLevel);

		internal delegate int Clone(int vFunc, IntPtr pWbemClassObject, out IntPtr ppCopy);

		internal delegate int CloneEnumWbemClassObject(out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IEnumWbemClassObject pCurrentEnumWbemClassObject, string strUser, IntPtr strPassword, string strAuthority);

		internal delegate int CompareTo(int vFunc, IntPtr pWbemClassObject, int lFlags, IntPtr pCompareTo);

		internal delegate int ConnectServerWmi(string strNetworkResource, string strUser, IntPtr strPassword, string strLocale, int lSecurityFlags, string strAuthority, IWbemContext pCtx, out IWbemServices ppNamespace, int impersonationLevel, int authenticationLevel);

		internal delegate int CreateClassEnumWmi(string strSuperclass, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority);

		internal delegate int CreateInstanceEnumWmi(string strFilter, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority);

		internal delegate int Delete(int vFunc, IntPtr pWbemClassObject, string wszName);

		internal delegate int DeleteMethod(int vFunc, IntPtr pWbemClassObject, string wszName);

		internal delegate int EndEnumeration(int vFunc, IntPtr pWbemClassObject);

		internal delegate int EndMethodEnumeration(int vFunc, IntPtr pWbemClassObject);

		internal delegate int ExecNotificationQueryWmi(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority);

		internal delegate int ExecQueryWmi(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority);

		internal delegate int Get(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, out object pVal, out int pType, out int plFlavor);

		internal delegate int GetCurrentApartmentType(int vFunc, IntPtr pComThreadingInfo, out WmiNetUtilsHelper.APTTYPE aptType);

		internal delegate int GetDemultiplexedStub(object pIUnknown, bool isLocal, out object ppIUnknown);

		internal delegate int GetMethod(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, out IntPtr ppInSignature, out IntPtr ppOutSignature);

		internal delegate int GetMethodOrigin(int vFunc, IntPtr pWbemClassObject, string wszMethodName, out string pstrClassName);

		internal delegate int GetMethodQualifierSet(int vFunc, IntPtr pWbemClassObject, string wszMethod, out IntPtr ppQualSet);

		internal delegate int GetNames(int vFunc, IntPtr pWbemClassObject, string wszQualifierName, int lFlags, ref object pQualifierVal, out string[] pNames);

		internal delegate int GetObjectText(int vFunc, IntPtr pWbemClassObject, int lFlags, out string pstrObjectText);

		internal delegate int GetPropertyHandle(int vFunc, IntPtr pWbemClassObject, string wszPropertyName, out int pType, out int plHandle);

		internal delegate int GetPropertyOrigin(int vFunc, IntPtr pWbemClassObject, string wszName, out string pstrClassName);

		internal delegate int GetPropertyQualifierSet(int vFunc, IntPtr pWbemClassObject, string wszProperty, out IntPtr ppQualSet);

		internal delegate int GetQualifierSet(int vFunc, IntPtr pWbemClassObject, out IntPtr ppQualSet);

		internal delegate int InheritsFrom(int vFunc, IntPtr pWbemClassObject, string strAncestor);

		internal delegate int Next(int vFunc, IntPtr pWbemClassObject, int lFlags, out string strName, out object pVal, out int pType, out int plFlavor);

		internal delegate int NextMethod(int vFunc, IntPtr pWbemClassObject, int lFlags, out string pstrName, out IntPtr ppInSignature, out IntPtr ppOutSignature);

		internal delegate int Put(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, ref object pVal, int Type);

		internal delegate int PutClassWmi(IntPtr pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority);

		internal delegate int PutInstanceWmi(IntPtr pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority);

		internal delegate int PutMethod(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, IntPtr pInSignature, IntPtr pOutSignature);

		internal delegate int QualifierSet_BeginEnumeration(int vFunc, IntPtr pWbemClassObject, int lFlags);

		internal delegate int QualifierSet_Delete(int vFunc, IntPtr pWbemClassObject, string wszName);

		internal delegate int QualifierSet_EndEnumeration(int vFunc, IntPtr pWbemClassObject);

		internal delegate int QualifierSet_Get(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, out object pVal, out int plFlavor);

		internal delegate int QualifierSet_GetNames(int vFunc, IntPtr pWbemClassObject, int lFlags, out string[] pNames);

		internal delegate int QualifierSet_Next(int vFunc, IntPtr pWbemClassObject, int lFlags, out string pstrName, out object pVal, out int plFlavor);

		internal delegate int QualifierSet_Put(int vFunc, IntPtr pWbemClassObject, string wszName, ref object pVal, int lFlavor);

		internal delegate int ResetSecurity(IntPtr hToken);

		internal delegate int SetSecurity(out bool pNeedtoReset, out IntPtr pHandle);

		internal delegate int SpawnDerivedClass(int vFunc, IntPtr pWbemClassObject, int lFlags, out IntPtr ppNewClass);

		internal delegate int SpawnInstance(int vFunc, IntPtr pWbemClassObject, int lFlags, out IntPtr ppNewInstance);

		internal delegate void VerifyClientKey();

		internal delegate int WritePropertyValue(int vFunc, IntPtr pWbemClassObject, int lHandle, int lNumBytes, string str);
	}
}