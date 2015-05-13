//
// AssemblyRef
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
using System.Management;
using System.Reflection;

namespace System.Management.Instrumentation
{
	internal class SchemaMapping
	{
		private Type classType;

		private ManagementClass newClass;

		private string className;

		private string classPath;

		private string codeClassName;

		private CodeWriter code;

		private InstrumentationType instrumentationType;

		public string ClassName
		{
			get
			{
				return this.className;
			}
		}

		public string ClassPath
		{
			get
			{
				return this.classPath;
			}
		}

		public Type ClassType
		{
			get
			{
				return this.classType;
			}
		}

		public CodeWriter Code
		{
			get
			{
				return this.code;
			}
		}

		public string CodeClassName
		{
			get
			{
				return this.codeClassName;
			}
		}

		public InstrumentationType InstrumentationType
		{
			get
			{
				return this.instrumentationType;
			}
		}

		public ManagementClass NewClass
		{
			get
			{
				return this.newClass;
			}
		}

		public SchemaMapping(Type type, SchemaNaming naming, Hashtable mapTypeToConverterClassName)
		{
			int num;
			bool flag;
			MemberInfo memberInfo;
			FieldInfo fieldInfo;
			PropertyInfo propertyInfo;
			MethodInfo getMethod;
			string memberName;
			Type propertyType;
			bool flag1;
			string str;
			string item;
			bool flag2;
			DictionaryEntry dictionaryEntry;
			string str1;
			string str2;
			CodeWriter codeWriter;
			CodeWriter codeWriter1;
			CodeWriter codeWriter2;
			CodeWriter codeWriter3;
			CodeWriter codeWriter4;
			CodeWriter codeWriter5;
			CodeWriter codeWriter6;
			CodeWriter codeWriter7;
			CodeWriter codeWriter8;
			CodeWriter codeWriter9;
			CodeWriter codeWriter10;
			CodeWriter codeWriter11;
			CodeWriter codeWriter12;
			CimType cimType;
			bool flag3;
			PropertyData propertyDatum;
			PropertyData item1;
			PropertyData propertyDatum1;
			MemberInfo[] members;
			int i;
			int num1;
			this.code = new CodeWriter();
			this.codeClassName = (string)mapTypeToConverterClassName[type];
			this.classType = type;
			bool flag4 = false;
			string baseClassName = ManagedNameAttribute.GetBaseClassName(type);
			this.className = ManagedNameAttribute.GetMemberName(type);
			this.instrumentationType = InstrumentationClassAttribute.GetAttribute(type).InstrumentationType;
			this.classPath = string.Concat(naming.NamespaceName, ":", this.className);
			if (baseClassName != null)
			{
				ManagementClass managementClass = new ManagementClass(string.Concat(naming.NamespaceName, ":", baseClassName));
				if (this.instrumentationType == InstrumentationType.Instance)
				{
					bool value = false;
					try
					{
						QualifierData qualifierDatum = managementClass.Qualifiers["abstract"];
						if (qualifierDatum.Value is bool)
						{
							value = (bool)qualifierDatum.Value;
						}
					}
					catch (ManagementException managementException1)
					{
						ManagementException managementException = managementException1;
						if (managementException.ErrorCode != ManagementStatus.NotFound)
						{
							throw;
						}
					}
					if (!value)
					{
						throw new Exception(RC.GetString("CLASSINST_EXCEPT"));
					}
				}
				this.newClass = managementClass.Derive(this.className);
			}
			else
			{
				this.newClass = new ManagementClass(naming.NamespaceName, "", null);
				this.newClass.SystemProperties["__CLASS"].Value = this.className;
			}
			CodeWriter codeWriter13 = this.code.AddChild(string.Concat("public class ", this.codeClassName, " : IWmiConverter"));
			CodeWriter codeWriter14 = codeWriter13.AddChild(new CodeWriter());
			codeWriter14.Line(string.Concat("static ManagementClass managementClass = new ManagementClass(@\"", this.classPath, "\");"));
			codeWriter14.Line("static IntPtr classWbemObjectIP;");
			codeWriter14.Line("static Guid iidIWbemObjectAccess = new Guid(\"49353C9A-516B-11D1-AEA6-00C04FB68820\");");
			codeWriter14.Line("internal ManagementObject instance = managementClass.CreateInstance();");
			codeWriter14.Line("object reflectionInfoTempObj = null ; ");
			codeWriter14.Line("FieldInfo reflectionIWbemClassObjectField = null ; ");
			codeWriter14.Line("IntPtr emptyWbemObject = IntPtr.Zero ; ");
			codeWriter14.Line("IntPtr originalObject = IntPtr.Zero ; ");
			codeWriter14.Line("bool toWmiCalled = false ; ");
			codeWriter14.Line("IntPtr theClone = IntPtr.Zero;");
			codeWriter14.Line("public static ManagementObject emptyInstance = managementClass.CreateInstance();");
			codeWriter14.Line("public IntPtr instWbemObjectAccessIP;");
			CodeWriter codeWriter15 = codeWriter13.AddChild(string.Concat("static ", this.codeClassName, "()"));
			codeWriter15.Line("classWbemObjectIP = (IntPtr)managementClass;");
			codeWriter15.Line("IntPtr wbemObjectAccessIP;");
			codeWriter15.Line("Marshal.QueryInterface(classWbemObjectIP, ref iidIWbemObjectAccess, out wbemObjectAccessIP);");
			codeWriter15.Line("int cimType;");
			CodeWriter codeWriter16 = codeWriter13.AddChild(string.Concat("public ", this.codeClassName, "()"));
			codeWriter16.Line("IntPtr wbemObjectIP = (IntPtr)instance;");
			codeWriter16.Line("originalObject = (IntPtr)instance;");
			codeWriter16.Line("Marshal.QueryInterface(wbemObjectIP, ref iidIWbemObjectAccess, out instWbemObjectAccessIP);");
			codeWriter16.Line("FieldInfo tempField = instance.GetType().GetField ( \"_wbemObject\", BindingFlags.Instance | BindingFlags.NonPublic );");
			codeWriter16.Line("if ( tempField == null )");
			codeWriter16.Line("{");
			codeWriter16.Line("   tempField = instance.GetType().GetField ( \"wbemObject\", BindingFlags.Instance | BindingFlags.NonPublic ) ;");
			codeWriter16.Line("}");
			codeWriter16.Line("reflectionInfoTempObj = tempField.GetValue (instance) ;");
			codeWriter16.Line("reflectionIWbemClassObjectField = reflectionInfoTempObj.GetType().GetField (\"pWbemClassObject\", BindingFlags.Instance | BindingFlags.NonPublic );");
			codeWriter16.Line("emptyWbemObject = (IntPtr) emptyInstance;");
			CodeWriter codeWriter17 = codeWriter13.AddChild(string.Concat("~", this.codeClassName, "()"));
			codeWriter17.AddChild("if(instWbemObjectAccessIP != IntPtr.Zero)").Line("Marshal.Release(instWbemObjectAccessIP);");
			codeWriter17.Line("if ( toWmiCalled == true )");
			codeWriter17.Line("{");
			codeWriter17.Line("\tMarshal.Release (originalObject);");
			codeWriter17.Line("}");
			CodeWriter codeWriter18 = codeWriter13.AddChild("public void ToWMI(object obj)");
			codeWriter18.Line("toWmiCalled = true ;");
			codeWriter18.Line("if(instWbemObjectAccessIP != IntPtr.Zero)");
			codeWriter18.Line("{");
			codeWriter18.Line("    Marshal.Release(instWbemObjectAccessIP);");
			codeWriter18.Line("    instWbemObjectAccessIP = IntPtr.Zero;");
			codeWriter18.Line("}");
			codeWriter18.Line("if(theClone != IntPtr.Zero)");
			codeWriter18.Line("{");
			codeWriter18.Line("    Marshal.Release(theClone);");
			codeWriter18.Line("    theClone = IntPtr.Zero;");
			codeWriter18.Line("}");
			codeWriter18.Line("IWOA.Clone_f(12, emptyWbemObject, out theClone) ;");
			codeWriter18.Line("Marshal.QueryInterface(theClone, ref iidIWbemObjectAccess, out instWbemObjectAccessIP) ;");
			codeWriter18.Line("reflectionIWbemClassObjectField.SetValue ( reflectionInfoTempObj, theClone ) ;");
			codeWriter18.Line(string.Format("{0} instNET = ({0})obj;", type.FullName.Replace('+', '.')));
			CodeWriter codeWriter19 = codeWriter13.AddChild(string.Concat("public static explicit operator IntPtr(", this.codeClassName, " obj)"));
			codeWriter19.Line("return obj.instWbemObjectAccessIP;");
			codeWriter14.Line("public ManagementObject GetInstance() {return instance;}");
			PropertyDataCollection properties = this.newClass.Properties;
			InstrumentationType instrumentationType = this.instrumentationType;
			switch (instrumentationType)
			{
				case InstrumentationType.Instance:
				{
					properties.Add("ProcessId", CimType.String, false);
					properties.Add("InstanceId", CimType.String, false);
					properties["ProcessId"].Qualifiers.Add("key", true);
					properties["InstanceId"].Qualifiers.Add("key", true);
					this.newClass.Qualifiers.Add("dynamic", true, false, false, false, true);
					this.newClass.Qualifiers.Add("provider", naming.DecoupledProviderInstanceName, false, false, false, true);
					num = 0;
					flag = false;
					members = type.GetMembers();
					for (i = 0; i < (int)members.Length; i++)
					{
						memberInfo = members[i];
						if ((memberInfo as FieldInfo != null || memberInfo as PropertyInfo != null) && (int)memberInfo.GetCustomAttributes(typeof(IgnoreMemberAttribute), false).Length <= 0)
						{
							if (memberInfo as FieldInfo == null)
							{
								if (memberInfo as PropertyInfo != null)
								{
									propertyInfo = memberInfo as PropertyInfo;
									if (!propertyInfo.CanRead)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									getMethod = propertyInfo.GetGetMethod();
									if (null == getMethod || getMethod.IsStatic)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									if ((int)getMethod.GetParameters().Length > 0)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
								}
							}
							else
							{
								fieldInfo = memberInfo as FieldInfo;
								if (fieldInfo.IsStatic)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
							}
							memberName = ManagedNameAttribute.GetMemberName(memberInfo);
							if (memberInfo as FieldInfo == null)
							{
								propertyType = (memberInfo as PropertyInfo).PropertyType;
							}
							else
							{
								propertyType = (memberInfo as FieldInfo).FieldType;
							}
							flag1 = false;
							if (propertyType.IsArray)
							{
								if (propertyType.GetArrayRank() != 1)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
								flag1 = true;
								propertyType = propertyType.GetElementType();
							}
							str = null;
							item = null;
							if (mapTypeToConverterClassName.Contains(propertyType))
							{
								item = (string)mapTypeToConverterClassName[propertyType];
								str = ManagedNameAttribute.GetMemberName(propertyType);
							}
							flag2 = false;
							if (propertyType == typeof(object))
							{
								flag2 = true;
								if (!flag4)
								{
									flag4 = true;
									codeWriter14.Line("static Hashtable mapTypeToConverter = new Hashtable();");
									foreach (DictionaryEntry dictionaryEntry1 in mapTypeToConverterClassName)
									{
										codeWriter15.Line(string.Format("mapTypeToConverter[typeof({0})] = typeof({1});", ((Type)dictionaryEntry1.Key).FullName.Replace('+', '.'), (string)dictionaryEntry1.Value));
									}
								}
							}
							str1 = string.Concat("prop_", (object)num);
							num1 = num;
							num = num1 + 1;
							str2 = string.Concat("handle_", (object)num1);
							codeWriter14.Line(string.Concat("static int ", str2, ";"));
							codeWriter15.Line(string.Format("IWOA.GetPropertyHandle_f27(27, wbemObjectAccessIP, \"{0}\", out cimType, out {1});", memberName, str2));
							codeWriter14.Line(string.Concat("PropertyData ", str1, ";"));
							codeWriter16.Line(string.Format("{0} = instance.Properties[\"{1}\"];", str1, memberName));
							if (!flag2)
							{
								if (str == null)
								{
									if (flag1)
									{
										if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
										{
											codeWriter18.AddChild(string.Format("if(null == instNET.{0})", memberInfo.Name)).Line(string.Format("{0}.Value = null;", str1));
											codeWriter18.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.WMITimeArrayToStringArray(instNET.{1});", str1, memberInfo.Name));
										}
										else
										{
											codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
										}
									}
									else
									{
										if (propertyType == typeof(byte) || propertyType == typeof(sbyte))
										{
											codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
											codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 1, ref instNET_{1});", str2, memberInfo.Name));
										}
										else
										{
											if (propertyType == typeof(short) || propertyType == typeof(ushort) || propertyType == typeof(char))
											{
												codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
												codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref instNET_{1});", str2, memberInfo.Name));
											}
											else
											{
												if (propertyType == typeof(uint) || propertyType == typeof(int) || propertyType == typeof(float))
												{
													codeWriter18.Line(string.Format("IWOA.WriteDWORD_f31(31, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
												}
												else
												{
													if (propertyType == typeof(ulong) || propertyType == typeof(long) || propertyType == typeof(double))
													{
														codeWriter18.Line(string.Format("IWOA.WriteQWORD_f33(33, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
													}
													else
													{
														if (propertyType != typeof(bool))
														{
															if (propertyType != typeof(string))
															{
																if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
																{
																	codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 52, SafeAssign.WMITimeToString(instNET.{1}));", str2, memberInfo.Name));
																}
																else
																{
																	codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
																}
															}
															else
															{
																codeWriter12 = codeWriter18.AddChild(string.Format("if(null != instNET.{0})", memberInfo.Name));
																codeWriter12.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, (instNET.{1}.Length+1)*2, instNET.{1});", str2, memberInfo.Name));
																codeWriter18.AddChild("else").Line(string.Format("IWOA.Put_f5(5, instWbemObjectAccessIP, \"{0}\", 0, ref nullObj, 8);", memberName));
																if (!flag)
																{
																	flag = true;
																	codeWriter14.Line("object nullObj = DBNull.Value;");
																}
															}
														}
														else
														{
															codeWriter18.Line(string.Format("if(instNET.{0})", memberInfo.Name));
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolTrue);", str2));
															codeWriter18.Line("else");
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolFalse);", str2));
														}
													}
												}
											}
										}
									}
								}
								else
								{
									if (!propertyType.IsValueType)
									{
										codeWriter5 = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
										codeWriter6 = codeWriter18.AddChild("else");
										codeWriter6.Line(string.Format("{0}.Value = null;", str1));
									}
									else
									{
										codeWriter5 = codeWriter18;
									}
									if (!flag1)
									{
										codeWriter14.Line(string.Format("{0} lazy_embeddedConverter_{1} = null;", item, str1));
										codeWriter9 = codeWriter13.AddChild(string.Format("{0} embeddedConverter_{1}", item, str1));
										codeWriter10 = codeWriter9.AddChild("get");
										codeWriter11 = codeWriter10.AddChild(string.Format("if(null == lazy_embeddedConverter_{0})", str1));
										codeWriter11.Line(string.Format("lazy_embeddedConverter_{0} = new {1}();", str1, item));
										codeWriter10.Line(string.Format("return lazy_embeddedConverter_{0};", str1));
										codeWriter5.Line(string.Format("embeddedConverter_{0}.ToWMI(instNET.{1});", str1, memberInfo.Name));
										codeWriter5.Line(string.Format("{0}.Value = embeddedConverter_{0}.instance;", str1));
									}
									else
									{
										codeWriter5.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
										codeWriter5.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
										codeWriter5.Line(string.Format("{0}[] embeddedConverters = new {0}[len];", item));
										codeWriter7 = codeWriter5.AddChild("for(int i=0;i<len;i++)");
										codeWriter7.Line(string.Format("embeddedConverters[i] = new {0}();", item));
										if (!propertyType.IsValueType)
										{
											codeWriter8 = codeWriter7.AddChild(string.Format("if(instNET.{0}[i] != null)", memberInfo.Name));
											codeWriter8.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										else
										{
											codeWriter7.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										codeWriter7.Line("embeddedObjects[i] = embeddedConverters[i].instance;");
										codeWriter5.Line(string.Format("{0}.Value = embeddedObjects;", str1));
									}
								}
							}
							else
							{
								codeWriter = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
								codeWriter1 = codeWriter18.AddChild("else");
								codeWriter1.Line(string.Format("{0}.Value = null;", str1));
								if (!flag1)
								{
									codeWriter4 = codeWriter.AddChild(string.Format("if(mapTypeToConverter.Contains(instNET.{0}.GetType()))", memberInfo.Name));
									codeWriter4.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}.GetType()];", memberInfo.Name));
									codeWriter4.Line("IWmiConverter converter = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter4.Line(string.Format("converter.ToWMI(instNET.{0});", memberInfo.Name));
									codeWriter4.Line(string.Format("{0}.Value = converter.GetInstance();", str1));
									codeWriter.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.GetInstance(instNET.{1});", str1, memberInfo.Name));
								}
								else
								{
									codeWriter.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
									codeWriter.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
									codeWriter.Line("IWmiConverter[] embeddedConverters = new IWmiConverter[len];");
									codeWriter2 = codeWriter.AddChild("for(int i=0;i<len;i++)");
									codeWriter3 = codeWriter2.AddChild(string.Format("if((instNET.{0}[i] != null) && mapTypeToConverter.Contains(instNET.{0}[i].GetType()))", memberInfo.Name));
									codeWriter3.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}[i].GetType()];", memberInfo.Name));
									codeWriter3.Line("embeddedConverters[i] = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter3.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
									codeWriter3.Line("embeddedObjects[i] = embeddedConverters[i].GetInstance();");
									codeWriter2.AddChild("else").Line(string.Format("embeddedObjects[i] = SafeAssign.GetManagementObject(instNET.{0}[i]);", memberInfo.Name));
									codeWriter.Line(string.Format("{0}.Value = embeddedObjects;", str1));
								}
							}
							cimType = CimType.String;
							if (memberInfo.DeclaringType == type)
							{
								flag3 = true;
								try
								{
									propertyDatum = this.newClass.Properties[memberName];
									CimType type1 = propertyDatum.Type;
									if (propertyDatum.IsLocal)
									{
										throw new ArgumentException(string.Format(RC.GetString("MEMBERCONFLILCT_EXCEPT"), memberInfo.Name), memberInfo.Name);
									}
								}
								catch (ManagementException managementException3)
								{
									ManagementException managementException2 = managementException3;
									if (managementException2.ErrorCode == ManagementStatus.NotFound)
									{
										flag3 = false;
									}
									else
									{
										throw;
									}
								}
								if (!flag3)
								{
									if (str == null)
									{
										if (!flag2)
										{
											if (propertyType != typeof(ManagementObject))
											{
												if (propertyType != typeof(sbyte))
												{
													if (propertyType != typeof(byte))
													{
														if (propertyType != typeof(short))
														{
															if (propertyType != typeof(ushort))
															{
																if (propertyType != typeof(int))
																{
																	if (propertyType != typeof(uint))
																	{
																		if (propertyType != typeof(long))
																		{
																			if (propertyType != typeof(ulong))
																			{
																				if (propertyType != typeof(float))
																				{
																					if (propertyType != typeof(double))
																					{
																						if (propertyType != typeof(bool))
																						{
																							if (propertyType != typeof(string))
																							{
																								if (propertyType != typeof(char))
																								{
																									if (propertyType != typeof(DateTime))
																									{
																										if (propertyType != typeof(TimeSpan))
																										{
																											SchemaMapping.ThrowUnsupportedMember(memberInfo);
																										}
																										else
																										{
																											cimType = CimType.DateTime;
																										}
																									}
																									else
																									{
																										cimType = CimType.DateTime;
																									}
																								}
																								else
																								{
																									cimType = CimType.Char16;
																								}
																							}
																							else
																							{
																								cimType = CimType.String;
																							}
																						}
																						else
																						{
																							cimType = CimType.Boolean;
																						}
																					}
																					else
																					{
																						cimType = CimType.Real64;
																					}
																				}
																				else
																				{
																					cimType = CimType.Real32;
																				}
																			}
																			else
																			{
																				cimType = CimType.UInt64;
																			}
																		}
																		else
																		{
																			cimType = CimType.SInt64;
																		}
																	}
																	else
																	{
																		cimType = CimType.UInt32;
																	}
																}
																else
																{
																	cimType = CimType.SInt32;
																}
															}
															else
															{
																cimType = CimType.UInt16;
															}
														}
														else
														{
															cimType = CimType.SInt16;
														}
													}
													else
													{
														cimType = CimType.UInt8;
													}
												}
												else
												{
													cimType = CimType.SInt8;
												}
											}
											else
											{
												cimType = CimType.Object;
											}
										}
										else
										{
											cimType = CimType.Object;
										}
									}
									else
									{
										cimType = CimType.Object;
									}
									try
									{
										properties.Add(memberName, cimType, flag1);
									}
									catch (ManagementException managementException5)
									{
										ManagementException managementException4 = managementException5;
										SchemaMapping.ThrowUnsupportedMember(memberInfo, managementException4);
									}
									if (propertyType == typeof(TimeSpan))
									{
										item1 = properties[memberName];
										item1.Qualifiers.Add("SubType", "interval", false, true, true, true);
									}
									if (str != null)
									{
										propertyDatum1 = properties[memberName];
										propertyDatum1.Qualifiers["CIMTYPE"].Value = string.Concat("object:", str);
									}
								}
							}
						}
					}
					codeWriter15.Line("Marshal.Release(wbemObjectAccessIP);");
					return;
				}
				case InstrumentationType.Event:
				{
					num = 0;
					flag = false;
					members = type.GetMembers();
					for (i = 0; i < (int)members.Length; i++)
					{
						memberInfo = members[i];
						if ((memberInfo as FieldInfo != null || memberInfo as PropertyInfo != null) && (int)memberInfo.GetCustomAttributes(typeof(IgnoreMemberAttribute), false).Length <= 0)
						{
							if (memberInfo as FieldInfo == null)
							{
								if (memberInfo as PropertyInfo != null)
								{
									propertyInfo = memberInfo as PropertyInfo;
									if (!propertyInfo.CanRead)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									getMethod = propertyInfo.GetGetMethod();
									if (null == getMethod || getMethod.IsStatic)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									if ((int)getMethod.GetParameters().Length > 0)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
								}
							}
							else
							{
								fieldInfo = memberInfo as FieldInfo;
								if (fieldInfo.IsStatic)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
							}
							memberName = ManagedNameAttribute.GetMemberName(memberInfo);
							if (memberInfo as FieldInfo == null)
							{
								propertyType = (memberInfo as PropertyInfo).PropertyType;
							}
							else
							{
								propertyType = (memberInfo as FieldInfo).FieldType;
							}
							flag1 = false;
							if (propertyType.IsArray)
							{
								if (propertyType.GetArrayRank() != 1)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
								flag1 = true;
								propertyType = propertyType.GetElementType();
							}
							str = null;
							item = null;
							if (mapTypeToConverterClassName.Contains(propertyType))
							{
								item = (string)mapTypeToConverterClassName[propertyType];
								str = ManagedNameAttribute.GetMemberName(propertyType);
							}
							flag2 = false;
							if (propertyType == typeof(object))
							{
								flag2 = true;
								if (!flag4)
								{
									flag4 = true;
									codeWriter14.Line("static Hashtable mapTypeToConverter = new Hashtable();");
									foreach (DictionaryEntry dictionaryEntry2 in mapTypeToConverterClassName)
									{
										codeWriter15.Line(string.Format("mapTypeToConverter[typeof({0})] = typeof({1});", ((Type)dictionaryEntry2.Key).FullName.Replace('+', '.'), (string)dictionaryEntry2.Value));
									}
								}
							}
							str1 = string.Concat("prop_", num);
							num1 = num;
							num = num1 + 1;
							str2 = string.Concat("handle_", num1);
							codeWriter14.Line(string.Concat("static int ", str2, ";"));
							codeWriter15.Line(string.Format("IWOA.GetPropertyHandle_f27(27, wbemObjectAccessIP, \"{0}\", out cimType, out {1});", memberName, str2));
							codeWriter14.Line(string.Concat("PropertyData ", str1, ";"));
							codeWriter16.Line(string.Format("{0} = instance.Properties[\"{1}\"];", str1, memberName));
							if (!flag2)
							{
								if (str == null)
								{
									if (flag1)
									{
										if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
										{
											codeWriter18.AddChild(string.Format("if(null == instNET.{0})", memberInfo.Name)).Line(string.Format("{0}.Value = null;", str1));
											codeWriter18.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.WMITimeArrayToStringArray(instNET.{1});", str1, memberInfo.Name));
										}
										else
										{
											codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
										}
									}
									else
									{
										if (propertyType == typeof(byte) || propertyType == typeof(sbyte))
										{
											codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
											codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 1, ref instNET_{1});", str2, memberInfo.Name));
										}
										else
										{
											if (propertyType == typeof(short) || propertyType == typeof(ushort) || propertyType == typeof(char))
											{
												codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
												codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref instNET_{1});", str2, memberInfo.Name));
											}
											else
											{
												if (propertyType == typeof(uint) || propertyType == typeof(int) || propertyType == typeof(float))
												{
													codeWriter18.Line(string.Format("IWOA.WriteDWORD_f31(31, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
												}
												else
												{
													if (propertyType == typeof(ulong) || propertyType == typeof(long) || propertyType == typeof(double))
													{
														codeWriter18.Line(string.Format("IWOA.WriteQWORD_f33(33, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
													}
													else
													{
														if (propertyType != typeof(bool))
														{
															if (propertyType != typeof(string))
															{
																if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
																{
																	codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 52, SafeAssign.WMITimeToString(instNET.{1}));", str2, memberInfo.Name));
																}
																else
																{
																	codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
																}
															}
															else
															{
																codeWriter12 = codeWriter18.AddChild(string.Format("if(null != instNET.{0})", memberInfo.Name));
																codeWriter12.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, (instNET.{1}.Length+1)*2, instNET.{1});", str2, memberInfo.Name));
																codeWriter18.AddChild("else").Line(string.Format("IWOA.Put_f5(5, instWbemObjectAccessIP, \"{0}\", 0, ref nullObj, 8);", memberName));
																if (!flag)
																{
																	flag = true;
																	codeWriter14.Line("object nullObj = DBNull.Value;");
																}
															}
														}
														else
														{
															codeWriter18.Line(string.Format("if(instNET.{0})", memberInfo.Name));
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolTrue);", str2));
															codeWriter18.Line("else");
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolFalse);", str2));
														}
													}
												}
											}
										}
									}
								}
								else
								{
									if (!propertyType.IsValueType)
									{
										codeWriter5 = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
										codeWriter6 = codeWriter18.AddChild("else");
										codeWriter6.Line(string.Format("{0}.Value = null;", str1));
									}
									else
									{
										codeWriter5 = codeWriter18;
									}
									if (!flag1)
									{
										codeWriter14.Line(string.Format("{0} lazy_embeddedConverter_{1} = null;", item, str1));
										codeWriter9 = codeWriter13.AddChild(string.Format("{0} embeddedConverter_{1}", item, str1));
										codeWriter10 = codeWriter9.AddChild("get");
										codeWriter11 = codeWriter10.AddChild(string.Format("if(null == lazy_embeddedConverter_{0})", str1));
										codeWriter11.Line(string.Format("lazy_embeddedConverter_{0} = new {1}();", str1, item));
										codeWriter10.Line(string.Format("return lazy_embeddedConverter_{0};", str1));
										codeWriter5.Line(string.Format("embeddedConverter_{0}.ToWMI(instNET.{1});", str1, memberInfo.Name));
										codeWriter5.Line(string.Format("{0}.Value = embeddedConverter_{0}.instance;", str1));
									}
									else
									{
										codeWriter5.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
										codeWriter5.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
										codeWriter5.Line(string.Format("{0}[] embeddedConverters = new {0}[len];", item));
										codeWriter7 = codeWriter5.AddChild("for(int i=0;i<len;i++)");
										codeWriter7.Line(string.Format("embeddedConverters[i] = new {0}();", item));
										if (!propertyType.IsValueType)
										{
											codeWriter8 = codeWriter7.AddChild(string.Format("if(instNET.{0}[i] != null)", memberInfo.Name));
											codeWriter8.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										else
										{
											codeWriter7.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										codeWriter7.Line("embeddedObjects[i] = embeddedConverters[i].instance;");
										codeWriter5.Line(string.Format("{0}.Value = embeddedObjects;", str1));
									}
								}
							}
							else
							{
								codeWriter = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
								codeWriter1 = codeWriter18.AddChild("else");
								codeWriter1.Line(string.Format("{0}.Value = null;", str1));
								if (!flag1)
								{
									codeWriter4 = codeWriter.AddChild(string.Format("if(mapTypeToConverter.Contains(instNET.{0}.GetType()))", memberInfo.Name));
									codeWriter4.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}.GetType()];", memberInfo.Name));
									codeWriter4.Line("IWmiConverter converter = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter4.Line(string.Format("converter.ToWMI(instNET.{0});", memberInfo.Name));
									codeWriter4.Line(string.Format("{0}.Value = converter.GetInstance();", str1));
									codeWriter.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.GetInstance(instNET.{1});", str1, memberInfo.Name));
								}
								else
								{
									codeWriter.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
									codeWriter.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
									codeWriter.Line("IWmiConverter[] embeddedConverters = new IWmiConverter[len];");
									codeWriter2 = codeWriter.AddChild("for(int i=0;i<len;i++)");
									codeWriter3 = codeWriter2.AddChild(string.Format("if((instNET.{0}[i] != null) && mapTypeToConverter.Contains(instNET.{0}[i].GetType()))", memberInfo.Name));
									codeWriter3.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}[i].GetType()];", memberInfo.Name));
									codeWriter3.Line("embeddedConverters[i] = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter3.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
									codeWriter3.Line("embeddedObjects[i] = embeddedConverters[i].GetInstance();");
									codeWriter2.AddChild("else").Line(string.Format("embeddedObjects[i] = SafeAssign.GetManagementObject(instNET.{0}[i]);", memberInfo.Name));
									codeWriter.Line(string.Format("{0}.Value = embeddedObjects;", str1));
								}
							}
							cimType = CimType.String;
							if (memberInfo.DeclaringType == type)
							{
								flag3 = true;
								try
								{
									propertyDatum = this.newClass.Properties[memberName];
									CimType type1 = propertyDatum.Type;
									if (propertyDatum.IsLocal)
									{
										throw new ArgumentException(string.Format(RC.GetString("MEMBERCONFLILCT_EXCEPT"), memberInfo.Name), memberInfo.Name);
									}
								}
								catch (ManagementException managementException3)
								{
									ManagementException managementException2 = managementException3;
									if (managementException2.ErrorCode == ManagementStatus.NotFound)
									{
										flag3 = false;
									}
									else
									{
										throw;
									}
								}
								if (!flag3)
								{
									if (str == null)
									{
										if (!flag2)
										{
											if (propertyType != typeof(ManagementObject))
											{
												if (propertyType != typeof(sbyte))
												{
													if (propertyType != typeof(byte))
													{
														if (propertyType != typeof(short))
														{
															if (propertyType != typeof(ushort))
															{
																if (propertyType != typeof(int))
																{
																	if (propertyType != typeof(uint))
																	{
																		if (propertyType != typeof(long))
																		{
																			if (propertyType != typeof(ulong))
																			{
																				if (propertyType != typeof(float))
																				{
																					if (propertyType != typeof(double))
																					{
																						if (propertyType != typeof(bool))
																						{
																							if (propertyType != typeof(string))
																							{
																								if (propertyType != typeof(char))
																								{
																									if (propertyType != typeof(DateTime))
																									{
																										if (propertyType != typeof(TimeSpan))
																										{
																											SchemaMapping.ThrowUnsupportedMember(memberInfo);
																										}
																										else
																										{
																											cimType = CimType.DateTime;
																										}
																									}
																									else
																									{
																										cimType = CimType.DateTime;
																									}
																								}
																								else
																								{
																									cimType = CimType.Char16;
																								}
																							}
																							else
																							{
																								cimType = CimType.String;
																							}
																						}
																						else
																						{
																							cimType = CimType.Boolean;
																						}
																					}
																					else
																					{
																						cimType = CimType.Real64;
																					}
																				}
																				else
																				{
																					cimType = CimType.Real32;
																				}
																			}
																			else
																			{
																				cimType = CimType.UInt64;
																			}
																		}
																		else
																		{
																			cimType = CimType.SInt64;
																		}
																	}
																	else
																	{
																		cimType = CimType.UInt32;
																	}
																}
																else
																{
																	cimType = CimType.SInt32;
																}
															}
															else
															{
																cimType = CimType.UInt16;
															}
														}
														else
														{
															cimType = CimType.SInt16;
														}
													}
													else
													{
														cimType = CimType.UInt8;
													}
												}
												else
												{
													cimType = CimType.SInt8;
												}
											}
											else
											{
												cimType = CimType.Object;
											}
										}
										else
										{
											cimType = CimType.Object;
										}
									}
									else
									{
										cimType = CimType.Object;
									}
									try
									{
										properties.Add(memberName, cimType, flag1);
									}
									catch (ManagementException managementException5)
									{
										ManagementException managementException4 = managementException5;
										SchemaMapping.ThrowUnsupportedMember(memberInfo, managementException4);
									}
									if (propertyType == typeof(TimeSpan))
									{
										item1 = properties[memberName];
										item1.Qualifiers.Add("SubType", "interval", false, true, true, true);
									}
									if (str != null)
									{
										propertyDatum1 = properties[memberName];
										propertyDatum1.Qualifiers["CIMTYPE"].Value = string.Concat("object:", str);
									}
								}
							}
						}
					}
					codeWriter15.Line("Marshal.Release(wbemObjectAccessIP);");
					return;
				}
				case InstrumentationType.Abstract:
				{
					this.newClass.Qualifiers.Add("abstract", true, false, false, false, true);
					num = 0;
					flag = false;
					members = type.GetMembers();
					for (i = 0; i < (int)members.Length; i++)
					{
						memberInfo = members[i];
						if ((memberInfo as FieldInfo != null || memberInfo as PropertyInfo != null) && (int)memberInfo.GetCustomAttributes(typeof(IgnoreMemberAttribute), false).Length <= 0)
						{
							if (memberInfo as FieldInfo == null)
							{
								if (memberInfo as PropertyInfo != null)
								{
									propertyInfo = memberInfo as PropertyInfo;
									if (!propertyInfo.CanRead)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									getMethod = propertyInfo.GetGetMethod();
									if (null == getMethod || getMethod.IsStatic)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									if ((int)getMethod.GetParameters().Length > 0)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
								}
							}
							else
							{
								fieldInfo = memberInfo as FieldInfo;
								if (fieldInfo.IsStatic)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
							}
							memberName = ManagedNameAttribute.GetMemberName(memberInfo);
							if (memberInfo as FieldInfo == null)
							{
								propertyType = (memberInfo as PropertyInfo).PropertyType;
							}
							else
							{
								propertyType = (memberInfo as FieldInfo).FieldType;
							}
							flag1 = false;
							if (propertyType.IsArray)
							{
								if (propertyType.GetArrayRank() != 1)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
								flag1 = true;
								propertyType = propertyType.GetElementType();
							}
							str = null;
							item = null;
							if (mapTypeToConverterClassName.Contains(propertyType))
							{
								item = (string)mapTypeToConverterClassName[propertyType];
								str = ManagedNameAttribute.GetMemberName(propertyType);
							}
							flag2 = false;
							if (propertyType == typeof(object))
							{
								flag2 = true;
								if (!flag4)
								{
									flag4 = true;
									codeWriter14.Line("static Hashtable mapTypeToConverter = new Hashtable();");
									foreach (DictionaryEntry dictionaryEntry3 in mapTypeToConverterClassName)
									{
										codeWriter15.Line(string.Format("mapTypeToConverter[typeof({0})] = typeof({1});", ((Type)dictionaryEntry3.Key).FullName.Replace('+', '.'), (string)dictionaryEntry3.Value));
									}
								}
							}
							str1 = string.Concat("prop_", (object)num);
							num1 = num;
							num = num1 + 1;
							str2 = string.Concat("handle_", (object)num1);
							codeWriter14.Line(string.Concat("static int ", str2, ";"));
							codeWriter15.Line(string.Format("IWOA.GetPropertyHandle_f27(27, wbemObjectAccessIP, \"{0}\", out cimType, out {1});", memberName, str2));
							codeWriter14.Line(string.Concat("PropertyData ", str1, ";"));
							codeWriter16.Line(string.Format("{0} = instance.Properties[\"{1}\"];", str1, memberName));
							if (!flag2)
							{
								if (str == null)
								{
									if (flag1)
									{
										if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
										{
											codeWriter18.AddChild(string.Format("if(null == instNET.{0})", memberInfo.Name)).Line(string.Format("{0}.Value = null;", str1));
											codeWriter18.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.WMITimeArrayToStringArray(instNET.{1});", str1, memberInfo.Name));
										}
										else
										{
											codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
										}
									}
									else
									{
										if (propertyType == typeof(byte) || propertyType == typeof(sbyte))
										{
											codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
											codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 1, ref instNET_{1});", str2, memberInfo.Name));
										}
										else
										{
											if (propertyType == typeof(short) || propertyType == typeof(ushort) || propertyType == typeof(char))
											{
												codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
												codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref instNET_{1});", str2, memberInfo.Name));
											}
											else
											{
												if (propertyType == typeof(uint) || propertyType == typeof(int) || propertyType == typeof(float))
												{
													codeWriter18.Line(string.Format("IWOA.WriteDWORD_f31(31, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
												}
												else
												{
													if (propertyType == typeof(ulong) || propertyType == typeof(long) || propertyType == typeof(double))
													{
														codeWriter18.Line(string.Format("IWOA.WriteQWORD_f33(33, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
													}
													else
													{
														if (propertyType != typeof(bool))
														{
															if (propertyType != typeof(string))
															{
																if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
																{
																	codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 52, SafeAssign.WMITimeToString(instNET.{1}));", str2, memberInfo.Name));
																}
																else
																{
																	codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
																}
															}
															else
															{
																codeWriter12 = codeWriter18.AddChild(string.Format("if(null != instNET.{0})", memberInfo.Name));
																codeWriter12.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, (instNET.{1}.Length+1)*2, instNET.{1});", str2, memberInfo.Name));
																codeWriter18.AddChild("else").Line(string.Format("IWOA.Put_f5(5, instWbemObjectAccessIP, \"{0}\", 0, ref nullObj, 8);", memberName));
																if (!flag)
																{
																	flag = true;
																	codeWriter14.Line("object nullObj = DBNull.Value;");
																}
															}
														}
														else
														{
															codeWriter18.Line(string.Format("if(instNET.{0})", memberInfo.Name));
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolTrue);", str2));
															codeWriter18.Line("else");
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolFalse);", str2));
														}
													}
												}
											}
										}
									}
								}
								else
								{
									if (!propertyType.IsValueType)
									{
										codeWriter5 = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
										codeWriter6 = codeWriter18.AddChild("else");
										codeWriter6.Line(string.Format("{0}.Value = null;", str1));
									}
									else
									{
										codeWriter5 = codeWriter18;
									}
									if (!flag1)
									{
										codeWriter14.Line(string.Format("{0} lazy_embeddedConverter_{1} = null;", item, str1));
										codeWriter9 = codeWriter13.AddChild(string.Format("{0} embeddedConverter_{1}", item, str1));
										codeWriter10 = codeWriter9.AddChild("get");
										codeWriter11 = codeWriter10.AddChild(string.Format("if(null == lazy_embeddedConverter_{0})", str1));
										codeWriter11.Line(string.Format("lazy_embeddedConverter_{0} = new {1}();", str1, item));
										codeWriter10.Line(string.Format("return lazy_embeddedConverter_{0};", str1));
										codeWriter5.Line(string.Format("embeddedConverter_{0}.ToWMI(instNET.{1});", str1, memberInfo.Name));
										codeWriter5.Line(string.Format("{0}.Value = embeddedConverter_{0}.instance;", str1));
									}
									else
									{
										codeWriter5.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
										codeWriter5.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
										codeWriter5.Line(string.Format("{0}[] embeddedConverters = new {0}[len];", item));
										codeWriter7 = codeWriter5.AddChild("for(int i=0;i<len;i++)");
										codeWriter7.Line(string.Format("embeddedConverters[i] = new {0}();", item));
										if (!propertyType.IsValueType)
										{
											codeWriter8 = codeWriter7.AddChild(string.Format("if(instNET.{0}[i] != null)", memberInfo.Name));
											codeWriter8.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										else
										{
											codeWriter7.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										codeWriter7.Line("embeddedObjects[i] = embeddedConverters[i].instance;");
										codeWriter5.Line(string.Format("{0}.Value = embeddedObjects;", str1));
									}
								}
							}
							else
							{
								codeWriter = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
								codeWriter1 = codeWriter18.AddChild("else");
								codeWriter1.Line(string.Format("{0}.Value = null;", str1));
								if (!flag1)
								{
									codeWriter4 = codeWriter.AddChild(string.Format("if(mapTypeToConverter.Contains(instNET.{0}.GetType()))", memberInfo.Name));
									codeWriter4.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}.GetType()];", memberInfo.Name));
									codeWriter4.Line("IWmiConverter converter = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter4.Line(string.Format("converter.ToWMI(instNET.{0});", memberInfo.Name));
									codeWriter4.Line(string.Format("{0}.Value = converter.GetInstance();", str1));
									codeWriter.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.GetInstance(instNET.{1});", str1, memberInfo.Name));
								}
								else
								{
									codeWriter.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
									codeWriter.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
									codeWriter.Line("IWmiConverter[] embeddedConverters = new IWmiConverter[len];");
									codeWriter2 = codeWriter.AddChild("for(int i=0;i<len;i++)");
									codeWriter3 = codeWriter2.AddChild(string.Format("if((instNET.{0}[i] != null) && mapTypeToConverter.Contains(instNET.{0}[i].GetType()))", memberInfo.Name));
									codeWriter3.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}[i].GetType()];", memberInfo.Name));
									codeWriter3.Line("embeddedConverters[i] = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter3.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
									codeWriter3.Line("embeddedObjects[i] = embeddedConverters[i].GetInstance();");
									codeWriter2.AddChild("else").Line(string.Format("embeddedObjects[i] = SafeAssign.GetManagementObject(instNET.{0}[i]);", memberInfo.Name));
									codeWriter.Line(string.Format("{0}.Value = embeddedObjects;", str1));
								}
							}
							cimType = CimType.String;
							if (memberInfo.DeclaringType == type)
							{
								flag3 = true;
								try
								{
									propertyDatum = this.newClass.Properties[memberName];
									CimType type1 = propertyDatum.Type;
									if (propertyDatum.IsLocal)
									{
										throw new ArgumentException(string.Format(RC.GetString("MEMBERCONFLILCT_EXCEPT"), memberInfo.Name), memberInfo.Name);
									}
								}
								catch (ManagementException managementException3)
								{
									ManagementException managementException2 = managementException3;
									if (managementException2.ErrorCode == ManagementStatus.NotFound)
									{
										flag3 = false;
									}
									else
									{
										throw;
									}
								}
								if (!flag3)
								{
									if (str == null)
									{
										if (!flag2)
										{
											if (propertyType != typeof(ManagementObject))
											{
												if (propertyType != typeof(sbyte))
												{
													if (propertyType != typeof(byte))
													{
														if (propertyType != typeof(short))
														{
															if (propertyType != typeof(ushort))
															{
																if (propertyType != typeof(int))
																{
																	if (propertyType != typeof(uint))
																	{
																		if (propertyType != typeof(long))
																		{
																			if (propertyType != typeof(ulong))
																			{
																				if (propertyType != typeof(float))
																				{
																					if (propertyType != typeof(double))
																					{
																						if (propertyType != typeof(bool))
																						{
																							if (propertyType != typeof(string))
																							{
																								if (propertyType != typeof(char))
																								{
																									if (propertyType != typeof(DateTime))
																									{
																										if (propertyType != typeof(TimeSpan))
																										{
																											SchemaMapping.ThrowUnsupportedMember(memberInfo);
																										}
																										else
																										{
																											cimType = CimType.DateTime;
																										}
																									}
																									else
																									{
																										cimType = CimType.DateTime;
																									}
																								}
																								else
																								{
																									cimType = CimType.Char16;
																								}
																							}
																							else
																							{
																								cimType = CimType.String;
																							}
																						}
																						else
																						{
																							cimType = CimType.Boolean;
																						}
																					}
																					else
																					{
																						cimType = CimType.Real64;
																					}
																				}
																				else
																				{
																					cimType = CimType.Real32;
																				}
																			}
																			else
																			{
																				cimType = CimType.UInt64;
																			}
																		}
																		else
																		{
																			cimType = CimType.SInt64;
																		}
																	}
																	else
																	{
																		cimType = CimType.UInt32;
																	}
																}
																else
																{
																	cimType = CimType.SInt32;
																}
															}
															else
															{
																cimType = CimType.UInt16;
															}
														}
														else
														{
															cimType = CimType.SInt16;
														}
													}
													else
													{
														cimType = CimType.UInt8;
													}
												}
												else
												{
													cimType = CimType.SInt8;
												}
											}
											else
											{
												cimType = CimType.Object;
											}
										}
										else
										{
											cimType = CimType.Object;
										}
									}
									else
									{
										cimType = CimType.Object;
									}
									try
									{
										properties.Add(memberName, cimType, flag1);
									}
									catch (ManagementException managementException5)
									{
										ManagementException managementException4 = managementException5;
										SchemaMapping.ThrowUnsupportedMember(memberInfo, managementException4);
									}
									if (propertyType == typeof(TimeSpan))
									{
										item1 = properties[memberName];
										item1.Qualifiers.Add("SubType", "interval", false, true, true, true);
									}
									if (str != null)
									{
										propertyDatum1 = properties[memberName];
										propertyDatum1.Qualifiers["CIMTYPE"].Value = string.Concat("object:", str);
									}
								}
							}
						}
					}
					codeWriter15.Line("Marshal.Release(wbemObjectAccessIP);");
					return;
				}
				default:
				{
					num = 0;
					flag = false;
					members = type.GetMembers();
					for (i = 0; i < (int)members.Length; i++)
					{
						memberInfo = members[i];
						if ((memberInfo as FieldInfo != null || memberInfo as PropertyInfo != null) && (int)memberInfo.GetCustomAttributes(typeof(IgnoreMemberAttribute), false).Length <= 0)
						{
							if (memberInfo as FieldInfo == null)
							{
								if (memberInfo as PropertyInfo != null)
								{
									propertyInfo = memberInfo as PropertyInfo;
									if (!propertyInfo.CanRead)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									getMethod = propertyInfo.GetGetMethod();
									if (null == getMethod || getMethod.IsStatic)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
									if ((int)getMethod.GetParameters().Length > 0)
									{
										SchemaMapping.ThrowUnsupportedMember(memberInfo);
									}
								}
							}
							else
							{
								fieldInfo = memberInfo as FieldInfo;
								if (fieldInfo.IsStatic)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
							}
							memberName = ManagedNameAttribute.GetMemberName(memberInfo);
							if (memberInfo as FieldInfo == null)
							{
								propertyType = (memberInfo as PropertyInfo).PropertyType;
							}
							else
							{
								propertyType = (memberInfo as FieldInfo).FieldType;
							}
							flag1 = false;
							if (propertyType.IsArray)
							{
								if (propertyType.GetArrayRank() != 1)
								{
									SchemaMapping.ThrowUnsupportedMember(memberInfo);
								}
								flag1 = true;
								propertyType = propertyType.GetElementType();
							}
							str = null;
							item = null;
							if (mapTypeToConverterClassName.Contains(propertyType))
							{
								item = (string)mapTypeToConverterClassName[propertyType];
								str = ManagedNameAttribute.GetMemberName(propertyType);
							}
							flag2 = false;
							if (propertyType == typeof(object))
							{
								flag2 = true;
								if (!flag4)
								{
									flag4 = true;
									codeWriter14.Line("static Hashtable mapTypeToConverter = new Hashtable();");
									foreach (DictionaryEntry dictionaryEntry4 in mapTypeToConverterClassName)
									{
										codeWriter15.Line(string.Format("mapTypeToConverter[typeof({0})] = typeof({1});", ((Type)dictionaryEntry4.Key).FullName.Replace('+', '.'), (string)dictionaryEntry4.Value));
									}
								}
							}
							str1 = string.Concat("prop_", (object)num);
							num1 = num;
							num = num1 + 1;
							str2 = string.Concat("handle_", (object)num1);
							codeWriter14.Line(string.Concat("static int ", str2, ";"));
							codeWriter15.Line(string.Format("IWOA.GetPropertyHandle_f27(27, wbemObjectAccessIP, \"{0}\", out cimType, out {1});", memberName, str2));
							codeWriter14.Line(string.Concat("PropertyData ", str1, ";"));
							codeWriter16.Line(string.Format("{0} = instance.Properties[\"{1}\"];", str1, memberName));
							if (!flag2)
							{
								if (str == null)
								{
									if (flag1)
									{
										if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
										{
											codeWriter18.AddChild(string.Format("if(null == instNET.{0})", memberInfo.Name)).Line(string.Format("{0}.Value = null;", str1));
											codeWriter18.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.WMITimeArrayToStringArray(instNET.{1});", str1, memberInfo.Name));
										}
										else
										{
											codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
										}
									}
									else
									{
										if (propertyType == typeof(byte) || propertyType == typeof(sbyte))
										{
											codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
											codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 1, ref instNET_{1});", str2, memberInfo.Name));
										}
										else
										{
											if (propertyType == typeof(short) || propertyType == typeof(ushort) || propertyType == typeof(char))
											{
												codeWriter18.Line(string.Format("{0} instNET_{1} = instNET.{1} ;", propertyType, memberInfo.Name));
												codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref instNET_{1});", str2, memberInfo.Name));
											}
											else
											{
												if (propertyType == typeof(uint) || propertyType == typeof(int) || propertyType == typeof(float))
												{
													codeWriter18.Line(string.Format("IWOA.WriteDWORD_f31(31, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
												}
												else
												{
													if (propertyType == typeof(ulong) || propertyType == typeof(long) || propertyType == typeof(double))
													{
														codeWriter18.Line(string.Format("IWOA.WriteQWORD_f33(33, instWbemObjectAccessIP, {0}, instNET.{1});", str2, memberInfo.Name));
													}
													else
													{
														if (propertyType != typeof(bool))
														{
															if (propertyType != typeof(string))
															{
																if (propertyType == typeof(DateTime) || propertyType == typeof(TimeSpan))
																{
																	codeWriter18.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 52, SafeAssign.WMITimeToString(instNET.{1}));", str2, memberInfo.Name));
																}
																else
																{
																	codeWriter18.Line(string.Format("{0}.Value = instNET.{1};", str1, memberInfo.Name));
																}
															}
															else
															{
																codeWriter12 = codeWriter18.AddChild(string.Format("if(null != instNET.{0})", memberInfo.Name));
																codeWriter12.Line(string.Format("IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, (instNET.{1}.Length+1)*2, instNET.{1});", str2, memberInfo.Name));
																codeWriter18.AddChild("else").Line(string.Format("IWOA.Put_f5(5, instWbemObjectAccessIP, \"{0}\", 0, ref nullObj, 8);", memberName));
																if (!flag)
																{
																	flag = true;
																	codeWriter14.Line("object nullObj = DBNull.Value;");
																}
															}
														}
														else
														{
															codeWriter18.Line(string.Format("if(instNET.{0})", memberInfo.Name));
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolTrue);", str2));
															codeWriter18.Line("else");
															codeWriter18.Line(string.Format("    IWOA.WritePropertyValue_f28(28, instWbemObjectAccessIP, {0}, 2, ref SafeAssign.boolFalse);", str2));
														}
													}
												}
											}
										}
									}
								}
								else
								{
									if (!propertyType.IsValueType)
									{
										codeWriter5 = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
										codeWriter6 = codeWriter18.AddChild("else");
										codeWriter6.Line(string.Format("{0}.Value = null;", str1));
									}
									else
									{
										codeWriter5 = codeWriter18;
									}
									if (!flag1)
									{
										codeWriter14.Line(string.Format("{0} lazy_embeddedConverter_{1} = null;", item, str1));
										codeWriter9 = codeWriter13.AddChild(string.Format("{0} embeddedConverter_{1}", item, str1));
										codeWriter10 = codeWriter9.AddChild("get");
										codeWriter11 = codeWriter10.AddChild(string.Format("if(null == lazy_embeddedConverter_{0})", str1));
										codeWriter11.Line(string.Format("lazy_embeddedConverter_{0} = new {1}();", str1, item));
										codeWriter10.Line(string.Format("return lazy_embeddedConverter_{0};", str1));
										codeWriter5.Line(string.Format("embeddedConverter_{0}.ToWMI(instNET.{1});", str1, memberInfo.Name));
										codeWriter5.Line(string.Format("{0}.Value = embeddedConverter_{0}.instance;", str1));
									}
									else
									{
										codeWriter5.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
										codeWriter5.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
										codeWriter5.Line(string.Format("{0}[] embeddedConverters = new {0}[len];", item));
										codeWriter7 = codeWriter5.AddChild("for(int i=0;i<len;i++)");
										codeWriter7.Line(string.Format("embeddedConverters[i] = new {0}();", item));
										if (!propertyType.IsValueType)
										{
											codeWriter8 = codeWriter7.AddChild(string.Format("if(instNET.{0}[i] != null)", memberInfo.Name));
											codeWriter8.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										else
										{
											codeWriter7.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
										}
										codeWriter7.Line("embeddedObjects[i] = embeddedConverters[i].instance;");
										codeWriter5.Line(string.Format("{0}.Value = embeddedObjects;", str1));
									}
								}
							}
							else
							{
								codeWriter = codeWriter18.AddChild(string.Format("if(instNET.{0} != null)", memberInfo.Name));
								codeWriter1 = codeWriter18.AddChild("else");
								codeWriter1.Line(string.Format("{0}.Value = null;", str1));
								if (!flag1)
								{
									codeWriter4 = codeWriter.AddChild(string.Format("if(mapTypeToConverter.Contains(instNET.{0}.GetType()))", memberInfo.Name));
									codeWriter4.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}.GetType()];", memberInfo.Name));
									codeWriter4.Line("IWmiConverter converter = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter4.Line(string.Format("converter.ToWMI(instNET.{0});", memberInfo.Name));
									codeWriter4.Line(string.Format("{0}.Value = converter.GetInstance();", str1));
									codeWriter.AddChild("else").Line(string.Format("{0}.Value = SafeAssign.GetInstance(instNET.{1});", str1, memberInfo.Name));
								}
								else
								{
									codeWriter.Line(string.Format("int len = instNET.{0}.Length;", memberInfo.Name));
									codeWriter.Line("ManagementObject[] embeddedObjects = new ManagementObject[len];");
									codeWriter.Line("IWmiConverter[] embeddedConverters = new IWmiConverter[len];");
									codeWriter2 = codeWriter.AddChild("for(int i=0;i<len;i++)");
									codeWriter3 = codeWriter2.AddChild(string.Format("if((instNET.{0}[i] != null) && mapTypeToConverter.Contains(instNET.{0}[i].GetType()))", memberInfo.Name));
									codeWriter3.Line(string.Format("Type type = (Type)mapTypeToConverter[instNET.{0}[i].GetType()];", memberInfo.Name));
									codeWriter3.Line("embeddedConverters[i] = (IWmiConverter)Activator.CreateInstance(type);");
									codeWriter3.Line(string.Format("embeddedConverters[i].ToWMI(instNET.{0}[i]);", memberInfo.Name));
									codeWriter3.Line("embeddedObjects[i] = embeddedConverters[i].GetInstance();");
									codeWriter2.AddChild("else").Line(string.Format("embeddedObjects[i] = SafeAssign.GetManagementObject(instNET.{0}[i]);", memberInfo.Name));
									codeWriter.Line(string.Format("{0}.Value = embeddedObjects;", str1));
								}
							}
							cimType = CimType.String;
							if (memberInfo.DeclaringType == type)
							{
								flag3 = true;
								try
								{
									propertyDatum = this.newClass.Properties[memberName];
									CimType type1 = propertyDatum.Type;
									if (propertyDatum.IsLocal)
									{
										throw new ArgumentException(string.Format(RC.GetString("MEMBERCONFLILCT_EXCEPT"), memberInfo.Name), memberInfo.Name);
									}
								}
								catch (ManagementException managementException3)
								{
									ManagementException managementException2 = managementException3;
									if (managementException2.ErrorCode == ManagementStatus.NotFound)
									{
										flag3 = false;
									}
									else
									{
										throw;
									}
								}
								if (!flag3)
								{
									if (str == null)
									{
										if (!flag2)
										{
											if (propertyType != typeof(ManagementObject))
											{
												if (propertyType != typeof(sbyte))
												{
													if (propertyType != typeof(byte))
													{
														if (propertyType != typeof(short))
														{
															if (propertyType != typeof(ushort))
															{
																if (propertyType != typeof(int))
																{
																	if (propertyType != typeof(uint))
																	{
																		if (propertyType != typeof(long))
																		{
																			if (propertyType != typeof(ulong))
																			{
																				if (propertyType != typeof(float))
																				{
																					if (propertyType != typeof(double))
																					{
																						if (propertyType != typeof(bool))
																						{
																							if (propertyType != typeof(string))
																							{
																								if (propertyType != typeof(char))
																								{
																									if (propertyType != typeof(DateTime))
																									{
																										if (propertyType != typeof(TimeSpan))
																										{
																											SchemaMapping.ThrowUnsupportedMember(memberInfo);
																										}
																										else
																										{
																											cimType = CimType.DateTime;
																										}
																									}
																									else
																									{
																										cimType = CimType.DateTime;
																									}
																								}
																								else
																								{
																									cimType = CimType.Char16;
																								}
																							}
																							else
																							{
																								cimType = CimType.String;
																							}
																						}
																						else
																						{
																							cimType = CimType.Boolean;
																						}
																					}
																					else
																					{
																						cimType = CimType.Real64;
																					}
																				}
																				else
																				{
																					cimType = CimType.Real32;
																				}
																			}
																			else
																			{
																				cimType = CimType.UInt64;
																			}
																		}
																		else
																		{
																			cimType = CimType.SInt64;
																		}
																	}
																	else
																	{
																		cimType = CimType.UInt32;
																	}
																}
																else
																{
																	cimType = CimType.SInt32;
																}
															}
															else
															{
																cimType = CimType.UInt16;
															}
														}
														else
														{
															cimType = CimType.SInt16;
														}
													}
													else
													{
														cimType = CimType.UInt8;
													}
												}
												else
												{
													cimType = CimType.SInt8;
												}
											}
											else
											{
												cimType = CimType.Object;
											}
										}
										else
										{
											cimType = CimType.Object;
										}
									}
									else
									{
										cimType = CimType.Object;
									}
									try
									{
										properties.Add(memberName, cimType, flag1);
									}
									catch (ManagementException managementException5)
									{
										ManagementException managementException4 = managementException5;
										SchemaMapping.ThrowUnsupportedMember(memberInfo, managementException4);
									}
									if (propertyType == typeof(TimeSpan))
									{
										item1 = properties[memberName];
										item1.Qualifiers.Add("SubType", "interval", false, true, true, true);
									}
									if (str != null)
									{
										propertyDatum1 = properties[memberName];
										propertyDatum1.Qualifiers["CIMTYPE"].Value = string.Concat("object:", str);
									}
								}
							}
						}
					}
					codeWriter15.Line("Marshal.Release(wbemObjectAccessIP);");
					return;
				}
			}
		}

		public static void ThrowUnsupportedMember(MemberInfo mi)
		{
			SchemaMapping.ThrowUnsupportedMember(mi, null);
		}

		public static void ThrowUnsupportedMember(MemberInfo mi, Exception innerException)
		{
			throw new ArgumentException(string.Format(RC.GetString("UNSUPPORTEDMEMBER_EXCEPT"), mi.Name), mi.Name, innerException);
		}
	}
}