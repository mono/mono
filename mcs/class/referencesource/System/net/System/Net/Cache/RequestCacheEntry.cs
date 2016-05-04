/*++
Copyright (c) Microsoft Corporation

Module Name:

    RequestCacheEntry.cs

Abstract:
    Describes the attributes of a cache entry.


Author:

    Alexei Vopilov    21-Dec-2002

Revision History:
    Aug 25 2003 moved to a new class, accomodated Whidbey-M3 changes

--*/
namespace System.Net.Cache {
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.IO;
    using System.Collections.Specialized;
    using System.Threading;
    using System.Globalization;

    internal class RequestCacheEntry {
        private bool        m_IsPrivateEntry;
        private long        m_StreamSize;
        private DateTime    m_ExpiresUtc;
        private int         m_HitCount;
        private DateTime    m_LastAccessedUtc;
        private DateTime    m_LastModifiedUtc;
        private DateTime    m_LastSynchronizedUtc;
        private TimeSpan    m_MaxStale;
        private int         m_UsageCount;
        private bool        m_IsPartialEntry;

        private StringCollection m_EntryMetadata;
        private StringCollection m_SystemMetadata;


        internal  RequestCacheEntry()
        {
            m_ExpiresUtc = m_LastAccessedUtc = m_LastModifiedUtc = m_LastSynchronizedUtc = DateTime.MinValue;
        }

#if !FEATURE_PAL
        // This ctor is for IERequestCache class only that relies on WinInet cache API.
        internal RequestCacheEntry(_WinInetCache.Entry entry, bool isPrivateEntry)
        {
            m_IsPrivateEntry = isPrivateEntry;
            m_StreamSize    = ((long)entry.Info.SizeHigh<<32) | (long)((ulong)entry.Info.SizeLow);
            m_ExpiresUtc    = (entry.Info.ExpireTime.IsNull? DateTime.MinValue: DateTime.FromFileTimeUtc(entry.Info.ExpireTime.ToLong()));
            m_HitCount      = entry.Info.HitRate;
            m_LastAccessedUtc= (entry.Info.LastAccessTime.IsNull? DateTime.MinValue: DateTime.FromFileTimeUtc(entry.Info.LastAccessTime.ToLong()));
            m_LastModifiedUtc= (entry.Info.LastModifiedTime.IsNull? DateTime.MinValue: DateTime.FromFileTimeUtc(entry.Info.LastModifiedTime.ToLong()));
            m_LastSynchronizedUtc= (entry.Info.LastSyncTime.IsNull? DateTime.MinValue: DateTime.FromFileTimeUtc(entry.Info.LastSyncTime.ToLong()));
            m_MaxStale      = TimeSpan.FromSeconds(entry.Info.U.ExemptDelta);
            if (m_MaxStale == Microsoft.Win32.WinInetCache.s_MaxTimeSpanForInt32)
            {
                m_MaxStale = TimeSpan.MaxValue;
            }
            m_UsageCount    = entry.Info.UseCount;
            m_IsPartialEntry = (entry.Info.EntryType & _WinInetCache.EntryType.Sparse) != 0;
        }
#endif // !FEATURE_PAL

        internal bool     IsPrivateEntry                          {get{return m_IsPrivateEntry;} set{m_IsPrivateEntry = value;}}
        internal long     StreamSize                              {get{return m_StreamSize;} set{m_StreamSize = value;}}
        internal DateTime ExpiresUtc                              {get{return m_ExpiresUtc;} set{m_ExpiresUtc = value;}}
        internal DateTime LastAccessedUtc                         {get{return m_LastAccessedUtc;} set{m_LastAccessedUtc = value;}}
        internal DateTime LastModifiedUtc                         {get{return m_LastModifiedUtc;} set{m_LastModifiedUtc = value;}}
        internal DateTime LastSynchronizedUtc                     {get{return m_LastSynchronizedUtc;} set{m_LastSynchronizedUtc = value;}}
        internal TimeSpan MaxStale                                {get{return m_MaxStale;} set{m_MaxStale = value;}}
        internal int      HitCount                                {get{return m_HitCount;} set{m_HitCount = value;}}
        internal int      UsageCount                              {get{return m_UsageCount;} set{m_UsageCount = value;}}
        internal bool     IsPartialEntry                          {get{return m_IsPartialEntry;} set{m_IsPartialEntry = value;}}

        internal StringCollection EntryMetadata                   {get{return m_EntryMetadata;} set{m_EntryMetadata = value;}}
        internal StringCollection SystemMetadata                  {get{return m_SystemMetadata;} set{m_SystemMetadata = value;}}

        internal virtual string ToString(bool verbose) {
            StringBuilder sb = new StringBuilder(512);
            sb.Append("\r\nIsPrivateEntry   = ").Append(IsPrivateEntry);
            sb.Append("\r\nIsPartialEntry   = ").Append(IsPartialEntry);
            sb.Append("\r\nStreamSize       = ").Append(StreamSize);
            sb.Append("\r\nExpires          = ").Append(ExpiresUtc == DateTime.MinValue? "": ExpiresUtc.ToString("r", CultureInfo.CurrentCulture));
            sb.Append("\r\nLastAccessed     = ").Append(LastAccessedUtc == DateTime.MinValue? "": LastAccessedUtc.ToString("r", CultureInfo.CurrentCulture));
            sb.Append("\r\nLastModified     = ").Append(LastModifiedUtc == DateTime.MinValue? "": LastModifiedUtc.ToString("r", CultureInfo.CurrentCulture));
            sb.Append("\r\nLastSynchronized = ").Append(LastSynchronizedUtc == DateTime.MinValue? "": LastSynchronizedUtc.ToString("r", CultureInfo.CurrentCulture));
            sb.Append("\r\nMaxStale(sec)    = ").Append(MaxStale == TimeSpan.MinValue? "": ((int)MaxStale.TotalSeconds).ToString(NumberFormatInfo.CurrentInfo));
            sb.Append("\r\nHitCount         = ").Append(HitCount.ToString(NumberFormatInfo.CurrentInfo));
            sb.Append("\r\nUsageCount       = ").Append(UsageCount.ToString(NumberFormatInfo.CurrentInfo));
            sb.Append("\r\n");
            if (verbose) {
                sb.Append("EntryMetadata:\r\n");
                if (m_EntryMetadata != null) {
                    foreach (string s in m_EntryMetadata) {
                        sb.Append(s).Append("\r\n");
                    }
                }
                sb.Append("---\r\nSystemMetadata:\r\n");
                if (m_SystemMetadata != null) {
                    foreach (string s in m_SystemMetadata) {
                        sb.Append(s).Append("\r\n");
                    }
                }
            }
            return sb.ToString();
        }
    }
}
