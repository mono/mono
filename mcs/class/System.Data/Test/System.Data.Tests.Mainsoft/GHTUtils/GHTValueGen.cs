// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.Reflection;
using System.Collections;
using System.Data;

//this class is used at the WebServices Test Harness
namespace GHTUtils
{

	public class ValueGen
	{
		private const int ARRAY_SIZE = 7;

		public static object GHTTypeGenerator( Type t )
		{
			object result = null;
			if ( t.Name == "XmlNode" || t.Name == "XmlElement" || t.Name == "DataSet" || t.Name == "DataTable" )
			{
				result = GetRandomValue( t );
				return result;
			}
			//====================================================================================
			// Primitive
			//====================================================================================
			else if ( isPrimitive( t ) )
			{
				result = GetRandomValue( t );
				return result;
			}
			//====================================================================================
			// Array
			//====================================================================================
			else if (typeof(Array).IsAssignableFrom(t) ) 
				//The Array class returns false because it is not an array.
				//To check for an array, use code such as typeof(Array).IsAssignableFrom(type).
			{
				result = GenerateArray(t);
				return result;
			}
			//====================================================================================
			// Collection
			//====================================================================================
			else if ( isCollection( t ) ) 
			{
				result = GenerateCollection(t);
				return result;
			}
			//====================================================================================
			// User Defined Type
			//====================================================================================
			else
			{
					result = Activator_CreateInstance( t );
					result = Generate ( result );
			}
			return result;
		}

		public static object GHTObjModifier( object obj )
				{
			if ( isPrimitive( obj.GetType() ) )
			{
				return GetModifiedValue( obj );
			}
			else if ( obj.GetType().IsArray ) 
			{
				Type ElementType;
				//get the type of the elements in the array.
				//work around of GH behavior for array of enums : will give type enum    (Array)obj).GetValue(0).GetType()
				//                                              : will give type Int32   obj.GetType().GetElementType()
				if ( ((Array)obj).Length > 0)
					ElementType = ((Array)obj).GetValue(0).GetType();
				else
                    ElementType = obj.GetType().GetElementType();

				if ( isPrimitive(ElementType) )
				{
					Array arr = (Array)obj;
					for (int i=0; i < arr.Length; i++)
					{
						arr.SetValue(GetModifiedValue( arr.GetValue( i ) ), i );
					}
					return arr;
				}
				else
				{
					Array arr = (Array)obj;
					for ( int i=0; i < arr.Length; i++ )
					{
						object new_obj  = arr.GetValue( i );
						new_obj = GHTObjModifier( new_obj );
						arr.SetValue( new_obj, i );
					}
					return arr;
				}
			}
			else if ( obj.GetType().IsEnum ) 
			{
				Array a = Enum.GetValues(obj.GetType());
				if (a.Length >= 2)
					return a.GetValue(a.Length-2);
				else
					return a.GetValue(a.Length-1); //leave the same value
			}
			else if (obj.GetType().Name == "DataTable")
			{
				ModifyDataTable((System.Data.DataTable)obj);
				return obj;
			}
			else if (obj.GetType().Name == "DataSet")
			{
				ModifyDataSet((System.Data.DataSet)obj);
				return obj;
			}
			else if (obj.GetType().Name == "XmlNode" || obj.GetType().Name == "XmlElement")
			{
				ModifyXmlElement((System.Xml.XmlElement)obj);
				return obj;
			}
			else if (isCollection(obj.GetType()))
			{
				ModifyCollection(obj);
				return obj;
			}
			else
			{
				object result = obj;
				Modify( result );
				return result;
			}
				}
	

		static object GetRandomValue(Type t)
		{
			object objOut =	null;
			string str = null;
			System.Threading.Thread.Sleep(10);
			System.Random rnd =	new	System.Random(unchecked((int)DateTime.Now.Ticks));

			if (t.FullName ==	"System.Boolean")
			{
				objOut = System.Convert.ToBoolean(rnd.Next(0, 1));
				return objOut;
			}
			else if	(t.FullName == "System.Byte")
			{
				objOut = System.Convert.ToByte(rnd.Next(System.Byte.MinValue+1, System.Byte.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.Char")
			{
				objOut = System.Convert.ToChar(rnd.Next(System.Char.MinValue+65, System.Char.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.DateTime")
			{
				//GH precision is only milliseconds
				objOut = System.Convert.ToDateTime(new System.DateTime(632083133257660000));
				return objOut;
			}
			else if	(t.FullName == "System.Decimal")
			{
				objOut = System.Convert.ToDecimal(rnd.Next(System.Int16.MinValue+1, System.Int16.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.Double")
			{
				//give max length of "MaxLength" digits
				int MaxLength = 2;
				str = rnd.NextDouble().ToString();
				if (str.Length > MaxLength) str = str.Remove(MaxLength+1,str.Length-(MaxLength+1));
				objOut = System.Convert.ToDouble(str);
				return objOut;
			}
			else if	(t.FullName == "System.Int16")
			{
				objOut = System.Convert.ToInt16(rnd.Next(System.Int16.MinValue+1,System.Int16.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.Int32")
			{
				objOut = System.Convert.ToInt32(rnd.Next(System.Int16.MinValue+1,System.Int16.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.Int64")
			{
				objOut = System.Convert.ToInt64(rnd.Next(System.Int16.MinValue+1,System.Int16.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.SByte")
			{
				objOut = System.Convert.ToSByte(rnd.Next(System.SByte.MinValue+1,System.SByte.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.Single")
			{
				objOut = System.Convert.ToSingle(rnd.Next(System.Int16.MinValue+1, System.Int16.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.String")
			{
				long size = DateTime.Now.Ticks;
				size = size % 99;
				if (size==0) size = 16;
				for	(int i=0; i<size ;i++)
				{
					str	+= System.Convert.ToChar(rnd.Next(System.Byte.MinValue+65, System.Byte.MaxValue-128));
				}
				objOut = str;
				return objOut;
			}
			else if	(t.FullName == "System.UInt16")
			{
				objOut = System.Convert.ToUInt16(rnd.Next(System.UInt16.MinValue+1,System.UInt16.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.UInt32")
			{
				objOut = System.Convert.ToUInt32(rnd.Next((int)System.UInt32.MinValue+1,System.Int32.MaxValue-128));
				return objOut;
			}
			else if	(t.FullName == "System.UInt64")
			{
				objOut = System.Convert.ToUInt64(rnd.Next((int)System.UInt64.MinValue+1,System.Int32.MaxValue-128));
				return objOut;
			}				 
			else if	(t.FullName == "System.Data.DataTable")
			{
				objOut = GenerateDataTable();
				return objOut;
			}				 
			else if	(t.FullName == "System.Data.DataSet")
			{
				objOut = GenerateDataSet();
				return objOut;
			}				   
			else if	(t.FullName == "System.Xml.XmlNode" || t.FullName == "System.Xml.XmlElement")
			{
				System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
				objOut = xmlDoc.CreateElement("myElement");
				((System.Xml.XmlElement)objOut).InnerText = "1234";    
//				((System.Xml.XmlElement)objOut).InnerXml = "<books>" +   
//					"<book>" + 
//					"<author>Carson</author>" + 
//					"<price format=\"dollar\">31.95</price>" + 
//					"<pubdate>05/01/2001</pubdate>" + 
//					"</book>" + 
//					"<pubinfo>" + 
//					"<publisher>MSPress</publisher>" + 
//					"<state>WA</state>" + 
//					"</pubinfo>" + 
//					"</books>";
				return objOut;
			}
			else
			{
				throw new System.NotImplementedException("GetRandomValue error: Type " + t.Name	+ "	not	implemented.");
			}
		}

		static object GetModifiedValue(object objIn)
		{
			object objOut =	null;

			if (objIn.GetType().FullName ==	"System.Boolean")
			{
				bool BoolVar =!(bool)objIn;
				return BoolVar  ;
			}
			else if	(objIn.GetType().FullName == "System.Byte")
			{
				if ((byte)objIn == byte.MaxValue) 
					return (byte)1;				
				else
					return System.Convert.ToByte((byte)objIn + (byte)1);
			}
			else if	(objIn.GetType().FullName == "System.Char")
			{
				if ((char)objIn == char.MaxValue)
					return (char)1;
				else
					return System.Convert.ToChar((char)objIn + (char)1);
			}
			else if	(objIn.GetType().FullName == "System.DateTime")
			{
				objOut = System.Convert.ToDateTime(objIn);
				objOut = ((DateTime)objOut).AddHours(1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.Decimal")
			{
				if ((decimal)objIn == decimal.MaxValue)
					objOut = (decimal)1;
				else 
					objOut = System.Convert.ToDecimal(System.Convert.ToDecimal(objIn) + (decimal)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.Double")
			{
				if ((double)objIn == double.MaxValue)
					objOut = (double)1;
				else 
					objOut = System.Convert.ToDouble(System.Convert.ToDouble(objIn) + (double)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.Int16")
			{
				if ((Int16)objIn == Int16.MaxValue)
					objOut = (Int16)1;
				else 
					objOut = System.Convert.ToInt16(System.Convert.ToInt16(objIn) + (Int16)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.Int32")
			{
				if ((Int32)objIn == Int32.MaxValue)
					objOut = (Int32)1;
				else 
					objOut = System.Convert.ToInt32(System.Convert.ToInt32(objIn) + (Int32)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.Int64")
			{
				if ((Int64)objIn == Int64.MaxValue)
					objOut = (Int64)1;
				else 
					objOut = System.Convert.ToInt64(System.Convert.ToInt64(objIn) + (Int64)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.SByte")
			{
				if ((SByte)objIn == SByte.MaxValue)
					objOut = (SByte)1;
				else 
					objOut = System.Convert.ToSByte(System.Convert.ToSByte(objIn) + (SByte)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.Single")
			{
				if ((Single)objIn == Single.MaxValue)
					objOut = (Single)1;
				else 
					objOut = System.Convert.ToSingle(System.Convert.ToSingle(objIn) + (Single)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.String")
			{
				string strin;
				strin = System.Convert.ToString(System.Convert.ToString(objIn));
				objOut = System.Convert.ToString("");
				for (int ii=0; ii < strin.Length; ii++)
					if ( strin[ii] > 'Z' )
						objOut += strin[ii].ToString().ToUpper();
					else	
						objOut += strin[ii].ToString().ToLower();
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.UInt16")
			{
				if ((UInt16)objIn == UInt16.MaxValue)
					objOut = (UInt16)1;
				else 
					objOut = System.Convert.ToUInt16(System.Convert.ToUInt16(objIn) + (UInt16)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.UInt32")
			{
				if ((UInt32)objIn == UInt32.MaxValue)
					objOut = (UInt32)1;
				else
					objOut = System.Convert.ToUInt32(System.Convert.ToUInt32(objIn) + (UInt32)1);
				return objOut;
			}
			else if	(objIn.GetType().FullName == "System.UInt64")
			{
				if ((UInt64)objIn == UInt64.MaxValue)
					objOut = (UInt64)1;
				else
					objOut = System.Convert.ToUInt64(System.Convert.ToUInt64(objIn) + (UInt64)1);
				return objOut;
			}				 
			else
			{
				throw new System.NotImplementedException("GetModifiedValue error: Type " + objIn.GetType().FullName + "	not	implemented.");
			}
		}
		


		static void Modify( object obj )
		{
			if ( obj.GetType().IsEnum ) 
			{
				Array a = Enum.GetValues(obj.GetType());
				if (a.Length >= 2)
					obj = a.GetValue(a.Length-2);
				else
					obj = a.GetValue(a.Length-1); //leave the same value
				return;
			}

			MemberInfo [] mic;

			mic = obj.GetType().GetMembers( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic );
			foreach ( MemberInfo mi in mic )
			{
				// ---------- FieldInfo ---------- 
				if ( mi is FieldInfo )
				{
					FieldInfo field = (FieldInfo)mi;
					Type fieldType = field.FieldType;
                    					

					if ( fieldType.IsArray )
					{
						// is array of primitive
						if ( isPrimitive( fieldType.GetElementType() ) )
						{
							Array arr = (Array)field.GetValue( obj );
							for (int i=0; i < arr.Length; i++)
							{
								arr.SetValue(GetModifiedValue( arr.GetValue( i ) ), i );
							}
							field.SetValue( obj, arr );
						}
						
						// is array of user defined type
						if ( !isSystem( fieldType ) &&  !isCollection( fieldType ) )
						{
							Array arr = (Array)field.GetValue( obj );
							for ( int i=0; i < arr.Length; i++ )
							{
								object new_obj  = arr.GetValue( i );
								Modify( new_obj );
								arr.SetValue( new_obj, i );
							}
							field.SetValue( obj, arr );
						}

					}
					else
					{
						if ( isPrimitive( fieldType ) )
						{
							field.SetValue( obj, GetModifiedValue( field.GetValue( obj ) ) );
						}
						if ( !isSystem( fieldType ) &&  !isCollection( fieldType ) )
						{
							object new_obj = field.GetValue( obj );
							Modify(new_obj);
							field.SetValue( obj, new_obj );
						}
						// object
						if ( isObject( fieldType ) )
						{
							object new_obj = field.GetValue( obj );
							Modify(new_obj);
							field.SetValue( obj, new_obj );
						} 
					}
				} // field info


				// ---------- PropertyInfo ---------- 
				//
				if ( mi is PropertyInfo )
				{
					PropertyInfo prop = (PropertyInfo)mi;

					if ( prop.PropertyType.IsArray )
					{
						// is array of primitive type member
						if ( isPrimitive( prop.PropertyType.GetElementType() ) )
						{
							Array arr = (Array)prop.GetValue( obj, null );
							for (int i=0; i < arr.Length; i++)
							{
								arr.SetValue(GetModifiedValue( arr.GetValue( i ) ), i );
							}
							prop.SetValue( obj, arr, null );
						}

						//is array user defined type
						if ( !isSystem( prop.PropertyType ) &&  !isCollection( prop.PropertyType ) )
						{
							Array arr = (Array)prop.GetValue( obj, null );
							for ( int i=0; i < arr.Length; i++ )
							{
								object new_obj  = arr.GetValue( i );
								Modify( new_obj );
								arr.SetValue( new_obj, i );
							}
							prop.SetValue( obj, arr, null );
						}
					}
					else
					{
						//primitive type
						if ( isPrimitive( prop.PropertyType ) )
						{
							prop.SetValue( obj, GetModifiedValue( prop.GetValue( obj, null ) ), null );
						}
						
						//user defined type
						if ( !isSystem( prop.PropertyType ) &&  !isCollection( prop.PropertyType ) )
						{
							object new_obj = prop.GetValue( obj, null );
							Modify(new_obj);
							prop.SetValue( obj, new_obj, null );
						}

						// object
						if ( isObject( prop.PropertyType ) )
						{
							object new_obj = prop.GetValue( obj, null );
							Modify(new_obj);
							prop.SetValue( obj, new_obj, null );
						} 
					} 
				} // field info
			} // for each
			//return obj;
		}

		static void ModifyDataSet(DataSet ds)
		{
			foreach (DataTable dt in ds.Tables)
			{
				ModifyDataTable(dt);
			}
		}
		static void ModifyDataTable(DataTable dt)
		{
			foreach(DataRow dr in dt.Rows)
			{
				foreach (DataColumn dc in dt.Columns)
				{
					switch (dc.DataType.Name)
					{
						case "String":
							dr[dc] = dr[dc].ToString() + "mod"; 
							break;
						case "Int32":
 							dr[dc] = Convert.ToInt32( dr[dc] ) * 100;
							break;
					}
				}
			}
		}

		static void ModifyXmlElement(System.Xml.XmlElement xmlElem)
		{
			xmlElem.InnerText = "54321";
//			xmlElem.InnerXml = "<books>" +   
//				"<book>" + 
//				"<author>Carson</author>" + 
//				"<price format=\"dollar\">33.99</price>" + 
//				"<pubdate>01/01/2003</pubdate>" + 
//				"</book>" + 
//				"<pubinfo>" + 
//				"<publisher>MisPress</publisher>" + 
//				"<state>CA</state>" + 
//				"</pubinfo>" + 
//				"</books>";
		}

		static void ModifyCollection(object co)
		{
			for (int i=0; i < ((IList)co).Count; i++)
			{
				object o = ((IList)co)[i];
				o = GHTObjModifier(o);
				((IList)co)[i] = o;
			}
		}

		static object Generate( object obj )
		{
			MemberInfo [] mic;

			if ( obj == null ) return null;

			if (obj.GetType().IsEnum)
			{
				Array a = Enum.GetValues(obj.GetType());
				return a.GetValue(a.Length-1);
			}

			if ( isObject( obj.GetType() ))
			{
				//obj = GetRandomValue( typeof( System.String ) );
				obj = new object();
				return obj;
			}

			mic = obj.GetType().GetMembers( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic );
			foreach ( MemberInfo mi in mic )
			{
				// FieldInfo
				//
				if ( mi is FieldInfo )
				{
					FieldInfo field = (FieldInfo)mi;
				
					// is array of primitive
					//
					if ( field.FieldType.IsArray )
					{
						if ( isPrimitive( field.FieldType.GetElementType() ) )
						{
							Array arr = Array.CreateInstance( field.FieldType.GetElementType(), ARRAY_SIZE);
							for (int i=0; i < arr.Length; i++)
							{
								arr.SetValue(GetRandomValue( field.FieldType.GetElementType() ), i );
							}
							field.SetValue( obj, arr );
						}
					}
					else
					{
						if ( isPrimitive( field.FieldType ) )
						{
							field.SetValue( obj, GetRandomValue( field.FieldType ) );
						}
					}

					// Collection type member
					//
					if ( isCollection( field.FieldType ) )
					{
						object new_obj = Activator_CreateInstance(field.FieldType);

						MethodInfo mm = null;
						MethodInfo [] mmi = field.FieldType.GetMethods(BindingFlags.DeclaredOnly | 
							BindingFlags.Instance |
							BindingFlags.Public);


						foreach ( MethodInfo m in mmi ) 
						{
							if ( m.Name == "Add" ) 
							{
								mm = m;
								break;
							}
						}

						if ( mm != null ) 
						{
							ParameterInfo [] pi = mm.GetParameters();
							Type prmType1 = pi[0].ParameterType;
							Type prmType2 = null;
							if (pi.Length > 1) prmType2 = pi[1].ParameterType;

							if ( field.FieldType.GetInterface("IList") != null )
							{
								for ( int i=0; i < ARRAY_SIZE; i++ )
								{
									if ( isPrimitive( prmType1 ) )
									{
										((IList)new_obj).Add( GetRandomValue( prmType1 ) );
									}
									else
									{
										object prm_obj = Activator_CreateInstance( prmType1 );
										((IList)new_obj).Add( Generate( prm_obj ) );
									}
								}
								field.SetValue( obj, new_obj );
							}

							if ( prmType2 != null)
							{
								if ( field.FieldType.GetInterface("IDictionary") != null)
								{
									for ( int i=0; i < ARRAY_SIZE; i++ )
									{
										if ( isPrimitive( prmType1 ) && isPrimitive( prmType2 ) )
										{
											((IDictionary)new_obj).Add( GetRandomValue( prmType1 ), GetRandomValue( prmType2 ) );
										}
										else
										{
											object prm_obj1 = Activator_CreateInstance( prmType1 );
											object prm_obj2 = Activator_CreateInstance( prmType2 );
											((IDictionary)new_obj).Add( Generate( prm_obj1 ), Generate( prm_obj2 ) );
										}
									}
								}
								field.SetValue( obj, new_obj );
							}
						}
					} // collection

					// is array of user defined type
					//
					if ( field.FieldType.IsArray )
					{
						if ( !isCollection( field.FieldType ) )
						{
							Array arr  = Array.CreateInstance( field.FieldType.GetElementType(), ARRAY_SIZE );
							for ( int i=0; i < arr.Length; i++ )
							{
								object new_obj = GHTTypeGenerator( field.FieldType.GetElementType() );
								arr.SetValue( new_obj, i );
							}
							field.SetValue( obj, arr );
						}
					}
					else
					{
						if ( !isCollection( field.FieldType ) )
						{
							object new_obj = GHTTypeGenerator( field.FieldType );
							field.SetValue( obj, new_obj );
						}

					} // user defined type

					// object
					//
					if ( isObject( field.FieldType ) )
					{
						object new_obj = Activator_CreateInstance(field.FieldType);
						new_obj = "GH test";
						field.SetValue( obj, new_obj );
					} // object
				} // field info
			
				// PropertyInfo
				//
				if ( mi is PropertyInfo )
				{
					PropertyInfo prop = (PropertyInfo)mi;

					// is array of primitive type member
					//
					if ( prop.PropertyType.IsArray )
					{
						if ( isPrimitive( prop.PropertyType.GetElementType() ) )
						{
							Array arr = Array.CreateInstance( prop.PropertyType.GetElementType(), ARRAY_SIZE);
							for (int i=0; i < arr.Length; i++)
							{
								arr.SetValue(GetRandomValue( prop.PropertyType.GetElementType() ), i );
							}
							prop.SetValue( obj, arr, null );
						}
					}
					else
					{
						if ( isPrimitive( prop.PropertyType ) )
						{
							prop.SetValue( obj, GetRandomValue( prop.PropertyType ), null );
						}
					} // primitive

					// Colletion type member
					//
					if ( isCollection( prop.PropertyType ) )
					{
						object new_obj = Activator_CreateInstance( prop.PropertyType );

						MethodInfo mm = null;
						MethodInfo [] mmi = prop.PropertyType.GetMethods( BindingFlags.DeclaredOnly | 
							BindingFlags.Instance |
							BindingFlags.Public);


						foreach ( MethodInfo m in mmi ) 
						{
							if ( m.Name == "Add" ) 
							{
								mm = m;
								break;
							}
						}

						if ( mm != null ) 
						{
							ParameterInfo [] pi = mm.GetParameters();
							Type prmType1 = pi[0].ParameterType;
							Type prmType2 = null;
							if (pi.Length > 1) prmType2 = pi[1].ParameterType;

							if ( prop.PropertyType.GetInterface("IList") != null )
							{
								for ( int i=0; i < ARRAY_SIZE; i++ )
								{
									if ( isPrimitive( prmType1 ) )
									{
										((IList)new_obj).Add( GetRandomValue( prmType1 ) );
									}
									else
									{
										object prm_obj = Activator_CreateInstance( prmType1 );
										((IList)new_obj).Add( Generate( prm_obj ) );
									}
								}
								prop.SetValue( obj, new_obj, null );
							}

							if ( prmType2 != null)
							{
								if ( prop.PropertyType.GetInterface("IDictionary") != null)
								{
									for ( int i=0; i < ARRAY_SIZE; i++ )
									{
										if ( isPrimitive( prmType1 ) && isPrimitive( prmType2 ) )
										{
											((IDictionary)new_obj).Add( GetRandomValue( prmType1 ), GetRandomValue( prmType2 ) );
										}
										else
										{
											object prm_obj1 = Activator_CreateInstance( prmType1 );
											object prm_obj2 = Activator_CreateInstance( prmType2 );
											((IDictionary)new_obj).Add( Generate( prm_obj1 ), Generate( prm_obj2 ) );
										}
									}
									prop.SetValue( obj, new_obj, null );
								}
							}
						}
					} // collection

					// is array user defined type
					//
					if ( prop.PropertyType.IsArray )
					{
						if ( !isSystem( prop.PropertyType ) &&  !isCollection( prop.PropertyType ) )
						{
							Array arr  = Array.CreateInstance( prop.PropertyType.GetElementType(), ARRAY_SIZE );
							for ( int i=0; i < arr.Length; i++ )
							{
								object new_obj  = Activator_CreateInstance( prop.PropertyType.GetElementType() );
								Generate( new_obj );
								arr.SetValue( new_obj, i );
							}
							prop.SetValue( obj, arr, null );
						}
					}
					else
					{
						if ( !isSystem( prop.PropertyType ) &&  !isCollection( prop.PropertyType ) )
						{
							object new_obj = Activator_CreateInstance( prop.PropertyType );
							Generate(new_obj);
							prop.SetValue( obj, new_obj, null );
						}
					} // user defined type

					// object
					//
					if ( isObject( prop.PropertyType ) )
					{
						object new_obj = Activator_CreateInstance( prop.PropertyType );
						new_obj = "GH test";
						prop.SetValue( obj, new_obj, null );
					} // object
				} // field info

			} // for each
			return obj;
		}

		static DataSet GenerateDataSet()
		{
			string strTemp = string.Empty;
			DataSet ds = new DataSet("CustOrdersDS");
			DataTable dtCusts = new DataTable("Customers");

			ds.Tables.Add(dtCusts);

			DataTable dtOrders = new DataTable("Orders");
			ds.Tables.Add(dtOrders);

			// add ID column with autoincrement numbering
			// and Unique constraint
			DataColumn dc = dtCusts.Columns.Add("ID", typeof(Int32));
			dc.AllowDBNull = false;
			dc.AutoIncrement = true;
			dc.AutoIncrementSeed = 1;
			dc.AutoIncrementStep = 1;
			dc.Unique = true;

			// make the ID column part of the PrimaryKey
			// for the table
			dtCusts.PrimaryKey = new DataColumn[] {dc};

			// add name and company columns with length restrictions
			// and default values
			dc = dtCusts.Columns.Add("Name", typeof(String));
			dc.MaxLength = 255;
			dc.DefaultValue = "nobody";
			dc = dtCusts.Columns.Add("Company", typeof(String));
			dc.MaxLength = 255;
			dc.DefaultValue = "nonexistent";

			// fill the table
			for (int i=0; i < 10; i++)
			{
				DataRow dr = dtCusts.NewRow();
				strTemp = (string)GetRandomValue(typeof(String));
				if (strTemp.Length > 255) strTemp = strTemp.Remove(0,254);
				dr["Name"] = strTemp;
				strTemp = (string)GetRandomValue(typeof(String));
				if (strTemp.Length > 255) strTemp = strTemp.Remove(0,254);
				dr["Company"] = strTemp; 
				dtCusts.Rows.Add(dr);
			}


			// add ID columns with autoincrement numbering
			// and Unique constraint
			dc = dtOrders.Columns.Add("ID", typeof(Int32));
			dc.AllowDBNull = false;
			dc.AutoIncrement = true;
			dc.AutoIncrementSeed = 1;
			dc.AutoIncrementStep = 1;
			dc.Unique = true;

			// add custid, date and total columns
			dtOrders.Columns.Add("CustID", typeof(Int32));
			dtOrders.Columns.Add("Date", typeof(DateTime));
			dtOrders.Columns.Add("Total", typeof(Decimal));

		
			for (int i=0; i < 10; i++)
			{
			
				DataRow dr = dtOrders.NewRow();
				dr["CustID"] = i;
				dr["Date"] = GetRandomValue(typeof(DateTime));
				dr["Total"] = i * i;
				dtOrders.Rows.Add(dr);
			}

			// make the ID column part of the PrimaryKey
			// for the table
			dtOrders.PrimaryKey = new DataColumn[] {dc};

			return ds;
		}
		static DataTable GenerateDataTable()
		{
			DataTable dt = new DataTable("Customers");
			string strTemp = string.Empty;

			// add ID column with autoincrement numbering
			// and Unique constraint
			DataColumn dc = dt.Columns.Add("ID", typeof(Int32));
			dc.AllowDBNull = false;
			dc.AutoIncrement = true;
			dc.AutoIncrementSeed = 1;
			dc.AutoIncrementStep = 1;
			dc.Unique = true;

			// make the ID column part of the PrimaryKey
			// for the table
			dt.PrimaryKey = new DataColumn[] {dc};

			// add name and company columns with length restrictions
			//' and default values
			dc = dt.Columns.Add("Name", typeof(String));
			dc.MaxLength = 255;
			dc.DefaultValue = "nobody";
			dc = dt.Columns.Add("Company", typeof(String));
			dc.MaxLength = 255;
			dc.DefaultValue = "nonexistent";

			// fill the table
			for (int i=0; i < 10; i++)
			{
				DataRow dr = dt.NewRow();
				strTemp = (string)GetRandomValue(typeof(String));
				if (strTemp.Length > 255) strTemp = strTemp.Remove(0,254);
				dr["Name"] = strTemp;
				strTemp = (string)GetRandomValue(typeof(String));
				if (strTemp.Length > 255) strTemp = strTemp.Remove(0,254);
				dr["Company"] = strTemp; 
				dt.Rows.Add(dr);
			}
			return dt;
		}

		static object GenerateCollection(Type t)
		{
			object new_obj = Activator_CreateInstance( t );

			MethodInfo MI = null;
			MethodInfo [] arrMI = t.GetMethods(BindingFlags.DeclaredOnly | 
				BindingFlags.Instance |
				BindingFlags.Public);

			foreach ( MethodInfo m in arrMI ) 
			{
				if ( m.Name == "Add" ) 
				{
					MI = m;
					break;
				}
			}

			if ( MI != null ) 
			{
				ParameterInfo [] pi = MI.GetParameters();
				Type prmType1 = pi[0].ParameterType;
				Type prmType2 = null;
				if (pi.Length > 1) prmType2 = pi[1].ParameterType;

				if ( t.GetInterface("IList") != null )
				{
					for ( int i=0; i < ARRAY_SIZE; i++ )
					{
						if ( isPrimitive( prmType1 ) )
						{
							((IList)new_obj).Add( GetRandomValue( prmType1 ) );
						}
						else
						{
							//object prm_obj = Activator_CreateInstance( prmType1 );
							//((IList)new_obj).Add( Generate( prm_obj ) );
							((IList)new_obj).Add(GHTTypeGenerator(prmType1));
						}
					}
					return new_obj;
				}

				if ( prmType2 != null)
				{
					if ( t.GetInterface("IDictionary") != null)
					{
						for ( int i=0; i < ARRAY_SIZE; i++ )
						{
							if ( isPrimitive( prmType1 ) && isPrimitive( prmType2 ) )
							{
								((IDictionary)new_obj).Add( GetRandomValue( prmType1 ), GetRandomValue( prmType2 ) );
							}
							else
							{
								object prm_obj1 = Activator_CreateInstance( prmType1 );
								object prm_obj2 = Activator_CreateInstance( prmType2 );
								((IDictionary)new_obj).Add( Generate( prm_obj1 ), Generate( prm_obj2 ) );
							}
						}
					}
					return new_obj;
				}
			}// if ( MI != null ) 
			return new_obj;
		}

		static Array GenerateArray(Type t)
		{
			if ( isPrimitive( t.GetElementType() ) )
			{
				Array arr = Array.CreateInstance( t.GetElementType(), ARRAY_SIZE);
				for (int i=0; i < arr.Length; i++)
				{
					arr.SetValue(GetRandomValue( t.GetElementType() ), i );
				}
				return arr;
			}
			else
			{
				Array arr  = Array.CreateInstance( t.GetElementType(), ARRAY_SIZE );
				for ( int i=0; i < arr.Length; i++ )
				{
					//object new_obj  = Activator_CreateInstance( t.GetElementType() );
					//Generate( new_obj );

					object new_obj  = GHTTypeGenerator(t.GetElementType());
					arr.SetValue( new_obj, i );
				}
				return arr;
			}
		}


		static bool isPrimitive(Type t)
		{
			if ( t.IsPrimitive ) return true;
			if ( t.Name == "String" ) return true;
			if ( t.Name == "DateTime" ) return true;
			if ( t.Name == "Decimal" ) return true;
			return false;
		}

		static bool isSystem(Type t)
		{
			if ( t.FullName == "System.Collections" ) return false;
			if ( t.FullName.StartsWith("System.") ) return true;
			return false;
		}

		static bool isObject(Type t)
		{
			if ( t.FullName == "System.Object" ) return true;
			return false;
		}

		static bool isCollection(Type t)
		{
			if ( t.GetInterface("IList") != null) return true;
			if ( t.GetInterface("IDictionary") != null) return true;
			if ( t.GetInterface("ICollection") != null) return true;
			return false;
		}



		static object Activator_CreateInstance(Type t)
		{
			try
			{
				if (t.IsEnum)
				{
					Array a = Enum.GetNames(t);
				 	return Enum.Parse(t,a.GetValue(0).ToString());
				}
				else
                    return t.GetConstructor(new Type[]{}).Invoke(new object[]{});
			}
			catch( Exception ex )
			{
				throw new Exception("Activator - Could not create type " + t.Name + " - " + ex.Message);
			}
		}

	}	

}
