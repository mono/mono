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
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Management
{
	internal sealed class IWbemQualifierSetFreeThreaded : IDisposable
	{
		private readonly static string name;

		public static Guid IID_IWbemClassObject;

		private IntPtr pWbemQualifierSet;

		private const string SerializationBlobName = "flatWbemClassObject";

		static IWbemQualifierSetFreeThreaded()
		{
			IWbemQualifierSetFreeThreaded.name = typeof(IWbemQualifierSetFreeThreaded).FullName;
			IWbemQualifierSetFreeThreaded.IID_IWbemClassObject = new Guid("DC12A681-737F-11CF-884D-00AA004B2E24");
		}

		public IWbemQualifierSetFreeThreaded(IntPtr pWbemQualifierSet)
		{
			this.pWbemQualifierSet = IntPtr.Zero;
			this.pWbemQualifierSet = pWbemQualifierSet;
		}

		public int BeginEnumeration_(int lFlags)
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.QualifierBeginEnumeration_f(7, this.pWbemQualifierSet, lFlags);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemQualifierSetFreeThreaded.name);
			}
		}

		public int Delete_(string wszName)
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.QualifierDelete_f(5, this.pWbemQualifierSet, wszName);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemQualifierSetFreeThreaded.name);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Dispose()
		{
			this.Dispose_(false);
		}

		private void Dispose_(bool finalization)
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				Marshal.Release(this.pWbemQualifierSet);
				this.pWbemQualifierSet = IntPtr.Zero;
			}
			if (!finalization)
			{
				GC.KeepAlive(this);
			}
			GC.SuppressFinalize(this);
		}

		public int EndEnumeration_()
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				int qualifierEndEnumerationF = WmiNetUtilsHelper.QualifierEndEnumeration_f(9, this.pWbemQualifierSet);
				GC.KeepAlive(this);
				return qualifierEndEnumerationF;
			}
			else
			{
				throw new ObjectDisposedException(IWbemQualifierSetFreeThreaded.name);
			}
		}

		~IWbemQualifierSetFreeThreaded()
		{
			try
			{
				this.Dispose_(true);
			}
			finally
			{
				//this.Finalize();
			}
		}

		public int Get_(string wszName, int lFlags, ref object pVal, ref int plFlavor)
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.QualifierGet_f(3, this.pWbemQualifierSet, wszName, lFlags, out pVal, out plFlavor);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemQualifierSetFreeThreaded.name);
			}
		}

		public int GetNames_(int lFlags, out string[] pNames)
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.QualifierGetNames_f(6, this.pWbemQualifierSet, lFlags, out pNames);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemQualifierSetFreeThreaded.name);
			}
		}

		public int Next_(int lFlags, out string pstrName, out object pVal, out int plFlavor)
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.QualifierNext_f(8, this.pWbemQualifierSet, lFlags, out pstrName, out pVal, out plFlavor);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemQualifierSetFreeThreaded.name);
			}
		}

		public int Put_(string wszName, ref object pVal, int lFlavor)
		{
			if (this.pWbemQualifierSet != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.QualifierPut_f(4, this.pWbemQualifierSet, wszName, ref pVal, lFlavor);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemQualifierSetFreeThreaded.name);
			}
		}
	}
}