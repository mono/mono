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
	public class PropertyDataCollection : ICollection, IEnumerable
	{
		private ManagementBaseObject parent;

		private bool isSystem;

		public int Count
		{
			get
			{
				int num;
				string[] strArrays = null;
				object obj = null;
				if (!this.isSystem)
				{
					num = 64;
				}
				else
				{
					num = 48;
				}
				int names_ = this.parent.wbemObject.GetNames_(null, num, ref obj, out strArrays);
				if (names_ < 0)
				{
					if (((long)names_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(names_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)names_);
					}
				}
				return (int)strArrays.Length;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public virtual PropertyData this[string propertyName]
		{
			get
			{
				if (propertyName != null)
				{
					return new PropertyData(this.parent, propertyName);
				}
				else
				{
					throw new ArgumentNullException("propertyName");
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

		internal PropertyDataCollection(ManagementBaseObject parent, bool isSystem)
		{
			this.parent = parent;
			this.isSystem = isSystem;
		}

		public virtual void Add(string propertyName, object propertyValue)
		{
			if (propertyValue != null)
			{
				if (this.parent.GetType() != typeof(ManagementObject))
				{
					CimType cimType = CimType.None;
					bool flag = false;
					object wmiValue = PropertyData.MapValueToWmiValue(propertyValue, out flag, out cimType);
					int num = (int)cimType;
					if (flag)
					{
						num = num | 0x2000;
					}
					int num1 = this.parent.wbemObject.Put_(propertyName, 0, ref wmiValue, num);
					if (num1 < 0)
					{
						if (((long)num1 & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num1);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
							return;
						}
					}
					return;
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			else
			{
				throw new ArgumentNullException("propertyValue");
			}
		}

		public void Add(string propertyName, object propertyValue, CimType propertyType)
		{
			if (propertyName != null)
			{
				if (this.parent.GetType() != typeof(ManagementObject))
				{
					int num = (int)propertyType;
					bool flag = false;
					if (propertyValue != null && propertyValue.GetType().IsArray)
					{
						flag = true;
						num = num | 0x2000;
					}
					object wmiValue = PropertyData.MapValueToWmiValue(propertyValue, propertyType, flag);
					int num1 = this.parent.wbemObject.Put_(propertyName, 0, ref wmiValue, num);
					if (num1 < 0)
					{
						if (((long)num1 & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num1);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
							return;
						}
					}
					return;
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			else
			{
				throw new ArgumentNullException("propertyName");
			}
		}

		public void Add(string propertyName, CimType propertyType, bool isArray)
		{
			if (propertyName != null)
			{
				if (this.parent.GetType() != typeof(ManagementObject))
				{
					int num = (int)propertyType;
					if (isArray)
					{
						num = num | 0x2000;
					}
					object value = DBNull.Value;
					int num1 = this.parent.wbemObject.Put_(propertyName, 0, ref value, num);
					if (num1 < 0)
					{
						if (((long)num1 & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(num1);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
							return;
						}
					}
					return;
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			else
			{
				throw new ArgumentNullException(propertyName);
			}
		}

		public void CopyTo(Array array, int index)
		{
			if (array != null)
			{
				if (index < array.GetLowerBound(0) || index > array.GetUpperBound(0))
				{
					throw new ArgumentOutOfRangeException("index");
				}
				else
				{
					string[] strArrays = null;
					object obj = null;
					int num = 0;
					if (!this.isSystem)
					{
						num = num | 64;
					}
					else
					{
						num = num | 48;
					}
					//num = num;
					int names_ = this.parent.wbemObject.GetNames_(null, num, ref obj, out strArrays);
					if (names_ >= 0)
					{
						if (index + (int)strArrays.Length <= array.Length)
						{
							string[] strArrays1 = strArrays;
							for (int i = 0; i < (int)strArrays1.Length; i++)
							{
								string str = strArrays1[i];
								int num1 = index;
								index = num1 + 1;
								array.SetValue(new PropertyData(this.parent, str), num1);
							}
						}
						else
						{
							throw new ArgumentException(null, "index");
						}
					}
					if (names_ < 0)
					{
						if (((long)names_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(names_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)names_);
							return;
						}
					}
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("array");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void CopyTo(PropertyData[] propertyArray, int index)
		{
			this.CopyTo(propertyArray, index);
		}

		public PropertyDataCollection.PropertyDataEnumerator GetEnumerator()
		{
			return new PropertyDataCollection.PropertyDataEnumerator(this.parent, this.isSystem);
		}

		public virtual void Remove(string propertyName)
		{
			if (this.parent.GetType() != typeof(ManagementObject))
			{
				int num = this.parent.wbemObject.Delete_(propertyName);
				if (num < 0)
				{
					if (((long)num & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
						return;
					}
				}
				return;
			}
			else
			{
				ManagementClass managementClass = new ManagementClass(this.parent.ClassPath);
				this.parent.SetPropertyValue(propertyName, managementClass.GetPropertyValue(propertyName));
				return;
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new PropertyDataCollection.PropertyDataEnumerator(this.parent, this.isSystem);
		}

		public class PropertyDataEnumerator : IEnumerator
		{
			private ManagementBaseObject parent;

			private string[] propertyNames;

			private int index;

			public PropertyData Current
			{
				get
				{
					if (this.index == -1 || this.index == (int)this.propertyNames.Length)
					{
						throw new InvalidOperationException();
					}
					else
					{
						return new PropertyData(this.parent, this.propertyNames[this.index]);
					}
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

			internal PropertyDataEnumerator(ManagementBaseObject parent, bool isSystem)
			{
				int num;
				this.parent = parent;
				this.propertyNames = null;
				this.index = -1;
				object obj = null;
				if (!isSystem)
				{
					num = 64;
				}
				else
				{
					num = 48;
				}
				int names_ = parent.wbemObject.GetNames_(null, num, ref obj, out this.propertyNames);
				if (names_ < 0)
				{
					if (((long)names_ & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(names_);
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)names_);
						return;
					}
				}
			}

			public bool MoveNext()
			{
				if (this.index != (int)this.propertyNames.Length)
				{
					PropertyDataCollection.PropertyDataEnumerator propertyDataEnumerator = this;
					propertyDataEnumerator.index = propertyDataEnumerator.index + 1;
					if (this.index == (int)this.propertyNames.Length)
					{
						return false;
					}
					else
					{
						return true;
					}
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				this.index = -1;
			}
		}
	}
}