//
// delegate.cs: Delegate Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace CIR {
	
	public class Delegate {

		public readonly string Name;
		public readonly string ReturnType;
		public int             mod_flags;
		public Parameters      Parameters;
		public Attributes      OptAttributes;
		public TypeBuilder     TypeBuilder;

		public ConstructorBuilder ConstructorBuilder;
		public MethodBuilder      InvokeBuilder;
		public MethodBuilder      BeginInvokeBuilder;
		public MethodBuilder      EndInvokeBuilder;
		
		public readonly RootContext RootContext;

		Type [] param_types;
		Type ret_type;
		
		Expression instance_expr;
		MethodBase delegate_method;
	
		Location loc;

		const int AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;

		public Delegate (RootContext rc, string type, int mod_flags, string name, Parameters param_list,
				 Attributes attrs, Location loc)
		{
			this.RootContext = rc;
			this.Name       = name;
			this.ReturnType = type;
			this.mod_flags  = Modifiers.Check (AllowedModifiers, mod_flags, Modifiers.PUBLIC);
			Parameters      = param_list;
			OptAttributes   = attrs;
			this.loc        = loc;
		}

		public void DefineDelegate (object parent_builder)
		{
			TypeAttributes attr;
			
			if (parent_builder is ModuleBuilder) {
				ModuleBuilder builder = (ModuleBuilder) parent_builder;
				
				attr = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;

				TypeBuilder = builder.DefineType (Name, attr, TypeManager.delegate_type);
								  
			} else {
				TypeBuilder builder = (TypeBuilder) parent_builder;
				
				attr = TypeAttributes.NestedPublic | TypeAttributes.Class | TypeAttributes.Sealed;

				TypeBuilder = builder.DefineNestedType (Name, attr, TypeManager.delegate_type);

			}

			RootContext.TypeManager.AddDelegateType (Name, TypeBuilder, this);
		}

		public void Populate (TypeContainer parent)
		{

			MethodAttributes mattr;
			int i;
			
			Type [] const_arg_types = new Type [2];

			const_arg_types [0] = TypeManager.object_type;
			const_arg_types [1] = TypeManager.intptr_type;

			mattr = MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
				MethodAttributes.HideBySig | MethodAttributes.Public;

			ConstructorBuilder = TypeBuilder.DefineConstructor (mattr,
									    CallingConventions.Standard,
									    const_arg_types);
			
			ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			
			// Here the various methods like Invoke, BeginInvoke etc are defined

 			param_types = Parameters.GetParameterInfo (parent);
  			ret_type = parent.LookupType (ReturnType, false);
  			CallingConventions cc = Parameters.GetCallingConvention ();

 			mattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

 			InvokeBuilder = TypeBuilder.DefineMethod ("Invoke", 
 								  mattr,		     
 								  cc,
 								  ret_type,		     
 								  param_types);

			for (i = 0 ; i < param_types.Length; i++) {
				Parameter p = Parameters.FixedParameters [i];
				string name = p.Name;
				ParameterBuilder pb = InvokeBuilder.DefineParameter (i+1, p.Attributes, name); 
			}
			
			InvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			int params_num = param_types.Length;
			Type [] async_param_types = new Type [params_num + 2];

			param_types.CopyTo (async_param_types, 0);

			async_param_types [params_num] = TypeManager.asynccallback_type;
			async_param_types [params_num + 1] = TypeManager.object_type;

			mattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual |
				MethodAttributes.NewSlot;
			
			BeginInvokeBuilder = TypeBuilder.DefineMethod ("BeginInvoke",
								       mattr,
								       cc,
								       TypeManager.iasyncresult_type,
								       async_param_types);

			for (i = 0 ; i < param_types.Length; i++) {
				Parameter p = Parameters.FixedParameters [i];
				string name = p.Name;
				BeginInvokeBuilder.DefineParameter (i + 1, p.Attributes, name); 
			}
			
			BeginInvokeBuilder.DefineParameter (i + 1, ParameterAttributes.None, "callback");
			BeginInvokeBuilder.DefineParameter (i + 2, ParameterAttributes.None, "object");
			
			BeginInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			Type [] end_param_types = new Type [1];

			end_param_types [0] = TypeManager.iasyncresult_type;
			
			EndInvokeBuilder = TypeBuilder.DefineMethod ("EndInvoke",
								     mattr,
								     cc,
								     ret_type,
								     end_param_types);
			EndInvokeBuilder.DefineParameter (1, ParameterAttributes.None, "result");
			
			EndInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);
			
		}

		// <summary>
		//  Verifies whether the method in question is compatible with the delegate
		//  Returns the method itself if okay and null if not.
		// </summary>
		public MethodBase VerifyMethod (MethodBase mb, Location loc)
		{
			ParameterData pd = Invocation.GetParameterData (mb);

			bool mismatch = false;
			for (int i = param_types.Length; i > 0; ) {
				i--;

				if (param_types [i] == pd.ParameterType (i))
					continue;
				else {
					mismatch = true;
					break;
				}
			}

			if (mismatch) {
				Report.Error (123, loc, "Method '" + Invocation.FullMethodDesc (mb) + "' does not match " +
					      "delegate '" + FullDelegateDesc () + "'");
				return null;
			}

			if (ret_type == ((MethodInfo) mb).ReturnType)
				return mb;
			else
				mismatch = true;

			if (mismatch) {
				Report.Error (123, loc, "Method '" + Invocation.FullMethodDesc (mb) + "' does not match " +
					      "delegate '" + FullDelegateDesc () + "'");
				return null;
			}

			return null;
		}

		// <summary>
		//  Verifies whether the invocation arguments are compatible with the
		//  delegate's target method
		// </summary>
		public bool VerifyApplicability (EmitContext ec, ArrayList args, Location loc)
		{
			int arg_count;

			if (args == null)
				arg_count = 0;
			else
				arg_count = args.Count;
			
			if (param_types.Length != arg_count) {
				Report.Error (1593, loc,
					      "Delegate '" + Name + "' does not take '" + arg_count + "' arguments");
				return false;
			}

			for (int i = arg_count; i > 0;) {
				i--;
				Expression conv;
				Argument a = (Argument) args [i];
				Expression a_expr = a.Expr;
				
				if (param_types [i] != a_expr.Type) {
					
					conv = Expression.ConvertImplicitStandard (ec, a_expr, param_types [i], loc);

					if (conv == null) {
						Report.Error (1594, loc,
							      "Delegate '" + Name + "' has some invalid arguments.");

						Report.Error (1503, loc,
						       "Argument " + (i+1) +
						       ": Cannot convert from '" +
						       TypeManager.CSharpName (a_expr.Type)
						       + "' to '" + TypeManager.CSharpName (param_types [i]) + "'");
						return false;
					}

					if (a_expr != conv)
						a.Expr = conv;
				}
			}

			return true;
		}

		// <summary>
		//  Verifies whether the delegate in question is compatible with this one in
		//  order to determine if instantiation from the same is possible.
		// </summary>
		public bool VerifyDelegate (Delegate del)
		{
			if (ret_type != del.TargetReturnType)
				return false;

			Type [] other_param_types = del.ParameterTypes;
			
			if (param_types.Length != other_param_types.Length)
				return false;

			for (int i = param_types.Length; i > 0; ) {
				i--;

				if (param_types [i] != other_param_types [i])
					return false;
			}

			// FIXME : Hey, what about parameter modifiers ?

			return true;

		}
		
		public string FullDelegateDesc ()
		{
			StringBuilder sb = new StringBuilder (TypeManager.CSharpName (System.Type.GetType (ReturnType)));
			
			sb.Append (" " + Name);
			sb.Append (" (");

			int length = param_types.Length;
			
			for (int i = length; i > 0; ) {
				i--;
				
				sb.Append (TypeManager.CSharpName (param_types [length - i - 1]));
				if (i != 0)
					sb.Append (", ");
			}
			
			sb.Append (")");
			return sb.ToString ();
			
		}
		
		// Hack around System.Reflection as found everywhere else
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf, MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();

			if ((mt & MemberTypes.Method) != 0) {
				if (filter (ConstructorBuilder, criteria))
					members.Add (ConstructorBuilder);

				if (filter (InvokeBuilder, criteria))
					members.Add (InvokeBuilder);

				if (filter (BeginInvokeBuilder, criteria))
					members.Add (BeginInvokeBuilder);

				if (filter (EndInvokeBuilder, criteria))
					members.Add (EndInvokeBuilder);
			}

			int count = members.Count;

			if (count > 0) {
				MemberInfo [] mi = new MemberInfo [count];
				members.CopyTo (mi, 0);
				return mi;
			}

			return null;
		}
		
		public void CloseDelegate ()
		{
			TypeBuilder.CreateType ();
		}
		
		public int ModFlags {
			get {
				return mod_flags;
			}
		}

		public Expression InstanceExpression {
			get {
				return instance_expr;
			}
			set {
				instance_expr = value;
			}
		}

		public MethodBase TargetMethod {
			get {
				return delegate_method;
			}
			set {
				delegate_method = value;
			}
		}

		public Type TargetReturnType {
			get {
				return ret_type;
			}
		}

		public Type [] ParameterTypes {
			get {
				return param_types;
			}
		}
		
	}

	public class NewDelegate : Expression {

		public ArrayList Arguments;

		MethodBase constructor_method;
		MethodBase delegate_method;
		Expression delegate_instance_expr;

		Location Location;
		
		public NewDelegate (Type type, ArrayList Arguments, Location loc)
		{
			this.type = type;
			this.Arguments = Arguments;
			this.Location  = loc; 
		}

		public override Expression DoResolve (EmitContext ec)
		{
			Delegate del = TypeManager.LookupDelegate (type);
			constructor_method = del.ConstructorBuilder;
			
			if (Arguments == null) {
				Report.Error (-11, Location,
					      "Delegate creation expression takes only one argument");
				return null;
			}
			
			if (Arguments.Count != 1) {
				Report.Error (-11, Location,
					      "Delegate creation expression takes only one argument");
				return null;
			}
			
			Argument a = (Argument) Arguments [0];
			
			if (!a.Resolve (ec))
				return null;
			
			Expression e = a.Expr;
			
			if (e is MethodGroupExpr) {
				MethodGroupExpr mg = (MethodGroupExpr) e;
				
				delegate_method  = del.VerifyMethod (mg.Methods [0], Location);
				
				if (delegate_method == null)
					return null;
				
				if (mg.InstanceExpression != null)
					delegate_instance_expr = mg.InstanceExpression.Resolve (ec);
				else
					delegate_instance_expr = null;
				
				if (delegate_instance_expr != null)
					if (delegate_instance_expr.Type.IsValueType)
						delegate_instance_expr = new BoxedCast (delegate_instance_expr);
				
				
				del.InstanceExpression = delegate_instance_expr;
				del.TargetMethod = delegate_method;
				
				eclass = ExprClass.Value;
				return this;
			}

			Type e_type = e.Type;

			Delegate d = TypeManager.LookupDelegate (e_type);

			if (d == null) {
				Report.Error (-12, Location, "Cannot create a delegate from something " +
					      "not a delegate or a method.");
				return null;
			}

			// This is what MS's compiler reports. We could always choose
			// to be more verbose and actually give delegate-level specifics
			
			if (!d.VerifyDelegate (del)) {
				Report.Error (29, Location, "Cannot implicitly convert type '" + d.Name + "' " +
					      "to type '" + del.Name + "'");
				return null;
			}

			delegate_instance_expr = d.InstanceExpression;
			delegate_method        = d.TargetMethod;
			
			del.InstanceExpression = d.InstanceExpression;
			del.TargetMethod       = d.TargetMethod;
			
			eclass = ExprClass.Value;
			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			if (delegate_instance_expr == null)
				ec.ig.Emit (OpCodes.Ldnull);
			else
				delegate_instance_expr.Emit (ec);
			
			ec.ig.Emit (OpCodes.Ldftn, (MethodInfo) delegate_method);
			ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) constructor_method);
		}
	}

	public class DelegateInvocation : ExpressionStatement {

		public Expression InstanceExpr;
		public ArrayList  Arguments;
		public Location   Location;

		MethodBase method;
		
		public DelegateInvocation (Expression instance_expr, ArrayList args, Location loc)
		{
			this.InstanceExpr = instance_expr;
			this.Arguments = args;
			this.Location = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			Delegate del = TypeManager.LookupDelegate (InstanceExpr.Type);

			if (del == null)
				return null;

			if (del.TargetMethod == null)
				return null;
			
			if (Arguments != null){
				for (int i = Arguments.Count; i > 0;){
					--i;
					Argument a = (Argument) Arguments [i];
					
					if (!a.Resolve (ec))
						return null;
				}
			}
			
			if (!del.VerifyApplicability (ec, Arguments, Location))
				return null;
			
			method = del.InvokeBuilder;
			type = ((MethodInfo) method).ReturnType;
			
			eclass = ExprClass.Value;
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Delegate del = TypeManager.LookupDelegate (InstanceExpr.Type);
			Invocation.EmitCall (ec, del.TargetMethod.IsStatic, InstanceExpr, method, Arguments);
		}

		public override void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			// 
			// Pop the return value if there is one
			//
			if (method is MethodInfo){
				if (((MethodInfo) method).ReturnType != TypeManager.void_type)
					ec.ig.Emit (OpCodes.Pop);
			}
		}

	}
}
