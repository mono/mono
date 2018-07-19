//------------------------------------------------------------------------------
// <copyright file="ZLibNative.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                               
//
// <owner>gpaperin</owner>
//------------------------------------------------------------------------------

using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
using System.Diagnostics.Contracts;
using System.Security;


namespace System.IO.Compression {

/// <summary>
/// This class provides declaration for constants and PInvokes as well as some basic tools for exposing the
/// native CLRCompression.dll (effectively, ZLib) library to managed code.
/// 
/// This file can be included into any managed project that needs to reference CLRCompression.dll.
/// Thus, it is located together with the CLRCompression build files that produce the unmanaged CLRCompression.dll
///
/// ** How to refer to this file from your build project: **
///
/// - The master copy of this file is located under 
///   ndp\fx\src\CLRCompression\System\IO\Compression\ZLibNative.cs .
///   (together with all native CLRCompression sources which are under ndp\fx\src\CLRCompression\zlib-1.2.3\)
///
/// - The CLRCompression assembly build project for will publish this file to
///   <binaries>\<build_flavour>\InterAPIsCandidates\NDP_FX\inc\ .
///   Projects within the NDP_FX partition can consume this file from there.
///
/// - After every change, this file must also be published to
///   InternalApis\NDP_FX\inc\ .
///   Projects outside NDP_FX can consume this files from that location.
/// 
/// In either case, the msbuild variable $(NDP_FXIncPath) should include the one of the above directories that
/// is appropriate for the project.
/// 
/// Projects outside the FX partition can declare a dependency on NDP_FX in their
/// partition.settings.targets, which will cause the right path to be automatically added to their include path list.
/// 
/// Projects inside FX can also use $(InterAPIsCandidatesPath)\inc\ instead of $(NDP_FXIncPath) if the latter is broken.
/// 
/// If some additional ZLib functionality needs to be exposed in future, there are (at least) 3 files that need
/// to be updated:
///   1) clrcompression.def :         To list the DLL entry points.
///   2) clrcompression.nativeproj :  To list all the required ZLib source files
///      (sources that are not required for the currently exposed functionality are commented out).
///   3) This file (ZLibNative.cs) :  To include PInvoke declarations / delegates for all necessary routines.
///   
/// 
/// See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.
/// </summary>
internal static class ZLibNative {

    #region File name constants

    public const string ZLibNativeDllName = "clrcompression.dll";
    private const string Kernel32DllName = "kernel32.dll";

    #endregion  // File name constants


    #region Constants defined in zlib.h

    public const string ZLibVersion = "1.2.3";        

    // This is the NULL pointer for using with ZLib pointers;
    // we prefer it to IntPtr.Zero to mimic the definition of Z_NULL in zlib.h:
    internal static readonly IntPtr ZNullPtr = (IntPtr) ((Int32) 0);


    public enum FlushCode : int {
        NoFlush = 0,
        PartialFlush = 1,
        SyncFlush = 2,
        FullFlush = 3,
        Finish = 4,
        Block = 5,
        //Trees = 6 // Only in ZLib 1.2.4 and later
    }


    public enum ErrorCode : int {
        Ok = 0,
        StreamEnd = 1,
        NeedDictionary = 2,
        ErrorNo = -1,
        StreamError = -2,
        DataError = -3,
        MemError = -4,
        BufError = -5,
        VersionError = -6
    }


    /// <summary>
    /// <p>ZLib can accept any integer value between 0 and 9 (inclusive) as a valid compression level parameter:
    /// 1 gives best speed, 9 gives best compression, 0 gives no compression at all (the input data is simply copied a block at a time).
    /// <code>CompressionLevel.DefaultCompression</code> = -1 requests a default compromise between speed and compression
    /// (currently equivalent to level 6).</p>
    /// 
    /// <p><strong>How to choose a compression level:</strong></p>
    /// 
    /// <p>The names <code>NoCompression</code>, <code>BestSpeed</code>, <code>BestCompression</code> are taken over from the corresponding
    /// ZLib definitions. However, extensive compression performance tests on real data show that they do not describe the reality well.
    /// We have run a large number of tests on different data sets including binary data, English language text, JPEG images and source code.
    /// The results show:</p>
    /// <ul>
    ///   <li><code>CompressionStrategy.DefaultStrategy</code> is the best strategy in most scenarios.</li>
    ///   <li><code>CompressionLevel</code> values over 6 do not significantly improve the compression rate,
    ///       however such values require additional compression time.</li>
    ///   <li>Thus it is not recommended to use a compression level higher than 6, including the
    ///       value <code>CompressionLevel.BestCompression</code>.</li>
    ///   <li>The optimal compression performance (time/rate ratio) tends to occur at compression level 6
    ///       (<code>CompressionLevel.DefaultCompression</code>).</li>
    ///   <li>In scenarios where runtime performance takes precedence over compression rate, smaller compression level values
    ///       can be considered.</li>
    /// </ul>
    /// <p>We recommend using one of the three following combinations:<br />
    /// (See also the constants <code>Deflate_DefaultWindowBits</code> and <code>Deflate_DefaultMemLevel</code> below.)</p>
    /// 
    /// <p><em>Optimal Compression:</em></p>
    /// <p><code>ZLibNative.CompressionLevel compressionLevel = (ZLibNative.CompressionLevel) 6;</code> <br />
    ///    <code>Int32 windowBits = 15;  // or -15 if no headers required</code> <br />
    ///    <code>Int32 memLevel = 8;</code> <br />
    ///    <code>ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;</code> </p>
    /// 
    ///<p><em>Fastest compression:</em></p>
    ///<p><code>ZLibNative.CompressionLevel compressionLevel = (ZLibNative.CompressionLevel) 1;</code> <br />
    ///   <code>Int32 windowBits = 15;  // or -15 if no headers required</code> <br />
    ///   <code>Int32 memLevel = 8; </code> <br />
    ///   <code>ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;</code> </p>
    ///
    /// <p><em>No compression (even faster, useful for data that cannot be compressed such some image formats):</em></p>
    /// <p><code>ZLibNative.CompressionLevel compressionLevel = (ZLibNative.CompressionLevel) 0;</code> <br />
    ///    <code>Int32 windowBits = 15;  // or -15 if no headers required</code> <br />
    ///    <code>Int32 memLevel = 7;</code> <br />
    ///    <code>ZLibNative.CompressionStrategy strategy = ZLibNative.CompressionStrategy.DefaultStrategy;</code> </p>
    /// </summary>
    public enum CompressionLevel : int {
        NoCompression = 0,
        BestSpeed = 1,        
        BestCompression = 9,    // Refer to "How to choose a compression level" above.
        DefaultCompression = -1
    }


    /// <summary>
    /// <p><strong>From the ZLib manual:</strong></p>
    /// <p><code>CompressionStrategy</code> is used to tune the compression algorithm.<br />
    /// Use the value <code>DefaultStrategy</code> for normal data, <code>Filtered</code> for data produced by a filter (or predictor),
    /// <code>HuffmanOnly</code> to force Huffman encoding only (no string match), or <code>Rle</code> to limit match distances to one
    /// (run-length encoding). Filtered data consists mostly of small values with a somewhat random distribution. In this case, the
    /// compression algorithm is tuned to compress them better. The effect of <code>Filtered</code> is to force more Huffman coding and]
    /// less string matching; it is somewhat intermediate between <code>DefaultStrategy</code> and <code>HuffmanOnly</code>.
    /// <code>Rle</code> is designed to be almost as fast as <code>HuffmanOnly</code>, but give better compression for PNG image data.
    /// The strategy parameter only affects the compression ratio but not the correctness of the compressed output even if it is not set
    /// appropriately. <code>Fixed</code> prevents the use of dynamic Huffman codes, allowing for a simpler decoder for special applications.</p>
    /// 
    /// <p><strong>For NetFx use:</strong></p>
    /// <p>We have investigated compression scenarios for a bunch of different requently occuring compression data and found that in all
    /// cases we invesigated so far, <code>DefaultStrategy</code> provided best results</p>
    /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.</p>
    /// </summary>
    public enum CompressionStrategy : int {
        Filtered = 1,
        HuffmanOnly = 2,
        Rle = 3,
        Fixed = 4,
        DefaultStrategy = 0
    }


    /// <summary>
    /// In version 1.2.3, ZLib provides on the <code>Deflated</code>-<code>CompressionMethod</code>.
    /// </summary>
    public enum CompressionMethod : int {
        Deflated = 8
    }

    #endregion  // Constants defined in zlib.h

    
    #region Defaults for ZLib parameters

    /// <summary>
    /// <p><strong>From the ZLib manual:</strong></p>
    /// <p>ZLib's <code>windowBits</code> parameter is the base two logarithm of the window size (the size of the history buffer).
    /// It should be in the range 8..15 for this version of the library. Larger values of this parameter result in better compression
    /// at the expense of memory usage. The default value is 15 if deflateInit is used instead.<br />
    /// <strong>Note</strong>:
    /// <code>windowBits</code> can also be –8..–15 for raw deflate. In this case, -windowBits determines the window size.
    /// <code>Deflate</code> will then generate raw deflate data with no ZLib header or trailer, and will not compute an adler32 check value.<br />
    /// <code>windowBits</code> can also be greater than 15 for optional gzip encoding. Add 16 to <code>windowBits</code> to write a simple
    /// GZip header and trailer around the compressed data instead of a ZLib wrapper. The GZip header will have no file name, no extra data,
    /// no comment, no modification time (set to zero), no header crc, and the operating system will be set to 255 (unknown).
    /// If a GZip stream is being written, <code>ZStream.adler</code> is a crc32 instead of an adler32.</p>
    /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.</p>
    /// </summary>
    public const int Deflate_DefaultWindowBits = -15; // Leagl values are 8..15 and -8..-15. 15 is the window size,
                                                        // negative val causes deflate to produce raw deflate data (no zlib header).

    /// <summary>
    /// <p><strong>From the ZLib manual:</strong></p>
    /// <p>The <code>memLevel</code> parameter specifies how much memory should be allocated for the internal compression state.
    /// <code>memLevel</code> = 1 uses minimum memory but is slow and reduces compression ratio; <code>memLevel</code> = 9 uses maximum
    /// memory for optimal speed. The default value is 8.</p>
    /// <p>See also: How to choose a compression level (in comments to <code>CompressionLevel</code>.</p>
    /// </summary>
    public const int Deflate_DefaultMemLevel = 8;     // Memory usage by deflate. Legal range: [1..9]. 8 is ZLib default.
                                                        // More is faster and better compression with more memory usage.
    #endregion  // Defaults for ZLib parameters


    #region ZLib stream descriptor data structure

    /// <summary>
    /// Do not construct instances of <code>ZStream</code> explicitly.
    /// Always use <code>ZLibNative.DeflateInit2_</code> or <code>ZLibNative.InflateInit2_</code> instead.
    /// Those methods will wrap this structure into a <code>SafeHandle</code> and thus make sure that it is always disposed correctly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]    
    internal struct ZStream {
        internal IntPtr nextIn;     //Bytef    *next_in;  /* next input byte */
        internal UInt32 availIn;    //uInt     avail_in;  /* number of bytes available at next_in */
        internal UInt32 totalIn;    //uLong    total_in;  /* total nb of input bytes read so far */

        internal IntPtr nextOut;    //Bytef    *next_out; /* next output byte should be put there */
        internal UInt32 availOut;   //uInt     avail_out; /* remaining free space at next_out */
        internal UInt32 totalOut;   //uLong    total_out; /* total nb of bytes output so far */
        
        internal IntPtr msg;        //char     *msg;      /* last error message, NULL if no error */

        internal IntPtr state;      //struct internal_state FAR *state; /* not visible by applications */

        internal IntPtr zalloc;     //alloc_func zalloc;  /* used to allocate the internal state */
        internal IntPtr zfree;      //free_func  zfree;   /* used to free the internal state */
        internal IntPtr opaque;     //voidpf   opaque;    /* private data object passed to zalloc and zfree */

        internal Int32 dataType;    //int      data_type; /* best guess about the data type: binary or text */
        internal UInt32 adler;      //uLong    adler;     /* adler32 value of the uncompressed data */
        internal UInt32 reserved;   //uLong    reserved;  /* reserved for future use */
    }

    #endregion  // ZLib stream descriptor data structure


    #region PInvoke declarations for wrapped native functions

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate ErrorCode DeflateInit2_Delegate(ref ZStream stream, CompressionLevel level, CompressionMethod method,
                                                     int windowBits, int memLevel, CompressionStrategy strategy,
                                                     [MarshalAs(UnmanagedType.LPStr)] string version, int streamSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate ErrorCode DeflateDelegate(ref ZStream stream, FlushCode flush);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate ErrorCode DeflateEndDelegate(ref ZStream stream);


    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate ErrorCode InflateInit2_Delegate(ref ZStream stream,
                                                     int windowBits,
                                                     [MarshalAs(UnmanagedType.LPStr)] string version, int streamSize);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate ErrorCode InflateDelegate(ref ZStream stream, FlushCode flush);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate ErrorCode InflateEndDelegate(ref ZStream stream);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    [SuppressUnmanagedCodeSecurity]
    [SecurityCritical]
    private delegate Int32 ZlibCompileFlagsDelegate();

    #endregion  // PInvoke declarations for wrapped native functions


    #region Declarations of Windows API needed to load the native library
    // As described at the top of this file, this file may be used in several managed DLLs that require ZLib
    // functionality. Thus we cannot rely on a specific internal XXXNativeMethod class to declare Windows APIs.
    // Instead, let us declare them explicitly:

    private class NativeMethods {

        [DllImport(Kernel32DllName, CharSet=CharSet.Ansi, BestFitMapping=false)]
        [ResourceExposure(ResourceScope.Process)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        internal static extern IntPtr GetProcAddress(SafeLibraryHandle moduleHandle, String procName);

        [DllImport(Kernel32DllName, CharSet=CharSet.Unicode, SetLastError=true)]
        [ResourceExposure(ResourceScope.Machine)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        internal static extern SafeLibraryHandle LoadLibrary(String libPath);

#if !FEATURE_NETCORE
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
#endif
        [DllImport(Kernel32DllName, ExactSpelling=true)]
        [SuppressUnmanagedCodeSecurity]
        [SecurityCritical]
        internal static extern bool FreeLibrary(IntPtr moduleHandle);
    }
    #endregion  // Declarations of Windows API needed to load the native library


    #region Handle to native DLL in memory


    // Handle type:
    [SecurityCritical]
    private class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid {

		[SecurityCritical]
        internal SafeLibraryHandle()
            : base(true) {
        }		
                
#if !FEATURE_NETCORE
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
        [SecurityCritical]
        protected override bool ReleaseHandle() {

            bool res = ZLibNative.NativeMethods.FreeLibrary(handle);
            handle = IntPtr.Zero;
            return res;
        }
    }  // private class SafeLibraryHandle            

    #endregion  // Handle to native DLL in memory


    /**
     * Do not remove the nested typing of types inside of <code>System.IO.Compression.ZLibNative</code>.
     * This was done on purpose to:
     * 
     * - Achieve the right encapsulation in a situation where <code>ZLibNative</code> may be compiled division-wide
     *   into different assemblies that wish to consume <code>CLRCompression</code>. Since <code>internal</code>
     *   scope is effectively like <code>public</code> scope when compiling <code>ZLibNative</code> into a higher
     *   level assembly, we need a combination of inner types and <code>private</code>-scope members to achieve
     *   the right encapsulation.
     *    
     * - Achieve late dynamic loading of <code>CLRCompression.dll</code> at the right time.
     *   The native assembly will not be loaded unless it is actually used since the loading is performed by a static
     *   constructor of an inner type that is not directly referenced by user code.
     *   
     *   In Dev12 we would like to create a proper feature for loading native assemblies from user-specified
     *   directories in order to PInvoke into them. This would preferably happen in the native interop/PInvoke
     *   layer; if not we can add a Framework level feature.
     */

    #region ZLib Stream Handle type

    /// <summary>
    /// The <code>ZLibStreamHandle</code> could be a <code>CriticalFinalizerObject</code> rather than a
    /// <code>SafeHandleMinusOneIsInvalid</code>. This would save an <code>IntPtr</code> field since
    /// <code>ZLibStreamHandle</code> does not actually use its <code>handle</code> field.
    /// Instead it uses a <code>private ZStream zStream</code> field which is the actual handle data
    /// structure requiring critical finalization.
    /// However, we would like to take advantage if the better debugability offered by the fact that a
    /// <em>releaseHandleFailed MDA</em> is raised if the <code>ReleaseHandle</code> method returns
    /// <code>false</code>, which can for instance happen if the underlying ZLib <code>XxxxEnd</code>
    /// routines return an failure error code.
    /// </summary>
    [SecurityCritical]
    public sealed class ZLibStreamHandle : SafeHandleMinusOneIsInvalid {


        #region Library loading and initialisation

        [SecurityCritical]
        private static class NativeZLibDLLStub {

            #region Function pointers to native functions:

            [SecurityCritical]
            internal static DeflateInit2_Delegate deflateInit2_Delegate;
            [SecurityCritical]
            internal static DeflateDelegate deflateDelegate;
            [SecurityCritical]
            internal static DeflateEndDelegate deflateEndDelegate;

            [SecurityCritical]
            internal static InflateInit2_Delegate inflateInit2_Delegate;
            [SecurityCritical]
            internal static InflateDelegate inflateDelegate;
            [SecurityCritical]
            internal static InflateEndDelegate inflateEndDelegate;

            [SecurityCritical]
            internal static ZlibCompileFlagsDelegate zlibCompileFlagsDelegate;

            #endregion  // Function pointers to native functions:


            #region Initialisation code

#if !FEATURE_NETCORE  // Security rules for CoreSys say the class is security critical, therefore this cannot be security safe critical.
            [SecuritySafeCritical]
#endif
            private static void LoadZLibDLL() {

#if !FEATURE_NETCORE
                new FileIOPermission(PermissionState.Unrestricted).Assert();
#endif

                String fxDir = RuntimeEnvironment.GetRuntimeDirectory();
                String zlibDllPath = Path.Combine(fxDir, ZLibNativeDllName);

                if (!File.Exists(zlibDllPath))
                    throw new DllNotFoundException(ZLibNativeDllName);

                SafeLibraryHandle libHndl = ZLibNative.NativeMethods.LoadLibrary(zlibDllPath);

                if (libHndl.IsInvalid) {

                    Int32 hresult = Marshal.GetHRForLastWin32Error();
                    Marshal.ThrowExceptionForHR(hresult, new IntPtr(-1));

                    // If Marshal.ThrowExceptionForHR did not throw, we still need to make sure to throw:
                    throw new InvalidOperationException();
                }
                
                ZLibStreamHandle.zlibLibraryHandle = libHndl;
            }


            [SecurityCritical]
            private static DT CreateDelegate<DT>(String entryPointName) {

                IntPtr entryPoint = ZLibNative.NativeMethods.GetProcAddress(ZLibStreamHandle.zlibLibraryHandle, entryPointName);

                if (IntPtr.Zero == entryPoint)
                    throw new EntryPointNotFoundException(ZLibNativeDllName + "!" + entryPointName);

                return (DT) (Object) Marshal.GetDelegateForFunctionPointer(entryPoint, typeof(DT));
            }


#if !FEATURE_NETCORE  // Security rules for CoreSys say the class is security critical, therefore this cannot be security safe critical.
            [SecuritySafeCritical]
#endif
            private static void InitDelegates() {

                Contract.Assert(null != ZLibStreamHandle.zlibLibraryHandle);
                Contract.Assert(!ZLibStreamHandle.zlibLibraryHandle.IsInvalid);

                deflateInit2_Delegate = CreateDelegate<DeflateInit2_Delegate>("deflateInit2_");

                deflateDelegate = CreateDelegate<DeflateDelegate>("deflate");

                deflateEndDelegate = CreateDelegate<DeflateEndDelegate>("deflateEnd");

                inflateInit2_Delegate = CreateDelegate<InflateInit2_Delegate>("inflateInit2_");

                inflateDelegate = CreateDelegate<InflateDelegate>("inflate");

                inflateEndDelegate = CreateDelegate<InflateEndDelegate>("inflateEnd");
                
                zlibCompileFlagsDelegate = CreateDelegate<ZlibCompileFlagsDelegate>("zlibCompileFlags");

#if !SILVERLIGHT
                RuntimeHelpers.PrepareDelegate(deflateInit2_Delegate);
                RuntimeHelpers.PrepareDelegate(deflateDelegate);
                RuntimeHelpers.PrepareDelegate(deflateEndDelegate);
                RuntimeHelpers.PrepareDelegate(inflateInit2_Delegate);
                RuntimeHelpers.PrepareDelegate(inflateDelegate);
                RuntimeHelpers.PrepareDelegate(inflateEndDelegate);
                RuntimeHelpers.PrepareDelegate(zlibCompileFlagsDelegate);
#endif // !SILVERLIGHT
            }


            [SecuritySafeCritical]
            static NativeZLibDLLStub() {

                LoadZLibDLL();
                InitDelegates();
            }
           
            #endregion  // Initialisation code

        }  // private static class NativeZLibDLLStub


        // Handle reference:
        [SecurityCritical]
        private static SafeLibraryHandle zlibLibraryHandle;

        #endregion  // Library loading and initialisation


        #region ZLibStream-SafeHandle-related routines

        public enum State { NotInitialized, InitializedForDeflate, InitializedForInflate, Disposed }

        private ZStream zStream;

        [SecurityCritical]
        private volatile State initializationState;

        
        public ZLibStreamHandle()

            : base(true) {

            this.zStream = new ZStream();
            this.zStream.zalloc = ZNullPtr;
            this.zStream.zfree = ZNullPtr;
            this.zStream.opaque = ZNullPtr;

            this.initializationState = State.NotInitialized;
            this.handle = IntPtr.Zero;
        }


        public State InitializationState {

            [Pure]
            [SecurityCritical]
            get { return initializationState; }
        }


#if !FEATURE_NETCORE
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
        [SecurityCritical]
        protected override bool ReleaseHandle() {
            
            // We are in a finalizer thread at the end of the App and the finalization of the dynamically loaded ZLib happend
            // to be scheduled first. In such case we have no hope of properly freeing zStream. If the process is dying - we
            // do not care. In other cases somethign went badly wrong anyway:
            if (zlibLibraryHandle == null || zlibLibraryHandle.IsInvalid)  
                return false;

            switch (InitializationState) {
                case State.NotInitialized:        return true;
                case State.InitializedForDeflate: return (DeflateEnd() == ZLibNative.ErrorCode.Ok);
                case State.InitializedForInflate: return (InflateEnd() == ZLibNative.ErrorCode.Ok);
                case State.Disposed:              return true;
                default: return false;  // This should never happen. Did we forget one of the State enum values in the switch?
            }
        }

        #endregion  // ZLibStream-SafeHandle-related routines


        #region Expose fields on ZStream for use by user / Fx code (add more as required)

        public IntPtr NextIn   { [SecurityCritical] get { return zStream.nextIn; }
                                 [SecurityCritical] set { zStream.nextIn = value; } }

        public UInt32 AvailIn  { [SecurityCritical] get { return zStream.availIn; }
                                 [SecurityCritical] set { zStream.availIn = value; } }

        public UInt32 TotalIn  { [SecurityCritical] get { return zStream.totalIn; } }

        public IntPtr NextOut  { [SecurityCritical] get { return zStream.nextOut; }
                                 [SecurityCritical] set { zStream.nextOut = value; } }

        public UInt32 AvailOut { [SecurityCritical] get { return zStream.availOut; }
                                 [SecurityCritical] set { zStream.availOut = value; } }

        public UInt32 TotalOut { [SecurityCritical] get { return zStream.totalOut; } }

        public Int32 DataType  { [SecurityCritical] get { return zStream.dataType; } }

        public UInt32 Adler    { [SecurityCritical] get { return zStream.adler; } }

        #endregion  // Expose fields on ZStream for use by user / Fx code (add more as required)

        
        #region Expose ZLib functions for use by user / Fx code (add more as required)
       
        [Pure]
        [SecurityCritical]
        private void EnsureNotDisposed() {

            if (InitializationState == State.Disposed)
                throw new ObjectDisposedException(this.GetType().Name);
        }


        [Pure]
        [SecurityCritical]
        private void EnsureState(State requiredState) {

            if (InitializationState != requiredState)
                throw new InvalidOperationException("InitializationState != " + requiredState.ToString());
        }       

       
        [SecurityCritical]
        public ErrorCode DeflateInit2_(CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy) {

            EnsureNotDisposed();
            EnsureState(State.NotInitialized);

            ErrorCode errC;
            bool addRefSuccess = false;
            
#if !FEATURE_NETCORE
            RuntimeHelpers.PrepareConstrainedRegions();
#endif
            try { } finally {

                errC = NativeZLibDLLStub.deflateInit2_Delegate(ref zStream, level, CompressionMethod.Deflated, windowBits, memLevel,
                                                               strategy, ZLibVersion, Marshal.SizeOf(zStream));
                initializationState = State.InitializedForDeflate;
                zlibLibraryHandle.DangerousAddRef(ref addRefSuccess);
            }

            Contract.Assert(addRefSuccess, "zlibLibraryHandle.DangerousAddRef in DeflateInit2_ should always succeed, but it did not.");

            return errC;
        }
        

        [SecurityCritical]
        public ErrorCode Deflate(FlushCode flush) {

            EnsureNotDisposed();
            EnsureState(State.InitializedForDeflate);
            
            return NativeZLibDLLStub.deflateDelegate(ref zStream, flush);
        }


        [SecurityCritical]
        public ErrorCode DeflateEnd() {

            EnsureNotDisposed();
            EnsureState(State.InitializedForDeflate);
            
            ErrorCode errC;

#if !FEATURE_NETCORE
            RuntimeHelpers.PrepareConstrainedRegions();
#endif
            try { } finally {

                errC = NativeZLibDLLStub.deflateEndDelegate(ref zStream);
                initializationState = State.Disposed;
                zlibLibraryHandle.DangerousRelease();
            }
            return errC;
        }
     

        [SecurityCritical]
        public ErrorCode InflateInit2_(int windowBits) {

            EnsureNotDisposed();
            EnsureState(State.NotInitialized);
            
            ErrorCode errC;
            bool addRefSuccess = false;

#if !FEATURE_NETCORE
            RuntimeHelpers.PrepareConstrainedRegions();
#endif
            try { } finally {

                errC = NativeZLibDLLStub.inflateInit2_Delegate(ref zStream, windowBits, ZLibVersion, Marshal.SizeOf(zStream));

                initializationState = State.InitializedForInflate;
                zlibLibraryHandle.DangerousAddRef(ref addRefSuccess);
            }

            Contract.Assert(addRefSuccess, "zlibLibraryHandle.DangerousAddRef in InflateInit2_ should always succeed, but it did not.");

            return errC;
        }


        [SecurityCritical]
        public ErrorCode Inflate(FlushCode flush) {

            EnsureNotDisposed();
            EnsureState(State.InitializedForInflate);
            
            return NativeZLibDLLStub.inflateDelegate(ref zStream, flush);
        }


        [SecurityCritical]
        public ErrorCode InflateEnd() {

            EnsureNotDisposed();
            EnsureState(State.InitializedForInflate);
            
            ErrorCode errC;

#if !FEATURE_NETCORE
            RuntimeHelpers.PrepareConstrainedRegions();
#endif
            try { } finally {

                errC = NativeZLibDLLStub.inflateEndDelegate(ref zStream);
                initializationState = State.Disposed;
                zlibLibraryHandle.DangerousRelease();
            }

            return errC;
        }


        [SecurityCritical]
        public string GetErrorMessage() {

            // This can work even after XxflateEnd().

            if (ZNullPtr.Equals(zStream.msg))
                return String.Empty;

            unsafe {
                String msgStr = new String((SByte*) zStream.msg);
                return msgStr;
            }
        }

        #endregion  // Expose ZLib functions for use by user / Fx code (add more as required)

        [SecurityCritical]
        internal static Int32 ZLibCompileFlags() {
            
            return NativeZLibDLLStub.zlibCompileFlagsDelegate();
        }

    }  // class ZLibStreamHandle

    #endregion  // ZLib Stream Handle type


    #region public factory methods for ZLibStreamHandle


    [SecurityCritical]
    public static ErrorCode CreateZLibStreamForDeflate(out ZLibStreamHandle zLibStreamHandle) {

        return CreateZLibStreamForDeflate(out zLibStreamHandle,
                                          CompressionLevel.DefaultCompression, Deflate_DefaultWindowBits,
                                          Deflate_DefaultMemLevel, CompressionStrategy.DefaultStrategy);
    }


    [SecurityCritical]
    public static ErrorCode CreateZLibStreamForDeflate(out ZLibStreamHandle zLibStreamHandle,
                                                       CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy) {

        zLibStreamHandle = new ZLibStreamHandle();
        return zLibStreamHandle.DeflateInit2_(level, windowBits, memLevel, strategy);
    }

    
    [SecurityCritical]
    public static ErrorCode CreateZLibStreamForInflate(out ZLibStreamHandle zLibStreamHandle) {

        return CreateZLibStreamForInflate(out zLibStreamHandle, Deflate_DefaultWindowBits);
    }


    [SecurityCritical]
    public static ErrorCode CreateZLibStreamForInflate(out ZLibStreamHandle zLibStreamHandle, int windowBits) {

        zLibStreamHandle = new ZLibStreamHandle();
        return zLibStreamHandle.InflateInit2_(windowBits);
    }
    
    #endregion  // public factory methods for ZLibStreamHandle


    #region public utility APIs

    [SecurityCritical]
    public static Int32 ZLibCompileFlags() {

        return ZLibStreamHandle.ZLibCompileFlags();
    }

    #endregion  // public utility APIs

}  // internal class ZLibNative

}  // namespace System.IO.Compression

// file ZLibNative.cs
