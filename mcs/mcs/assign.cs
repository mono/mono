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

namespace CIR {

	// <remarks>
	//   This interface is implemented by Expressions whose values can not
	//   store the result on the top of the stack.
	//
	//   Expressions implementing this (Properties and Indexers) would
	//   perform an assignment of the Expression "source" into its final
	//   location.
	//
	//   No values on the top of the stack are expected to be left by
	//   invoking this method.
	// </remarks>
	public interface IAssignMethod {
		void EmitAssign (EmitContext ec, Expression source);
	}

	// <remarks>
	//   The LocalTemporary class is used to hold temporary values of a given
	//   type to "simulate" the expression semantics on property and indexer
	//   access whose return values are void.
	//
	//   The local temporary is used to alter the normal flow of code generation
	//   basically it creates a local variable, and its emit instruction generates
	//   code to access this value, return its address or save its value.
	// </remarks>
	public class LocalTemporary : Expression, IStackStore, IMemoryLocation {
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

	// <remarks>
	//   The Assign node takes care of assigning the value of source into
	//   the expression represented by target. 
	// </remarks>
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

			if (target.ExprClass != ExprClass.Variable){
				Report.Error (131, l,
					      "Left hand of an assignment must be a variable, " +
					      "a property or an indexer");
				return null;
			}

			return this;
		}

		void Emit (EmitContext ec, bool is_statement)
		{
			ILGenerator ig = ec.ig;
			ExprClass eclass = target.ExprClass;
			
			if (eclass == ExprClass.Variable){

				//
				// If it is an instance field, load the this pointer
				//
				if (target is FieldExpr){
					FieldExpr fe = (FieldExpr) target;
					
					if (!fe.FieldInfo.IsStatic)
						ig.Emit (OpCodes.Ldarg_0);
				}

				source.Emit (ec);

				if (!is_statement)
					ig.Emit (OpCodes.Dup);

				((IStackStore) target).Store (ec);
			} else if (eclass == ExprClass.PropertyAccess ||
				   eclass == ExprClass.IndexerAccess){
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
			} else {
				Console.WriteLine ("Unhandled class: " + eclass + "\n Type:" + target);
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




