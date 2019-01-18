using System.Runtime.InteropServices.ComTypes;

namespace System.Runtime.InteropServices
{
	public static partial class Marshal
	{
		public static readonly int SystemDefaultCharSize;
		public static readonly int SystemMaxDBCSCharSize;
		public static int AddRef(System.IntPtr pUnk) { throw null; }
		public static System.IntPtr AllocCoTaskMem(int cb) { throw null; }
		public static System.IntPtr AllocHGlobal(int cb) { throw null; }
		public static System.IntPtr AllocHGlobal(System.IntPtr cb) { throw null; }
		public static bool AreComObjectsAvailableForCleanup() { throw null; }
		public static object BindToMoniker(string monikerName) { throw null; }
		public static void ChangeWrapperHandleStrength(object otp, bool fIsWeak) { }        
		public static void CleanupUnusedObjectsInCurrentContext() { }
		public static void Copy(byte[] source, int startIndex, System.IntPtr destination, int length) { }
		public static void Copy(char[] source, int startIndex, System.IntPtr destination, int length) { }
		public static void Copy(double[] source, int startIndex, System.IntPtr destination, int length) { }
		public static void Copy(short[] source, int startIndex, System.IntPtr destination, int length) { }
		public static void Copy(int[] source, int startIndex, System.IntPtr destination, int length) { }
		public static void Copy(long[] source, int startIndex, System.IntPtr destination, int length) { }
		public static void Copy(System.IntPtr source, byte[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr source, char[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr source, double[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr source, short[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr source, int[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr source, long[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr source, System.IntPtr[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr source, float[] destination, int startIndex, int length) { }
		public static void Copy(System.IntPtr[] source, int startIndex, System.IntPtr destination, int length) { }
		public static void Copy(float[] source, int startIndex, System.IntPtr destination, int length) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static System.IntPtr CreateAggregatedObject(System.IntPtr pOuter, object o) { throw null; }
		public static System.IntPtr CreateAggregatedObject<T>(System.IntPtr pOuter, T o) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static object CreateWrapperOfType(object o, System.Type t) { throw null; }
		public static TWrapper CreateWrapperOfType<T, TWrapper>(T o) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static void DestroyStructure(System.IntPtr ptr, System.Type structuretype) { }
		public static void DestroyStructure<T>(System.IntPtr ptr) { }
		public static int FinalReleaseComObject(object o) { throw null; }
		public static void FreeBSTR(System.IntPtr ptr) { }
		public static void FreeCoTaskMem(System.IntPtr ptr) { }
		public static void FreeHGlobal(System.IntPtr hglobal) { }
		public static Guid GenerateGuidForType(System.Type type) { throw null; }
		public static string GenerateProgIdForType(System.Type type) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static System.IntPtr GetComInterfaceForObject(object o, System.Type T) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
//        public static System.IntPtr GetComInterfaceForObject(object o, System.Type T, System.Runtime.InteropServices.CustomQueryInterfaceMode mode) { throw null; }
		public static System.IntPtr GetComInterfaceForObject<T, TInterface>(T o) { throw null; }
		public static object GetComObjectData(object obj, object key) { throw null; }              
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static System.Delegate GetDelegateForFunctionPointer(System.IntPtr ptr, System.Type t) { throw null; }
		public static TDelegate GetDelegateForFunctionPointer<TDelegate>(System.IntPtr ptr) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("GetExceptionCode() may be unavailable in future releases.")]
		public static int GetExceptionCode() { throw null; }
		public static System.Exception GetExceptionForHR(int errorCode) { throw null; }
		public static System.Exception GetExceptionForHR(int errorCode, System.IntPtr errorInfo) { throw null; }
		public static IntPtr GetExceptionPointers() { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static System.IntPtr GetFunctionPointerForDelegate(System.Delegate d) { throw null; }
		public static System.IntPtr GetFunctionPointerForDelegate<TDelegate>(TDelegate d) { throw null; }
		public static System.IntPtr GetHINSTANCE(System.Reflection.Module m) { throw null; }
		public static int GetHRForException(System.Exception e) { throw null; }
		public static int GetHRForLastWin32Error() { throw null; }
		public static System.IntPtr GetIDispatchForObject(object o) { throw null; }
		public static System.IntPtr GetIUnknownForObject(object o) { throw null; }
		public static int GetLastWin32Error() { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("GetNativeVariantForObject(Object, IntPtr) may be unavailable in future releases.")]
		public static void GetNativeVariantForObject(object obj, System.IntPtr pDstNativeVariant) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("GetNativeVariantForObject<T>(T, IntPtr) may be unavailable in future releases.")]
		public static void GetNativeVariantForObject<T>(T obj, System.IntPtr pDstNativeVariant) { }
		public static object GetObjectForIUnknown(System.IntPtr pUnk) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("GetObjectForNativeVariant(IntPtr) may be unavailable in future releases.")]
		public static object GetObjectForNativeVariant(System.IntPtr pSrcNativeVariant) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("GetObjectForNativeVariant<T>(IntPtr) may be unavailable in future releases.")]
		public static T GetObjectForNativeVariant<T>(System.IntPtr pSrcNativeVariant) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("GetObjectsForNativeVariants(IntPtr, Int32) may be unavailable in future releases.")]
		public static object[] GetObjectsForNativeVariants(System.IntPtr aSrcNativeVariant, int cVars) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("GetObjectsForNativeVariants<T>(IntPtr, Int32) may be unavailable in future releases.")]
		public static T[] GetObjectsForNativeVariants<T>(System.IntPtr aSrcNativeVariant, int cVars) { throw null; }
		public static int GetStartComSlot(System.Type t) { throw null; }
		public static int GetEndComSlot(System.Type t) { throw null; }
		public static object GetTypedObjectForIUnknown(System.IntPtr pUnk, System.Type t) { throw null; }
		public static System.Type GetTypeFromCLSID(System.Guid clsid) { throw null; }
//        public static string GetTypeInfoName(System.Runtime.InteropServices.ComTypes.ITypeInfo typeInfo) { throw null; }
		public static object GetUniqueObjectForIUnknown(System.IntPtr unknown) { throw null; }
		public static bool IsComObject(object o) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static System.IntPtr OffsetOf(System.Type t, string fieldName) { throw null; }
		public static System.IntPtr OffsetOf<T>(string fieldName) { throw null; }
		public static void Prelink(System.Reflection.MethodInfo m) { }
		public static void PrelinkAll(Type c) { }
		public static string PtrToStringAuto(System.IntPtr ptr) { throw null; }
		public static string PtrToStringAuto(System.IntPtr ptr, int len) { throw null; }        
		public static string PtrToStringAnsi(System.IntPtr ptr) { throw null; }
		public static string PtrToStringAnsi(System.IntPtr ptr, int len) { throw null; }
		public static string PtrToStringBSTR(System.IntPtr ptr) { throw null; }
		public static string PtrToStringUni(System.IntPtr ptr) { throw null; }
		public static string PtrToStringUni(System.IntPtr ptr, int len) { throw null; }
		public static string PtrToStringUTF8(System.IntPtr ptr) { throw null; }
		public static string PtrToStringUTF8(System.IntPtr ptr, int byteLen) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static void PtrToStructure(System.IntPtr ptr, object structure) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static object PtrToStructure(System.IntPtr ptr, System.Type structureType) { throw null; }
		public static T PtrToStructure<T>(System.IntPtr ptr) { throw null; }
		public static void PtrToStructure<T>(System.IntPtr ptr, T structure) { }
		public static bool IsTypeVisibleFromCom(Type t) { throw null;}
		public static int QueryInterface(System.IntPtr pUnk, ref System.Guid iid, out System.IntPtr ppv) { throw null; }
		public static byte ReadByte(System.IntPtr ptr) { throw null; }
		public static byte ReadByte(System.IntPtr ptr, int ofs) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("ReadByte(Object, Int32) may be unavailable in future releases.")]
		public static byte ReadByte(object ptr, int ofs) { throw null; }
		public static short ReadInt16(System.IntPtr ptr) { throw null; }
		public static short ReadInt16(System.IntPtr ptr, int ofs) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("ReadInt16(Object, Int32) may be unavailable in future releases.")]
		public static short ReadInt16(object ptr, int ofs) { throw null; }
		public static int ReadInt32(System.IntPtr ptr) { throw null; }
		public static int ReadInt32(System.IntPtr ptr, int ofs) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("ReadInt32(Object, Int32) may be unavailable in future releases.")]
		public static int ReadInt32(object ptr, int ofs) { throw null; }
		public static long ReadInt64(System.IntPtr ptr) { throw null; }
		public static long ReadInt64(System.IntPtr ptr, int ofs) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("ReadInt64(Object, Int32) may be unavailable in future releases.")]
		public static long ReadInt64(object ptr, int ofs) { throw null; }
		public static System.IntPtr ReadIntPtr(System.IntPtr ptr) { throw null; }
		public static System.IntPtr ReadIntPtr(System.IntPtr ptr, int ofs) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("ReadIntPtr(Object, Int32) may be unavailable in future releases.")]
		public static System.IntPtr ReadIntPtr(object ptr, int ofs) { throw null; }
		public static System.IntPtr ReAllocCoTaskMem(System.IntPtr pv, int cb) { throw null; }
		public static System.IntPtr ReAllocHGlobal(System.IntPtr pv, System.IntPtr cb) { throw null; }
		public static int Release(System.IntPtr pUnk) { throw null; }
		public static int ReleaseComObject(object o) { throw null; }
		[System.CLSCompliant(false)]
		public static IntPtr SecureStringToBSTR(System.Security.SecureString s) { throw null; }
		[System.CLSCompliant(false)]
		public static IntPtr SecureStringToCoTaskMemAnsi(System.Security.SecureString s) { throw null; }
		[System.CLSCompliant(false)]
		public static IntPtr SecureStringToCoTaskMemUnicode(System.Security.SecureString s) { throw null; }
		[System.CLSCompliant(false)]
		public static IntPtr SecureStringToGlobalAllocAnsi(System.Security.SecureString s) { throw null; }
		[System.CLSCompliant(false)]
		public static IntPtr SecureStringToGlobalAllocUnicode(System.Security.SecureString s) { throw null; }
		public static bool SetComObjectData(object obj, object key, object data) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static int SizeOf(object structure) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static int SizeOf(System.Type t) { throw null; }
		public static int SizeOf<T>() { throw null; }
		public static int SizeOf<T>(T structure) { throw null; }
		public static System.IntPtr StringToBSTR(string s) { throw null; }
		public static System.IntPtr StringToCoTaskMemAnsi(string s) { throw null; }
		public static System.IntPtr StringToCoTaskMemAuto(string s) { throw null; }
		public static System.IntPtr StringToCoTaskMemUni(string s) { throw null; }
		public static System.IntPtr StringToCoTaskMemUTF8(string s) { throw null; }
		public static System.IntPtr StringToHGlobalAnsi(string s) { throw null; }
		public static System.IntPtr StringToHGlobalAuto(string s) { throw null; }
		public static System.IntPtr StringToHGlobalUni(string s) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static void StructureToPtr(object structure, System.IntPtr ptr, bool fDeleteOld) { }
		public static void StructureToPtr<T>(T structure, System.IntPtr ptr, bool fDeleteOld) { }
		public static void ThrowExceptionForHR(int errorCode) { }
		public static void ThrowExceptionForHR(int errorCode, System.IntPtr errorInfo) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		public static System.IntPtr UnsafeAddrOfPinnedArrayElement(System.Array arr, int index) { throw null; }
		public static System.IntPtr UnsafeAddrOfPinnedArrayElement<T>(T[] arr, int index) { throw null; }
		public static void WriteByte(System.IntPtr ptr, byte val) { }
		public static void WriteByte(System.IntPtr ptr, int ofs, byte val) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("WriteByte(Object, Int32, Byte) may be unavailable in future releases.")]
		public static void WriteByte(object ptr, int ofs, byte val) { throw null; }
		public static void WriteInt16(System.IntPtr ptr, char val) { }
		public static void WriteInt16(System.IntPtr ptr, short val) { }
		public static void WriteInt16(System.IntPtr ptr, int ofs, char val) { }
		public static void WriteInt16(System.IntPtr ptr, int ofs, short val) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("WriteInt16(Object, Int32, Char) may be unavailable in future releases.")]
		public static void WriteInt16(object ptr, int ofs, char val) { throw null; }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("WriteInt16(Object, Int32, Int16) may be unavailable in future releases.")]
		public static void WriteInt16(object ptr, int ofs, short val) { throw null; }
		public static void WriteInt32(System.IntPtr ptr, int val) { }
		public static void WriteInt32(System.IntPtr ptr, int ofs, int val) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("WriteInt32(Object, Int32, Int32) may be unavailable in future releases.")]
		public static void WriteInt32(object ptr, int ofs, int val) { throw null; }
		public static void WriteInt64(System.IntPtr ptr, int ofs, long val) { }
		public static void WriteInt64(System.IntPtr ptr, long val) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("WriteInt64(Object, Int32, Int64) may be unavailable in future releases.")]
		public static void WriteInt64(object ptr, int ofs, long val) { throw null; }
		public static void WriteIntPtr(System.IntPtr ptr, int ofs, System.IntPtr val) { }
		public static void WriteIntPtr(System.IntPtr ptr, System.IntPtr val) { }
		[System.ComponentModel.EditorBrowsableAttribute((System.ComponentModel.EditorBrowsableState)(1))]
		[System.ObsoleteAttribute("WriteIntPtr(Object, Int32, IntPtr) may be unavailable in future releases.")]
		public static void WriteIntPtr(object ptr, int ofs, System.IntPtr val) { throw null; }
		public static void ZeroFreeBSTR(System.IntPtr s) { }
		public static void ZeroFreeCoTaskMemAnsi(System.IntPtr s) { }
		public static void ZeroFreeCoTaskMemUnicode(System.IntPtr s) { }
		public static void ZeroFreeGlobalAllocAnsi(System.IntPtr s) { }
		public static void ZeroFreeGlobalAllocUnicode(System.IntPtr s) { }
		public static void ZeroFreeCoTaskMemUTF8(System.IntPtr s) { }


		internal static void SetLastWin32Error (int error)
		{
		}

		internal static IntPtr AllocBSTR (int length)
		{
			throw new NotImplementedException ();
		}
	}

	partial class Marshal
	{
		public static void FreeLibrary(System.IntPtr handle) { }
		public static System.IntPtr GetLibraryExport(System.IntPtr handle, string name) { throw null; }
		public static System.IntPtr LoadLibrary(string libraryPath) { throw null; }
		public static System.IntPtr LoadLibrary(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath) { throw null; }
		public static bool TryGetLibraryExport(System.IntPtr handle, string name, out System.IntPtr address) { throw null; }
		public static bool TryLoadLibrary(string libraryPath, out System.IntPtr handle) { throw null; }
		public static bool TryLoadLibrary(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath, out System.IntPtr handle) { throw null; }
	}

	partial class Marshal
	{
		public static IntPtr GetComInterfaceForObject(object o, Type T, CustomQueryInterfaceMode mode)
		{
			throw new PlatformNotSupportedException(SR.PlatformNotSupported_ComInterop);
		}

		public static string GetTypeInfoName(ITypeInfo typeInfo)
		{
			throw new PlatformNotSupportedException(SR.PlatformNotSupported_ComInterop);
		}
	}
}