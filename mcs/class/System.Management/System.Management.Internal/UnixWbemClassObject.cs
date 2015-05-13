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
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Management
{
	internal class UnixWbemClassObject : IWbemClassObject_DoNotMarshal
	{
		private IUnixWbemClassHandler _handler;

		internal UnixWbemClassObject (IUnixWbemClassHandler handler)
		{
			_handler = handler;
		}

		public void AddMethod (string key, UnixCimMethodInfo method)
		{
			_handler.AddMethod (key, method);
		}

		public void AddProperty (string key, object obj)
		{
			_handler.AddProperty (key,  obj);
		}

		public UnixWbemClassObject WithProperty (string key, object obj)
		{
			AddProperty (key,  obj);
			return this;
		}

		public UnixWbemClassObject WithMethod (string key, UnixCimMethodInfo method)
		{
			AddMethod (key,  method);
			return this;
		}

		internal IDictionary<string, object> Properties {
			get { return _handler.Properties; }
		}

		#region IWbemClassObject_DoNotMarshal implementation

		public virtual int BeginEnumeration_ (int lEnumFlags)
		{
			return 0;
		}

		public virtual int BeginMethodEnumeration_ (int lEnumFlags)
		{
			if (lEnumFlags == 22) {
				/* NORMAL METHODS */
			}
			return 0;
		}

		public virtual int Clone_ (out IWbemClassObject_DoNotMarshal ppCopy)
		{
			ppCopy = this;
			return 0;
		}

		public virtual int CompareTo_ (int lFlags, IWbemClassObject_DoNotMarshal pCompareTo)
		{
			throw new NotImplementedException ();
		}

		public virtual int Delete_ (string wszName)
		{
			throw new NotImplementedException ();
		}

		public virtual int DeleteMethod_ (string wszName)
		{
			throw new NotImplementedException ();
		}

		public virtual int EndEnumeration_ ()
		{
			return 0;
		}

		public virtual int EndMethodEnumeration_ ()
		{
			return 0;
		}

		public virtual int Get_ (string wszName, int lFlags, out object pVal, out int pType, out int plFlavor)
		{
			pVal = _handler.GetProperty (wszName);
			if (pVal == null) {
				var info = _handler.PropertyInfos.FirstOrDefault (x => x.Name == wszName);
				if (string.IsNullOrEmpty (info.Name))
				{
					pType = (int)CimType.Object;
				}
				else {
					pType = (int)info.Type;
				}
			}
			else if (pVal.GetType () == typeof(object)) {
				pType = (int)CimType.String;
			} else {
				pType = CimTypeConverter.GetMiType (pVal.GetType ());
			}
			plFlavor = 0;
			return 0;
		}

		public virtual int GetMethod_ (string wszName, int lFlags, out IWbemClassObject_DoNotMarshal ppInSignature, out IWbemClassObject_DoNotMarshal ppOutSignature)
		{
			var method = _handler.Methods.FirstOrDefault (x => x.Name.Equals (wszName, StringComparison.OrdinalIgnoreCase));
			ppInSignature = null;
			ppOutSignature = null;
			if (string.IsNullOrEmpty (method.Name)) {
				return 0x40005;
			}
			UnixWbemMethodCreator.CreateSignature (method, out ppInSignature, out ppOutSignature);
			return 0;
		}

		public virtual int GetMethodOrigin_ (string wszMethodName, out string pstrClassName)
		{
			pstrClassName = string.Empty;
			return 0;
		}

		public virtual int ExecuteMethod_ (string wszMethodName, object ppInSignature, out object ppOutSignature)
		{
			var inSign = ppInSignature as UnixWbemClassObject;
			UnixWbemClassObject outSign = null;
			var outHandler = _handler.InvokeMethod (wszMethodName, inSign._handler);
			if (outHandler != null) {
				outSign = new UnixWbemClassObject (outHandler);
			}
			ppOutSignature = outSign;
			return 0;
		}

		public virtual int GetMethodQualifierSet_ (string wszMethod, out IWbemQualifierSet_DoNotMarshal ppQualSet)
		{
			ppQualSet = null;
			return 0;
		}

		public virtual int GetNames_ (string wszQualifierName, int lFlags, ref object pQualifierVal, out string[] pNames)
		{
			pNames = new string[0];
			switch (lFlags) {
				case 64:
					pNames = _handler.PropertyNames.ToArray ();
					break;
				case 48:
					pNames = _handler.SystemPropertyNames.ToArray ();
					break;
			}
			return 0;
		}

		public virtual int GetObjectText_ (int lFlags, out string pstrObjectText)
		{
			pstrObjectText = null;
			return 0;
		}

		public virtual int GetPropertyOrigin_ (string wszName, out string pstrClassName)
		{
			pstrClassName = null;
			return 0;
		}

		public virtual int GetPropertyQualifierSet_ (string wszProperty, out IWbemQualifierSet_DoNotMarshal ppQualSet)
		{
			ppQualSet = null;
			return 0;
		}

		public virtual int GetQualifierSet_ (out IWbemQualifierSet_DoNotMarshal ppQualSet)
		{
			ppQualSet = new UnixWbemObjectQualifierSet(_handler);
			return 0;
		}

		public virtual int InheritsFrom_ (string strAncestor)
		{
			return 0;
		}

		public virtual int Next_ (int lFlags, out string strName, out object pVal, out int pType, out int plFlavor)
		{
			strName = null;
			pVal = null;
			pType = (int)CimType.String;
			plFlavor = 0;
			return 0;
		}

		public virtual int NextMethod_ (int lFlags, out string pstrName, out IWbemClassObject_DoNotMarshal ppInSignature, out IWbemClassObject_DoNotMarshal ppOutSignature)
		{
			var method = _handler.NextMethod ();
			pstrName = null;
			ppInSignature = null;
			ppOutSignature = null;
			if (string.IsNullOrEmpty (method.Name)) {
				return 0x40005;
			}
			pstrName = method.Name;
			UnixWbemMethodCreator.CreateSignature (method, out ppInSignature, out ppOutSignature);

			return 0;
		}

		public virtual int Put_ (string wszName, int lFlags, ref object pVal, int Type)
		{
			var info = this._handler.PropertyInfos.FirstOrDefault (x => x.Name.Equals (wszName, StringComparison.InvariantCultureIgnoreCase));
			if (!string.IsNullOrEmpty (info.Name))
			{
				if (Type != (int)info.Type)
				{
					throw new InvalidCastException("Property is not of type " + info.Type.ToString());
				}
				this._handler.WithProperty (wszName, pVal);
			}
			return 0;
		}

		public virtual int PutMethod_ (string wszName, int lFlags, IWbemClassObject_DoNotMarshal pInSignature, IWbemClassObject_DoNotMarshal pOutSignature)
		{
			pInSignature = null;
			pOutSignature = null;
			return 0;
		}

		public virtual int SpawnDerivedClass_ (int lFlags, out IWbemClassObject_DoNotMarshal ppNewClass)
		{
			ppNewClass = this;
			return 0;
		}

		public virtual int SpawnInstance_ (int lFlags, out IWbemClassObject_DoNotMarshal ppNewInstance)
		{
			ppNewInstance = this;
			return 0;
		}

		public object NativeObject { get { return _handler; } }

		public static IntPtr ToPointer (IWbemClassObject_DoNotMarshal obj)
		{
			if (obj == null) return IntPtr.Zero;
			return Marshal.GetIUnknownForObject(obj.NativeObject);
		}

		public static UnixWbemClassObject ToManaged(IntPtr pUnk)
		{
			IUnixWbemClassHandler handler = (IUnixWbemClassHandler)Marshal.GetObjectForIUnknown (pUnk);
			return new UnixWbemClassObject(handler);
		}

		#endregion
	}
}
