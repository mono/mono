//
// ecore.cs: Core of the Expression representation for the intermediate tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//
//

namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	/// <remarks>
	///   The ExprClass class contains the is used to pass the 
	///   classification of an expression (value, variable, namespace,
	///   type, method group, property access, event access, indexer access,
	///   nothing).
	/// </remarks>
	public enum ExprClass : byte {
		Invalid,
		
		Value,
		Variable,
		Namespace,
		Type,
		MethodGroup,
		PropertyAccess,
		EventAccess,
		IndexerAccess,
		Nothing, 
	}

	/// <summary>
	///   This interface is implemented by variables
	/// </summary>
	public interface IMemoryLocation {
		/// <summary>
		///   The AddressOf method should generate code that loads
		///   the address of the object and leaves it on the stack
		/// </summary>
		void AddressOf (EmitContext ec);
	}

	/// <remarks>
	///   Base class for expressions
	/// </remarks>
	public abstract class Expression {
		public ExprClass eclass;
		protected Type      type;
		
		public Type Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		/// <summary>
		///   Utility wrapper routine for Error, just to beautify the code
		/// </summary>
		static protected void Error (int error, string s)
		{
			Report.Error (error, s);
		}

		static protected void Error (int error, Location loc, string s)
		{
			Report.Error (error, loc, s);
		}
		
		/// <summary>
		///   Utility wrapper routine for Warning, just to beautify the code
		/// </summary>
		static protected void Warning (int warning, string s)
		{
			Report.Warning (warning, s);
		}

		static public void error30 (Location loc, Type source, Type target)
		{
			Report.Error (30, loc, "Cannot convert type '" +
				      TypeManager.CSharpName (source) + "' to '" +
				      TypeManager.CSharpName (target) + "'");
		}

		/// <summary>
		///   Performs semantic analysis on the Expression
		/// </summary>
		///
		/// <remarks>
		///   The Resolve method is invoked to perform the semantic analysis
		///   on the node.
		///
		///   The return value is an expression (it can be the
		///   same expression in some cases) or a new
		///   expression that better represents this node.
		///   
		///   For example, optimizations of Unary (LiteralInt)
		///   would return a new LiteralInt with a negated
		///   value.
		///   
		///   If there is an error during semantic analysis,
		///   then an error should be reported (using Report)
		///   and a null value should be returned.
		///   
		///   There are two side effects expected from calling
		///   Resolve(): the the field variable "eclass" should
		///   be set to any value of the enumeration
		///   `ExprClass' and the type variable should be set
		///   to a valid type (this is the type of the
		///   expression).
		/// </remarks>
		public abstract Expression DoResolve (EmitContext ec);

		public virtual Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}
		
		/// <summary>
		///   Resolves an expression and performs semantic analysis on it.
		/// </summary>
		///
		/// <remarks>
		///   Currently Resolve wraps DoResolve to perform sanity
		///   checking and assertion checking on what we expect from Resolve.
		/// </remarks>
		public Expression Resolve (EmitContext ec)
		{
			Expression e = DoResolve (ec);

			if (e != null){
				if (e is SimpleName){
					SimpleName s = (SimpleName) e;
					
					Report.Error (
						103, s.Location,
						"The name `" + s.Name + "' could not be found in `" +
						ec.TypeContainer.Name + "'");
					return null;
				}
				
				if (e.eclass == ExprClass.Invalid)
					throw new Exception ("Expression " + e.GetType () +
							     " ExprClass is Invalid after resolve");

				if (e.eclass != ExprClass.MethodGroup)
					if (e.type == null)
						throw new Exception (
							"Expression " + e.GetType () +
							" did not set its type after Resolve\n" +
							"called from: " + this.GetType ());
			}

			return e;
		}

		/// <summary>
		///   Performs expression resolution and semantic analysis, but
		///   allows SimpleNames to be returned.
		/// </summary>
		///
		/// <remarks>
		///   This is used by MemberAccess to construct long names that can not be
		///   partially resolved (namespace-qualified names for example).
		/// </remarks>
		public Expression ResolveWithSimpleName (EmitContext ec)
		{
			Expression e;

			if (this is SimpleName)
				e = ((SimpleName) this).DoResolveAllowStatic (ec);
			else 
				e = DoResolve (ec);

			if (e != null){
				if (e is SimpleName)
					return e;

				if (e.eclass == ExprClass.Invalid)
					throw new Exception ("Expression " + e +
							     " ExprClass is Invalid after resolve");

				if (e.eclass != ExprClass.MethodGroup)
					if (e.type == null)
						throw new Exception ("Expression " + e +
								     " did not set its type after Resolve");
			}

			return e;
		}
		
		/// <summary>
		///   Resolves an expression for LValue assignment
		/// </summary>
		///
		/// <remarks>
		///   Currently ResolveLValue wraps DoResolveLValue to perform sanity
		///   checking and assertion checking on what we expect from Resolve
		/// </remarks>
		public Expression ResolveLValue (EmitContext ec, Expression right_side)
		{
			Expression e = DoResolveLValue (ec, right_side);

			if (e != null){
				if (e is SimpleName){
					SimpleName s = (SimpleName) e;
					
					Report.Error (
						103, s.Location,
						"The name `" + s.Name + "' could not be found in `" +
						ec.TypeContainer.Name + "'");
					return null;
				}

				if (e.eclass == ExprClass.Invalid)
					throw new Exception ("Expression " + e +
							     " ExprClass is Invalid after resolve");

				if (e.eclass != ExprClass.MethodGroup)
					if (e.type == null)
						throw new Exception ("Expression " + e +
								     " did not set its type after Resolve");
			}

			return e;
		}
		
		/// <summary>
		///   Emits the code for the expression
		/// </summary>
		///
		/// <remarks>
		///   The Emit method is invoked to generate the code
		///   for the expression.  
		/// </remarks>
		public abstract void Emit (EmitContext ec);

		/// <summary>
		///   Protected constructor.  Only derivate types should
		///   be able to be created
		/// </summary>

		protected Expression ()
		{
			eclass = ExprClass.Invalid;
			type = null;
		}

		/// <summary>
		///   Returns a literalized version of a literal FieldInfo
		/// </summary>
		///
		/// <remarks>
		///   The possible return values are:
		///      IntConstant, UIntConstant
		///      LongLiteral, ULongConstant
		///      FloatConstant, DoubleConstant
		///      StringConstant
		///
		///   The value returned is already resolved.
		/// </remarks>
		public static Constant Constantify (object v, Type t)
		{
			if (t == TypeManager.int32_type)
				return new IntConstant ((int) v);
			else if (t == TypeManager.uint32_type)
				return new UIntConstant ((uint) v);
			else if (t == TypeManager.int64_type)
				return new LongConstant ((long) v);
			else if (t == TypeManager.uint64_type)
				return new ULongConstant ((ulong) v);
			else if (t == TypeManager.float_type)
				return new FloatConstant ((float) v);
			else if (t == TypeManager.double_type)
				return new DoubleConstant ((double) v);
			else if (t == TypeManager.string_type)
				return new StringConstant ((string) v);
			else if (t == TypeManager.short_type)
				return new ShortConstant ((short)v);
			else if (t == TypeManager.ushort_type)
				return new UShortConstant ((ushort)v);
			else if (t == TypeManager.sbyte_type)
				return new SByteConstant (((sbyte)v));
			else if (t == TypeManager.byte_type)
				return new ByteConstant ((byte)v);
			else if (t == TypeManager.char_type)
				return new CharConstant ((char)v);
			else if (TypeManager.IsEnumType (t)){
				Expression e = Constantify (v, v.GetType ());

				return new EnumConstant ((Constant) e, t);
			} else
				throw new Exception ("Unknown type for constant (" + t +
						     "), details: " + v);
		}

		/// <summary>
		///   Returns a fully formed expression after a MemberLookup
		/// </summary>
		public static Expression ExprClassFromMemberInfo (EmitContext ec, MemberInfo mi, Location loc)
		{
			if (mi is EventInfo)
				return new EventExpr ((EventInfo) mi, loc);
			else if (mi is FieldInfo)
				return new FieldExpr ((FieldInfo) mi, loc);
			else if (mi is PropertyInfo)
				return new PropertyExpr ((PropertyInfo) mi, loc);
		        else if (mi is Type){
				return new TypeExpr ((System.Type) mi);
			}

			return null;
		}

		//
		// Returns whether the array of memberinfos contains the given method
		//
		static bool ArrayContainsMethod (MemberInfo [] array, MethodBase new_method)
		{
			Type [] new_args = TypeManager.GetArgumentTypes (new_method);

			foreach (MethodBase method in array){
				if (method.Name != new_method.Name)
					continue;
				
				Type [] old_args = TypeManager.GetArgumentTypes (method);
				int old_count = old_args.Length;
				int i;

				if (new_args.Length != old_count)
					continue;
				
				for (i = 0; i < old_count; i++){
					if (old_args [i] != new_args [i])
						break;
				}
				if (i != old_count)
					continue;

				if (!(method is MethodInfo && new_method is MethodInfo))
					return true;
				
				if (((MethodInfo) method).ReturnType == ((MethodInfo) new_method).ReturnType)
					return true;
			}
			return false;
		}
		
		//
		// We copy methods from `new_members' into `target_list' if the signature
		// for the method from in the new list does not exist in the target_list
		//
		// The name is assumed to be the same.
		//
		public static ArrayList CopyNewMethods (ArrayList target_list, MemberInfo [] new_members)
		{
			if (target_list == null){
				target_list = new ArrayList ();

				target_list.AddRange (new_members);
				return target_list;
			}
			
			MemberInfo [] target_array = new MemberInfo [target_list.Count];
			target_list.CopyTo (target_array, 0);
			
			foreach (MemberInfo mi in new_members){
				MethodBase new_method = (MethodBase) mi;

				if (!ArrayContainsMethod (target_array, new_method))
					target_list.Add (new_method);
			}
			return target_list;
		}
		
		//
		// FIXME: Probably implement a cache for (t,name,current_access_set)?
		//
		// This code could use some optimizations, but we need to do some
		// measurements.  For example, we could use a delegate to `flag' when
		// something can not any longer be a method-group (because it is something
		// else).
		//
		// Return values:
		//     If the return value is an Array, then it is an array of
		//     MethodBases
		//   
		//     If the return value is an MemberInfo, it is anything, but a Method
		//
		//     null on error.
		//
		// FIXME: When calling MemberLookup inside an `Invocation', we should pass
		// the arguments here and have MemberLookup return only the methods that
		// match the argument count/type, unlike we are doing now (we delay this
		// decision).
		//
		// This is so we can catch correctly attempts to invoke instance methods
		// from a static body (scan for error 120 in ResolveSimpleName).
		//
		//
		// FIXME: Potential optimization, have a static ArrayList
		//

		public static Expression MemberLookup (EmitContext ec, Type t, string name,
						       MemberTypes mt, BindingFlags bf, Location loc)
		{
			Type source_type = ec.ContainerType;

			if (source_type != null){
				if (source_type == t || source_type.IsSubclassOf (t))
					bf |= BindingFlags.NonPublic;
			}

			//
			// Lookup for members starting in the type requested and going
			// up the hierarchy until a match is found.
			//
			// As soon as a non-method match is found, we return.
			//
			// If methods are found though, then the search proceeds scanning
			// for more public methods in the hierarchy with signatures that
			// do not match any of the signatures found so far.
			//
			ArrayList method_list = null;
			Type current_type = t;
			bool searching = true;
			do {
				MemberInfo [] mi;

				mi = RootContext.TypeManager.FindMembers (
					current_type, mt, bf | BindingFlags.DeclaredOnly,
					System.Type.FilterName, name);
				
				if (current_type == TypeManager.object_type)
					searching = false;
				else {
					current_type = current_type.BaseType;

					//
					// This happens with interfaces, they have a null
					// basetype
					//
					if (current_type == null)
						searching = false;
				}

				if (mi == null)
					continue;
				
				int count = mi.Length;

				if (count == 0)
					continue;
				
				if (count == 1 && !(mi [0] is MethodBase))
					return Expression.ExprClassFromMemberInfo (ec, mi [0], loc);

				//
				// We found methods, turn the search into "method scan"
				// mode.
				//
				method_list = CopyNewMethods (method_list, mi);
				mt &= (MemberTypes.Method | MemberTypes.Constructor);
			} while (searching);

			if (method_list != null && method_list.Count > 0)
				return new MethodGroupExpr (method_list);

			return null;
		}

		public const MemberTypes AllMemberTypes =
			MemberTypes.Constructor |
			MemberTypes.Event       |
			MemberTypes.Field       |
			MemberTypes.Method      |
			MemberTypes.NestedType  |
			MemberTypes.Property;
		
		public const BindingFlags AllBindingFlags =
			BindingFlags.Public |
			BindingFlags.Static |
			BindingFlags.Instance;

		public static Expression MemberLookup (EmitContext ec, Type t, string name, Location loc)
		{
			return MemberLookup (ec, t, name, AllMemberTypes, AllBindingFlags, loc);
		}

		/// <summary>
		///   This is a wrapper for MemberLookup that is not used to "probe", but
		///   to find a final definition.  If the final definition is not found, we
		///   look for private members and display a useful debugging message if we
		///   find it.
		/// </summary>
		public static Expression MemberLookupFinal (EmitContext ec, Type t, string name, 
							    Location loc)
		{
			Expression e;

			e = MemberLookup (ec, t, name, AllMemberTypes, AllBindingFlags, loc);

			if (e != null)
				return e;
			
			e = MemberLookup (ec, t, name, AllMemberTypes,
					  AllBindingFlags | BindingFlags.NonPublic, loc);
			if (e == null){
				Report.Error (
					117, loc, "`" + t + "' does not contain a definition " +
					"for `" + name + "'");
			} else {
				Report.Error (
					122, loc, "`" + t + "." + name +
					"' is inaccessible due to its protection level");
			}
			
			return null;
		}
		
		static public Expression ImplicitReferenceConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (target_type == TypeManager.object_type) {
				//
				// A pointer type cannot be converted to object
				// 
				if (expr_type.IsPointer)
					return null;
				
				if (expr_type.IsClass)
					return new EmptyCast (expr, target_type);
				if (expr_type.IsValueType)
					return new BoxedCast (expr);
			} else if (expr_type.IsSubclassOf (target_type)) {
				return new EmptyCast (expr, target_type);
			} else {
				// from the null type to any reference-type.
				if (expr is NullLiteral && !target_type.IsValueType)
					return new EmptyCast (expr, target_type);

				// from any class-type S to any interface-type T.
				if (expr_type.IsClass && target_type.IsInterface) {
					if (TypeManager.ImplementsInterface (expr_type, target_type))
						return new EmptyCast (expr, target_type);
					else
						return null;
				}

				// from any interface type S to interface-type T.
				if (expr_type.IsInterface && target_type.IsInterface) {

					if (TypeManager.ImplementsInterface (expr_type, target_type))
						return new EmptyCast (expr, target_type);
					else
						return null;
				}
				
				// from an array-type S to an array-type of type T
				if (expr_type.IsArray && target_type.IsArray) {
					if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {

						Type expr_element_type = expr_type.GetElementType ();
						Type target_element_type = target_type.GetElementType ();

						if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
							if (StandardConversionExists (expr_element_type,
										      target_element_type))
								return new EmptyCast (expr, target_type);
					}
				}
				
				
				// from an array-type to System.Array
				if (expr_type.IsArray && target_type == TypeManager.array_type)
					return new EmptyCast (expr, target_type);
				
				// from any delegate type to System.Delegate
				if (expr_type.IsSubclassOf (TypeManager.delegate_type) &&
				    target_type == TypeManager.delegate_type)
					return new EmptyCast (expr, target_type);
					
				// from any array-type or delegate type into System.ICloneable.
				if (expr_type.IsArray || expr_type.IsSubclassOf (TypeManager.delegate_type))
					if (target_type == TypeManager.icloneable_type)
						return new EmptyCast (expr, target_type);
				
				return null;

			}
			
			return null;
		}

		/// <summary>
		///   Handles expressions like this: decimal d; d = 1;
		///   and changes them into: decimal d; d = new System.Decimal (1);
		/// </summary>
		static Expression InternalTypeConstructor (EmitContext ec, Expression expr, Type target)
		{
			ArrayList args = new ArrayList ();

			args.Add (new Argument (expr, Argument.AType.Expression));

			Expression ne = new New (target.FullName, args,
						 new Location (-1));

			return ne.Resolve (ec);
		}

		/// <summary>
		///   Implicit Numeric Conversions.
		///
		///   expr is the expression to convert, returns a new expression of type
		///   target_type or null if an implicit conversion is not possible.
		/// </summary>
		static public Expression ImplicitNumericConversion (EmitContext ec, Expression expr,
								    Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			
			//
			// Attempt to do the implicit constant expression conversions

			if (expr is IntConstant){
				Expression e;
				
				e = TryImplicitIntConversion (target_type, (IntConstant) expr);

				if (e != null)
					return e;
			} else if (expr is LongConstant && target_type == TypeManager.uint64_type){
				//
				// Try the implicit constant expression conversion
				// from long to ulong, instead of a nice routine,
				// we just inline it
				//
				long v = ((LongConstant) expr).Value;
				if (v > 0)
					return new ULongConstant ((ulong) v);
			}
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);

				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if (target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if (target_type == TypeManager.uint32_type)
					return new EmptyCast (expr, target_type);

				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)){
				//
				// From long/ulong to float, double
				//
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);	
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			}

			return null;
		}

		/// <summary>
		///  Determines if a standard implicit conversion exists from
		///  expr_type to target_type
		/// </summary>
		public static bool StandardConversionExists (Type expr_type, Type target_type)
		{
			if (expr_type == target_type)
				return true;

			// First numeric conversions 
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
				//
				if ((target_type == TypeManager.int32_type) || 
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type)  ||
				    (target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
	
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
				// 
				if ((target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
				//
				if ((target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)) {
				//
				// From long/ulong to float, double
				//
				if ((target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;

			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return true;
			}	
			
			// Next reference conversions

			if (target_type == TypeManager.object_type) {
				if ((expr_type.IsClass) ||
				    (expr_type.IsValueType))
					return true;
				
			} else if (expr_type.IsSubclassOf (target_type)) {
				return true;
				
			} else {
				// from any class-type S to any interface-type T.
				if (expr_type.IsClass && target_type.IsInterface)
					return true;
				
				// from any interface type S to interface-type T.
				// FIXME : Is it right to use IsAssignableFrom ?
				if (expr_type.IsInterface && target_type.IsInterface)
					if (target_type.IsAssignableFrom (expr_type))
						return true;
				
				// from an array-type S to an array-type of type T
				if (expr_type.IsArray && target_type.IsArray) {
					if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {
						
						Type expr_element_type = expr_type.GetElementType ();
						Type target_element_type = target_type.GetElementType ();
						
						if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
							if (StandardConversionExists (expr_element_type,
										      target_element_type))
								return true;
					}
				}
				
				// from an array-type to System.Array
				if (expr_type.IsArray && target_type.IsAssignableFrom (expr_type))
					return true;
				
				// from any delegate type to System.Delegate
				if (expr_type.IsSubclassOf (TypeManager.delegate_type) &&
				    target_type == TypeManager.delegate_type)
					if (target_type.IsAssignableFrom (expr_type))
						return true;
					
				// from any array-type or delegate type into System.ICloneable.
				if (expr_type.IsArray || expr_type.IsSubclassOf (TypeManager.delegate_type))
					if (target_type == TypeManager.icloneable_type)
						return true;
				
				// from the null type to any reference-type.
				// FIXME : How do we do this ?

			}

			return false;
		}

		static EmptyExpression MyEmptyExpr;
		/// <summary>
		///   Tells whether an implicit conversion exists from expr_type to
		///   target_type
		/// </summary>
		public bool ImplicitConversionExists (EmitContext ec, Type expr_type, Type target_type,
						      Location l)
		{
			if (MyEmptyExpr == null)
				MyEmptyExpr = new EmptyExpression (expr_type);
			else
				MyEmptyExpr.SetType (expr_type);

			return ConvertImplicit (ec, MyEmptyExpr, target_type, l) != null;
		}
		
		/// <summary>
		///  Finds "most encompassed type" according to the spec (13.4.2)
		///  amongst the methods in the MethodGroupExpr which convert from a
		///  type encompassing source_type
		/// </summary>
		static Type FindMostEncompassedType (MethodGroupExpr me, Type source_type)
		{
			Type best = null;
			
			for (int i = me.Methods.Length; i > 0; ) {
				i--;

				MethodBase mb = me.Methods [i];
				ParameterData pd = Invocation.GetParameterData (mb);
				Type param_type = pd.ParameterType (0);

				if (StandardConversionExists (source_type, param_type)) {
					if (best == null)
						best = param_type;
					
					if (StandardConversionExists (param_type, best))
						best = param_type;
				}
			}

			return best;
		}
		
		/// <summary>
		///  Finds "most encompassing type" according to the spec (13.4.2)
		///  amongst the methods in the MethodGroupExpr which convert to a
		///  type encompassed by target_type
		/// </summary>
		static Type FindMostEncompassingType (MethodGroupExpr me, Type target)
		{
			Type best = null;
			
			for (int i = me.Methods.Length; i > 0; ) {
				i--;
				
				MethodInfo mi = (MethodInfo) me.Methods [i];
				Type ret_type = mi.ReturnType;
				
				if (StandardConversionExists (ret_type, target)) {
					if (best == null)
						best = ret_type;

					if (!StandardConversionExists (ret_type, best))
						best = ret_type;
				}
				
			}
			
			return best;

		}
		

		/// <summary>
		///  User-defined Implicit conversions
		/// </summary>
		static public Expression ImplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, false);
		}

		/// <summary>
		///  User-defined Explicit conversions
		/// </summary>
		static public Expression ExplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, true);
		}
		
		/// <summary>
		///   User-defined conversions
		/// </summary>
		static public Expression UserDefinedConversion (EmitContext ec, Expression source,
								Type target, Location loc,
								bool look_for_explicit)
		{
			Expression mg1 = null, mg2 = null, mg3 = null, mg4 = null;
			Expression mg5 = null, mg6 = null, mg7 = null, mg8 = null;
			Expression e;
			MethodBase method = null;
			Type source_type = source.Type;

			string op_name;
			
			// If we have a boolean type, we need to check for the True operator

			// FIXME : How does the False operator come into the picture ?
			// FIXME : This doesn't look complete and very correct !
			if (target == TypeManager.bool_type)
				op_name = "op_True";
			else
				op_name = "op_Implicit";
			
			mg1 = MemberLookup (ec, source_type, op_name, loc);

			if (source_type.BaseType != null)
				mg2 = MemberLookup (ec, source_type.BaseType, op_name, loc);
			
			mg3 = MemberLookup (ec, target, op_name, loc);

			if (target.BaseType != null)
				mg4 = MemberLookup (ec, target.BaseType, op_name, loc);

			MethodGroupExpr union1 = Invocation.MakeUnionSet (mg1, mg2);
			MethodGroupExpr union2 = Invocation.MakeUnionSet (mg3, mg4);

			MethodGroupExpr union3 = Invocation.MakeUnionSet (union1, union2);

			MethodGroupExpr union4 = null;

			if (look_for_explicit) {

				op_name = "op_Explicit";
				
				mg5 = MemberLookup (ec, source_type, op_name, loc);

				if (source_type.BaseType != null)
					mg6 = MemberLookup (ec, source_type.BaseType, op_name, loc);
				
				mg7 = MemberLookup (ec, target, op_name, loc);
				
				if (target.BaseType != null)
					mg8 = MemberLookup (ec, target.BaseType, op_name, loc);
				
				MethodGroupExpr union5 = Invocation.MakeUnionSet (mg5, mg6);
				MethodGroupExpr union6 = Invocation.MakeUnionSet (mg7, mg8);

				union4 = Invocation.MakeUnionSet (union5, union6);
			}
			
			MethodGroupExpr union = Invocation.MakeUnionSet (union3, union4);

			if (union != null) {

				Type most_specific_source, most_specific_target;

				most_specific_source = FindMostEncompassedType (union, source_type);
				if (most_specific_source == null)
					return null;

				most_specific_target = FindMostEncompassingType (union, target);
				if (most_specific_target == null) 
					return null;
				
				int count = 0;
				
				for (int i = union.Methods.Length; i > 0;) {
					i--;

					MethodBase mb = union.Methods [i];
					ParameterData pd = Invocation.GetParameterData (mb);
					MethodInfo mi = (MethodInfo) union.Methods [i];

					if (pd.ParameterType (0) == most_specific_source &&
					    mi.ReturnType == most_specific_target) {
						method = mb;
						count++;
					}
				}

				if (method == null || count > 1) {
					Report.Error (-11, loc, "Ambiguous user defined conversion");
					return null;
				}
				
				//
				// This will do the conversion to the best match that we
				// found.  Now we need to perform an implict standard conversion
				// if the best match was not the type that we were requested
				// by target.
				//
				if (look_for_explicit)
					source = ConvertExplicitStandard (ec, source, most_specific_source, loc);
				else
					source = ConvertImplicitStandard (ec, source,
									  most_specific_source, loc);

				if (source == null)
					return null;

				e =  new UserCast ((MethodInfo) method, source);
				
				if (e.Type != target){
					if (!look_for_explicit)
						e = ConvertImplicitStandard (ec, e, target, loc);
					else
						e = ConvertExplicitStandard (ec, e, target, loc);

					return e;
				} else
					return e;
			}
			
			return null;
		}
		
		/// <summary>
		///   Converts implicitly the resolved expression `expr' into the
		///   `target_type'.  It returns a new expression that can be used
		///   in a context that expects a `target_type'. 
		/// </summary>
		static public Expression ConvertImplicit (EmitContext ec, Expression expr,
							  Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr_type == target_type)
				return expr;

			if (target_type == null)
				throw new Exception ("Target type is null");

			e = ConvertImplicitStandard (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitUserConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			return null;
		}

		
		/// <summary>
		///   Attempts to apply the `Standard Implicit
		///   Conversion' rules to the expression `expr' into
		///   the `target_type'.  It returns a new expression
		///   that can be used in a context that expects a
		///   `target_type'.
		///
		///   This is different from `ConvertImplicit' in that the
		///   user defined implicit conversions are excluded. 
		/// </summary>
		static public Expression ConvertImplicitStandard (EmitContext ec, Expression expr,
								  Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr_type == target_type)
				return expr;

			e = ImplicitNumericConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitReferenceConversion (expr, target_type);
			if (e != null)
				return e;

			if (target_type.IsSubclassOf (TypeManager.enum_type) && expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;

				if (i.Value == 0)
					return new EmptyCast (expr, target_type);
			}

			if (ec.InUnsafe) {
				if (expr_type.IsPointer){
					if (target_type == TypeManager.void_ptr_type)
						return new EmptyCast (expr, target_type);
				}
				
				if (target_type.IsPointer){
					if (expr is NullLiteral)
						return new EmptyCast (expr, target_type);
				}
			}

			return null;
		}

		/// <summary>
		///   Attemps to perform an implict constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		static protected Expression TryImplicitIntConversion (Type target_type, IntConstant ic)
		{
			int value = ic.Value;

			//
			// FIXME: This should really return constants instead of EmptyCasts
			//
			if (target_type == TypeManager.sbyte_type){
				if (value >= SByte.MinValue && value <= SByte.MaxValue)
					return new SByteConstant ((sbyte) value);
			} else if (target_type == TypeManager.byte_type){
				if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
					return new ByteConstant ((byte) value);
			} else if (target_type == TypeManager.short_type){
				if (value >= Int16.MinValue && value <= Int16.MaxValue)
					return new ShortConstant ((short) value);
			} else if (target_type == TypeManager.ushort_type){
				if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
					return new UShortConstant ((ushort) value);
			} else if (target_type == TypeManager.uint32_type){
				if (value >= 0)
					return new UIntConstant ((uint) value);
			} else if (target_type == TypeManager.uint64_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (value >= 0)
					return new ULongConstant ((ulong) value);
			}
			
			if (value == 0 && ic is IntLiteral && TypeManager.IsEnumType (target_type))
				return new EnumConstant (ic, target_type);

			return null;
		}

		/// <summary>
		///   Attemptes to implicityly convert `target' into `type', using
		///   ConvertImplicit.  If there is no implicit conversion, then
		///   an error is signaled
		/// </summary>
		static public Expression ConvertImplicitRequired (EmitContext ec, Expression source,
								  Type target_type, Location loc)
		{
			Expression e;
			
			e = ConvertImplicit (ec, source, target_type, loc);
			if (e != null)
				return e;

			if (source is DoubleLiteral && target_type == TypeManager.float_type){
				Error (664, loc,
				       "Double literal cannot be implicitly converted to " +
				       "float type, use F suffix to create a float literal");
			}
			
			string msg = "Cannot convert implicitly from `"+
				TypeManager.CSharpName (source.Type) + "' to `" +
				TypeManager.CSharpName (target_type) + "'";

			Error (29, loc, msg);

			return null;
		}

		/// <summary>
		///   Performs the explicit numeric conversions
		/// </summary>
		static Expression ConvertNumericExplicit (EmitContext ec, Expression expr,
							  Type target_type)
		{
			Type expr_type = expr.Type;
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U1);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U2);
				if (target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U4);
				if (target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U8);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_CH);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to sbyte and char
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U1_I1);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U1_CH);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to sbyte, byte, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U1);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U2);
				if (target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U4);
				if (target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U8);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_CH);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to sbyte, byte, short, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_I2);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_CH);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to sbyte, byte, short, ushort, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_I2);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U2);
				if (target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U4);
				if (target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U8);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_CH);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to sbyte, byte, short, ushort, int, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I2);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_U2);
				if (target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I4);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_CH);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long to sbyte, byte, short, ushort, int, uint, ulong, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_I2);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U2);
				if (target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_I4);
				if (target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U4);
				if (target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U8);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_CH);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to sbyte, byte, short, ushort, int, uint, long, char
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I2);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_U2);
				if (target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I4);
				if (target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_U4);
				if (target_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I8);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_CH);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to sbyte, byte, short
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.CH_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.CH_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.CH_I2);
			} else if (expr_type == TypeManager.float_type){
				//
				// From float to sbyte, byte, short,
				// ushort, int, uint, long, ulong, char
				// or decimal
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I2);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U2);
				if (target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I4);
				if (target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U4);
				if (target_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I8);
				if (target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U8);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_CH);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} else if (expr_type == TypeManager.double_type){
				//
				// From double to byte, byte, short,
				// ushort, int, uint, long, ulong,
				// char, float or decimal
				//
				if (target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I1);
				if (target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U1);
				if (target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I2);
				if (target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U2);
				if (target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I4);
				if (target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U4);
				if (target_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I8);
				if (target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U8);
				if (target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_CH);
				if (target_type == TypeManager.float_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_R4);
				if (target_type == TypeManager.decimal_type)
					return InternalTypeConstructor (ec, expr, target_type);
			} 

			// decimal is taken care of by the op_Explicit methods.

			return null;
		}

		/// <summary>
		///  Returns whether an explicit reference conversion can be performed
		///  from source_type to target_type
		/// </summary>
		static bool ExplicitReferenceConversionExists (Type source_type, Type target_type)
		{
			bool target_is_value_type = target_type.IsValueType;
			
			if (source_type == target_type)
				return true;
			
			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return true;
					
			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (target_type.IsSubclassOf (source_type))
				return true;

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (!target_type.IsSubclassOf (source_type))
					return true;
			}
			    
			//
			// From any class type S to any interface T, provides S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed &&
			    !target_type.IsAssignableFrom (source_type))
				return true;

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface &&
			    (!target_type.IsSealed || source_type.IsAssignableFrom (target_type)))
				return true;

			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = source_type.GetElementType ();
					Type target_element_type = target_type.GetElementType ();
					
					if (!source_element_type.IsValueType && !target_element_type.IsValueType)
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return true;
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsSubclassOf (TypeManager.array_type)){
				return true;
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    target_type.IsSubclassOf (TypeManager.delegate_type))
				return true;

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return true;
			
			return false;
		}

		/// <summary>
		///   Implements Explicit Reference conversions
		/// </summary>
		static Expression ConvertReferenceExplicit (Expression source, Type target_type)
		{
			Type source_type = source.Type;
			bool target_is_value_type = target_type.IsValueType;
			
			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return new ClassCast (source, target_type);


			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (target_type.IsSubclassOf (source_type))
				return new ClassCast (source, target_type);

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){

				Type [] ifaces = source_type.GetInterfaces ();

				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
			}
			    
			//
			// From any class type S to any interface T, provides S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed) {
				
				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
				
			}

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface) {

				if (target_type.IsSealed)
					return null;
				
				if (TypeManager.ImplementsInterface (target_type, source_type))
					return new ClassCast (source, target_type);
				else
					return null;
			}
			
			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = source_type.GetElementType ();
					Type target_element_type = target_type.GetElementType ();
					
					if (!source_element_type.IsValueType && !target_element_type.IsValueType)
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return new ClassCast (source, target_type);
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsSubclassOf (TypeManager.array_type)){
				return new ClassCast (source, target_type);
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    target_type.IsSubclassOf (TypeManager.delegate_type))
				return new ClassCast (source, target_type);

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return new ClassCast (source, target_type);
			
			return null;
		}
		
		/// <summary>
		///   Performs an explicit conversion of the expression `expr' whose
		///   type is expr.Type to `target_type'.
		/// </summary>
		static public Expression ConvertExplicit (EmitContext ec, Expression expr,
							  Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression ne = ConvertImplicitStandard (ec, expr, target_type, loc);

			if (ne != null)
				return ne;

			ne = ConvertNumericExplicit (ec, expr, target_type);
			if (ne != null)
				return ne;

			//
			// Unboxing conversion.
			//
			if (expr_type == TypeManager.object_type && target_type.IsValueType)
				return new UnboxCast (expr, target_type);

			//
			// Enum types
			//
			if (expr_type.IsSubclassOf (TypeManager.enum_type)) {
				Expression e;

				//
				// FIXME: Is there any reason we should have EnumConstant
				// dealt with here instead of just using always the
				// UnderlyingSystemType to wrap the type?
				//
				if (expr is EnumConstant)
					e = ((EnumConstant) expr).Child;
				else {
					e = new EmptyCast (expr, TypeManager.EnumToUnderlying (expr_type));
				}
				
				e = ConvertImplicit (ec, e, target_type, loc);
				if (e != null)
					return e;
				
				return ConvertNumericExplicit (ec, e, target_type);
			}
			
			ne = ConvertReferenceExplicit (expr, target_type);
			if (ne != null)
				return ne;

			ne = ExplicitUserConversion (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			error30 (loc, expr_type, target_type);
			return null;
		}

		/// <summary>
		///   Same as ConverExplicit, only it doesn't include user defined conversions
		/// </summary>
		static public Expression ConvertExplicitStandard (EmitContext ec, Expression expr,
								  Type target_type, Location l)
		{
			Expression ne = ConvertImplicitStandard (ec, expr, target_type, l);

			if (ne != null)
				return ne;

			ne = ConvertNumericExplicit (ec, expr, target_type);
			if (ne != null)
				return ne;

			ne = ConvertReferenceExplicit (expr, target_type);
			if (ne != null)
				return ne;

			error30 (l, expr.Type, target_type);
			return null;
		}

		static string ExprClassName (ExprClass c)
		{
			switch (c){
			case ExprClass.Invalid:
				return "Invalid";
			case ExprClass.Value:
				return "value";
			case ExprClass.Variable:
				return "variable";
			case ExprClass.Namespace:
				return "namespace";
			case ExprClass.Type:
				return "type";
			case ExprClass.MethodGroup:
				return "method group";
			case ExprClass.PropertyAccess:
				return "property access";
			case ExprClass.EventAccess:
				return "event access";
			case ExprClass.IndexerAccess:
				return "indexer access";
			case ExprClass.Nothing:
				return "null";
			}
			throw new Exception ("Should not happen");
		}
		
		/// <summary>
		///   Reports that we were expecting `expr' to be of class `expected'
		/// </summary>
		protected void report118 (Location loc, Expression expr, string expected)
		{
			string kind = "Unknown";
			
			if (expr != null)
				kind = ExprClassName (expr.eclass);

			Error (118, loc, "Expression denotes a `" + kind +
			       "' where a `" + expected + "' was expected");
		}

		static void error31 (Location l, string val, Type t)
		{
			Report.Error (31, l, "Constant value `" + val + "' cannot be converted to " +
				      TypeManager.CSharpName (t));
		}
		
		/// <summary>
		///   Converts the IntConstant, UIntConstant, LongConstant or
		///   ULongConstant into the integral target_type.   Notice
		///   that we do not return an `Expression' we do return
		///   a boxed integral type.
		///
		///   FIXME: Since I added the new constants, we need to
		///   also support conversions from CharConstant, ByteConstant,
		///   SByteConstant, UShortConstant, ShortConstant
		///
		///   This is used by the switch statement, so the domain
		///   of work is restricted to the literals above, and the
		///   targets are int32, uint32, char, byte, sbyte, ushort,
		///   short, uint64 and int64
		/// </summary>
		public static object ConvertIntLiteral (Constant c, Type target_type, Location loc)
		{
			string s = "";

			if (c.Type == target_type)
				return ((Constant) c).GetValue ();

			//
			// Make into one of the literals we handle, we dont really care
			// about this value as we will just return a few limited types
			// 
			if (c is EnumConstant)
				c = ((EnumConstant)c).WidenToCompilerConstant ();

			if (c is IntConstant){
				int v = ((IntConstant) c).Value;
				
				if (target_type == TypeManager.uint32_type){
					if (v >= 0)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v >= SByte.MinValue && v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v >= Int16.MinValue && v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v >= UInt16.MinValue && v <= UInt16.MaxValue)
						return (ushort) v;
				} else if (target_type == TypeManager.int64_type)
					return (long) v;
			        else if (target_type == TypeManager.uint64_type){
					if (v > 0)
						return (ulong) v;
				}

				s = v.ToString ();
			} else if (c is	UIntConstant){
				uint v = ((UIntConstant) c).Value;

				if (target_type == TypeManager.int32_type){
					if (v <= Int32.MaxValue)
						return (int) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v <= UInt16.MaxValue)
						return (ushort) v;
				} else if (target_type == TypeManager.int64_type)
					return (long) v;
			        else if (target_type == TypeManager.uint64_type)
					return (ulong) v;
				s = v.ToString ();
			} else if (c is	LongConstant){ 
				long v = ((LongConstant) c).Value;

				if (target_type == TypeManager.int32_type){
					if (v >= UInt32.MinValue && v <= UInt32.MaxValue)
						return (int) v;
				} else if (target_type == TypeManager.uint32_type){
					if (v >= 0 && v <= UInt32.MaxValue)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v >= SByte.MinValue && v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v >= Int16.MinValue && v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v >= UInt16.MinValue && v <= UInt16.MaxValue)
						return (ushort) v;
			        } else if (target_type == TypeManager.uint64_type){
					if (v > 0)
						return (ulong) v;
				}
				s = v.ToString ();
			} else if (c is	ULongConstant){
				ulong v = ((ULongConstant) c).Value;

				if (target_type == TypeManager.int32_type){
					if (v <= Int32.MaxValue)
						return (int) v;
				} else if (target_type == TypeManager.uint32_type){
					if (v <= UInt32.MaxValue)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= (int) SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= UInt16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v <= UInt16.MaxValue)
						return (ushort) v;
			        } else if (target_type == TypeManager.int64_type){
					if (v <= Int64.MaxValue)
						return (long) v;
				}
				s = v.ToString ();
			} else if (c is ByteConstant){
				byte v = ((ByteConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type)
					return (uint) v;
				else if (target_type == TypeManager.char_type)
					return (char) v;
				else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type)
					return (short) v;
				else if (target_type == TypeManager.ushort_type)
					return (ushort) v;
			        else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;
				s = v.ToString ();
			} else if (c is SByteConstant){
				sbyte v = ((SByteConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type){
					if (v >= 0)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= 0)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= 0)
						return (byte) v;
				} else if (target_type == TypeManager.short_type)
					return (short) v;
				else if (target_type == TypeManager.ushort_type){
					if (v >= 0)
						return (ushort) v;
			        } else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type){
					if (v >= 0)
						return (ulong) v;
				}
				s = v.ToString ();
			} else if (c is ShortConstant){
				short v = ((ShortConstant) c).Value;
				
				if (target_type == TypeManager.int32_type){
					return (int) v;
				} else if (target_type == TypeManager.uint32_type){
					if (v >= 0)
						return (uint) v;
				} else if (target_type == TypeManager.char_type){
					if (v >= 0)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v >= SByte.MinValue && v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.ushort_type){
					if (v >= 0)
						return (ushort) v;
			        } else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;

				s = v.ToString ();
			} else if (c is UShortConstant){
				ushort v = ((UShortConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type)
					return (uint) v;
				else if (target_type == TypeManager.char_type){
					if (v >= Char.MinValue && v <= Char.MaxValue)
						return (char) v;
				} else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= Int16.MaxValue)
						return (short) v;
			        } else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;

				s = v.ToString ();
			} else if (c is CharConstant){
				char v = ((CharConstant) c).Value;
				
				if (target_type == TypeManager.int32_type)
					return (int) v;
				else if (target_type == TypeManager.uint32_type)
					return (uint) v;
				else if (target_type == TypeManager.byte_type){
					if (v >= Byte.MinValue && v <= Byte.MaxValue)
						return (byte) v;
				} else if (target_type == TypeManager.sbyte_type){
					if (v <= SByte.MaxValue)
						return (sbyte) v;
				} else if (target_type == TypeManager.short_type){
					if (v <= Int16.MaxValue)
						return (short) v;
				} else if (target_type == TypeManager.ushort_type)
					return (short) v;
			        else if (target_type == TypeManager.int64_type)
					return (long) v;
				else if (target_type == TypeManager.uint64_type)
					return (ulong) v;

				s = v.ToString ();
			}
			error31 (loc, s, target_type);
			return null;
		}		
			
	}

	/// <summary>
	///   This is just a base class for expressions that can
	///   appear on statements (invocations, object creation,
	///   assignments, post/pre increment and decrement).  The idea
	///   being that they would support an extra Emition interface that
	///   does not leave a result on the stack.
	/// </summary>
	public abstract class ExpressionStatement : Expression {

		/// <summary>
		///   Requests the expression to be emitted in a `statement'
		///   context.  This means that no new value is left on the
		///   stack after invoking this method (constrasted with
		///   Emit that will always leave a value on the stack).
		/// </summary>
		public abstract void EmitStatement (EmitContext ec);
	}

	/// <summary>
	///   This kind of cast is used to encapsulate the child
	///   whose type is child.Type into an expression that is
	///   reported to return "return_type".  This is used to encapsulate
	///   expressions which have compatible types, but need to be dealt
	///   at higher levels with.
	///
	///   For example, a "byte" expression could be encapsulated in one
	///   of these as an "unsigned int".  The type for the expression
	///   would be "unsigned int".
	///
	/// </summary>
	public class EmptyCast : Expression {
		protected Expression child;

		public EmptyCast (Expression child, Type return_type)
		{
			eclass = child.eclass;
			type = return_type;
			this.child = child;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
		}
	}

	/// <summary>
	///  This class is used to wrap literals which belong inside Enums
	/// </summary>
	public class EnumConstant : Constant {
		public Constant Child;

		public EnumConstant (Constant child, Type enum_type)
		{
			eclass = child.eclass;
			this.Child = child;
			type = enum_type;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Child.Emit (ec);
		}

		public override object GetValue ()
		{
			return Child.GetValue ();
		}

		//
		// Converts from one of the valid underlying types for an enumeration
		// (int32, uint32, int64, uint64, short, ushort, byte, sbyte) to
		// one of the internal compiler literals: Int/UInt/Long/ULong Literals.
		//
		public Constant WidenToCompilerConstant ()
		{
			Type t = TypeManager.EnumToUnderlying (Child.Type);
			object v = ((Constant) Child).GetValue ();;
			
			if (t == TypeManager.int32_type)
				return new IntConstant ((int) v);
			if (t == TypeManager.uint32_type)
				return new UIntConstant ((uint) v);
			if (t == TypeManager.int64_type)
				return new LongConstant ((long) v);
			if (t == TypeManager.uint64_type)
				return new ULongConstant ((ulong) v);
			if (t == TypeManager.short_type)
				return new ShortConstant ((short) v);
			if (t == TypeManager.ushort_type)
				return new UShortConstant ((ushort) v);
			if (t == TypeManager.byte_type)
				return new ByteConstant ((byte) v);
			if (t == TypeManager.sbyte_type)
				return new SByteConstant ((sbyte) v);

			throw new Exception ("Invalid enumeration underlying type: " + t);
		}

		//
		// Extracts the value in the enumeration on its native representation
		//
		public object GetPlainValue ()
		{
			Type t = TypeManager.EnumToUnderlying (Child.Type);
			object v = ((Constant) Child).GetValue ();;
			
			if (t == TypeManager.int32_type)
				return (int) v;
			if (t == TypeManager.uint32_type)
				return (uint) v;
			if (t == TypeManager.int64_type)
				return (long) v;
			if (t == TypeManager.uint64_type)
				return (ulong) v;
			if (t == TypeManager.short_type)
				return (short) v;
			if (t == TypeManager.ushort_type)
				return (ushort) v;
			if (t == TypeManager.byte_type)
				return (byte) v;
			if (t == TypeManager.sbyte_type)
				return (sbyte) v;

			return null;
		}
		
		public override string AsString ()
		{
			return Child.AsString ();
		}
	}

	/// <summary>
	///   This kind of cast is used to encapsulate Value Types in objects.
	///
	///   The effect of it is to box the value type emitted by the previous
	///   operation.
	/// </summary>
	public class BoxedCast : EmptyCast {

		public BoxedCast (Expression expr)
			: base (expr, TypeManager.object_type)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (OpCodes.Box, child.Type);
		}
	}

	public class UnboxCast : EmptyCast {
		public UnboxCast (Expression expr, Type return_type)
			: base (expr, return_type)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Type t = type;
			ILGenerator ig = ec.ig;
			
			base.Emit (ec);
			ig.Emit (OpCodes.Unbox, t);

			//
			// Load the object from the pointer
			//
		basic_type:
			
			if (t == TypeManager.int32_type)
				ig.Emit (OpCodes.Ldind_I4);
			else if (t == TypeManager.uint32_type)
				ig.Emit (OpCodes.Ldind_U4);
			else if (t == TypeManager.short_type)
				ig.Emit (OpCodes.Ldind_I2);
			else if (t == TypeManager.ushort_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.char_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.byte_type)
				ig.Emit (OpCodes.Ldind_U1);
			else if (t == TypeManager.sbyte_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.uint64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.int64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.float_type)
				ig.Emit (OpCodes.Ldind_R4);
			else if (t == TypeManager.double_type)
				ig.Emit (OpCodes.Ldind_R8);
			else if (t == TypeManager.bool_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.intptr_type)
				ig.Emit (OpCodes.Ldind_I);
			else if (TypeManager.IsEnumType (t)){
				t = TypeManager.EnumToUnderlying (t);
				goto basic_type;
			} else
				ig.Emit (OpCodes.Ldobj, t);
		}
	}
	
	/// <summary>
	///   This is used to perform explicit numeric conversions.
	///
	///   Explicit numeric conversions might trigger exceptions in a checked
	///   context, so they should generate the conv.ovf opcodes instead of
	///   conv opcodes.
	/// </summary>
	public class ConvCast : EmptyCast {
		public enum Mode : byte {
			I1_U1, I1_U2, I1_U4, I1_U8, I1_CH,
			U1_I1, U1_CH,
			I2_I1, I2_U1, I2_U2, I2_U4, I2_U8, I2_CH,
			U2_I1, U2_U1, U2_I2, U2_CH,
			I4_I1, I4_U1, I4_I2, I4_U2, I4_U4, I4_U8, I4_CH,
			U4_I1, U4_U1, U4_I2, U4_U2, U4_I4, U4_CH,
			I8_I1, I8_U1, I8_I2, I8_U2, I8_I4, I8_U4, I8_U8, I8_CH,
			U8_I1, U8_U1, U8_I2, U8_U2, U8_I4, U8_U4, U8_I8, U8_CH,
			CH_I1, CH_U1, CH_I2,
			R4_I1, R4_U1, R4_I2, R4_U2, R4_I4, R4_U4, R4_I8, R4_U8, R4_CH,
			R8_I1, R8_U1, R8_I2, R8_U2, R8_I4, R8_U4, R8_I8, R8_U8, R8_CH, R8_R4
		}

		Mode mode;
		
		public ConvCast (Expression child, Type return_type, Mode m)
			: base (child, return_type)
		{
			mode = m;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			base.Emit (ec);

			if (ec.CheckState){
				switch (mode){
				case Mode.I1_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I1_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I1_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I1_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I1_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U1_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U1_CH: /* nothing */ break;

				case Mode.I2_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I2_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I2_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I2_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I2_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I2_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U2_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U2_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U2_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I4_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I4_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I4_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I4_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I4_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I4_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U4_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U4_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U4_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U4_U2: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U4_I4: ig.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U4_CH: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;

				case Mode.I8_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I8_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I8_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I8_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I8_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.I8_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I8_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I8_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_Ovf_U4_Un); break;
				case Mode.U8_I8: ig.Emit (OpCodes.Conv_Ovf_I8_Un); break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;

				case Mode.CH_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.CH_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.CH_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;

				case Mode.R4_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R4_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R4_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R4_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R4_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R4_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R4_I8: ig.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R4_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R4_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.R8_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R8_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R8_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R8_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R8_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R8_I8: ig.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R8_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R8_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_R4: ig.Emit (OpCodes.Conv_R4); break;
				}
			} else {
				switch (mode){
				case Mode.I1_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I1_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I1_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I1_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I1_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U1_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U1_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.I2_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I2_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I2_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I2_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I2_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I2_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U2_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U2_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U2_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.I4_U4: /* nothing */ break;
				case Mode.I4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I4_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U4_I4: /* nothing */ break;
				case Mode.U4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.I8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.I8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.I8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I8_U8: /* nothing */ break;
				case Mode.I8_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.U8_I8: /* nothing */ break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.CH_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.CH_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.CH_I2: ig.Emit (OpCodes.Conv_I2); break;

				case Mode.R4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.R4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.R4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.R4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R4_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.R4_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.R4_I8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.R4_U8: ig.Emit (OpCodes.Conv_U8); break;
				case Mode.R4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.R8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.R8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.R8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.R8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.R8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.R8_I8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.R8_U8: ig.Emit (OpCodes.Conv_U8); break;
				case Mode.R8_CH: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_R4: ig.Emit (OpCodes.Conv_R4); break;
				}
			}
		}
	}
	
	public class OpcodeCast : EmptyCast {
		OpCode op, op2;
		bool second_valid;
		
		public OpcodeCast (Expression child, Type return_type, OpCode op)
			: base (child, return_type)
			
		{
			this.op = op;
			second_valid = false;
		}

		public OpcodeCast (Expression child, Type return_type, OpCode op, OpCode op2)
			: base (child, return_type)
			
		{
			this.op = op;
			this.op2 = op2;
			second_valid = true;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (op);

			if (second_valid)
				ec.ig.Emit (op2);
		}			
	}

	/// <summary>
	///   This kind of cast is used to encapsulate a child and cast it
	///   to the class requested
	/// </summary>
	public class ClassCast : EmptyCast {
		public ClassCast (Expression child, Type return_type)
			: base (child, return_type)
			
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);

			ec.ig.Emit (OpCodes.Castclass, type);
		}			
		
	}
	
	/// <summary>
	///   SimpleName expressions are initially formed of a single
	///   word and it only happens at the beginning of the expression.
	/// </summary>
	///
	/// <remarks>
	///   The expression will try to be bound to a Field, a Method
	///   group or a Property.  If those fail we pass the name to our
	///   caller and the SimpleName is compounded to perform a type
	///   lookup.  The idea behind this process is that we want to avoid
	///   creating a namespace map from the assemblies, as that requires
	///   the GetExportedTypes function to be called and a hashtable to
	///   be constructed which reduces startup time.  If later we find
	///   that this is slower, we should create a `NamespaceExpr' expression
	///   that fully participates in the resolution process. 
	///   
	///   For example `System.Console.WriteLine' is decomposed into
	///   MemberAccess (MemberAccess (SimpleName ("System"), "Console"), "WriteLine")
	///   
	///   The first SimpleName wont produce a match on its own, so it will
	///   be turned into:
	///   MemberAccess (SimpleName ("System.Console"), "WriteLine").
	///   
	///   System.Console will produce a TypeExpr match.
	///   
	///   The downside of this is that we might be hitting `LookupType' too many
	///   times with this scheme.
	/// </remarks>
	public class SimpleName : Expression {
		public readonly string Name;
		public readonly Location Location;
		
		public SimpleName (string name, Location l)
		{
			Name = name;
			Location = l;
		}

		public static void Error120 (Location l, string name)
		{
			Report.Error (
				120, l,
				"An object reference is required " +
				"for the non-static field `"+name+"'");
		}
		
		//
		// Checks whether we are trying to access an instance
		// property, method or field from a static body.
		//
		Expression MemberStaticCheck (Expression e)
		{
			if (e is FieldExpr){
				FieldInfo fi = ((FieldExpr) e).FieldInfo;
				
				if (!fi.IsStatic){
					Error120 (Location, Name);
					return null;
				}
			} else if (e is MethodGroupExpr){
				MethodGroupExpr mg = (MethodGroupExpr) e;

				if (!mg.RemoveInstanceMethods ()){
					Error120 (Location, mg.Methods [0].Name);
					return null;
				}
				return e;
			} else if (e is PropertyExpr){
				if (!((PropertyExpr) e).IsStatic){
					Error120 (Location, Name);
					return null;
				}
			} else if (e is EventExpr) {
				if (!((EventExpr) e).IsStatic) {
					Error120 (Location, Name);
					return null;
				}
			}

			return e;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			return SimpleNameResolve (ec, false);
		}

		public Expression DoResolveAllowStatic (EmitContext ec)
		{
			return SimpleNameResolve (ec, true);
		}

		/// <remarks>
		///   7.5.2: Simple Names. 
		///
		///   Local Variables and Parameters are handled at
		///   parse time, so they never occur as SimpleNames.
		///
		///   The `allow_static' flag is used by MemberAccess only
		///   and it is used to inform us that it is ok for us to 
		///   avoid the static check, because MemberAccess might end
		///   up resolving the Name as a Type name and the access as
		///   a static type access.
		///
		///   ie: Type Type; .... { Type.GetType (""); }
		///
		///   Type is both an instance variable and a Type;  Type.GetType
		///   is the static method not an instance method of type.
		/// </remarks>
		Expression SimpleNameResolve (EmitContext ec, bool allow_static)
		{
			Expression e;

			//
			// Stage 1: Performed by the parser (binding to locals or parameters).
			//

			//
			// Stage 2: Lookup members 
			//
			e = MemberLookup (ec, ec.TypeContainer.TypeBuilder, Name, Location);
			if (e == null){
				//
				// Stage 3: Lookup symbol in the various namespaces. 
				//
				DeclSpace ds = ec.TypeContainer;
				Type t;
				string alias_value;
				
				if ((t = RootContext.LookupType (ds, Name, true, Location)) != null)
					return new TypeExpr (t);
				
				//
				// Stage 2 part b: Lookup up if we are an alias to a type
				// or a namespace.
				//
				// Since we are cheating: we only do the Alias lookup for
				// namespaces if the name does not include any dots in it
				//
				
				if (Name.IndexOf ('.') == -1 && (alias_value = ec.TypeContainer.LookupAlias (Name)) != null) {
				// System.Console.WriteLine (Name + " --> " + alias_value);
					if ((t = RootContext.LookupType (ds, alias_value, true, Location))
					    != null)
						return new TypeExpr (t);
					
				// we have alias value, but it isn't Type, so try if it's namespace
					return new SimpleName (alias_value, Location);
				}
				
				// No match, maybe our parent can compose us
				// into something meaningful.
				return this;
			}
			
			//
			// Stage 2 continues here. 
			// 
			if (e is TypeExpr)
				return e;

			if (e is FieldExpr){
				FieldExpr fe = (FieldExpr) e;
				FieldInfo fi = fe.FieldInfo;

				if (ec.IsStatic){
					if (!allow_static && !fi.IsStatic){
						Error120 (Location, Name);
						return null;
					}
				} else {
					// If we are not in static code and this
					// field is not static, set the instance to `this'.

					if (!fi.IsStatic)
						fe.InstanceExpression = ec.This;
				}

				
				if (fi is FieldBuilder) {
					Const c = TypeManager.LookupConstant ((FieldBuilder) fi);
					
					if (c != null) {
						object o = c.LookupConstantValue (ec);
						object real_value = ((Constant)c.Expr).GetValue ();
						return Constantify (real_value, fi.FieldType);
					}
				}

				return e;
			} 				

			if (e is EventExpr) {
				//
				// If the event is local to this class, we transform ourselves into
				// a FieldExpr
				//
				EventExpr ee = (EventExpr) e;

				Expression ml = MemberLookup (
					ec, ec.TypeContainer.TypeBuilder, ee.EventInfo.Name,
					MemberTypes.Event, AllBindingFlags, Location);

				if (ml != null) {
					MemberInfo mi = ec.TypeContainer.GetFieldFromEvent ((EventExpr) ml);

					if (mi == null) {
						//
						// If this happens, then we have an event with its own
						// accessors and private field etc so there's no need
						// to transform ourselves : we should instead flag an error
						//
						Assign.error70 (ee.EventInfo, Location);
						return null;
					}

					ml = ExprClassFromMemberInfo (ec, mi, Location);
					
					if (ml == null) {
						Report.Error (-200, Location, "Internal error!!");
						return null;
					}

					Expression instance_expr;
					
					FieldInfo fi = ((FieldExpr) ml).FieldInfo;

					if (fi.IsStatic)
						instance_expr = null;
					else
						instance_expr = ec.This;

					instance_expr = instance_expr.Resolve (ec);

					if (instance_expr != null)
						instance_expr = instance_expr.Resolve (ec);
					
					return MemberAccess.ResolveMemberAccess (ec, ml, instance_expr, Location, null);
				}
			}
				
			
			if (ec.IsStatic){
				if (allow_static)
					return e;

				return MemberStaticCheck (e);
			} else
				return e;
		}

		public override void Emit (EmitContext ec)
		{
			//
			// If this is ever reached, then we failed to
			// find the name as a namespace
			//

			Error (103, Location, "The name `" + Name +
			       "' does not exist in the class `" +
			       ec.TypeContainer.Name + "'");
		}
	}
	
	/// <summary>
	///   Fully resolved expression that evaluates to a type
	/// </summary>
	public class TypeExpr : Expression {
		public TypeExpr (Type t)
		{
			Type = t;
			eclass = ExprClass.Type;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("Implement me");
		}
	}

	/// <summary>
	///   MethodGroup Expression.
	///  
	///   This is a fully resolved expression that evaluates to a type
	/// </summary>
	public class MethodGroupExpr : Expression {
		public MethodBase [] Methods;
		Expression instance_expression = null;
		
		public MethodGroupExpr (MemberInfo [] mi)
		{
			Methods = new MethodBase [mi.Length];
			mi.CopyTo (Methods, 0);
			eclass = ExprClass.MethodGroup;
		}

		public MethodGroupExpr (ArrayList l)
		{
			Methods = new MethodBase [l.Count];

			l.CopyTo (Methods, 0);
			eclass = ExprClass.MethodGroup;
		}
		
		//
		// `A method group may have associated an instance expression' 
		// 
		public Expression InstanceExpression {
			get {
				return instance_expression;
			}

			set {
				instance_expression = value;
			}
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("This should never be reached");
		}

		bool RemoveMethods (bool keep_static)
		{
			ArrayList smethods = new ArrayList ();
			int top = Methods.Length;
			int i;
			
			for (i = 0; i < top; i++){
				MethodBase mb = Methods [i];

				if (mb.IsStatic == keep_static)
					smethods.Add (mb);
			}

			if (smethods.Count == 0)
				return false;

			Methods = new MethodBase [smethods.Count];
			smethods.CopyTo (Methods, 0);

			return true;
		}
		
		/// <summary>
		///   Removes any instance methods from the MethodGroup, returns
		///   false if the resulting set is empty.
		/// </summary>
		public bool RemoveInstanceMethods ()
		{
			return RemoveMethods (true);
		}

		/// <summary>
		///   Removes any static methods from the MethodGroup, returns
		///   false if the resulting set is empty.
		/// </summary>
		public bool RemoveStaticMethods ()
		{
			return RemoveMethods (false);
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to a Field
	/// </summary>
	public class FieldExpr : Expression, IAssignMethod, IMemoryLocation {
		public readonly FieldInfo FieldInfo;
		public Expression InstanceExpression;
		Location loc;
		
		public FieldExpr (FieldInfo fi, Location l)
		{
			FieldInfo = fi;
			eclass = ExprClass.Variable;
			type = fi.FieldType;
			loc = l;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			if (!FieldInfo.IsStatic){
				if (InstanceExpression == null){
					throw new Exception ("non-static FieldExpr without instance var\n" +
							     "You have to assign the Instance variable\n" +
							     "Of the FieldExpr to set this\n");
				}

				InstanceExpression = InstanceExpression.Resolve (ec);
				if (InstanceExpression == null)
					return null;
			}

			return this;
		}

		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			Expression e = DoResolve (ec);

			if (e == null)
				return null;
			
			if (!FieldInfo.IsInitOnly)
				return this;

			//
			// InitOnly fields can only be assigned in constructors
			//

			if (ec.IsConstructor)
				return this;

			Report.Error (191, loc,
				      "Readonly field can not be assigned outside " +
				      "of constructor or variable initializer");
			
			return null;
		}

		override public void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			bool is_volatile = false;
				
			if (FieldInfo is FieldBuilder){
				Field f = TypeManager.GetField (FieldInfo);
				if (f != null && (f.ModFlags & Modifiers.VOLATILE) != 0)
					is_volatile = true;
				
				f.status |= Field.Status.USED;
			}
			
			if (FieldInfo.IsStatic){
				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			} else {
				if (InstanceExpression.Type.IsValueType){
					IMemoryLocation ml;
					
					if (!(InstanceExpression is IMemoryLocation)){
						LocalTemporary tempo = new LocalTemporary (
							ec, InstanceExpression.Type);

						InstanceExpression.Emit (ec);
						tempo.Store (ec);
						ml = tempo;
					} else
						ml = (IMemoryLocation) InstanceExpression;

					ml.AddressOf (ec);
				} else 
					InstanceExpression.Emit (ec);

				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldfld, FieldInfo);
			}
		}

		public void EmitAssign (EmitContext ec, Expression source)
		{
			bool is_static = FieldInfo.IsStatic;
			ILGenerator ig = ec.ig;
			
			if (!is_static){
				Expression instance = InstanceExpression;

				if (instance.Type.IsValueType){
					if (instance is IMemoryLocation){
						IMemoryLocation ml = (IMemoryLocation) instance;

						ml.AddressOf (ec);
					} else
						throw new Exception ("The " + instance + " of type " +
								     instance.Type +
								     " represents a ValueType and does " +
								     "not implement IMemoryLocation");
				} else
					instance.Emit (ec);
			}
			source.Emit (ec);

			if (FieldInfo is FieldBuilder){
				Field f = TypeManager.GetField (FieldInfo);
				if (f != null && (f.ModFlags & Modifiers.VOLATILE) != 0)
					ig.Emit (OpCodes.Volatile);
			}
			
			if (is_static)
				ig.Emit (OpCodes.Stsfld, FieldInfo);
			else 
				ig.Emit (OpCodes.Stfld, FieldInfo);

			if (FieldInfo is FieldBuilder){
				Field f = TypeManager.GetField (FieldInfo);

				f.status |= Field.Status.ASSIGNED;
			}
		}
		
		public void AddressOf (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			if (FieldInfo is FieldBuilder){
				Field f = TypeManager.GetField (FieldInfo);
				if (f != null && (f.ModFlags & Modifiers.VOLATILE) != 0)
					ig.Emit (OpCodes.Volatile);
			}

			//
			// FIXME:
			//
			// Mhm.  We do not know what we are being used for:
			// READING or WRITING the field.
			//
			// I think we want an extra argument to AddressOf to pass
			// this semantic information.
			//
			// For now: just flag both assigned and used.
			//
			if (FieldInfo is FieldBuilder){
				Field f = TypeManager.GetField (FieldInfo);

				f.status |= Field.Status.ASSIGNED | Field.Status.USED;
			}
			
			if (FieldInfo.IsStatic)
				ig.Emit (OpCodes.Ldsflda, FieldInfo);
			else {
				InstanceExpression.Emit (ec);
				ig.Emit (OpCodes.Ldflda, FieldInfo);
			}

			
		}
	}
	
	/// <summary>
	///   Expression that evaluates to a Property.  The Assign class
	///   might set the `Value' expression if we are in an assignment.
	///
	///   This is not an LValue because we need to re-write the expression, we
	///   can not take data from the stack and store it.  
	/// </summary>
	public class PropertyExpr : ExpressionStatement, IAssignMethod {
		public readonly PropertyInfo PropertyInfo;
		public readonly bool IsStatic;
		public bool IsBase;
		MethodInfo [] Accessors;
		Location loc;
		
		Expression instance_expr;
		
		public PropertyExpr (PropertyInfo pi, Location l)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
			IsStatic = false;
			loc = l;
			Accessors = TypeManager.GetAccessors (pi);

			if (Accessors != null)
				for (int i = 0; i < Accessors.Length; i++){
					if (Accessors [i] != null)
						if (Accessors [i].IsStatic)
							IsStatic = true;
				}
			else
				Accessors = new MethodInfo [2];
			
			type = pi.PropertyType;
		}

		//
		// The instance expression associated with this expression
		//
		public Expression InstanceExpression {
			set {
				instance_expr = value;
			}

			get {
				return instance_expr;
			}
		}

		public bool VerifyAssignable ()
		{
			if (!PropertyInfo.CanWrite){
				Report.Error (200, loc, 
					      "The property `" + PropertyInfo.Name +
					      "' can not be assigned to, as it has not set accessor");
				return false;
			}

			return true;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			if (!PropertyInfo.CanRead){
				Report.Error (154, loc, 
					      "The property `" + PropertyInfo.Name +
					      "' can not be used in " +
					      "this context because it lacks a get accessor");
				return null;
			}

			type = PropertyInfo.PropertyType;

			return this;
		}

		override public void Emit (EmitContext ec)
		{
			Invocation.EmitCall (ec, IsBase, IsStatic, instance_expr, Accessors [0], null);
			
		}

		//
		// Implements the IAssignMethod interface for assignments
		//
		public void EmitAssign (EmitContext ec, Expression source)
		{
			Argument arg = new Argument (source, Argument.AType.Expression);
			ArrayList args = new ArrayList ();

			args.Add (arg);
			Invocation.EmitCall (ec, false, IsStatic, instance_expr, Accessors [1], args);
		}

		override public void EmitStatement (EmitContext ec)
		{
			Emit (ec);
			ec.ig.Emit (OpCodes.Pop);
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to an Event
	/// </summary>
	public class EventExpr : Expression {
		public readonly EventInfo EventInfo;
		Location loc;
		public Expression InstanceExpression;

		public readonly bool IsStatic;

		MethodInfo add_accessor, remove_accessor;
		
		public EventExpr (EventInfo ei, Location loc)
		{
			EventInfo = ei;
			this.loc = loc;
			eclass = ExprClass.EventAccess;

			add_accessor = TypeManager.GetAddMethod (ei);
			remove_accessor = TypeManager.GetRemoveMethod (ei);
			
			if (add_accessor.IsStatic || remove_accessor.IsStatic)
					IsStatic = true;

			if (EventInfo is MyEventBuilder)
				type = ((MyEventBuilder) EventInfo).EventType;
			else
				type = EventInfo.EventHandlerType;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			// We are born fully resolved
			return this;
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("Should not happen I think");
		}

		public void EmitAddOrRemove (EmitContext ec, Expression source)
		{
			Expression handler = ((Binary) source).Right;
			
			Argument arg = new Argument (handler, Argument.AType.Expression);
			ArrayList args = new ArrayList ();
				
			args.Add (arg);
			
			if (((Binary) source).Oper == Binary.Operator.Addition)
				Invocation.EmitCall (
					ec, false, IsStatic, InstanceExpression, add_accessor, args);
			else
				Invocation.EmitCall (
					ec, false, IsStatic, InstanceExpression, remove_accessor, args);
		}
	}
}	
