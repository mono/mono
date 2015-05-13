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
	public class QualifierDataCollection : ICollection, IEnumerable
	{
		private ManagementBaseObject parent;

		private string propertyOrMethodName;

		private QualifierType qualifierSetType;

		public int Count
		{
			get
			{
				int num;
				string[] strArrays = null;
				try
				{
					IWbemQualifierSetFreeThreaded typeQualifierSet = this.GetTypeQualifierSet();
					int names_ = typeQualifierSet.GetNames_(0, out strArrays);
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
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					if (this.qualifierSetType != QualifierType.PropertyQualifier || managementException.ErrorCode != ManagementStatus.SystemProperty)
					{
						throw;
					}
					else
					{
						num = 0;
					}
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

		public virtual QualifierData this[string qualifierName]
		{
			get
			{
				if (qualifierName != null)
				{
					return new QualifierData(this.parent, this.propertyOrMethodName, qualifierName, this.qualifierSetType);
				}
				else
				{
					throw new ArgumentNullException("qualifierName");
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

		internal QualifierDataCollection(ManagementBaseObject parent)
		{
			this.parent = parent;
			this.qualifierSetType = QualifierType.ObjectQualifier;
			this.propertyOrMethodName = null;
		}

		internal QualifierDataCollection(ManagementBaseObject parent, string propertyOrMethodName, QualifierType type)
		{
			this.parent = parent;
			this.propertyOrMethodName = propertyOrMethodName;
			this.qualifierSetType = type;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public virtual void Add(string qualifierName, object qualifierValue)
		{
			this.Add(qualifierName, qualifierValue, false, false, false, true);
		}

		public virtual void Add(string qualifierName, object qualifierValue, bool isAmended, bool propagatesToInstance, bool propagatesToSubclass, bool isOverridable)
		{
			int num = 0;
			if (isAmended)
			{
				num = num | 128;
			}
			if (propagatesToInstance)
			{
				num = num | 1;
			}
			if (propagatesToSubclass)
			{
				num = num | 2;
			}
			if (!isOverridable)
			{
				num = num | 16;
			}
			int num1 = this.GetTypeQualifierSet().Put_(qualifierName, ref qualifierValue, num);
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
		}

		public void CopyTo(Array array, int index)
		{
			IWbemQualifierSetFreeThreaded typeQualifierSet;
			if (array != null)
			{
				if (index < array.GetLowerBound(0) || index > array.GetUpperBound(0))
				{
					throw new ArgumentOutOfRangeException("index");
				}
				else
				{
					string[] strArrays = null;
					try
					{
						typeQualifierSet = this.GetTypeQualifierSet();
					}
					catch (ManagementException managementException1)
					{
						ManagementException managementException = managementException1;
						if (this.qualifierSetType != QualifierType.PropertyQualifier || managementException.ErrorCode != ManagementStatus.SystemProperty)
						{
							throw;
						}
						else
						{
							return;
						}
					}
					int names_ = typeQualifierSet.GetNames_(0, out strArrays);
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
					if (index + (int)strArrays.Length <= array.Length)
					{
						string[] strArrays1 = strArrays;
						for (int i = 0; i < (int)strArrays1.Length; i++)
						{
							string str = strArrays1[i];
							int num = index;
							index = num + 1;
							array.SetValue(new QualifierData(this.parent, this.propertyOrMethodName, str, this.qualifierSetType), num);
						}
					}
					else
					{
						throw new ArgumentException(null, "index");
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
		public void CopyTo(QualifierData[] qualifierArray, int index)
		{
			this.CopyTo(qualifierArray, index);
		}

		public QualifierDataCollection.QualifierDataEnumerator GetEnumerator()
		{
			return new QualifierDataCollection.QualifierDataEnumerator(this.parent, this.propertyOrMethodName, this.qualifierSetType);
		}

		private IWbemQualifierSetFreeThreaded GetTypeQualifierSet()
		{
			return this.GetTypeQualifierSet(this.qualifierSetType);
		}

		private IWbemQualifierSetFreeThreaded GetTypeQualifierSet(QualifierType qualifierSetType)
		{
			IWbemQualifierSetFreeThreaded wbemQualifierSetFreeThreaded = null;
			int qualifierSet_ = 0;
			QualifierType qualifierType = qualifierSetType;
			if (qualifierType == QualifierType.ObjectQualifier)
			{
				qualifierSet_ = this.parent.wbemObject.GetQualifierSet_(out wbemQualifierSetFreeThreaded);
			}
			else if (qualifierType == QualifierType.PropertyQualifier)
			{
				qualifierSet_ = this.parent.wbemObject.GetPropertyQualifierSet_(this.propertyOrMethodName, out wbemQualifierSetFreeThreaded);
			}
			else if (qualifierType == QualifierType.MethodQualifier)
			{
				qualifierSet_ = this.parent.wbemObject.GetMethodQualifierSet_(this.propertyOrMethodName, out wbemQualifierSetFreeThreaded);
			}
			else
			{
				throw new ManagementException(ManagementStatus.Unexpected, null, null);
			}
			if (qualifierSet_ < 0)
			{
				if (((long)qualifierSet_ & (long)-4096) != (long)-2147217408)
				{
					Marshal.ThrowExceptionForHR(qualifierSet_);
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)qualifierSet_);
				}
			}
			return wbemQualifierSetFreeThreaded;
		}

		public virtual void Remove(string qualifierName)
		{
			int num = this.GetTypeQualifierSet().Delete_(qualifierName);
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
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new QualifierDataCollection.QualifierDataEnumerator(this.parent, this.propertyOrMethodName, this.qualifierSetType);
		}

		public class QualifierDataEnumerator : IEnumerator
		{
			private ManagementBaseObject parent;

			private string propertyOrMethodName;

			private QualifierType qualifierType;

			private string[] qualifierNames;

			private int index;

			public QualifierData Current
			{
				get
				{
					if (this.index == -1 || this.index == (int)this.qualifierNames.Length)
					{
						throw new InvalidOperationException();
					}
					else
					{
						return new QualifierData(this.parent, this.propertyOrMethodName, this.qualifierNames[this.index], this.qualifierType);
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

			internal QualifierDataEnumerator(ManagementBaseObject parent, string propertyOrMethodName, QualifierType qualifierType)
			{
				this.index = -1;
				this.parent = parent;
				this.propertyOrMethodName = propertyOrMethodName;
				this.qualifierType = qualifierType;
				this.qualifierNames = null;
				IWbemQualifierSetFreeThreaded wbemQualifierSetFreeThreaded = null;
				int qualifierSet_ = 0;
				QualifierType qualifierType1 = qualifierType;
				if (qualifierType1 == QualifierType.ObjectQualifier)
				{
					qualifierSet_ = parent.wbemObject.GetQualifierSet_(out wbemQualifierSetFreeThreaded);
				}
				else if (qualifierType1 == QualifierType.PropertyQualifier)
				{
					qualifierSet_ = parent.wbemObject.GetPropertyQualifierSet_(propertyOrMethodName, out wbemQualifierSetFreeThreaded);
				}
				else if (qualifierType1 == QualifierType.MethodQualifier)
				{
					qualifierSet_ = parent.wbemObject.GetMethodQualifierSet_(propertyOrMethodName, out wbemQualifierSetFreeThreaded);
				}
				else
				{
					throw new ManagementException(ManagementStatus.Unexpected, null, null);
				}
				if (qualifierSet_ >= 0)
				{
					qualifierSet_ = wbemQualifierSetFreeThreaded.GetNames_(0, out this.qualifierNames);
					if (qualifierSet_ < 0)
					{
						if (((long)qualifierSet_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(qualifierSet_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)qualifierSet_);
							return;
						}
					}
					return;
				}
				else
				{
					this.qualifierNames = new string[0];
					return;
				}
			}

			public bool MoveNext()
			{
				if (this.index != (int)this.qualifierNames.Length)
				{
					QualifierDataCollection.QualifierDataEnumerator qualifierDataEnumerator = this;
					qualifierDataEnumerator.index = qualifierDataEnumerator.index + 1;
					if (this.index == (int)this.qualifierNames.Length)
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