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

namespace Mono.MonoBASIC {

	/// <summary>
	///   Holds Delegates
	/// </summary>
	public class Delegate : DeclSpace {
 		public Expression ReturnType;
		public Parameters      Parameters;

		public ConstructorBuilder ConstructorBuilder;
		public MethodBuilder      InvokeBuilder;
		public MethodBuilder      BeginInvokeBuilder;
		public MethodBuilder      EndInvokeBuilder;
		
		Type [] param_types;
		Type ret_type;
		
		Expression instance_expr;
		MethodBase delegate_method;
	
		const int AllowedModifiers =
			Modifiers.SHADOWS |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
		    Modifiers.UNSAFE |
			Modifiers.PRIVATE;

 		public Delegate (TypeContainer parent, Expression type, int mod_flags,
				 string name, Parameters param_list,
				 Attributes attrs, Location l)
			: base (parent, name, attrs, l)
		{
			this.ReturnType = type;

			ModFlags        = Modifiers.Check (AllowedModifiers, mod_flags,
							   IsTopLevel ? Modifiers.INTERNAL :
							   Modifiers.PUBLIC, l);
			Parameters      = param_list;
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Delegate;
			}
		}

		public override TypeBuilder DefineType ()
		{
			TypeAttributes attr;

			if (TypeBuilder != null)
				return TypeBuilder;
			
			if (IsTopLevel) {
				ModuleBuilder builder = CodeGen.ModuleBuilder;
				attr = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed;

				TypeBuilder = builder.DefineType (
					Name, attr, TypeManager.multicast_delegate_type);
			} else {
				TypeBuilder builder = Parent.TypeBuilder;
				attr = TypeAttributes.NestedPublic | TypeAttributes.Class |
					TypeAttributes.Sealed;

				string name = Name.Substring (1 + Name.LastIndexOf ('.'));
				TypeBuilder = builder.DefineNestedType (
					name, attr, TypeManager.multicast_delegate_type);
			}

			TypeManager.AddDelegateType (Name, TypeBuilder, this);

			return TypeBuilder;
		}

 		public override bool DefineMembers (TypeContainer container)
		{
			return true;
		}

 		public override bool Define (TypeContainer container)
		{
			MethodAttributes mattr;
			int i;

			// FIXME: POSSIBLY make this static, as it is always constant
			//
			Type [] const_arg_types = new Type [2];
			const_arg_types [0] = TypeManager.object_type;
			const_arg_types [1] = TypeManager.intptr_type;

			mattr = MethodAttributes.RTSpecialName | MethodAttributes.SpecialName |
				MethodAttributes.HideBySig | MethodAttributes.Public;

			ConstructorBuilder = TypeBuilder.DefineConstructor (mattr,
									    CallingConventions.Standard,
									    const_arg_types);

			ConstructorBuilder.DefineParameter (1, ParameterAttributes.None, "object");
			ConstructorBuilder.DefineParameter (2, ParameterAttributes.None, "method");
			//
			// HACK because System.Reflection.Emit is lame
			//
			//
			// FIXME: POSSIBLY make these static, as they are always the same
			Parameter [] fixed_pars = new Parameter [2];
			fixed_pars [0] = new Parameter (null, null, Parameter.Modifier.NONE, null);
			fixed_pars [1] = new Parameter (null, null, Parameter.Modifier.NONE, null);
			Parameters const_parameters = new Parameters (fixed_pars, null, Location);
			
			TypeManager.RegisterMethod (
				ConstructorBuilder,
				new InternalParameters (const_arg_types, const_parameters),
				const_arg_types);
				
			
			ConstructorBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			//
			// Here the various methods like Invoke, BeginInvoke etc are defined
			//
			// First, call the `out of band' special method for
			// defining recursively any types we need:
			
			if (!Parameters.ComputeAndDefineParameterTypes (this))
				return false;
			
 			param_types = Parameters.GetParameterInfo (this);
			if (param_types == null)
				return false;

			//
			// Invoke method
			//

			// Check accessibility
			foreach (Type partype in param_types)
				if (!container.AsAccessible (partype, ModFlags)) {
					Report.Error (59, Location,
						      "Inconsistent accessibility: parameter type `" +
						      TypeManager.MonoBASIC_Name (partype) + "` is less " +
						      "accessible than delegate `" + Name + "'");
					return false;
				}
			
 			ReturnType = ResolveTypeExpr (ReturnType, false, Location);
   			ret_type = ReturnType.Type;
			if (ret_type == null)
				return false;

			if (!container.AsAccessible (ret_type, ModFlags)) {
				Report.Error (58, Location,
					      "Inconsistent accessibility: return type `" +
					      TypeManager.MonoBASIC_Name (ret_type) + "` is less " +
					      "accessible than delegate `" + Name + "'");
				return false;
			}

			//
			// We don't have to check any others because they are all
			// guaranteed to be accessible - they are standard types.
			//
			
  			CallingConventions cc = Parameters.GetCallingConvention ();

 			mattr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;

 			InvokeBuilder = TypeBuilder.DefineMethod ("Invoke", 
 								  mattr,		     
 								  cc,
 								  ret_type,		     
 								  param_types);

			i = 0;
			if (Parameters.FixedParameters != null){
				int top = Parameters.FixedParameters.Length;
				Parameter p;
				
				for (; i < top; i++) {
					p = Parameters.FixedParameters [i];

					InvokeBuilder.DefineParameter (
						i+1, p.Attributes, p.Name);
				}
			}
			if (Parameters.ArrayParameter != null){
				Parameter p = Parameters.ArrayParameter;
				
				InvokeBuilder.DefineParameter (
					i+1, p.Attributes, p.Name);
			}
			
			InvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			TypeManager.RegisterMethod (InvokeBuilder,
						    new InternalParameters (container, Parameters),
						    param_types);

			//
			// BeginInvoke
			//
			int params_num = param_types.Length;
			Type [] async_param_types = new Type [params_num + 2];

			param_types.CopyTo (async_param_types, 0);

			async_param_types [params_num] = TypeManager.asynccallback_type;
			async_param_types [params_num + 1] = TypeManager.object_type;

			mattr = MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.Virtual | MethodAttributes.NewSlot;
			
			BeginInvokeBuilder = TypeBuilder.DefineMethod ("BeginInvoke",
								       mattr,
								       cc,
								       TypeManager.iasyncresult_type,
								       async_param_types);

			i = 0;
			if (Parameters.FixedParameters != null){
				int top = Parameters.FixedParameters.Length;
				Parameter p;
				
				for (i = 0 ; i < top; i++) {
					p = Parameters.FixedParameters [i];

					BeginInvokeBuilder.DefineParameter (
						i+1, p.Attributes, p.Name);
				}
			}
			if (Parameters.ArrayParameter != null){
				Parameter p = Parameters.ArrayParameter;
				
				BeginInvokeBuilder.DefineParameter (
					i+1, p.Attributes, p.Name);
				i++;
			}

			BeginInvokeBuilder.DefineParameter (i + 1, ParameterAttributes.None, "callback");
			BeginInvokeBuilder.DefineParameter (i + 2, ParameterAttributes.None, "object");
			
			BeginInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			Parameter [] async_params = new Parameter [params_num + 2];
			int n = 0;
			if (Parameters.FixedParameters != null){
				Parameters.FixedParameters.CopyTo (async_params, 0);
				n = Parameters.FixedParameters.Length;
			}
			if (Parameters.ArrayParameter != null)
				async_params [n] = Parameters.ArrayParameter;
			
			async_params [params_num] = new Parameter (
				TypeManager.system_asynccallback_expr, "callback",
								   Parameter.Modifier.NONE, null);
			async_params [params_num + 1] = new Parameter (
				TypeManager.system_object_expr, "object",
								   Parameter.Modifier.NONE, null);

			Parameters async_parameters = new Parameters (async_params, null, Location);
			
			async_parameters.ComputeAndDefineParameterTypes (this);
			TypeManager.RegisterMethod (BeginInvokeBuilder,
						    new InternalParameters (container, async_parameters),
						    async_param_types);

			//
			// EndInvoke
			//
			Type [] end_param_types = new Type [1];
			end_param_types [0] = TypeManager.iasyncresult_type;
			
			EndInvokeBuilder = TypeBuilder.DefineMethod ("EndInvoke",
								     mattr,
								     cc,
								     ret_type,
								     end_param_types);
			EndInvokeBuilder.DefineParameter (1, ParameterAttributes.None, "result");
			
			EndInvokeBuilder.SetImplementationFlags (MethodImplAttributes.Runtime);

			Parameter [] end_params = new Parameter [1];
			end_params [0] = new Parameter (
				TypeManager.system_iasyncresult_expr, "result",
							Parameter.Modifier.NONE, null);

			TypeManager.RegisterMethod (
				EndInvokeBuilder, new InternalParameters (
					container,
					new Parameters (
						end_params, null, Location)),
						    end_param_types);

			return true;
		}

		/// <summary>
		///  Verifies whether the method in question is compatible with the delegate
		///  Returns the method itself if okay and null if not.
		/// </summary>
		public static MethodBase VerifyMethod (EmitContext ec, Type delegate_type, MethodBase mb,
						       Location loc)
		{
			ParameterData pd = Invocation.GetParameterData (mb);

			int pd_count = pd.Count;

			Expression ml = Expression.MemberLookup (
				ec, delegate_type, "Invoke", loc);

			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return null;
			}

			MethodBase invoke_mb = ((MethodGroupExpr) ml).Methods [0];

			ParameterData invoke_pd = Invocation.GetParameterData (invoke_mb);

			if (invoke_pd.Count != pd_count)
				return null;

			for (int i = pd_count; i > 0; ) {
				i--;

				if (invoke_pd.ParameterType (i) == pd.ParameterType (i))
					continue;
				else
					return null;
			}

			if (((MethodInfo) invoke_mb).ReturnType == ((MethodInfo) mb).ReturnType)
				return mb;
			else
				return null;
		}

		// <summary>
		//  Verifies whether the invocation arguments are compatible with the
		//  delegate's target method
		// </summary>
		public static bool VerifyApplicability (EmitContext ec,
							Type delegate_type,
							ArrayList args,
							Location loc)
		{
			int arg_count;

			if (args == null)
				arg_count = 0;
			else
				arg_count = args.Count;

			Expression ml = Expression.MemberLookup (
				ec, delegate_type, "Invoke", loc);

			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!" + delegate_type);
				return false;
			}
			
			MethodBase mb = ((MethodGroupExpr) ml).Methods [0];
			ParameterData pd = Invocation.GetParameterData (mb);

			int pd_count = pd.Count;

			bool not_params_method = (pd_count == 0) ||
				(pd.ParameterModifier (pd_count - 1) != Parameter.Modifier.PARAMS);

			if (not_params_method && pd_count != arg_count) {
				Report.Error (1593, loc,
					      "Delegate '" + delegate_type.ToString ()
					      + "' does not take '" + arg_count + "' arguments");
				return false;
			}

			return Invocation.VerifyArgumentsCompat (ec, args, arg_count, mb, !not_params_method,
								 delegate_type, loc);
		}
		
		/// <summary>
		///  Verifies whether the delegate in question is compatible with this one in
		///  order to determine if instantiation from the same is possible.
		/// </summary>
		public static bool VerifyDelegate (EmitContext ec, Type delegate_type, Type probe_type, Location loc)
		{
			Expression ml = Expression.MemberLookup (
				ec, delegate_type, "Invoke", loc);
			
			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return false;
			}
			
			MethodBase mb = ((MethodGroupExpr) ml).Methods [0];
			ParameterData pd = Invocation.GetParameterData (mb);

			Expression probe_ml = Expression.MemberLookup (
				ec, delegate_type, "Invoke", loc);
			
			if (!(probe_ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return false;
			}
			
			MethodBase probe_mb = ((MethodGroupExpr) probe_ml).Methods [0];
			ParameterData probe_pd = Invocation.GetParameterData (probe_mb);
			
			if (((MethodInfo) mb).ReturnType != ((MethodInfo) probe_mb).ReturnType)
				return false;

			if (pd.Count != probe_pd.Count)
				return false;

			for (int i = pd.Count; i > 0; ) {
				i--;

				if (pd.ParameterType (i) != probe_pd.ParameterType (i) ||
				    pd.ParameterModifier (i) != probe_pd.ParameterModifier (i))
					return false;
			}
			
			return true;
		}
		
		public static string FullDelegateDesc (Type del_type, MethodBase mb, ParameterData pd)
		{
			StringBuilder sb = new StringBuilder (TypeManager.MonoBASIC_Name (((MethodInfo) mb).ReturnType));
			
			sb.Append (" " + del_type.ToString ());
			sb.Append (" (");

			int length = pd.Count;
			
			for (int i = length; i > 0; ) {
				i--;
				
				sb.Append (TypeManager.MonoBASIC_Name (pd.ParameterType (length - i - 1)));
				if (i != 0)
					sb.Append (", ");
			}
			
			sb.Append (")");
			return sb.ToString ();
			
		}
		
		// Hack around System.Reflection as found everywhere else
		public override MemberList FindMembers (MemberTypes mt, BindingFlags bf,
							MemberFilter filter, object criteria)
		{
			ArrayList members = new ArrayList ();

			if ((mt & MemberTypes.Method) != 0) {
				if (ConstructorBuilder != null)
				if (filter (ConstructorBuilder, criteria))
					members.Add (ConstructorBuilder);

				if (InvokeBuilder != null)
				if (filter (InvokeBuilder, criteria))
					members.Add (InvokeBuilder);

				if (BeginInvokeBuilder != null)
				if (filter (BeginInvokeBuilder, criteria))
					members.Add (BeginInvokeBuilder);

				if (EndInvokeBuilder != null)
				if (filter (EndInvokeBuilder, criteria))
					members.Add (EndInvokeBuilder);
			}

			return new MemberList (members);
		}

		public override MemberCache MemberCache {
			get {
				return null;
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

		public NewDelegate (Type type, ArrayList Arguments, Location loc)
		{
			this.type = type;
			this.Arguments = Arguments;
			this.loc  = loc; 
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (Arguments == null) {
				Report.Error (-11, loc,
					      "Delegate creation expression takes only one argument");
				return null;
			}

			if (Arguments.Count != 1) {
				Report.Error (-11, loc,
					      "Delegate creation expression takes only one argument");
				return null;
			}

			Expression ml = Expression.MemberLookup (
				ec, type, ".ctor", loc);

			if (!(ml is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: Could not find delegate constructor!");
				return null;
			}

			constructor_method = ((MethodGroupExpr) ml).Methods [0];
			Argument a = (Argument) Arguments [0];
			
			if (!a.ResolveMethodGroup (ec, Location))
				return null;
			
			Expression e = a.Expr;

			Expression invoke_method = Expression.MemberLookup (
				ec, type, "Invoke", MemberTypes.Method,
				Expression.AllBindingFlags, loc);

			if (invoke_method == null) {
				Report.Error (-200, loc, "Internal error ! Could not find Invoke method!");
				return null;
			}

			if (e is MethodGroupExpr) {
				MethodGroupExpr mg = (MethodGroupExpr) e;

				foreach (MethodInfo mi in mg.Methods){
					delegate_method  = Delegate.VerifyMethod (ec, type, mi, loc);

					if (delegate_method != null)
						break;
				}
					
				if (delegate_method == null) {
					string method_desc;
					if (mg.Methods.Length > 1)
						method_desc = mg.Methods [0].Name;
					else
						method_desc = Invocation.FullMethodDesc (mg.Methods [0]);

					MethodBase dm = ((MethodGroupExpr) invoke_method).Methods [0];
					ParameterData param = Invocation.GetParameterData (dm);
					string delegate_desc = Delegate.FullDelegateDesc (type, dm, param);

					Report.Error (30408, loc, "Method '" + method_desc + "' does not " +
						      "match delegate '" + delegate_desc + "'");

					return null;
				}

				//
				// Check safe/unsafe of the delegate
				//
				if (!ec.InUnsafe){
					ParameterData param = Invocation.GetParameterData (delegate_method);
					int count = param.Count;
					
					for (int i = 0; i < count; i++){
						if (param.ParameterType (i).IsPointer){
							Expression.UnsafeError (loc);
							return null;
						}
					}
				}
						
				if (mg.InstanceExpression != null)
					delegate_instance_expr = mg.InstanceExpression.Resolve (ec);
				else {
					if (!ec.IsStatic)
						delegate_instance_expr = ec.This;
					else
						delegate_instance_expr = null;
				}

				if (delegate_instance_expr != null)
					if (delegate_instance_expr.Type.IsValueType)
						delegate_instance_expr = new BoxedCast (delegate_instance_expr);
				
				eclass = ExprClass.Value;
				return this;
			}

			Type e_type = e.Type;

			if (!TypeManager.IsDelegateType (e_type)) {
				Report.Error (-12, loc, "Cannot create a delegate from something " +
					      "not a delegate or a method.");
				return null;
			}

			// This is what MS' compiler reports. We could always choose
			// to be more verbose and actually give delegate-level specifics
			
			if (!Delegate.VerifyDelegate (ec, type, e_type, loc)) {
				Report.Error (29, loc, "Cannot implicitly convert type '" + e_type + "' " +
					      "to type '" + type + "'");
				return null;
			}
				
			delegate_instance_expr = e;
			delegate_method = ((MethodGroupExpr) invoke_method).Methods [0];
			
			eclass = ExprClass.Value;
			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			if (delegate_instance_expr == null ||
			    delegate_method.IsStatic)
				ec.ig.Emit (OpCodes.Ldnull);
			else
				delegate_instance_expr.Emit (ec);
			
			if (delegate_method.IsVirtual) {
				ec.ig.Emit (OpCodes.Dup);
				ec.ig.Emit (OpCodes.Ldvirtftn, (MethodInfo) delegate_method);
			} else
				ec.ig.Emit (OpCodes.Ldftn, (MethodInfo) delegate_method);
			ec.ig.Emit (OpCodes.Newobj, (ConstructorInfo) constructor_method);
		}
	}

	public class DelegateInvocation : ExpressionStatement {

		public Expression InstanceExpr;
		public ArrayList  Arguments;

		MethodBase method;
		
		public DelegateInvocation (Expression instance_expr, ArrayList args, Location loc)
		{
			this.InstanceExpr = instance_expr;
			this.Arguments = args;
			this.loc = loc;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			if (InstanceExpr is EventExpr) {
				
				EventInfo ei = ((EventExpr) InstanceExpr).EventInfo;
				
				Expression ml = MemberLookup (
					ec, ec.ContainerType, ei.Name,
					MemberTypes.Event, AllBindingFlags | BindingFlags.DeclaredOnly, loc);

				if (ml == null) {
				        //
					// If this is the case, then the Event does not belong 
					// to this Type and so, according to the spec
					// cannot be accessed directly
					//
					// Note that target will not appear as an EventExpr
					// in the case it is being referenced within the same type container;
					// it will appear as a FieldExpr in that case.
					//
					
					Assign.error70 (ei, loc);
					return null;
				}
			}
			
			
			Type del_type = InstanceExpr.Type;
			if (del_type == null)
				return null;
			
			if (Arguments != null){
				foreach (Argument a in Arguments){
					if (!a.Resolve (ec, loc))
						return null;
				}
			}
			
			if (!Delegate.VerifyApplicability (ec, del_type, Arguments, loc))
				return null;

			Expression lookup = Expression.MemberLookup (ec, del_type, "Invoke", loc);
			if (!(lookup is MethodGroupExpr)) {
				Report.Error (-100, loc, "Internal error: could not find Invoke method!");
				return null;
			}
			
			method = ((MethodGroupExpr) lookup).Methods [0];
			type = ((MethodInfo) method).ReturnType;
			eclass = ExprClass.Value;
			
			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Delegate del = TypeManager.LookupDelegate (InstanceExpr.Type);

			//
			// Invocation on delegates call the virtual Invoke member
			// so we are always `instance' calls
			//
			Invocation.EmitCall (ec, false, false, InstanceExpr, method, Arguments, loc);
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
