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
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Management
{
	[Serializable]
	internal sealed class IWbemClassObjectFreeThreaded : IDisposable, ISerializable
	{
		private readonly static string name;

		public static Guid IID_IWbemClassObject;

		private IntPtr pWbemClassObject;

		private const string SerializationBlobName = "flatWbemClassObject";

		static IWbemClassObjectFreeThreaded()
		{
			IWbemClassObjectFreeThreaded.name = typeof(IWbemClassObjectFreeThreaded).FullName;
			IWbemClassObjectFreeThreaded.IID_IWbemClassObject = new Guid("DC12A681-737F-11CF-884D-00AA004B2E24");
		}

		public IWbemClassObjectFreeThreaded(IntPtr pWbemClassObject)
		{
			this.pWbemClassObject = IntPtr.Zero;
			this.pWbemClassObject = pWbemClassObject;
		}

		public IWbemClassObjectFreeThreaded(SerializationInfo info, StreamingContext context)
		{
			this.pWbemClassObject = IntPtr.Zero;
			byte[] value = info.GetValue("flatWbemClassObject", typeof(byte[])) as byte[];
			if (value != null)
			{
				this.DeserializeFromBlob(value);
				return;
			}
			else
			{
				throw new SerializationException();
			}
		}

		public int BeginEnumeration_(int lEnumFlags)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.BeginEnumeration_f(8, this.pWbemClassObject, lEnumFlags);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int BeginMethodEnumeration_(int lEnumFlags)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.BeginMethodEnumeration_f(22, this.pWbemClassObject, lEnumFlags);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int Clone_(out IWbemClassObjectFreeThreaded ppCopy)
		{
			IntPtr intPtr;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int cloneF = WmiNetUtilsHelper.Clone_f(12, this.pWbemClassObject, out intPtr);
				if (cloneF >= 0)
				{
					ppCopy = new IWbemClassObjectFreeThreaded(intPtr);
				}
				else
				{
					ppCopy = null;
				}
				GC.KeepAlive(this);
				return cloneF;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		/*
		[DllImport("ole32.dll", CharSet=CharSet.None)]
		private static extern void CoMarshalInterface(IStream pStm, ref Guid riid, IntPtr Unk, uint dwDestContext, IntPtr pvDestContext, uint mshlflags);
		*/

		private static void CoMarshalInterface (IStream pStm, ref Guid riid, IntPtr Unk, uint dwDestContext, IntPtr pvDestContext, uint mshlflags)
		{
			
		}


		public int CompareTo_ (bool lFlags, IWbemClassObjectFreeThreaded pCompareTo)
		{
			return CompareTo_ (lFlags ? 1 : 0, pCompareTo);
		}

		public int CompareTo_(int lFlags, IWbemClassObjectFreeThreaded pCompareTo)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.CompareTo_f(16, this.pWbemClassObject, lFlags, pCompareTo.pWbemClassObject);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		/*
		[DllImport("ole32.dll", CharSet=CharSet.None)]
		private static extern IntPtr CoUnmarshalInterface(IStream pStm, ref Guid riid);

		[DllImport("ole32.dll", CharSet=CharSet.None)]
		private static extern IStream CreateStreamOnHGlobal(IntPtr hGlobal, int fDeleteOnRelease);
		*/

		private static IntPtr CoUnmarshalInterface(IStream pStm, ref Guid riid)
		{
			return IntPtr.Zero;
		}

		private static IStream CreateStreamOnHGlobal(IntPtr hGlobal, int fDeleteOnRelease)
		{
			return null;
		}

		public int Delete_(string wszName)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.Delete_f(6, this.pWbemClassObject, wszName);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int DeleteMethod_(string wszName)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.DeleteMethod_f(21, this.pWbemClassObject, wszName);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		private void DeserializeFromBlob(byte[] rg)
		{
			IntPtr zero = IntPtr.Zero;
			IStream stream = null;
			try
			{
				this.pWbemClassObject = IntPtr.Zero;
				zero = Marshal.AllocHGlobal((int)rg.Length);
				Marshal.Copy(rg, 0, zero, (int)rg.Length);
				stream = IWbemClassObjectFreeThreaded.CreateStreamOnHGlobal(zero, 0);
				this.pWbemClassObject = IWbemClassObjectFreeThreaded.CoUnmarshalInterface(stream, ref IWbemClassObjectFreeThreaded.IID_IWbemClassObject);
			}
			finally
			{
				if (stream != null)
				{
					Marshal.ReleaseComObject(stream);
				}
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Dispose()
		{
			this.Dispose_(false);
		}

		private void Dispose_(bool finalization)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				if (Marshal.IsComObject (this.pWbemClassObject)) {
					Marshal.Release(this.pWbemClassObject);
				}
				this.pWbemClassObject = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}

		public int EndEnumeration_()
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int endEnumerationF = WmiNetUtilsHelper.EndEnumeration_f(10, this.pWbemClassObject);
				GC.KeepAlive(this);
				return endEnumerationF;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int EndMethodEnumeration_()
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int endMethodEnumerationF = WmiNetUtilsHelper.EndMethodEnumeration_f(24, this.pWbemClassObject);
				GC.KeepAlive(this);
				return endMethodEnumerationF;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		~IWbemClassObjectFreeThreaded()
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

		public int Get_(string wszName, int lFlags, ref object pVal, ref int pType, ref int plFlavor)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.Get_f(4, this.pWbemClassObject, wszName, lFlags, out pVal, out pType, out plFlavor);
				if (num == -2147217393 && string.Compare(wszName, "__path", StringComparison.OrdinalIgnoreCase) == 0)
				{
					num = 0;
					pType = 8;
					plFlavor = 64;
					pVal = DBNull.Value;
				}
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		/*
		[DllImport("ole32.dll", CharSet=CharSet.None)]
		private static extern IntPtr GetHGlobalFromStream(IStream pstm);
		*/

		private static IntPtr GetHGlobalFromStream (IStream pstm)
		{
			return IntPtr.Zero;
		}


		public int GetMethod_(string wszName, int lFlags, out IWbemClassObjectFreeThreaded ppInSignature, out IWbemClassObjectFreeThreaded ppOutSignature)
		{
			IntPtr intPtr;
			IntPtr intPtr1;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.GetMethod_f(19, this.pWbemClassObject, wszName, lFlags, out intPtr, out intPtr1);
				ppInSignature = null;
				ppOutSignature = null;
				if (num >= 0)
				{
					if (intPtr != IntPtr.Zero)
					{
						ppInSignature = new IWbemClassObjectFreeThreaded(intPtr);
					}
					if (intPtr1 != IntPtr.Zero)
					{
						ppOutSignature = new IWbemClassObjectFreeThreaded(intPtr1);
					}
				}
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int GetMethodOrigin_(string wszMethodName, out string pstrClassName)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.GetMethodOrigin_f(26, this.pWbemClassObject, wszMethodName, out pstrClassName);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int GetMethodQualifierSet_(string wszMethod, out IWbemQualifierSetFreeThreaded ppQualSet)
		{
			IntPtr intPtr;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.GetMethodQualifierSet_f(25, this.pWbemClassObject, wszMethod, out intPtr);
				if (num >= 0)
				{
					ppQualSet = new IWbemQualifierSetFreeThreaded(intPtr);
				}
				else
				{
					ppQualSet = null;
				}
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int GetNames_(string wszQualifierName, int lFlags, ref object pQualifierVal, out string[] pNames)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.GetNames_f(7, this.pWbemClassObject, wszQualifierName, lFlags, ref pQualifierVal, out pNames);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int GetObjectText_(int lFlags, out string pstrObjectText)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.GetObjectText_f(13, this.pWbemClassObject, lFlags, out pstrObjectText);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int GetPropertyOrigin_(string wszName, out string pstrClassName)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.GetPropertyOrigin_f(17, this.pWbemClassObject, wszName, out pstrClassName);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int GetPropertyQualifierSet_(string wszProperty, out IWbemQualifierSetFreeThreaded ppQualSet)
		{
			IntPtr intPtr;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.GetPropertyQualifierSet_f(11, this.pWbemClassObject, wszProperty, out intPtr);
				if (num >= 0)
				{
					ppQualSet = new IWbemQualifierSetFreeThreaded(intPtr);
				}
				else
				{
					ppQualSet = null;
				}
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int GetQualifierSet_(out IWbemQualifierSetFreeThreaded ppQualSet)
		{
			IntPtr intPtr;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int getQualifierSetF = WmiNetUtilsHelper.GetQualifierSet_f(3, this.pWbemClassObject, out intPtr);
				if (getQualifierSetF >= 0)
				{
					ppQualSet = new IWbemQualifierSetFreeThreaded(intPtr);
				}
				else
				{
					ppQualSet = null;
				}
				GC.KeepAlive(this);
				return getQualifierSetF;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		private static IntPtr GlobalLock(IntPtr hGlobal)
		{
			return hGlobal;
		}

		private static int GlobalUnlock(IntPtr pData)
		{
			return 0;
		}

		public int InheritsFrom_(string strAncestor)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.InheritsFrom_f(18, this.pWbemClassObject, strAncestor);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int Next_(int lFlags, ref string strName, ref object pVal, ref int pType, ref int plFlavor)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				pVal = null;
				strName = null;
				int num = WmiNetUtilsHelper.Next_f(9, this.pWbemClassObject, lFlags, out strName, out pVal, out pType, out plFlavor);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int NextMethod_(int lFlags, out string pstrName, out IWbemClassObjectFreeThreaded ppInSignature, out IWbemClassObjectFreeThreaded ppOutSignature)
		{
			IntPtr intPtr;
			IntPtr intPtr1;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.NextMethod_f(23, this.pWbemClassObject, lFlags, out pstrName, out intPtr, out intPtr1);
				ppInSignature = null;
				ppOutSignature = null;
				if (num >= 0)
				{
					if (intPtr != IntPtr.Zero)
					{
						ppInSignature = new IWbemClassObjectFreeThreaded(intPtr);
					}
					if (intPtr1 != IntPtr.Zero)
					{
						ppOutSignature = new IWbemClassObjectFreeThreaded(intPtr1);
					}
				}
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public static implicit operator IntPtr(IWbemClassObjectFreeThreaded wbemClassObject)
		{
			if (wbemClassObject != null)
			{
				return wbemClassObject.pWbemClassObject;
			}
			else
			{
				return IntPtr.Zero;
			}
		}

		public int Put_(string wszName, int lFlags, ref object pVal, int Type)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.Put_f(5, this.pWbemClassObject, wszName, lFlags, ref pVal, Type);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int PutMethod_(string wszName, int lFlags, IWbemClassObjectFreeThreaded pInSignature, IWbemClassObjectFreeThreaded pOutSignature)
		{
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.PutMethod_f(20, this.pWbemClassObject, wszName, lFlags, pInSignature, pOutSignature);
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		private byte[] SerializeToBlob()
		{
			System.Runtime.InteropServices.ComTypes.STATSTG sTATSTG;
			byte[] numArray = null;
			IStream stream = null;
			IntPtr zero = IntPtr.Zero;
			try
			{
				stream = IWbemClassObjectFreeThreaded.CreateStreamOnHGlobal(IntPtr.Zero, 1);
				IWbemClassObjectFreeThreaded.CoMarshalInterface(stream, ref IWbemClassObjectFreeThreaded.IID_IWbemClassObject, this.pWbemClassObject, 2, IntPtr.Zero, 2);
				stream.Stat(out sTATSTG, 0);
				numArray = new byte[sTATSTG.cbSize];
				zero = IWbemClassObjectFreeThreaded.GlobalLock(IWbemClassObjectFreeThreaded.GetHGlobalFromStream(stream));
				Marshal.Copy(zero, numArray, 0, (int)sTATSTG.cbSize);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					IWbemClassObjectFreeThreaded.GlobalUnlock(zero);
				}
				if (stream != null)
				{
					Marshal.ReleaseComObject(stream);
				}
			}
			GC.KeepAlive(this);
			return numArray;
		}

		public int SpawnDerivedClass_(int lFlags, out IWbemClassObjectFreeThreaded ppNewClass)
		{
			IntPtr intPtr;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.SpawnDerivedClass_f(14, this.pWbemClassObject, lFlags, out intPtr);
				if (num >= 0)
				{
					ppNewClass = new IWbemClassObjectFreeThreaded(intPtr);
				}
				else
				{
					ppNewClass = null;
				}
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		public int SpawnInstance_(int lFlags, out IWbemClassObjectFreeThreaded ppNewInstance)
		{
			IntPtr intPtr;
			if (this.pWbemClassObject != IntPtr.Zero)
			{
				int num = WmiNetUtilsHelper.SpawnInstance_f(15, this.pWbemClassObject, lFlags, out intPtr);
				if (num >= 0)
				{
					ppNewInstance = new IWbemClassObjectFreeThreaded(intPtr);
				}
				else
				{
					ppNewInstance = null;
				}
				GC.KeepAlive(this);
				return num;
			}
			else
			{
				throw new ObjectDisposedException(IWbemClassObjectFreeThreaded.name);
			}
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		void System.Runtime.Serialization.ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("flatWbemClassObject", this.SerializeToBlob());
		}

		private enum MSHCTX
		{
			MSHCTX_LOCAL,
			MSHCTX_NOSHAREDMEM,
			MSHCTX_DIFFERENTMACHINE,
			MSHCTX_INPROC
		}

		private enum MSHLFLAGS
		{
			MSHLFLAGS_NORMAL,
			MSHLFLAGS_TABLESTRONG,
			MSHLFLAGS_TABLEWEAK,
			MSHLFLAGS_NOPING
		}

		private enum STATFLAG
		{
			STATFLAG_DEFAULT,
			STATFLAG_NONAME
		}
	}
}