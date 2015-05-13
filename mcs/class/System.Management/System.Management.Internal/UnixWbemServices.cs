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
using System.Collections.Generic;
using System.Linq;
namespace System.Management
{
	internal class UnixWbemServices : IWbemServices
	{
		private string _currentNamespace;

		internal UnixWbemServices ()
			: this(CimNamespaces.CimV2)
		{

		}

		internal UnixWbemServices (string nameSpace)
		{
			_currentNamespace = nameSpace;
		}

		internal string CurrentNamespace {
			get { return _currentNamespace; }
		}

		#region IWbemServices implementation

		public int CancelAsyncCall_ (IWbemObjectSink pSink)
		{
			return 0;
		}

		public int CreateClassEnum_ (string strSuperclass, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = null;
			return 0;
		}

		public int CreateClassEnumAsync_ (string strSuperclass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int CreateInstanceEnum_ (string strFilter, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			var items = WbemClientFactory.Get (_currentNamespace, strFilter);
			ppEnum = new UnixEnumWbemClassObject(items);
			return 0;
		}

		public int CreateInstanceEnumAsync_ (string strFilter, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int DeleteClass_ (string strClass, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return 0;
		}

		public int DeleteClassAsync_ (string strClass, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int DeleteInstance_ (string strObjectPath, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
			return 0;
		}

		public int DeleteInstanceAsync_ (string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
			return 0;
		}

		public int ExecMethod_ (string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IntPtr pInParams, out IWbemClassObjectFreeThreaded ppOutParams, IntPtr ppCallResult)
		{
			int result = 0;
			ppOutParams = null;
			object outParams = null;
			var inParams = UnixWbemClassObject.ToManaged (pInParams);
			if (strObjectPath.Contains ("="))
			{
				string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
				var items = WbemClientFactory.Get (_currentNamespace, strQuery);
				foreach (var obj in items) 
				{
					result = obj.ExecuteMethod_ (strMethodName, inParams, out outParams);
					IntPtr outPtr = UnixWbemClassObject.ToPointer ((IWbemClassObject_DoNotMarshal)outParams);
					ppOutParams = new IWbemClassObjectFreeThreaded(outPtr);
				}
			}
			else {
				var obj = WbemClientFactory.Get(strObjectPath);
				result = obj.ExecuteMethod_ (strMethodName, inParams, out outParams);
				IntPtr outPtr = UnixWbemClassObject.ToPointer ((IWbemClassObject_DoNotMarshal)outParams);
				ppOutParams = new IWbemClassObjectFreeThreaded(outPtr);
			}
			return result;
		}

		public int ExecMethodAsync_ (string strObjectPath, string strMethodName, int lFlags, IWbemContext pCtx, IntPtr pInParams, IWbemObjectSink pResponseHandler)
		{
			string strQuery = QueryParser.GetQueryFromPath (strObjectPath);
			return 0;
		}

		public int ExecNotificationQuery_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			ppEnum = new UnixEnumWbemClassObject(WbemClientFactory.Get (_currentNamespace, strQuery));
			return 0;
		}

		public int ExecNotificationQueryAsync_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int ExecQuery_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, out IEnumWbemClassObject ppEnum)
		{
			IEnumerable<IWbemClassObject_DoNotMarshal> list = WbemClientFactory.Get(_currentNamespace, strQuery);

			ppEnum = new UnixEnumWbemClassObject(list);
			return 0;
		}

		public int ExecQueryAsync_ (string strQueryLanguage, string strQuery, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			IEnumerable<IWbemClassObject_DoNotMarshal> list = WbemClientFactory.Get(_currentNamespace, strQuery);
			pResponseHandler.Indicate_ (list.Count(), new IntPtr[0]);
			return 0;
		}

		public int GetObject_ (string strObjectPath, int lFlags, IWbemContext pCtx, out IWbemClassObjectFreeThreaded ppObject, IntPtr ppCallResult)
		{
			if (ppCallResult == IntPtr.Zero) {
				//Get Class from Path
				var handler = WbemClientFactory.Get (strObjectPath);

				//Get IntPtr for Class
				ppCallResult = System.Runtime.InteropServices.Marshal.GetIUnknownForObject (handler);
			}
			ppObject = new IWbemClassObjectFreeThreaded(ppCallResult);
			return 0;
		}

		public int GetObjectAsync_ (string strObjectPath, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int OpenNamespace_ (string strNamespace, int lFlags, IWbemContext pCtx, out IWbemServices ppWorkingNamespace, IntPtr ppCallResult)
		{
			ppWorkingNamespace = new UnixWbemServices(strNamespace);
			return 0;
		}

		public int PutClass_ (IntPtr pObject, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return 0;
		}

		public int PutClassAsync_ (IntPtr pObject, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int PutInstance_ (IntPtr pInst, int lFlags, IWbemContext pCtx, IntPtr ppCallResult)
		{
			return 0;
		}

		public int PutInstanceAsync_ (IntPtr pInst, int lFlags, IWbemContext pCtx, IWbemObjectSink pResponseHandler)
		{
			return 0;
		}

		public int QueryObjectSink_ (int lFlags, out IWbemObjectSink ppResponseHandler)
		{
			ppResponseHandler = null;
			return 0;
		}

		#endregion
	}
}
