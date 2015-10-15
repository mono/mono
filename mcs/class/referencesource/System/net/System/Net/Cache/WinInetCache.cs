/*++
Copyright (c) Microsoft Corporation

Module Name:

    _WinInetCache.cs

Abstract:
    The class implements low-level object model for
    communications with the caching part of WinInet DLL


Author:

    Alexei Vopilov    21-Dec-2002

Revision History:

--*/
namespace System.Net.Cache {
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Collections;
using System.Text;
using System.Collections.Specialized;
using System.Threading;
using System.Globalization;

    //
    // WinInet OS Provider implementation (Caching part only)
    //
    // Contains methods marked with unsafe keyword
    //
    internal static class _WinInetCache {

        private const int  c_CharSz = 2;


        //
        // DATA Definitions
        //

        //  Cache Entry declarations
        [Flags]
        internal enum EntryType {
            NormalEntry     = 0x00000041, // ored with HTTP_1_1_CACHE_ENTRY
            StickyEntry     = 0x00000044, // ored with HTTP_1_1_CACHE_ENTRY
            Edited          = 0x00000008,
            TrackOffline    = 0x00000010,
            TrackOnline     = 0x00000020,
            Sparse          = 0x00010000,
            Cookie          = 0x00100000,
            UrlHistory      = 0x00200000,
//            FindDefaultFilter   = NormalEntry|StickyEntry|Cookie|UrlHistory|TrackOffline|TrackOnline
        }
        /*
            Some More IE private entry types
        HTTP_1_1_CACHE_ENTRY            0x00000040
        STATIC_CACHE_ENTRY              0x00000080
        MUST_REVALIDATE_CACHE_ENTRY     0x00000100
        COOKIE_ACCEPTED_CACHE_ENTRY     0x00001000
        COOKIE_LEASHED_CACHE_ENTRY      0x00002000
        COOKIE_DOWNGRADED_CACHE_ENTRY   0x00004000
        COOKIE_REJECTED_CACHE_ENTRY     0x00008000
        PENDING_DELETE_CACHE_ENTRY      0x00400000
        OTHER_USER_CACHE_ENTRY          0x00800000
        PRIVACY_IMPACTED_CACHE_ENTRY    0x02000000
        POST_RESPONSE_CACHE_ENTRY       0x04000000
        INSTALLED_CACHE_ENTRY           0x10000000
        POST_















*/


        //  Some supported Entry fields references
        [Flags]
        internal enum Entry_FC {
            None            = 0x0,
            Attribute       = 0x00000004,
            Hitrate         = 0x00000010,
            Modtime         = 0x00000040,
            Exptime         = 0x00000080,
            Acctime         = 0x00000100,
            Synctime        = 0x00000200,
            Headerinfo      = 0x00000400,
            ExemptDelta     = 0x00000800
        }

        //  Error status codes, some are mapped to native ones
        internal enum Status {
            Success                 = 0,
            InsufficientBuffer      = 122,
            FileNotFound            = 2,
            NoMoreItems             = 259,
            NotEnoughStorage        = 8,
            SharingViolation        = 32,
            InvalidParameter        = 87,

            // Below are extensions of native errors (no real errors exist)
            Warnings                = 0x1000000,

            FatalErrors             = Warnings + 0x1000,
            CorruptedHeaders        = (int)FatalErrors+1,
            InternalError           = (int)FatalErrors+2
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct FILETIME {
            public uint Low;
            public uint High;

            public static readonly FILETIME Zero = new FILETIME(0L);

            public FILETIME(long time) {
                unchecked {
                    Low  = (uint)time;
                    High = (uint)(time>>32);
                }
            }

            public long ToLong() {
                return ((long)High<<32) | Low;
            }

            public bool IsNull {
                get {return Low == 0 && High == 0;}
            }
        }
        //
        // It's an unmanamged layout of WinInet cache Entry Info
        // The pointer on this guy will represent the entry info for wininet
        // native calls
        //
        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
        internal struct EntryBuffer {
            public static int MarshalSize = Marshal.SizeOf(typeof(EntryBuffer));

            public  int         StructSize;        // version of cache system MUST BE == sizeof(this)
            // We replace this with an offset from the struct start
            public  IntPtr      _OffsetSourceUrlName;
            // We replace this with an offset from the struct start
            public  IntPtr      _OffsetFileName;   // embedded pointer to the local file name.
            public  EntryType   EntryType;         // cache type bit mask.
            public  int         UseCount;          // current users count of the cache entry.
            public  int         HitRate;           // num of times the cache entry was retrieved.
            public  int         SizeLow;           // low DWORD of the file size.
            public  int         SizeHigh;          // high DWORD of the file size.
            public  FILETIME    LastModifiedTime;  // last modified time of the file in GMT format.
            public  FILETIME    ExpireTime;        // expire time of the file in GMT format
            public  FILETIME    LastAccessTime;    // last accessed time in GMT format
            public  FILETIME    LastSyncTime;      // last time the URL was synchronized with the source
            // We replace this with an offset from the struct start
            public  IntPtr      _OffsetHeaderInfo; // embedded pointer to the header info.
            public  int         HeaderInfoChars;   // size of the above header.
            // We replace this with an offset from the struct start
            public  IntPtr     _OffsetExtension;   // File extension used to retrive the urldata as a file.

            [StructLayout(LayoutKind.Explicit)]
            public struct Rsv {
                [FieldOffset(0)] public  int         ExemptDelta;       // Exemption delta from last access
                [FieldOffset(0)] public  int         Reserved;          // To keep the unmanaged layout
            }
            public Rsv U;

        }

        //
        // This class holds a manged version of native WinInet buffer
        //
        internal class Entry {
            public const int    DefaultBufferSize = 2048;

            public Status       Error;
            public string       Key;
            public string       Filename;           // filled by Create() or returned by LookupXXX()
            public string       FileExt;            // filled by Create() or returned by LookupXXX()
            public int          OptionalLength;     // should be always null
            public string       OriginalUrl;        // should be always null
            public string       MetaInfo;           // referenced to by Entry_FC.Headerinfo
            public int          MaxBufferBytes;     // contains the buffer size in bytes on input and
                                                    // copied bytes count in the buffer on output
            // The tail represents the entry info in its unmanaged layout;
            public EntryBuffer  Info;

            public Entry(string key, int maxHeadersSize) {
                Key = key;
                MaxBufferBytes = maxHeadersSize;
                if (maxHeadersSize != Int32.MaxValue && (Int32.MaxValue - (key.Length + EntryBuffer.MarshalSize + 1024)*2) > maxHeadersSize) {
                    //
                    // The buffer size is restricted mostly by headers, we reserve 1k more CHARS for additional
                    // metadata, otherwise user has to play with the maxHeadersSize parameter
                    //
                    //
                    MaxBufferBytes += (key.Length + EntryBuffer.MarshalSize + 1024)*2;
                }
                Info.EntryType = EntryType.NormalEntry;
            }
        }

        //
        // Method Definitions
        //


        //
        // Looks up an entry based on url string key.
        // Parses Headers and strings into entry mamabers.
        // Parses the raw output into entry.Info member.
        //
        unsafe internal static Status LookupInfo(Entry entry) {

            byte[] entryBuffer = new byte[Entry.DefaultBufferSize];
            int size      = entryBuffer.Length;
            byte[] buffer = entryBuffer;

            //We may need to adjust the buffer size (using 64 attempts although I would rather try to death)
            for (int k = 0; k < 64; ++k) {
                fixed (byte* entryPtr = buffer) {

                    bool found = UnsafeNclNativeMethods.UnsafeWinInetCache.GetUrlCacheEntryInfoW(entry.Key, entryPtr, ref size);

                    if (found) {
                        entryBuffer = buffer;
                        entry.MaxBufferBytes = size;
                        EntryFixup(entry, (EntryBuffer*) entryPtr, buffer);
                        entry.Error = Status.Success;
                        return entry.Error;
                    }

                    entry.Error = (Status)Marshal.GetLastWin32Error();
                    if (entry.Error == Status.InsufficientBuffer) {
                        if ((object) buffer == (object)entryBuffer) {
                            // did not reallocate yet.
                            if (size <= entry.MaxBufferBytes) {
                                buffer = new byte[size];
                                continue;
                            }
                        }
                    }
                    //some error has occured
                    break;
                }
            }
            return entry.Error;
        }

        //
        // Lookups an entry based on url string key.
        // If exists, locks the entry and hands out a managed handle representing a locked entry.
        //
        unsafe internal static SafeUnlockUrlCacheEntryFile LookupFile(Entry entry) {

            byte[] buffer       = new byte[Entry.DefaultBufferSize];
            int size            = buffer.Length;
            SafeUnlockUrlCacheEntryFile handle = null;

            try {
                while (true) {
                    fixed (byte* entryPtr = buffer) {
                        //We may need to adjust the buffer size
                        entry.Error = SafeUnlockUrlCacheEntryFile.GetAndLockFile(entry.Key, entryPtr, ref size, out handle);

                        if (entry.Error  == Status.Success) {
                            entry.MaxBufferBytes = size;
                            EntryFixup(entry, (EntryBuffer*) entryPtr, buffer);
                            //The method is available in TRAVE
                            return handle;
                        }


                        if (entry.Error == Status.InsufficientBuffer) {
                            if (size <= entry.MaxBufferBytes) {
                                buffer = new byte[size];
                                continue;
                            }
                        }
                        //some error has occured
                        break;
                    }

                }
            }
            catch(Exception e) {
                if (handle != null)
                    handle.Close();

                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                    throw;

                if (entry.Error == Status.Success) {
                    entry.Error = Status.InternalError;
                }
            }
            return null;
        }


        //
        // Does the fixup of the returned buffer by converting internal pointer to offsets
        // it also does copying of non-string values from unmanaged buffer to Entry.Buffer members
        //
        unsafe private static Status EntryFixup(Entry entry, EntryBuffer* bufferPtr, byte[] buffer) {
            unchecked {
                bufferPtr->_OffsetExtension     = bufferPtr->_OffsetExtension == IntPtr.Zero? IntPtr.Zero: (IntPtr)((byte*)bufferPtr->_OffsetExtension - (byte*)bufferPtr);
                bufferPtr->_OffsetFileName      = bufferPtr->_OffsetFileName == IntPtr.Zero? IntPtr.Zero: (IntPtr)((byte*)bufferPtr->_OffsetFileName - (byte*)bufferPtr);
                bufferPtr->_OffsetHeaderInfo    = bufferPtr->_OffsetHeaderInfo == IntPtr.Zero? IntPtr.Zero: (IntPtr)((byte*)bufferPtr->_OffsetHeaderInfo - (byte*)bufferPtr);
                bufferPtr->_OffsetSourceUrlName = bufferPtr->_OffsetSourceUrlName == IntPtr.Zero? IntPtr.Zero: (IntPtr)((byte*)bufferPtr->_OffsetSourceUrlName - (byte*)bufferPtr);

                // Get a managed EntryBuffer copy out of byte[]
                entry.Info = *bufferPtr;
                entry.OriginalUrl   = GetEntryBufferString(bufferPtr, (int)(bufferPtr->_OffsetSourceUrlName));
                entry.Filename      = GetEntryBufferString(bufferPtr, (int)(bufferPtr->_OffsetFileName));
                entry.FileExt       = GetEntryBufferString(bufferPtr, (int)(bufferPtr->_OffsetExtension));
            }
            return GetEntryHeaders(entry, bufferPtr, buffer);
        }

        //
        // Returns a filename for a cache entry to write to.
        //
        internal static Status CreateFileName(Entry entry) {

            entry.Error = Status.Success;
            StringBuilder sb = new StringBuilder(UnsafeNclNativeMethods.UnsafeWinInetCache.MAX_PATH);
            if (UnsafeNclNativeMethods.UnsafeWinInetCache.CreateUrlCacheEntryW(entry.Key, entry.OptionalLength, entry.FileExt, sb, 0)) {
                entry.Filename = sb.ToString();
                return Status.Success;
            }
            entry.Error = (Status) Marshal.GetLastWin32Error();
            return entry.Error;
        }


        //
        // Associates a file with a cache entry.
        //
        internal static Status Commit(Entry entry) {
            string s = entry.MetaInfo;
            if (s == null) {
                s = string.Empty;
            }
            if ((s.Length + entry.Key.Length + entry.Filename.Length + (entry.OriginalUrl==null? 0: entry.OriginalUrl.Length)) > entry.MaxBufferBytes/c_CharSz) {
                entry.Error = Status.InsufficientBuffer;
                return entry.Error;
            }

            entry.Error = Status.Success;
            unsafe {
                fixed (char *ptr = s) {

                    byte* realBytesPtr = s.Length == 0? null: (byte*)ptr;
                    if (!UnsafeNclNativeMethods.UnsafeWinInetCache.CommitUrlCacheEntryW(
                                                                    entry.Key,
                                                                    entry.Filename,
                                                                    entry.Info.ExpireTime,
                                                                    entry.Info.LastModifiedTime,
                                                                    entry.Info.EntryType,
                                                                    realBytesPtr,
                                                                    s.Length,
                                                                    null,               // FileExt is reserved, must be null
                                                                    entry.OriginalUrl   // It's better to not play with redirections
                                                                                        // OrigianlUri should be nulled by the caller
                                                                    ))
                    {
                        entry.Error = (Status)Marshal.GetLastWin32Error();
                    }
                }
            }

            return entry.Error;
        }

        //
        // Updates a Cached Entry metadata according to attibutes flags.
        //
        internal static Status Update(Entry newEntry, Entry_FC attributes) {
            // Currently WinInet does not support headers update,
            // hence don't need space for them although we'll need recreate a cache entry
            // if headers update is requested

            byte[]  buffer = new byte[EntryBuffer.MarshalSize];
            newEntry.Error = Status.Success;
            unsafe {
                fixed (byte *bytePtr = buffer) {
                    EntryBuffer *ePtr = (EntryBuffer*) bytePtr;
                    *ePtr = newEntry.Info;
                    //set the version just in case
                    ePtr->StructSize =  EntryBuffer.MarshalSize;

                    if ((attributes & Entry_FC.Headerinfo) == 0) {
                        if (!UnsafeNclNativeMethods.UnsafeWinInetCache.SetUrlCacheEntryInfoW(newEntry.Key, bytePtr, attributes)) {
                            newEntry.Error = (Status)Marshal.GetLastWin32Error();
                        }
                    }
                    else {
                        // simulating headers update using Edited cache entry feature of WinInet
                        Entry oldEntry = new Entry(newEntry.Key, newEntry.MaxBufferBytes);

                        SafeUnlockUrlCacheEntryFile handle = null;
                        bool wasEdited = false;
                        try {
                            // lock the entry and get the filename out.
                            handle = LookupFile(oldEntry);
                            if (handle == null) {
                                //The same error would happen on update attributes, return it.
                                newEntry.Error = oldEntry.Error;
                                return newEntry.Error;
                            }

                            //Copy strings from old entry that are not present in the method parameters
                            newEntry.Filename       = oldEntry.Filename;
                            newEntry.OriginalUrl    = oldEntry.OriginalUrl;
                            newEntry.FileExt        = oldEntry.FileExt;

                            // We don't need to update this and some other attributes since will replace entire entry
                            attributes &= ~Entry_FC.Headerinfo;

                            //Copy attributes from an old entry that are not present in the method parameters
                            if ((attributes & Entry_FC.Exptime) == 0) {
                                newEntry.Info.ExpireTime = oldEntry.Info.ExpireTime;
                            }

                            if ((attributes & Entry_FC.Modtime) == 0) {
                                newEntry.Info.LastModifiedTime = oldEntry.Info.LastModifiedTime;
                            }

                            if ((attributes & Entry_FC.Attribute) == 0) {
                                newEntry.Info.EntryType = oldEntry.Info.EntryType;
                                newEntry.Info.U.ExemptDelta = oldEntry.Info.U.ExemptDelta;
                                if ((oldEntry.Info.EntryType & EntryType.StickyEntry) == EntryType.StickyEntry) {
                                    attributes |= (Entry_FC.Attribute | Entry_FC.ExemptDelta);
                                }
                            }

                            // Those attributes will be taken care of by Commit()
                            attributes &= ~(Entry_FC.Exptime|Entry_FC.Modtime);

                            wasEdited = (oldEntry.Info.EntryType & EntryType.Edited) != 0;

                            if (!wasEdited) {
                                // Prevent the file from being deleted on entry Remove (kinda hack)
                                oldEntry.Info.EntryType |= EntryType.Edited;
                                // Recursion!
                                if (Update(oldEntry, Entry_FC.Attribute) != Status.Success) {
                                    newEntry.Error = oldEntry.Error;
                                    return newEntry.Error;
                                }
                            }
                        }
                        finally {
                            if (handle != null) {
                                handle.Close();
                            }
                        }

                        // At this point we try to delete the exisintg item and create a new one with the same
                        // filename and the new headers.
                        //We wish to ignore any errors from Remove since are going to replace the entry.
                        Remove(oldEntry);
                        if (Commit(newEntry) != Status.Success) {
                            if (!wasEdited) {
                                //revert back the original entry type
                                oldEntry.Info.EntryType &= ~EntryType.Edited;
                                Update(oldEntry, Entry_FC.Attribute);
                                // Being already in error mode, cannot do much if Update fails.
                            }
                            return newEntry.Error;
                        }

                        // Now see what's left in attributes change request.
                        if (attributes != Entry_FC.None) {
                            Update(newEntry, attributes);
                        }
                        //At this point newEntry.Error should contain the resulting status
                        //and we replaced the entry in the cache with the same body
                        //but different headers. Some more attributes may have changed as well.
                    }
                }
            }

            return newEntry.Error;
        }

        //
        // Updates a Cached Entry metadata according to attibutes flags.
        //
        internal static Status Remove(Entry entry) {
            entry.Error = Status.Success;
            if (!UnsafeNclNativeMethods.UnsafeWinInetCache.DeleteUrlCacheEntryW(entry.Key)) {
                entry.Error = (Status)Marshal.GetLastWin32Error();
            }
            return entry.Error;
        }


        //
        // Gets the managed copy of a null terminated string resided in the buffer
        //
#if DEBUG
        /*
        // Consider removing.
        private static unsafe string GetEntryBufferString(byte[] buffer, int offset) {
            fixed (void* bufferPtr = buffer) {
                return GetEntryBufferString(bufferPtr, offset);
            }
        }
        */
#endif
        private static unsafe string GetEntryBufferString(void* bufferPtr, int offset) {
            if (offset == 0) {
                return null;
            }
            IntPtr pointer = new IntPtr((byte*)bufferPtr + offset);
            return Marshal.PtrToStringUni(pointer);
        }

        //
        // Gets the headers and optionally other meta data out of a cached entry
        //
        // Whenever an empty line found in the buffer, the resulting array of
        // collections will grow in size
        //
        private static unsafe Status GetEntryHeaders(Entry entry, EntryBuffer* bufferPtr, byte[] buffer) {
            entry.Error = Status.Success;
            entry.MetaInfo = null;

            //
            if (bufferPtr->_OffsetHeaderInfo == IntPtr.Zero || bufferPtr->HeaderInfoChars == 0 || (bufferPtr->EntryType & EntryType.UrlHistory) != 0) {
                return Status.Success;
            }

            int bufferCharLength = bufferPtr->HeaderInfoChars + ((int)(bufferPtr->_OffsetHeaderInfo))/c_CharSz;
            if (bufferCharLength*c_CharSz > entry.MaxBufferBytes) {
                // WinInet 

                bufferCharLength = entry.MaxBufferBytes/c_CharSz;
            }
            //WinInet may put terminating nulls at the end of the buffer, remove them.
            while (((char*)bufferPtr)[bufferCharLength-1] == 0)
                {--bufferCharLength;}
            entry.MetaInfo = Encoding.Unicode.GetString(buffer, (int)bufferPtr->_OffsetHeaderInfo, (bufferCharLength-(int)bufferPtr->_OffsetHeaderInfo/2)*2);
            return entry.Error;
        }
/********************
            ArrayList result = new ArrayList();
            NameValueCollection collection = new NameValueCollection();
            int offset = (int)bufferPtr->_OffsetHeaderInfo/c_CharSz;
            char *charPtr = (char*)bufferPtr;
            {
                int i = offset+1;
                for (; i < entry.MaxBufferBytes/c_CharSz; ++i) {
                    if ((charPtr[i] == ':' || (charPtr[i] == '\n' && charPtr[(i-1)] == '\r'))) {
                        break;
                    }
                }
                if (i < entry.MaxBufferBytes/c_CharSz) {
                    //If this looks like a status line
                    if (charPtr[i] == '\n' && i > offset+1) {
                        string s = Encoding.Unicode.GetString(buffer, offset*2, (i-offset-1)*2);
                        offset = i+1;
                        collection[string.Empty] = s;
                    }
                }
            }
            int bufferCharLength = bufferPtr->HeaderInfoChars + ((int)(bufferPtr->_OffsetHeaderInfo))/c_CharSz;
            if (bufferCharLength*c_CharSz > entry.MaxBufferBytes) {
                // WinInet 






































*/
#if DEBUG

        /*
        // Consider removing.
        //
        // For debugging will return a readbale representation of a cached entry info
        //
        private static string DebugEntryBuffer(byte[] buffer, int entryBufSize) {

            EntryBuffer Info;
            if (entryBufSize < EntryBuffer.MarshalSize) {
                throw new ArgumentOutOfRangeException("size");
            }
            unsafe {
                fixed(void* vptr = buffer) {
                    IntPtr ptr = new IntPtr(vptr);
                    Info = (EntryBuffer)Marshal.PtrToStructure(ptr,typeof(EntryBuffer));
                }
            }

            string allHeaders = null;
            //




























*/
#endif  //TRAVE

    } //END WinInet class
}
