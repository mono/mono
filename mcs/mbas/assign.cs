//
// assign.cs: Assignments.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.
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
		//
		// This method will emit the code for the actual assignment
		//
		void EmitAssign (EmitContext ec, Expression source);

		//
		// This method is invoked before any code generation takes
		// place, and it is a mechanism to inform that the expression
		// will be invoked more than once, and that the method should
		// use temporary values to avoid having side effects
		//
		// Example: a [ g () ] ++
		//
		void CacheTemporaries (EmitContext ec);
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

		public void Release (EmitContext ec)
		{
			ec.FreeTemporaryStorage (builder);
			builder = null;
		}
		
		public LocalTemporary (LocalBuilder b, Type t)
		{
			type = t;
			eclass = ExprClass.Value;
			builder = b;
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

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			ec.ig.Emit (OpCodes.Ldloca, builder);
		}
	}

	/// <summary>
	///   The Assign node takes care of assigning the value of source into
	///   the expression represented by target. 
	/// </summary>
	public class Assign : ExpressionStatement {
		protected Expression target, source;
		public Location l;

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

		public static void error70 (EventInfo ei, Location l)
		{
			Report.Error (70, l, "The event '" + ei.Name +
				      "' can only appear on the left-side of a += or -= (except when" +
				      " used from within the type '" + ei.DeclaringType + "')");
		}

		//
		// Will return either `this' or an instance of `New'.
		//
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

				if (source_type != target_type){
					source = ConvertImplicitRequired (ec, source, target_type, l);
					if (source == null)
						return null;
				}

				//
				// FIXME: Maybe handle this in the LValueResolve
				//
				if (!property_assign.VerifyAssignable ())
					return null;
				
				return this;
			}

			if (target is IndexerAccess)
				return this;

			if (target is EventExpr) {

				Binary tmp;
				EventInfo ei = ((EventExpr) target).EventInfo;


				Expression ml = MemberLookup (
					ec, ec.ContainerType, ei.Name,
					MemberTypes.Event, AllBindingFlags, l);

				if (ml == null) {
				        //
					// If this is the case, then the Event does not belong 
					// to this Type and so, according to the spec
					// is allowed to only appear on the left hand of
					// the += and -= operators
					//
					// Note that target will not appear as an EventExpr
					// in the case it is being referenced within the same type container;
					// it will appear as a FieldExpr in that case.
					//
					
					if (!(source is Binary)) {
						error70 (ei, l);
						return null;
					} else {
						tmp = ((Binary) source);
						if (tmp.Oper != Binary.Operator.Addition &&
						    tmp.Oper != Binary.Operator.Subtraction) {
							error70 (ei, l);
							return null;
						}
					}
				}
			}
			
			if (source is New && target_type.IsValueType){
				New n = (New) source;

				n.ValueTypeVariable = target;
				return n;
			}

			if (target.eclass != ExprClass.Variable && target.eclass != ExprClass.EventAccess){
				Report.Error (131, l,
					      "Left hand of an assignment must be a variable, " +
					      "a property or an indexer");
				return null;
			}

			if (target_type == source_type)
				return this;
			
			//
			// If this assignemnt/operator was part of a compound binary
			// operator, then we allow an explicit conversion, as detailed
			// in the spec. 
			//

			if (this is CompoundAssign){
				CompoundAssign a = (CompoundAssign) this;
				
				Binary b = source as Binary;
				if (b != null && b.IsBuiltinOperator){
					//
					// 1. if the source is explicitly convertible to the
					//    target_type
					//
					
					source = ConvertExplicit (ec, source, target_type, l);
					if (source == null){
						Error_CannotConvertImplicit (l, source_type, target_type);
						return null;
					}
				
					//
					// 2. and the original right side is implicitly convertible to
					// the type of target_type.
					//
					if (StandardConversionExists (a.original_source, target_type))
						return this;

					Error_CannotConvertImplicit (l, a.original_source.Type, target_type);
					return null;
				}
			}
			
			source = ConvertImplicitRequired (ec, source, target_type, l);
			if (source == null)
				return null;

			return this;
		}

		void Emit (EmitContext ec, bool is_statement)
		{
			if (target is EventExpr) {
				((EventExpr) target).EmitAddOrRemove (ec, source);
				return;
			}

			//
			// FIXME! We need a way to "probe" if the process can
			// just use `dup' to propagate the result
			// 
			IAssignMethod am = (IAssignMethod) target;

			if (this is CompoundAssign){
				am.CacheTemporaries (ec);
			}
			
			if (is_statement)
				am.EmitAssign (ec, source);
			else {
				LocalTemporary tempo;
				
				tempo = new LocalTemporary (ec, source.Type);
				
				source.Emit (ec);
				tempo.Store (ec);
				am.EmitAssign (ec, tempo);
				tempo.Emit (ec);
				tempo.Release (ec);
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

	
	//
	// This class is used for compound assignments.  
	//
	class CompoundAssign : Assign {
		Binary.Operator op;
		public Expression original_source;
		
		public CompoundAssign (Binary.Operator op, Expression target, Expression source, Location l)
			: base (target, source, l)
		{
			original_source = source;
			this.op = op;
		}

		public Expression ResolveSource (EmitContext ec)
		{
			return original_source.Resolve (ec);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			target = target.ResolveLValue (ec, source);
			if (target == null)
				return null;

			original_source = original_source.Resolve (ec);
			if (original_source == null)
				return null;

			//
			// Only now we can decouple the original source/target
			// into a tree, to guarantee that we do not have side
			// effects.
			//
			source = new Binary (op, target, original_source, l);
			return base.DoResolve (ec);
		}
	}
}




