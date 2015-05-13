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
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace System.Management
{
	public static class UnixWmiNetUtils
	{
		internal static int BeginEnumeration(int vFunc, IntPtr pWbemClassObject, int lEnumFlags)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.BeginEnumeration_ (lEnumFlags);
		}
		
		internal static int BeginMethodEnumeration(int vFunc, IntPtr pWbemClassObject, int lEnumFlags)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.BeginMethodEnumeration_ (lEnumFlags);
		}
		
		internal static int BlessIWbemServices(IWbemServices pIUnknown, string strUser, IntPtr password, string strAuthority, int impersonationLevel, int authenticationLevel)
		{
			return 0;
		}
		
		internal static int BlessIWbemServicesObject(object pIUnknown, string strUser, IntPtr password, string strAuthority, int impersonationLevel, int authenticationLevel)
		{
			return 0;
		}
		
		internal static int Clone(int vFunc, IntPtr pWbemClassObject, out IntPtr ppCopy)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			IWbemClassObject_DoNotMarshal result = null;
			int ret = obj.Clone_ (out result);
			ppCopy = UnixWbemClassObject.ToPointer (result);
			return ret;
		}
		
		internal static int CloneEnumWbemClassObject(out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IEnumWbemClassObject pCurrentEnumWbemClassObject, string strUser, IntPtr strPassword, string strAuthority)
		{
			ppEnum = pCurrentEnumWbemClassObject;
			return 0;
		}
		
		internal static int CompareTo(int vFunc, IntPtr pWbemClassObject, int lFlags, IntPtr pCompareTo)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			UnixWbemClassObject obj2 = UnixWbemClassObject.ToManaged (pCompareTo);
			return obj.CompareTo_ (lFlags, obj2);
		}
		
		internal static int ConnectServerWmi(string strNetworkResource, string strUser, IntPtr strPassword, string strLocale, int lSecurityFlags, string strAuthority, IWbemContext pCtx, out IWbemServices ppNamespace, int impersonationLevel, int authenticationLevel)
		{
			ppNamespace = new UnixWbemServices();
			return 0;
		}
		
		internal static int CreateClassEnumWmi(string strSuperclass, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority)
		{
			var svc = pCurrentNamespace as UnixWbemServices;
			var items = WbemClientFactory.Get (svc.CurrentNamespace, "SELECT * FROM " + strSuperclass);
			ppEnum = new UnixEnumWbemClassObject(items);
			return 0;
		}
		
		internal static int CreateInstanceEnumWmi(string strFilter, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority)
		{
			var svc = pCurrentNamespace as UnixWbemServices;
			var items = WbemClientFactory.Get (svc.CurrentNamespace, "SELECT * FROM " + strFilter);

			ppEnum = new UnixEnumWbemClassObject(items);
			return 0;
		}
		
		internal static int Delete(int vFunc, IntPtr pWbemClassObject, string wszName)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			obj.Delete_ (wszName);
			return 0;
		}
		
		internal static int DeleteMethod(int vFunc, IntPtr pWbemClassObject, string wszName)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.DeleteMethod_ (wszName);
		}
		
		internal static int EndEnumeration(int vFunc, IntPtr pWbemClassObject)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.EndEnumeration_ ();
		}
		
		internal static int EndMethodEnumeration(int vFunc, IntPtr pWbemClassObject)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.EndMethodEnumeration_ ();
		}
		
		internal static int ExecNotificationQueryWmi(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority)
		{
			ppEnum = null;
			return 0;
		}
		
		internal static int ExecQueryWmi(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority)
		{
			var svc = pCurrentNamespace as UnixWbemServices;
			IEnumerable<IWbemClassObject_DoNotMarshal> list = WbemClientFactory.Get(svc.CurrentNamespace, strQuery);
			ppEnum = new UnixEnumWbemClassObject(list);
			return 0;
		}
		
		internal static int Get (int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, out object pVal, out int pType, out int plFlavor)
		{
			UnixWbemClassObject target = UnixWbemClassObject.ToManaged (pWbemClassObject);
			pVal = null;
			pType = (int)CimType.Object;
			plFlavor = 0;
			int ret = 0;
			if (target != null) {
				ret = target.Get_ (wszName, lFlags, out pVal, out pType, out plFlavor);
			}
			return ret;
		}
		
		internal static int GetCurrentApartmentType(int vFunc, IntPtr pComThreadingInfo, out WmiNetUtilsHelper.APTTYPE aptType)
		{
			aptType  = WmiNetUtilsHelper.APTTYPE.APTTYPE_CURRENT;
			return 0;
		}
		
		internal static int GetDemultiplexedStub(object pIUnknown, bool isLocal, out object ppIUnknown)
		{
			ppIUnknown = pIUnknown;
			return 0;
		}
		
		internal static int GetMethod(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, out IntPtr ppInSignature, out IntPtr ppOutSignature)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			IWbemClassObject_DoNotMarshal inSign = null;
			IWbemClassObject_DoNotMarshal outSign = null;
			int ret = obj.GetMethod_ (wszName, lFlags, out inSign, out outSign);
			ppInSignature = UnixWbemClassObject.ToPointer (inSign);
			ppOutSignature = UnixWbemClassObject.ToPointer (outSign);
			return ret;
		}
		
		internal static int GetMethodOrigin(int vFunc, IntPtr pWbemClassObject, string wszMethodName, out string pstrClassName)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.GetMethodOrigin_ (wszMethodName, out pstrClassName);
		}
		
		internal static int GetMethodQualifierSet(int vFunc, IntPtr pWbemClassObject, string wszMethod, out IntPtr ppQualSet)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			IWbemQualifierSet_DoNotMarshal result = null;
			int ret = obj.GetMethodQualifierSet_(wszMethod, out result);
			ppQualSet = UnixWbemObjectQualifierSet.ToPointer (result);
			return ret;
		}
		
		internal static int GetNames(int vFunc, IntPtr pWbemClassObject, string wszQualifierName, int lFlags, ref object pQualifierVal, out string[] pNames)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.GetNames_ (wszQualifierName, lFlags, ref pQualifierVal, out pNames);
		}
		
		internal static int GetObjectText(int vFunc, IntPtr pWbemClassObject, int lFlags, out string pstrObjectText)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.GetObjectText_ (lFlags, out pstrObjectText);
		}
		
		internal static int GetPropertyHandle(int vFunc, IntPtr pWbemClassObject, string wszPropertyName, out int pType, out int plHandle)
		{
			pType = (int)CimType.String;
			plHandle = 0;
			return 0;
		}
		
		internal static int GetPropertyOrigin(int vFunc, IntPtr pWbemClassObject, string wszName, out string pstrClassName)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.GetPropertyOrigin_ (wszName, out pstrClassName);
		}
		
		internal static int GetPropertyQualifierSet(int vFunc, IntPtr pWbemClassObject, string wszProperty, out IntPtr ppQualSet)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			IWbemQualifierSet_DoNotMarshal result = null;
			obj.GetPropertyQualifierSet_ (wszProperty, out result);
			ppQualSet = UnixWbemObjectQualifierSet.ToPointer (result);
			return 0;
		}
		
		internal static int GetQualifierSet(int vFunc, IntPtr pWbemClassObject, out IntPtr ppQualSet)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			IWbemQualifierSet_DoNotMarshal qualifiersObj = null;
			int ret = obj.GetQualifierSet_ (out qualifiersObj);
			ppQualSet = UnixWbemObjectQualifierSet.ToPointer (qualifiersObj);
			return ret;
		}
		
		internal static int InheritsFrom(int vFunc, IntPtr pWbemClassObject, string strAncestor)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.InheritsFrom_ (strAncestor);
		}
		
		internal static int Next(int vFunc, IntPtr pWbemClassObject, int lFlags, out string strName, out object pVal, out int pType, out int plFlavor)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.Next_ (lFlags, out strName, out pVal, out pType, out plFlavor);
		}
		
		internal static int NextMethod (int vFunc, IntPtr pWbemClassObject, int lFlags, out string pstrName, out IntPtr ppInSignature, out IntPtr ppOutSignature)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			IWbemClassObject_DoNotMarshal inSign = null;
			IWbemClassObject_DoNotMarshal outSign = null;
			int ret = obj.NextMethod_ (lFlags, out pstrName, out inSign, out outSign);
			if (ret >= 0 && inSign != null && outSign != null) {
				ppInSignature = UnixWbemClassObject.ToPointer (inSign);
				ppOutSignature = UnixWbemClassObject.ToPointer (outSign);
			}
			else {
				ppInSignature =  IntPtr.Zero;
				ppOutSignature = IntPtr.Zero;
			}
			return ret;
		}
		
		internal static int Put(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, ref object pVal, int Type)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			return obj.Put_ (wszName, lFlags, ref pVal, Type);
		}
		
		internal static int PutClassWmi(IntPtr pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority)
		{
			return pCurrentNamespace.PutClass_ (pObject, lFlags, pCtx, ppCallResult);
		}
		
		internal static int PutInstanceWmi(IntPtr pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult, int impLevel, int authnLevel, IWbemServices pCurrentNamespace, string strUser, IntPtr strPassword, string strAuthority)
		{
			return pCurrentNamespace.PutInstance_ (pInst, lFlags, pCtx, ppCallResult);
		}
		
		internal static int PutMethod(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, IntPtr pInSignature, IntPtr pOutSignature)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			UnixWbemClassObject inSign = UnixWbemClassObject.ToManaged (pInSignature);
			UnixWbemClassObject outSign =  UnixWbemClassObject.ToManaged (pOutSignature);
			return obj.PutMethod_ (wszName, lFlags, inSign, outSign);
		}
		
		internal static int QualifierSet_BeginEnumeration(int vFunc, IntPtr pWbemClassObject, int lFlags)
		{
			UnixWbemObjectQualifierSet obj = UnixWbemObjectQualifierSet.ToManaged (pWbemClassObject);
			return obj.BeginEnumeration_(lFlags);
		}
		
		internal static int QualifierSet_Delete(int vFunc, IntPtr pWbemClassObject, string wszName)
		{
			UnixWbemObjectQualifierSet obj = UnixWbemObjectQualifierSet.ToManaged (pWbemClassObject);
			return obj.Delete_ (wszName);
		}
		
		internal static int QualifierSet_EndEnumeration(int vFunc, IntPtr pWbemClassObject)
		{
			UnixWbemObjectQualifierSet obj = UnixWbemObjectQualifierSet.ToManaged (pWbemClassObject);
			return obj.EndEnumeration_ ();
		}
		
		internal static int QualifierSet_Get(int vFunc, IntPtr pWbemClassObject, string wszName, int lFlags, out object pVal, out int plFlavor)
		{
			UnixWbemObjectQualifierSet obj = UnixWbemObjectQualifierSet.ToManaged (pWbemClassObject);
			return obj.Get_ (wszName, lFlags, out pVal, out plFlavor);
		}
		
		internal static int QualifierSet_GetNames (int vFunc, IntPtr pWbemClassObject, int lFlags, out string[] pNames)
		{
			if (vFunc == 3 || vFunc == 6) {
				/* 3 = Property - 6 = Object */
				UnixWbemObjectQualifierSet obj = UnixWbemObjectQualifierSet.ToManaged (pWbemClassObject);
				return obj.GetNames_ (lFlags, out pNames);
			}
			pNames = new string[0];
			return 0;
		}
		
		internal static int QualifierSet_Next(int vFunc, IntPtr pWbemClassObject, int lFlags, out string pstrName, out object pVal, out int plFlavor)
		{
			UnixWbemObjectQualifierSet obj = UnixWbemObjectQualifierSet.ToManaged (pWbemClassObject);
			return obj.Next_ (lFlags, out pstrName, out pVal, out plFlavor);
		}
		
		internal static int QualifierSet_Put(int vFunc, IntPtr pWbemClassObject, string wszName, ref object pVal, int lFlavor)
		{
			UnixWbemObjectQualifierSet obj = UnixWbemObjectQualifierSet.ToManaged (pWbemClassObject);
			return obj.Put_ (wszName, ref pVal, lFlavor);
		}
		
		internal static int ResetSecurity(IntPtr hToken)
		{
			return 0;
		}
		
		internal static int SetSecurity(out bool pNeedtoReset, out IntPtr pHandle)
		{
			pNeedtoReset = false;
			pHandle = IntPtr.Zero;
			return 0;
		}
		
		internal static int SpawnDerivedClass(int vFunc, IntPtr pWbemClassObject, int lFlags, out IntPtr ppNewClass)
		{
			ppNewClass = pWbemClassObject;
			return 0;
		}
		
		internal static int SpawnInstance(int vFunc, IntPtr pWbemClassObject, int lFlags, out IntPtr ppNewInstance)
		{
			ppNewInstance = pWbemClassObject;
			return 0;
		}
		
		internal static void VerifyClientKey()
		{
			/* TODO: Where is the client key */
		}
		
		internal static int WritePropertyValue(int vFunc, IntPtr pWbemClassObject, int lHandle, int lNumBytes, string str)
		{
			UnixWbemClassObject obj = UnixWbemClassObject.ToManaged (pWbemClassObject);
			/** TODO: How to do this !!! */
			return 0;
		}
	}
}
