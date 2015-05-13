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
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Management
{
	internal class UnixWbemObjectQualifierSet : IWbemQualifierSet_DoNotMarshal
	{
		private readonly IUnixWbemClassHandler _handler;
		private int _currentIndex;

		public UnixWbemObjectQualifierSet (IUnixWbemClassHandler handler)
		{
			_handler = handler;
		}

		#region IWbemQualifierSet_DoNotMarshal implementation

		public int BeginEnumeration_ (int lFlags)
		{
			_currentIndex = 0;
			return _handler.QualifierNames.Count();
		}

		public int Delete_ (string wszName)
		{
			throw new NotImplementedException ();
		}

		public int EndEnumeration_ ()
		{
			_currentIndex = 0;
			return 0;
		}

		public int Get_ (string wszName, int lFlags, out object pVal, out int plFlavor)
		{
			var obj = _handler.GetQualifier (wszName);
			pVal = obj.Value; 
			plFlavor = 0;
			return 0;
		}

		public int GetNames_ (int lFlags, out string[] pNames)
		{
			pNames = _handler.QualifierNames.ToArray ();
			return 0;
		}

		public int Next_ (int lFlags, out string pstrName, out object pVal, out int plFlavor)
		{
			var obj = _handler.GetQualifier (_currentIndex);
			pstrName = obj.Name;
			pVal = obj.Value; 
			plFlavor = 0;
			_currentIndex++;
			return 0;
		}

		public int Put_ (string wszName, ref object pVal, int lFlavor)
		{
			throw new NotImplementedException ();
		}

		public object NativeObject { get { return _handler; } }
		
		public static IntPtr ToPointer (IWbemQualifierSet_DoNotMarshal obj)
		{
			return Marshal.GetIUnknownForObject(obj.NativeObject);
		}
		
		public static UnixWbemObjectQualifierSet ToManaged(IntPtr pUnk)
		{
			IUnixWbemClassHandler handler = (IUnixWbemClassHandler)Marshal.GetObjectForIUnknown (pUnk);
			return new UnixWbemObjectQualifierSet(handler);
		}

		#endregion
	}
}
