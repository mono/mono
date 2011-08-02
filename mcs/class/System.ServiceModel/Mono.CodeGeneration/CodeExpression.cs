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

#if !MONOTOUCH
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public abstract class CodeExpression: CodeItem
	{
		internal CodeVariableReference var;

		public abstract Type GetResultType ();
		
		public virtual void GenerateAsStatement (ILGenerator gen)
		{
			Generate (gen);
			gen.Emit (OpCodes.Pop);
		}
		
		public CodeExpression CallToString ()
		{
			return new CodeMethodCall (this, "ToString");
		}
		
		public static CodeExpression AreEqual (CodeExpression e1, CodeExpression e2)
		{
			return new CodeEquals (e1, e2);
		}
		
		public static CodeExpression AreNotEqual (CodeExpression e1, CodeExpression e2)
		{
			return new CodeNotEquals (e1, e2);
		}
		
		public static CodeExpression IsGreaterThan (CodeExpression e1, CodeExpression e2)
		{
			return new CodeGreaterThan (e1, e2);
		}
		
		public static CodeExpression IsSmallerThan (CodeExpression e1, CodeExpression e2)
		{
			return new CodeLessThan (e1, e2);
		}
		
		public static CodeExpression IsGreaterEqualThan (CodeExpression e1, CodeExpression e2)
		{
			return new CodeGreaterEqualThan (e1, e2);
		}
		
		public static CodeExpression IsSmallerEqualThan (CodeExpression e1, CodeExpression e2)
		{
			return new CodeLessEqualThan (e1, e2);
		}
		
		public static CodeExpression Not (CodeExpression e)
		{
			return new CodeNot (e);
		}
		
		public static CodeExpression Add (CodeExpression e1, CodeExpression e2)
		{
			return new CodeAdd (e1, e2);
		}
		
		public static CodeExpression Subtract (CodeExpression e1, CodeExpression e2)
		{
			return new CodeSub (e1, e2);
		}
		
		public static CodeExpression Multiply (CodeExpression e1, CodeExpression e2)
		{
			return new CodeMul (e1, e2);
		}
		
		public static CodeExpression Divide (CodeExpression e1, CodeExpression e2)
		{
			return new CodeDiv (e1, e2);
		}
		
		public CodeExpression CastTo (Type type)
		{
			return new CodeCast (type, this);
		}
		
		public CodeExpression And (CodeExpression other)
		{
			return new CodeAnd (this, other);
		}
		
		public CodeExpression Is (Type type)
		{
			return new CodeIs (type, this);
		}
		
		public CodeExpression Call (string name, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (this, name, parameters);
		}
		
		public CodeExpression Call (MethodInfo method, params CodeExpression[] parameters)
		{
			return new CodeMethodCall (this, method, parameters);
		}
		
		public CodeValueReference MemGet (string name)
		{
			MemberInfo[] mems = GetResultType().GetMember (name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
			if (mems.Length == 0) throw new InvalidOperationException ("Field '" + name + "' not found in " + GetResultType()); 
			return MemGet (mems[0]);
		}
		
		public CodeValueReference MemGet (MemberInfo member)
		{
			if (member is FieldInfo) 
				return new CodeFieldReference (this, (FieldInfo)member);
			else if (member is PropertyInfo)
				return new CodePropertyReference (this, (PropertyInfo)member);
			else
				throw new InvalidOperationException (member.Name + " is not either a field or a property");
		}
		
		public CodeValueReference this [CodeExpression index]
		{
			get { return new CodeArrayItem (this, index); }
		}
		
		public CodeValueReference this [string name]
		{
			get { return MemGet (name); }
		}
		
		public CodeValueReference this [FieldInfo field]
		{
			get { return new CodeFieldReference (this, field); }
		}
		
		public CodeValueReference this [PropertyInfo prop]
		{
			get { return new CodePropertyReference (this, prop); }
		}
		
		public CodeExpression ArrayLength
		{
			get { return new CodeArrayLength (this); }
		}
		
		public CodeExpression IsNull
		{
			get { return new CodeEquals (this, new CodeLiteral (null, this.GetResultType())); }
		}
		
		public static CodeExpression NullValue (Type type)
		{
			return new CodeLiteral (null, type);
		}
		
		public bool IsNumber
		{
			get {
				return CodeGenerationHelper.IsNumber (GetResultType ());
			}
		}
	}
	
	public abstract class CodeConditionExpression: CodeExpression
	{
		public virtual void GenerateForBranch (ILGenerator gen, Label label, bool jumpCase)
		{
			Generate (gen);
			if (jumpCase)
				gen.Emit (OpCodes.Brtrue, label);
			else
				gen.Emit (OpCodes.Brfalse, label);
		}
	}
}
#endif
