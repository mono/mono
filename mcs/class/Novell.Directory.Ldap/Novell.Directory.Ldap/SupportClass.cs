/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.SupportClass.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

// Support classes replicate the functionality of the original code, but in some cases they are 
// substantially different architecturally. Although every effort is made to preserve the 
// original architecture of the application in the converted project, the user should be aware that 
// the primary goal of these support classes is to replicate functionality, and that at times 
// the architecture of the resulting solution may differ somewhat.
//

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


	public class Integer32 : System.Object
	{
		private System.Int32 _wintv;

		public Integer32(System.Int32 ival)    
		{
			_wintv=ival;
		}
	
		public System.Int32 intValue
		{	
			get
			{
				return _wintv;
			}
			set
			{
				_wintv=value;
			}
		}
	}
		
	/// <summary>
	/// Contains conversion support elements such as classes, interfaces and static methods.
	/// </summary>
	public class SupportClass
	{
		/// <summary>
		/// Receives a byte array and returns it transformed in an sbyte array
		/// </summary>
		/// <param name="byteArray">Byte array to process</param>
		/// <returns>The transformed array</returns>
		[CLSCompliantAttribute(false)]
		public static sbyte[] ToSByteArray(byte[] byteArray)
		{
			sbyte[] sbyteArray = new sbyte[byteArray.Length];
			for(int index=0; index < byteArray.Length; index++)
				sbyteArray[index] = (sbyte) byteArray[index];
			return sbyteArray;
		}
		/*******************************/
		/// <summary>
		/// Converts an array of sbytes to an array of bytes
		/// </summary>
		/// <param name="sbyteArray">The array of sbytes to be converted</param>
		/// <returns>The new array of bytes</returns>
		[CLSCompliantAttribute(false)]
		public static byte[] ToByteArray(sbyte[] sbyteArray)
		{
			byte[] byteArray = new byte[sbyteArray.Length];
			for(int index=0; index < sbyteArray.Length; index++)
				byteArray[index] = (byte) sbyteArray[index];
			return byteArray;
		}

		/// <summary>
		/// Converts a string to an array of bytes
		/// </summary>
		/// <param name="sourceString">The string to be converted</param>
		/// <returns>The new array of bytes</returns>
		public static byte[] ToByteArray(string sourceString)
		{
			byte[] byteArray = new byte[sourceString.Length];
			for (int index=0; index < sourceString.Length; index++)
				byteArray[index] = (byte) sourceString[index];
			return byteArray;
		}

		/// <summary>
		/// Converts a array of object-type instances to a byte-type array.
		/// </summary>
		/// <param name="tempObjectArray">Array to convert.</param>
		/// <returns>An array of byte type elements.</returns>
		public static byte[] ToByteArray(object[] tempObjectArray)
		{
			byte[] byteArray = new byte[tempObjectArray.Length];
			for (int index = 0; index < tempObjectArray.Length; index++)
				byteArray[index] = (byte)tempObjectArray[index];
			return byteArray;
		}


		/*******************************/
		/// <summary>Reads a number of characters from the current source Stream and writes the data to the target array at the specified index.</summary>
		/// <param name="sourceStream">The source Stream to read from.</param>
		/// <param name="target">Contains the array of characteres read from the source Stream.</param>
		/// <param name="start">The starting index of the target array.</param>
		/// <param name="count">The maximum number of characters to read from the source Stream.</param>
		/// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source Stream. Returns -1 if the end of the stream is reached.</returns>
		[CLSCompliantAttribute(false)]
		public static System.Int32 ReadInput(System.IO.Stream sourceStream, ref sbyte[] target, int start, int count)
		{
			// Returns 0 bytes if not enough space in target
			if (target.Length == 0)
				return 0;

			byte[] receiver = new byte[target.Length];
			int bytesRead=0;
			int startIndex=start;
			int bytesToRead=count;
			while( bytesToRead > 0 )	{
				int n= sourceStream.Read(receiver, startIndex, bytesToRead);
				if (n==0)
					break;
				bytesRead+=n;
				startIndex+=n;
				bytesToRead-=n;
			}
			// Returns -1 if EOF
			if (bytesRead == 0)	
				return -1;
                
			for(int i = start; i < start + bytesRead; i++)
				target[i] = (sbyte)receiver[i];
                
			return bytesRead;
		}

		/// <summary>Reads a number of characters from the current source TextReader and writes the data to the target array at the specified index.</summary>
		/// <param name="sourceTextReader">The source TextReader to read from</param>
		/// <param name="target">Contains the array of characteres read from the source TextReader.</param>
		/// <param name="start">The starting index of the target array.</param>
		/// <param name="count">The maximum number of characters to read from the source TextReader.</param>
		/// <returns>The number of characters read. The number will be less than or equal to count depending on the data available in the source TextReader. Returns -1 if the end of the stream is reached.</returns>
		[CLSCompliantAttribute(false)]
		public static System.Int32 ReadInput(System.IO.TextReader sourceTextReader, ref sbyte[] target, int start, int count)
		{
			// Returns 0 bytes if not enough space in target
			if (target.Length == 0) return 0;

			char[] charArray = new char[target.Length];
			int bytesRead = sourceTextReader.Read(charArray, start, count);

			// Returns -1 if EOF
			if (bytesRead == 0) return -1;

			for(int index=start; index<start+bytesRead; index++)
				target[index] = (sbyte)charArray[index];

			return bytesRead;
		}

		/*******************************/
		/// <summary>
		/// This method returns the literal value received
		/// </summary>
		/// <param name="literal">The literal to return</param>
		/// <returns>The received value</returns>
		public static long Identity(long literal)
		{
			return literal;
		}

		/// <summary>
		/// This method returns the literal value received
		/// </summary>
		/// <param name="literal">The literal to return</param>
		/// <returns>The received value</returns>
		[CLSCompliantAttribute(false)]
		public static ulong Identity(ulong literal)
		{
			return literal;
		}

		/// <summary>
		/// This method returns the literal value received
		/// </summary>
		/// <param name="literal">The literal to return</param>
		/// <returns>The received value</returns>
		public static float Identity(float literal)
		{
			return literal;
		}

		/// <summary>
		/// This method returns the literal value received
		/// </summary>
		/// <param name="literal">The literal to return</param>
		/// <returns>The received value</returns>
		public static double Identity(double literal)
		{
			return literal;
		}

		/*******************************/
		/// <summary>
		/// The class performs token processing from strings
		/// </summary>
		public class Tokenizer
		{
			//Element list identified
			private System.Collections.ArrayList elements;
			//Source string to use
			private string source;
			//The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
			private string delimiters = " \t\n\r";		

			private bool returnDelims=false;
			/// <summary>
			/// Initializes a new class instance with a specified string to process
			/// </summary>
			/// <param name="source">String to tokenize</param>
			public Tokenizer(string source)
			{			
				this.elements = new System.Collections.ArrayList();
				this.elements.AddRange(source.Split(this.delimiters.ToCharArray()));
				this.RemoveEmptyStrings();
				this.source = source;
			}

			/// <summary>
			/// Initializes a new class instance with a specified string to process
			/// and the specified token delimiters to use
			/// </summary>
			/// <param name="source">String to tokenize</param>
			/// <param name="delimiters">String containing the delimiters</param>
			public Tokenizer(string source, string delimiters)
			{
				this.elements = new System.Collections.ArrayList();
				this.delimiters = delimiters;
				this.elements.AddRange(source.Split(this.delimiters.ToCharArray()));
				this.RemoveEmptyStrings();
				this.source = source;
			}

			public Tokenizer(string source, string delimiters,bool retDel)
			{
				this.elements = new System.Collections.ArrayList();
				this.delimiters = delimiters;
				this.source = source;
				this.returnDelims = retDel;
				if( returnDelims)
					Tokenize();
				else
					this.elements.AddRange(source.Split(this.delimiters.ToCharArray()));
				this.RemoveEmptyStrings();
			}
		
			private void Tokenize()
			{
				string tempstr = this.source;
				string toks = "";
				if (tempstr.IndexOfAny(this.delimiters.ToCharArray()) < 0 && tempstr.Length > 0)
				{
					this.elements.Add(tempstr);
				}
				else if (tempstr.IndexOfAny(this.delimiters.ToCharArray()) < 0 && tempstr.Length <= 0)
				{
					return;
				}
				while (tempstr.IndexOfAny(this.delimiters.ToCharArray()) >= 0)
				{
					if(tempstr.IndexOfAny(this.delimiters.ToCharArray()) == 0)
					{
						if (tempstr.Length > 1 )
						{
							this.elements.Add(tempstr.Substring(0,1));
							tempstr=tempstr.Substring(1);
						}
						else
							tempstr = "";
					}
					else
					{
						toks = tempstr.Substring(0,tempstr.IndexOfAny(this.delimiters.ToCharArray()));
						this.elements.Add(toks);
						this.elements.Add(tempstr.Substring(toks.Length,1));
						if ( tempstr.Length > (toks.Length + 1))
						{
							tempstr = tempstr.Substring(toks.Length + 1);
						}
						else
                                                        tempstr = "";
					}
				}
				if (tempstr.Length > 0)
				{
					this.elements.Add(tempstr);
				}
			}
						
			/// <summary>
			/// Current token count for the source string
			/// </summary>
			public int Count
			{
				get
				{
					return (this.elements.Count);
				}
			}

			/// <summary>
			/// Determines if there are more tokens to return from the source string
			/// </summary>
			/// <returns>True or false, depending if there are more tokens</returns>
			public bool HasMoreTokens()
			{
				return (this.elements.Count > 0);			
			}

			/// <summary>
			/// Returns the next token from the token list
			/// </summary>
			/// <returns>The string value of the token</returns>
			public string NextToken()
			{			
				string result;
				if (source == "") throw new System.Exception();
				else
				{
					if(returnDelims){
//						Tokenize();
						RemoveEmptyStrings();		
						result = (string) this.elements[0];
						this.elements.RemoveAt(0);
						return result;
					}
					else
					{
						this.elements = new System.Collections.ArrayList();
						this.elements.AddRange(this.source.Split(delimiters.ToCharArray()));
						RemoveEmptyStrings();		
						result = (string) this.elements[0];
						this.elements.RemoveAt(0);				
						this.source = this.source.Remove(this.source.IndexOf(result),result.Length);
						this.source = this.source.TrimStart(this.delimiters.ToCharArray());
						return result;
					}
				}			
			}

			/// <summary>
			/// Returns the next token from the source string, using the provided
			/// token delimiters
			/// </summary>
			/// <param name="delimiters">String containing the delimiters to use</param>
			/// <returns>The string value of the token</returns>
			public string NextToken(string delimiters)
			{
				this.delimiters = delimiters;
				return NextToken();
			}

			/// <summary>
			/// Removes all empty strings from the token list
			/// </summary>
			private void RemoveEmptyStrings()
			{
				for (int index=0; index < this.elements.Count; index++)
					if ((string)this.elements[index]== "")
					{
						this.elements.RemoveAt(index);
						index--;
					}
			}
		}

		/*******************************/
		/// <summary>
		/// Provides support for DateFormat
		/// </summary>
		public class DateTimeFormatManager
		{
			static public DateTimeFormatHashTable manager = new DateTimeFormatHashTable();

			/// <summary>
			/// Hashtable class to provide functionality for dateformat properties
			/// </summary>
			public class DateTimeFormatHashTable :System.Collections.Hashtable 
			{
				/// <summary>
				/// Sets the format for datetime.
				/// </summary>
				/// <param name="format">DateTimeFormat instance to set the pattern</param>
				/// <param name="newPattern">A string with the pattern format</param>
				public void SetDateFormatPattern(System.Globalization.DateTimeFormatInfo format, System.String newPattern)
				{
					if (this[format] != null)
						((DateTimeFormatProperties) this[format]).DateFormatPattern = newPattern;
					else
					{
						DateTimeFormatProperties tempProps = new DateTimeFormatProperties();
						tempProps.DateFormatPattern  = newPattern;
						Add(format, tempProps);
					}
				}

				/// <summary>
				/// Gets the current format pattern of the DateTimeFormat instance
				/// </summary>
				/// <param name="format">The DateTimeFormat instance which the value will be obtained</param>
				/// <returns>The string representing the current datetimeformat pattern</returns>
				public string GetDateFormatPattern(System.Globalization.DateTimeFormatInfo format)
				{
					if (this[format] == null)
						return "d-MMM-yy";
					else
						return ((DateTimeFormatProperties) this[format]).DateFormatPattern;
				}
		
				/// <summary>
				/// Sets the datetimeformat pattern to the giving format
				/// </summary>
				/// <param name="format">The datetimeformat instance to set</param>
				/// <param name="newPattern">The new datetimeformat pattern</param>
				public void SetTimeFormatPattern(System.Globalization.DateTimeFormatInfo format, System.String newPattern)
				{
					if (this[format] != null)
						((DateTimeFormatProperties) this[format]).TimeFormatPattern = newPattern;
					else
					{
						DateTimeFormatProperties tempProps = new DateTimeFormatProperties();
						tempProps.TimeFormatPattern  = newPattern;
						Add(format, tempProps);
					}
				}

				/// <summary>
				/// Gets the current format pattern of the DateTimeFormat instance
				/// </summary>
				/// <param name="format">The DateTimeFormat instance which the value will be obtained</param>
				/// <returns>The string representing the current datetimeformat pattern</returns>
				public string GetTimeFormatPattern(System.Globalization.DateTimeFormatInfo format)
				{
					if (this[format] == null)
						return "h:mm:ss tt";
					else
						return ((DateTimeFormatProperties) this[format]).TimeFormatPattern;
				}

				/// <summary>
				/// Internal class to provides the DateFormat and TimeFormat pattern properties on .NET
				/// </summary>
				class DateTimeFormatProperties
				{
					public string DateFormatPattern = "d-MMM-yy";
					public string TimeFormatPattern = "h:mm:ss tt";
				}
			}	
		}
		/*******************************/
		/// <summary>
		/// Gets the DateTimeFormat instance and date instance to obtain the date with the format passed
		/// </summary>
		/// <param name="format">The DateTimeFormat to obtain the time and date pattern</param>
		/// <param name="date">The date instance used to get the date</param>
		/// <returns>A string representing the date with the time and date patterns</returns>
		public static string FormatDateTime(System.Globalization.DateTimeFormatInfo format, System.DateTime date)
		{
			string timePattern = DateTimeFormatManager.manager.GetTimeFormatPattern(format);
			string datePattern = DateTimeFormatManager.manager.GetDateFormatPattern(format);
			return date.ToString(datePattern + " " + timePattern, format);            
		}

		/*******************************/
		/// <summary>
		/// Adds a new key-and-value pair into the hash table
		/// </summary>
		/// <param name="collection">The collection to work with</param>
		/// <param name="key">Key used to obtain the value</param>
		/// <param name="newValue">Value asociated with the key</param>
		/// <returns>The old element associated with the key</returns>
		public static System.Object PutElement(System.Collections.IDictionary collection, System.Object key, System.Object newValue)
		{
			System.Object element = collection[key];
			collection[key] = newValue;
			return element;
		}

		/*******************************/
		/// <summary>
		/// This class contains static methods to manage arrays.
		/// </summary>
		public class ArrayListSupport
		{
			/// <summary>
			/// Obtains an array containing all the elements of the collection. 
			/// </summary>
			/// <param name="collection">The collection from wich to obtain the elements.</param>
			/// <param name="objects">The array containing all the elements of the collection.</param>
			/// <returns>The array containing all the elements of the collection.</returns>
			public static System.Object[] ToArray(System.Collections.ArrayList collection, System.Object[] objects)
			{	
				int index = 0;
				System.Collections.IEnumerator tempEnumerator = collection.GetEnumerator();
				while (tempEnumerator.MoveNext())
					objects[index++] = tempEnumerator.Current;
				return objects;
			}
		}


		/*******************************/
		/// <summary>
		/// Removes the first occurrence of an specific object from an ArrayList instance.
		/// </summary>
		/// <param name="arrayList">The ArrayList instance</param>
		/// <param name="element">The element to remove</param>
		/// <returns>True if item is found in the ArrayList; otherwise, false</returns>  
		public static System.Boolean VectorRemoveElement(System.Collections.ArrayList arrayList, System.Object element)
		{
			System.Boolean containsItem = arrayList.Contains(element);
			arrayList.Remove(element);
			return containsItem;
		}

		/*******************************/
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
			public ThreadClass(string Name)
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
			public ThreadClass(System.Threading.ThreadStart Start, string Name)
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


		/*******************************/
		/// <summary>
		/// This class contains different methods to manage Collections.
		/// </summary>
		public class CollectionSupport : System.Collections.CollectionBase
		{
			/// <summary>
			/// Creates an instance of the Collection by using an inherited constructor.
			/// </summary>
			public CollectionSupport() : base()
			{			
			}

			/// <summary>
			/// Adds an specified element to the collection.
			/// </summary>
			/// <param name="element">The element to be added.</param>
			/// <returns>Returns true if the element was successfuly added. Otherwise returns false.</returns>
			public virtual bool Add(System.Object element)
			{
				return (this.List.Add(element) != -1);
			}	

			/// <summary>
			/// Adds all the elements contained in the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(System.Collections.ICollection collection)
			{
				bool result = false;
				if (collection!=null)
				{
					System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(collection).GetEnumerator();
					while (tempEnumerator.MoveNext())
					{
						if (tempEnumerator.Current != null)
							result = this.Add(tempEnumerator.Current);
					}
				}
				return result;
			}


			/// <summary>
			/// Adds all the elements contained in the specified support class collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(CollectionSupport collection)
			{
				return this.AddAll((System.Collections.ICollection)collection);
			}

			/// <summary>
			/// Verifies if the specified element is contained into the collection. 
			/// </summary>
			/// <param name="element"> The element that will be verified.</param>
			/// <returns>Returns true if the element is contained in the collection. Otherwise returns false.</returns>
			public virtual bool Contains(System.Object element)
			{
				return this.List.Contains(element);
			}

			/// <summary>
			/// Verifies if all the elements of the specified collection are contained into the current collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be verified.</param>
			/// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
			public virtual bool ContainsAll(System.Collections.ICollection collection)
			{
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(collection).GetEnumerator();
				while (tempEnumerator.MoveNext())
					if (!(result = this.Contains(tempEnumerator.Current)))
						break;
				return result;
			}

			/// <summary>
			/// Verifies if all the elements of the specified collection are contained into the current collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be verified.</param>
			/// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
			public virtual bool ContainsAll(CollectionSupport collection)
			{
				return this.ContainsAll((System.Collections.ICollection) collection);
			}

			/// <summary>
			/// Verifies if the collection is empty.
			/// </summary>
			/// <returns>Returns true if the collection is empty. Otherwise returns false.</returns>
			public virtual bool IsEmpty()
			{
				return (this.Count == 0);
			}

			/// <summary>
			/// Removes an specified element from the collection.
			/// </summary>
			/// <param name="element">The element to be removed.</param>
			/// <returns>Returns true if the element was successfuly removed. Otherwise returns false.</returns>
			public virtual bool Remove(System.Object element)
			{
				bool result = false;
				if (this.Contains(element))
				{
					this.List.Remove(element);
					result = true;
				}
				return result;
			}

			/// <summary>
			/// Removes all the elements contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be removed.</param>
			/// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
			public virtual bool RemoveAll(System.Collections.ICollection collection)
			{ 
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(collection).GetEnumerator();
				while (tempEnumerator.MoveNext())
				{
					if (this.Contains(tempEnumerator.Current))
						result = this.Remove(tempEnumerator.Current);
				}
				return result;
			}

			/// <summary>
			/// Removes all the elements contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be removed.</param>
			/// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
			public virtual bool RemoveAll(CollectionSupport collection)
			{ 
				return this.RemoveAll((System.Collections.ICollection) collection);
			}

			/// <summary>
			/// Removes all the elements that aren't contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to verify the elements that will be retained.</param>
			/// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
			public virtual bool RetainAll(System.Collections.ICollection collection)
			{
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				CollectionSupport tempCollection = new CollectionSupport();
				tempCollection.AddAll(collection);
				while (tempEnumerator.MoveNext())
					if (!tempCollection.Contains(tempEnumerator.Current))
					{
						result = this.Remove(tempEnumerator.Current);
					
						if (result == true)
						{
							tempEnumerator = this.GetEnumerator();
						}
					}
				return result;
			}

			/// <summary>
			/// Removes all the elements that aren't contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to verify the elements that will be retained.</param>
			/// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
			public virtual bool RetainAll(CollectionSupport collection)
			{
				return this.RetainAll((System.Collections.ICollection) collection);
			}

			/// <summary>
			/// Obtains an array containing all the elements of the collection.
			/// </summary>
			/// <returns>The array containing all the elements of the collection</returns>
			public virtual System.Object[] ToArray()
			{	
				int index = 0;
				System.Object[] objects = new System.Object[this.Count];
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				while (tempEnumerator.MoveNext())
					objects[index++] = tempEnumerator.Current;
				return objects;
			}

			/// <summary>
			/// Obtains an array containing all the elements of the collection.
			/// </summary>
			/// <param name="objects">The array into which the elements of the collection will be stored.</param>
			/// <returns>The array containing all the elements of the collection.</returns>
			public virtual System.Object[] ToArray(System.Object[] objects)
			{	
				int index = 0;
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				while (tempEnumerator.MoveNext())
					objects[index++] = tempEnumerator.Current;
				return objects;
			}

			/// <summary>
			/// Creates a CollectionSupport object with the contents specified in array.
			/// </summary>
			/// <param name="array">The array containing the elements used to populate the new CollectionSupport object.</param>
			/// <returns>A CollectionSupport object populated with the contents of array.</returns>
			public static CollectionSupport ToCollectionSupport(System.Object[] array)
			{
				CollectionSupport tempCollectionSupport = new CollectionSupport();             
				tempCollectionSupport.AddAll(array);
				return tempCollectionSupport;
			}
		}

		/*******************************/
		/// <summary>
		/// This class contains different methods to manage list collections.
		/// </summary>
		public class ListCollectionSupport : System.Collections.ArrayList
		{
			/// <summary>
			/// Creates a new instance of the class ListCollectionSupport.
			/// </summary>
			public ListCollectionSupport() : base()
			{
			}
 
			/// <summary>
			/// Creates a new instance of the class ListCollectionSupport.
			/// </summary>
			/// <param name="collection">The collection to insert into the new object.</param>
			public ListCollectionSupport(System.Collections.ICollection collection) : base(collection)
			{
			}

			/// <summary>
			/// Creates a new instance of the class ListCollectionSupport with the specified capacity.
			/// </summary>
			/// <param name="capacity">The capacity of the new array.</param>
			public ListCollectionSupport(int capacity) : base(capacity)
			{
			}

			/// <summary>
			/// Adds an object to the end of the List.
			/// </summary>          
			/// <param name="valueToInsert">The value to insert in the array list.</param>
			/// <returns>Returns true after adding the value.</returns>
			public virtual bool Add(System.Object valueToInsert)
			{
				base.Insert(this.Count, valueToInsert);
				return true;
			}

			/// <summary>
			/// Adds all the elements contained into the specified collection, starting at the specified position.
			/// </summary>
			/// <param name="index">Position at which to add the first element from the specified collection.</param>
			/// <param name="list">The list used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(int index, System.Collections.IList list)
			{
				bool result = false;
				if (list!=null)
				{
					System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(list).GetEnumerator();
					int tempIndex = index;
					while (tempEnumerator.MoveNext())
					{
						base.Insert(tempIndex++, tempEnumerator.Current);
						result = true;
					}
				}
				return result;
			}

			/// <summary>
			/// Adds all the elements contained in the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(System.Collections.IList collection)
			{
				return this.AddAll(this.Count,collection);
			}

			/// <summary>
			/// Adds all the elements contained in the specified support class collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(CollectionSupport collection)
			{
				return this.AddAll(this.Count,collection);
			}

			/// <summary>
			/// Adds all the elements contained into the specified support class collection, starting at the specified position.
			/// </summary>
			/// <param name="index">Position at which to add the first element from the specified collection.</param>
			/// <param name="list">The list used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(int index, CollectionSupport collection)
			{
				return this.AddAll(index,(System.Collections.IList)collection);
			}
		
			/// <summary>
			/// Creates a copy of the ListCollectionSupport.
			/// </summary>
			/// <returns> A copy of the ListCollectionSupport.</returns>
			public virtual System.Object ListCollectionClone()
			{
				return MemberwiseClone();
			}


			/// <summary>
			/// Returns an iterator of the collection.
			/// </summary>
			/// <returns>An IEnumerator.</returns>
			public virtual System.Collections.IEnumerator ListIterator()
			{
				return base.GetEnumerator();
			}

			/// <summary>
			/// Removes all the elements contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be removed.</param>
			/// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
			public virtual bool RemoveAll(System.Collections.ICollection collection)
			{ 
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(collection).GetEnumerator();
				while (tempEnumerator.MoveNext())
				{
					result = true;
					if (base.Contains(tempEnumerator.Current))
						base.Remove(tempEnumerator.Current);
				}
				return result;
			}
		
			/// <summary>
			/// Removes all the elements contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be removed.</param>
			/// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
			public virtual bool RemoveAll(CollectionSupport collection)
			{ 
				return this.RemoveAll((System.Collections.ICollection) collection);
			}		

			/// <summary>
			/// Removes the value in the specified index from the list.
			/// </summary>          
			/// <param name="index">The index of the value to remove.</param>
			/// <returns>Returns the value removed.</returns>
			public virtual System.Object RemoveElement(int index)
			{
				System.Object objectRemoved = this[index];
				this.RemoveAt(index);
				return objectRemoved;
			}

			/// <summary>
			/// Removes an specified element from the collection.
			/// </summary>
			/// <param name="element">The element to be removed.</param>
			/// <returns>Returns true if the element was successfuly removed. Otherwise returns false.</returns>
			public virtual bool RemoveElement(System.Object element)
			{

				bool result = false;
				if (this.Contains(element))
				{
					base.Remove(element);
					result = true;
				}
				return result;
			}

			/// <summary>
			/// Removes the first value from an array list.
			/// </summary>          
			/// <returns>Returns the value removed.</returns>
			public virtual System.Object RemoveFirst()
			{
				System.Object objectRemoved = this[0];
				this.RemoveAt(0);
				return objectRemoved;
			}

			/// <summary>
			/// Removes the last value from an array list.
			/// </summary>
			/// <returns>Returns the value removed.</returns>
			public virtual System.Object RemoveLast()
			{
				System.Object objectRemoved = this[this.Count-1];
				base.RemoveAt(this.Count-1);
				return objectRemoved;
			}

			/// <summary>
			/// Removes all the elements that aren't contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to verify the elements that will be retained.</param>
			/// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
			public virtual bool RetainAll(System.Collections.ICollection collection)
			{
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				ListCollectionSupport tempCollection = new ListCollectionSupport(collection);
				while (tempEnumerator.MoveNext())
					if (!tempCollection.Contains(tempEnumerator.Current))
					{
						result = this.RemoveElement(tempEnumerator.Current);
					
						if (result == true)
						{
							tempEnumerator = this.GetEnumerator();
						}
					}
				return result;
			}
		
			/// <summary>
			/// Removes all the elements that aren't contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to verify the elements that will be retained.</param>
			/// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
			public virtual bool RetainAll(CollectionSupport collection)
			{
				return this.RetainAll((System.Collections.ICollection) collection);
			}		

			/// <summary>
			/// Verifies if all the elements of the specified collection are contained into the current collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be verified.</param>
			/// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
			public virtual bool ContainsAll(System.Collections.ICollection collection)
			{
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(collection).GetEnumerator();
				while (tempEnumerator.MoveNext())
					if(!(result = this.Contains(tempEnumerator.Current)))
						break;
				return result;
			}
		
			/// <summary>
			/// Verifies if all the elements of the specified collection are contained into the current collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be verified.</param>
			/// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
			public virtual bool ContainsAll(CollectionSupport collection)
			{
				return this.ContainsAll((System.Collections.ICollection) collection);
			}		

			/// <summary>
			/// Returns a new list containing a portion of the current list between a specified range. 
			/// </summary>
			/// <param name="startIndex">The start index of the range.</param>
			/// <param name="endIndex">The end index of the range.</param>
			/// <returns>A ListCollectionSupport instance containing the specified elements.</returns>
			public virtual ListCollectionSupport SubList(int startIndex, int endIndex)
			{
				int index = 0;
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				ListCollectionSupport result = new ListCollectionSupport();
				for(index = startIndex; index < endIndex; index++)
					result.Add(this[index]);
				return (ListCollectionSupport)result;
			}

			/// <summary>
			/// Obtains an array containing all the elements of the collection.
			/// </summary>
			/// <param name="objects">The array into which the elements of the collection will be stored.</param>
			/// <returns>The array containing all the elements of the collection.</returns>
			public virtual System.Object[] ToArray(System.Object[] objects)
			{	
				if (objects.Length < this.Count)
					objects = new System.Object[this.Count];
				int index = 0;
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				while (tempEnumerator.MoveNext())
					objects[index++] = tempEnumerator.Current;
				return objects;
			}

			/// <summary>
			/// Returns an iterator of the collection starting at the specified position.
			/// </summary>
			/// <param name="index">The position to set the iterator.</param>
			/// <returns>An IEnumerator at the specified position.</returns>
			public virtual System.Collections.IEnumerator ListIterator(int index)
			{
				if ((index < 0) || (index > this.Count)) throw new System.IndexOutOfRangeException();			
				System.Collections.IEnumerator tempEnumerator= this.GetEnumerator();
				if (index > 0)
				{
					int i=0;
					while ((tempEnumerator.MoveNext()) && (i < index - 1))
						i++;
				}
				return tempEnumerator;			
			}
	
			/// <summary>
			/// Gets the last value from a list.
			/// </summary>
			/// <returns>Returns the last element of the list.</returns>
			public virtual System.Object GetLast()
			{
				if (this.Count == 0) throw new System.ArgumentOutOfRangeException();
				else
				{
					return this[this.Count - 1];
				}									 
			}
		
			/// <summary>
			/// Return whether this list is empty.
			/// </summary>
			/// <returns>True if the list is empty, false if it isn't.</returns>
			public virtual bool IsEmpty()
			{
				return (this.Count == 0);
			}
		
			/// <summary>
			/// Replaces the element at the specified position in this list with the specified element.
			/// </summary>
			/// <param name="index">Index of element to replace.</param>
			/// <param name="element">Element to be stored at the specified position.</param>
			/// <returns>The element previously at the specified position.</returns>
			public virtual System.Object Set(int index, System.Object element)
			{
				System.Object result = this[index];
				this[index] = element;
				return result;
			} 

			/// <summary>
			/// Returns the element at the specified position in the list.
			/// </summary>
			/// <param name="index">Index of element to return.</param>
			/// <param name="element">Element to be stored at the specified position.</param>
			/// <returns>The element at the specified position in the list.</returns>
			public virtual System.Object Get(int index)
			{
				return this[index];
			}
		}

		/*******************************/
		/// <summary>
		/// This class manages array operations.
		/// </summary>
		public class ArraysSupport
		{
			/// <summary>
			/// Compares the entire members of one array whith the other one.
			/// </summary>
			/// <param name="array1">The array to be compared.</param>
			/// <param name="array2">The array to be compared with.</param>
			/// <returns>True if both arrays are equals otherwise it returns false.</returns>
			/// <remarks>Two arrays are equal if they contains the same elements in the same order.</remarks>
			public static bool IsArrayEqual(System.Array array1, System.Array array2)
			{
				if (array1.Length != array2.Length)
					return false;
				for (int i = 0; i < array1.Length; i++)
					if (!(array1.GetValue(i).Equals(array2.GetValue(i))))
						return false;
				return true;
			}

			/// <summary>
			/// Fills the array with an specific value from an specific index to an specific index.
			/// </summary>
			/// <param name="array">The array to be filled.</param>
			/// <param name="fromindex">The first index to be filled.</param>
			/// <param name="toindex">The last index to be filled.</param>
			/// <param name="val">The value to fill the array with.</param>
			public static void FillArray(System.Array array, System.Int32 fromindex,System.Int32 toindex, System.Object val)
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
			public static void FillArray(System.Array array, System.Object val)
			{
				FillArray(array, 0, array.Length, val);
			}
		}


		/*******************************/
		/// <summary>
		/// This class manages a set of elements.
		/// </summary>
		public class SetSupport : System.Collections.ArrayList
		{
			/// <summary>
			/// Creates a new set.
			/// </summary>
			public SetSupport(): base()
			{           
			}

			/// <summary>
			/// Creates a new set initialized with System.Collections.ICollection object
			/// </summary>
			/// <param name="collection">System.Collections.ICollection object to initialize the set object</param>
			public SetSupport(System.Collections.ICollection collection): base(collection)
			{           
			}

			/// <summary>
			/// Creates a new set initialized with a specific capacity.
			/// </summary>
			/// <param name="capacity">value to set the capacity of the set object</param>
			public SetSupport(int capacity): base(capacity)
			{           
			}
	 
			/// <summary>
			/// Adds an element to the set.
			/// </summary>
			/// <param name="objectToAdd">The object to be added.</param>
			/// <returns>True if the object was added, false otherwise.</returns>
			public new virtual bool Add(object objectToAdd)
			{
				if (this.Contains(objectToAdd))
					return false;
				else
				{
					base.Add(objectToAdd);
					return true;
				}
			}
	 
			/// <summary>
			/// Adds all the elements contained in the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(System.Collections.ICollection collection)
			{
				bool result = false;
				if (collection!=null)
				{
					System.Collections.IEnumerator tempEnumerator = new System.Collections.ArrayList(collection).GetEnumerator();
					while (tempEnumerator.MoveNext())
					{
						if (tempEnumerator.Current != null)
							result = this.Add(tempEnumerator.Current);
					}
				}
				return result;
			}
		
			/// <summary>
			/// Adds all the elements contained in the specified support class collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be added.</param>
			/// <returns>Returns true if all the elements were successfuly added. Otherwise returns false.</returns>
			public virtual bool AddAll(CollectionSupport collection)
			{
				return this.AddAll((System.Collections.ICollection)collection);
			}
	 
			/// <summary>
			/// Verifies that all the elements of the specified collection are contained into the current collection. 
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be verified.</param>
			/// <returns>True if the collection contains all the given elements.</returns>
			public virtual bool ContainsAll(System.Collections.ICollection collection)
			{
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = collection.GetEnumerator();
				while (tempEnumerator.MoveNext())
					if (!(result = this.Contains(tempEnumerator.Current)))
						break;
				return result;
			}
		
			/// <summary>
			/// Verifies if all the elements of the specified collection are contained into the current collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be verified.</param>
			/// <returns>Returns true if all the elements are contained in the collection. Otherwise returns false.</returns>
			public virtual bool ContainsAll(CollectionSupport collection)
			{
				return this.ContainsAll((System.Collections.ICollection) collection);
			}		
	 
			/// <summary>
			/// Verifies if the collection is empty.
			/// </summary>
			/// <returns>True if the collection is empty, false otherwise.</returns>
			public virtual bool IsEmpty()
			{
				return (this.Count == 0);
			}
	 	 
			/// <summary>
			/// Removes an element from the set.
			/// </summary>
			/// <param name="elementToRemove">The element to be removed.</param>
			/// <returns>True if the element was removed.</returns>
			public new virtual bool Remove(object elementToRemove)
			{
				bool result = false;
				if (this.Contains(elementToRemove))
					result = true;
				base.Remove(elementToRemove);
				return result;
			}
		
			/// <summary>
			/// Removes all the elements contained in the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be removed.</param>
			/// <returns>True if all the elements were successfuly removed, false otherwise.</returns>
			public virtual bool RemoveAll(System.Collections.ICollection collection)
			{ 
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = collection.GetEnumerator();
				while (tempEnumerator.MoveNext())
				{
					if ((result == false) && (this.Contains(tempEnumerator.Current)))
						result = true;
					this.Remove(tempEnumerator.Current);
				}
				return result;
			}
		
			/// <summary>
			/// Removes all the elements contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to extract the elements that will be removed.</param>
			/// <returns>Returns true if all the elements were successfuly removed. Otherwise returns false.</returns>
			public virtual bool RemoveAll(CollectionSupport collection)
			{ 
				return this.RemoveAll((System.Collections.ICollection) collection);
			}		

			/// <summary>
			/// Removes all the elements that aren't contained in the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to verify the elements that will be retained.</param>
			/// <returns>True if all the elements were successfully removed, false otherwise.</returns>
			public virtual bool RetainAll(System.Collections.ICollection collection)
			{
				bool result = false;
				System.Collections.IEnumerator tempEnumerator = collection.GetEnumerator();
				SetSupport tempSet = (SetSupport)collection;
				while (tempEnumerator.MoveNext())
					if (!tempSet.Contains(tempEnumerator.Current))
					{
						result = this.Remove(tempEnumerator.Current);
						tempEnumerator = this.GetEnumerator();
					}
				return result;
			}
		
			/// <summary>
			/// Removes all the elements that aren't contained into the specified collection.
			/// </summary>
			/// <param name="collection">The collection used to verify the elements that will be retained.</param>
			/// <returns>Returns true if all the elements were successfully removed. Otherwise returns false.</returns>
			public virtual bool RetainAll(CollectionSupport collection)
			{
				return this.RetainAll((System.Collections.ICollection) collection);
			}		
	 
			/// <summary>
			/// Obtains an array containing all the elements of the collection.
			/// </summary>
			/// <returns>The array containing all the elements of the collection.</returns>
			public new virtual object[] ToArray()
			{
				int index = 0;
				object[] tempObject= new object[this.Count];
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				while (tempEnumerator.MoveNext())
					tempObject[index++] = tempEnumerator.Current;
				return tempObject;
			}

			/// <summary>
			/// Obtains an array containing all the elements in the collection.
			/// </summary>
			/// <param name="objects">The array into which the elements of the collection will be stored.</param>
			/// <returns>The array containing all the elements of the collection.</returns>
			public virtual object[] ToArray(object[] objects)
			{
				int index = 0;
				System.Collections.IEnumerator tempEnumerator = this.GetEnumerator();
				while (tempEnumerator.MoveNext())
					objects[index++] = tempEnumerator.Current;
				return objects;
			}
		}
		/*******************************/
		/// <summary>
		/// This class manages different operation with collections.
		/// </summary>


		/*******************************/
		/// <summary>
		/// Removes the element with the specified key from a Hashtable instance.
		/// </summary>
		/// <param name="hashtable">The Hashtable instance</param>
		/// <param name="key">The key of the element to remove</param>
		/// <returns>The element removed</returns>  
		public static System.Object HashtableRemove(System.Collections.Hashtable hashtable, System.Object key)
		{
			System.Object element = hashtable[key];
			hashtable.Remove(key);
			return element;
		}

		/*******************************/
		/// <summary>
		/// Sets the size of the ArrayList. If the new size is greater than the current capacity, then new null items are added to the end of the ArrayList. If the new size is lower than the current size, then all elements after the new size are discarded
		/// </summary>
		/// <param name="arrayList">The ArrayList to be changed</param>
		/// <param name="newSize">The new ArrayList size</param>
		public static void SetSize(System.Collections.ArrayList arrayList, int newSize)
		{
			if (newSize < 0) throw new System.ArgumentException();
			else
			{
				if (newSize < arrayList.Count)
					arrayList.RemoveRange(newSize,(arrayList.Count-newSize));
				else
					while(newSize > arrayList.Count)
						arrayList.Add(null);
			}
		}

		/*******************************/
		/// <summary>
		/// Adds an element to the top end of a Stack instance.
		/// </summary>
		/// <param name="stack">The Stack instance</param>
		/// <param name="element">The element to add</param>
		/// <returns>The element added</returns>  
		public static System.Object StackPush(System.Collections.Stack stack, System.Object element)
		{
			stack.Push(element);
			return element;
		}

		/*******************************/
		/// <summary>
		/// Copies an array of chars obtained from a String into a specified array of chars
		/// </summary>
		/// <param name="sourceString">The String to get the chars from</param>
		/// <param name="sourceStart">Position of the String to start getting the chars</param>
		/// <param name="sourceEnd">Position of the String to end getting the chars</param>
		/// <param name="destinationArray">Array to return the chars</param>
		/// <param name="destinationStart">Position of the destination array of chars to start storing the chars</param>
		/// <returns>An array of chars</returns>
		public static void GetCharsFromString(string sourceString, int sourceStart, int sourceEnd, ref char[] destinationArray, int destinationStart)
		{	
			int sourceCounter;
			int destinationCounter;
			sourceCounter = sourceStart;
			destinationCounter = destinationStart;
			while (sourceCounter < sourceEnd)
			{
				destinationArray[destinationCounter] = (char) sourceString[sourceCounter];
				sourceCounter++;
				destinationCounter++;
			}
		}

		/*******************************/
		/// <summary>
		/// Creates an output file stream to write to the file with the specified name.
		/// </summary>
		/// <param name="FileName">Name of the file to write.</param>
		/// <param name="Append">True in order to write to the end of the file, false otherwise.</param>
		/// <returns>New instance of FileStream with the proper file mode.</returns>
		public static System.IO.FileStream GetFileStream(string FileName, bool Append)
		{
			if (Append)
				return new System.IO.FileStream(FileName, System.IO.FileMode.Append);
			else
				return new System.IO.FileStream(FileName, System.IO.FileMode.Create);
		}


		/*******************************/
		/// <summary>
		/// Converts an array of sbytes to an array of chars
		/// </summary>
		/// <param name="sByteArray">The array of sbytes to convert</param>
		/// <returns>The new array of chars</returns>
		[CLSCompliantAttribute(false)]
		public static char[] ToCharArray(sbyte[] sByteArray) 
		{
			char[] charArray = new char[sByteArray.Length];	   
			sByteArray.CopyTo(charArray, 0);
			return charArray;
		}

		/// <summary>
		/// Converts an array of bytes to an array of chars
		/// </summary>
		/// <param name="byteArray">The array of bytes to convert</param>
		/// <returns>The new array of chars</returns>
		public static char[] ToCharArray(byte[] byteArray) 
		{
			char[] charArray = new char[byteArray.Length];	   
			byteArray.CopyTo(charArray, 0);
			return charArray;
		}

		/*******************************/
		/// <summary>
		/// Encapsulates the functionality of message digest algorithms such as SHA-1 or MD5.
		/// </summary>
		public class MessageDigestSupport
		{
			private System.Security.Cryptography.HashAlgorithm algorithm;
			private byte[] data;
			private int position;
			private string algorithmName;

			/// <summary>
			/// The HashAlgorithm instance that provide the cryptographic hash algorithm
			/// </summary>
			public System.Security.Cryptography.HashAlgorithm Algorithm
			{
				get
				{
					return this.algorithm;
				}
				set
				{
					this.algorithm  = value;
				}
			}

			/// <summary>
			/// The digest data
			/// </summary>
			public byte[] Data
			{
				get
				{
					return this.data;
				}
				set
				{
					this.data  = value;
				}
			}

			/// <summary>
			/// The name of the cryptographic hash algorithm used in the instance
			/// </summary>
			public string AlgorithmName
			{
				get
				{
					return this.algorithmName;
				}
			}

			/// <summary>
			/// Creates a message digest using the specified name to set Algorithm property.
			/// </summary>
			/// <param name="algorithm">The name of the algorithm to use</param>
			public MessageDigestSupport(System.String algorithm)
			{			
				if (algorithm.Equals("SHA-1"))
				{
					this.algorithmName = "SHA";
				}
				else 
				{
					this.algorithmName = algorithm;
				}
				this.Algorithm = (System.Security.Cryptography.HashAlgorithm) System.Security.Cryptography.CryptoConfig.CreateFromName(this.algorithmName);			
				this.position  = 0;
			}

			/// <summary>
			/// Computes the hash value for the internal data digest.
			/// </summary>
			/// <returns>The array of signed bytes with the resulting hash value</returns>
			[CLSCompliantAttribute(false)]
			public sbyte[] DigestData()
			{
				sbyte[] result = ToSByteArray(this.Algorithm.ComputeHash(this.data));
				this.Reset();
				return result;
			}

			/// <summary>
			/// Performs and update on the digest with the specified array and then completes the digest
			/// computation.
			/// </summary>
			/// <param name="newData">The array of bytes for final update to the digest</param>
			/// <returns>An array of signed bytes with the resulting hash value</returns>
			[CLSCompliantAttribute(false)]
			public sbyte[] DigestData(byte[] newData)
			{
				this.Update(newData);
				return this.DigestData();
			}

			/// <summary>
			/// Updates the digest data with the specified array of bytes by making an append
			/// operation in the internal array of data.
			/// </summary>
			/// <param name="newData">The array of bytes for the update operation</param>
			public void Update(byte[] newData)
			{
				if (position == 0)
				{
					this.Data = newData;
					this.position = this.Data.Length - 1;
				}
				else
				{
					byte[] oldData = this.Data;
					this.Data = new byte[newData.Length + position + 1];
					oldData.CopyTo(this.Data, 0);
					newData.CopyTo(this.Data, oldData.Length);
	            
					this.position = this.Data.Length - 1;
				}
			}
        
			/// <summary>
			/// Updates the digest data with the input byte by calling the method Update with an array.
			/// </summary>
			/// <param name="newData">The input byte for the update</param>
			public void Update(byte newData)
			{
				byte[] newDataArray = new byte[1];
				newDataArray[0] = newData;
				this.Update(newDataArray);
			}

			/// <summary>
			/// Updates the specified count of bytes with the input array of bytes starting at the
			/// input offset.
			/// </summary>
			/// <param name="newData">The array of bytes for the update operation</param>
			/// <param name="offset">The initial position to start from in the array of bytes</param>
			/// <param name="count">The number of bytes fot the update</param>
			public void Update(byte[] newData, int offset, int count)
			{
				byte[] newDataArray = new byte[count];
				System.Array.Copy(newData, offset, newDataArray, 0, count);
				this.Update(newDataArray);
			}
		
			/// <summary>
			/// Resets the digest data to the initial state.
			/// </summary>
			public void Reset()
			{
				this.data = null;
				this.position = 0;
			}

			/// <summary>
			/// Returns a string representation of the Message Digest
			/// </summary>
			/// <returns>A string representation of the object</returns>
			public override string ToString()
			{
				return this.Algorithm.ToString();
			}

			/// <summary>
			/// Generates a new instance of the MessageDigestSupport class using the specified algorithm
			/// </summary>
			/// <param name="algorithm">The name of the algorithm to use</param>
			/// <returns>A new instance of the MessageDigestSupport class</returns>
			public static MessageDigestSupport GetInstance(System.String algorithm)
			{
				return new MessageDigestSupport(algorithm);
			}
		
			/// <summary>
			/// Compares two arrays of signed bytes evaluating equivalence in digest data
			/// </summary>
			/// <param name="firstDigest">An array of signed bytes for comparison</param>
			/// <param name="secondDigest">An array of signed bytes for comparison</param>
			/// <returns>True if the input digest arrays are equal</returns>
			[CLSCompliantAttribute(false)]
			public static bool EquivalentDigest(System.SByte[] firstDigest, System.SByte[] secondDigest)
			{
				bool result = false;
				if (firstDigest.Length == secondDigest.Length)
				{
					int index = 0;
					result = true;
					while(result && index < firstDigest.Length)
					{
						result = firstDigest[index] == secondDigest[index];
						index++;
					}
				}
			
				return result;
			}
		}

		/*******************************/
		/// <summary>
		/// This class uses a cryptographic Random Number Generator to provide support for
		/// strong pseudo-random number generation.
		/// </summary>
		public class SecureRandomSupport
		{
			private System.Security.Cryptography.RNGCryptoServiceProvider generator;

			/// <summary>
			/// Initializes a new instance of the random number generator.
			/// </summary>
			public SecureRandomSupport()
			{
				this.generator = new System.Security.Cryptography.RNGCryptoServiceProvider();
			}

			/// <summary>
			/// Initializes a new instance of the random number generator with the given seed.
			/// </summary>
			/// <param name="seed">The initial seed for the generator</param>
			public SecureRandomSupport(byte[] seed)
			{
				this.generator = new System.Security.Cryptography.RNGCryptoServiceProvider(seed);
			}

			/// <summary>
			/// Returns an array of bytes with a sequence of cryptographically strong random values
			/// </summary>
			/// <param name="randomnumbersarray">The array of bytes to fill</param>
			[CLSCompliantAttribute(false)]
			public sbyte[] NextBytes(byte[] randomnumbersarray)
			{			
				this.generator.GetBytes(randomnumbersarray);
				return ToSByteArray(randomnumbersarray);
			}

			/// <summary>
			/// Returns the given number of seed bytes generated for the first running of a new instance 
			/// of the random number generator
			/// </summary>
			/// <param name="numberOfBytes">Number of seed bytes to generate</param>
			/// <returns>Seed bytes generated</returns>
			public static byte[] GetSeed(int numberOfBytes)
			{
				System.Security.Cryptography.RNGCryptoServiceProvider generatedSeed = new System.Security.Cryptography.RNGCryptoServiceProvider();
				byte[] seeds = new byte[numberOfBytes];
				generatedSeed.GetBytes(seeds);
				return seeds;
			}

			/// <summary>
			/// Creates a new instance of the random number generator with the seed provided by the user
			/// </summary>
			/// <param name="newSeed">Seed to create a new random number generator</param>
			public void SetSeed(byte[] newSeed)
			{
				this.generator = new System.Security.Cryptography.RNGCryptoServiceProvider(newSeed);
			}

			/// <summary>
			/// Creates a new instance of the random number generator with the seed provided by the user
			/// </summary>
			/// <param name="newSeed">Seed to create a new random number generator</param>
			public void SetSeed(long newSeed)
			{
				byte[] bytes = new byte[8];
				for (int index= 7; index > 0 ; index--)
				{
					bytes[index] = (byte)(newSeed - (long)((newSeed >> 8) << 8));
					newSeed  = (long)(newSeed >> 8);
				}			
				SetSeed(bytes);
			}
		}

		/*******************************/
		/// <summary>
		/// Interface used by classes which must be single threaded.
		/// </summary>
		public interface SingleThreadModel
		{
		}


		/*******************************/
		/// <summary>
		/// Creates an instance of a received Type.
		/// </summary>
		/// <param name="classType">The Type of the new class instance to return.</param>
		/// <returns>An Object containing the new instance.</returns>
		public static System.Object CreateNewInstance(System.Type classType)
		{
			System.Object instance = null;
			System.Type[] constructor = new System.Type[]{};
			System.Reflection.ConstructorInfo[] constructors = null;
       
			constructors = classType.GetConstructors();

			if (constructors.Length == 0)
				throw new System.UnauthorizedAccessException();
			else
			{
				for(int i = 0; i < constructors.Length; i++)
				{
					System.Reflection.ParameterInfo[] parameters = constructors[i].GetParameters();

					if (parameters.Length == 0)
					{
						instance = classType.GetConstructor(constructor).Invoke(new System.Object[]{});
						break;
					}
					else if (i == constructors.Length -1)     
						throw new System.MethodAccessException();
				}                       
			}
			return instance;
		}


		/*******************************/
		/// <summary>
		/// Writes the exception stack trace to the received stream
		/// </summary>
		/// <param name="throwable">Exception to obtain information from</param>
		/// <param name="stream">Output sream used to write to</param>
		public static void WriteStackTrace(System.Exception throwable, System.IO.TextWriter stream)
		{
			stream.Write(throwable.StackTrace);
			stream.Flush();
		}

		/*******************************/
		/// <summary>
		/// Determines whether two Collections instances are equals.
		/// </summary>
		/// <param name="source">The first Collections to compare. </param>
		/// <param name="target">The second Collections to compare. </param>
		/// <returns>Return true if the first collection is the same instance as the second collection, otherwise return false.</returns>
		public static bool EqualsSupport(System.Collections.ICollection source, System.Collections.ICollection target )
		{
			System.Collections.IEnumerator sourceEnumerator = ReverseStack(source);
			System.Collections.IEnumerator targetEnumerator = ReverseStack(target);
     
			if (source.Count != target.Count)
				return false;
			while(sourceEnumerator.MoveNext() && targetEnumerator.MoveNext())
				if (!sourceEnumerator.Current.Equals(targetEnumerator.Current))
					return false;
			return true;
		}
	
		/// <summary>
		/// Determines if a Collection is equal to the Object.
		/// </summary>
		/// <param name="source">The first Collections to compare.</param>
		/// <param name="target">The Object to compare.</param>
		/// <returns>Return true if the first collection contains the same values of the second Object, otherwise return false.</returns>
		public static bool EqualsSupport(System.Collections.ICollection source, System.Object target)
		{
			if((target.GetType())!= (typeof(System.Collections.ICollection)))
				return false;
			else
				return EqualsSupport(source,(System.Collections.ICollection)target);
		}

		/// <summary>
		/// Determines if a IDictionaryEnumerator is equal to the Object.
		/// </summary>
		/// <param name="source">The first IDictionaryEnumerator to compare.</param>
		/// <param name="target">The second Object to compare.</param>
		/// <returns>Return true if the first IDictionaryEnumerator contains the same values of the second Object, otherwise return false.</returns>
		public static bool EqualsSupport(System.Collections.IDictionaryEnumerator source, System.Object target)
		{
			if((target.GetType())!= (typeof(System.Collections.IDictionaryEnumerator)))
				return false;
			else
				return EqualsSupport(source,(System.Collections.IDictionaryEnumerator)target);
		}

		/// <summary>
		/// Determines whether two IDictionaryEnumerator instances are equals.
		/// </summary>
		/// <param name="source">The first IDictionaryEnumerator to compare.</param>
		/// <param name="target">The second IDictionaryEnumerator to compare.</param>
		/// <returns>Return true if the first IDictionaryEnumerator contains the same values as the second IDictionaryEnumerator, otherwise return false.</returns>
		public static bool EqualsSupport(System.Collections.IDictionaryEnumerator source, System.Collections.IDictionaryEnumerator target )
		{
			while(source.MoveNext() && target.MoveNext())
				if (source.Key.Equals(target.Key))
					if(source.Value.Equals(target.Value))
						return true;
			return false;
		}

		/// <summary>
		/// Reverses the Stack Collection received.
		/// </summary>
		/// <param name="collection">The collection to reverse.</param>
		/// <returns>The collection received in reverse order if it was a System.Collections.Stack type, otherwise it does 
		/// nothing to the collection.</returns>
		public static System.Collections.IEnumerator ReverseStack(System.Collections.ICollection collection)
		{
			if((collection.GetType()) == (typeof(System.Collections.Stack)))
			{
				System.Collections.ArrayList collectionStack = new System.Collections.ArrayList(collection);
				collectionStack.Reverse();
				return collectionStack.GetEnumerator();
			}
			else
				return collection.GetEnumerator();
		}

	}

	public class AbstractSetSupport : SupportClass.SetSupport
	{
		/// <summary>
		/// The constructor with no parameters to create an abstract set.
		/// </summary>
		public AbstractSetSupport()
		{
		}
	}
