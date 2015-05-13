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
	public class PropertyData
	{
		private ManagementBaseObject parent;

		private string propertyName;

		private object propertyValue;

		private long propertyNullEnumValue;

		private int propertyType;

		private int propertyFlavor;

		private QualifierDataCollection qualifiers;

		public bool IsArray
		{
			get
			{
				this.RefreshPropertyInfo();
				return (this.propertyType & 0x2000) != 0;
			}
		}

		public bool IsLocal
		{
			get
			{
				this.RefreshPropertyInfo();
				if ((this.propertyFlavor & 32) != 0)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public string Name
		{
			get
			{
				if (this.propertyName != null)
				{
					return this.propertyName;
				}
				else
				{
					return "";
				}
			}
		}

		internal long NullEnumValue
		{
			get
			{
				return this.propertyNullEnumValue;
			}
			set
			{
				this.propertyNullEnumValue = value;
			}
		}

		public string Origin
		{
			get
			{
				string empty = null;
				int propertyOrigin_ = this.parent.wbemObject.GetPropertyOrigin_(this.propertyName, out empty);
				if (propertyOrigin_ < 0)
				{
					if (propertyOrigin_ != -2147217393)
					{
						if (((long)propertyOrigin_ & (long)-4096) != (long)-2147217408)
						{
							Marshal.ThrowExceptionForHR(propertyOrigin_);
						}
						else
						{
							ManagementException.ThrowWithExtendedInfo((ManagementStatus)propertyOrigin_);
						}
					}
					else
					{
						empty = string.Empty;
					}
				}
				return empty;
			}
		}

		public QualifierDataCollection Qualifiers
		{
			get
			{
				if (this.qualifiers == null)
				{
					this.qualifiers = new QualifierDataCollection(this.parent, this.propertyName, QualifierType.PropertyQualifier);
				}
				return this.qualifiers;
			}
		}

		public CimType Type
		{
			get
			{
				this.RefreshPropertyInfo();
				return (CimType)(this.propertyType & -8193);
			}
		}

		public object Value
		{
			get
			{
				this.RefreshPropertyInfo();
				return ValueTypeSafety.GetSafeObject(PropertyData.MapWmiValueToValue(this.propertyValue, (CimType)(this.propertyType & -8193), 0 != (this.propertyType & 0x2000)));
			}
			set
			{
				this.RefreshPropertyInfo();
				object wmiValue = PropertyData.MapValueToWmiValue(value, (CimType)(this.propertyType & -8193), 0 != (this.propertyType & 0x2000));
				int num = this.parent.wbemObject.Put_(this.propertyName, 0, ref wmiValue, this.propertyType);
				if (num >= 0)
				{
					if (this.parent.GetType() == typeof(ManagementObject))
					{
						((ManagementObject)this.parent).Path.UpdateRelativePath((string)this.parent["__RELPATH"]);
					}
					return;
				}
				else
				{
					if (((long)num & (long)-4096) != (long)-2147217408)
					{
						Marshal.ThrowExceptionForHR(num);
						return;
					}
					else
					{
						ManagementException.ThrowWithExtendedInfo((ManagementStatus)num);
						return;
					}
				}
			}
		}

		internal PropertyData(ManagementBaseObject parent, string propName)
		{
			this.parent = parent;
			this.propertyName = propName;
			this.qualifiers = null;
			this.RefreshPropertyInfo();
		}

		internal static object MapValueToWmiValue(object val, CimType type, bool isArray)
		{
			object value = DBNull.Value;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			if (val != null)
			{
				if (!isArray)
				{
					CimType cimType = type;
					switch (cimType)
					{
						case CimType.SInt16:
						{
							value = Convert.ToInt16(val, (IFormatProvider)invariantCulture.GetFormat(typeof(short)));
							break;
						}
						case CimType.SInt32:
						{
							value = Convert.ToInt32(val, (IFormatProvider)invariantCulture.GetFormat(typeof(int)));
							break;
						}
						case CimType.Real32:
						{
							value = Convert.ToSingle(val, (IFormatProvider)invariantCulture.GetFormat(typeof(float)));
							break;
						}
						case CimType.Real64:
						{
							value = Convert.ToDouble(val, (IFormatProvider)invariantCulture.GetFormat(typeof(double)));
							break;
						}
						case CimType.SInt16 | CimType.Real32:
						case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64:
						/*case 9: */
						case CimType.SInt16 | CimType.String:
						case CimType.Real32 | CimType.String:
						case CimType.SInt16 | CimType.Real32 | CimType.String:
						case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64 | CimType.Boolean | CimType.String | CimType.Object:
						{
							value = val;
							break;
						}
						case CimType.String:
						{
							value = val.ToString();
							break;
						}
						case CimType.Boolean:
						{
							value = Convert.ToBoolean(val, (IFormatProvider)invariantCulture.GetFormat(typeof(bool)));
							break;
						}
						case CimType.Object:
						{
							if (val as ManagementBaseObject == null)
							{
								value = val;
								break;
							}
							else
							{
								value = Marshal.GetObjectForIUnknown(((ManagementBaseObject)val).wbemObject);
								break;
							}
						}
						case CimType.SInt8:
						{
							value = (short)Convert.ToSByte(val, (IFormatProvider)invariantCulture.GetFormat(typeof(short)));
							break;
						}
						case CimType.UInt8:
						{
							value = Convert.ToByte(val, (IFormatProvider)invariantCulture.GetFormat(typeof(byte)));
							break;
						}
						case CimType.UInt16:
						{
							value = (int)Convert.ToUInt16(val, (IFormatProvider)invariantCulture.GetFormat(typeof(ushort)));
							break;
						}
						case CimType.UInt32:
						{
							value = (int)Convert.ToUInt32(val, (IFormatProvider)invariantCulture.GetFormat(typeof(uint)));
							break;
						}
						case CimType.SInt64:
						{
							long num = Convert.ToInt64(val, (IFormatProvider)invariantCulture.GetFormat(typeof(long)));
							value = num.ToString((IFormatProvider)invariantCulture.GetFormat(typeof(long)));
							break;
						}
						case CimType.UInt64:
						{
							ulong num1 = Convert.ToUInt64(val, (IFormatProvider)invariantCulture.GetFormat(typeof(ulong)));
							value = num1.ToString((IFormatProvider)invariantCulture.GetFormat(typeof(ulong)));
							break;
						}
						default:
						{
							if (cimType == CimType.DateTime || cimType == CimType.Reference)
							{
								value = val.ToString();
								break;
							}
							else if (cimType == CimType.Char16)
							{
								value = (short)Convert.ToChar(val, (IFormatProvider)invariantCulture.GetFormat(typeof(char)));
								break;
							}
							value = val;
							break;
						}
					}
				}
				else
				{
					Array arrays = (Array)val;
					int length = arrays.Length;
					CimType cimType1 = type;
					switch (cimType1)
					{
						case CimType.SInt16:
						{
							if (val as short[] == null)
							{
								value = new short[length];
								for (int i = 0; i < length; i++)
								{
									((short[])value)[i] = Convert.ToInt16(arrays.GetValue(i), (IFormatProvider)invariantCulture.GetFormat(typeof(short)));
								}
								break;
							}
							else
							{
								value = val;
								break;
							}
						}
						case CimType.SInt32:
						{
							if (val as int[] == null)
							{
								value = new int[length];
								for (int j = 0; j < length; j++)
								{
									((int[])value)[j] = Convert.ToInt32(arrays.GetValue(j), (IFormatProvider)invariantCulture.GetFormat(typeof(int)));
								}
								break;
							}
							else
							{
								value = val;
								break;
							}
						}
						case CimType.Real32:
						{
							if (val as float[] == null)
							{
								value = new float[length];
								for (int k = 0; k < length; k++)
								{
									((float[])value)[k] = Convert.ToSingle(arrays.GetValue(k), (IFormatProvider)invariantCulture.GetFormat(typeof(float)));
								}
								break;
							}
							else
							{
								value = val;
								break;
							}
						}
						case CimType.Real64:
						{
							if (val as double[] == null)
							{
								value = new double[length];
								for (int l = 0; l < length; l++)
								{
									((double[])value)[l] = Convert.ToDouble(arrays.GetValue(l), (IFormatProvider)invariantCulture.GetFormat(typeof(double)));
								}
								break;
							}
							else
							{
								value = val;
								break;
							}
						}
						case CimType.SInt16 | CimType.Real32:
						case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64:
						/*case 9: */
						case CimType.SInt16 | CimType.String:
						case CimType.Real32 | CimType.String:
						case CimType.SInt16 | CimType.Real32 | CimType.String:
						case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64 | CimType.Boolean | CimType.String | CimType.Object:
						{
							value = val;
							break;
						}
						case CimType.String:
						{
							if (val as string[] == null)
							{
								value = new string[length];
								for (int m = 0; m < length; m++)
								{
									((string[])value)[m] = arrays.GetValue(m).ToString();
								}
								break;
							}
							else
							{
								value = val;
								break;
							}
						}
						case CimType.Boolean:
						{
							if (val as bool[] == null)
							{
								value = new bool[length];
								for (int n = 0; n < length; n++)
								{
									((bool[])value)[n] = Convert.ToBoolean(arrays.GetValue(n), (IFormatProvider)invariantCulture.GetFormat(typeof(bool)));
								}
								break;
							}
							else
							{
								value = val;
								break;
							}
						}
						case CimType.Object:
						{
							value = new IWbemClassObject_DoNotMarshal[length];
							for (int o = 0; o < length; o++)
							{
								((IWbemClassObject_DoNotMarshal[])value)[o] = (IWbemClassObject_DoNotMarshal)Marshal.GetObjectForIUnknown(((ManagementBaseObject)arrays.GetValue(o)).wbemObject);
							}
							break;
						}
						case CimType.SInt8:
						{
							value = new short[length];
							for (int p = 0; p < length; p++)
							{
								((short[])value)[p] = Convert.ToSByte(arrays.GetValue(p), (IFormatProvider)invariantCulture.GetFormat(typeof(sbyte)));
							}
							break;
						}
						case CimType.UInt8:
						{
							if (val as byte[] == null)
							{
								value = new byte[length];
								for (int q = 0; q < length; q++)
								{
									((byte[])value)[q] = Convert.ToByte(arrays.GetValue(q), (IFormatProvider)invariantCulture.GetFormat(typeof(byte)));
								}
								break;
							}
							else
							{
								value = val;
								break;
							}
						}
						case CimType.UInt16:
						{
							value = new int[length];
							for (int r = 0; r < length; r++)
							{
								((int[])value)[r] = Convert.ToUInt16(arrays.GetValue(r), (IFormatProvider)invariantCulture.GetFormat(typeof(ushort)));
							}
							break;
						}
						case CimType.UInt32:
						{
							value = new int[length];
							for (int s = 0; s < length; s++)
							{
								((uint[])value)[s] = Convert.ToUInt32(arrays.GetValue(s), (IFormatProvider)invariantCulture.GetFormat(typeof(uint)));
							}
							break;
						}
						case CimType.SInt64:
						{
							value = new string[length];
							for (int t = 0; t < length; t++)
							{
								long num2 = Convert.ToInt64(arrays.GetValue(t), (IFormatProvider)invariantCulture.GetFormat(typeof(long)));
								((string[])value)[t] = num2.ToString((IFormatProvider)invariantCulture.GetFormat(typeof(long)));
							}
							break;
						}
						case CimType.UInt64:
						{
							value = new string[length];
							for (int u = 0; u < length; u++)
							{
								ulong num3 = Convert.ToUInt64(arrays.GetValue(u), (IFormatProvider)invariantCulture.GetFormat(typeof(ulong)));
								((string[])value)[u] = num3.ToString((IFormatProvider)invariantCulture.GetFormat(typeof(ulong)));
							}
							break;
						}
						default:
						{
							if (cimType1 == CimType.DateTime || cimType1 == CimType.Reference)
							{
								if (val as string[] == null)
								{
									value = new string[length];
									for (int m = 0; m < length; m++)
									{
										((string[])value)[m] = arrays.GetValue(m).ToString();
									}
									break;
								}
								else
								{
									value = val;
									break;
								}
							}
							else if (cimType1 == CimType.Char16)
							{
								value = new short[length];
								for (int v = 0; v < length; v++)
								{
									((short[])value)[v] = (short)Convert.ToChar(arrays.GetValue(v), (IFormatProvider)invariantCulture.GetFormat(typeof(char)));
								}
								break;
							}
							value = val;
							break;
						}
					}
				}
			}
			return value;
		}

		internal static object MapValueToWmiValue(object val, out bool isArray, out CimType type)
		{
			object value = DBNull.Value;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			isArray = false;
			type = CimType.None;
			if (val != null)
			{
				isArray = val.GetType().IsArray;
				Type type1 = val.GetType();
				if (!isArray)
				{
					if (type1 != typeof(ushort))
					{
						if (type1 != typeof(uint))
						{
							if (type1 != typeof(ulong))
							{
								if (type1 != typeof(sbyte))
								{
									if (type1 != typeof(byte))
									{
										if (type1 != typeof(short))
										{
											if (type1 != typeof(int))
											{
												if (type1 != typeof(long))
												{
													if (type1 != typeof(bool))
													{
														if (type1 != typeof(float))
														{
															if (type1 != typeof(double))
															{
																if (type1 != typeof(char))
																{
																	if (type1 != typeof(string))
																	{
																		if (val as ManagementBaseObject != null)
																		{
																			type = CimType.Object;
																			value = Marshal.GetObjectForIUnknown(((ManagementBaseObject)val).wbemObject);
																		}
																	}
																	else
																	{
																		type = CimType.String;
																		value = val;
																	}
																}
																else
																{
																	type = CimType.Char16;
																	value = ((IConvertible)(char)val).ToInt16(null);
																}
															}
															else
															{
																type = CimType.Real64;
																value = val;
															}
														}
														else
														{
															type = CimType.Real32;
															value = val;
														}
													}
													else
													{
														type = CimType.Boolean;
														value = val;
													}
												}
												else
												{
													type = CimType.SInt64;
													value = val.ToString();
												}
											}
											else
											{
												type = CimType.SInt32;
												value = val;
											}
										}
										else
										{
											type = CimType.SInt16;
											value = val;
										}
									}
									else
									{
										type = CimType.UInt8;
										value = val;
									}
								}
								else
								{
									type = CimType.SInt8;
									value = ((IConvertible)(sbyte)val).ToInt16(null);
								}
							}
							else
							{
								type = CimType.UInt64;
								ulong num = (ulong)val;
								value = num.ToString((IFormatProvider)invariantCulture.GetFormat(typeof(ulong)));
							}
						}
						else
						{
							type = CimType.UInt32;
							if (((uint)val & -2147483648) == 0)
							{
								value = Convert.ToInt32(val, (IFormatProvider)invariantCulture.GetFormat(typeof(int)));
							}
							else
							{
								value = Convert.ToString(val, (IFormatProvider)invariantCulture.GetFormat(typeof(uint)));
							}
						}
					}
					else
					{
						type = CimType.UInt16;
						value = ((IConvertible)(ushort)val).ToInt32(null);
					}
				}
				else
				{
					Type elementType = type1.GetElementType();
					if (!elementType.IsPrimitive)
					{
						if (elementType != typeof(string))
						{
							if (val as ManagementBaseObject[] != null)
							{
								Array arrays = (Array)val;
								int length = arrays.Length;
								type = CimType.Object;
								value = new IWbemClassObject_DoNotMarshal[length];
								for (int i = 0; i < length; i++)
								{
									((IWbemClassObject_DoNotMarshal[])value)[i] = (IWbemClassObject_DoNotMarshal)Marshal.GetObjectForIUnknown(((ManagementBaseObject)arrays.GetValue(i)).wbemObject);
								}
							}
						}
						else
						{
							type = CimType.String;
							value = (string[])val;
						}
					}
					else
					{
						if (elementType != typeof(byte))
						{
							if (elementType != typeof(sbyte))
							{
								if (elementType != typeof(bool))
								{
									if (elementType != typeof(ushort))
									{
										if (elementType != typeof(short))
										{
											if (elementType != typeof(int))
											{
												if (elementType != typeof(uint))
												{
													if (elementType != typeof(ulong))
													{
														if (elementType != typeof(long))
														{
															if (elementType != typeof(float))
															{
																if (elementType != typeof(double))
																{
																	if (elementType == typeof(char))
																	{
																		char[] chrArray = (char[])val;
																		int length1 = (int)chrArray.Length;
																		type = CimType.Char16;
																		value = new short[length1];
																		for (int j = 0; j < length1; j++)
																		{
																			((short[])value)[j] = ((IConvertible)chrArray[j]).ToInt16(null);
																		}
																	}
																}
																else
																{
																	type = CimType.Real64;
																	value = (double[])val;
																}
															}
															else
															{
																type = CimType.Real32;
																value = (float[])val;
															}
														}
														else
														{
															long[] numArray = (long[])val;
															int num1 = (int)numArray.Length;
															type = CimType.SInt64;
															value = new string[num1];
															for (int k = 0; k < num1; k++)
															{
																((string[])value)[k] = numArray[k].ToString((IFormatProvider)invariantCulture.GetFormat(typeof(long)));
															}
														}
													}
													else
													{
														ulong[] numArray1 = (ulong[])val;
														int length2 = (int)numArray1.Length;
														type = CimType.UInt64;
														value = new string[length2];
														for (int l = 0; l < length2; l++)
														{
															((string[])value)[l] = numArray1[l].ToString((IFormatProvider)invariantCulture.GetFormat(typeof(ulong)));
														}
													}
												}
												else
												{
													uint[] numArray2 = (uint[])val;
													int num2 = (int)numArray2.Length;
													type = CimType.UInt32;
													value = new string[num2];
													for (int m = 0; m < num2; m++)
													{
														((string[])value)[m] = numArray2[m].ToString((IFormatProvider)invariantCulture.GetFormat(typeof(uint)));
													}
												}
											}
											else
											{
												type = CimType.SInt32;
												value = (int[])val;
											}
										}
										else
										{
											type = CimType.SInt16;
											value = (short[])val;
										}
									}
									else
									{
										ushort[] numArray3 = (ushort[])val;
										int length3 = (int)numArray3.Length;
										type = CimType.UInt16;
										value = new int[length3];
										for (int n = 0; n < length3; n++)
										{
											((int[])value)[n] = ((IConvertible)numArray3[n]).ToInt32(null);
										}
									}
								}
								else
								{
									type = CimType.Boolean;
									value = (bool[])val;
								}
							}
							else
							{
								sbyte[] numArray4 = (sbyte[])val;
								int num3 = (int)numArray4.Length;
								type = CimType.SInt8;
								value = new short[num3];
								for (int o = 0; o < num3; o++)
								{
									((short[])value)[o] = ((IConvertible)numArray4[o]).ToInt16(null);
								}
							}
						}
						else
						{
							byte[] numArray5 = (byte[])val;
							int length4 = (int)numArray5.Length;
							type = CimType.UInt8;
							value = new short[length4];
							for (int p = 0; p < length4; p++)
							{
								((short[])value)[p] = ((IConvertible)numArray5[p]).ToInt16(null);
							}
						}
					}
				}
			}
			return value;
		}

		internal static object MapWmiValueToValue(object wmiValue, CimType type, bool isArray)
		{
			object managementBaseObject = null;
			if (DBNull.Value != wmiValue && wmiValue != null)
			{
				if (!isArray)
				{
					CimType cimType = type;
					switch (cimType)
					{
						case CimType.Object:
						{
							managementBaseObject = new ManagementBaseObject(new IWbemClassObjectFreeThreaded(Marshal.GetIUnknownForObject(wmiValue)));
							break;
						}
						case CimType.SInt16 | CimType.Real32 | CimType.String:
						case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64 | CimType.Boolean | CimType.String | CimType.Object:
						case CimType.UInt8:
						{
							managementBaseObject = wmiValue;
							break;
						}
						case CimType.SInt8:
						{
							managementBaseObject = (sbyte)((short)wmiValue);
							break;
						}
						case CimType.UInt16:
						{
							managementBaseObject = (ushort)(wmiValue);
							break;
						}
						case CimType.UInt32:
						{
							managementBaseObject = (uint)(wmiValue);
							break;
						}
						case CimType.SInt64:
						{
							managementBaseObject = wmiValue is long ? (long)wmiValue : Convert.ToInt64((string)wmiValue, (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(long)));
							break;
						}
						case CimType.UInt64:
						{
							managementBaseObject = wmiValue is ulong ? (ulong)wmiValue : Convert.ToUInt64((string)wmiValue, (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(ulong)));
							break;
						}
						default:
						{
							if (cimType == CimType.Char16)
							{
								managementBaseObject = (char)((ushort)((short)wmiValue));
								break;
							}
							else
							{
								managementBaseObject = wmiValue;
								break;
							}
						}
					}
				}
				else
				{
					Array arrays = (Array)wmiValue;
					int length = arrays.Length;
					CimType cimType1 = type;
					switch (cimType1)
					{
						case CimType.Object:
						{
							managementBaseObject = new ManagementBaseObject[length];
							for (int i = 0; i < length; i++)
							{
								((ManagementBaseObject[])managementBaseObject)[i] = new ManagementBaseObject(new IWbemClassObjectFreeThreaded(Marshal.GetIUnknownForObject(arrays.GetValue(i))));
							}
							break;
						}
						case CimType.SInt16 | CimType.Real32 | CimType.String:
						case CimType.SInt16 | CimType.SInt32 | CimType.Real32 | CimType.Real64 | CimType.Boolean | CimType.String | CimType.Object:
						case CimType.UInt8:
						{
							managementBaseObject = wmiValue;
							break;
						}
						case CimType.SInt8:
						{
							managementBaseObject = new sbyte[length];
							for (int j = 0; j < length; j++)
							{
								((sbyte[])managementBaseObject)[j] = (sbyte)((short)arrays.GetValue(j));
							}
							break;
						}
						case CimType.UInt16:
						{
							managementBaseObject = new ushort[length];
							for (int k = 0; k < length; k++)
							{
								((ushort[])managementBaseObject)[k] = (ushort)((int)arrays.GetValue(k));
							}
							break;
						}
						case CimType.UInt32:
						{
							managementBaseObject = new uint[length];
							for (int l = 0; l < length; l++)
							{
								((uint[])managementBaseObject)[l] = (uint)arrays.GetValue(l);
							}
							break;
						}
						case CimType.SInt64:
						{
							managementBaseObject = new long[length];
							for (int m = 0; m < length; m++)
							{
								((long[])managementBaseObject)[m] = Convert.ToInt64((string)arrays.GetValue(m), (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(long)));
							}
							break;
						}
						case CimType.UInt64:
						{
							managementBaseObject = new ulong[length];
							for (int n = 0; n < length; n++)
							{
								((ulong[])managementBaseObject)[n] = Convert.ToUInt64((string)arrays.GetValue(n), (IFormatProvider)CultureInfo.CurrentCulture.GetFormat(typeof(ulong)));
							}
							break;
						}
						default:
						{
							if (cimType1 == CimType.Char16)
							{
								managementBaseObject = new char[length];
								for (int o = 0; o < length; o++)
								{
									((char[])managementBaseObject)[o] = (char)((ushort)((short)arrays.GetValue(o)));
								}
								break;
							}
							else
							{
								managementBaseObject = wmiValue;
								break;
							}
						}
					}
				}
			}
			return managementBaseObject;
		}

		private void RefreshPropertyInfo()
		{
			this.propertyValue = null;
			int num = this.parent.wbemObject.Get_(this.propertyName, 0, ref this.propertyValue, ref this.propertyType, ref this.propertyFlavor);
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
	}
}