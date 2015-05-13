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
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Management
{
	public class QualifierData
	{
		private ManagementBaseObject parent;

		private string propertyOrMethodName;

		private string qualifierName;

		private QualifierType qualifierType;

		private object qualifierValue;

		private int qualifierFlavor;

		private IWbemQualifierSetFreeThreaded qualifierSet;

		public bool IsAmended
		{
			get
			{
				this.RefreshQualifierInfo();
				return 128 == (this.qualifierFlavor & 128);
			}
			set
			{
				this.RefreshQualifierInfo();
				int num = this.qualifierFlavor & -97;
				if (!value)
				{
					num = num & -129;
				}
				else
				{
					num = num | 128;
				}
				int num1 = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, num);
				if (((long)num1 & (long)-4096) != (long)-2147217408)
				{
					if (((long)num1 & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num1);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
					return;
				}
			}
		}

		public bool IsLocal
		{
			get
			{
				this.RefreshQualifierInfo();
				return 0 == (this.qualifierFlavor & 96);
			}
		}

		public bool IsOverridable
		{
			get
			{
				this.RefreshQualifierInfo();
				return 0 == (this.qualifierFlavor & 16);
			}
			set
			{
				this.RefreshQualifierInfo();
				int num = this.qualifierFlavor & -97;
				if (!value)
				{
					num = num | 16;
				}
				else
				{
					num = num & -17;
				}
				int num1 = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, num);
				if (((long)num1 & (long)-4096) != (long)-2147217408)
				{
					if (((long)num1 & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num1);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
					return;
				}
			}
		}

		public string Name
		{
			get
			{
				if (this.qualifierName != null)
				{
					return this.qualifierName;
				}
				else
				{
					return "";
				}
			}
		}

		public bool PropagatesToInstance
		{
			get
			{
				this.RefreshQualifierInfo();
				return 1 == (this.qualifierFlavor & 1);
			}
			set
			{
				this.RefreshQualifierInfo();
				int num = this.qualifierFlavor & -97;
				if (!value)
				{
					num = num & -2;
				}
				else
				{
					num = num | 1;
				}
				int num1 = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, num);
				if (((long)num1 & (long)-4096) != (long)-2147217408)
				{
					if (((long)num1 & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num1);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
					return;
				}
			}
		}

		public bool PropagatesToSubclass
		{
			get
			{
				this.RefreshQualifierInfo();
				return 2 == (this.qualifierFlavor & 2);
			}
			set
			{
				this.RefreshQualifierInfo();
				int num = this.qualifierFlavor & -97;
				if (!value)
				{
					num = num & -3;
				}
				else
				{
					num = num | 2;
				}
				int num1 = this.qualifierSet.Put_(this.qualifierName, ref this.qualifierValue, num);
				if (((long)num1 & (long)-4096) != (long)-2147217408)
				{
					if (((long)num1 & (long)-2147483648) != (long)0)
					{
						Marshal.ThrowExceptionForHR(num1);
					}
					return;
				}
				else
				{
					ManagementException.ThrowWithExtendedInfo((ManagementStatus)num1);
					return;
				}
			}
		}

		public object Value
		{
			get
			{
				this.RefreshQualifierInfo();
				return ValueTypeSafety.GetSafeObject(this.qualifierValue);
			}
			set
			{
				this.RefreshQualifierInfo();
				object wmiValue = QualifierData.MapQualValueToWmiValue(value);
				int num = this.qualifierSet.Put_(this.qualifierName, ref wmiValue, this.qualifierFlavor & -97);
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
		}

		internal QualifierData(ManagementBaseObject parent, string propName, string qualName, QualifierType type)
		{
			this.parent = parent;
			this.propertyOrMethodName = propName;
			this.qualifierName = qualName;
			this.qualifierType = type;
			this.RefreshQualifierInfo();
		}

		private static object MapQualValueToWmiValue(object qualVal)
		{
			Type type;
			object value = DBNull.Value;
			if (qualVal != null)
			{
				if (qualVal as Array == null)
				{
					value = qualVal;
				}
				else
				{
					if (qualVal as int[] != null || qualVal as double[] != null || qualVal as string[] != null || qualVal as bool[] != null)
					{
						value = qualVal;
					}
					else
					{
						Array arrays = (Array)qualVal;
						int length = arrays.Length;
						if (length > 0)
						{
							type = arrays.GetValue(0).GetType();
						}
						else
						{
							type = null;
						}
						Type type1 = type;
						if (type1 != typeof(int))
						{
							if (type1 != typeof(double))
							{
								if (type1 != typeof(string))
								{
									if (type1 != typeof(bool))
									{
										value = arrays;
									}
									else
									{
										value = new bool[length];
										for (int i = 0; i < length; i++)
										{
											((bool[])value)[i] = Convert.ToBoolean(arrays.GetValue(i), (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(bool)));
										}
									}
								}
								else
								{
									value = new string[length];
									for (int j = 0; j < length; j++)
									{
										((string[])value)[j] = arrays.GetValue(j).ToString();
									}
								}
							}
							else
							{
								value = new double[length];
								for (int k = 0; k < length; k++)
								{
									((double[])value)[k] = Convert.ToDouble(arrays.GetValue(k), (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(double)));
								}
							}
						}
						else
						{
							value = new int[length];
							for (int l = 0; l < length; l++)
							{
								((int[])value)[l] = Convert.ToInt32(arrays.GetValue(l), (IFormatProvider)CultureInfo.InvariantCulture.GetFormat(typeof(int)));
							}
						}
					}
				}
			}
			return value;
		}

		private void RefreshQualifierInfo()
		{
			int qualifierSet_ = -2147217407;
			this.qualifierSet = null;
			QualifierType qualifierType = this.qualifierType;
			if (qualifierType == QualifierType.ObjectQualifier)
			{
				qualifierSet_ = this.parent.wbemObject.GetQualifierSet_(out this.qualifierSet);
			}
			else if (qualifierType == QualifierType.PropertyQualifier)
			{
				qualifierSet_ = this.parent.wbemObject.GetPropertyQualifierSet_(this.propertyOrMethodName, out this.qualifierSet);
			}
			else if (qualifierType == QualifierType.MethodQualifier)
			{
				qualifierSet_ = this.parent.wbemObject.GetMethodQualifierSet_(this.propertyOrMethodName, out this.qualifierSet);
			}
			else
			{
				throw new ManagementException(ManagementStatus.Unexpected, null, null);
			}
			if (((long)qualifierSet_ & (long)-2147483648) == (long)0)
			{
				this.qualifierValue = null;
				if (this.qualifierSet != null)
				{
					qualifierSet_ = this.qualifierSet.Get_(this.qualifierName, 0, ref this.qualifierValue, ref this.qualifierFlavor);
				}
			}
			if (((long)qualifierSet_ & (long)-4096) != (long)-2147217408)
			{
				if (((long)qualifierSet_ & (long)-2147483648) != (long)0)
				{
					Marshal.ThrowExceptionForHR(qualifierSet_);
				}
				return;
			}
			else
			{
				ManagementException.ThrowWithExtendedInfo((ManagementStatus)qualifierSet_);
				return;
			}
		}
	}
}