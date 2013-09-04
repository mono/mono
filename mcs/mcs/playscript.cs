//
// playscript.cs: PlayScript expressions and support
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or Apache License, Version 2.0
//
// Copyright 2013 Zynga Inc.
// Copyright 2013 Xamarin Inc
//

using System;
using System.Collections.Generic;
using Mono.CSharp;
using SLE = System.Linq.Expressions;

#if STATIC
using IKVM.Reflection;
using IKVM.Reflection.Emit;
#else
using System.Reflection;
using System.Reflection.Emit;
#endif

namespace Mono.PlayScript
{
	public abstract class PlayScriptExpression : Expression
	{
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ("Expression trees conversion not implemented in PlayScript");
		}
	}

	static class ExpressionExtension
	{
		public static TypeSpec TryToResolveAsType (this Expression expr, IMemberContext mc)
		{
			//
			// TODO: Don't have probing type resolver yet
			//
			var errors_printer = new SessionReportPrinter ();
			var old = mc.Module.Compiler.Report.SetPrinter (errors_printer);
			TypeSpec t;

			try {
				t = expr.ResolveAsType (mc);
			} finally {
				mc.Module.Compiler.Report.SetPrinter (old);
			}

			if (t != null && errors_printer.ErrorsCount == 0)
				return t;

			return null;
		}
	}

	public class ExpressionSeries : PlayScriptExpression
	{
		public ExpressionSeries (List<Expression> expressions, Location loc)
		{
			Expressions = expressions;
			this.loc = loc;
		}

		public List<Expression> Expressions { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			int i;
			for (i = 0; i < Expressions.Count; ++i) {
				Expressions[i] = Expressions[i].Resolve (rc);
			}

			--i;
			type = Expressions[i].Type;
			eclass = Expressions[i].eclass;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			int i;
			for (i = 0; i < Expressions.Count - 1; ++i) {
				var expr = Expressions [i];
				if (expr.IsSideEffectFree)
					continue;

				var expr_stmt = expr as ExpressionStatement;
				if (expr_stmt != null) {
					expr_stmt.EmitStatement (ec);
					continue;
				}

				expr.Emit (ec);
				ec.Emit (OpCodes.Pop);
			}

			Expressions[i].Emit (ec);
		}
	}

	public abstract class CollectionInitialization : PlayScriptExpression
	{
		protected Expression ctor;
		protected TemporaryVariableReference instance;
		protected List<Invocation> inserts;

		protected CollectionInitialization (ArrayInitializer initializer)
		{
			this.Initializer = initializer;
			loc = Initializer.Location;
		}

		public ArrayInitializer Initializer { get; private set; }

		protected List<Invocation> ResolveInitializations (ResolveContext rc, Expression instance, MethodSpec pushMethod)
		{
			List<Invocation> all = new List<Invocation> (Initializer.Count);
			foreach (var expr in Initializer.Elements) {

				var call_args = new Arguments (1);
				call_args.Add (new Argument (expr));

				var mg = MethodGroupExpr.CreatePredefined (pushMethod, rc.CurrentType, loc);
				mg.InstanceExpression = instance;

				var inv = new Invocation (mg, call_args);
				inv.Resolve (rc);

				all.Add (inv);
			}

			return all;
		}

		public override void Emit (EmitContext ec)
		{
			if (instance != null) {
				instance.EmitAssign (ec, ctor);
				foreach (var insert in inserts)
					insert.EmitStatement (ec);

				instance.EmitLoad (ec);
			} else {
				ctor.Emit (ec);
			}
		}
	}

	public class ArrayCreation : CollectionInitialization
	{
		public ArrayCreation (ArrayInitializer initializer)
			: base (initializer)
		{
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			type = rc.Module.PlayscriptTypes.Array.Resolve ();
			if (type == null)
				return null;

			var count = Initializer.Elements == null ? 0 : Initializer.Count;

			var ctor_args = new Arguments (1);
			ctor_args.Add (new Argument (new IntLiteral (rc.BuiltinTypes, count, loc)));

			ctor = new New (new TypeExpression (type, loc), ctor_args, loc).Resolve (rc);

			if (count != 0) {
				instance = TemporaryVariableReference.Create (type, rc.CurrentBlock, loc);

				var push = rc.Module.PlayScriptMembers.ArrayPush.Resolve (loc);
				if (push == null)
					return null;

				inserts = ResolveInitializations (rc, instance, push);
			}

			eclass = ExprClass.Value;
			return this;
		}


		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	class BinaryOperators
	{
		public static Expression ResolveOperator (ResolveContext rc, Binary op, Expression left, Expression right)
		{
			string method = "Comparison";
			string oper;
			switch (op.Oper) {
			case Binary.Operator.Equality:
				oper = "Equality";
				break;
			case Binary.Operator.Inequality:
				oper = "Inequality";
				break;
			case Binary.Operator.GreaterThan:
				oper = "GreaterThan";
				break;
			case Binary.Operator.GreaterThanOrEqual:
				oper = "GreaterThanOrEqual";
				break;
			case Binary.Operator.LessThan:
				oper = "LessThan";
				break;
			case Binary.Operator.LessThanOrEqual:
				oper = "LessThanOrEqual";
				break;
			case Binary.Operator.StrictEquality:
				oper = "StrictEquality";
				break;
			case Binary.Operator.StrictInequality:
				oper = "StrictInequality";
				break;
			default:
				throw new NotImplementedException ();
			}

			var loc = op.Location;

			var ps = new MemberAccess (new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "PlayScript", loc), "Runtime", loc);

			var args = new Arguments (3);
			args.Add (new Argument (new MemberAccess (new MemberAccess (ps, "BinaryOperator", loc), oper, loc)));
			args.Add (new Argument (left));
			args.Add (new Argument (right));


			//
			// ActionScript does not really care about types for this for example following cases are all valid
			// 1.0 == 1
			// "3" > null
			// We defer to runtime to do the complex coercion
			//
			return new Invocation (new MemberAccess (new TypeExpression (rc.Module.PlayscriptTypes.Operations.Resolve (), loc), method, loc), args).Resolve (rc);
		}
	}

	public class New : CSharp.New
	{
		public New (Expression requestedType, Arguments arguments, Location loc)
			: base (requestedType, arguments, loc)
		{
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			var te = RequestedType.TryToResolveAsType (rc);
			if (te != null) {
				return base.DoResolve (rc);
			}

			var expr = RequestedType.Resolve (rc);
			if (expr == null)
				return null;

			if (expr.Type != rc.Module.PlayscriptTypes.Class && expr.Type != rc.Module.PlayscriptTypes.Object) {
				rc.Report.ErrorPlayScript (1180, RequestedType.Location, "Call to a possibly undefined method `{0}'", expr.GetSignatureForError ());
				return null;
			}

			if (arguments == null) {
				arguments = new Arguments (1);
			} else {
				bool dynamic;
				arguments.Resolve (rc, out dynamic);
			}

			var ms = rc.Module.PlayScriptMembers.OperationsClassOf.Resolve (loc);
			if (ms == null)
				return null;

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (1);
			call_args.Add (new Argument (expr));

			arguments.Insert (0, new Argument (new Invocation (mg, call_args).Resolve (rc), Argument.AType.DynamicTypeName));
			return new DynamicConstructorBinder (type, arguments, loc).Resolve (rc);
		}
	}

	public class NewVector : CollectionInitialization
	{
		FullNamedExpression elementType;

		public NewVector (FullNamedExpression elementType, ArrayInitializer initializer, Location loc)
			: base (initializer)
		{
			this.elementType = elementType;
			this.loc = loc;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			var element = elementType.ResolveAsType (rc);
			type = rc.Module.PlayscriptTypes.Vector.Resolve ();
			if (type == null || element == null)
				return null;

			type = type.MakeGenericType (rc, new [] { element });

			var count = Initializer.Elements == null ? 0 : Initializer.Count;

			var ctor_args = new Arguments (1);
			ctor_args.Add (new Argument (new IntLiteral (rc.BuiltinTypes, count, loc)));

			ctor = new New (new TypeExpression (type, loc), ctor_args, loc).Resolve (rc);

			if (count != 0) {
				instance = TemporaryVariableReference.Create (type, rc.CurrentBlock, loc);

				var push = rc.Module.PlayScriptMembers.VectorPush.Resolve (loc);
				if (push == null)
					return null;

				push = MemberCache.GetMember (type, push);
	
				inserts = ResolveInitializations (rc, instance, push);
			}

			eclass = ExprClass.Value;
			return this;
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class UntypedTypeExpression : TypeExpr
	{
		public UntypedTypeExpression (Location loc)
		{
			this.loc = loc;
		}

		public override string GetSignatureForError ()
		{
			return "*";
		}

		public override TypeSpec ResolveAsType (IMemberContext mc)
		{
			//
			// An untyped variable is not the same as a variable of type Object.
			// The key difference is that untyped variables can hold the special value
			// undefined, while a variable of type Object cannot hold that value,
			// any conversion is done at runtime.
			//
			return mc.Module.PlayscriptTypes.UndefinedType;
		}
	}

	public class UntypedBlockVariable : BlockVariable
	{
		public UntypedBlockVariable (LocalVariable li, Location loc)
			: base (li, loc)
		{
		}

		public new FullNamedExpression TypeExpression {
			get {
				return type_expr;
			}
			set {
				type_expr = value;
			}
		}

		public override TypeSpec ResolveType (BlockContext bc)
		{
			if (type_expr == null) {
				if (Initializer is LocalFunction)
					type_expr = new TypeExpression (bc.Module.PlayscriptTypes.Function, loc);
				else
					type_expr = new UntypedTypeExpression (loc);
			}

			return base.ResolveType (bc);
		}
	}

	public class ObjectInitializer : PlayScriptExpression
	{
		public ObjectInitializer (List<Expression> initializer, Location loc)
		{
			Initializer = initializer;
			this.loc = loc;
		}

		public List<Expression> Initializer { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			var type = new TypeExpression (rc.Module.PlayscriptTypes.Object, loc);

			var expr = Initializer == null ?
				new CSharp.New (type, null, loc) :
				new NewInitialize (type, null, new CollectionOrObjectInitializers (Initializer, loc), loc);
			
			return expr.Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class SuperBaseInitializer : CSharp.ConstructorBaseInitializer
	{
		public SuperBaseInitializer (Arguments args, Location loc)
			: base (args, loc)
		{
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			// TODO: PS1201: A super statement cannot occur after a this, super, return, or throw statement.

			return base.DoResolve (ec);
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class Catch : CSharp.Catch
	{
		public Catch (Block block, Location loc)
			: base (block, loc)
		{
		}

		public override bool IsGeneral {
			get {
				return false;
			}
		}

		protected override bool ResolveType (BlockContext bc)
		{
			if (TypeExpression == null) {
				type = bc.Module.PlayscriptTypes.Error.Resolve ();
				return true;
			}

			type = TypeExpression.ResolveAsType (bc);
			if (type == null)
				return false;

			// TODO: It's probably incorrect
			if (type == bc.Module.PlayscriptTypes.UndefinedType) {
				type = bc.Module.PlayscriptTypes.Error.Resolve ();
				return true;
			}

			if (!TypeSpec.IsBaseClass (type, bc.BuiltinTypes.Exception, false)) {
				// TODO: Somehow need to wrap user type to Exception

				throw new NotImplementedException ();
			}

			return true;
		}
	}

	public class Delete : ExpressionStatement
	{
		public Delete (Expression expr, Location l)
		{
			this.Expression = expr;
			loc = l;
		}

		public Expression Expression { get; private set; }

		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotImplementedException ("Expression trees conversion not implemented in PlayScript");
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			var expr = Expression.Resolve (rc);

			var dcma = expr as DynamicClassMemberAccess;
			if (dcma != null) {
				var ms = rc.Module.PlayScriptMembers.BinderDeleteProperty.Resolve (loc);
				if (ms == null)
					return null;

				var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
				var call_args = new Arguments (2);
				call_args.Add (new Argument (dcma.Instance));
				call_args.Add (dcma.Arguments [0]);

				return new Invocation (mg, call_args).Resolve (rc);
			}

			//
			// Fixed properties cannot be deleted but it can be used with delete operator
			//
			var pe = expr as PropertyExpr;
			if (pe != null) {
				rc.Report.WarningPlayScript (3601, loc, "The declared property `{0}' cannot be deleted. To free associated memory, set its value to null",
					pe.GetSignatureForError ());

				expr = new BoolConstant (rc.BuiltinTypes, false, loc);
				return expr.Resolve (rc);
			}

			Expression = expr;
			eclass = ExprClass.Value;
			type = rc.BuiltinTypes.Bool;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Expression.Emit (ec);

			// Always returns true
			ec.EmitInt (1);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Expression.Emit (ec);
			if (Expression.Type.Kind != MemberKind.Void)
				ec.Emit (OpCodes.Pop);
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class TypeOf : PlayScriptExpression
	{		
		public TypeOf (Expression expr, Location l)
		{
			Expression = expr;
			loc = l;
		}

		public Expression Expression { get; private set; }
				
		protected override Expression DoResolve (ResolveContext rc)
		{
			var expr = Expression.Resolve (rc);
			if (expr == null)
				return null;

			var ms = rc.Module.PlayScriptMembers.OperationsTypeOf.Resolve (loc);
			if (ms == null)
				return null;

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (1);
			call_args.Add (new Argument (expr));

			return new Invocation (mg, call_args).Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class RestArrayParameter : Parameter
	{
		PredefinedAttribute attr;

		public RestArrayParameter (string name, Attributes attrs, Location loc)
			: base (null, name, Modifier.RestArray, attrs, loc)
		{
		}

		public override void ApplyAttributes (MethodBuilder mb, ConstructorBuilder cb, int index, CSharp.PredefinedAttributes pa)
		{
			base.ApplyAttributes (mb, cb, index, pa);

			attr.EmitAttribute (builder);
		}

		public override string GetSignatureForError ()
		{
			return GetModifierSignature (ModFlags);
		}

		public override TypeSpec Resolve (IMemberContext rc, int index)
		{
			TypeExpression = new TypeExpression (rc.Module.PlayscriptTypes.Array.Resolve (), Location);
			attr = rc.Module.PlayscriptAttributes.RestArrayParameter;
			attr.Define ();

			return base.Resolve (rc, index);
		}
	}

	public class RegexLiteral : Constant, ILiteralConstant
	{
		readonly public string Regex;
		readonly public string Options;

		public RegexLiteral (string regex, string options, Location loc)
			: base (loc)
		{
			Regex = regex;
			Options = options ?? "";
		}

		public override bool IsLiteral {
			get { return true; }
		}

		public override object GetValue ()
		{
			return "/" + Regex + "/" + Options;
		}
		
		public override string GetValueAsLiteral ()
		{
			return GetValue () as String;
		}
		
		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}

		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			throw new NotSupportedException ();
		}
		
		public override bool IsDefaultValue {
			get {
				return Regex == null && Options == "";
			}
		}
		
		public override bool IsNegative {
			get {
				return false;
			}
		}
		
		public override bool IsNull {
			get {
				return IsDefaultValue;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			return null;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
/*
			if (rc.Target == Target.JavaScript) {
				type = rc.Module.PredefinedTypes.AsRegExp.Resolve();
				eclass = ExprClass.Value;
				return this;
			}
*/
			var args = new Arguments(2);
			args.Add (new Argument(new StringLiteral(rc.BuiltinTypes, Regex, this.Location)));
			args.Add (new Argument(new StringLiteral(rc.BuiltinTypes, Options, this.Location)));

			return new New(new TypeExpression(rc.Module.PlayscriptTypes.RegExp.Resolve(), this.Location), 
			               args, this.Location).Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
/*
		public override void EmitJs (JsEmitContext jec)
		{
			jec.Buf.Write (GetValue () as String, Location);
		}
*/
#if FULL_AST
		public char[] ParsedValue { get; set; }
#endif

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class XmlLiteral : Constant, ILiteralConstant
	{
		readonly public string Xml;

		public XmlLiteral (string xml, Location loc)
			: base (loc)
		{
			Xml = xml;
		}
		
		public override bool IsLiteral {
			get { return true; }
		}
		
		public override object GetValue ()
		{
			return Xml;
		}
		
		public override string GetValueAsLiteral ()
		{
			return GetValue () as String;
		}
		
		public override long GetValueAsLong ()
		{
			throw new NotSupportedException ();
		}
		
		public override void EncodeAttributeValue (IMemberContext rc, AttributeEncoder enc, TypeSpec targetType)
		{
			throw new NotSupportedException ();
		}
		
		public override bool IsDefaultValue {
			get {
				return Xml == null;
			}
		}
		
		public override bool IsNegative {
			get {
				return false;
			}
		}
		
		public override bool IsNull {
			get {
				return IsDefaultValue;
			}
		}

		public override Constant ConvertExplicitly (bool in_checked_context, TypeSpec target_type)
		{
			return null;
		}
		
		protected override Expression DoResolve (ResolveContext rc)
		{
			var args = new Arguments(1);
			args.Add (new Argument(new StringLiteral(rc.BuiltinTypes, Xml, this.Location)));

			return new New(new TypeExpression(rc.Module.PlayscriptTypes.Xml.Resolve(), this.Location), 
			               args, this.Location).Resolve (rc);
		}
		
		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
/*		
		public override void EmitJs (JsEmitContext jec)
		{
			jec.Buf.Write (GetValue () as String, Location);
		}
*/		
#if FULL_AST
		public char[] ParsedValue { get; set; }
#endif
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class In : PlayScriptExpression
	{
		public In (Expression propertyExpr, Expression expression, Location loc)
		{
			this.PropertyExpression = propertyExpr;
			this.Expression = expression;
			this.loc = loc;
		}

		public Expression Expression { get; private set; }

		public Expression PropertyExpression { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			PropertyExpression = PropertyExpression.Resolve (rc);
			Expression = Expression.Resolve (rc);
			if (PropertyExpression == null || Expression == null)
				return null;

			if (Expression is MethodGroupExpr) {
				var res = new BoolConstant (rc.BuiltinTypes, false, Location);
				res.Resolve (rc);
				return res;
			}

			var ms = rc.Module.PlayScriptMembers.BinderHasProperty.Resolve (loc);
			if (ms == null)
				return null;

			var args = new Arguments (3);
			args.Add (new Argument (Expression));
			args.Add (new Argument (new CSharp.TypeOf (rc.CurrentType, loc)));
			args.Add (new Argument (PropertyExpression));

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			return new Invocation (mg, args).Resolve (rc);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class Is : CSharp.Is
	{
		public Is (Expression expr, Expression probeType, Location loc)
			: base (expr, probeType, loc)
		{
		}

		protected override Expression ResolveProbeType (ResolveContext rc)
		{
			probe_type_expr = ProbeType.TryToResolveAsType (rc);
			if (probe_type_expr != null)
				return this;

			var probe_type = ProbeType.Resolve (rc);
			if (probe_type == null)
				return null;

			probe_type = CSharp.Convert.ImplicitConversionRequired (rc, probe_type, rc.Module.PlayscriptTypes.Class, loc);
			if (probe_type == null)
				return null;

			var ms = rc.Module.PlayScriptMembers.OperationsIsOfType.Resolve (loc);
			if (ms == null)
				return null;

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var targs = new TypeArguments (
				new TypeExpression (expr.Type, loc));
			targs.Resolve (rc);
			mg.SetTypeArguments (rc, targs);

			var call_args = new Arguments (1);
			call_args.Add (new Argument (expr, Argument.AType.Ref));
			call_args.Add (new Argument (probe_type));

			return new Invocation (mg, call_args).Resolve (rc);
		}
	}

	class ImplicitTypeOf : CSharp.TypeOf
	{
		public ImplicitTypeOf (TypeExpression expr, Location loc)
			: base (expr, loc)
		{
		}

		public new TypeExpression TypeExpression {
			get {
				return (TypeExpression) base.TypeExpression;
			}
		}
	}

	public class LocalFunction : AnonymousMethodExpression
	{
		TypeSpec return_type;

		public LocalFunction (FullNamedExpression returnType, Location loc)
			: base (loc)
		{
			ReturnType = returnType;
		}

		public FullNamedExpression ReturnType { get; private set; }

		protected override AnonymousMethodBody CompatibleMethodFactory (ResolveContext rc, TypeSpec return_type, TypeSpec delegate_type, ParametersCompiled p, ParametersBlock b)
		{
			string d_name;
			int arity = p.Count;
			TypeSpec[] targs;

			if (return_type.Kind == MemberKind.Void) {
				d_name = "Action";
				targs = p.Types;
			} else {
				d_name = "Func";
				++arity;

				targs = new TypeSpec [arity];
				Array.Copy (p.Types, targs, p.Types.Length);
				targs [arity - 1] = return_type;
			}

			TypeExpr te = null;
			Namespace type_ns = rc.Module.GlobalRootNamespace.GetNamespace ("System", true);
			if (type_ns != null) {
				te = type_ns.LookupType (rc, d_name, arity, LookupMode.Normal, loc);
				if (te != null) {
					delegate_type = te.Type.MakeGenericType (rc, targs);
				}
			}

			return base.CompatibleMethodFactory (rc, return_type, delegate_type, p, b);
		}
		
		protected override TypeSpec ResolveReturnType (ResolveContext rc, TypeSpec delegateType)
		{
			if (return_type == null) {
				return_type = ReturnType.ResolveAsType (rc);
			}

			return return_type;
		}

		protected override ParametersCompiled ResolveParameters (ResolveContext rc, TypeInferenceContext tic, TypeSpec delegate_type)
		{
			return Parameters;
		}
	}

	public class LocalFunctionDeclaration : Statement
	{
		public LocalFunctionDeclaration (string name, Location loc)
		{
			Name = name;
			this.loc = loc;
		}

		public LocalFunction Function { get; set; }
		
		public Location Location {
			get {
				return loc;
			}
		}

		public string Name { get; private set; }

		protected override void DoEmit (EmitContext ec)
		{
			//
			// This is only AST declaration there is nothing to emit. The initialization
			// has been moved to block initializer because the function needs to be available
			// anywhere in the block.
			//
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			var t = target as LocalFunctionDeclaration;
			t.Function = (LocalFunction) Function.Clone (clonectx);
		}
	}

	public class UseNamespace : Statement
	{
		public UseNamespace (string ns, Location loc)
		{
			Namespace = ns;
			this.loc = loc;
		}

		public string Namespace { get; private set; }

		public override bool Resolve (BlockContext bc)
		{
			// TODO: Implement by adding the name to BlockContext namespaces list. Then when 
			// doing the namespace lookup get the list and do search with prefixes from the list
			// It looks like once the name is added (used) it's never removed even if the scope
			// is different
			return true;
		}
		
		public override bool ResolveUnreachable (BlockContext bc, bool warn)
		{
			return true;
		}
		
		public override void Emit (EmitContext ec)
		{
			// Nothing, not even sequence point
		}

		protected override void DoEmit (EmitContext ec)
		{
		}
		
		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
		}
		
		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
	}

	public class StatementFromExpression : Statement
	{
		public StatementFromExpression (Expression expr)
		{
			this.Expr = expr;
		}

		public Expression Expr { get; private set; }

		public override bool Resolve (BlockContext bc)
		{
			//
			// Any expression can act as a statement in PS, we need to wrap it to make
			// the inheritance chain clean
			//
			Expr = Expr.Resolve (bc);
			return Expr != null;
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (Expr.IsSideEffectFree)
				return;

			Expr.Emit (ec);
			ec.Emit (OpCodes.Pop);
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			var t = target as StatementFromExpression;
			t.Expr = Expr.Clone (clonectx);
		}
	}

	public class WithContext
	{
		Stack<Expression> object_context;
		Assign initialization;

		public WithContext ()
		{
		}

		public WithContext (Stack<Expression> context)
		{
			this.object_context = context;
		}

		public bool IsInitialized {
			get {
				return initialization != null;
			}
		}

		public Stack<Expression> ObjectContext {
			get {
				return object_context;
			}
		}

		public MethodGroupExpr RegisterMethod { get; private set; }
		public MethodGroupExpr UnregisterMethod { get; private set; }

		public TemporaryVariableReference Variable { get; private set; }

		public Assign Initialize (BlockContext bc, Location loc)
		{
			var doc = bc.Module.PlayscriptTypes.DefaultObjectContext.Resolve ();
			if (doc == null)
				return null;

			Variable = TemporaryVariableReference.Create (doc, bc.CurrentBlock, loc);
			if (Variable.Resolve (bc) == null)
				return null;

			var ms = bc.Module.PlayScriptMembers.DefaultObjectContextCreate.Resolve (loc);
			if (ms == null)
				return null;

			var mg = MethodGroupExpr.CreatePredefined (ms, doc, loc);
			var inv = new Invocation (mg, new Arguments (0)).Resolve (bc);
			initialization = new CompilerAssign (Variable, inv, loc);
			initialization.Resolve (bc);

			ms = bc.Module.PlayScriptMembers.DefaultObjectContextRegister.Resolve (loc);
			if (ms == null)
				return null;

			RegisterMethod = MethodGroupExpr.CreatePredefined (ms, doc, loc);
			RegisterMethod.InstanceExpression = Variable;

			ms = bc.Module.PlayScriptMembers.DefaultObjectContextUnregister.Resolve (loc);
			if (ms == null)
				return null;

			UnregisterMethod = MethodGroupExpr.CreatePredefined (ms, doc, loc);
			UnregisterMethod.InstanceExpression = Variable;

			return initialization;
		}

		public void SetObjectContext (Expression expr)
		{
			if (object_context == null)
				object_context = new Stack<Expression> ();

			object_context.Push (expr);
		}
	}

	public class ObjectContextRuntimeExpression : PlayScriptExpression
	{
		public ObjectContextRuntimeExpression (TypeSpec type)
		{
			this.eclass = ExprClass.Variable;
			this.type = type;
			this.loc = Location.Null;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
	}

	class ObjectContextMemberBinder : DynamicMemberAssignable
	{
		readonly string name;
		readonly Expression context;

		public ObjectContextMemberBinder (string name, Expression context, Location loc)
			: base (CreateFakeArgument (context), loc)
		{
			this.name = name;
			this.context = context;
		}

		static Arguments CreateFakeArgument (Expression context)
		{
			Arguments args = new Arguments (1);
			args.Add (new Argument (context));
			return args;
		}

		protected override Expression CreateCallSiteBinder (ResolveContext rc, Arguments args, bool isSet)
		{
			Arguments binder_args = new Arguments (3);

			binder_args.Add (new Argument (new StringLiteral (rc.BuiltinTypes, name, loc)));
			binder_args.Add (new Argument (new CSharp.TypeOf (rc.CurrentType, loc)));
			binder_args.Add (new Argument (context));

			isSet |= (flags & CSharpBinderFlags.ValueFromCompoundAssignment) != 0;
			return new Invocation (GetPlayScriptBinder (rc, isSet ? "SetMember" : "GetMember", loc), binder_args);
		}

		protected MemberAccess GetPlayScriptBinder (ResolveContext rc, string name, Location loc)
		{
			var binder_type = rc.Module.PlayscriptTypes.Binder.Resolve ();
			return new MemberAccess (new TypeExpression (binder_type, loc), name, loc);
		}
	}

	public class With : Statement
	{
		Assign initialization;
		MethodGroupExpr register, unregister;

		public With (Expression expr, Block block, Location loc)
		{
			this.Expr = expr;
			this.Block = block;
			this.loc = loc;
		}

		public Block Block { get; private set; }
		public Expression Expr { get; private set; }

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
			var t = target as With;
			t.Expr = Expr.Clone (clonectx);
			t.Block = clonectx.LookupBlock (Block);
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (initialization != null)
				initialization.EmitStatement (ec);

			if (register != null) {
				// TODO: Should be inside try-finally but need to check which
				// expressions then won't be allowed

				var args = new Arguments (1);
				args.Add (new Argument (Expr));
				register.EmitCall (ec, args);
			}

			Block.Emit (ec);

			if (unregister != null)
				unregister.EmitCall (ec, new Arguments (0));
		}

		public override bool Resolve (BlockContext bc)
		{
			Expression expr;

			var t = Expr.TryToResolveAsType (bc);
			if (t != null) {
				expr = new TypeExpression (t, loc);
			} else {
				expr = Expr.Resolve (bc);
				if (expr == null)
					return false;

				Expr = expr;
			}

			var ctx = bc.DefaultObjectContext;
			if (ctx == null) {
				bc.DefaultObjectContext = ctx = new WithContext ();
			}

			if (expr.eclass == ExprClass.Type || expr.Type.IsSealed) {
				// 
				// All member lookups can be done during compilation
				//
				ctx.SetObjectContext (expr);
			} else {
				if (!ctx.IsInitialized) {
					initialization = ctx.Initialize (bc, loc);
					if (initialization == null)
						return false;
				}

				ctx.SetObjectContext (ctx.Variable);
				register = ctx.RegisterMethod;
				unregister = ctx.UnregisterMethod;
			}

			Block.Resolve (bc);

			ctx.ObjectContext.Pop ();
			return true;
		}
	}

	/// <summary>
	///   Implementation of the ActionScript E4X xml query.
	/// </summary>
	public class AsXmlQueryExpression : Expression
	{
		protected Expression expr;
		protected Expression query;
		
		public AsXmlQueryExpression (Expression expr, Expression query, Location l)
		{
			this.expr = expr;
			this.query = query;
			loc = l;
		}
		
		public Expression Expr {
			get {
				return expr;
			}
		}

		public Expression Query {
			get {
				return query;
			}
		}

		public override bool ContainsEmitWithAwait ()
		{
			throw new NotSupportedException ();
		}
		
		public override Expression CreateExpressionTree (ResolveContext ec)
		{
			throw new NotSupportedException ("ET");
		}
		
		protected override Expression DoResolve (ResolveContext ec)
		{
			// TODO: Implement XML query expression.
			return null;
		}
		
		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			AsXmlQueryExpression target = (AsXmlQueryExpression) t;
			
			target.expr = expr.Clone (clonectx);
			target.query = query.Clone (clonectx);
		}
		
		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("Missing Resolve call");
		}

		public override object Accept (StructuralVisitor visitor)
		{
			return visitor.Visit (this);
		}
		
	}

	public class SimpleName : CSharp.SimpleName
	{
		public SimpleName (string name, Location loc)
			: base (name, loc)
		{
		}

		// TODO: targs should be always null
		public SimpleName (string name, TypeArguments targs, Location loc)
			: base (name, targs, loc)
		{
		}

		public override Expression LookupNameExpression (ResolveContext rc, MemberLookupRestrictions restrictions)
		{
			int lookup_arity = Arity;
			bool errorMode = false;
			Expression e;
			Block current_block = rc.CurrentBlock;
			INamedBlockVariable variable = null;
			bool variable_found = false;

			if (rc.DefaultObjectContext != null) {
				e = LookupObjectContext (rc, Name, restrictions);
				if (e != null)
					return e;
			}

			//
			// Stage 1: binding to local variables or parameters
			//
			if (current_block != null && lookup_arity == 0) {
				if (current_block.ParametersBlock.TopBlock.GetLocalName (Name, current_block.Original, ref variable)) {
					if (!variable.IsDeclared) {
//						rc.Report.Warning (7156, 1, loc, "Use of local variable before declaration");
						if (variable is LocalVariable) {
							var locVar = variable as LocalVariable;
//							if (locVar.Type == null && locVar.TypeExpr != null) {
//								locVar.DeclFlags |= LocalVariable.Flags.AsIgnoreMultiple;
//								locVar.Type = locVar.TypeExpr.ResolveAsType (rc);
//							}
						}
						e = variable.CreateReferenceExpression (rc, loc);
						if (e != null) {
							if (Arity > 0)
								Error_TypeArgumentsCannotBeUsed (rc, "variable", Name, loc);

							return e;
						}
					} else {
						e = variable.CreateReferenceExpression (rc, loc);
						if (e != null) {
							if (Arity > 0)
								Error_TypeArgumentsCannotBeUsed (rc, "variable", Name, loc);

							return e;
						}
					}
				}
			}

				//
				// Stage 2: Lookup members if we are inside a type up to top level type for nested types
				//
				TypeSpec member_type = rc.CurrentType;
				for (; member_type != null; member_type = member_type.DeclaringType) {
					e = MemberLookup (rc, errorMode, member_type, Name, lookup_arity, restrictions, loc);
					if (e == null)
						continue;

					var me = e as MemberExpr;
					if (me == null) {
						// The name matches a type, defer to ResolveAsTypeStep
						if (e is TypeExpr)
							break;

						continue;
					}

					me = me.ResolveMemberAccess (rc, null, null);

					if (Arity > 0) {
						targs.Resolve (rc);
						me.SetTypeArguments (rc, targs);
					}

					return me;
				}

			// Stage 3: Global names lookup
			e = LookupGlobalName (rc, Name, restrictions);
			if (e != null)
				return e;

			//
			// Stage 3: Lookup nested types, namespaces and type parameters in the context
			//
			if ((restrictions & MemberLookupRestrictions.InvocableOnly) == 0 && !variable_found) {
				if (IsPossibleTypeOrNamespace (rc)) {
					if (variable != null) {
						rc.Report.SymbolRelatedToPreviousError (variable.Location, Name);
						rc.Report.Error (135, loc, "`{0}' conflicts with a declaration in a child block", Name);
					}

					var fne = ResolveAsTypeOrNamespace (rc);
					if (fne != null && (restrictions & MemberLookupRestrictions.PlayScriptConversion) == 0) {
						var te = fne as TypeExpression;
						if (te != null)
							return new ImplicitTypeOf (te, loc);
					}

					return fne;
				}
			}

			// TODO: Use C# rules too?

			// TODO: Handle errors
			throw new NotImplementedException ();
		}

		public override FullNamedExpression ResolveAsTypeOrNamespace (IMemberContext mc)
		{
			var fne = ResolveKnownTypes (mc);
			if (fne != null)
				return fne;

			return base.ResolveAsTypeOrNamespace (mc);
		}

		// TODO: Add ambiguity checks
		// PS1000: var:Number:Number = 0; is ambiguous between local variable and global type
		TypeExpression ResolveKnownTypes (IMemberContext mc)
		{
			var types = mc.Module.Compiler.BuiltinTypes;
			switch (Name) {
//			case "Object":
//				return new TypeExpression (mc.Module.PlayscriptTypes.Object, loc);
			case "Boolean":
				return new TypeExpression (types.Bool, loc);
			case "Number":
				return new TypeExpression (types.Double, loc);
			default:
				return null;
			}
		}

		Expression LookupObjectContext (ResolveContext rc, string name, MemberLookupRestrictions restrictions)
		{
			var ctx = rc.DefaultObjectContext;
			foreach (var expr in ctx.ObjectContext) {
				if (expr == ctx.Variable) {
					return new ObjectContextMemberBinder (name, expr, loc).Resolve (rc);
				}

				var type = expr.Type;

				var me = MemberLookup (rc, false, type, name, 0, restrictions, loc) as MemberExpr;
				if (me == null) {
					//
					// Try to look for extension method when member lookup failed
					//
					if (MethodGroupExpr.IsExtensionMethodArgument (expr)) {
						var methods = rc.LookupExtensionMethod (type, Name, 0);
						if (methods != null) {
							var emg = new ExtensionMethodGroupExpr (methods, expr, loc);
							//if (HasTypeArguments) {
							//	if (!targs.Resolve (rc))
							//		return null;
							//
							//	emg.SetTypeArguments (rc, targs);
							//}

							//
							// Run defined assigned checks on expressions resolved with
							// disabled flow-analysis
							//
							//if (sn != null && !errorMode) {
							//	var vr = expr as VariableReference;
							//	if (vr != null)
							//		vr.VerifyAssigned (rc);
							//}

							// TODO: it should really skip the checks bellow
							return emg.Resolve (rc);
						}
					}

					continue;
				}

				return me.ResolveMemberAccess (rc, null, null);
			}

			return null;
		}

		Expression LookupGlobalName (ResolveContext rc, string name, MemberLookupRestrictions restrictions)
		{
			var ns = rc.Module.GlobalRootNamespace.GetNamespace (PredefinedTypes.RootNamespace, false);
			if (ns == null)
				return null;

			var types = ns.GetAllTypes (PackageGlobalContainer.Name);
			if (types == null)
				return null;

			MemberExpr me = null;
			foreach (var type in types) {
				var e = MemberLookup (rc, false, type, name, 0, restrictions, loc) as MemberExpr;
				if (e == null)
					continue;

				//
				// Compiled names have always priority over imported names
				//
				if (!type.MemberDefinition.IsImported) {
					me = e;
					break;
				}

				if (me == null) {
					me = e;
					continue;
				}

				// TODO: Figure out how to handle
				throw new NotImplementedException ("Imported package name collision");
			}

			if (me == null)
				return null;

			return me.ResolveMemberAccess (rc, null, null);
		}
	}

	public class QualifiedMemberAccess : MemberAccess
	{
		public QualifiedMemberAccess (Expression expr, string namespaceName, string identifier, Location l)
			: base (expr, identifier, l)
		{
			this.Namespace = namespaceName;
		}

		public string Namespace { get; private set; }

		public override Expression LookupNameExpression (ResolveContext rc, MemberLookupRestrictions restrictions)
		{
			TypeSpec expr_type;
			var tne = expr as ATypeNameExpression;
			if (tne == null) {
				expr_type = rc.CurrentType;
			} else {
				var res = tne.LookupNameExpression (rc, MemberLookupRestrictions.None);
				if (res == null)
					return null;

				res = res.Resolve (rc);
				if (res == null)
					return null;

				expr_type = res.Type;
			}

			var name = Namespace + "." + Name;
			var member_lookup = MemberLookup (rc, false, expr_type, name, 0, restrictions, loc);

			// TODO: Implement correct rules for out of context namespaces
			if (member_lookup == null)
				return null;

			var me = (MemberExpr) member_lookup;
			return me.ResolveMemberAccess (rc, expr, null);
		}
	}

	public class NamespaceMemberName : MemberName
	{
		public NamespaceMemberName (string namespaceName, string name, Location loc)
			: base (new MemberName (namespaceName, loc), name, loc)
		{
		}

		public override string LookupName {
			get {
				return Left.LookupName + "." + base.LookupName;
			}
		}

		public override string GetSignatureForError ()
		{
			return Left.GetSignatureForError () + "::" + Name;
		}
	}

	public class NamespaceField : FieldBase
	{
		public class NamespaceInitializer : FieldInitializer
		{
			readonly NamespaceField field;

			public NamespaceInitializer (NamespaceField field, Expression value, Location loc)
				: base (field, value, loc)
			{
				this.field = field;
			}

			protected override ExpressionStatement ResolveInitializer (ResolveContext rc)
			{
				if (source == null)
					return null;

				source = source.Resolve (rc);
				if (source == null)
					return null;

				source = GetStringValue (rc, source);
				if (source == null) {
					rc.Report.ErrorPlayScript (1171, loc, "A namespace initializer must be either a literal string or another namespace.");
					return null;
				}

				var args = new Arguments (2);
				args.Add (new Argument (new NullLiteral (loc)));
				args.Add (new Argument (source));
				source = new New (new TypeExpression (field.MemberType, loc), args, loc);

				return base.ResolveInitializer (rc);
			}

			public static StringConstant GetStringValue (IMemberContext mc, Expression source)
			{
				var sc = source as StringConstant;
				if (sc != null)
					return sc;

				var fe = source as FieldExpr;
				if (fe == null)
					return null;

				var nf = fe.Spec as NamespaceFieldSpec;
				if (nf == null)
					return null;

				return new StringConstant (mc.Module.Compiler.BuiltinTypes, nf.GetValue (), Location.Null);
			}
		}

		const Modifiers AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public NamespaceField (TypeDefinition parent, Modifiers mod, MemberName name, Expression initializer, Attributes attrs)
			: base (parent, null, mod, AllowedModifiers, name, attrs)
		{
			Initializer = initializer;
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			ModFlags |= Modifiers.STATIC;

			FieldAttributes field_attr = FieldAttributes.InitOnly | ModifiersExtensions.FieldAttr (ModFlags);
			FieldBuilder = Parent.TypeBuilder.DefineField (GetFullName (MemberName), MemberType.GetMetaInfo (), field_attr);

			spec = new NamespaceFieldSpec (Parent.Definition, this, MemberType, FieldBuilder, ModFlags);
			Parent.MemberCache.AddMember (spec);

			return true;
		}

		public override void Emit ()
		{
			base.Emit ();

			Module.PlayscriptAttributes.NamespaceField.EmitAttribute (FieldBuilder, GetValue ());
		}

		public string GetValue ()
		{
			if (initializer == null)
				return null;

			var sc = NamespaceInitializer.GetStringValue (this, initializer);
			if (sc == null)
				return null;

			return sc.Value;
		}

		protected override bool ResolveMemberType ()
		{
			member_type = Module.PlayscriptTypes.Namespace.Resolve ();
			return true;
		}
	}

	class NamespaceFieldSpec : FieldSpec
	{
		string value;

		public NamespaceFieldSpec (TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, FieldInfo info, Modifiers modifiers)
			: base (declaringType, definition, memberType, info, modifiers)
		{
		}

		public NamespaceFieldSpec (TypeSpec declaringType, IMemberDefinition definition, TypeSpec memberType, FieldInfo info, Modifiers modifiers, string value)
			: this (declaringType, definition, memberType, info, modifiers)
		{
			this.value = value;
		}

		public string GetValue ()
		{
			var def = MemberDefinition as NamespaceField;
			if (def != null)
				return def.GetValue ();

			return value;
		}
	}

	interface IConstantProperty
	{
		Expression Initializer { get; }
	}

	class ImportedPropertyConstant : ImportedMemberDefinition, IConstantProperty
	{
		public ImportedPropertyConstant (MemberInfo member, TypeSpec type, MetadataImporter importer)
			: base (member, type, importer)
		{
		}

		public Expression Initializer { get; set; }
	}

	public class ConstantField : FieldBase
	{
		public class Property : CSharp.Property, IConstantProperty
		{
			public Property (TypeDefinition parent, FullNamedExpression type, Modifiers mod, MemberName name, Attributes attrs)
				: base (parent, type, mod, name, attrs)
			{
			}

			public Expression Initializer { get; set; }

			public override void Emit ()
			{
				var init = Initializer.Resolve (null);
				if (init == null)
					return;

				var c = init as Constant;
				if (c == null) {
					var set_field = new Field (Parent, new TypeExpression (Compiler.BuiltinTypes.Bool, Location), Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | (ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
						new MemberName ("<" + GetFullName (MemberName) + ">__SetField", Location), null);

					var lazy_field = new Field (Parent, type_expr, Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | (ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
						new MemberName ("<" + GetFullName (MemberName) + ">__LazyField", Location), null);

					set_field.Define ();
					lazy_field.Define ();

					Parent.AddField (set_field);
					Parent.AddField (lazy_field);

					// 
					// if (!SetField) {
					//   SetField = true;
					//   LazyField = Initializer;
					// }
					//
					var set_f_expr = new FieldExpr (set_field, Location);
					var lazy_f_expr = new FieldExpr (lazy_field, Location);
					if (!IsStatic) {
						set_f_expr.InstanceExpression = new CompilerGeneratedThis (CurrentType, Location);
						lazy_f_expr.InstanceExpression = new CompilerGeneratedThis (CurrentType, Location);
					}

					var expl = new ExplicitBlock (Get.Block, Location, Location);
					Get.Block.AddScopeStatement (new If (new Unary (Unary.Operator.LogicalNot, set_f_expr, Location), expl, Location));

					expl.AddStatement (new StatementExpression (new CompilerAssign (lazy_f_expr, init, Location)));
					Get.Block.AddStatement (new Return (lazy_f_expr, Location));

					Module.PlayscriptAttributes.ConstantField.EmitAttribute (PropertyBuilder);
				} else {
					//
					// Simple constant, just return a value
					//
					Get.Block.AddStatement (new Return (init, Location));

					var expr = CSharp.Convert.ImplicitConversionStandard (null, c, Compiler.BuiltinTypes.Object, Location);
					if (expr == null)
						throw new NotImplementedException ();

					//
					// Store compile time constant to attribute for easier import
					//
					Module.PlayscriptAttributes.ConstantField.EmitAttribute (this, PropertyBuilder, expr);
				}

				base.Emit ();
			}
		}

		class ConstInitializer : PlayScriptExpression
		{
			bool in_transit;
			readonly Property prop;
			Expression expr;

			public ConstInitializer (Property prop, Expression expr, Location loc)
			{
				this.loc = loc;
				this.prop = prop;
				this.expr = expr;
			}

			protected override Expression DoResolve (ResolveContext unused)
			{
				if (type != null)
					return expr;

				//
				// Use a context in which the constant was declared and
				// not the one in which is referenced
				//
				var bc = new BlockContext (prop, prop.Get.Block, prop.MemberType);
				bc.Set (ResolveContext.Options.ConstantScope);

				expr = DoResolveInitializer (bc);
				type = expr.Type;

				return expr;
			}

			public override Constant ResolveAsPlayScriptConstant (ResolveContext rc)
			{
				return DoResolve (rc) as Constant;
			}

			Expression DoResolveInitializer (ResolveContext rc)
			{
				if (in_transit) {
					// PS seems to use default value but how to get the right order
					throw new NotImplementedException ("Constant circular definition");
				} else {
					in_transit = true;
					expr = expr.Resolve (rc);
				}

				in_transit = false;

				if (expr != null) {
					var res = CSharp.Convert.ImplicitConversion (rc, expr, prop.MemberType, expr.Location);
					if (res == null) {
						rc.Report.ErrorPlayScript (1184, expr.Location, "Incompatible default value of type `{0}' where `{1}' is expected",
							expr.Type.GetSignatureForError (), prop.MemberType.GetSignatureForError ());
					} else {
						expr = res;
					}
				}

				if (expr == null) {
					expr = New.Constantify (prop.MemberType, Location);
					if (expr == null)
						expr = Constant.CreateConstantFromValue (prop.MemberType, null, Location);
					expr = expr.Resolve (rc);
				}

				return expr;
			}

			public override void Emit (EmitContext ec)
			{
				throw new NotSupportedException ();
			}
		}

		const Modifiers AllowedModifiers =
			Modifiers.STATIC |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public ConstantField (TypeDefinition parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base (parent, type, mod_flags, AllowedModifiers, name, attrs)
		{
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (Parent is PackageGlobalContainer)
				ModFlags |= Modifiers.STATIC;

			var t = new TypeExpression (MemberType, TypeExpression.Location);
			var init = Initializer ?? ImplicitVariableInitializer.GetConstantValue (this, MemberType, Location);

			var prop = new Property (Parent, t, ModFlags, MemberName, attributes);
			prop.Initializer = new ConstInitializer (prop, init, init.Location);
			prop.Get = new Property.GetMethod (prop, 0, null, prop.Location);
			prop.Get.Block = new ToplevelBlock (Compiler, Location);

			if (!prop.Define ())
				return false;

			var idx = Parent.Members.IndexOf (this);
			Parent.Members[idx] = prop;

			if (declarators != null) {
				foreach (var d in declarators) {
					init = d.Initializer ?? new DefaultValueExpression (t, Location);

					prop = new Property (Parent, t, ModFlags, new MemberName (d.Name.Value, d.Name.Location), attributes);
					prop.Initializer = new ConstInitializer (prop, init, init.Location);

					prop.Get = new Property.GetMethod (prop, 0, null, prop.Location);
					prop.Get.Block = new ToplevelBlock (Compiler, Location); ;

					prop.Define ();
					Parent.PartialContainer.Members.Add (prop);
				}
			}

			return true;
		}
	}

	public class FieldDeclarator : CSharp.FieldDeclarator
	{
		public FieldDeclarator (FieldBase field, SimpleMemberName name, Expression initializer, FullNamedExpression typeExpr)
			: base (field, name, initializer)
		{
			this.TypeExpression = typeExpr;
		}

		public FieldDeclarator (FieldBase field, SimpleMemberName name, Expression initializer)
			: base (field, name, initializer)
		{
		}

		public FullNamedExpression TypeExpression { get; private set; }

		public override FullNamedExpression GetFieldTypeExpression (FieldBase field)
		{
			return TypeExpression;
		}
	}

	public class BlockVariableDeclarator : CSharp.BlockVariableDeclarator
	{
		TypeSpec type;

		public BlockVariableDeclarator (LocalVariable li, Expression initializer, FullNamedExpression typeExpr, Location loc)
			: base (li, initializer)
		{
			this.TypeExpression = typeExpr;
			this.Location = loc;
		}

		public BlockVariableDeclarator (LocalVariable li, Expression initializer, Location loc)
			: base (li, initializer)
		{
			this.Location = loc;
		}

		public Location Location { get; private set; }
		public FullNamedExpression TypeExpression { get; private set; }

		public override TypeSpec ResolveType (BlockContext bc, TypeSpec unused)
		{
			if (type != null)
				return type;

			if (TypeExpression == null) {
				if (Initializer is LocalFunction)
					TypeExpression = new TypeExpression (bc.Module.PlayscriptTypes.Function, Location);
				else
					TypeExpression = new UntypedTypeExpression (Location);
			}

			type = TypeExpression.ResolveAsType (bc);
			if (type == null)
				return null;

			Variable.Type = type;
			return type;
		}
	}

	//
	// When variable is used before it's declared it has to be initialized
	// to correct PS value which in some cases does not match CIL default
	// values
	//
	public class ImplicitVariableInitializer : Statement
	{
		BlockVariable variable;

		public ImplicitVariableInitializer (BlockVariable variable)
		{
			this.variable = variable;
		}

		public override bool Resolve (BlockContext bc)
		{
			var existing = variable.Variable.Type;

			var type = variable.ResolveType (bc);
			if (type == null)
				return false;

			bool constant = variable.Variable.IsConstant;
			if (existing != null) {
				if (constant) {
					Error_NameConflict (bc, variable.loc, variable.Variable.Name);
				} else {
					var undefined = bc.Module.PlayscriptTypes.UndefinedType;
					if (type == undefined) {
						variable.Variable.Type = existing;
					} else if (existing == undefined) {
						// Nothing to do
					} else if (!TypeSpecComparer.IsEqual (existing, type)) {
						Error_NameConflict (bc, variable.loc, variable.Variable.Name);
					}
				}
			}

			if (type == bc.Module.PlayscriptTypes.UndefinedType) {
				bc.Module.PlayScriptMembers.UndefinedValue.Resolve (variable.loc);
			}

			if (variable.Declarators != null) {
				foreach (BlockVariableDeclarator d in variable.Declarators) {
					existing = d.Variable.Type;
					type = d.ResolveType (bc, null);
					if (type == null)
						continue;

					if (existing != null) {
						if (constant) {
							Error_NameConflict (bc, d.Location, d.Variable.Name);
						} else {
							var undefined = bc.Module.PlayscriptTypes.UndefinedType;
							if (type == undefined) {
								variable.Variable.Type = existing;
							} else if (existing == undefined) {
								// Nothing to do
							} else if (!TypeSpecComparer.IsEqual (existing, type)) {
								Error_NameConflict (bc, d.Location, d.Variable.Name);
							}
						}
					}

					if (type == bc.Module.PlayscriptTypes.UndefinedType) {
						bc.Module.PlayScriptMembers.UndefinedValue.Resolve (d.Variable.Location);
					}
				}
			}

			return true;
		}

		static void Error_NameConflict (BlockContext bc, Location loc, string name)
		{
			bc.Report.ErrorPlayScript (1151, loc, "A conflict exists with definition `{0}'", name);
		}

		protected override void DoEmit (EmitContext ec)
		{
			if (variable.Variable.IsConstant)
				return;

			EmitVariableInitialization (ec, variable.Variable);

			if (variable.Declarators != null) {
				foreach (BlockVariableDeclarator d in variable.Declarators) {
					EmitVariableInitialization (ec, d.Variable);
				}
			}
		}

		static void EmitVariableInitialization (EmitContext ec, LocalVariable li)
		{
			li.CreateBuilder (ec);

			// TOOD: Optimize to initialize only when read before assignment
			if (li.Type == ec.Module.PlayscriptTypes.Number) {
				ec.Emit (OpCodes.Ldc_R8, double.NaN);
				li.EmitAssign (ec);
				return;
			}

			if (li.Type == ec.Module.PlayscriptTypes.UndefinedType) {
				var spec = ec.Module.PlayScriptMembers.UndefinedValue.Get ();
				var fe = new FieldExpr (spec, Location.Null);

				fe.Emit (ec);
				li.EmitAssign (ec);
				return;
			}
		}

		public static Constant GetConstantValue (IMemberContext mc, TypeSpec type, Location loc)
		{
			if (type == mc.Module.PlayscriptTypes.Number) {
				return new DoubleConstant (type, double.NaN, loc);
			}

			if (TypeSpec.IsReferenceType (type))
				return new NullConstant (type, loc);

			return New.Constantify (type, loc);
		}

		protected override void CloneTo (CloneContext clonectx, Statement target)
		{
		}
	}

	public class Field : CSharp.Field
	{
		const Modifiers AllowedModifiers =
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC;

		public Field (TypeDefinition parent, FullNamedExpression type, Modifiers mod, MemberName name, Attributes attrs)
			: base (parent, type, mod, AllowedModifiers, Modifiers.INTERNAL, name, attrs)
		{
		}

		public override bool Define ()
		{
			if (Parent is PackageGlobalContainer)
				ModFlags |= Modifiers.STATIC;

			return base.Define ();
		}
	}

	public class Method : CSharp.Method
	{
		const Modifiers AllowedModifiers =
			Modifiers.STATIC |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.OVERRIDE |
			Modifiers.VIRTUAL | // virtual should be no-op but we extend AS here to have better metadata
			Modifiers.SEALED; // TODO: FINAL

		private Method (TypeDefinition parent, FullNamedExpression return_type, Modifiers mod,
					MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base (parent, return_type, mod, AllowedModifiers, Modifiers.INTERNAL, name, parameters, attrs)
		{
		}

		public bool HasNoReturnType { get; set; }

		public static new Method Create (TypeDefinition parent, FullNamedExpression returnType, Modifiers mod,
				   MemberName name, ParametersCompiled parameters, Attributes attrs)
		{
			var m = new Method (parent, returnType, mod, name, parameters, attrs);

			if (parent is PackageGlobalContainer) {
				if ((mod & Modifiers.OVERRIDE) != 0) {
					m.Report.ErrorPlayScript (1010, m.Location, "`{0}': The override attribute may be used only on definitions inside a class",
						m.GetSignatureForError ());
				}

				if ((mod & Modifiers.VIRTUAL) != 0) {
					m.Report.ErrorPlayScript (1011, m.Location, "`{0}': The override attribute may be used only on definitions inside a class",
						m.GetSignatureForError ());
				}

				if ((mod & Modifiers.STATIC) != 0) {
					m.Report.ErrorPlayScript (1012, m.Location, "`{0}': The static attribute may be used only on definitions inside a class",
						m.GetSignatureForError ());
				}
			}

			if ((mod & Modifiers.AccessibilityMask) == 0) {
				m.Report.WarningPlayScript (1085, m.Location, "`{0}': will be scoped to the default namespace `internal'",
					m.GetSignatureForError ());
			}

			return m;
		}

		protected override bool CheckOverrideAgainstBase (MemberSpec base_member)
		{
			bool ok = true;

			if ((base_member.Modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) == 0) {
				ModFlags &= ~Modifiers.OVERRIDE;
				ModFlags |= Modifiers.VIRTUAL;
			}

			if ((base_member.Modifiers & Modifiers.SEALED) != 0) {
				Report.SymbolRelatedToPreviousError (base_member);
				Report.ErrorPlayScript (1025, Location, "`{0}': Cannot redefine a final method", GetSignatureForError ());
				ok = false;
			}

			var base_member_type = ((IInterfaceMemberSpec) base_member).MemberType;
			if (!TypeSpecComparer.Override.IsEqual (MemberType, base_member_type)) {
				Report.SymbolRelatedToPreviousError (base_member);

				// TODO: Add all not matching incompatibilites as PS1023
				Report.ErrorPlayScript (1023, Location, "`{0}': Incompatible override: Return type must be `{1}' to match overridden member `{2}'",
					GetSignatureForError (), base_member_type.GetSignatureForError (), base_member.GetSignatureForError ());
				ok = false;
			}

			return ok;
		}

		public override bool Define ()
		{
			if (Parent is PackageGlobalContainer) {
				ModFlags |= Modifiers.STATIC;
			}

			//
			// Allowed but makes no sense
			//
			if ((ModFlags & (Modifiers.VIRTUAL | Modifiers.PRIVATE)) == (Modifiers.VIRTUAL | Modifiers.PRIVATE))
				ModFlags &= ~Modifiers.VIRTUAL;

			return base.Define ();
		}

		protected override void Error_OverrideWithoutBase (MemberSpec candidate)
		{
			if (candidate == null) {
				Report.ErrorPlayScript (1020, Location, "`{0}': Method marked override must override another method", GetSignatureForError ());
				return;
			}

			base.Error_OverrideWithoutBase (candidate);
		}
	}

	public class E4XIndexer : PlayScriptExpression
	{
		public enum Operator
		{
			Attribute,	// .@
			Namespace	// ::
		}

		readonly Operator oper;
		readonly Arguments args;
		Expression expr;

		public E4XIndexer (Operator oper, Expression expr, Arguments args, Location loc)
		{
			this.oper = oper;
			this.expr = expr;
			this.args = args;
			this.loc = loc;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			switch (oper) {
			case Operator.Attribute:
				return MakeInvocation ("attribute").Resolve (rc);
			case Operator.Namespace:
				return MakeInvocation ("namespace").Resolve (rc);
			}

			throw new NotImplementedException ();
		}

		Expression MakeInvocation (string method)
		{
			return new Invocation (new MemberAccess (expr, method, loc), args);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
	}

	public class E4XOperator : PlayScriptExpression
	{
		public enum Operator
		{
			Descendant,		// ..
			ChildAll,		// .*
			ChildAttribute,	// .@
			DescendantAll,	// ..*
			Namespace		// ::
		}

		readonly Operator oper;
		readonly string name;
		Expression expr;
		
		public E4XOperator (Operator oper, Expression expr, string name, Location loc)
		{
			this.oper = oper;
			this.expr = expr;
			this.name = name;
			this.loc = loc;
		}

		protected override Expression DoResolve (ResolveContext rc)
		{
			switch (oper) {
			case Operator.ChildAll:
				return MakeInvocation (rc, "children").Resolve (rc);
			case Operator.DescendantAll:
				return MakeInvocation (rc, "descendants").Resolve (rc);
			case Operator.ChildAttribute:
				return MakeInvocation (rc, "attribute", name).Resolve (rc);
			case Operator.Descendant:
				return MakeInvocation (rc, "descendants", name).Resolve (rc);
			case Operator.Namespace:
				return MakeInvocation (rc, "namespace", name).Resolve (rc);
			}

			throw new NotImplementedException ();
		}

		Expression MakeInvocation (ResolveContext rc, string method, string arg = null)
		{
			Arguments args = null;
			if (arg != null) {
				args = new Arguments (1);
				args.Add (new Argument (new StringLiteral (rc.BuiltinTypes, arg, loc)));
			}

			return new Invocation (new MemberAccess (expr, method, loc), args);
		}

		public override void Emit (EmitContext ec)
		{
			throw new NotSupportedException ();
		}
	}

	public class ForIn : ForIterator
	{
		public ForIn (Statement variableIterant, Expression expr, Statement stmt, Block body, Location loc)
			: base (variableIterant, expr, stmt, body, loc)
		{
		}

		protected override PredefinedMember<MethodSpec> GetValuesMethod (BlockContext bc)
		{
			return bc.Module.PlayScriptMembers.BinderGetKeys;
		}

		protected override TypeSpec GetUntypedIterantType (BlockContext bc)
		{
			return bc.BuiltinTypes.String;
		}
	}

	public class ForEach : ForIterator
	{
		public ForEach (Statement variableIterant, Expression expr, Statement stmt, Block body, Location loc)
			: base (variableIterant, expr, stmt, body, loc)
		{
		}

		protected override PredefinedMember<MethodSpec> GetValuesMethod (BlockContext bc)
		{
			return bc.Module.PlayScriptMembers.BinderGetValues;
		}

		protected override TypeSpec GetUntypedIterantType (BlockContext bc)
		{
			return bc.BuiltinTypes.Dynamic;
		}
	}

	public abstract class ForIterator : CSharp.Foreach
	{
		public ForIterator (Statement variableIterant, Expression expr, Statement stmt, Block body, Location loc)
			: base (null, null, expr, stmt, body, loc)
		{
			this.Iterant = variableIterant;
		}

		public Statement Iterant { get; private set; }

		protected abstract PredefinedMember<MethodSpec> GetValuesMethod (BlockContext bc);
		protected abstract TypeSpec GetUntypedIterantType (BlockContext bc);

		public override bool Resolve (BlockContext bc)
		{
			expr = expr.Resolve (bc);
			if (expr == null)
				return false;

			body.AddStatement (Statement);

			// TODO: ?
			//if (expr.eclass == ExprClass.MethodGroup || expr is AnonymousMethodExpression) {
			//	ec.Report.Error (446, expr.Location, "Foreach statement cannot operate on a `{0}'",
			//		expr.ExprClassName);
			//	return false;
			//}

			var ms = GetValuesMethod (bc).Resolve (loc);
			if (ms == null)
				return false;

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (1);
			call_args.Add (new Argument (Expr));

			expr = new Invocation (mg, call_args).Resolve (bc);
			if (expr == null)
				return false;

			var untyped = Iterant as UntypedBlockVariable;
			if (untyped != null) {
				untyped.TypeExpression = new TypeExpression (GetUntypedIterantType (bc), loc);
			}

			if (!Iterant.Resolve (bc))
				return false;

			var var = Iterant as BlockVariable;
			if (var != null) {
				type = var.TypeExpression;
				variable = var.Variable;

				statement = new CollectionForeach (this, variable, Expr);
			} else {
				LocalVariableReference lvr = null;
				var sfe = Iterant as StatementFromExpression;
				if (sfe != null) {
					if (!sfe.Resolve (bc))
						return false;

					lvr = sfe.Expr as LocalVariableReference;
				}

				if (lvr == null) {
					bc.Report.ErrorPlayScript (1105, Iterant.loc, "Target of assignment must be a reference value");
					return false;
				}

				statement = new CollectionForeach (this, lvr, Expr);
			}

			return statement.Resolve (bc);
		}
	}

	public class UsingType : UsingNamespace
	{
		protected TypeSpec resolvedType;

		public UsingType (ATypeNameExpression expr, Location loc)
			: base (expr, loc)
		{
		}

		public override void Define (NamespaceContainer ctx)
		{
			resolved = NamespaceExpression.ResolveAsTypeOrNamespace (ctx);
			if (resolved != null) {
				resolvedType = resolved.ResolveAsType (ctx);
			}
		}

		public TypeSpec ResolvedType
		{
			get { return resolvedType; }
		}
	}

	class DynamicClassMemberAccess : PlayScriptExpression, IAssignMethod
	{
		Expression invocation;

		public DynamicClassMemberAccess (ElementAccess ea)
			: this (ea.Expr, ea.Arguments, ea.Location)
		{
		}

		public DynamicClassMemberAccess (Expression instance, Arguments args, Location loc)
		{
			this.Instance = instance;
			this.Arguments = args;
			this.loc = loc;
		}

		public Arguments Arguments { get; private set; }

		public Expression Instance { get; private set; }

		protected override Expression DoResolve (ResolveContext rc)
		{
			var ms = rc.Module.PlayScriptMembers.BinderGetMember.Resolve (loc);
			if (ms == null)
				return null;

			// TODO: Figure out what value = dc["a", "b"] is supposed to do

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (3);
			call_args.Add (new Argument (Instance));
			call_args.Add (new Argument (new CSharp.TypeOf (rc.CurrentType, loc)));
			call_args.Add (Arguments [0]);

			invocation = new Invocation (mg, call_args).Resolve (rc);
			if (invocation == null)
				return null;

			eclass = ExprClass.Variable;
			type = invocation.Type;
			return this;
		}

		public override Expression DoResolveLValue (ResolveContext rc, Expression rhs)
		{
			var ms = rc.Module.PlayScriptMembers.BinderSetMember.Resolve (loc);
			if (ms == null)
				return null;

			// TODO: Figure out what dc["a", "b"] = value is supposed to do

			var mg = MethodGroupExpr.CreatePredefined (ms, ms.DeclaringType, loc);
			var call_args = new Arguments (3);
			call_args.Add (new Argument (Instance));
			call_args.Add (new Argument (new CSharp.TypeOf (rc.CurrentType, loc)));
			call_args.Add (Arguments [0]);
			call_args.Add (new Argument (rhs));

			invocation = new Invocation (mg, call_args).Resolve (rc);

			eclass = ExprClass.Variable;
			type = rhs.Type;
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			invocation.Emit (ec);
		}

		public void Emit (EmitContext ec, bool leave_copy)
		{
			throw new NotImplementedException ();
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool isCompound)
		{
			if (leave_copy || isCompound)
				throw new NotImplementedException ();

			invocation.Emit (ec);
		}
	}

	class Package : NamespaceContainer
	{
		static readonly MemberName DefaultPackageName = new MemberName (PredefinedTypes.RootNamespace, Location.Null);
		TypeDefinition globals;
		ToplevelBlock block;

		private Package (MemberName name, NamespaceContainer parent)
			: base (name, parent)
		{
		}

		public static Package Create (MemberName name, NamespaceContainer parent)
		{
			if (name == null)
				return new Package (DefaultPackageName, parent);

			return new Package (name, parent) {
				Usings = new List<UsingNamespace> () {
					new UsingNamespace (new SimpleName (PredefinedTypes.RootNamespace, name.Location), name.Location)
				}
			};
		}

		public override TypeSpec CurrentType {
			get {
				return globals == null ? null : globals.CurrentType;
			}
		}

		public bool IsTopLevel {
			get {
				return MemberName == DefaultPackageName;
			}
		}

		public ToplevelBlock GetBlock ()
		{
			if (block == null)
				block = new ToplevelBlock (Compiler, ParametersCompiled.EmptyReadOnlyParameters, Location);

			return block;
		}

		public override string GetSignatureForError ()
		{
			if (IsTopLevel)
				return "";

			return base.GetSignatureForError ();
		}

		public TypeDefinition GetGlobalsTypeDefinition ()
		{
			if (globals == null) {
				globals = new PackageGlobalContainer (this);
				AddTypeContainer (globals);
			}

			return globals;
		}

		public override void EmitContainer ()
		{
			if (block != null) {
				var bc = new BlockContext (this, block, Compiler.BuiltinTypes.Void);
				bc.Set (ResolveContext.Options.ConstructorScope);

				if (block.Resolve (null, bc, null)) {
					var cb = Module.DefineModuleInitializer ();

					EmitContext ec = new EmitContext (this, cb.GetILGenerator (), bc.ReturnType, null);
					ec.With (EmitContext.Options.ConstructorScope, true);

					block.Emit (ec);
				}
			}

			base.EmitContainer ();
		}
	}

	class PackageGlobalContainer : CompilerGeneratedContainer
	{
		public const string Name = "<Globals>";

		public PackageGlobalContainer (TypeContainer parent)
			: base (parent, new MemberName (Name), Modifiers.PUBLIC | Modifiers.STATIC)
		{
			IsPlayScriptType = true;
		}

		public override void Emit ()
		{
			base.Emit ();

			Module.PlayscriptAttributes.PlayScript.EmitAttribute (TypeBuilder);	
		}

		public override string GetSignatureForError ()
		{
			return null;
		}
	}

	class PredefinedTypes
	{
		public readonly BuiltinTypeSpec Object;
		public readonly BuiltinTypeSpec Function;
		public readonly BuiltinTypeSpec Class;
		public readonly BuiltinTypeSpec Number;
		public readonly BuiltinTypeSpec String;

		public readonly PredefinedType Vector;
		public readonly PredefinedType Array;
		public readonly PredefinedType Error;
		public readonly PredefinedType RegExp;
		public readonly PredefinedType Xml;
		public readonly PredefinedType Namespace;

		public readonly PredefinedType Binder;
		public readonly PredefinedType Operations;
		public readonly PredefinedType DefaultObjectContext;
		public readonly PredefinedType UndefinedValueType;


		//
		// Internal type used for undefined type comparisons
		//
		public readonly BuiltinTypeSpec UndefinedType;

		//
		// The namespace used for the root package.
		//
		public const string RootNamespace = "_root";

		public PredefinedTypes (ModuleContainer module)
		{
			var root_ns = module.GlobalRootNamespace.GetNamespace (RootNamespace, true);
			var builtin_types = module.Compiler.BuiltinTypes;
	
			// Setup type aliases

			Object = new BuiltinTypeSpec ("Object", BuiltinTypeSpec.Type.Object);
			Object.SetDefinition (builtin_types.Object);
			Object.Modifiers |= Modifiers.DYNAMIC;
			root_ns.AddType (module, Object);

			Function = new BuiltinTypeSpec (MemberKind.Delegate, RootNamespace, "Function", BuiltinTypeSpec.Type.Delegate);
			Function.SetDefinition (builtin_types.Delegate);
			Function.Modifiers |= Modifiers.SEALED;
			root_ns.AddType (module, Function);

			Class = new BuiltinTypeSpec (MemberKind.Class, RootNamespace, "Class", BuiltinTypeSpec.Type.Type);
			Class.SetDefinition (builtin_types.Type);
			Class.Modifiers |= Modifiers.DYNAMIC;
			root_ns.AddType (module, Class);

			String = new BuiltinTypeSpec (MemberKind.Class, RootNamespace, "String", BuiltinTypeSpec.Type.String);
			String.SetDefinition (module.Compiler.BuiltinTypes.String);
			String.Modifiers |= Modifiers.SEALED;
			root_ns.AddType (module, String);

			// For now use same instance
			UndefinedType = builtin_types.Dynamic;
			Number = builtin_types.Double;

			// Known predefined types
			Array = new PredefinedType (module, MemberKind.Class, RootNamespace, "Array");
			Vector = new PredefinedType (module, MemberKind.Class, RootNamespace, "Vector", 1);
			Error = new PredefinedType (module, MemberKind.Class, RootNamespace, "Error");
			RegExp = new PredefinedType (module, MemberKind.Class, RootNamespace, "RegExp");
			Xml = new PredefinedType (module, MemberKind.Class, RootNamespace, "XML");
			Namespace = new PredefinedType (module, MemberKind.Class, RootNamespace, "Namespace");

			Binder = new PredefinedType (module, MemberKind.Class, "PlayScript.Runtime", "Binder");
			Operations = new PredefinedType (module, MemberKind.Class, "PlayScript.Runtime", "Operations");
			DefaultObjectContext = new PredefinedType (module, MemberKind.Class, "PlayScript.Runtime", "DefaultObjectContext");
			UndefinedValueType = new PredefinedType (module, MemberKind.Class, "PlayScript.Runtime", "Undefined");

			// Define types which are used for early comparisons
			Array.Define ();
		}

		public void PopulateImplicitMembers (ModuleContainer module)
		{
			var builtin_types = module.Compiler.BuiltinTypes;

			Object.MemberCache = new MemberCache (builtin_types.Object.MemberCache, true);

			var tostring_filter = CSharp.MemberFilter.Method ("ToString", 0, ParametersCompiled.EmptyReadOnlyParameters, null);
			var tostring = MemberCache.FindMember (Object, tostring_filter, BindingRestriction.DeclaredOnly) as MethodSpec;
			if (tostring != null) {
				Object.MemberCache.AddMember (
					new MethodSpec (MemberKind.Method, Object, tostring.MemberDefinition, tostring.ReturnType, tostring.Parameters, tostring.Modifiers),
					"toString");
			}

			String.MemberCache = new MemberCache (builtin_types.String.MemberCache, true);

			var length = MemberCache.FindMember (String, CSharp.MemberFilter.Property ("Length", null), BindingRestriction.DeclaredOnly) as PropertySpec;
			if (length != null) {
				String.MemberCache.AddMember (
					new PropertySpec (MemberKind.Property, String, length.MemberDefinition, length.MemberType, length.MetaInfo, length.Modifiers) {
						Get = length.Get
					}, "length");
			}

			if (Error.Define () && tostring != null) {
				Error.TypeSpec.MemberCache.AddMember (
					new MethodSpec (MemberKind.Method, Error.TypeSpec, tostring.MemberDefinition, tostring.ReturnType, tostring.Parameters, tostring.Modifiers),
					"toString");
			}
		}
	}

	class PredefinedMembers
	{
		public readonly PredefinedMember<MethodSpec> ArrayPush;
		public readonly PredefinedMember<MethodSpec> VectorPush;
		public readonly PredefinedMember<MethodSpec> BinderGetMember;
		public readonly PredefinedMember<MethodSpec> BinderGetKeys;
		public readonly PredefinedMember<MethodSpec> BinderGetValues;
		public readonly PredefinedMember<MethodSpec> BinderSetMember;
		public readonly PredefinedMember<MethodSpec> BinderHasProperty;
		public readonly PredefinedMember<MethodSpec> BinderDeleteProperty;
		public readonly PredefinedMember<MethodSpec> BinderDelegateInvoke;
		public readonly PredefinedMember<MethodSpec> OperationsTypeOf;
		public readonly PredefinedMember<MethodSpec> OperationsClassOf;
		public readonly PredefinedMember<MethodSpec> OperationsIsOfType;
		public readonly PredefinedMember<MethodSpec> DefaultObjectContextCreate;
		public readonly PredefinedMember<MethodSpec> DefaultObjectContextRegister;
		public readonly PredefinedMember<MethodSpec> DefaultObjectContextUnregister;
		public readonly PredefinedMember<FieldSpec> UndefinedValue;

		public PredefinedMembers (ModuleContainer module)
		{
			var types = module.PredefinedTypes;
			var btypes = module.Compiler.BuiltinTypes;
			var ptypes = module.PlayscriptTypes;

			var tp = new TypeParameter (0, new MemberName ("T"), null, null, Variance.None);

			ArrayPush = new PredefinedMember<MethodSpec> (module, ptypes.Array, "push", btypes.Object);
			VectorPush = new PredefinedMember<MethodSpec> (module, ptypes.Vector, "push", new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null));
			BinderGetMember = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "GetMember", btypes.Object, btypes.Type, btypes.Object);
			BinderGetKeys = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "GetKeys", btypes.Object);
			BinderGetValues = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "GetValues", btypes.Object);
			BinderSetMember = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "SetMember", btypes.Object, btypes.Type, btypes.Object, btypes.Object);
			BinderDeleteProperty = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "DeleteProperty", btypes.Object, btypes.Object);
			BinderDelegateInvoke = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "DelegateInvoke", btypes.Delegate, ArrayContainer.MakeType (module, btypes.Object));
			BinderHasProperty = new PredefinedMember<MethodSpec> (module, ptypes.Binder, "HasProperty", btypes.Object, btypes.Type, btypes.Object);
			OperationsTypeOf = new PredefinedMember<MethodSpec> (module, ptypes.Operations, "TypeOf", btypes.Object);
			OperationsClassOf = new PredefinedMember<MethodSpec> (module, ptypes.Operations, "ClassOf", btypes.Object);
			OperationsIsOfType = new PredefinedMember<MethodSpec> (module, ptypes.Operations, CSharp.MemberFilter.Method ("IsOfType", 1, null, null));
			DefaultObjectContextCreate = new PredefinedMember<MethodSpec> (module, ptypes.DefaultObjectContext, "Create");
			DefaultObjectContextRegister = new PredefinedMember<MethodSpec> (module, ptypes.DefaultObjectContext, "Register", btypes.Object);
			DefaultObjectContextUnregister = new PredefinedMember<MethodSpec> (module, ptypes.DefaultObjectContext, "Unregister");
			UndefinedValue = new PredefinedMember<FieldSpec> (module, ptypes.UndefinedValueType, CSharp.MemberFilter.Field ("Value", btypes.Object));
		}
	}

	class PredefinedAttributes
	{
		public class PredefinedConstantAttribute : PredefinedAttribute
		{
			PredefinedMember<MethodSpec> ctor_definition;

			public PredefinedConstantAttribute (ModuleContainer module, string ns, string name)
				: base (module, ns, name)
			{
			}

			public void EmitAttribute (IMemberContext mc, PropertyBuilder builder, Expression constant)
			{
				if (ctor_definition == null) {
					if (!Define ())
						return;

					ctor_definition = new PredefinedMember<MethodSpec> (module, type, CSharp.MemberFilter.Constructor (
						ParametersCompiled.CreateFullyResolved (module.Compiler.BuiltinTypes.Object)));
				}

				var ctor = ctor_definition.Get ();
				if (ctor == null)
					return;

				AttributeEncoder encoder = new AttributeEncoder ();
				constant.EncodeAttributeValue (mc, encoder, ctor.Parameters.Types [0]);
				encoder.EncodeEmptyNamedArguments ();

				builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), encoder.ToArray ());
			}
		}

		public class PredefinedNamespaceAttribute : PredefinedAttribute
		{
			PredefinedMember<MethodSpec> ctor_definition;

			public PredefinedNamespaceAttribute (ModuleContainer module, string ns, string name)
				: base (module, ns, name)
			{
			}

			public void EmitAttribute (FieldBuilder builder, string value)
			{
				if (ctor_definition == null) {
					if (!Define ())
						return;

					ctor_definition = new PredefinedMember<MethodSpec> (module, type, CSharp.MemberFilter.Constructor (
						ParametersCompiled.CreateFullyResolved (module.Compiler.BuiltinTypes.String)));
				}

				var ctor = ctor_definition.Get ();
				if (ctor == null)
					return;

				AttributeEncoder encoder = new AttributeEncoder ();
				encoder.Encode (value);
				encoder.EncodeEmptyNamedArguments ();

				builder.SetCustomAttribute ((ConstructorInfo) ctor.GetMetaInfo (), encoder.ToArray ());
			}
		}

		public readonly PredefinedConstantAttribute ConstantField;
		public readonly PredefinedNamespaceAttribute NamespaceField;
		public readonly PredefinedAttribute DynamicClass;
		public readonly PredefinedAttribute PlayScript;
		public readonly PredefinedAttribute RestArrayParameter;

		public PredefinedAttributes (ModuleContainer module)
		{
			var ns = "PlayScript.Runtime.CompilerServices";
			ConstantField = new PredefinedConstantAttribute (module, ns, "ConstantFieldAttribute");
			NamespaceField = new PredefinedNamespaceAttribute (module, ns, "NamespaceFieldAttribute");
			DynamicClass = new PredefinedAttribute (module, ns, "DynamicClassAttribute");
			PlayScript = new PredefinedAttribute (module, ns, "PlayScriptAttribute");
			RestArrayParameter = new PredefinedAttribute (module, ns, "RestArrayParameterAttribute");
		}
	}

	static class Convert
	{
		public static Expression ImplicitConversion (ResolveContext rc, Expression expr, TypeSpec target)
		{
			Expression e;

			e = ImplicitReferenceConversion (rc, expr, expr.Type, target);
			if (e != null)
				return e;

			e = ImplicitNumericConversion (expr, expr.Type, target);
			if (e != null)
				return e;

			return null;
		}

		static Expression ImplicitReferenceConversion (ResolveContext rc, Expression expr, TypeSpec exprType, TypeSpec targetType)
		{
			if (targetType == rc.Module.PlayscriptTypes.Class && exprType == rc.BuiltinTypes.Type)
				return expr;

			if (exprType.BuiltinType == BuiltinTypeSpec.Type.Dynamic && targetType == rc.Module.PlayscriptTypes.Object)
				return expr;

			return null;
		}

		static Expression ImplicitNumericConversion (Expression expr, TypeSpec expr_type, TypeSpec target_type)
		{
			switch (expr_type.BuiltinType) {
			case BuiltinTypeSpec.Type.Int:
				//
				// From int to uint
				//
				switch (target_type.BuiltinType) {
				case BuiltinTypeSpec.Type.UInt:
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U4);
				}

				break;

			case BuiltinTypeSpec.Type.UInt:
				//
				// From uint to int
				//
				switch (target_type.BuiltinType) {
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I4);
				}

				break;
			case BuiltinTypeSpec.Type.Double:
				//
				// From double to int
				//
				switch (target_type.BuiltinType) {
				case BuiltinTypeSpec.Type.Int:
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U4);
				}
				break;
			}

			return null;
		}
	}

	sealed class ErrorMessage : AbstractMessage
	{
		public ErrorMessage (int code, Location loc, string message, List<string> extraInfo)
			: base (code, loc, message, extraInfo)
		{
		}

		public ErrorMessage (AbstractMessage aMsg)
			: base (aMsg)
		{
		}

		public override bool IsWarning {
			get {
				return false;
			}
		}

		public override string LanguagePrefix {
			get {
				return "PS";
			}
		}

		public override string MessageType {
			get {
				return "error";
			}
		}
	}

	sealed class WarningMessage : AbstractMessage
	{
		public WarningMessage (int code, Location loc, string message, List<string> extra_info)
			: base (code, loc, message, extra_info)
		{
		}

		public override bool IsWarning {
			get {
				return true;
			}
		}

		public override string MessageType {
			get {
				return "warning";
			}
		}

		public override string LanguagePrefix {
			get {
				return "PS";
			}
		}
	}

#region SLE support

	static class Expressions
	{
		public static SLE.Expression StrictEqual (SLE.Expression left, SLE.Expression right)
		{
			return new BinaryExpression (50 | 1 >> 16, left, right);
		}
		
		public static SLE.Expression StrictNotEqual (SLE.Expression left, SLE.Expression right)
		{
			return new BinaryExpression (51 | 1 >> 16, left, right);
		}
	}

	class BinaryExpression : SLE.Expression
	{
		readonly SLE.Expression left, right;
		readonly int oper;

		public BinaryExpression (int oper, SLE.Expression left, SLE.Expression right)
		{
			this.oper = oper;
			this.left = left;
			this.right = right;
		}

		public override bool CanReduce {
			get {
				return true;
			}
		}

		public override SLE.ExpressionType NodeType {
			get {
				return SLE.ExpressionType.Extension;
			}
		}

		public override System.Type Type {
			get {
				return typeof (bool);
			}
		}

		public override SLE.Expression Reduce ()
		{
			// TODO: Do something about it
			// Probably need to move the comparison code to avoid cyclic dependency
			var asm = System.Reflection.Assembly.Load (new System.Reflection.AssemblyName ("PlayScript.Core"));
			var t = asm.GetType ("PlayScript.Runtime.Operations", true);
			var tt = asm.GetType ("PlayScript.Runtime.BinaryOperator", true);
			var m = t.GetMethod ("Comparison", new [] { tt, typeof (object), typeof (object) });

			return SLE.Expression.Call (m, SLE.Expression.Constant (System.Enum.ToObject (tt, oper)),
				SLE.Expression.Convert (left, typeof (object)),
				SLE.Expression.Convert (right, typeof (object)));
		}
	}


#endregion

}
