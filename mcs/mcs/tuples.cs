//
// tuples.cs: Tuples types
//
// Author:
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright (C) Microsoft Corporation.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

#if STATIC
using MetaType = IKVM.Reflection.Type;
using IKVM.Reflection;
using IKVM.Reflection.Emit;
#else
using MetaType = System.Type;
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace Mono.CSharp
{
	class TupleTypeExpr : TypeExpr
	{
		TypeArguments elements;
		List<string> names;

		public TupleTypeExpr (TypeArguments elements, List<string> names, Location loc)
		{
			this.elements = elements;
			this.names = names;
			this.loc = loc;
		}

		public override TypeSpec ResolveAsType (IMemberContext mc, bool allowUnboundTypeArguments = false)
		{
			var length = elements.Count;
			if (length > 7)
				throw new NotImplementedException ("tuples > 7");

			eclass = ExprClass.Type;

			var otype = mc.Module.PredefinedTypes.Tuples [length - 1].Resolve ();
			if (otype == null)
				return null;

			GenericTypeExpr ctype = new GenericTypeExpr (otype, elements, loc);
			type = ctype.ResolveAsType (mc);

			if (names != null && CheckElementNames (mc) && type != null) {
				type = NamedTupleSpec.MakeType (mc.Module, (InflatedTypeSpec) type, names);
			}

			return type;
		}

		bool CheckElementNames (IMemberContext mc)
		{
			int first_name = -1;
			for (int i = 0; i < names.Count; ++i) {
				var name = names [i];
				if (name == null)
					continue;

				if (IsReservedName (name)) {
					mc.Module.Compiler.Report.Error (8126, loc, "The tuple element name `{0}' is reserved", name);
					names [i] = null;
					continue;
				}

				if (name.StartsWith ("Item", StringComparison.Ordinal)) {
					var idx = name.Substring (4);
					uint value;
					if (uint.TryParse (idx, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value != i + 1) {
						mc.Module.Compiler.Report.Error (8125, loc, "The tuple element name `{0}' can only be used at position {1}", name, idx);
						names [i] = null;
						continue;
					}
				}

				if (first_name < 0) {
					first_name = i;
					continue;
				}

				for (int ii = first_name; ii < i; ++ii) {
					if (name == names [ii]) {
						mc.Module.Compiler.Report.Error (8127, loc, "The tuple element name `{0}' is a duplicate", name);
						names [i] = null;
						break;
					}
				}
			}

			return first_name >= 0;
		}

		static bool IsReservedName (string name)
		{
			switch (name) {
			case "CompareTo":
			case "Deconstruct":
			case "Equals":
			case "GetHashCode":
			case "Rest":
			case "ToString":
				return true;
			default:
				return false;
			}
		}

		public override string GetSignatureForError ()
		{
			var sb = new StringBuilder ();
			for (int i = 0; i < elements.Count; ++i) {
				sb.Append (elements.Arguments [i].GetSignatureForError ());

				if (names [i] != null) {
					sb.Append (" ");
					sb.Append (names [i]);
				}

				if (i != 0)
					sb.Append (",");
			}

			return sb.ToString ();
		}
	}

	public class NamedTupleSpec : TypeSpec
	{
		InflatedTypeSpec tuple;
		IList<string> elements;

		private NamedTupleSpec (InflatedTypeSpec tupleDefinition, IList<string> elements)
			: base (tupleDefinition.Kind, tupleDefinition.DeclaringType, tupleDefinition.MemberDefinition, null, tupleDefinition.Modifiers)
		{
			tuple = tupleDefinition;
			this.elements = elements;

			state |= StateFlags.HasNamedTupleElement | StateFlags.Tuple;
		}

		public IList<string> Elements {
			get {
				return elements;
			}
		}

		public override TypeSpec [] TypeArguments {
			get {
				return tuple.TypeArguments;
			}
		}

		protected override void InitializeMemberCache (bool onlyTypes)
		{
			cache = tuple.MemberCache;
		}

		public override MetaType GetMetaInfo ()
		{
			return tuple.GetMetaInfo ();
		}

		public MemberSpec FindElement (IMemberContext mc, string name, Location loc)
		{
			// TODO: cache it
			for (int i = 0; i < elements.Count; ++i) {
				var ename = elements [i];
				if (ename == null || ename != name)
					continue;

				var member_name = GetElementPropertyName (i);
				var ms = MemberCache.FindMember (tuple, MemberFilter.Field (member_name, null), BindingRestriction.DeclaredOnly | BindingRestriction.InstanceOnly);
				if (ms == null) {
					mc.Module.Compiler.Report.Error (8128, loc, "Member `{0}' was not found on type '{1}'", member_name, tuple.GetSignatureForError ());
					return null;
				}

				return ms;
			}

			return null;
		}

		public override string GetSignatureForError ()
		{
			//
			// csc reports names as well but it seems to me redundant when
			// they are not included in any type conversion
			//
			return tuple.GetSignatureForError ();
		}

		public string GetSignatureForErrorWithNames ()
		{
			// TODO: Include names
			return tuple.GetSignatureForError ();
		}

		public static NamedTupleSpec MakeType (ModuleContainer module, InflatedTypeSpec tupleType, IList<string> names)
		{
			// TODO: cache it
			return new NamedTupleSpec (tupleType, names);
		}

		public static string GetElementPropertyName (int index)
		{
			return "Item" + (index + 1).ToString (CultureInfo.InvariantCulture);
		}

		public static bool CheckOverrideName (IParametersMember member, IParametersMember baseMember)
		{
			var btypes = baseMember.Parameters.Types;
			var ttypes = member.Parameters.Types;
			for (int ii = 0; ii < baseMember.Parameters.Count; ++ii) {
				if (!CheckOverrideName (ttypes [ii], btypes [ii])) {
					return false;
				}
			}

			return true;
		}
	
		public static bool CheckOverrideName (TypeSpec type, TypeSpec baseType)
		{
			var btype_ntuple = baseType as NamedTupleSpec;
			var mtype_ntuple = type as NamedTupleSpec;
			if (btype_ntuple == null && mtype_ntuple == null)
				return true;

			if (btype_ntuple == null || mtype_ntuple == null)
				return false;

			var b_elements = btype_ntuple.elements;
			var m_elements = mtype_ntuple.elements;
			for (int i = 0; i < b_elements.Count; ++i) {
				if (b_elements [i] != m_elements [i])
					return false;
			}

			return true;
		}
	}

	class TupleLiteralElement
	{
		public TupleLiteralElement (string name, Expression expr, Location loc)
		{
			this.Name = name;
			this.Expr = expr;
			this.Location = loc;
		}

		public TupleLiteralElement (Expression expr)
		{
			this.Expr = expr;
			this.Location = expr.Location;
		}

		public TupleLiteralElement Clone (CloneContext clonectx)
		{
			return new TupleLiteralElement (Name, Expr.Clone (clonectx), Location);
		}

		public string Name { get; private set; }
		public Expression Expr { get; set; }
		public Location Location { get; private set; }
	}

	sealed class TupleLiteral : Expression
	{
		List<TupleLiteralElement> elements;

		public TupleLiteral (List<TupleLiteralElement> elements, Location loc)
		{
			this.elements = elements;
			this.loc = loc;
		}

		public List<TupleLiteralElement> Elements {
			get {
				return elements;
			}
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			var clone = new List<TupleLiteralElement> (elements.Count);
			foreach (var te in elements)
				clone.Add (te.Clone (clonectx));

			TupleLiteral target = (TupleLiteral)t;
			target.elements = clone;
		}

		public static bool ContainsNoTypeElement (TypeSpec type)
		{
			var ta = type.TypeArguments;

			for (int i = 0; i < ta.Length; ++i) {
				var et = ta [i];
				if (InternalType.HasNoType (et))
					return true;

				if (et.IsTupleType && ContainsNoTypeElement (et))
					return true;
			}

			return false;
		}

		public override Expression CreateExpressionTree (ResolveContext rc)
		{
			rc.Report.Error (8143, loc, "An expression tree cannot contain a tuple literal");
			return ErrorExpression.Instance;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			var ta = new TypeArguments ();
			List<string> names = null;

			for (int i = 0; i < elements.Count; ++i) {
				var el = elements [i];
				var expr = el.Expr.Resolve (rc);
				if (expr == null) {
					el.Expr = null;
					ta = null;
					continue;
				}

				if (expr.Type.Kind == MemberKind.Void) {
					rc.Report.Error (8210, expr.Location, "A tuple literal cannot not contain a value of type `{0}'", expr.Type.GetSignatureForError ());
					expr = null;
					ta = null;
					continue;
				}

				if (el.Name != null) {
					if (names == null) {
						names = new List<string> ();
						for (int ii = 0; ii < i; ++ii) {
							names.Add (null);
						}
					}

					names.Add (el.Name);
				}

				el.Expr = expr;

				if (ta != null)
					ta.Add (new TypeExpression (expr.Type, expr.Location));
			}

			eclass = ExprClass.Value;

			if (ta == null)
				return null;

			var t = new TupleTypeExpr (ta, names, loc);
			type = t.ResolveAsType (rc) ?? InternalType.ErrorType;

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			foreach (var el in elements) {
				el.Expr.Emit (ec);
			}

			// TODO: Needs arguments check
			var ctor = MemberCache.FindMember (type, MemberFilter.Constructor (null), BindingRestriction.DeclaredOnly | BindingRestriction.InstanceOnly) as MethodSpec;

			ec.Emit (OpCodes.Newobj, ctor);
		}

		public override void Error_ValueCannotBeConverted (ResolveContext rc, TypeSpec target, bool expl)
		{
			rc.Report.Error (8135, Location, "Tuple literal `{0}' cannot be converted to type `{1}'", type.GetSignatureForError (), target.GetSignatureForError ());
		}
	}

	//
	// Used when converting from a tuple literal or tuple instance to different tuple type
	//
	class TupleLiteralConversion : Expression
	{
		List<Expression> elements;
		Expression source;

		public TupleLiteralConversion (Expression source, TypeSpec type, List<Expression> elements, Location loc)
		{
			this.source = source;
			this.type = type;
			this.elements = elements;
			this.loc = loc;

			eclass = source.eclass;
		}

		public override Expression CreateExpressionTree (ResolveContext rc)
		{
			rc.Report.Error (8144, loc, "An expression tree cannot contain a tuple conversion");
			return null;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			// Should not be reached
			throw new NotSupportedException ();
		}

		public override void Emit (EmitContext ec)
		{
			if (!(source is TupleLiteral)) {
				var assign = source as CompilerAssign;
				if (assign != null)
					assign.EmitStatement (ec);
				else
					source.Emit (ec);
			}
			
			foreach (var el in elements) {
				el.Emit (ec);
			}

			// TODO: Needs arguments check
			var ctor = MemberCache.FindMember (type, MemberFilter.Constructor (null), BindingRestriction.DeclaredOnly | BindingRestriction.InstanceOnly) as MethodSpec;

			ec.Emit (OpCodes.Newobj, ctor);
		}
	}

	class TupleDeconstruct : ExpressionStatement, IAssignMethod
	{
		Expression source;
		List<Expression> targetExprs;
		List<Expression> tempExprs;
		List<BlockVariable> variables;
		Expression instance;

		public TupleDeconstruct (List<Expression> targetExprs, Expression source, Location loc)
		{
			this.source = source;
			this.targetExprs = targetExprs;
			this.loc = loc;

			tempExprs = new List<Expression> ();
		}

		public TupleDeconstruct (List<BlockVariable> variables, Expression source, Location loc)
		{
			this.source = source;
			this.variables = variables;
			this.loc = loc;

			tempExprs = new List<Expression> ();
		}

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			ec.Report.Error (832, loc, "An expression tree cannot contain an assignment operator");
			
			throw new NotImplementedException ();
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			var src = source.Resolve (rc);
			if (src == null)
				return null;
			
			if (InternalType.HasNoType (src.Type)) {
				rc.Report.Error (8131, source.Location, "Deconstruct assignment requires an expression with a type on the right-hand-side");
				return null;
			}

			var src_type = src.Type;

			if (src_type.IsTupleType) {
				int target_count;

				if (targetExprs == null) {
					target_count = variables.Count;
					targetExprs = new List<Expression> (target_count);
				} else {
					target_count = targetExprs.Count;
				}

				if (src_type.Arity != target_count) {
					rc.Report.Error (8132, loc, "Cannot deconstruct a tuple of `{0}' elements into `{1}' variables",
						src_type.Arity.ToString (CultureInfo.InvariantCulture), target_count.ToString (CultureInfo.InvariantCulture));
					return null;
				}

				var tupleLiteral = src as TupleLiteral;
				if (tupleLiteral == null && !ExpressionAnalyzer.IsInexpensiveLoad (src)) {
					var expr_variable = LocalVariable.CreateCompilerGenerated (source.Type, rc.CurrentBlock, loc);
					source = new CompilerAssign (expr_variable.CreateReferenceExpression (rc, loc), source, loc);
					instance = expr_variable.CreateReferenceExpression (rc, loc);
				}

				var element_srcs = new List<Expression> ();
				var src_names = new List<string> ();
				for (int i = 0; i < target_count; ++i) {
					var element_src = tupleLiteral == null ? new MemberAccess (instance, NamedTupleSpec.GetElementPropertyName (i)) : tupleLiteral.Elements [i].Expr;
					element_srcs.Add (element_src);
					if (element_src is VariableReference)
						src_names.Add ((element_src as VariableReference)?.Name);
				}

				for (int i = 0; i < target_count; ++i) {
					var tle = src_type.TypeArguments [i];

					if (variables != null) {
						var variable = variables [i].Variable;

						if (variable.Type == InternalType.Discard) {
							variables [i] = null;
							targetExprs.Add (EmptyExpressionStatement.Instance);
							continue;
						}

						var variable_type = variables [i].TypeExpression;

						targetExprs.Add (new LocalVariableReference (variable, variable.Location));

						if (variable_type is VarExpr) {
							if (InternalType.HasNoType (tle)) {
								rc.Report.Error (8130, Location, "Cannot infer the type of implicitly-typed deconstruction variable `{0}'", variable.Name);
								tle = InternalType.ErrorType;
							}

							variable.Type = tle;
						} else {
							variable.Type = variable_type.ResolveAsType (rc);
						}

						variable.PrepareAssignmentAnalysis ((BlockContext)rc);
					}

					var element_target = (targetExprs [i] as SimpleName)?.LookupNameExpression (rc, MemberLookupRestrictions.None);

					if (element_target != null && src_names.Contains ((element_target as VariableReference)?.Name)) {
						var tempType = element_target.Resolve (rc).Type;

						var temp = new LocalTemporary (tempType);
						tempExprs.Add (new SimpleAssign (temp, element_srcs [i]).Resolve (rc));
						targetExprs [i] = new SimpleAssign (targetExprs [i], temp).Resolve (rc);
					} else {
						targetExprs [i] = new SimpleAssign (targetExprs [i], element_srcs [i]).Resolve (rc);
					}
				}

				eclass = ExprClass.Value;

				// TODO: The type is same only if there is no target element conversion
				// var res = (/*byte*/ b, /*short*/ s) = (2, 4);
				type = src.Type;
				return this;
			}

			if (src_type.BuiltinType == BuiltinTypeSpec.Type.Dynamic) {
				rc.Report.Error (8133, loc, "Cannot deconstruct dynamic objects");
				return null;
			}

			/*
			var args = new Arguments (targetExprs.Count);
			foreach (var t in targetExprs) {
				args.Add (new Argument (t, Argument.AType.Out));
			}

			var invocation = new Invocation (new MemberAccess (src, "Deconstruct"), args);
			var res = invocation.Resolve (rc);
			*/

			throw new NotImplementedException ("Custom deconstruct");
		}

		public override void Emit (EmitContext ec)
		{
			if (instance != null)
				((ExpressionStatement)source).EmitStatement (ec);

			foreach (ExpressionStatement expr in tempExprs) {
				var temp = (expr as Assign)?.Target as LocalTemporary;
				if (temp == null)
					continue;

				temp.AddressOf (ec, AddressOp.LoadStore);
				ec.Emit (OpCodes.Initobj, temp.Type);
				expr.Emit (ec);
			}

			foreach (ExpressionStatement expr in targetExprs) {
				expr.Emit (ec);

				var temp = (expr as Assign)?.Source as LocalTemporary;
				if (temp != null)
					temp.Release (ec);
			}

			var ctor = MemberCache.FindMember (type, MemberFilter.Constructor (null), BindingRestriction.DeclaredOnly | BindingRestriction.InstanceOnly) as MethodSpec;
			ec.Emit (OpCodes.Newobj, ctor);
		}

		public override void EmitStatement (EmitContext ec)
		{
			if (variables != null) {
				foreach (var lv in variables) {
					lv?.Variable.CreateBuilder (ec);
				}
			}
			
			if (instance != null)
				((ExpressionStatement) source).EmitStatement (ec);

			foreach (ExpressionStatement expr in tempExprs) {
				var temp = (expr as Assign)?.Target as LocalTemporary;
				if (temp == null)
					continue;

				temp.AddressOf (ec, AddressOp.LoadStore);
				ec.Emit (OpCodes.Initobj, temp.Type);
				expr.EmitStatement (ec);
			}

			foreach (ExpressionStatement expr in targetExprs) {
				expr.EmitStatement (ec);

				var temp = (expr as Assign)?.Source as LocalTemporary;
				if (temp != null)
					temp.Release (ec);
			}
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			if (leave_copy)
				throw new NotImplementedException ();

			EmitStatement (ec);
		}

		public override void FlowAnalysis (FlowAnalysisContext fc)
		{
			source.FlowAnalysis (fc);
			foreach (var expr in targetExprs)
				expr.FlowAnalysis (fc);
		}

		public void SetGeneratedFieldAssigned (FlowAnalysisContext fc)
		{
			if (variables == null)
				return;

			foreach (var lv in variables)
				fc.SetVariableAssigned (lv.Variable.VariableInfo);
		}
	}
}