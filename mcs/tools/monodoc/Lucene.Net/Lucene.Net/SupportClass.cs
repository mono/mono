/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
	      
        /// <summary>
        /// Gets or sets a value indicating the scheduling priority of a thread
        /// </summary>
        public System.Threading.ThreadPriority Priority
        {
            get
            {
                return threadField.Priority;
            }
            set
            {
                threadField.Priority = value;
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
            lock(this)
            {
                threadField.Join(new System.TimeSpan(MiliSeconds * 10000));
            }
        }
	      
        /// <summary>
        /// Blocks the calling thread until a thread terminates or the specified time elapses
        /// </summary>
        /// <param name="MiliSeconds">Time of wait in milliseconds</param>
        /// <param name="NanoSeconds">Time of wait in nanoseconds</param>
        public void Join(long MiliSeconds, int NanoSeconds)
        {
            lock(this)
            {
                threadField.Join(new System.TimeSpan(MiliSeconds * 10000 + NanoSeconds * 100));
            }
        }
	      
        /// <summary>
        /// Resumes a thread that has been suspended
        /// </summary>
        public void Resume()
        {
            threadField.Resume();
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
        public void Abort(System.Object stateInfo)
        {
            lock(this)
            {
                threadField.Abort(stateInfo);
            }
        }
	      
        /// <summary>
        /// Suspends the thread, if the thread is already suspended it has no effect
        /// </summary>
        public void Suspend()
        {
            threadField.Suspend();
        }
	      
        /// <summary>
        /// Obtain a String that represents the current Object
        /// </summary>
        /// <returns>A String that represents the current Object</returns>
        public override System.String ToString()
        {
            return "Thread[" + Name + "," + Priority.ToString() + "," + "" + "]";
        }
	     
        /// <summary>
        /// Gets the currently running thread
        /// </summary>
        /// <returns>The currently running thread</returns>
        public static ThreadClass Current()
        {
            ThreadClass CurrentThread = new ThreadClass();
            CurrentThread.Instance = System.Threading.Thread.CurrentThread;
            return CurrentThread;
        }
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
    public class Date
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        static public long GetTime(DateTime dateTime)
        {
            TimeSpan ts = dateTime.Subtract(new DateTime(1970, 1, 1));
            ts = ts.Subtract(TimeZone.CurrentTimeZone.GetUtcOffset(dateTime));
            return ts.Ticks / TimeSpan.TicksPerMillisecond;
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
                    return System.Single.Parse(s.Substring(0, s.Length - 1));
                else
                    return System.Single.Parse(s);
            }
            catch(System.FormatException fex)
            {
                throw fex;					
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static int Get(System.String key, int defValue)
        {
            System.String theValue = System.Configuration.ConfigurationSettings.AppSettings.Get(key);
            if (theValue == null)
            {
                return defValue;
            }
            return System.Convert.ToInt16(theValue.Trim());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static long Get(System.String key, long defValue)
        {
            System.String theValue = System.Configuration.ConfigurationSettings.AppSettings.Get(key);
            if (theValue == null)
            {
                return defValue;
            }
            return System.Convert.ToInt32(theValue.Trim());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defValue"></param>
        /// <returns></returns>
        public static System.String Get(System.String key, System.String defValue)
        {
            System.String theValue = System.Configuration.ConfigurationSettings.AppSettings.Get(key);
            if (theValue == null)
            {
                return defValue;
            }
            return theValue;
        }
    }
}