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
	internal class UnixEnumWbemClassObject : IEnumWbemClassObject
	{
		private IEnumerable<IWbemClassObject_DoNotMarshal> _objects;
		private IEnumerator<IWbemClassObject_DoNotMarshal> _enumerator;

		internal UnixEnumWbemClassObject (IEnumerable<IWbemClassObject_DoNotMarshal> objects)
		{
  			_objects = objects;
			_enumerator = _objects.GetEnumerator ();
		}

		#region IEnumWbemClassObject implementation

		public int Clone_ (out IEnumWbemClassObject ppEnum)
		{
			ppEnum = this;
			return 0;
		}

		public int Next_ (int lTimeout, int uCount, IWbemClassObject_DoNotMarshal[] apObjects, out uint puReturned)
		{
			uint ret = 0;
			while(_enumerator.MoveNext())
			{
				apObjects[ret] = _enumerator.Current;
				ret++;
				if (ret >= uCount) break;
			}
			puReturned = ret;
			return ret > 0 ? 0 : 1;
		}

		public int NextAsync_ (uint uCount, IWbemObjectSink pSink)
		{
			return 0;
		}

		public int Reset_ ()
		{
			_enumerator = _objects.GetEnumerator ();
			return 0;
		}

		public int Skip_ (int lTimeout, uint nCount)
		{
			return 0;
		}

		#endregion
	}
}
