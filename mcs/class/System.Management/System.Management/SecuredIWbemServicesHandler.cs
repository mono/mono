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

namespace System.Management
{
	internal class SecuredIWbemServicesHandler
	{
		private IWbemServices pWbemServiecsSecurityHelper;

		private ManagementScope scope;

		internal SecuredIWbemServicesHandler(ManagementScope theScope, IWbemServices pWbemServiecs)
		{
			this.scope = theScope;
			this.pWbemServiecsSecurityHelper = pWbemServiecs;
		}

		internal int CancelAsyncCall_(IWbemObjectSink pSink)
		{
			int num = this.pWbemServiecsSecurityHelper.CancelAsyncCall_(pSink);
			return num;
		}

		internal int CreateClassEnum_(string strSuperClass, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.CreateClassEnumWmi_f(strSuperClass, lFlags, pCtx, out ppEnum, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int CreateClassEnumAsync_(string strSuperClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.CreateClassEnumAsync_(strSuperClass, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int CreateInstanceEnum_(string strFilter, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.CreateInstanceEnumWmi_f(strFilter, lFlags, pCtx, out ppEnum, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int CreateInstanceEnumAsync_(string strFilter, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.CreateInstanceEnumAsync_(strFilter, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int DeleteClass_(string strClass, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			int num = -2147217407;
			if (!object.ReferenceEquals(ppCallResult, IntPtr.Zero))
			{
				num = this.pWbemServiecsSecurityHelper.DeleteClass_(strClass, lFlags, pCtx, ppCallResult);
			}
			return num;
		}

		internal int DeleteClassAsync_(string strClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.DeleteClassAsync_(strClass, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int DeleteInstance_(string strObjectPath, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			int num = -2147217407;
			if (!object.ReferenceEquals(ppCallResult, IntPtr.Zero))
			{
				num = this.pWbemServiecsSecurityHelper.DeleteInstance_(strObjectPath, lFlags, pCtx, ppCallResult);
			}
			return num;
		}

		internal int DeleteInstanceAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.DeleteInstanceAsync_(strObjectPath, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int ExecMethod_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObjectFreeThreaded pInParams, ref IWbemClassObjectFreeThreaded ppOutParams, IntPtr ppCallResult)
		{
			int num = -2147217407;
			if (!object.ReferenceEquals(ppCallResult, IntPtr.Zero))
			{
				num = this.pWbemServiecsSecurityHelper.ExecMethod_(strObjectPath, strMethodName, lFlags, pCtx, pInParams, out ppOutParams, ppCallResult);
			}
			return num;
		}

		internal int ExecMethodAsync_(string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IWbemClassObjectFreeThreaded pInParams, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.ExecMethodAsync_(strObjectPath, strMethodName, lFlags, pCtx, pInParams, pResponseHandler);
			return num;
		}

		internal int ExecNotificationQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.ExecNotificationQueryWmi_f(strQueryLanguage, strQuery, lFlags, pCtx, out ppEnum, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int ExecNotificationQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.ExecNotificationQueryAsync_(strQueryLanguage, strQuery, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int ExecQuery_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, ref IEnumWbemClassObject ppEnum)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.ExecQueryWmi_f(strQueryLanguage, strQuery, lFlags, pCtx, out ppEnum, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int ExecQueryAsync_(string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.ExecQueryAsync_(strQueryLanguage, strQuery, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int GetObject_(string strObjectPath, int lFlags, IWbemContext pCtx, ref IWbemClassObjectFreeThreaded ppObject, IntPtr ppCallResult)
		{
			int object_ = -2147217407;
			if (ppObject == null)  //TODO: REVIEW && !object.ReferenceEquals(ppCallResult, IntPtr.Zero))
			{
				object_ = this.pWbemServiecsSecurityHelper.GetObject_(strObjectPath, lFlags, pCtx, out ppObject, ppCallResult);
			}
			return object_;
		}

		internal int GetObjectAsync_(string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int objectAsync_ = this.pWbemServiecsSecurityHelper.GetObjectAsync_(strObjectPath, lFlags, pCtx, pResponseHandler);
			return objectAsync_;
		}

		internal int OpenNamespace_(string strNamespace, int lFlags, ref IWbemServices ppWorkingNamespace, IntPtr ppCallResult)
		{
			int num = -2147217396;
			return num;
		}

		internal int PutClass_(IWbemClassObjectFreeThreaded pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.PutClassWmi_f(pObject, lFlags, pCtx, ppCallResult, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int PutClassAsync_(IWbemClassObjectFreeThreaded pObject, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.PutClassAsync_(pObject, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int PutInstance_(IWbemClassObjectFreeThreaded pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			int num = -2147217407;
			if (this.scope != null)
			{
				num = WmiNetUtilsHelper.PutInstanceWmi_f(pInst, lFlags, pCtx, ppCallResult, (int)this.scope.Options.Authentication, (int)this.scope.Options.Impersonation, this.pWbemServiecsSecurityHelper, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Authority);
			}
			return num;
		}

		internal int PutInstanceAsync_(IWbemClassObjectFreeThreaded pInst, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.PutInstanceAsync_(pInst, lFlags, pCtx, pResponseHandler);
			return num;
		}

		internal int QueryObjectSink_(int lFlags, ref IWbemObjectSink ppResponseHandler)
		{
			int num = this.pWbemServiecsSecurityHelper.QueryObjectSink_(lFlags, out ppResponseHandler);
			return num;
		}
	}
}