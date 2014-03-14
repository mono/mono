// Shared.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2006-2011 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// Last Saved: <2011-August-02 19:41:01>
//
// ------------------------------------------------------------------
//
// This module defines some shared utility classes and methods.
//
// Created: Tue, 27 Mar 2007  15:30
//

using System;
using System.IO;
using System.Security.Permissions;

namespace Ionic.Zip
{
    /// <summary>
    /// Collects general purpose utility methods.
    /// </summary>
    internal static class SharedUtilities
    {
        /// private null constructor
        //private SharedUtilities() { }

        // workitem 8423
        internal static Int64 GetFileLength(string fileName)
        {
            if (!File.Exists(fileName))
                throw new System.IO.FileNotFoundException(fileName);

            long fileLength = 0L;
            FileShare fs = FileShare.ReadWrite;
#if !NETCF
            // FileShare.Delete is not defined for the Compact Framework
            fs |= FileShare.Delete;
#endif
            using (var s = File.Open(fileName, FileMode.Open, FileAccess.Read, fs))
            {
                fileLength = s.Length;
            }
            return fileLength;
        }


        [System.Diagnostics.Conditional("NETCF")]
        internal static void Workaround_Ladybug318918(Stream s)
        {
            // This is a workaround for this issue:
            // https://connect.microsoft.com/VisualStudio/feedback/details/318918
            // It's required only on NETCF.
            s.Flush();
        }


#if LEGACY
        /// <summary>
        /// Round the given DateTime value to an even second value.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        /// Round up in the case of an odd second value.  The rounding does not consider
        /// fractional seconds.
        /// </para>
        /// <para>
        /// This is useful because the Zip spec allows storage of time only to the nearest
        /// even second.  So if you want to compare the time of an entry in the archive with
        /// it's actual time in the filesystem, you need to round the actual filesystem
        /// time, or use a 2-second threshold for the comparison.
        /// </para>
        /// <para>
        /// This is most nautrally an extension method for the DateTime class but this
        /// library is built for .NET 2.0, not for .NET 3.5; This means extension methods
        /// are a no-no.
        /// </para>
        /// </remarks>
        /// <param name="source">The DateTime value to round</param>
        /// <returns>The ruonded DateTime value</returns>
        internal static DateTime RoundToEvenSecond(DateTime source)
        {
            // round to nearest second:
            if ((source.Second % 2) == 1)
                source += new TimeSpan(0, 0, 1);

            DateTime dtRounded = new DateTime(source.Year, source.Month, source.Day, source.Hour, source.Minute, source.Second);
            //if (source.Millisecond >= 500) dtRounded = dtRounded.AddSeconds(1);
            return dtRounded;
        }
#endif

#if YOU_LIKE_REDUNDANT_CODE
        internal static string NormalizePath(string path)
        {
            // remove leading single dot slash
            if (path.StartsWith(".\\")) path = path.Substring(2);

            // remove intervening dot-slash
            path = path.Replace("\\.\\", "\\");

            // remove double dot when preceded by a directory name
            var re = new System.Text.RegularExpressions.Regex(@"^(.*\\)?([^\\\.]+\\\.\.\\)(.+)$");
            path = re.Replace(path, "$1$3");
            return path;
        }
#endif

        private static System.Text.RegularExpressions.Regex doubleDotRegex1 =
            new System.Text.RegularExpressions.Regex(@"^(.*/)?([^/\\.]+/\\.\\./)(.+)$");

        private static string SimplifyFwdSlashPath(string path)
        {
            if (path.StartsWith("./")) path = path.Substring(2);
            path = path.Replace("/./", "/");

            // Replace foo/anything/../bar with foo/bar
            path = doubleDotRegex1.Replace(path, "$1$3");
            return path;
        }


        /// <summary>
        /// Utility routine for transforming path names from filesystem format (on Windows that means backslashes) to
        /// a format suitable for use within zipfiles. This means trimming the volume letter and colon (if any) And
        /// swapping backslashes for forward slashes.
        /// </summary>
        /// <param name="pathName">source path.</param>
        /// <returns>transformed path</returns>
        internal static string NormalizePathForUseInZipFile(string pathName)
        {
            // boundary case
            if (String.IsNullOrEmpty(pathName)) return pathName;

            // trim volume if necessary
            if ((pathName.Length >= 2)  && ((pathName[1] == ':') && (pathName[2] == '\\')))
                pathName =  pathName.Substring(3);

            // swap slashes
            pathName = pathName.Replace('\\', '/');

            // trim all leading slashes
            while (pathName.StartsWith("/")) pathName = pathName.Substring(1);

            return SimplifyFwdSlashPath(pathName);
        }


        static System.Text.Encoding ibm437 = System.Text.Encoding.GetEncoding("IBM437");
        static System.Text.Encoding utf8 = System.Text.Encoding.GetEncoding("UTF-8");

        internal static byte[] StringToByteArray(string value, System.Text.Encoding encoding)
        {
            byte[] a = encoding.GetBytes(value);
            return a;
        }
        internal static byte[] StringToByteArray(string value)
        {
            return StringToByteArray(value, ibm437);
        }

        //internal static byte[] Utf8StringToByteArray(string value)
        //{
        //    return StringToByteArray(value, utf8);
        //}

        //internal static string StringFromBuffer(byte[] buf, int maxlength)
        //{
        //    return StringFromBuffer(buf, maxlength, ibm437);
        //}

        internal static string Utf8StringFromBuffer(byte[] buf)
        {
            return StringFromBuffer(buf, utf8);
        }

        internal static string StringFromBuffer(byte[] buf, System.Text.Encoding encoding)
        {
            // this form of the GetString() method is required for .NET CF compatibility
            string s = encoding.GetString(buf, 0, buf.Length);
            return s;
        }


        internal static int ReadSignature(System.IO.Stream s)
        {
            int x = 0;
            try { x = _ReadFourBytes(s, "n/a"); }
            catch (BadReadException) { }
            return x;
        }


        internal static int ReadEntrySignature(System.IO.Stream s)
        {
            // handle the case of ill-formatted zip archives - includes a data descriptor
            // when none is expected.
            int x = 0;
            try
            {
                x = _ReadFourBytes(s, "n/a");
                if (x == ZipConstants.ZipEntryDataDescriptorSignature)
                {
                    // advance past data descriptor - 12 bytes if not zip64
                    s.Seek(12, SeekOrigin.Current);
                    // workitem 10178
                    Workaround_Ladybug318918(s);
                    x = _ReadFourBytes(s, "n/a");
                    if (x != ZipConstants.ZipEntrySignature)
                    {
                        // Maybe zip64 was in use for the prior entry.
                        // Therefore, skip another 8 bytes.
                        s.Seek(8, SeekOrigin.Current);
                        // workitem 10178
                        Workaround_Ladybug318918(s);
                        x = _ReadFourBytes(s, "n/a");
                        if (x != ZipConstants.ZipEntrySignature)
                        {
                            // seek back to the first spot
                            s.Seek(-24, SeekOrigin.Current);
                            // workitem 10178
                            Workaround_Ladybug318918(s);
                            x = _ReadFourBytes(s, "n/a");
                        }
                    }
                }
            }
            catch (BadReadException) { }
            return x;
        }


        internal static int ReadInt(System.IO.Stream s)
        {
            return _ReadFourBytes(s, "Could not read block - no data!  (position 0x{0:X8})");
        }

        private static int _ReadFourBytes(System.IO.Stream s, string message)
        {
            int n = 0;
            byte[] block = new byte[4];
#if NETCF
            // workitem 9181
            // Reading here in NETCF sometimes reads "backwards". Seems to happen for
            // larger files.  Not sure why. Maybe an error in caching.  If the data is:
            //
            // 00100210: 9efa 0f00 7072 6f6a 6563 742e 6963 7750  ....project.icwP
            // 00100220: 4b05 0600 0000 0006 0006 0091 0100 008e  K...............
            // 00100230: 0010 0000 00                             .....
            //
            // ...and the stream Position is 10021F, then a Read of 4 bytes is returning
            // 50776369, instead of 06054b50. This seems to happen the 2nd time Read()
            // is called from that Position..
            //
            // submitted to connect.microsoft.com
            // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=318918#tabs
            //
            for (int i = 0; i < block.Length; i++)
            {
                n+= s.Read(block, i, 1);
            }
#else
            n = s.Read(block, 0, block.Length);
#endif
            if (n != block.Length) throw new BadReadException(String.Format(message, s.Position));
            int data = unchecked((((block[3] * 256 + block[2]) * 256) + block[1]) * 256 + block[0]);
            return data;
        }



        /// <summary>
        ///   Finds a signature in the zip stream. This is useful for finding
        ///   the end of a zip entry, for example, or the beginning of the next ZipEntry.
        /// </summary>
        ///
        /// <remarks>
        ///   <para>
        ///     Scans through 64k at a time.
        ///   </para>
        ///
        ///   <para>
        ///     If the method fails to find the requested signature, the stream Position
        ///     after completion of this method is unchanged. If the method succeeds in
        ///     finding the requested signature, the stream position after completion is
        ///     direct AFTER the signature found in the stream.
        ///   </para>
        /// </remarks>
        ///
        /// <param name="stream">The stream to search</param>
        /// <param name="SignatureToFind">The 4-byte signature to find</param>
        /// <returns>The number of bytes read</returns>
        internal static long FindSignature(System.IO.Stream stream, int SignatureToFind)
        {
            long startingPosition = stream.Position;

            int BATCH_SIZE = 65536; //  8192;
            byte[] targetBytes = new byte[4];
            targetBytes[0] = (byte)(SignatureToFind >> 24);
            targetBytes[1] = (byte)((SignatureToFind & 0x00FF0000) >> 16);
            targetBytes[2] = (byte)((SignatureToFind & 0x0000FF00) >> 8);
            targetBytes[3] = (byte)(SignatureToFind & 0x000000FF);
            byte[] batch = new byte[BATCH_SIZE];
            int n = 0;
            bool success = false;
            do
            {
                n = stream.Read(batch, 0, batch.Length);
                if (n != 0)
                {
                    for (int i = 0; i < n; i++)
                    {
                        if (batch[i] == targetBytes[3])
                        {
                            long curPosition = stream.Position;
                            stream.Seek(i - n, System.IO.SeekOrigin.Current);
                            // workitem 10178
                            Workaround_Ladybug318918(stream);

                            // workitem 7711
                            int sig = ReadSignature(stream);

                            success = (sig == SignatureToFind);
                            if (!success)
                            {
                                stream.Seek(curPosition, System.IO.SeekOrigin.Begin);
                                // workitem 10178
                                Workaround_Ladybug318918(stream);
                            }
                            else
                                break; // out of for loop
                        }
                    }
                }
                else break;
                if (success) break;

            } while (true);

            if (!success)
            {
                stream.Seek(startingPosition, System.IO.SeekOrigin.Begin);
                // workitem 10178
                Workaround_Ladybug318918(stream);
                return -1;  // or throw?
            }

            // subtract 4 for the signature.
            long bytesRead = (stream.Position - startingPosition) - 4;

            return bytesRead;
        }


        // If I have a time in the .NET environment, and I want to use it for
        // SetWastWriteTime() etc, then I need to adjust it for Win32.
        internal static DateTime AdjustTime_Reverse(DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc) return time;
            DateTime adjusted = time;
            if (DateTime.Now.IsDaylightSavingTime() && !time.IsDaylightSavingTime())
                adjusted = time - new System.TimeSpan(1, 0, 0);

            else if (!DateTime.Now.IsDaylightSavingTime() && time.IsDaylightSavingTime())
                adjusted = time + new System.TimeSpan(1, 0, 0);

            return adjusted;
        }

#if NECESSARY
        // If I read a time from a file with GetLastWriteTime() (etc), I need
        // to adjust it for display in the .NET environment.
        internal static DateTime AdjustTime_Forward(DateTime time)
        {
            if (time.Kind == DateTimeKind.Utc) return time;
            DateTime adjusted = time;
            if (DateTime.Now.IsDaylightSavingTime() && !time.IsDaylightSavingTime())
                adjusted = time + new System.TimeSpan(1, 0, 0);

            else if (!DateTime.Now.IsDaylightSavingTime() && time.IsDaylightSavingTime())
                adjusted = time - new System.TimeSpan(1, 0, 0);

            return adjusted;
        }
#endif


        internal static DateTime PackedToDateTime(Int32 packedDateTime)
        {
            // workitem 7074 & workitem 7170
            if (packedDateTime == 0xFFFF || packedDateTime == 0)
                return new System.DateTime(1995, 1, 1, 0, 0, 0, 0);  // return a fixed date when none is supplied.

            Int16 packedTime = unchecked((Int16)(packedDateTime & 0x0000ffff));
            Int16 packedDate = unchecked((Int16)((packedDateTime & 0xffff0000) >> 16));

            int year = 1980 + ((packedDate & 0xFE00) >> 9);
            int month = (packedDate & 0x01E0) >> 5;
            int day = packedDate & 0x001F;

            int hour = (packedTime & 0xF800) >> 11;
            int minute = (packedTime & 0x07E0) >> 5;
            //int second = packedTime & 0x001F;
            int second = (packedTime & 0x001F) * 2;

            // validation and error checking.
            // this is not foolproof but will catch most errors.
            if (second >= 60) { minute++; second = 0; }
            if (minute >= 60) { hour++; minute = 0; }
            if (hour >= 24) { day++; hour = 0; }

            DateTime d = System.DateTime.Now;
            bool success= false;
            try
            {
                d = new System.DateTime(year, month, day, hour, minute, second, 0);
                success= true;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                if (year == 1980 && (month == 0 || day == 0))
                {
                    try
                    {
                        d = new System.DateTime(1980, 1, 1, hour, minute, second, 0);
                success= true;
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                        try
                        {
                            d = new System.DateTime(1980, 1, 1, 0, 0, 0, 0);
                success= true;
                        }
                        catch (System.ArgumentOutOfRangeException) { }

                    }
                }
                // workitem 8814
                // my god, I can't believe how many different ways applications
                // can mess up a simple date format.
                else
                {
                    try
                    {
                        while (year < 1980) year++;
                        while (year > 2030) year--;
                        while (month < 1) month++;
                        while (month > 12) month--;
                        while (day < 1) day++;
                        while (day > 28) day--;
                        while (minute < 0) minute++;
                        while (minute > 59) minute--;
                        while (second < 0) second++;
                        while (second > 59) second--;
                        d = new System.DateTime(year, month, day, hour, minute, second, 0);
                        success= true;
                    }
                    catch (System.ArgumentOutOfRangeException) { }
                }
            }
            if (!success)
            {
                string msg = String.Format("y({0}) m({1}) d({2}) h({3}) m({4}) s({5})", year, month, day, hour, minute, second);
                throw new ZipException(String.Format("Bad date/time format in the zip file. ({0})", msg));

            }
            // workitem 6191
            //d = AdjustTime_Reverse(d);
            d = DateTime.SpecifyKind(d, DateTimeKind.Local);
            return d;
        }


        internal
         static Int32 DateTimeToPacked(DateTime time)
        {
            // The time is passed in here only for purposes of writing LastModified to the
            // zip archive. It should always be LocalTime, but we convert anyway.  And,
            // since the time is being written out, it needs to be adjusted.

            time = time.ToLocalTime();
            // workitem 7966
            //time = AdjustTime_Forward(time);

            // see http://www.vsft.com/hal/dostime.htm for the format
            UInt16 packedDate = (UInt16)((time.Day & 0x0000001F) | ((time.Month << 5) & 0x000001E0) | (((time.Year - 1980) << 9) & 0x0000FE00));
            UInt16 packedTime = (UInt16)((time.Second / 2 & 0x0000001F) | ((time.Minute << 5) & 0x000007E0) | ((time.Hour << 11) & 0x0000F800));

            Int32 result = (Int32)(((UInt32)(packedDate << 16)) | packedTime);
            return result;
        }


        /// <summary>
        ///   Create a pseudo-random filename, suitable for use as a temporary
        ///   file, and open it.
        /// </summary>
        /// <remarks>
        /// <para>
        ///   The System.IO.Path.GetRandomFileName() method is not available on
        ///   the Compact Framework, so this library provides its own substitute
        ///   on NETCF.
        /// </para>
        /// <para>
        ///   This method produces a filename of the form
        ///   DotNetZip-xxxxxxxx.tmp, where xxxxxxxx is replaced by randomly
        ///   chosen characters, and creates that file.
        /// </para>
        /// </remarks>
        internal static void CreateAndOpenUniqueTempFile(string dir,
                                                       out Stream fs,
                                                       out string filename)
        {
            // workitem 9763
            // http://dotnet.org.za/markn/archive/2006/04/15/51594.aspx
            // try 3 times:
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    filename = Path.Combine(dir, InternalGetTempFileName());
                    fs = new FileStream(filename, FileMode.CreateNew);
                    return;
                }
                catch (IOException)
                {
                    if (i == 2) throw;
                }
            }
            throw new IOException();
        }

#if NETCF || SILVERLIGHT
        internal static string InternalGetTempFileName()
        {
            return "DotNetZip-" + GenerateRandomStringImpl(8,0) + ".tmp";
        }

        internal static string GenerateRandomStringImpl(int length, int delta)
        {
            bool WantMixedCase = (delta == 0);
            System.Random rnd = new System.Random();

            string result = "";
            char[] a = new char[length];

            for (int i = 0; i < length; i++)
            {
               // delta == 65 means uppercase
               // delta == 97 means lowercase
                if (WantMixedCase)
                    delta = (rnd.Next(2) == 0) ? 65 : 97;
                a[i] = (char)(rnd.Next(26) + delta);
            }

            result = new System.String(a);
            return result;
        }
#else
        internal static string InternalGetTempFileName()
        {
            return "DotNetZip-" + Path.GetRandomFileName().Substring(0, 8) + ".tmp";
        }

#endif


        /// <summary>
        /// Workitem 7889: handle ERROR_LOCK_VIOLATION during read
        /// </summary>
        /// <remarks>
        /// This could be gracefully handled with an extension attribute, but
        /// This assembly is built for .NET 2.0, so I cannot use them.
        /// </remarks>
        internal static int ReadWithRetry(System.IO.Stream s, byte[] buffer, int offset, int count, string FileName)
        {
            int n = 0;
            bool done = false;
#if !NETCF && !SILVERLIGHT
            int retries = 0;
#endif
            do
            {
                try
                {
                    n = s.Read(buffer, offset, count);
                    done = true;
                }
#if NETCF || SILVERLIGHT
                catch (System.IO.IOException)
                {
                    throw;
                }
#else
                catch (System.IO.IOException ioexc1)
                {
                    // Check if we can call GetHRForException,
                    // which makes unmanaged code calls.
                    var p = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                    if (p.IsUnrestricted())
                    {
                        uint hresult = _HRForException(ioexc1);
                        if (hresult != 0x80070021)  // ERROR_LOCK_VIOLATION
                            throw new System.IO.IOException(String.Format("Cannot read file {0}", FileName), ioexc1);
                        retries++;
                        if (retries > 10)
                            throw new System.IO.IOException(String.Format("Cannot read file {0}, at offset 0x{1:X8} after 10 retries", FileName, offset), ioexc1);

                        // max time waited on last retry = 250 + 10*550 = 5.75s
                        // aggregate time waited after 10 retries: 250 + 55*550 = 30.5s
                        System.Threading.Thread.Sleep(250 + retries * 550);
                    }
                    else
                    {
                        // The permission.Demand() failed. Therefore, we cannot call
                        // GetHRForException, and cannot do the subtle handling of
                        // ERROR_LOCK_VIOLATION.  Just bail.
                        throw;
                    }
                }
#endif
            }
            while (!done);

            return n;
        }


#if !NETCF
        // workitem 8009
        //
        // This method must remain separate.
        //
        // Marshal.GetHRForException() is needed to do special exception handling for
        // the read.  But, that method requires UnmanagedCode permissions, and is marked
        // with LinkDemand for UnmanagedCode.  In an ASP.NET medium trust environment,
        // where UnmanagedCode is restricted, will generate a SecurityException at the
        // time of JIT of the method that calls a method that is marked with LinkDemand
        // for UnmanagedCode. The SecurityException, if it is restricted, will occur
        // when this method is JITed.
        //
        // The Marshal.GetHRForException() is factored out of ReadWithRetry in order to
        // avoid the SecurityException at JIT compile time. Because _HRForException is
        // called only when the UnmanagedCode is allowed.  This means .NET never
        // JIT-compiles this method when UnmanagedCode is disallowed, and thus never
        // generates the JIT-compile time exception.
        //
#endif
        private static uint _HRForException(System.Exception ex1)
        {
            return unchecked((uint)System.Runtime.InteropServices.Marshal.GetHRForException(ex1));
        }

    }



    /// <summary>
    ///   A decorator stream. It wraps another stream, and performs bookkeeping
    ///   to keep track of the stream Position.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     In some cases, it is not possible to get the Position of a stream, let's
    ///     say, on a write-only output stream like ASP.NET's
    ///     <c>Response.OutputStream</c>, or on a different write-only stream
    ///     provided as the destination for the zip by the application.  In this
    ///     case, programmers can use this counting stream to count the bytes read
    ///     or written.
    ///   </para>
    ///   <para>
    ///     Consider the scenario of an application that saves a self-extracting
    ///     archive (SFX), that uses a custom SFX stub.
    ///   </para>
    ///   <para>
    ///     Saving to a filesystem file, the application would open the
    ///     filesystem file (getting a <c>FileStream</c>), save the custom sfx stub
    ///     into it, and then call <c>ZipFile.Save()</c>, specifying the same
    ///     FileStream. <c>ZipFile.Save()</c> does the right thing for the zipentry
    ///     offsets, by inquiring the Position of the <c>FileStream</c> before writing
    ///     any data, and then adding that initial offset into any ZipEntry
    ///     offsets in the zip directory. Everything works fine.
    ///   </para>
    ///   <para>
    ///     Now suppose the application is an ASPNET application and it saves
    ///     directly to <c>Response.OutputStream</c>. It's not possible for DotNetZip to
    ///     inquire the <c>Position</c>, so the offsets for the SFX will be wrong.
    ///   </para>
    ///   <para>
    ///     The workaround is for the application to use this class to wrap
    ///     <c>HttpResponse.OutputStream</c>, then write the SFX stub and the ZipFile
    ///     into that wrapper stream. Because <c>ZipFile.Save()</c> can inquire the
    ///     <c>Position</c>, it will then do the right thing with the offsets.
    ///   </para>
    /// </remarks>
    internal class CountingStream : System.IO.Stream
    {
        // workitem 12374: this class is now public
        private System.IO.Stream _s;
        private Int64 _bytesWritten;
        private Int64 _bytesRead;
        private Int64 _initialOffset;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="stream">The underlying stream</param>
        public CountingStream(System.IO.Stream stream)
            : base()
        {
            _s = stream;
            try
            {
                _initialOffset = _s.Position;
            }
            catch
            {
                _initialOffset = 0L;
            }
        }

        /// <summary>
        ///   Gets the wrapped stream.
        /// </summary>
        public Stream WrappedStream
        {
            get
            {
                return _s;
            }
        }

        /// <summary>
        ///   The count of bytes written out to the stream.
        /// </summary>
        public Int64 BytesWritten
        {
            get { return _bytesWritten; }
        }

        /// <summary>
        ///   the count of bytes that have been read from the stream.
        /// </summary>
        public Int64 BytesRead
        {
            get { return _bytesRead; }
        }

        /// <summary>
        ///    Adjust the byte count on the stream.
        /// </summary>
        ///
        /// <param name='delta'>
        ///   the number of bytes to subtract from the count.
        /// </param>
        ///
        /// <remarks>
        ///   <para>
        ///     Subtract delta from the count of bytes written to the stream.
        ///     This is necessary when seeking back, and writing additional data,
        ///     as happens in some cases when saving Zip files.
        ///   </para>
        /// </remarks>
        public void Adjust(Int64 delta)
        {
            _bytesWritten -= delta;
            if (_bytesWritten < 0)
                throw new InvalidOperationException();
            if (_s as CountingStream != null)
                ((CountingStream)_s).Adjust(delta);
        }

        /// <summary>
        ///   The read method.
        /// </summary>
        /// <param name="buffer">The buffer to hold the data read from the stream.</param>
        /// <param name="offset">the offset within the buffer to copy the first byte read.</param>
        /// <param name="count">the number of bytes to read.</param>
        /// <returns>the number of bytes read, after decryption and decompression.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int n = _s.Read(buffer, offset, count);
            _bytesRead += n;
            return n;
        }

        /// <summary>
        ///   Write data into the stream.
        /// </summary>
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count == 0) return;
            _s.Write(buffer, offset, count);
            _bytesWritten += count;
        }

        /// <summary>
        ///   Whether the stream can be read.
        /// </summary>
        public override bool CanRead
        {
            get { return _s.CanRead; }
        }

        /// <summary>
        ///   Whether it is possible to call Seek() on the stream.
        /// </summary>
        public override bool CanSeek
        {
            get { return _s.CanSeek; }
        }

        /// <summary>
        ///   Whether it is possible to call Write() on the stream.
        /// </summary>
        public override bool CanWrite
        {
            get { return _s.CanWrite; }
        }

        /// <summary>
        ///   Flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            _s.Flush();
        }

        /// <summary>
        ///   The length of the underlying stream.
        /// </summary>
        public override long Length
        {
            get { return _s.Length; }   // bytesWritten??
        }

        /// <summary>
        ///   Returns the sum of number of bytes written, plus the initial
        ///   offset before writing.
        /// </summary>
        public long ComputedPosition
        {
            get { return _initialOffset + _bytesWritten; }
        }


        /// <summary>
        ///   The Position of the stream.
        /// </summary>
        public override long Position
        {
            get { return _s.Position; }
            set
            {
                _s.Seek(value, System.IO.SeekOrigin.Begin);
                // workitem 10178
                Ionic.Zip.SharedUtilities.Workaround_Ladybug318918(_s);
            }
        }

        /// <summary>
        ///   Seek in the stream.
        /// </summary>
        /// <param name="offset">the offset point to seek to</param>
        /// <param name="origin">the reference point from which to seek</param>
        /// <returns>The new position</returns>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            return _s.Seek(offset, origin);
        }

        /// <summary>
        ///   Set the length of the underlying stream.  Be careful with this!
        /// </summary>
        ///
        /// <param name='value'>the length to set on the underlying stream.</param>
        public override void SetLength(long value)
        {
            _s.SetLength(value);
        }
    }


}
