//
// Information.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Chris J Breisch
//     2003 Tipic, Inc. (http://www.tipic.com)
//     2004 Rafael Teixeira
//

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.VisualBasic
{
	using CompilerServices;

	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class Information {

		private Information ()
		{
			//Nothing to do here...
		}

		private static int [] QBColorTable = { 0, 8388608, 32768, 8421376, 
						       128, 8388736, 32896, 12632256, 
						       8421504, 16711680, 65280, 16776960, 
						       255, 16711935, 65535, 16777215 };



		public static System.Boolean IsArray (Object VarName)
		{
			if(VarName == null)
				return false;
			else
				return VarName is Array;
		}

		public static System.Boolean IsError (Object Expression)
		{ 
			if(Expression == null )
				return false;
			else
				return Expression is Exception;
		}

		public static System.Boolean IsReference (Object Expression)
		{
			return (false == Expression is ValueType);
		}



		public static System.Boolean IsDate (System.Object Expression)
		{
			if(Expression == null)
				return false;
			else if(Expression is DateTime)
				return true;
			else if(Expression is String) {
				try {
					DateTime.Parse((String) Expression);
				}
				catch (Exception e) {
					return false;
				}

				return true;
			}
			else
				return false;
 		}


		public static System.Boolean IsDBNull (System.Object Expression) 
		{ 
			if(Expression == null)
				return false;
			else
				return Expression is DBNull;
		}

		public static System.Boolean IsNothing (System.Object Expression) 
		{ 
 			return (Expression == null);
 		}

		public static System.Int32 RGB (int Red, int Green, int Blue) 
		{ 
			if(Red < 0)
				throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Red"));
			if(Green < 0)
				throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Green"));
			if(Blue < 0)
				throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Blue"));

			if(Red > 255) 
				Red = 255;
			if(Green > 255) 
				Green = 255;
			if(Blue > 255) 
				Blue = 255;

			return (((Red & 0xFF) << 16) | ((Green & 0xFF) << 8) | ((Blue & 0xFF) << 0));
		}




		public static System.Boolean IsNumeric (System.Object Expression) 
		{ 

			if(Expression == null || Expression is DateTime)
 				return false;

			if(Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal ||
			    Expression is Single || Expression is Double || Expression is Boolean)
				return true;
	


			try {
				if(Expression is string)
					Double.Parse(Expression as string);
				else
					Double.Parse(Expression.ToString());
				return true;
			} catch {} // just dismiss errors but return false

			return false;
		}

		public static System.Int32 QBColor (System.Int32 Color) 
		{ 
			if(Color < 0 || Color > 15)
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_InvalidValue1", "Color"));
			return QBColorTable[Color];
		}

		public static String VbTypeName(String UrtName)
		{

			if(UrtName == null)
				return null;

			String tmpName = UrtName.Trim().ToLower(CultureInfo.InvariantCulture);

			if(tmpName.StartsWith("system."))
				tmpName = tmpName.Substring(7);

			if(tmpName.Equals("object"))
				return "Object";
			if(tmpName.Equals("int16"))
				return "Short";
			if(tmpName.Equals("clrint16"))
				return "Short";
			if(tmpName.Equals("int32"))
				return "Integer";
			if(tmpName.Equals("clrint32"))
				return "Integer";
			if(tmpName.Equals("int64"))
				return "Long";
			if(tmpName.Equals("clrint64"))
				return "Long";
			if(tmpName.Equals("clrsingle"))
				return "Single";
			if(tmpName.Equals("single"))
				return "Single";
			if(tmpName.Equals("double"))
				return "Double";
			if(tmpName.Equals("clrdouble"))
				return "Double";
			if(tmpName.Equals("boolean"))
				return "Boolean";
			if(tmpName.Equals("clrboolean"))
				return "Boolean";
			if(tmpName.Equals("char"))
				return "Char";
			if(tmpName.Equals("clrchar"))
				return "char";
			if(tmpName.Equals("string"))
				return "String";
			if(tmpName.Equals("java.lang.string"))
				return "String";
			if(tmpName.Equals("byte"))
				return "Byte";
			if(tmpName.Equals("clrbyte"))
				return "Byte";
			if(tmpName.Equals("decimal"))
				return "Decimal";
			if(tmpName.Equals("datetime"))
				return "Date";

			return null;
		}

		internal static String TypeNameOfCOMObject (Object obj, Boolean flag) 
		{
			throw new NotImplementedException("Method Microsoft.VisualBasic.Information.TypeNameOfCOMObject() is not supported");
		}

		public static System.Int32 UBound (System.Array Array, 
						   [Optional, DefaultValue(1)] 
						   System.Int32 Rank) 
		{ 
			Exception e;

			if(Array == null) {
				e =  new ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", "Array"));
				throw (ArgumentNullException) ExceptionUtils.VbMakeException(e, 9);
			}

			e = ExceptionUtils.VbMakeException((Exception) new ArgumentNullException(VBUtils.GetResourceString(9)), 9);

			if(!IsArray(Array))
			{
				throw new ArgumentException("Not array arrived to UBound method of Information class");
			}

			if(Rank < 1 || Rank > Array.Rank)
				throw new RankException(Utils.GetResourceString("Argument_InvalidRank1", "Rank"));

			return Array.GetUpperBound(Rank-1);
		}

		public static System.Int32 LBound (System.Array Array, 
						   [Optional, DefaultValue(1)] 
						   System.Int32 Rank) 
		{ 
			Exception e;

			if(Array == null) {
				e = new ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", "Array"));
				throw (ArgumentNullException)ExceptionUtils.VbMakeException(e, 9);
			}

			if(!IsArray(Array)) {
				throw new ArgumentException("Not array arrived to LBound method of Information class");
			}

			if(Rank < 1 || Rank > Array.Rank)
				throw new RankException(Utils.GetResourceString("Argument_InvalidRank1", "Rank"));

			return Array.GetLowerBound(Rank-1);
		}

		public static Microsoft.VisualBasic.ErrObject Err() 
		{ 
			return ProjectData.Err;
		}

		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
		public static System.Int32 Erl () 
		{ 
			return ProjectData.Err.Erl;
		}

		public static System.String TypeName (System.Object VarName) 
		{ 
			String name;
			Boolean isArray = false;

			if(VarName == null)
				return "Nothing";

			Type type = VarName.GetType();
			if(type.IsArray) {
				isArray = true;
				type = type.GetElementType();
			}
			if(type.IsEnum)
				name = type.Name;
			else {
				switch(Type.GetTypeCode(type)) {
				case TypeCode.DBNull:
					name = "DBNull";
					break;
				case TypeCode.Boolean:
					name = "Boolean";
					break;
				case TypeCode.Char:
					name = "Char";
					break;
				case TypeCode.Byte:
					name = "Byte";
					break;
				case TypeCode.Int16:
					name = "Short";
					break;
				case TypeCode.Int32:
					name = "Integer";
					break;
				case TypeCode.Int64:
					name = "Long";
					break;
				case TypeCode.Single:
					name = "Single";
					break;
				case TypeCode.Double:
					name = "Double";
					break;
				case TypeCode.Decimal:
					name = "Decimal";
					break;
				case TypeCode.DateTime:
					name = "Date";
					break;
				case TypeCode.String:
					name = "String";
					break;
				default :
					name = type.Name;
					break;
				}
				
				// the following is commented even with mainsoft

				// if(type.get_IsCOMObject() && name.equals("__ComObject"))
				// {
				//	throw new NotImplementedException("COM Objects unsupported in TypeName of Information");
				// }
			}
			int index = name.IndexOf('+');
			if(index >= 0)
				name = name.Substring(index + 1);

			if(isArray == true) {
				int rank = (VarName as Array).Rank;
				if(rank == 1)
					name = name + "[]";
				else {
					name = name + "[";
					for (int i = 0; i < rank - 1; i++)
						name += ',';
					name = name + "]";
				}
				name = Utils.VBFriendlyNameOfTypeName(name);
			}
			return name;
		}

		public static System.String SystemTypeName (System.String VbName) 
		{ 
			String tmpStr = VbName.Trim().ToLower(CultureInfo.InvariantCulture);
			if(tmpStr.Equals("object"))
				return "System.Object";
			if(tmpStr.Equals("short"))
				return "System.Int16";
			if(tmpStr.Equals("integer"))
				return "System.Int32";
			if(tmpStr.Equals("single"))
				return "System.Single";
			if(tmpStr.Equals("double"))
				return "System.Double";
			if(tmpStr.Equals("date"))
				return "System.DateTime";
			if(tmpStr.Equals("string"))
				return "System.String";
			if(tmpStr.Equals("boolean"))
				return "System.Boolean";
			if(tmpStr.Equals("decimal"))
				return "System.Decimal";
			if(tmpStr.Equals("byte"))
				return "System.Byte";
			if(tmpStr.Equals("char"))
				return "System.Char";
			if(tmpStr.Equals("long"))
				return "System.Int64";

			return null;
		}

		[MonoTODO]
		public static VariantType VarType(Object VarName)
		{
			if(VarName == null)
				return VariantType.Object;

			if(VarName is Exception)
				return VariantType.Error;

			return varType(VarName.GetType());
		}

		private static VariantType varType(Type varType)
		{
			if(varType == null)
				return VariantType.Object;

			if(varType.IsArray) {

				Type type = varType.GetElementType();
				if(type.IsArray)
					return VariantType.ObjectArray;

				int elemVal = (int) Information.varType(type);
				return (VariantType)((int)VariantType.Array | elemVal);
			}
        
			if(varType.IsEnum)
				varType = Enum.GetUnderlyingType(varType);

			if(varType == null)
				return VariantType.Empty;

			switch(Type.GetTypeCode(varType)) {
			case TypeCode.Empty:    
				return VariantType.Empty;
			case TypeCode.DBNull:   
				return VariantType.Null;
			case TypeCode.Boolean:  
				return VariantType.Boolean;
			case TypeCode.Char:     
				return VariantType.Char;
			case TypeCode.Byte:     
				return VariantType.Byte;
			case TypeCode.Int16:    
				return VariantType.Short;
			case TypeCode.Int32:    
				return VariantType.Integer;
			case TypeCode.Int64:    
				return VariantType.Long;
			case TypeCode.Single:   
				return VariantType.Single;
			case TypeCode.Double:   
				return VariantType.Double;
			case TypeCode.Decimal:  
				return VariantType.Decimal;
			case TypeCode.DateTime: 
				return VariantType.Date;
			case TypeCode.String:   
				return VariantType.String;
			}

			if(varType.IsSubclassOf(typeof(System.Exception)))
				return VariantType.Error;

			if(varType.IsValueType)
				return VariantType.UserDefinedType;

			return VariantType.Object;
		}
 	}
}


