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
using System.Collections;
using System.Reflection.Emit;
using System.Reflection;

namespace Mono.CodeGeneration
{
	public class CodeGenerationHelper
	{
		public static void GenerateMethodCall (ILGenerator gen, CodeExpression target, MethodBase method, params CodeExpression[] parameters)
		{
			Type[] ptypes = Type.EmptyTypes;
			// It could raise an error since GetParameters() on MethodBuilder is not supported.
			if (parameters.Length > 0) {
				ParameterInfo[] pars = method.GetParameters ();
				ptypes = new Type[pars.Length];
				for (int n=0; n<ptypes.Length; n++) ptypes[n] = pars[n].ParameterType;
			}
			GenerateMethodCall (gen, target, method, ptypes, parameters);
		}
		
		public static void GenerateMethodCall (ILGenerator gen, CodeExpression target, CodeMethod method, params CodeExpression[] parameters)
		{
			GenerateMethodCall (gen, target, method.MethodBase, method.ParameterTypes, parameters);
		}
		
		static void GenerateMethodCall (ILGenerator gen, CodeExpression target, MethodBase method, Type[] parameterTypes, params CodeExpression[] parameters)
		{
			OpCode callOp;
			
			if (parameterTypes.Length != parameters.Length)
				throw GetMethodException (method, "Invalid number of parameters, expected " + parameterTypes.Length + ", found " + parameters.Length + ".");  
			
			if (!object.ReferenceEquals (target, null)) 
			{
				target.Generate (gen);
				
				Type targetType = target.GetResultType();
				if (targetType.IsValueType) {
					LocalBuilder lb = gen.DeclareLocal (targetType);
					gen.Emit (OpCodes.Stloc, lb);
					gen.Emit (OpCodes.Ldloca, lb);
					callOp = OpCodes.Call;
				}
				else
					callOp = OpCodes.Callvirt;
			}
			else
				callOp = OpCodes.Call;

			for (int n=0; n<parameterTypes.Length; n++) {
				try {
					CodeExpression par = parameters[n];
					par.Generate (gen);
					GenerateSafeConversion (gen, parameterTypes[n], par.GetResultType());
				}
				catch (InvalidOperationException ex) {
					throw GetMethodException (method, "Parameter " + n + ". " + ex.Message);  
				}
			}
			
			if (method is MethodInfo)
				gen.Emit (callOp, (MethodInfo)method);
			else if (method is ConstructorInfo)
				gen.Emit (callOp, (ConstructorInfo)method);
		}
		
		public static Exception GetMethodException (MethodBase method, string msg)
		{
			return new InvalidOperationException ("Call to method " + method.DeclaringType + "." + method.Name + ": " + msg);  
		}
		
		public static void GenerateSafeConversion (ILGenerator gen, Type targetType, Type sourceType)
		{
			if (!targetType.IsAssignableFrom (sourceType)) {
				throw new InvalidOperationException ("Invalid type conversion. Found '" + sourceType + "', expected '" + targetType + "'.");
			}
			
			if (targetType == typeof(object) && sourceType.IsValueType) {
				gen.Emit (OpCodes.Box, sourceType);
			}
		}

		public static void LoadFromPtr (ILGenerator ig, Type t)
		{
			if (t == typeof(int))
				ig.Emit (OpCodes.Ldind_I4);
			else if (t == typeof(uint))
				ig.Emit (OpCodes.Ldind_U4);
			else if (t == typeof(short))
				ig.Emit (OpCodes.Ldind_I2);
			else if (t == typeof(ushort))
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == typeof(char))
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == typeof(byte))
				ig.Emit (OpCodes.Ldind_U1);
			else if (t == typeof(sbyte))
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == typeof(ulong))
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == typeof(long))
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == typeof(float))
				ig.Emit (OpCodes.Ldind_R4);
			else if (t == typeof(double))
				ig.Emit (OpCodes.Ldind_R8);
			else if (t == typeof(bool))
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == typeof(IntPtr))
				ig.Emit (OpCodes.Ldind_I);
			else if (t.IsEnum) {
				if (t == typeof(Enum))
					ig.Emit (OpCodes.Ldind_Ref);
				else
					LoadFromPtr (ig, System.Enum.GetUnderlyingType (t));
			} else if (t.IsValueType)
				ig.Emit (OpCodes.Ldobj, t);
			else
				ig.Emit (OpCodes.Ldind_Ref);
		}

		public static void SaveToPtr (ILGenerator ig, Type t)
		{
			if (t == typeof(int))
				ig.Emit (OpCodes.Stind_I4);
			else if (t == typeof(uint))
				ig.Emit (OpCodes.Stind_I4);
			else if (t == typeof(short))
				ig.Emit (OpCodes.Stind_I2);
			else if (t == typeof(ushort))
				ig.Emit (OpCodes.Stind_I2);
			else if (t == typeof(char))
				ig.Emit (OpCodes.Stind_I2);
			else if (t == typeof(byte))
				ig.Emit (OpCodes.Stind_I1);
			else if (t == typeof(sbyte))
				ig.Emit (OpCodes.Stind_I1);
			else if (t == typeof(ulong))
				ig.Emit (OpCodes.Stind_I8);
			else if (t == typeof(long))
				ig.Emit (OpCodes.Stind_I8);
			else if (t == typeof(float))
				ig.Emit (OpCodes.Stind_R4);
			else if (t == typeof(double))
				ig.Emit (OpCodes.Stind_R8);
			else if (t == typeof(bool))
				ig.Emit (OpCodes.Stind_I1);
			else if (t == typeof(IntPtr))
				ig.Emit (OpCodes.Stind_I);
			else if (t.IsEnum) {
				if (t == typeof(Enum))
					ig.Emit (OpCodes.Stind_Ref);
				else
					SaveToPtr (ig, System.Enum.GetUnderlyingType (t));
			} else if (t.IsValueType)
				ig.Emit (OpCodes.Stobj, t);
			else
				ig.Emit (OpCodes.Stind_Ref);
		}

		public static bool IsNumber (Type t)
		{
			switch (Type.GetTypeCode (t))
			{
				case TypeCode.Byte:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;
				default:
					return false;
			}
		}
		
		public static void GeneratePrimitiveValue ()
		{
		}
	}
}
#endif
