//
// pending.cs: Pending method implementation
//
// Author:
//   Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	struct TypeAndMethods {
		public Type          type;
		public MethodInfo [] methods;
		
		// Far from ideal, but we want to avoid creating a copy
		// of methods above.
		public Type [][]     args;
		
		//
		// This flag on the method says `We found a match, but
		// because it was private, we could not use the match
		//
		public bool []       found;
	}

	public class PendingImplementation {
		/// <summary>
		///   The container for this PendingImplementation
		/// </summary>
		TypeContainer container;
		
		/// <summary>
		///   This filter is used by FindMembers, and it is used to
		///   extract only virtual/abstract fields
		/// </summary>
		static MemberFilter virtual_method_filter;

		/// <summary>
		///   This is the array of TypeAndMethods that describes the pending implementations
		///   (both interfaces and abstract methods in parent class)
		/// </summary>
		TypeAndMethods [] pending_implementations;

		static bool IsVirtualFilter (MemberInfo m, object filterCriteria)
		{
			if (!(m is MethodInfo))
				return false;

			return ((MethodInfo) m).IsVirtual;
		}

		/// <summary>
		///   Inits the virtual_method_filter
		/// </summary>
		static PendingImplementation ()
		{
			virtual_method_filter = new MemberFilter (IsVirtualFilter);
		}

		// <remarks>
		//   Returns a list of the abstract methods that are exposed by all of our
		//   parents that we must implement.  Notice that this `flattens' the
		//   method search space, and takes into account overrides.  
		// </remarks>
		static ArrayList GetAbstractMethods (Type t)
		{
			ArrayList list = null;
			bool searching = true;
			Type current_type = t;
			
			do {
				MemberInfo [] mi;
				
				mi = TypeContainer.FindMembers (
					current_type, MemberTypes.Method,
					BindingFlags.Public | BindingFlags.Instance |
					BindingFlags.DeclaredOnly,
					virtual_method_filter, null);

				if (current_type == TypeManager.object_type)
					searching = false;
				else {
					current_type = current_type.BaseType;
					if (!current_type.IsAbstract)
						searching = false;
				}

				if (mi == null)
					continue;

				int count = mi.Length;
				if (count == 0)
					continue;

				if (count == 1 && !(mi [0] is MethodBase))
					searching = false;
				else 
					list = TypeManager.CopyNewMethods (list, mi);
			} while (searching);

			if (list == null)
				return null;
			
			for (int i = 0; i < list.Count; i++){
				while (list.Count > i && !((MethodInfo) list [i]).IsAbstract)
					list.RemoveAt (i);
			}

			if (list.Count == 0)
				return null;

			return list;
		}

		PendingImplementation (TypeContainer container, Type [] ifaces, ArrayList abstract_methods, int total)
		{
			TypeBuilder type_builder = container.TypeBuilder;
			
			this.container = container;
			pending_implementations = new TypeAndMethods [total];

			int i = 0;
			if (ifaces != null){
				foreach (Type t in ifaces){
					MethodInfo [] mi;

					if (t is TypeBuilder){
						Interface iface;

						iface = TypeManager.LookupInterface (t);
						
						mi = iface.GetMethods ();
					} else
						mi = t.GetMethods ();

					int count = mi.Length;
					pending_implementations [i].type = t;
					pending_implementations [i].methods = mi;
					pending_implementations [i].args = new Type [count][];
					pending_implementations [i].found = new bool [count];

					int j = 0;
					foreach (MethodInfo m in mi){
						Type [] types = TypeManager.GetArgumentTypes (m);

						pending_implementations [i].args [j] = types;
						j++;
					}
					i++;
				}
			}

			if (abstract_methods != null){
				int count = abstract_methods.Count;
				pending_implementations [i].methods = new MethodInfo [count];
				
				abstract_methods.CopyTo (pending_implementations [i].methods, 0);
				pending_implementations [i].found = new bool [count];
				pending_implementations [i].args = new Type [count][];
				pending_implementations [i].type = type_builder;
				
				int j = 0;
				foreach (MemberInfo m in abstract_methods){
					MethodInfo mi = (MethodInfo) m;
					
					Type [] types = TypeManager.GetArgumentTypes (mi);
					
					pending_implementations [i].args [j] = types;
					j++;
				}
			}
		}
		
		//
		// Factory method: if there are pending implementation methods, we return a PendingImplementation
		// object, otherwise we return null.
		//
		// Register method implementations are either abstract methods
		// flagged as such on the base class or interface methods
		//
		static public PendingImplementation GetPendingImplementations (TypeContainer container)
		{
			TypeBuilder type_builder = container.TypeBuilder;
			Type [] ifaces;
			Type b = type_builder.BaseType;
			int icount = 0;

			//
			// Notice that TypeBuilders will only return the interfaces that the Type
			// is supposed to implement, not all the interfaces that the type implements.
			//
			// Completely broken.  Anyways, we take advantage of this, so we only register
			// the implementations that we need, as they are those that are listed by the
			// TypeBuilder.
			//
			ifaces = type_builder.GetInterfaces ();
#if DEBUG
			{
				Type x = type_builder;

				while (x != null){
					Type [] iff = x.GetInterfaces ();
					Console.WriteLine ("Type: " + x.Name);
					
					foreach (Type tt in iff){
						Console.WriteLine ("  Iface: " + tt.Name);
					}
					x = x.BaseType;
				}
			}
#endif
					
			icount = ifaces.Length;

			//
			// If we are implementing an abstract class, and we are not
			// ourselves abstract, and there are abstract methods (C# allows
			// abstract classes that have no abstract methods), then allocate
			// one slot.
			//
			// We also pre-compute the methods.
			//
			bool implementing_abstract = ((b != null) && b.IsAbstract && !type_builder.IsAbstract);
			ArrayList abstract_methods = null;

			if (implementing_abstract){
				abstract_methods = GetAbstractMethods (b);
				
				if (abstract_methods == null)
					implementing_abstract = false;
			}
			
			int total = icount +  (implementing_abstract ? 1 : 0);
			if (total == 0)
				return null;

			return new PendingImplementation (container, ifaces, abstract_methods, total);
		}

		public enum Operation {
			//
			// If you change this, review the whole InterfaceMethod routine as there
			// are a couple of assumptions on these three states
			//
			Lookup, ClearOne, ClearAll
		}
		
		/// <summary>
		///   Whether the specified method is an interface method implementation
		/// </summary>
		public MethodInfo IsInterfaceMethod (Type t, string name, Type ret_type, Type [] args)
		{
			return InterfaceMethod (t, name, ret_type, args, Operation.Lookup);
		}

		public void ImplementMethod (Type t, string name, Type ret_type, Type [] args, bool clear_one) 
		{
			InterfaceMethod (t, name, ret_type, args, clear_one ? Operation.ClearOne : Operation.ClearAll);
		}
		
		/// <remarks>
		///   If a method in Type `t' (or null to look in all interfaces
		///   and the base abstract class) with name `Name', return type `ret_type' and
		///   arguments `args' implements an interface, this method will
		///   return the MethodInfo that this method implements.
		///
		///   The `Operation op' controls whether to lookup, clear the pending bit, or clear
		///   all the methods with the given signature.
		/// </remarks>
		public MethodInfo InterfaceMethod (Type t, string name, Type ret_type, Type [] args, Operation op)
		{
			int arg_len = args.Length;

			if (pending_implementations == null)
				return null;

			foreach (TypeAndMethods tm in pending_implementations){
				if (!(t == null || tm.type == t))
					continue;

				int i = 0;
				foreach (MethodInfo m in tm.methods){
					if (m == null){
						i++;
						continue;
					}

					if (name != m.Name){
						i++;
						continue;
					}

					if (ret_type != m.ReturnType){
						if (!((ret_type == null && m.ReturnType == TypeManager.void_type) ||
						      (m.ReturnType == null && ret_type == TypeManager.void_type)))
						{
							i++;
							continue;
						}
					}

					//
					// Check if we have the same parameters
					//
					if (tm.args [i].Length != arg_len){
						i++;
						continue;
					}

					int j, top = args.Length;
					bool fail = false;
					
					for (j = 0; j < top; j++){
						if (tm.args [i][j] != args[j]){
							fail = true;
							break;
						}
					}
					if (fail){
						i++;
						continue;
					}

					if (op != Operation.Lookup)
						tm.methods [i] = null;
					tm.found [i] = true;

					//
					// Lookups and ClearOne return
					//
					if (op != Operation.ClearAll)
						return m;
				}

				// If a specific type was requested, we can stop now.
				if (tm.type == t)
					return null;
			}
			return null;
		}

		/// <summary>
		///   C# allows this kind of scenarios:
		///   interface I { void M (); }
		///   class X { public void M (); }
		///   class Y : X, I { }
		///
		///   For that case, we create an explicit implementation function
		///   I.M in Y.
		/// </summary>
		void DefineProxy (Type iface, MethodInfo parent_method, MethodInfo iface_method,
				  Type [] args)
		{
			MethodBuilder proxy;

			string proxy_name = iface.Name + "." + iface_method.Name;

			proxy = container.TypeBuilder.DefineMethod (
				proxy_name,
				MethodAttributes.HideBySig |
				MethodAttributes.NewSlot |
				MethodAttributes.Virtual,
				CallingConventions.Standard | CallingConventions.HasThis,
				parent_method.ReturnType, args);

			int top = args.Length;
			ILGenerator ig = proxy.GetILGenerator ();

			ig.Emit (OpCodes.Ldarg_0);
			for (int i = 0; i < top; i++){
				switch (i){
				case 0:
					ig.Emit (OpCodes.Ldarg_1); break;
				case 1:
					ig.Emit (OpCodes.Ldarg_2); break;
				case 2:
					ig.Emit (OpCodes.Ldarg_3); break;
				default:
					ig.Emit (OpCodes.Ldarg, i - 1); break;
				}
			}
			ig.Emit (OpCodes.Call, parent_method);
			ig.Emit (OpCodes.Ret);

			container.TypeBuilder.DefineMethodOverride (proxy, iface_method);
		}
		
		/// <summary>
		///   This function tells whether one of our parent classes implements
		///   the given method (which turns out, it is valid to have an interface
		///   implementation in a parent
		/// </summary>
		bool ParentImplements (Type iface_type, MethodInfo mi)
		{
			MethodSignature ms;
			
			Type [] args = TypeManager.GetArgumentTypes (mi);
			ms = new MethodSignature (mi.Name, mi.ReturnType, args);
			MemberInfo [] list = TypeContainer.FindMembers (
				container.TypeBuilder.BaseType, MemberTypes.Method | MemberTypes.Property,
				BindingFlags.Public | BindingFlags.Instance,
				MethodSignature.method_signature_filter, ms);

			if (list == null || list.Length == 0)
				return false;
			
			DefineProxy (iface_type, (MethodInfo) list [0], mi, args);
			return true;
		}
		
		/// <summary>
		///   Verifies that any pending abstract methods or interface methods
		///   were implemented.
		/// </summary>
		public bool VerifyPendingMethods ()
		{
			int top = pending_implementations.Length;
			bool errors = false;
			int i;
			
			for (i = 0; i < top; i++){
				Type type = pending_implementations [i].type;
				int j = 0;
				
				foreach (MethodInfo mi in pending_implementations [i].methods){
					if (mi == null)
						continue;

					if (type.IsInterface){
						if (ParentImplements (type, mi))
							continue;
 
						string extra = "";
						
						if (pending_implementations [i].found [j])
							extra = ".  (method might be private or static)";
						Report.Error (
							536, container.Location,
							"`" + container.Name + "' does not implement " +
							"interface member `" +
							type.FullName + "." + mi.Name + "'" + extra);
					} else {
						Report.Error (
							534, container.Location,
							"`" + container.Name + "' does not implement " +
							"inherited abstract member `" +
							type.FullName + "." + mi.Name + "'");
					}
					errors = true;
					j++;
				}
			}
			return errors;
		}
	} /* end of class */
}
