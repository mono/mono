// System.Runtime.InteropServices.Marshal.cs
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
// Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001-2002 Ximian, Inc.

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.CompilerServices;
using System;
using System.Security;
using System.Reflection;
using System.Threading;

#if NET_2_0
using System.Runtime.ConstrainedExecution;
#endif

namespace System.Runtime.InteropServices
{
	[SuppressUnmanagedCodeSecurity ()]
	public
#if NET_2_0
	static
#else
	sealed
#endif
	class Marshal
	{
		/* fields */
		public static readonly int SystemMaxDBCSCharSize = 2; // don't know what this is
		public static readonly int SystemDefaultCharSize = 2;

#if !NET_2_0
		private Marshal () {}
#endif

		[MonoTODO]
		public static int AddRef (IntPtr pUnk) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr AllocCoTaskMem (int cb);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr AllocHGlobal (IntPtr cb);

		public static IntPtr AllocHGlobal (int cb) {
			return AllocHGlobal ((IntPtr)cb);
		}

		[MonoTODO]
		public static object BindToMoniker (string monikerName) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ChangeWrapperHandleStrength (object otp, bool fIsWeak) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void copy_to_unmanaged (Array source, int startIndex,
						      IntPtr destination, int length);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void copy_from_unmanaged (IntPtr source, int startIndex,
							Array destination, int length);

		public static void Copy (byte[] source, int startIndex, IntPtr destination, int length) {
			copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (char[] source, int startIndex, IntPtr destination, int length) {
			copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (short[] source, int startIndex, IntPtr destination, int length) {
			copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (int[] source, int startIndex, IntPtr destination, int length) {
			copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (long[] source, int startIndex, IntPtr destination, int length) {
			copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (float[] source, int startIndex, IntPtr destination, int length) {
			copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (double[] source, int startIndex, IntPtr destination, int length) {
			copy_to_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (IntPtr source, byte[] destination, int startIndex, int length) {
			copy_from_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (IntPtr source, char[] destination, int startIndex, int length) {
			copy_from_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (IntPtr source, short[] destination, int startIndex, int length) {
			copy_from_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (IntPtr source, int[] destination, int startIndex, int length) {
			copy_from_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (IntPtr source, long[] destination, int startIndex, int length) {
			copy_from_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (IntPtr source, float[] destination, int startIndex, int length) {
			copy_from_unmanaged (source, startIndex, destination, length);
		}

		public static void Copy (IntPtr source, double[] destination, int startIndex, int length) {
			copy_from_unmanaged (source, startIndex, destination, length);
		}

		[MonoTODO]
		public static object CreateWrapperOfType (object o, Type t) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void DestroyStructure (IntPtr ptr, Type structuretype);

		[MonoTODO]
		public static void FreeBSTR (IntPtr ptr) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void FreeCoTaskMem (IntPtr ptr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void FreeHGlobal (IntPtr hglobal);

#if NET_2_0
		[MonoTODO]
		public static void ZeroFreeBSTR (IntPtr ptr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ZeroFreeCoTaskMemAnsi (IntPtr ptr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ZeroFreeCoTaskMemUni (IntPtr ptr) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ZeroFreeGlobalAllocAnsi (IntPtr hglobal) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ZeroFreeGlobalAllocUni (IntPtr hglobal) {
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public static Guid GenerateGuidForType (Type type) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GenerateProgIdForType (Type type) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetActiveObject (string progID) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr GetComInterfaceForObject (object o, Type T) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public static IntPtr GetComInterfaceForObjectInContext (object o, Type t) {
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public static object GetComObjectData (object obj, object key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetComSlotForMethodInfo (MemberInfo m) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetEndComSlot (Type t) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetExceptionCode() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr GetExceptionPointers() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr GetHINSTANCE (Module m) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetHRForException (Exception e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetHRForLastWin32Error() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr GetIDispatchForObject (object o) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public static IntPtr GetIDispatchForObjectInContext (object o) {
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public static IntPtr GetITypeInfoForType (Type t) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr GetIUnknownForObject (object o) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public static IntPtr GetIUnknownForObjectInContext (object o) {
			throw new NotImplementedException ();
		}
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern int GetLastWin32Error();

		[MonoTODO]
		public static IntPtr GetManagedThunkForUnmanagedMethodPtr (IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static MemberInfo GetMethodInfoForComSlot (Type t, int slot, ref ComMemberType memberType) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void GetNativeVariantForObject (object obj, IntPtr pDstNativeVariant) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetObjectForIUnknown (IntPtr pUnk) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetObjectForNativeVariant (IntPtr pSrcNativeVariant) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object[] GetObjectsForNativeVariants (IntPtr aSrcNativeVariant, int cVars) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetStartComSlot (Type t) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Thread GetThreadFromFiberCookie (int cookie) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object GetTypedObjectForIUnknown (IntPtr pUnk, Type t) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Type GetTypeForITypeInfo (IntPtr piTypeInfo) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[Obsolete]
#endif
		[MonoTODO]
		public static string GetTypeInfoName (UCOMITypeInfo pTI) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[Obsolete]
#endif
		[MonoTODO]
		public static Guid GetTypeLibGuid (UCOMITypeLib pTLB) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Guid GetTypeLibGuidForAssembly (Assembly asm) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[Obsolete]
#endif
		[MonoTODO]
		public static int GetTypeLibLcid (UCOMITypeLib pTLB) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[Obsolete]
#endif
		[MonoTODO]
		public static string GetTypeLibName (UCOMITypeLib pTLB) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public static void GetTypeLibVersionForAssembly (Assembly inputAssembly, out int majorVersion, out int minorVersion) {
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public static IntPtr GetUnmanagedThunkForManagedMethodPtr (IntPtr pfnMethodToWrap, IntPtr pbSignature, int cbSignature) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsComObject (object o) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool IsTypeVisibleFromCom (Type t) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int NumParamBytes (MethodInfo m) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr OffsetOf (Type t, string fieldName);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void Prelink (MethodInfo m);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void PrelinkAll (Type c);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string PtrToStringAnsi (IntPtr ptr);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string PtrToStringAnsi (IntPtr ptr, int len);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string PtrToStringAuto (IntPtr ptr);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string PtrToStringAuto (IntPtr ptr, int len);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string PtrToStringUni (IntPtr ptr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string PtrToStringUni (IntPtr ptr, int len);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static string PtrToStringBSTR (IntPtr ptr);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void PtrToStructure (IntPtr ptr, object structure);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static object PtrToStructure (IntPtr ptr, Type structureType);

		[MonoTODO]
		public static int QueryInterface (IntPtr pUnk, ref Guid iid, out IntPtr ppv) {
			throw new NotImplementedException ();
		}

		public static byte ReadByte (IntPtr ptr) {
			return ReadByte (ptr, 0);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static byte ReadByte (IntPtr ptr, int ofs);

		[MonoTODO]
		public static byte ReadByte ([In, MarshalAs (UnmanagedType.AsAny)] object ptr, int ofs) {
			throw new NotImplementedException ();
		}

		public static short ReadInt16 (IntPtr ptr) {
			return ReadInt16 (ptr, 0);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static short ReadInt16 (IntPtr ptr, int ofs);

		[MonoTODO]
		public static short ReadInt16 ([In, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		public static int ReadInt32 (IntPtr ptr) {
			return ReadInt32 (ptr, 0);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int ReadInt32 (IntPtr ptr, int ofs);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MonoTODO]
		public static int ReadInt32 ([In, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		public static long ReadInt64 (IntPtr ptr) {
			return ReadInt64 (ptr, 0);
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long ReadInt64 (IntPtr ptr, int ofs);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MonoTODO]
		public static long ReadInt64 ([In, MarshalAs (UnmanagedType.AsAny)] object ptr, int ofs) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		public static IntPtr ReadIntPtr (IntPtr ptr) {
			return ReadIntPtr (ptr, 0);
		}
		
#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr ReadIntPtr (IntPtr ptr, int ofs);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MonoTODO]
		public static IntPtr ReadIntPtr ([In, MarshalAs (UnmanagedType.AsAny)] object ptr, int ofs) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr ReAllocCoTaskMem (IntPtr pv, int cb) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr ReAllocHGlobal (IntPtr pv, IntPtr cb);

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.Success)]
#endif
		[MonoTODO]
		public static int Release (IntPtr pUnk) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int ReleaseComObject (object o) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[Obsolete]
#endif
		[MonoTODO]
		public static void ReleaseThreadCache() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool SetComObjectData (object obj, object key, object data) {
			throw new NotImplementedException ();
		}

		public static int SizeOf (object structure) {
			return SizeOf (structure.GetType ());
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int SizeOf (Type t);

		[MonoTODO]
		public static IntPtr StringToBSTR (string s) {
			throw new NotImplementedException ();
		}

		public static IntPtr StringToCoTaskMemAnsi (string s) {
			int length = s.Length + 1;
			IntPtr ctm = AllocCoTaskMem (length);

			byte[] asBytes = new byte[length];
			for (int i = 0; i < s.Length; i++)
				asBytes[i] = (byte)s[i];
			asBytes[s.Length] = 0;

			copy_to_unmanaged (asBytes, 0, ctm, length);
			return ctm;
		}

		[MonoTODO]
		public static IntPtr StringToCoTaskMemAuto (string s) {
			throw new NotImplementedException ();
		}

		public static IntPtr StringToCoTaskMemUni (string s) {
			int length = s.Length + 1;
			IntPtr ctm = AllocCoTaskMem (length * 2);

			char[] asChars = new char[length];
			s.CopyTo (0, asChars, 0, s.Length);
			asChars[s.Length] = '\0';

			copy_to_unmanaged (asChars, 0, ctm, length);
			return ctm;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr StringToHGlobalAnsi (string s);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr StringToHGlobalAuto (string s);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr StringToHGlobalUni (string s);

#if NET_2_0
		[MonoTODO]
		public static IntPtr SecureStringToBSTR (SecureString s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr SecureStringToCoTaskMemAnsi (SecureString s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr SecureStringToCoTaskMemUni (SecureString s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr SecureStringToGlobalAllocAnsi (SecureString s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr SecureStringToGlobalAllocUni (SecureString s) {
			throw new NotImplementedException ();
		}
#endif

#if NET_2_0
		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void StructureToPtr (object structure, IntPtr ptr, bool fDeleteOld);

		[MonoTODO]
		public static void ThrowExceptionForHR (int errorCode) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void ThrowExceptionForHR (int errorCode, IntPtr errorInfo) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr UnsafeAddrOfPinnedArrayElement (Array arr, int index);

		public static void WriteByte (IntPtr ptr, byte val) {
			WriteByte (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteByte (IntPtr ptr, int ofs, byte val);

		[MonoTODO]
		public static void WriteByte ([In, Out, MarshalAs (UnmanagedType.AsAny)] object ptr, int ofs, byte val) {
			throw new NotImplementedException ();
		}

		public static void WriteInt16 (IntPtr ptr, short val) {
			WriteInt16 (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteInt16 (IntPtr ptr, int ofs, short val);

		[MonoTODO]
		public static void WriteInt16 ([In, Out, MarshalAs (UnmanagedType.AsAny)] object ptr, int ofs, short val) {
			throw new NotImplementedException ();
		}

		public static void WriteInt16 (IntPtr ptr, char val) {
			WriteInt16 (ptr, 0, val);
		}

		[MonoTODO]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteInt16 (IntPtr ptr, int ofs, char val);

		[MonoTODO]
		public static void WriteInt16([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, char val) {
			throw new NotImplementedException ();
		}

		public static void WriteInt32 (IntPtr ptr, int val) {
			WriteInt32 (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteInt32 (IntPtr ptr, int ofs, int val);

		[MonoTODO]
		public static void WriteInt32([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, int val) {
			throw new NotImplementedException ();
		}

		public static void WriteInt64 (IntPtr ptr, long val) {
			WriteInt64 (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteInt64 (IntPtr ptr, int ofs, long val);

		[MonoTODO]
		public static void WriteInt64 ([In, Out, MarshalAs (UnmanagedType.AsAny)] object ptr, int ofs, long val) {
			throw new NotImplementedException ();
		}

		public static void WriteIntPtr (IntPtr ptr, IntPtr val) {
			WriteIntPtr (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteIntPtr (IntPtr ptr, int ofs, IntPtr val);

		[MonoTODO]
		public static void WriteIntPtr([In, Out, MarshalAs(UnmanagedType.AsAny)] object ptr, int ofs, IntPtr val) {
			throw new NotImplementedException ();
		}

#if NET_2_0
		[MonoTODO]
		public static int FinalReleaseComObject (object o) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Exception GetExceptionForHR (int errorCode) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Exception GetExceptionForHR (int errorCode, IntPtr errorInfo) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Delegate GetDelegateForFunctionPointerInternal (IntPtr ptr, Type t);

		public static Delegate GetDelegateForFunctionPointer (IntPtr ptr, Type t) {
			if (!t.IsSubclassOf (typeof (MulticastDelegate)) || (t == typeof (MulticastDelegate)))
				throw new ArgumentException ("Type is not a delegate", "t");
			if (ptr == IntPtr.Zero)
				throw new ArgumentNullException ("ptr");

			return GetDelegateForFunctionPointerInternal (ptr, t);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern IntPtr GetFunctionPointerForDelegateInternal (Delegate d);
		
		public static IntPtr GetFunctionPointerForDelegate (Delegate d) {
			if (d == null)
				throw new ArgumentNullException ("d");
			
			return GetFunctionPointerForDelegateInternal (d);
		}
#endif
	}
}
