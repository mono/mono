//
// Literal.cs
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, 2004 Cesar Lopez Nataren 
// (C) 2005, Novell Inc, (http://novell.com)
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
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using Microsoft.JScript.Vsa;

namespace Microsoft.JScript {

	internal class This : AST {

		internal This (AST parent, Location location)
			: base (parent, location)
		{
		}

		internal override bool Resolve (Environment env)
		{
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			if (InFunction)
				ec.ig.Emit (OpCodes.Ldarg_0);
			else
				CodeGenerator.emit_get_default_this (ec.ig, InFunction);
		}
	}

	internal abstract class Constant : Exp {

		internal Constant (AST parent, Location location)
			: base (parent, location)
		{
		}

		internal override bool Resolve (Environment env)
		{
			return true;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}
	}

	internal class BooleanConstant : Constant, ICanLookupPrototype {
		internal bool Value;

		internal BooleanConstant (AST parent, bool val, Location location)
			: base (parent, location)
		{
			this.Value = val;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (Value)
				ig.Emit (OpCodes.Ldc_I4_1);
			else
				ig.Emit (OpCodes.Ldc_I4_0);
			
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}

		bool ICanLookupPrototype.ResolveFieldAccess (AST ast)
		{
			if (ast is Identifier) {
				Identifier name = (Identifier) ast;
				Type prototype = typeof (NumberPrototype);
				MemberInfo [] members = prototype.GetMember (name.name.Value);
				return members.Length > 0;
			} else
				return false;
		}
	}

	internal abstract class NumericConstant : Constant, ICanLookupPrototype {
		
		internal NumericConstant (AST parent, Location location)
			: base (parent, location)
		{
		}

		bool ICanLookupPrototype.ResolveFieldAccess (AST ast)
		{
			if (ast is Identifier) {
				Identifier name = (Identifier) ast;
				Type prototype = typeof (NumberPrototype);
				MemberInfo [] members = prototype.GetMember (name.name.Value);
				return members.Length > 0;
			} else
				return false;
		}
	}

	internal class ByteConstant : NumericConstant {
		byte Value;
			
		internal ByteConstant (AST parent, byte v, Location location)
			: base (parent, location)
		{
			Value = v;
		}

		internal override void Emit (EmitContext ec)
		{
			IntConstant.EmitInt (ec.ig, Value);
		}
	}

	internal class ShortConstant : NumericConstant {
		short Value;

		internal ShortConstant (AST parent, short v, Location location)
			: base (parent, location)
		{
			Value = v;
		}

		internal override void Emit (EmitContext ec)
		{
			IntConstant.EmitInt (ec.ig, Value);
		}
	}

	internal class IntConstant : NumericConstant {
		int Value;

		internal IntConstant (AST parent, int v, Location location)
			: base (parent, location)
		{
			Value = v;
		}

		static public void EmitInt (ILGenerator ig, int i)
		{
			switch (i){
			case -1:
				ig.Emit (OpCodes.Ldc_I4_M1);
				break;
				
			case 0:
				ig.Emit (OpCodes.Ldc_I4_0);
				break;
				
			case 1:
				ig.Emit (OpCodes.Ldc_I4_1);
				break;
				
			case 2:
				ig.Emit (OpCodes.Ldc_I4_2);
				break;
				
			case 3:
				ig.Emit (OpCodes.Ldc_I4_3);
				break;
				
			case 4:
				ig.Emit (OpCodes.Ldc_I4_4);
				break;
				
			case 5:
				ig.Emit (OpCodes.Ldc_I4_5);
				break;
				
			case 6:
				ig.Emit (OpCodes.Ldc_I4_6);
				break;
				
			case 7:
				ig.Emit (OpCodes.Ldc_I4_7);
				break;
				
			case 8:
				ig.Emit (OpCodes.Ldc_I4_8);
				break;

			default:
				if (i >= -128 && i <= 127){
					ig.Emit (OpCodes.Ldc_I4_S, (sbyte) i);
				} else
					ig.Emit (OpCodes.Ldc_I4, i);
				break;
			}
		}

		internal override void Emit (EmitContext ec)
		{
			EmitInt (ec.ig, Value);
		}
	}

	internal class LongConstant : NumericConstant {
		long Value;

                internal LongConstant (AST parent, long v, Location location)
			: base (parent, location)
                {
                        Value = v;
                }

                internal override void Emit (EmitContext ec)
                {
                        ILGenerator ig = ec.ig;

                        EmitLong (ig, Value);
                }

                static internal void EmitLong (ILGenerator ig, long l)
                {
			ig.Emit (OpCodes.Ldc_I8, l);
                }
	}

	internal class FloatConstant : NumericConstant {
		float Value;

                internal FloatConstant (AST parent, float v, Location location)
			: base (parent, location)
                {
                        Value = v;
                }

                internal override void Emit (EmitContext ec)
                {
                        ec.ig.Emit (OpCodes.Ldc_R4, Value);
                }
	}


        internal class DoubleConstant : NumericConstant {
		double Value;

		internal DoubleConstant (AST parent, double v, Location location)
			: base (parent, location)
                {
                        Value = v;
                }

		internal override void Emit (EmitContext ec)
                {
                        ec.ig.Emit (OpCodes.Ldc_R8, Value);
                }
	}

	internal class ObjectLiteral : Exp {
		
		internal ArrayList elems;
		
		internal ObjectLiteral (ArrayList elems, Location location)
			: base (null, location)
		{
			this.elems = elems;
		}

		internal override bool Resolve (Environment env, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (env);
		}

		internal override bool Resolve (Environment env)
		{
			bool r = true;
			foreach (AST ast in elems)
				r &= ast.Resolve (env);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, typeof (ScriptObject).GetField ("engine"));
			ig.Emit (OpCodes.Call, typeof (Microsoft.JScript.Vsa.VsaEngine).GetMethod ("GetOriginalObjectConstructor"));
			ig.Emit (OpCodes.Call, typeof (ObjectConstructor).GetMethod ("ConstructObject"));

			foreach (ObjectLiteralItem item in elems) {
				ig.Emit (OpCodes.Dup);
				item.Emit (ec);
				ig.Emit (OpCodes.Call, typeof (JSObject).GetMethod ("SetMemberValue2"));
			}
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}

		internal void Add (ObjectLiteralItem item)
		{
			elems.Add (item);
		}
	}

	internal class ObjectLiteralItem : AST {
		internal string property_name;
		internal AST exp;

		internal ObjectLiteralItem (object obj)
			: base (null, null)
		{
			if (obj != null)
				property_name = obj.ToString ();
		}

		internal override bool Resolve (Environment env)
		{
			return exp.Resolve (env);
		}

		internal override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldstr, property_name);
			exp.Emit (ec);
			CodeGenerator.EmitBox (ec.ig, exp);
		}
	}

	internal class PropertyName {
		string name;
		internal string Name {
			get { return name; }
			set { name = value; }
		}
	}

	internal class RegExpLiteral : AST {
		internal string re;
		internal string flags;

		const char IGNORE_CASE = 'i';
		const char GLOBAL = 'g';
		const char MULTI_LINE = 'm';
		
		internal RegExpLiteral (AST parent, string re, string flags, Location location)
			: base (parent, location)
		{
			this.re = re;
			this.flags = flags;
		}

		internal override bool Resolve (Environment env)
		{
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			TypeBuilder type = ec.type_builder;

			FieldBuilder field = type.DefineField (SemanticAnalyser.NextAnonymousRegExpObj, typeof (RegExpObject), FieldAttributes.Public | FieldAttributes.Static);

			Label label = ig.DefineLabel ();

			ig.Emit (OpCodes.Ldsfld, field);
			ig.Emit (OpCodes.Brtrue, label);

			CodeGenerator.load_engine (InFunction, ig);
			
			ig.Emit (OpCodes.Call, typeof (VsaEngine).GetMethod ("GetOriginalRegExpConstructor"));
			ig.Emit (OpCodes.Ldstr, re);

			emit_flag (ig, flags.IndexOfAny (new char [] {IGNORE_CASE}) > -1);
			emit_flag (ig, flags.IndexOfAny (new char [] {GLOBAL}) > -1);
			emit_flag (ig, flags.IndexOfAny (new char [] {MULTI_LINE}) > -1);
			
			ig.Emit (OpCodes.Call, typeof (RegExpConstructor).GetMethod ("Construct"));
			ig.Emit (OpCodes.Castclass, typeof (RegExpObject));
			ig.Emit (OpCodes.Stsfld, field);

			ig.MarkLabel (label);			
			ig.Emit (OpCodes.Ldsfld, field);
		}

		void emit_flag (ILGenerator ig, bool cond)
		{
 			if (cond)
				ig.Emit (OpCodes.Ldc_I4_1);
			else
				ig.Emit (OpCodes.Ldc_I4_0);
		}
	}		
}
