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

		// 
		// Whether it is optional, this is used to allow the explicit/implicit
		// implementation when a base class already implements an interface. 
		//
		// For example:
		//
		// class X : IA { }  class Y : X, IA { IA.Explicit (); }
		//
		public bool          optional;
		
		// Far from ideal, but we want to avoid creating a copy
		// of methods above.
		public Type [][]     args;

		//This is used to store the modifiers of arguments
		public Parameter.Modifier [][] mods;
		
		//
		// This flag on the method says `We found a match, but
		// because it was private, we could not use the match
		//
		public bool []       found;

		// If a method is defined here, then we always need to
		// create a proxy for it.  This is used when implementing
		// an interface's indexer with a different IndexerName.
		public MethodInfo [] need_proxy;

		//
		// The name of the indexer (if it exists), precompute set/get, because
		// they would be recomputed many times inside a loop later on.
		//
		public string set_indexer_name;
		public string get_indexer_name;
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
		///   (both interfaces and abstract methods in base class)
		/// </summary>
		TypeAndMethods [] pending_implementations;

		static bool IsVirtualFilter (MemberInfo m, object filterCriteria)
		{
			MethodInfo mi = m as MethodInfo;
			return (mi == null) ? false : mi.IsVirtual;
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
		//   bases that we must implement.  Notice that this `flattens' the
		//   method search space, and takes into account overrides.  
		// </remarks>
		static ArrayList GetAbstractMethods (Type t)
		{
			ArrayList list = null;
			bool searching = true;
			Type current_type = t;
			
			do {
				MemberList mi;
				
				mi = TypeContainer.FindMembers (
					current_type, MemberTypes.Method,
					BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.Instance | BindingFlags.DeclaredOnly,
					virtual_method_filter, null);

				if (current_type == TypeManager.object_type)
					searching = false;
				else {
					current_type = current_type.BaseType;
					if (!current_type.IsAbstract)
						searching = false;
				}

				if (mi.Count == 0)
					continue;

				if (mi.Count == 1 && !(mi [0] is MethodBase))
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

		PendingImplementation (TypeContainer container, MissingInterfacesInfo [] missing_ifaces, ArrayList abstract_methods, int total)
		{
			TypeBuilder type_builder = container.TypeBuilder;
			
			this.container = container;
			pending_implementations = new TypeAndMethods [total];

			int i = 0;
			if (abstract_methods != null) {
				int count = abstract_methods.Count;
				pending_implementations [i].methods = new MethodInfo [count];
				pending_implementations [i].need_proxy = new MethodInfo [count];
				
				abstract_methods.CopyTo (pending_implementations [i].methods, 0);
				pending_implementations [i].found = new bool [count];
				pending_implementations [i].args = new Type [count][];
				pending_implementations [i].mods = new Parameter.Modifier [count][];
				pending_implementations [i].type = type_builder;

				string indexer_name = TypeManager.IndexerPropertyName (type_builder);
				pending_implementations [i].set_indexer_name = "set_" + indexer_name;
				pending_implementations [i].get_indexer_name = "get_" + indexer_name;
				
				int j = 0;
				foreach (MemberInfo m in abstract_methods) {
					MethodInfo mi = (MethodInfo) m;
					
					ParameterData pd = TypeManager.GetParameterData (mi);
					Type [] types = pd.Types;
					
					pending_implementations [i].args [j] = types;
					pending_implementations [i].mods [j] = null;
					if (pd.Count > 0) {
						Parameter.Modifier [] pm = new Parameter.Modifier [pd.Count];
						for (int k = 0; k < pd.Count; k++)
							pm [k] = pd.ParameterModifier (k);
						pending_implementations [i].mods [j] = pm;
					}
						
					j++;
				}
				++i;
			}

			foreach (MissingInterfacesInfo missing in missing_ifaces) {
				MethodInfo [] mi;
				Type t = missing.Type;
				
				if (!t.IsInterface)
					continue;

				if (t is TypeBuilder){
					TypeContainer iface;

					iface = TypeManager.LookupInterface (t);
					
					mi = iface.GetMethods ();
				} else 
					mi = t.GetMethods ();
				
				int count = mi.Length;
				pending_implementations [i].type = t;
				pending_implementations [i].optional = missing.Optional;
				pending_implementations [i].methods = mi;
				pending_implementations [i].args = new Type [count][];
				pending_implementations [i].mods = new Parameter.Modifier [count][];
				pending_implementations [i].found = new bool [count];
				pending_implementations [i].need_proxy = new MethodInfo [count];
				string indexer_name = TypeManager.IndexerPropertyName (t);

				pending_implementations [i].set_indexer_name = "set_" + indexer_name;
				pending_implementations [i].get_indexer_name = "get_" + indexer_name;
				
				int j = 0;
				foreach (MethodInfo m in mi){
  					pending_implementations [i].args [j] = Type.EmptyTypes;
					pending_implementations [i].mods [j] = null;

					// If there is a previous error, just ignore
					if (m == null)
						continue;

 					ParameterData pd = TypeManager.GetParameterData (m);
					pending_implementations [i].args [j] = pd.Types;
 					
 					if (pd.Count > 0){
 						Parameter.Modifier [] pm = new Parameter.Modifier [pd.Count];
 						for (int k = 0; k < pd.Count; k++)
 							pm [k] = pd.ParameterModifier (k);
 						pending_implementations [i].mods [j] = pm;
 					}
			
					j++;
				}
				i++;
			}
		}

		struct MissingInterfacesInfo {
			public Type Type;
			public bool Optional;

			public MissingInterfacesInfo (Type t)
			{
				Type = t;
				Optional = false;
			}
		}

		static MissingInterfacesInfo [] EmptyMissingInterfacesInfo = new MissingInterfacesInfo [0];
		
		static MissingInterfacesInfo [] GetMissingInterfaces (TypeBuilder type_builder)
		{
			//
			// Notice that TypeBuilders will only return the interfaces that the Type
			// is supposed to implement, not all the interfaces that the type implements.
			//
			// Even better -- on MS it returns an empty array, no matter what.
			//
			// Completely broken.  So we do it ourselves!
			//
			Type [] impl = TypeManager.GetExplicitInterfaces (type_builder);

			if (impl == null || impl.Length == 0)
				return EmptyMissingInterfacesInfo;

			MissingInterfacesInfo [] ret = new MissingInterfacesInfo [impl.Length];

			for (int i = 0; i < impl.Length; i++)
				ret [i] = new MissingInterfacesInfo (impl [i]);

			// we really should not get here because Object doesnt implement any
			// interfaces. But it could implement something internal, so we have
			// to handle that case.
			if (type_builder.BaseType == null)
				return ret;
			
			Type [] base_impls = TypeManager.GetInterfaces (type_builder.BaseType);
			
			foreach (Type t in base_impls) {
				for (int i = 0; i < ret.Length; i ++) {
					if (t == ret [i].Type) {
						ret [i].Optional = true;
						break;
					}
				}
			}
			return ret;
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
			MissingInterfacesInfo [] missing_interfaces;
			Type b = type_builder.BaseType;

			missing_interfaces = GetMissingInterfaces (type_builder);

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
			
			int total = missing_interfaces.Length +  (implementing_abstract ? 1 : 0);
			if (total == 0)
				return null;

			return new PendingImplementation (container, missing_interfaces, abstract_methods, total);
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
		public MethodInfo IsInterfaceMethod (Type t, string name, Type ret_type, ParameterData args)
		{
			return InterfaceMethod (t, name, ret_type, args, Operation.Lookup, null);
		}

		public MethodInfo IsInterfaceIndexer (Type t, Type ret_type, ParameterData args)
		{
			return InterfaceMethod (t, null, ret_type, args, Operation.Lookup, null);
		}

		public void ImplementMethod (Type t, string name, Type ret_type, ParameterData args, bool clear_one) 
		{
			InterfaceMethod (t, name, ret_type, args,
					 clear_one ? Operation.ClearOne : Operation.ClearAll, null);
		}

		public void ImplementIndexer (Type t, MethodInfo mi, Type ret_type, ParameterData args, bool clear_one) 
		{
			InterfaceMethod (t, null, ret_type, args,
					 clear_one ? Operation.ClearOne : Operation.ClearAll, mi);
		}
		
		/// <remarks>
		///   If a method in Type `t' (or null to look in all interfaces
		///   and the base abstract class) with name `Name', return type `ret_type' and
		///   arguments `args' implements an interface, this method will
		///   return the MethodInfo that this method implements.
		///
		///   If `name' is null, we operate solely on the method's signature.  This is for
		///   instance used when implementing indexers.
		///
		///   The `Operation op' controls whether to lookup, clear the pending bit, or clear
		///   all the methods with the given signature.
		///
		///   The `MethodInfo need_proxy' is used when we're implementing an interface's
		///   indexer in a class.  If the new indexer's IndexerName does not match the one
		///   that was used in the interface, then we always need to create a proxy for it.
		///
		/// </remarks>
		public MethodInfo InterfaceMethod (Type t, string name, Type ret_type, ParameterData args,
						   Operation op, MethodInfo need_proxy)
		{
			int arg_len = args.Count;

			if (pending_implementations == null)
				return null;

			foreach (TypeAndMethods tm in pending_implementations){
				if (!(t == null || tm.type == t))
					continue;

				int method_count = tm.methods.Length;
				MethodInfo m;
				for (int i = 0; i < method_count; i++){
					m = tm.methods [i];

					if (m == null)
						continue;

					string mname = TypeManager.GetMethodName (m);

					//
					// `need_proxy' is not null when we're implementing an
					// interface indexer and this is Clear(One/All) operation.
					//
					// If `name' is null, then we do a match solely based on the
					// signature and not on the name (this is done in the Lookup
					// for an interface indexer).
					//
					if (name == null){
						if (mname != tm.get_indexer_name && mname != tm.set_indexer_name)
							continue;
					} else if ((need_proxy == null) && (name != mname))
						continue;

					Type rt = TypeManager.TypeToCoreType (m.ReturnType);
					if (!TypeManager.IsEqual (ret_type, rt) &&
					    !(ret_type == null && rt == TypeManager.void_type) &&
					    !(rt == null && ret_type == TypeManager.void_type))
						continue;

					//
					// Check if we have the same parameters
					//

					if (tm.args [i] == null && arg_len != 0)
						continue;
					if (tm.args [i] != null && tm.args [i].Length != arg_len)
						continue;

					int j;

					for (j = 0; j < arg_len; j++) {
						if (!TypeManager.IsEqual (tm.args [i][j], args.ParameterType (j)))
							break;
						if (tm.mods [i][j] == args.ParameterModifier (j))
							continue;
						// The modifiers are different, but if one of them
						// is a PARAMS modifier, and the other isn't, ignore
						// the difference.
						if (tm.mods [i][j] != Parameter.Modifier.PARAMS &&
						    args.ParameterModifier (j) != Parameter.Modifier.PARAMS)
							break;
					}
					if (j != arg_len)
						continue;

					if (op != Operation.Lookup){
						// If `t != null', then this is an explicitly interface
						// implementation and we can always clear the method.
						// `need_proxy' is not null if we're implementing an
						// interface indexer.  In this case, we need to create
						// a proxy if the implementation's IndexerName doesn't
						// match the IndexerName in the interface.
						bool name_matches = false;
						if (name == mname || mname == tm.get_indexer_name || mname == tm.set_indexer_name)
							name_matches = true;

						if ((t == null) && (need_proxy != null) && !name_matches)
							tm.need_proxy [i] = need_proxy;
						else 
							tm.methods [i] = null;
					}
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
		void DefineProxy (Type iface, MethodInfo base_method, MethodInfo iface_method,
				  Type [] args)
		{
			MethodBuilder proxy;

			string proxy_name = SimpleName.RemoveGenericArity (iface.Name) + '.' + iface_method.Name;

			proxy = container.TypeBuilder.DefineMethod (
				proxy_name,
				MethodAttributes.HideBySig |
				MethodAttributes.NewSlot |
				MethodAttributes.Virtual,
				CallingConventions.Standard | CallingConventions.HasThis,
				base_method.ReturnType, args);

			ParameterData pd = TypeManager.GetParameterData (iface_method);
			proxy.DefineParameter (0, ParameterAttributes.None, "");
			for (int i = 0; i < pd.Count; i++) {
				string name = pd.ParameterName (i);
				ParameterAttributes attr = Parameter.GetParameterAttributes (pd.ParameterModifier (i));
				proxy.DefineParameter (i + 1, attr, name);
			}

			int top = args.Length;
			ILGenerator ig = proxy.GetILGenerator ();

			for (int i = 0; i <= top; i++)
				ParameterReference.EmitLdArg (ig, i);

			ig.Emit (OpCodes.Call, base_method);
			ig.Emit (OpCodes.Ret);

			container.TypeBuilder.DefineMethodOverride (proxy, iface_method);
		}
		
		/// <summary>
		///   This function tells whether one of our base classes implements
		///   the given method (which turns out, it is valid to have an interface
		///   implementation in a base
		/// </summary>
		bool BaseImplements (Type iface_type, MethodInfo mi)
		{
			MethodSignature ms;
			
			Type [] args = TypeManager.GetParameterData (mi).Types;
			ms = new MethodSignature (mi.Name, TypeManager.TypeToCoreType (mi.ReturnType), args);
			MemberList list = TypeContainer.FindMembers (
				container.TypeBuilder.BaseType, MemberTypes.Method | MemberTypes.Property,
				BindingFlags.Public | BindingFlags.Instance,
				MethodSignature.method_signature_filter, ms);

			if (list.Count == 0)
				return false;

			if (TypeManager.ImplementsInterface (container.TypeBuilder.BaseType, iface_type))
				return true;

			MethodInfo base_method = (MethodInfo) list [0];

			if (base_method.DeclaringType.IsInterface)
				return false;

			if (!base_method.IsAbstract && !base_method.IsVirtual)
				// FIXME: We can avoid creating a proxy if base_method can be marked 'final virtual' instead.
				//        However, it's too late now, the MethodBuilder has already been created (see bug 377519)
				DefineProxy (iface_type, base_method, mi, args);

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

				bool base_implements_type = type.IsInterface &&
					container.TypeBuilder.BaseType != null &&
					TypeManager.ImplementsInterface (container.TypeBuilder.BaseType, type);

				foreach (MethodInfo mi in pending_implementations [i].methods){
					if (mi == null)
						continue;

					if (type.IsInterface){
						MethodInfo need_proxy =
							pending_implementations [i].need_proxy [j];

						if (need_proxy != null) {
							Type [] args = TypeManager.GetParameterData (mi).Types;
							DefineProxy (type, need_proxy, mi, args);
							continue;
						}

						if (base_implements_type || BaseImplements (type, mi))
							continue;
						
						if (pending_implementations [i].optional)
							continue;

						Report.SymbolRelatedToPreviousError (mi);
						if (pending_implementations [i].found [j]) {
							if (mi.IsSpecialName) {
								string name = TypeManager.CSharpName (mi.DeclaringType) + '.' + mi.Name.Substring (4);
								Report.Error (551, container.Location, "Explicit interface implementation `{0}.{1}' is missing accessor `{2}'",
									container.GetSignatureForError (), name, TypeManager.CSharpSignature (mi, true));
							} else {
								string[] methodLabel = TypeManager.CSharpSignature (mi).Split ('.');
								Report.Error (536, container.Location,
									"`{0}' does not implement interface member `{1}'. `{2}.{3}' " +
									"is either static, not public, or has the wrong return type",
									container.Name, TypeManager.CSharpSignature (mi),
									container.Name, methodLabel[methodLabel.Length - 1]);
							}
						}
						else {
							Report.Error (535, container.Location, "`{0}' does not implement interface member `{1}'",
								container.GetSignatureForError (), TypeManager.CSharpSignature (mi));
						}
					} else {
						Report.Error (534, container.Location, "`{0}' does not implement inherited abstract member `{1}'",
							container.GetSignatureForError (), TypeManager.CSharpSignature (mi, true));
					}
					errors = true;
					j++;
				}
			}
			return errors;
		}
	} /* end of class */
}
