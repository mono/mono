//
// assign.cs: Assignments.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	/// <summary>
	///   This interface is implemented by expressions that can be assigned to.
	/// </summary>
	/// <remarks>
	///   This interface is implemented by Expressions whose values can not
	///   store the result on the top of the stack.
	///
	///   Expressions implementing this (Properties, Indexers and Arrays) would
	///   perform an assignment of the Expression "source" into its final
	///   location.
	///
	///   No values on the top of the stack are expected to be left by
	///   invoking this method.
	/// </remarks>
	public interface IAssignMethod {
		void EmitAssign (EmitContext ec, Expression source);
	}

	/// <summary>
	///   An Expression to hold a temporary value.
	/// </summary>
	/// <remarks>
	///   The LocalTemporary class is used to hold temporary values of a given
	///   type to "simulate" the expression semantics on property and indexer
	///   access whose return values are void.
	///
	///   The local temporary is used to alter the normal flow of code generation
	///   basically it creates a local variable, and its emit instruction generates
	///   code to access this value, return its address or save its value.
	/// </remarks>
	public class LocalTemporary : Expression, IMemoryLocation {
		LocalBuilder builder;
		
		public LocalTemporary (EmitContext ec, Type t)
		{
			type = t;
			eclass = ExprClass.Value;
			builder = ec.GetTemporaryStorage (t);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldloc, builder); 
		}

		public void Store (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Stloc, builder);
		}

		public void AddressOf (EmitContext ec)
		{
			ec.ig.Emit (OpCodes.Ldloca, builder);
		}
	}

	/// <summary>
	///   The Assign node takes care of assigning the value of source into
	///   the expression represented by target. 
	/// </summary>
	public class Assign : ExpressionStatement {
		Expression target, source;
		Location l;

		public Assign (Expression target, Expression source, Location l)
		{
			this.target = target;
			this.source = source;
			this.l = l;
		}

		public Expression Target {
			get {
				return target;
			}

			set {
				target = value;
			}
		}

		public Expression Source {
			get {
				return source;
			}

			set {
				source = value;
			}
		}

		void error70 (EventInfo ei)
		{
			Report.Error (70, l, "The event '" + ei.Name +
				      "' can only appear on the left-side of a += or -= (except when" +
				      " used from within the type '" + ei.DeclaringType + "')");
		}

		public override Expression DoResolve (EmitContext ec)
		{
			source = source.Resolve (ec);
			if (source == null)
				return null;

			target = target.ResolveLValue (ec, source);
			
			if (target == null)
				return null;

			Type target_type = target.Type;
			Type source_type = source.Type;

			type = target_type;
			eclass = ExprClass.Value;
			
			//
			// If we are doing a property assignment, then
			// set the `value' field on the property, and Resolve
			// it.
			//
			if (target is PropertyExpr){
				PropertyExpr property_assign = (PropertyExpr) target;

				//
				// FIXME: Maybe handle this in the LValueResolve
				//
				if (!property_assign.VerifyAssignable ())
					return null;
				
				return this;
			}

			if (target is IndexerAccess){
				IndexerAccess ia = (IndexerAccess) target;

				return this;
			}

			if (target is EventExpr) {

				Binary tmp;
				EventInfo ei = ((EventExpr) target).EventInfo;

				//
				// If this is the case, then the Event does not belong 
				// to this TypeContainer and so, according to the spec
				// is allowed to only appear on the left hand of
				// the += and -= operators
				//
				// Note that if target will not appear as an EventExpr
				// in the case it is being referenced within the same type container;
				// it will appear as a FieldExpr in that case.
				
				if (!(source is Binary)) {
					error70 (ei);
					return null;
				} else {
					tmp = ((Binary) source);
					if (tmp.Oper != Binary.Operator.Addition &&
					    tmp.Oper != Binary.Operator.Subtraction) {
						error70 (ei);
						return null;
					}
				}
				
				return this;
			}

			if (source is New && target_type.IsSubclassOf (TypeManager.value_type)){
				New n = (New) source;

				n.ValueTypeVariable = target;

				return n;
			}

			if (target_type != source_type){
				source = ConvertImplicitRequired (ec, source, target_type, l);
				if (source == null)
					return null;
			}

			if (target.eclass != ExprClass.Variable){
				Report.Error (131, l,
					      "Left hand of an assignment must be a variable, " +
					      "a property or an indexer");
				return null;
			}

			return this;
		}

		void Emit (EmitContext ec, bool is_statement)
		{
			if (!EventIsLocal && target is EventExpr) {
				((EventExpr) target).EmitAddOrRemove (ec, source);
				return;
			}
			
			//
			// FIXME! We need a way to "probe" if the process can
			// just use `dup' to propagate the result
			// 
			IAssignMethod am = (IAssignMethod) target;
			
			if (is_statement)
				am.EmitAssign (ec, source);
			else {
				LocalTemporary tempo;
				
				tempo = new LocalTemporary (ec, source.Type);
				
				source.Emit (ec);
				tempo.Store (ec);
				am.EmitAssign (ec, source);
				tempo.Emit (ec);
			}
		}
		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec, true);
		}
	}
}




