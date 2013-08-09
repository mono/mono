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
// Copyright (C) Lluis Sanchez Gual, 2004
//

#if !FULL_AOT_RUNTIME
using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeLiteral: CodeExpression
	{
		object value;
		Type type;
		
		public CodeLiteral (object value)
		{
			this.value = value;
			if (value != null) type = value.GetType ();
			else type = typeof(object);
		}
			
		public CodeLiteral (object value, Type type)
		{
			this.value = value;
			this.type = type;
		}

		public object Value {
			get { return value; }
		}

		public override void Generate (ILGenerator gen)
		{
			object value = this.value;
			
			if (value == null)
			{
				gen.Emit (OpCodes.Ldnull);
				return;
			}
			
			if (value is Enum)
				value = Convert.ChangeType (value, (value.GetType().UnderlyingSystemType), CultureInfo.InvariantCulture);
			
			if (value is Type) {
				gen.Emit (OpCodes.Ldtoken, (Type)value);
				gen.Emit (OpCodes.Call, typeof(Type).GetMethod ("GetTypeFromHandle"));
				return;
			}
			
			switch (Type.GetTypeCode (type))
			{
				case TypeCode.String:
					gen.Emit (OpCodes.Ldstr, (string)value);
					break;
					
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.UInt16:
				case TypeCode.Int32:
				case TypeCode.UInt32:
					int i = (int)value;
					switch (i)
					{
						case 0: gen.Emit (OpCodes.Ldc_I4_0); break;
						case 1: gen.Emit (OpCodes.Ldc_I4_1); break;
						case 2: gen.Emit (OpCodes.Ldc_I4_2); break;
						case 3: gen.Emit (OpCodes.Ldc_I4_3); break;
						case 4: gen.Emit (OpCodes.Ldc_I4_4); break;
						case 5: gen.Emit (OpCodes.Ldc_I4_5); break;
						case 6: gen.Emit (OpCodes.Ldc_I4_6); break;
						case 7: gen.Emit (OpCodes.Ldc_I4_7); break;
						case 8: gen.Emit (OpCodes.Ldc_I4_8); break;
						case -1: gen.Emit (OpCodes.Ldc_I4_M1); break;
						default: gen.Emit (OpCodes.Ldc_I4, i); break;
					}
					break;
					
				case TypeCode.Int64:
				case TypeCode.UInt64:
					gen.Emit (OpCodes.Ldc_I8, (long)value);
					break;
					
				case TypeCode.Single:
					gen.Emit (OpCodes.Ldc_R4, (float)value);
					break;
					
				case TypeCode.Double:
					gen.Emit (OpCodes.Ldc_R8, (double)value);
					break;
					
				case TypeCode.Boolean:
					if ((bool)value)
						gen.Emit (OpCodes.Ldc_I4_1);
					else
						gen.Emit (OpCodes.Ldc_I4_0);
					break;
					
				default:
					throw new InvalidOperationException ("Literal type " + value.GetType() + " not supported");
			}
		}
		
		public override void PrintCode (CodeWriter cp)
		{
			if (value is string)
				cp.Write ("\"").Write (value.ToString ()).Write ("\"");
			else if (value == null)
				cp.Write ("null");
			else if (value is Type)
				cp.Write ("typeof(" + ((Type)value).Name + ")");
			else if (value is Enum)
				cp.Write (value.GetType().Name + "." + value);
			else
				cp.Write (value.ToString ());
		}
		
		public override Type GetResultType ()
		{
			return type;
		}
	}
}
#endif
