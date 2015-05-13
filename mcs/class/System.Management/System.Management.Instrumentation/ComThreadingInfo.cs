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

namespace System.Management.Instrumentation
{
	internal class ComThreadingInfo
	{
		private Guid IID_IUnknown;

		private ComThreadingInfo.APTTYPE apartmentType;

		private ComThreadingInfo.THDTYPE threadType;

		private Guid logicalThreadId;

		public ComThreadingInfo.APTTYPE ApartmentType
		{
			get
			{
				return this.apartmentType;
			}
		}

		public static ComThreadingInfo Current
		{
			get
			{
				return new ComThreadingInfo();
			}
		}

		public Guid LogicalThreadId
		{
			get
			{
				return this.logicalThreadId;
			}
		}

		public ComThreadingInfo.THDTYPE ThreadType
		{
			get
			{
				return this.threadType;
			}
		}

		private ComThreadingInfo()
		{
			this.IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
			ComThreadingInfo.IComThreadingInfo comThreadingInfo = (ComThreadingInfo.IComThreadingInfo)ComThreadingInfo.CoGetObjectContext(ref this.IID_IUnknown);
			this.apartmentType = comThreadingInfo.GetCurrentApartmentType();
			this.threadType = comThreadingInfo.GetCurrentThreadType();
			this.logicalThreadId = comThreadingInfo.GetCurrentLogicalThreadId();
		}

		/*
		[DllImport("ole32.dll", CharSet=CharSet.None)]
		private static extern object CoGetObjectContext(ref Guid riid);
		*/

		private static object CoGetObjectContext (ref Guid riid)
		{
			return null;
		}


		public override string ToString()
		{
			return string.Format("{{{0}}} - {1} - {2}", this.LogicalThreadId, this.ApartmentType, this.ThreadType);
		}

		public enum APTTYPE
		{
			APTTYPE_CURRENT = -1,
			APTTYPE_STA = 0,
			APTTYPE_MTA = 1,
			APTTYPE_NA = 2,
			APTTYPE_MAINSTA = 3
		}

		[Guid("000001ce-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IComThreadingInfo
		{
			ComThreadingInfo.APTTYPE GetCurrentApartmentType();

			Guid GetCurrentLogicalThreadId();

			ComThreadingInfo.THDTYPE GetCurrentThreadType();

			void SetCurrentLogicalThreadId(Guid rguid);
		}

		public enum THDTYPE
		{
			THDTYPE_BLOCKMESSAGES,
			THDTYPE_PROCESSMESSAGES
		}
	}
}