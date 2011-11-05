/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;

/// <summary>
/// This interface should be implemented by any class whose instances are intended 
/// to be executed by a thread.
/// </summary>
public interface IThreadRunnable
{
    /// <summary>
    /// This method has to be implemented in order that starting of the thread causes the object's 
    /// run method to be called in that separately executing thread.
    /// </summary>
    void Run();
}

/// <summary>
/// Contains conversion support elements such as classes, interfaces and static methods.
/// </summary>
public class SupportClass
{
    public interface Checksum
    {
        void Reset();
        void Update(int b);
        void Update(byte[] b);
        void Update(byte[] b, int offset, int length);
        Int64 GetValue();
    }

    public class CRC32 : Checksum
    {
        private static readonly UInt32[] crcTable = InitializeCRCTable();

        private static UInt32[] InitializeCRCTable()
        {
            UInt32[] crcTable = new UInt32[256];
            for (UInt32 n = 0; n < 256; n++)
            {
                UInt32 c = n;
                for (int k = 8; --k >= 0; )
                {
                    if ((c & 1) != 0)
                        c = 0xedb88320 ^ (c >> 1);
                    else
                        c = c >> 1;
                }
                crcTable[n] = c;
            }
            return crcTable;
        }

        private UInt32 crc = 0;

        public Int64 GetValue()
        {
            return (Int64)crc & 0xffffffffL;
        }

        public void Reset()
        {
            crc = 0;
        }

        public void Update(int bval)
        {
            UInt32 c = ~crc;
            c = crcTable[(c ^ bval) & 0xff] ^ (c >> 8);
            crc = ~c;
        }

        public void Update(byte[] buf, int off, int len)
        {
            UInt32 c = ~crc;
            while (--len >= 0)
                c = crcTable[(c ^ buf[off++]) & 0xff] ^ (c >> 8);
            crc = ~c;
        }

        public void Update(byte[] buf)
        {
            Update(buf, 0, buf.Length);
        }
    }

    public class TextSupport
    {
        /// <summary>
        /// Copies an array of chars obtained from a String into a specified array of chars
        /// </summary>
        /// <param name="sourceString">The String to get the chars from</param>
        /// <param name="sourceStart">Position of the String to start getting the chars</param>
        /// <param name="sourceEnd">Position of the String to end getting the chars</param>
        /// <param name="destinationArray">Array to return the chars</param>
        /// <param name="destinationStart">Position of the destination array of chars to start storing the chars</param>
        /// <returns>An array of chars</returns>
        public static void GetCharsFromString(string sourceString, int sourceStart, int sourceEnd, char[] destinationArray, int destinationStart)
        {
            int sourceCounter;
            int destinationCounter;
            sourceCounter = sourceStart;
            destinationCounter = destinationStart;
            while (sourceCounter < sourceEnd)
            {
                destinationArray[destinationCounter] = (char)sourceString[sourceCounter];
                sourceCounter++;
                destinationCounter++;
            }
        }
    }

    /// <summary>
    /// Support class used to handle threads
    /// </summary>
    public class ThreadClass : IThreadRunnable
    {
        /// <summary>
        /// The instance of System.Threading.Thread
        /// </summary>
        private System.Threading.Thread threadField;


        /// <summary>
        /// Initializes a new instance of the ThreadClass class
        /// </summary>
        public ThreadClass()
        {
            threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Name">The name of the thread</param>
        public ThreadClass(System.String Name)
        {
            threadField = new System.Threading.Thread(new System.Threading.ThreadStart(Run));
            this.Name = Name;
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        public ThreadClass(System.Threading.ThreadStart Start)
        {
            threadField = new System.Threading.Thread(Start);
        }

        /// <summary>
        /// Initializes a new instance of the Thread class.
        /// </summary>
        /// <param name="Start">A ThreadStart delegate that references the methods to be invoked when this thread begins executing</param>
        /// <param name="Name">The name of the thread</param>
        public ThreadClass(System.Threading.ThreadStart Start, System.String Name)
        {
            threadField = new System.Threading.Thread(Start);
            this.Name = Name;
        }

        /// <summary>
        /// This method has no functionality unless the method is overridden
        /// </summary>
        public virtual void Run()
        {
        }

        /// <summary>
        /// Causes the operating system to change the state of the current thread instance to ThreadState.Running
        /// </summary>
        public virtual void Start()
        {
            threadField.Start();
        }

        /// <summary>
        /// Interrupts a thread that is in the WaitSleepJoin thread state
        /// </summary>
        public virtual void Interrupt()
        {
            threadField.Interrupt();
        }

        /// <summary>
        /// Gets the current thread instance
        /// </summary>
        public System.Threading.Thread Instance
        {
            get
            {
                return threadField;
            }
            set
            {
                threadField = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the thread
        /// </summary>
        public System.String Name
        {
            get
            {
                return threadField.Name;
            }
            set
            {
                if (threadField.Name == null)
                    threadField.Name = value;
            }
        }

        public void SetDaemon(bool isDaemon)
        {
            threadField.IsBackground = isDaemon;
        }

        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of a thread
        /// </summary>
        public System.Threading.ThreadPriority Priority
        {
            get
            {
                try
                {
                    return threadField.Priority;
                }
                catch
                {
                    return System.Threading.ThreadPriority.Normal;
                }
            }
            set
            {
                try
                {
                    threadField.Priority = value;
                }
                catch{}
                
            }
        }

        /// <summary>
        /// Gets a value indicating the execution status of the current thread
        /// </summary>
        public bool IsAlive
        {
            get
            {
                return threadField.IsAlive;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not a thread is a background thread.
        /// </summary>
        public bool IsBackground
        {
            get
            {
                return threadField.IsBackground;
            }
            set
            {
                threadField.IsBackground = value;
            }
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates
        /// </summary>
        public void Join()
        {
            threadField.Join();
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="MiliSeconds">Time of wait in milliseconds</param>
        public void Join(long MiliSeconds)
        {
            threadField.Join(new System.TimeSpan(MiliSeconds * 10000));
        }

        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="MiliSeconds">Time of wait in milliseconds</param>
        /// <param name="NanoSeconds">Time of wait in nanoseconds</param>
        public void Join(long MiliSeconds, int NanoSeconds)
        {
            threadField.Join(new System.TimeSpan(MiliSeconds * 10000 + NanoSeconds * 100));
        }

        /// <summary>
        /// Resumes a thread that has been suspended
        /// </summary>
        public void Resume()
        {
            System.Threading.Monitor.PulseAll(threadField);
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, 
        /// to begin the process of terminating the thread. Calling this method 
        /// usually terminates the thread
        /// </summary>
        public void Abort()
        {
            threadField.Abort();
        }

        /// <summary>
        /// Raises a ThreadAbortException in the thread on which it is invoked, 
        /// to begin the process of terminating the thread while also providing
        /// exception information about the thread termination. 
        /// Calling this method usually terminates the thread.
        /// </summary>
        /// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted</param>
        public void Abort(object stateInfo)
        {
            threadField.Abort(stateInfo);
        }

        /// <summary>
        /// Suspends the thread, if the thread is already suspended it has no effect
        /// </summary>
        public void Suspend()
        {
            System.Threading.Monitor.Wait(threadField);
        }

        /// <summary>
        /// Obtain a String that represents the current object
        /// </summary>
        /// <returns>A String that represents the current object</returns>
        public override System.String ToString()
        {
            return "Thread[" + Name + "," + Priority.ToString() + "]";
        }

        [ThreadStatic]
        static ThreadClass This = null;

        // named as the Java version
        public static ThreadClass CurrentThread()
        {
            return Current();
        }

        public static void Sleep(long ms)
        {
            // casting long ms to int ms could lose resolution, however unlikely
            // that someone would want to sleep for that long...
            System.Threading.Thread.Sleep((int)ms);
        }

        /// <summary>
        /// Gets the currently running thread
        /// </summary>
        /// <returns>The currently running thread</returns>
        public static ThreadClass Current()
        {
            if (This == null)
            {
                This = new ThreadClass();
                This.Instance = System.Threading.Thread.CurrentThread;
            }
            return This;
        }

        public static bool operator ==(ThreadClass t1, object t2)
        {
            if (((object)t1) == null) return t2 == null;
            return t1.Equals(t2);
        }

        public static bool operator !=(ThreadClass t1, object t2)
        {
            return !(t1 == t2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ThreadClass) return this.threadField.Equals( ((ThreadClass)obj).threadField  );
            return false;
        }

        public override int GetHashCode()
        {
            return this.threadField.GetHashCode();
        }
    }

    /// <summary>
    /// Represents the methods to support some operations over files.
    /// </summary>
    public class FileSupport
    {
        /// <summary>
        /// Returns an array of abstract pathnames representing the files and directories of the specified path.
        /// </summary>
        /// <param name="path">The abstract pathname to list it childs.</param>
        /// <returns>An array of abstract pathnames childs of the path specified or null if the path is not a directory</returns>
        public static System.IO.FileInfo[] GetFiles(System.IO.FileInfo path)
        {
            if ((path.Attributes & System.IO.FileAttributes.Directory) > 0)
            {																 
                String[] fullpathnames = System.IO.Directory.GetFileSystemEntries(path.FullName);
                System.IO.FileInfo[] result = new System.IO.FileInfo[fullpathnames.Length];
                for (int i = 0; i < result.Length ; i++)
                    result[i] = new System.IO.FileInfo(fullpathnames[i]);
                return result;
            }
            else
                return null;
        }

        /// <summary>
        /// Returns a list of files in a give directory.
        /// </summary>
        /// <param name="fullName">The full path name to the directory.</param>
        /// <param name="indexFileNameFilter"></param>
        /// <returns>An array containing the files.</returns>
        public static System.String[] GetLuceneIndexFiles(System.String fullName, 
                                                          Mono.Lucene.Net.Index.IndexFileNameFilter indexFileNameFilter)
        {
            System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(fullName);
            System.Collections.ArrayList list = new System.Collections.ArrayList();
            foreach (System.IO.FileInfo fInfo in dInfo.GetFiles())
            {
                if (indexFileNameFilter.Accept(fInfo, fInfo.Name) == true)
                {
                    list.Add(fInfo.Name);
                }
            }
            System.String[] retFiles = new System.String[list.Count];
            list.CopyTo(retFiles);
            return retFiles;
        }

        // Disable the obsolete warning since we must use FileStream.Handle
        // because Mono does not support FileSystem.SafeFileHandle at present.
#pragma warning disable 618

        /// <summary>
        /// Flushes the specified file stream. Ensures that all buffered
        /// data is actually written to the file system.
        /// </summary>
        /// <param name="fileStream">The file stream.</param>
        public static void Sync(System.IO.FileStream fileStream)
        {
            if (fileStream == null)
                throw new ArgumentNullException("fileStream");

            fileStream.Flush();

            //if (OS.IsWindows)
            //{
            //    if (!FlushFileBuffers(fileStream.Handle))
            //        throw new System.IO.IOException();
            //}
            //else if (OS.IsUnix)
            //{
            //    if (fsync(fileStream.Handle) != IntPtr.Zero)
            //    throw new System.IO.IOException();
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}
        }

#pragma warning restore 618

        //[System.Runtime.InteropServices.DllImport("libc")]
        //extern static IntPtr fsync(IntPtr fd);

        //[System.Runtime.InteropServices.DllImport("kernel32.dll")]
        //extern static bool FlushFileBuffers(IntPtr hFile);
    }

    /// <summary>
    /// A simple class for number conversions.
    /// </summary>
    public class Number
    {
        /// <summary>
        /// Min radix value.
        /// </summary>
        public const int MIN_RADIX = 2;
        /// <summary>
        /// Max radix value.
        /// </summary>
        public const int MAX_RADIX = 36;

        private const System.String digits = "0123456789abcdefghijklmnopqrstuvwxyz";


        /// <summary>
        /// Converts a number to System.String.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static System.String ToString(long number)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();

            if (number == 0)
            {
                s.Append("0");
            }
            else
            {
                if (number < 0)
                {
                    s.Append("-");
                    number = -number;
                }

                while (number > 0)
                {
                    char c = digits[(int)number % 36];
                    s.Insert(0, c);
                    number = number / 36;
                }
            }

            return s.ToString();
        }
           

        /// <summary>
        /// Converts a number to System.String.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static System.String ToString(float f)
        {
            if (((float)(int)f) == f)
            {
                return ((int)f).ToString() + ".0";
            }
            else
            {
                return f.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            }
        }

        /// <summary>
        /// Converts a number to System.String in the specified radix.
        /// </summary>
        /// <param name="i">A number to be converted.</param>
        /// <param name="radix">A radix.</param>
        /// <returns>A System.String representation of the number in the specified redix.</returns>
        public static System.String ToString(long i, int radix)
        {
            if (radix < MIN_RADIX || radix > MAX_RADIX)
                radix = 10;

            char[] buf = new char[65];
            int charPos = 64;
            bool negative = (i < 0);

            if (!negative) 
            {
                i = -i;
            }

            while (i <= -radix) 
            {
                buf[charPos--] = digits[(int)(-(i % radix))];
                i = i / radix;
            }
            buf[charPos] = digits[(int)(-i)];

            if (negative) 
            {
                buf[--charPos] = '-';
            }

            return new System.String(buf, charPos, (65 - charPos)); 
        }

        /// <summary>
        /// Parses a number in the specified radix.
        /// </summary>
        /// <param name="s">An input System.String.</param>
        /// <param name="radix">A radix.</param>
        /// <returns>The parsed number in the specified radix.</returns>
        public static long Parse(System.String s, int radix)
        {
            if (s == null) 
            {
                throw new ArgumentException("null");
            }

            if (radix < MIN_RADIX) 
            {
                throw new NotSupportedException("radix " + radix +
                    " less than Number.MIN_RADIX");
            }
            if (radix > MAX_RADIX) 
            {
                throw new NotSupportedException("radix " + radix +
                    " greater than Number.MAX_RADIX");
            }

            long result = 0;
            long mult = 1;

            s = s.ToLower();
            
            for (int i = s.Length - 1; i >= 0; i--)
            {
                int weight = digits.IndexOf(s[i]);
                if (weight == -1)
                    throw new FormatException("Invalid number for the specified radix");

                result += (weight * mult);
                mult *= radix;
            }

            return result;
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static int URShift(int number, int bits)
        {
            return (int) (((uint) number) >> bits);
        }


        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        public static long URShift(long number, int bits)
        {
            return (long) (((ulong) number) >> bits);
        }


        /// <summary>
        /// Returns the index of the first bit that is set to true that occurs 
        /// on or after the specified starting index. If no such bit exists 
        /// then -1 is returned.
        /// </summary>
        /// <param name="bits">The BitArray object.</param>
        /// <param name="fromIndex">The index to start checking from (inclusive).</param>
        /// <returns>The index of the next set bit.</returns>
        public static int NextSetBit(System.Collections.BitArray bits, int fromIndex)
        {
            for (int i = fromIndex; i < bits.Length; i++)
            {
                if (bits[i] == true)
                {
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// Converts a System.String number to long.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static long ToInt64(System.String s)
        {
            long number = 0;
            long factor;

            // handle negative number
            if (s.StartsWith("-"))
            {
                s = s.Substring(1);
                factor = -1;
            }
            else
            {
                factor = 1;
            }

            // generate number
            for (int i = s.Length - 1; i > -1; i--)
            {
                int n = digits.IndexOf(s[i]);

                // not supporting fractional or scientific notations
                if (n < 0)
                    throw new System.ArgumentException("Invalid or unsupported character in number: " + s[i]);

                number += (n * factor);
                factor *= 36;
            }

            return number;
        }
    }

    /// <summary>
    /// Mimics Java's Character class.
    /// </summary>
    public class Character
    {
        private const char charNull= '\0';
        private const char charZero = '0';
        private const char charA = 'a';

        /// <summary>
        /// </summary>
        public static int MAX_RADIX
        {
            get
            {
                return 36;
            }
        }

        /// <summary>
        /// </summary>
        public static int MIN_RADIX
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="digit"></param>
        /// <param name="radix"></param>
        /// <returns></returns>
        public static char ForDigit(int digit, int radix)
        {
            // if radix or digit is out of range,
            // return the null character.
            if (radix < Character.MIN_RADIX)
                return charNull;
            if (radix > Character.MAX_RADIX)
                return charNull;
            if (digit < 0)
                return charNull;
            if (digit >= radix)
                return charNull;

            // if digit is less than 10,
            // return '0' plus digit
            if (digit < 10)
                return (char) ( (int) charZero + digit);

            // otherwise, return 'a' plus digit.
            return (char) ((int) charA + digit - 10);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Double
    {
        public static System.Double Parse(System.String s)
        {
            try
            {
                return System.Double.Parse(s.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            }
            catch (OverflowException)
            {
                return double.MaxValue;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Single
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="style"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static System.Single Parse(System.String s, System.Globalization.NumberStyles style, System.IFormatProvider provider)
        {
            try
            {
                if (s.EndsWith("f") || s.EndsWith("F"))
                    return System.Single.Parse(s.Substring(0, s.Length - 1), style, provider);
                else
                    return System.Single.Parse(s, style, provider);
            }
            catch (System.FormatException fex)
            {
                throw fex;					
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static System.Single Parse(System.String s, System.IFormatProvider provider)
        {
            try
            {
                if (s.EndsWith("f") || s.EndsWith("F"))
                    return System.Single.Parse(s.Substring(0, s.Length - 1), provider);
                else
                    return System.Single.Parse(s, provider);
            }
            catch (System.FormatException fex)
            {
                throw fex;					
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static System.Single Parse(System.String s, System.Globalization.NumberStyles style)
        {
            try
            {
                if (s.EndsWith("f") || s.EndsWith("F"))
                    return System.Single.Parse(s.Substring(0, s.Length - 1), style);
                else
                    return System.Single.Parse(s, style);
            }
            catch(System.FormatException fex)
            {
                throw fex;					
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static System.Single Parse(System.String s)
        {
            try
            {
                if (s.EndsWith("f") || s.EndsWith("F"))
                    return System.Single.Parse(s.Substring(0, s.Length - 1).Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
                else
                    return System.Single.Parse(s.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator));
            }
            catch(System.FormatException fex)
            {
                throw fex;					
            }
        }

        public static bool TryParse(System.String s, out float f)
        {
            bool ok = false;

            if (s.EndsWith("f") || s.EndsWith("F"))
                ok = System.Single.TryParse(s.Substring(0, s.Length - 1).Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out f);
            else
                ok = System.Single.TryParse(s.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out f);

            return ok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static string ToString(float f)
        {
            return f.ToString().Replace(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToString(float f, string format)
        {
            return f.ToString(format).Replace(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
        }

        public static int FloatToIntBits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        public static float IntBitsToFloat(int value)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AppSettings
    {
        static System.Collections.Specialized.ListDictionary settings = new System.Collections.Specialized.ListDictionary();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        public static void Set(System.String key, int defValue)
        {
            settings[key] = defValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        public static void Set(System.String key, long defValue)
        {
            settings[key] = defValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void Set(System.String key, System.String defValue)
        {
            settings[key] = defValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public static void Set(System.String key, bool defValue)
        {
            settings[key] = defValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static int Get(System.String key, int defValue)
        {
            if (settings[key] != null)
            {
                return (int) settings[key];
            }

            System.String theValue = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            if (theValue == null)
            {
                return defValue;
            }
            int retValue = System.Convert.ToInt32(theValue.Trim());
            settings[key] = retValue;
            return retValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static long Get(System.String key, long defValue)
        {
            if (settings[key] != null)
            {
                return (long) settings[key];
            }

            System.String theValue = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            if (theValue == null)
            {
                return defValue;
            }
            long retValue = System.Convert.ToInt64(theValue.Trim());
            settings[key] = retValue;
            return retValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static System.String Get(System.String key, System.String defValue)
        {
            if (settings[key] != null)
            {
                return (System.String) settings[key];
            }

            System.String theValue = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            if (theValue == null)
            {
                return defValue;
            }
            settings[key] = theValue;
            return theValue;
        }

        public static bool Get(System.String key, bool defValue)
        {
            if (settings[key] != null)
            {
                return (bool)settings[key];
            }

            System.String theValue = System.Configuration.ConfigurationManager.AppSettings.Get(key);
            if (theValue == null)
            {
                return defValue;
            }
            bool retValue = System.Convert.ToBoolean(theValue.Trim());
            settings[key] = retValue;
            return retValue;
        }
    }

    /// <summary>
    /// This class provides supporting methods of java.util.BitSet
    /// that are not present in System.Collections.BitArray.
    /// </summary>
    public class BitSetSupport
    {
        /// <summary>
        /// Returns the next set bit at or after index, or -1 if no such bit exists.
        /// </summary>
        /// <param name="bitArray"></param>
        /// <param name="index">the index of bit array at which to start checking</param>
        /// <returns>the next set bit or -1</returns>
        public static int NextSetBit(System.Collections.BitArray bitArray, int index)
        {
            while (index < bitArray.Length)
            {
                // if index bit is set, return it
                // otherwise check next index bit
                if (bitArray.Get(index))
                    return index;
                else
                    index++;
            }
            // if no bits are set at or after index, return -1
            return -1;
        }

        /// <summary>
        /// Returns the next un-set bit at or after index, or -1 if no such bit exists.
        /// </summary>
        /// <param name="bitArray"></param>
        /// <param name="index">the index of bit array at which to start checking</param>
        /// <returns>the next set bit or -1</returns>
        public static int NextClearBit(System.Collections.BitArray bitArray, int index)
        {
            while (index < bitArray.Length)
            {
                // if index bit is not set, return it
                // otherwise check next index bit
                if (!bitArray.Get(index))
                    return index;
                else
                    index++;
            }
            // if no bits are set at or after index, return -1
            return -1;
        }

        /// <summary>
        /// Returns the number of bits set to true in this BitSet.
        /// </summary>
        /// <param name="bits">The BitArray object.</param>
        /// <returns>The number of bits set to true in this BitSet.</returns>
        public static int Cardinality(System.Collections.BitArray bits)
        {
            int count = 0;
            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Summary description for TestSupportClass.
    /// </summary>
    public class Compare
    {
        /// <summary>
        /// Compares two Term arrays for equality.
        /// </summary>
        /// <param name="t1">First Term array to compare</param>
        /// <param name="t2">Second Term array to compare</param>
        /// <returns>true if the Terms are equal in both arrays, false otherwise</returns>
        public static bool CompareTermArrays(Mono.Lucene.Net.Index.Term[] t1, Mono.Lucene.Net.Index.Term[] t2)
        {
            if (t1.Length != t2.Length)
                return false;
            for (int i = 0; i < t1.Length; i++)
            {
                if (t1[i].CompareTo(t2[i]) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    #region WEAKHASHTABLE
    /// <summary>
    /// A Hashtable which holds weak references to its keys so they
    /// can be collected during GC. 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Count = {Values.Count}")]
    public class WeakHashTable : Hashtable, IEnumerable
    {
        /// <summary>
        /// A weak referene wrapper for the hashtable keys. Whenever a key\value pair 
        /// is added to the hashtable, the key is wrapped using a WeakKey. WeakKey saves the
        /// value of the original object hashcode for fast comparison.
        /// </summary>
        class WeakKey 
        {
            WeakReference reference;
            int hashCode;

            public WeakKey(object key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                hashCode = key.GetHashCode();
                reference = new WeakReference(key);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }

            public object Target
            {
                get { return reference.Target; }
            }

            public bool IsAlive
            {
                get { return reference.IsAlive; }
            }
        }

        /// <summary>
        /// A Dictionary enumerator which wraps the original hashtable enumerator 
        /// and performs 2 tasks: Extract the real key from a WeakKey and skip keys
        /// that were already collected.
        /// </summary>
        class WeakDictionaryEnumerator : IDictionaryEnumerator
        {
            IDictionaryEnumerator baseEnumerator;
            object currentKey;
            object currentValue;

            public WeakDictionaryEnumerator(IDictionaryEnumerator baseEnumerator)
            {
                this.baseEnumerator = baseEnumerator;
            }

            public DictionaryEntry Entry
            {
                get
                {
                    return new DictionaryEntry(this.currentKey, this.currentValue);
                }
            }

            public object Key
            {
                get
                {
                    return this.currentKey;
                }
            }

            public object Value
            {
                get
                {
                    return this.currentValue;
                }
            }

            public object Current
            {
                get
                {
                    return Entry;
                }
            }

            public bool MoveNext()
            {
                while (baseEnumerator.MoveNext())
                {
                    object key = ((WeakKey)baseEnumerator.Key).Target;
                    if (key != null)
                    {
                        this.currentKey = key;
                        this.currentValue = baseEnumerator.Value;
                        return true;
                    }
                }
                return false;
            }

            public void Reset()
            {
                baseEnumerator.Reset();
                this.currentKey = null;
                this.currentValue = null;
            }
        }


        /// <summary>
        /// Serves as a simple "GC Monitor" that indicates whether cleanup is needed. 
        /// If collectableObject.IsAlive is false, GC has occurred and we should perform cleanup
        /// </summary>
        WeakReference collectableObject = new WeakReference(new Object());

        /// <summary>
        /// Customize the hashtable lookup process by overriding KeyEquals. KeyEquals
        /// will compare both WeakKey to WeakKey and WeakKey to real keys
        /// </summary>
        protected override bool KeyEquals(object x, object y)
        {
            if (x == y)
                return true;

            if (x is WeakKey)
            {
                x = ((WeakKey)x).Target;
                if (x == null)
                    return false;
            }

            if (y is WeakKey)
            {
                y = ((WeakKey)y).Target;
                if (y == null)
                    return false;
            }

            return x.Equals(y);
        }

        protected override int GetHash(object key)
        {
            return key.GetHashCode();
        }

        /// <summary>
        /// Perform cleanup if GC occurred
        /// </summary>
        private void CleanIfNeeded()
        {
            if (collectableObject.Target == null)
            {
                Clean();
                collectableObject = new WeakReference(new Object());
            }
        }

        /// <summary>
        /// Iterate over all keys and remove keys that were collected
        /// </summary>
        private void Clean()
        {
            foreach (WeakKey wtk in ((Hashtable)base.Clone()).Keys)
            {
                if (!wtk.IsAlive)
                {
                    Remove(wtk);
                }
            }
        }


        /// <summary>
        /// Wrap each key with a WeakKey and add it to the hashtable
        /// </summary>
        public override void Add(object key, object value)
        {
            CleanIfNeeded();
            base.Add(new WeakKey(key), value);
        }

        public override IDictionaryEnumerator GetEnumerator()
        {
            Hashtable tmp = null;
            tmp = (Hashtable)base.Clone();
            return new WeakDictionaryEnumerator(tmp.GetEnumerator());
        }

        /// <summary>
        /// Create a temporary copy of the real keys and return that
        /// </summary>
        public override ICollection Keys
        {
            get
            {
                ArrayList keys = new ArrayList(Count);
                Hashtable tmpTable = (Hashtable)base.Clone();
                
                foreach (WeakKey key in tmpTable.Keys)
                {
                    object realKey = key.Target;
                    if (realKey != null)
                        keys.Add(realKey);
                }
                
                return keys;
            }
        }

        public override object this[object key]
        {
            get
            {
                return base[key];
            }
            set
            {
                CleanIfNeeded();
                base[new WeakKey(key)] = value;
            }
        }

        public override void CopyTo(Array array, int index)
        {
            int arrayIndex = index;
            foreach (DictionaryEntry de in this)
            {
                array.SetValue(de, arrayIndex++);
            }
        }

        public override int Count
        {
            get
            {
                CleanIfNeeded();
                return base.Count;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    #endregion

    public class Cryptography
    {
        static public bool FIPSCompliant = false;

        static public System.Security.Cryptography.HashAlgorithm GetHashAlgorithm()
        {
            if (FIPSCompliant)
            {
                //LUCENENET-175
                //No Assumptions should be made on the HashAlgorithm. It may change in time.
                //SHA256 SHA384 SHA512 etc.
                return System.Security.Cryptography.SHA1.Create();
            }
            return System.Security.Cryptography.MD5.Create();
        }
    }

    /// <summary>
    /// Support class used to handle Hashtable addition, which does a check 
    /// first to make sure the added item is unique in the hash.
    /// </summary>
    public class CollectionsHelper
    {
        public static void Add(System.Collections.Hashtable hashtable, System.Object item)
        {
            hashtable.Add(item, item);
        }

        public static void AddIfNotContains(System.Collections.Hashtable hashtable, System.Object item)
        {
            if (hashtable.Contains(item) == false)
            {
                hashtable.Add(item, item);
            }
        }

        public static void AddIfNotContains(System.Collections.ArrayList hashtable, System.Object item)
        {
            if (hashtable.Contains(item) == false)
            {
                hashtable.Add(item);
            }
        }

        public static void AddAll(System.Collections.Hashtable hashtable, System.Collections.ICollection items)
        {
            System.Collections.IEnumerator iter = items.GetEnumerator();
            System.Object item;
            while (iter.MoveNext())
            {
                item = iter.Current;
                hashtable.Add(item, item);
            }
        }

        public static void AddAllIfNotContains(System.Collections.Hashtable hashtable, System.Collections.IList items)
        {
            System.Object item;
            for (int i = 0; i < items.Count; i++)
            {
                item = items[i];
                if (hashtable.Contains(item) == false)
                {
                    hashtable.Add(item, item);
                }
            }
        }

        public static void AddAllIfNotContains(System.Collections.Hashtable hashtable, System.Collections.ICollection items)
        {
            System.Collections.IEnumerator iter = items.GetEnumerator();
            System.Object item;
            while (iter.MoveNext())
            {
                item = iter.Current;
                if (hashtable.Contains(item) == false)
                {
                    hashtable.Add(item, item);
                }
            }
        }

        public static void AddAllIfNotContains(System.Collections.Generic.IDictionary<string,string> hashtable, System.Collections.Generic.ICollection<string> items)
        {
            foreach (string s in items)
            {
                if (hashtable.ContainsKey(s) == false)
                {
                    hashtable.Add(s, s);
                }
            }
        }

        public static void AddAll(System.Collections.Generic.IDictionary<string, string> hashtable, System.Collections.Generic.ICollection<string> items)
        {
            foreach (string s in items)
            {
                hashtable.Add(s, s);
            }
        }

        public static bool Contains(System.Collections.Generic.ICollection<string> col, string item)
        {
            foreach (string s in col) if (s == item) return true;
            return false;
        }

        public static bool Contains(System.Collections.ICollection col, System.Object item)
        {
            System.Collections.IEnumerator iter = col.GetEnumerator();
            while (iter.MoveNext())
            {
                if (iter.Current.Equals(item))
                    return true;
            }
            return false;
        }


        public static System.String CollectionToString(System.Collections.Generic.IDictionary<string, string> c)
        {
            Hashtable t = new Hashtable();
            foreach (string key in c.Keys)
            {
                t.Add(key, c[key]);
            }
            return CollectionToString(t);
        }

        /// <summary>
        /// Converts the specified collection to its string representation.
        /// </summary>
        /// <param name="c">The collection to convert to string.</param>
        /// <returns>A string representation of the specified collection.</returns>
        public static System.String CollectionToString(System.Collections.ICollection c)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();

            if (c != null)
            {

                System.Collections.ArrayList l = new System.Collections.ArrayList(c);

                bool isDictionary = (c is System.Collections.BitArray || c is System.Collections.Hashtable || c is System.Collections.IDictionary || c is System.Collections.Specialized.NameValueCollection || (l.Count > 0 && l[0] is System.Collections.DictionaryEntry));
                for (int index = 0; index < l.Count; index++)
                {
                    if (l[index] == null)
                        s.Append("null");
                    else if (!isDictionary)
                        s.Append(l[index]);
                    else
                    {
                        isDictionary = true;
                        if (c is System.Collections.Specialized.NameValueCollection)
                            s.Append(((System.Collections.Specialized.NameValueCollection)c).GetKey(index));
                        else
                            s.Append(((System.Collections.DictionaryEntry)l[index]).Key);
                        s.Append("=");
                        if (c is System.Collections.Specialized.NameValueCollection)
                            s.Append(((System.Collections.Specialized.NameValueCollection)c).GetValues(index)[0]);
                        else
                            s.Append(((System.Collections.DictionaryEntry)l[index]).Value);

                    }
                    if (index < l.Count - 1)
                        s.Append(", ");
                }

                if (isDictionary)
                {
                    if (c is System.Collections.ArrayList)
                        isDictionary = false;
                }
                if (isDictionary)
                {
                    s.Insert(0, "{");
                    s.Append("}");
                }
                else
                {
                    s.Insert(0, "[");
                    s.Append("]");
                }
            }
            else
                s.Insert(0, "null");
            return s.ToString();
        }

        /// <summary>
        /// Compares two string arrays for equality.
        /// </summary>
        /// <param name="l1">First string array list to compare</param>
        /// <param name="l2">Second string array list to compare</param>
        /// <returns>true if the strings are equal in both arrays, false otherwise</returns>
        public static bool CompareStringArrays(System.String[] l1, System.String[] l2)
        {
            if (l1.Length != l2.Length)
                return false;
            for (int i = 0; i < l1.Length; i++)
            {
                if (l1[i] != l2[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Sorts an IList collections
        /// </summary>
        /// <param name="list">The System.Collections.IList instance that will be sorted</param>
        /// <param name="Comparator">The Comparator criteria, null to use natural comparator.</param>
        public static void Sort(System.Collections.IList list, System.Collections.IComparer Comparator)
        {
            if (((System.Collections.ArrayList)list).IsReadOnly)
                throw new System.NotSupportedException();

            if ((Comparator == null) || (Comparator is System.Collections.Comparer))
            {
                try
                {
                    ((System.Collections.ArrayList)list).Sort();
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
            }
            else
            {
                try
                {
                    ((System.Collections.ArrayList)list).Sort(Comparator);
                }
                catch (System.InvalidOperationException e)
                {
                    throw new System.InvalidCastException(e.Message);
                }
            }
        }

        /// <summary>
        /// Fills the array with an specific value from an specific index to an specific index.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="fromindex">The first index to be filled.</param>
        /// <param name="toindex">The last index to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill(System.Array array, System.Int32 fromindex, System.Int32 toindex, System.Object val)
        {
            System.Object Temp_Object = val;
            System.Type elementtype = array.GetType().GetElementType();
            if (elementtype != val.GetType())
                Temp_Object = System.Convert.ChangeType(val, elementtype);
            if (array.Length == 0)
                throw (new System.NullReferenceException());
            if (fromindex > toindex)
                throw (new System.ArgumentException());
            if ((fromindex < 0) || ((System.Array)array).Length < toindex)
                throw (new System.IndexOutOfRangeException());
            for (int index = (fromindex > 0) ? fromindex-- : fromindex; index < toindex; index++)
                array.SetValue(Temp_Object, index);
        }


        /// <summary>
        /// Fills the array with an specific value.
        /// </summary>
        /// <param name="array">The array to be filled.</param>
        /// <param name="val">The value to fill the array with.</param>
        public static void Fill(System.Array array, System.Object val)
        {
            Fill(array, 0, array.Length, val);
        }

        /// <summary>
        /// Compares the entire members of one array whith the other one.
        /// </summary>
        /// <param name="array1">The array to be compared.</param>
        /// <param name="array2">The array to be compared with.</param>
        /// <returns>Returns true if the two specified arrays of Objects are equal 
        /// to one another. The two arrays are considered equal if both arrays 
        /// contain the same number of elements, and all corresponding pairs of 
        /// elements in the two arrays are equal. Two objects e1 and e2 are 
        /// considered equal if (e1==null ? e2==null : e1.equals(e2)). In other 
        /// words, the two arrays are equal if they contain the same elements in 
        /// the same order. Also, two array references are considered equal if 
        /// both are null.</returns>
        public static bool Equals(System.Array array1, System.Array array2)
        {
            bool result = false;
            if ((array1 == null) && (array2 == null))
                result = true;
            else if ((array1 != null) && (array2 != null))
            {
                if (array1.Length == array2.Length)
                {
                    int length = array1.Length;
                    result = true;
                    for (int index = 0; index < length; index++)
                    {
                        System.Object o1 = array1.GetValue(index);
                        System.Object o2 = array2.GetValue(index);
                        if (o1 == null && o2 == null)
                            continue;   // they match
                        else if (o1 == null || !o1.Equals(o2))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }

    /// <summary>A collection of <typeparamref name="TItem"/> which can be
    /// looked up by instances of <typeparamref name="TKey"/>.</summary>
    /// <typeparam name="TItem">The type of the items contains in this
    /// collection.</typeparam>
    /// <typeparam name="TKey">The type of the keys that can be used to look
    /// up the items.</typeparam>
    internal class GeneralKeyedCollection<TKey, TItem> : System.Collections.ObjectModel.KeyedCollection<TKey, TItem>
    {
        /// <summary>Creates a new instance of the
        /// <see cref="GeneralKeyedCollection"/> class.</summary>
        /// <param name="converter">The <see cref="Converter{TInput, TOutput}"/> which will convert
        /// instances of <typeparamref name="TItem"/> to <typeparamref name="TKey"/>
        /// when the override of <see cref="GetKeyForItem(TItem)"/> is called.</param>
        internal GeneralKeyedCollection(Converter<TItem, TKey> converter) : base()
        {
            // If the converter is null, throw an exception.
            if (converter == null) throw new ArgumentNullException("converter");

            // Store the converter.
            this.converter = converter;

            // That's all folks.
            return;
        }

        /// <summary>The <see cref="Converter{TInput, TOutput}"/> which will convert
        /// instances of <typeparamref name="TItem"/> to <typeparamref name="TKey"/>
        /// when the override of <see cref="GetKeyForItem(TItem)"/> is called.</summary>
        private readonly Converter<TItem, TKey> converter;

        /// <summary>Converts an item that is added to the collection to
        /// a key.</summary>
        /// <param name="item">The instance of <typeparamref name="TItem"/>
        /// to convert into an instance of <typeparamref name="TKey"/>.</param>
        /// <returns>The instance of <typeparamref name="TKey"/> which is the
        /// key for this item.</returns>
        protected override TKey GetKeyForItem(TItem item)
        {
            // The converter is not null.
            System.Diagnostics.Debug.Assert(converter != null);

            // Call the converter.
            return converter(item);
        }

        /// <summary>Determines if a key for an item exists in this
        /// collection.</summary>
        /// <param name="key">The instance of <typeparamref name="TKey"/>
        /// to see if it exists in this collection.</param>
        /// <returns>True if the key exists in the collection, false otherwise.</returns>
        public bool ContainsKey(TKey key)
        {
            // Call the dictionary - it is lazily created when the first item is added
            if (Dictionary != null)
            {
                return Dictionary.ContainsKey(key);
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>Represents a strongly typed list of objects that can be accessed by index.
    /// Provides methods to search, sort, and manipulate lists. Also provides functionality
    /// to compare lists against each other through an implementations of
    /// <see cref="IEquatable{T}"/>.</summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [Serializable]
    public class EquatableList<T> : System.Collections.Generic.List<T>,
        IEquatable<System.Collections.Generic.IEnumerable<T>>,
        ICloneable
    {
        /// <summary>Initializes a new instance of the 
        /// <see cref="ComparableList{T}"/> class that is empty and has the 
        /// default initial capacity.</summary>
        public EquatableList() : base() { }

        /// <summary>Initializes a new instance of the <see cref="ComparableList{T}"/>
        /// class that contains elements copied from the specified collection and has
        /// sufficient capacity to accommodate the number of elements copied.</summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public EquatableList(System.Collections.Generic.IEnumerable<T> collection) : base(collection) { }

        /// <summary>Initializes a new instance of the <see cref="ComparableList{T}"/> 
        /// class that is empty and has the specified initial capacity.</summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public EquatableList(int capacity) : base(capacity) { }

        /// <summary>Adds a range of objects represented by the <see cref="ICollection"/>
        /// implementation.</summary>
        /// <param name="c">The <see cref="ICollection"/>
        /// implementation to add to this list.</param>
        public void AddRange(ICollection c)
        {
            // If the collection is null, throw an exception.
            if (c == null) throw new ArgumentNullException("c");

            // Pre-compute capacity.
            Capacity = Math.Max(c.Count + Count, Capacity);

            // Cycle through the items and add.
            foreach (T item in c)
            {
                // Add the item.
                Add(item);
            }
        }

        /// <summary>Compares the counts of two <see cref="IEnumerable{T}"/>
        /// implementations.</summary>
        /// <remarks>This uses a trick in LINQ, sniffing types for implementations
        /// of interfaces that might supply shortcuts when trying to make comparisons.
        /// In this case, that is the <see cref="ICollection{T}"/> and
        /// <see cref="ICollection"/> interfaces, either of which can provide a count
        /// which can be used in determining the equality of sequences (if they don't have
        /// the same count, then they can't be equal).</remarks>
        /// <param name="x">The <see cref="IEnumerable{T}"/> from the left hand side of the
        /// comparison to check the count of.</param>
        /// <param name="y">The <see cref="IEnumerable{T}"/> from the right hand side of the
        /// comparison to check the count of.</param>
        /// <returns>Null if the result is indeterminate.  This occurs when either <paramref name="x"/>
        /// or <paramref name="y"/> doesn't implement <see cref="ICollection"/> or <see cref="ICollection{T}"/>.
        /// Otherwise, it will get the count from each and return true if they are equal, false otherwise.</returns>
        private static bool? EnumerableCountsEqual(System.Collections.Generic.IEnumerable<T> x, System.Collections.Generic.IEnumerable<T> y)
        {
            // Get the ICollection<T> and ICollection interfaces.
            System.Collections.Generic.ICollection<T> xOfTCollection = x as System.Collections.Generic.ICollection<T>;
            System.Collections.Generic.ICollection<T> yOfTCollection = y as System.Collections.Generic.ICollection<T>;
            ICollection xCollection = x as ICollection;
            ICollection yCollection = y as ICollection;

            // The count in x and y.
            int? xCount = xOfTCollection != null ? xOfTCollection.Count : xCollection != null ? xCollection.Count : (int?)null;
            int? yCount = yOfTCollection != null ? yOfTCollection.Count : yCollection != null ? yCollection.Count : (int?)null;

            // If either are null, return null, the result is indeterminate.
            if (xCount == null || yCount == null)
            {
                // Return null, indeterminate.
                return null;
            }

            // Both counts are non-null, compare.
            return xCount == yCount;
        }

        /// <summary>Compares the contents of a <see cref="IEnumerable{T}"/>
        /// implementation to another one to determine equality.</summary>
        /// <remarks>Thinking of the <see cref="IEnumerable{T}"/> implementation as
        /// a string with any number of characters, the algorithm checks
        /// each item in each list.  If any item of the list is not equal (or
        /// one list contains all the elements of another list), then that list
        /// element is compared to the other list element to see which
        /// list is greater.</remarks>
        /// <param name="x">The <see cref="IEnumerable{T}"/> implementation
        /// that is considered the left hand side.</param>
        /// <param name="y">The <see cref="IEnumerable{T}"/> implementation
        /// that is considered the right hand side.</param>
        /// <returns>True if the items are equal, false otherwise.</returns>
        private static bool Equals(System.Collections.Generic.IEnumerable<T> x,
            System.Collections.Generic.IEnumerable<T> y)
        {
            // If x and y are null, then return true, they are the same.
            if (x == null && y == null)
            {
                // They are the same, return 0.
                return true;
            }

            // If one is null, then return a value based on whether or not
            // one is null or not.
            if (x == null || y == null)
            {
                // Return false, one is null, the other is not.
                return false;
            }

            // Check to see if the counts on the IEnumerable implementations are equal.
            // This is a shortcut, if they are not equal, then the lists are not equal.
            // If the result is indeterminate, then get out.
            bool? enumerableCountsEqual = EnumerableCountsEqual(x, y);

            // If the enumerable counts have been able to be calculated (indicated by
            // a non-null value) and it is false, then no need to iterate through the items.
            if (enumerableCountsEqual != null && !enumerableCountsEqual.Value)
            {
                // The sequences are not equal.
                return false;
            }

            // The counts of the items in the enumerations are equal, or indeterminate
            // so a full iteration needs to be made to compare each item.
            // Get the default comparer for T first.
            System.Collections.Generic.EqualityComparer<T> defaultComparer =
                System.Collections.Generic.EqualityComparer<T>.Default;

            // Get the enumerator for y.
            System.Collections.Generic.IEnumerator<T> otherEnumerator = y.GetEnumerator();

            // Call Dispose on IDisposable if there is an implementation on the
            // IEnumerator<T> returned by a call to y.GetEnumerator().
            using (otherEnumerator as IDisposable)
            {
                // Cycle through the items in this list.
                foreach (T item in x)
                {
                    // If there isn't an item to get, then this has more
                    // items than that, they are not equal.
                    if (!otherEnumerator.MoveNext())
                    {
                        // Return false.
                        return false;
                    }

                    // Perform a comparison.  Must check this on the left hand side
                    // and that on the right hand side.
                    bool comparison = defaultComparer.Equals(item, otherEnumerator.Current);

                    // If the value is false, return false.
                    if (!comparison)
                    {
                        // Return the value.
                        return comparison;
                    }
                }

                // If there are no more items, then return true, the sequences
                // are equal.
                if (!otherEnumerator.MoveNext())
                {
                    // The sequences are equal.
                    return true;
                }

                // The other sequence has more items than this one, return
                // false, these are not equal.
                return false;
            }
        }

#region IEquatable<IEnumerable<T>> Members
        /// <summary>Compares this sequence to another <see cref="IEnumerable{T}"/>
        /// implementation, returning true if they are equal, false otherwise.</summary>
        /// <param name="other">The other <see cref="IEnumerable{T}"/> implementation
        /// to compare against.</param>
        /// <returns>True if the sequence in <paramref name="other"/> 
        /// is the same as this one.</returns>
        public bool Equals(System.Collections.Generic.IEnumerable<T> other)
        {
            // Compare to the other sequence.  If 0, then equal.
            return Equals(this, other);
        }
#endregion

        /// <summary>Compares this object for equality against other.</summary>
        /// <param name="obj">The other object to compare this object against.</param>
        /// <returns>True if this object and <paramref name="obj"/> are equal, false
        /// otherwise.</returns>
        public override bool Equals(object obj)
        {
            // Call the strongly typed version.
            return Equals(obj as System.Collections.Generic.IEnumerable<T>);
        }

        /// <summary>Gets the hash code for the list.</summary>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode()
        {
            // Call the static method, passing this.
            return GetHashCode(this);
        }

        #if __MonoCS__
        public static int GetHashCode<T>(System.Collections.Generic.IEnumerable<T> source)
        #else
        /// <summary>Gets the hash code for the list.</summary>
        /// <param name="source">The <see cref="IEnumerable{T}"/>
        /// <param name="source">The <see cref="IEnumerable<T>"/>
        /// implementation which will have all the contents hashed.</param>
        /// <returns>The hash code value.</returns>
        public static int GetHashCode(System.Collections.Generic.IEnumerable<T> source)
        #endif
        {
            // If source is null, then return 0.
            if (source == null) return 0;

            // Seed the hash code with the hash code of the type.
            // This is done so that you don't have a lot of collisions of empty
            // ComparableList instances when placed in dictionaries
            // and things that rely on hashcodes.
            int hashCode = typeof(T).GetHashCode();

            // Iterate through the items in this implementation.
            foreach (T item in source)
            {
                // Adjust the hash code.
                hashCode = 31 * hashCode + (item == null ? 0 : item.GetHashCode());
            }

            // Return the hash code.
            return hashCode;
        }

        // TODO: When diverging from Java version of Lucene, can uncomment these to adhere to best practices when overriding the Equals method and implementing IEquatable<T>.
        ///// <summary>Overload of the == operator, it compares a
        ///// <see cref="ComparableList{T}"/> to an <see cref="IEnumerable{T}"/>
        ///// implementation.</summary>
        ///// <param name="x">The <see cref="ComparableList{T}"/> to compare
        ///// against <paramref name="y"/>.</param>
        ///// <param name="y">The <see cref="IEnumerable{T}"/> to compare
        ///// against <paramref name="x"/>.</param>
        ///// <returns>True if the instances are equal, false otherwise.</returns>
        //public static bool operator ==(EquatableList<T> x, System.Collections.Generic.IEnumerable<T> y)
        //{
        //    // Call Equals.
        //    return Equals(x, y);
        //}

        ///// <summary>Overload of the == operator, it compares a
        ///// <see cref="ComparableList{T}"/> to an <see cref="IEnumerable{T}"/>
        ///// implementation.</summary>
        ///// <param name="y">The <see cref="ComparableList{T}"/> to compare
        ///// against <paramref name="x"/>.</param>
        ///// <param name="x">The <see cref="IEnumerable{T}"/> to compare
        ///// against <paramref name="y"/>.</param>
        ///// <returns>True if the instances are equal, false otherwise.</returns>
        //public static bool operator ==(System.Collections.Generic.IEnumerable<T> x, EquatableList<T> y)
        //{
        //    // Call equals.
        //    return Equals(x, y);
        //}

        ///// <summary>Overload of the != operator, it compares a
        ///// <see cref="ComparableList{T}"/> to an <see cref="IEnumerable{T}"/>
        ///// implementation.</summary>
        ///// <param name="x">The <see cref="ComparableList{T}"/> to compare
        ///// against <paramref name="y"/>.</param>
        ///// <param name="y">The <see cref="IEnumerable{T}"/> to compare
        ///// against <paramref name="x"/>.</param>
        ///// <returns>True if the instances are not equal, false otherwise.</returns>
        //public static bool operator !=(EquatableList<T> x, System.Collections.Generic.IEnumerable<T> y)
        //{
        //    // Return the negative of the equals operation.
        //    return !(x == y);
        //}

        ///// <summary>Overload of the != operator, it compares a
        ///// <see cref="ComparableList{T}"/> to an <see cref="IEnumerable{T}"/>
        ///// implementation.</summary>
        ///// <param name="y">The <see cref="ComparableList{T}"/> to compare
        ///// against <paramref name="x"/>.</param>
        ///// <param name="x">The <see cref="IEnumerable{T}"/> to compare
        ///// against <paramref name="y"/>.</param>
        ///// <returns>True if the instances are not equal, false otherwise.</returns>
        //public static bool operator !=(System.Collections.Generic.IEnumerable<T> x, EquatableList<T> y)
        //{
        //    // Return the negative of the equals operation.
        //    return !(x == y);
        //}

        #region ICloneable Members

        /// <summary>Clones the <see cref="EquatableList{T}"/>.</summary>
        /// <remarks>This is a shallow clone.</remarks>
        /// <returns>A new shallow clone of this
        /// <see cref="EquatableList{T}"/>.</returns>
        public object Clone()
        {
            // Just create a new one, passing this to the constructor.
            return new EquatableList<T>(this);
        }

        #endregion
    }

    /// <summary>
    /// A simple wrapper to allow for the use of the GeneralKeyedCollection.  The
    /// wrapper is required as there can be several keys for an object depending
    /// on how many interfaces it implements.
    /// </summary>
    internal sealed class AttributeImplItem
    {
        internal AttributeImplItem(Type key, Mono.Lucene.Net.Util.AttributeImpl value)
        {
            this.Key = key;
            this.Value = value;
        }
        internal Type Key;
        internal Mono.Lucene.Net.Util.AttributeImpl Value;
    }

    /// <summary>
    /// Provides platform infos.
    /// </summary>
    public class OS
    {
        static bool isUnix;
        static bool isWindows;

        static OS()
        {
            PlatformID pid = Environment.OSVersion.Platform;
            isWindows = pid == PlatformID.Win32NT || pid == PlatformID.Win32Windows;

            // we use integers instead of enum tags because "MacOS"
            // requires 2.0 SP2, 3.0 SP2 or 3.5 SP1.
            // 128 is mono's old platform tag for Unix.
            int id = (int)pid;
            isUnix = id == 4 || id == 6 || id == 128;
        }

        /// <summary>
        /// Whether we run under a Unix platform.
        /// </summary>
        public static bool IsUnix
        {
            get { return isUnix; }
        }

        /// <summary>
        /// Whether we run under a supported Windows platform.
        /// </summary>
        public static bool IsWindows
        {
            get { return isWindows; }
        }
    }

    public class SharpZipLib
    {
        static System.Reflection.Assembly asm = null;

        static SharpZipLib()
        {
            try
            {
                asm = System.Reflection.Assembly.Load("ICSharpCode.SharpZipLib");
            }
            catch{}
        }

        public static Deflater CreateDeflater()
        {
            if (asm == null) throw new System.IO.FileNotFoundException("Can not load ICSharpCode.SharpZipLib.dll"); 
            return new Deflater(asm.CreateInstance("ICSharpCode.SharpZipLib.Zip.Compression.Deflater"));
        }

        public static Inflater CreateInflater()
        {
            if (asm == null) throw new System.IO.FileNotFoundException("Can not load ICSharpCode.SharpZipLib.dll");
            return new Inflater(asm.CreateInstance("ICSharpCode.SharpZipLib.Zip.Compression.Inflater"));
        }


        public class Inflater
        {
            delegate void SetInputDelegate(byte[] buffer);
            delegate bool GetIsFinishedDelegate();
            delegate int InflateDelegate(byte[] buffer);

            SetInputDelegate setInputMethod;
            GetIsFinishedDelegate getIsFinishedMethod;
            InflateDelegate inflateMethod;

            internal Inflater(object inflaterInstance)
            {
                Type type = inflaterInstance.GetType();

                setInputMethod = (SetInputDelegate)Delegate.CreateDelegate(
                    typeof(SetInputDelegate),
                    inflaterInstance,
                    type.GetMethod("SetInput", new Type[] { typeof(byte[]) }));

                getIsFinishedMethod = (GetIsFinishedDelegate)Delegate.CreateDelegate(
                    typeof(GetIsFinishedDelegate),
                    inflaterInstance,
                    type.GetMethod("get_IsFinished", Type.EmptyTypes));

                inflateMethod = (InflateDelegate)Delegate.CreateDelegate(
                    typeof(InflateDelegate),
                    inflaterInstance,
                    type.GetMethod("Inflate", new Type[] { typeof(byte[]) }));
            }

            public void SetInput(byte[] buffer)
            {
                setInputMethod(buffer);
            }

            public bool IsFinished
            {
                get { return getIsFinishedMethod(); }
            }

            public int Inflate(byte[] buffer)
            {
                return inflateMethod(buffer);
            }
        }


        public class Deflater 
        {
            delegate void SetLevelDelegate(int level);
            delegate void SetInputDelegate(byte[] input, int offset, int count);
            delegate void FinishDelegate();
            delegate bool GetIsFinishedDelegate();
            delegate int DeflateDelegate(byte[] output);

            SetLevelDelegate setLevelMethod;
            SetInputDelegate setInputMethod;
            FinishDelegate finishMethod;
            GetIsFinishedDelegate getIsFinishedMethod;
            DeflateDelegate deflateMethod;

            public const int BEST_COMPRESSION = 9;

            internal Deflater(object deflaterInstance)
            {
                Type type = deflaterInstance.GetType();

                setLevelMethod = (SetLevelDelegate)Delegate.CreateDelegate(
                    typeof(SetLevelDelegate),
                    deflaterInstance,
                    type.GetMethod("SetLevel", new Type[] { typeof(int) }));

                setInputMethod = (SetInputDelegate)Delegate.CreateDelegate(
                    typeof(SetInputDelegate),
                    deflaterInstance,
                    type.GetMethod("SetInput", new Type[] { typeof(byte[]), typeof(int), typeof(int) }));

                finishMethod = (FinishDelegate)Delegate.CreateDelegate(
                    typeof(FinishDelegate),
                    deflaterInstance,
                    type.GetMethod("Finish", Type.EmptyTypes));

                getIsFinishedMethod = (GetIsFinishedDelegate)Delegate.CreateDelegate(
                    typeof(GetIsFinishedDelegate),
                    deflaterInstance,
                    type.GetMethod("get_IsFinished", Type.EmptyTypes));

                deflateMethod = (DeflateDelegate)Delegate.CreateDelegate(
                    typeof(DeflateDelegate),
                    deflaterInstance,
                    type.GetMethod("Deflate", new Type[] { typeof(byte[]) }));
            }
            
            public void SetLevel(int level)
            {
                setLevelMethod(level);
            }

            public void SetInput(byte[] input, int offset, int count)
            {
                setInputMethod(input, offset, count);
            }

            public void Finish()
            {
                finishMethod();
            }

            public bool IsFinished
            {
                get { return getIsFinishedMethod(); }
            }

            public int Deflate(byte[] output)
            {
                return deflateMethod(output);
            }
        }
    }

    /// <summary>
    /// For Debuging purposes.
    /// </summary>
    public class CloseableThreadLocalProfiler
    {
        public static bool _EnableCloseableThreadLocalProfiler = false;
        public static System.Collections.Generic.List<WeakReference> Instances = new System.Collections.Generic.List<WeakReference>();

        public static bool EnableCloseableThreadLocalProfiler
        {
            get { return _EnableCloseableThreadLocalProfiler; }
            set
            {
                _EnableCloseableThreadLocalProfiler = value;
                lock (Instances)
                    Instances.Clear();
            }
        }
    }

    public class BuildType
    {
#if DEBUG
       public static bool Debug = true;
#else
        public static bool Debug = false;
#endif
    }
}
