// System.Runtime.InteropServices.Marshal
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
// Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001-2002 Ximian, Inc.

using System.Runtime.CompilerServices;
using System;
using System.Reflection;
using System.Threading;

namespace System.Runtime.InteropServices
{
	public sealed class Marshal
	{
		/* fields */
		public static readonly int SystemMaxDBCSCharSize = 2; // don't know what this is
		public static readonly int SystemDefaultCharSize = 2;
		
		private Marshal () {}

		[MonoTODO]
		public static int AddRef (IntPtr pUnk) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static IntPtr AllocCoTaskMem (int cb) {
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
		public static void FreeCoTaskMem (IntPtr ptr) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void FreeHGlobal (IntPtr hglobal);

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

		[MonoTODO]
		public static IntPtr GetITypeInfoForType (Type t) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr GetIUnknownForObject (object o) {
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
		public static string GetTypeInfoName (UCOMITypeInfo pTI) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Guid GetTypeLibGuid (UCOMITypeLib pTLB) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Guid GetTypeLibGuidForAssembly (Assembly asm) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int GetTypeLibLcid (UCOMITypeLib pTLB) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetTypeLibName (UCOMITypeLib pTLB) {
			throw new NotImplementedException ();
		}

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
		public static byte ReadByte (object ptr, int ofs) {
			throw new NotImplementedException ();
		}

		public static short ReadInt16 (IntPtr ptr) {
			return ReadInt16 (ptr, 0);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static short ReadInt16 (IntPtr ptr, int ofs);

		[MonoTODO]
		public static short ReadInt16 (object ptr, int ofs) {
			throw new NotImplementedException ();
		}

		public static int ReadInt32 (IntPtr ptr) {
			return ReadInt32 (ptr, 0);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int ReadInt32 (IntPtr ptr, int ofs);

		[MonoTODO]
		public static int ReadInt32 (object ptr, int ofs) {
			throw new NotImplementedException ();
		}

		public static long ReadInt64 (IntPtr ptr) {
			return ReadInt64 (ptr, 0);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static long ReadInt64 (IntPtr ptr, int ofs);

		[MonoTODO]
		public static long ReadInt64(object ptr, int ofs) {
			throw new NotImplementedException ();
		}

		public static IntPtr ReadIntPtr (IntPtr ptr) {
			return ReadIntPtr (ptr, 0);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr ReadIntPtr (IntPtr ptr, int ofs);

		[MonoTODO]
		public static IntPtr ReadIntPtr(object ptr, int ofs) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr ReAllocCoTaskMem (IntPtr pv, int cb) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr ReAllocHGlobal (IntPtr pv, IntPtr cb);

		[MonoTODO]
		public static int Release (IntPtr pUnk) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int ReleaseComObject (object o) {
			throw new NotImplementedException ();
		}

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

		[MonoTODO]
		public static IntPtr StringToCoTaskMemAnsi (string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr StringToCoTaskMemAuto (string s) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr StringToCoTaskMemUni (string s) {
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr StringToHGlobalAnsi (string s);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr StringToHGlobalAuto (string s);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr StringToHGlobalUni (string s);

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

		[MonoTODO]
		public static IntPtr UnsafeAddrOfPinnedArrayElement (Array arr, int index) {
			throw new NotImplementedException ();
		}

		public static void WriteByte (IntPtr ptr, byte val) {
			WriteByte (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteByte (IntPtr ptr, int ofs, byte val);

		[MonoTODO]
		public static void WriteByte(object ptr, int ofs, byte val) {
			throw new NotImplementedException ();
		}

		public static void WriteInt16 (IntPtr ptr, short val) {
			WriteInt16 (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteInt16 (IntPtr ptr, int ofs, short val);

		[MonoTODO]
		public static void WriteInt16(object ptr, int ofs, short val) {
			throw new NotImplementedException ();
		}

		public static void WriteInt32 (IntPtr ptr, int val) {
			WriteInt32 (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteInt32 (IntPtr ptr, int ofs, int val);

		[MonoTODO]
		public static void WriteInt32(object ptr, int ofs, int val) {
			throw new NotImplementedException ();
		}

		public static void WriteInt64 (IntPtr ptr, long val) {
			WriteInt64 (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteInt64 (IntPtr ptr, int ofs, long val);

		[MonoTODO]
		public static void WriteInt64(object ptr, int ofs, long val) {
			throw new NotImplementedException ();
		}

		public static void WriteIntPtr (IntPtr ptr, IntPtr val) {
			WriteIntPtr (ptr, 0, val);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void WriteIntPtr (IntPtr ptr, int ofs, IntPtr val);

		[MonoTODO]
		public static void WriteIntPtr(object ptr, int ofs, IntPtr val) {
			throw new NotImplementedException ();
		}

	}
}
