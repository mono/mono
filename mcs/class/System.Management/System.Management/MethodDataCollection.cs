//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
using System;
using System.Collections;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.Management
{
	public class MethodDataCollection : ICollection, IEnumerable
	{
		private ManagementObject parent;

		public int Count
		{
			get
			{
				int num = 0;
				IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
				IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded1 = null;
				int num1 = -2147217407;
				lock (typeof(MethodDataCollection.enumLock))
				{
					try
					{
						num1 = this.parent.wbemObject.BeginMethodEnumeration_(0);
						if (num1 >= 0)
						{
							string str = "";
							while (str != null && num1 >= 0 && num1 != 0x40005)
							{
								str = null;
								wbemClassObjectFreeThreaded = null;
								wbemClassObjectFreeThreaded1 = null;
								num1 = this.parent.wbemObject.NextMethod_(0, out str, out wbemClassObjectFreeThreaded, out wbemClassObjectFreeThreaded1);
								if (num1 < 0 || num1 == 0x40005)
								{
									continue;
								}
								num++;
							}
							this.parent.wbemObject.EndMethodEnumeration_();
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						ManagementException.ThrowWithExtendedInfo(cOMException);
					}
				}
				if (((long)num1 & (long)-4096) != (long)-2147217408)
				{
					if (((long)num1 & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num1);
					}
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
				}
				return num;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public virtual MethodData this[string methodName]
		{
			get
			{
				if (methodName != null)
				{
					return new MethodData(this.parent, methodName);
				}
				else
				{
					throw new ArgumentNullException("methodName");
				}
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		internal MethodDataCollection(ManagementObject parent)
		{
			this.parent = parent;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public virtual void Add(string methodName)
		{
			this.Add(methodName, null, null);
		}

		public virtual void Add(string methodName, ManagementBaseObject inParameters, ManagementBaseObject outParameters)
		{
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
			IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded1 = null;
			if (this.parent.GetType() != typeof(ManagementObject))
			{
				if (inParameters != null)
				{
					wbemClassObjectFreeThreaded = inParameters.wbemObject;
				}
				if (outParameters != null)
				{
					wbemClassObjectFreeThreaded1 = outParameters.wbemObject;
				}
				int num = -2147217407;
				try
				{
					num = this.parent.wbemObject.PutMethod_(methodName, 0, wbemClassObjectFreeThreaded, wbemClassObjectFreeThreaded1);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					ManagementException.ThrowWithExtendedInfo(cOMException);
				}
				if (((long)num & (long)-4096) != (long)-2147217408)
				{
					if (((long)num & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
					return;
				}
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		public void CopyTo(Array array, int index)
		{
			foreach (MethodData methodDatum in this)
			{
				int num = index;
				index = num + 1;
				array.SetValue(methodDatum, num);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void CopyTo(MethodData[] methodArray, int index)
		{
			this.CopyTo(methodArray, index);
		}

		public MethodDataCollection.MethodDataEnumerator GetEnumerator()
		{
			return new MethodDataCollection.MethodDataEnumerator(this.parent);
		}

		public virtual void Remove(string methodName)
		{
			if (this.parent.GetType() != typeof(ManagementObject))
			{
				int num = -2147217407;
				try
				{
					num = this.parent.wbemObject.DeleteMethod_(methodName);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					ManagementException.ThrowWithExtendedInfo(cOMException);
				}
				if (((long)num & (long)-4096) != (long)-2147217408)
				{
					if (((long)num & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
					return;
				}
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new MethodDataCollection.MethodDataEnumerator(this.parent);
		}

		private class enumLock
		{
			public enumLock()
			{
			}
		}

		public class MethodDataEnumerator : IEnumerator
		{
			private ManagementObject parent;

			private ArrayList methodNames;

			private IEnumerator en;

			public MethodData Current
			{
				get
				{
					return new MethodData(this.parent, (string)this.en.Current);
				}
			}

			object System.Collections.IEnumerator.Current
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return this.Current;
				}
			}

			internal MethodDataEnumerator(ManagementObject parent)
			{
				this.parent = parent;
				this.methodNames = new ArrayList();
				IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded = null;
				IWbemClassObjectFreeThreaded wbemClassObjectFreeThreaded1 = null;
				int num = -2147217407;
				lock (typeof(MethodDataCollection.enumLock))
				{
					try
					{
						num = parent.wbemObject.BeginMethodEnumeration_(0);
						if (num >= 0)
						{
							string str = "";
							while (str != null && num >= 0 && num != 0x40005)
							{
								str = null;
								num = parent.wbemObject.NextMethod_(0, out str, out wbemClassObjectFreeThreaded, out wbemClassObjectFreeThreaded1);
								if (num < 0 || num == 0x40005)
								{
									continue;
								}
								this.methodNames.Add(str);
							}
							parent.wbemObject.EndMethodEnumeration_();
						}
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						ManagementException.ThrowWithExtendedInfo(cOMException);
					}
					this.en = this.methodNames.GetEnumerator();
				}
				if (((long)num & (long)-4096) != (long)-2147217408)
				{
					if (((long)num & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
					return;
				}
			}

			public bool MoveNext()
			{
				return this.en.MoveNext();
			}

			public void Reset()
			{
				this.en.Reset();
			}
		}
	}
}