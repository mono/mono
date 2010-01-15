//
// membercache.cs: A container for all member lookups
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2010 Novell, Inc
//
//

using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using System.Reflection;

namespace Mono.CSharp {

	[Flags]
	public enum MemberKind
	{
		Constructor = 1,
		Event = 1 << 1,
		Field = 1 << 2,
		Method = 1 << 3,
		Property = 1 << 4,
		Indexer = 1 << 5,
		Operator = 1 << 6,
		Destructor	= 1 << 7,
		//Constant = 1 << 8,

		NestedType	= 1 << 10,

		Class		= 1 << 11,
		Struct		= 1 << 12,
		Delegate	= 1 << 13,
		Enum		= 1 << 14,
		Interface	= 1 << 15,

		MaskType = Constructor | Event | Field | Method | Property | NestedType | Indexer | Operator | Destructor,
		All = MaskType
	}

	[Flags]
	public enum BindingRestriction
	{
		None = 0,

		// Member has to be accessible
		AccessibleOnly = 1,

		// Inspect only queried type members
		DeclaredOnly = 1 << 1,

		// Excluded static
		InstanceOnly = 1 << 2,

		// 
		NoOverloadableOverrides	= 1 << 3
	}
/*
	public struct MemberFilter : IEquatable<MemberCore>
	{
		public readonly string Name;
		public readonly MemberKind Kind;
		public readonly TypeSpec[] Parameters;
		public readonly TypeSpec MemberType;

		public MemberFilter (IMethod m)
		{
			Name = m.MethodBuilder.Name;
			Kind = MemberKind.Method;
			Parameters = m.Parameters.Types;
			MemberType = m.ReturnType;
		}

		public MemberFilter (string name, MemberKind kind)
		{
			Name = name;
			Kind = kind;
			Parameters = null;
			MemberType = null;
		}

		public MemberFilter (string name, MemberKind kind, TypeSpec[] param, TypeSpec type)
			: this (name, kind)
		{
			Name = name;
			Kind = kind;
			Parameters = param;
			MemberType = type;
		}

		public static MemberFilter Constuctor (TypeSpec[] param)
		{
			return new MemberFilter (System.Reflection.ConstructorInfo.ConstructorName, MemberKind.Constructor, param, null);
		}

		public static MemberFilter Property (string name, TypeSpec type)
		{
			return new MemberFilter (name, MemberKind.Property, null, type);
		}

		public static MemberFilter Field (string name, TypeSpec type)
		{
			return new MemberFilter (name, MemberKind.Field, null, type);
		}

		public static MemberFilter Method (string name, TypeSpec[] param, TypeSpec type)
		{
			return new MemberFilter (name, MemberKind.Method, param, type);
		}

		#region IEquatable<MemberCore> Members

		public bool Equals (MemberCore other)
		{
			// Is the member of the correct type ?
			if ((other.MemberKind & Kind & MemberKind.MaskType) == 0)
				return false;

			if (Parameters != null) {
				if (other is IParametersMember) {
					AParametersCollection other_param = ((IParametersMember) other).Parameters;
					if (TypeSpecArrayComparer.Default.Equals (Parameters, other_param.Types))
						return true;
				}

				return false;
			}

			if (MemberType != null) {
				//throw new NotImplementedException ();
			}

			return true;
		}

		#endregion
	}
*/ 
	/// <summary>
	///   This is a readonly list of MemberInfo's.      
	/// </summary>
	public class MemberList : IList<MemberInfo> {
		public readonly IList<MemberInfo> List;
		int count;

		/// <summary>
		///   Create a new MemberList from the given IList.
		/// </summary>
		public MemberList (IList<MemberInfo> list)
		{
			if (list != null)
				this.List = list;
			else
				this.List = new List<MemberInfo> ();
			count = List.Count;
		}

		/// <summary>
		///   Concatenate the ILists `first' and `second' to a new MemberList.
		/// </summary>
		public MemberList (IList<MemberInfo> first, IList<MemberInfo> second)
		{
			var list = new List<MemberInfo> ();
			list.AddRange (first);
			list.AddRange (second);
			count = list.Count;
			List = list;
		}

		public static readonly MemberList Empty = new MemberList (Array.AsReadOnly (new MemberInfo[0]));

		/// <summary>
		///   Cast the MemberList into a MemberInfo[] array.
		/// </summary>
		/// <remarks>
		///   This is an expensive operation, only use it if it's really necessary.
		/// </remarks>
		public static explicit operator MemberInfo [] (MemberList list)
		{
			Timer.StartTimer (TimerType.MiscTimer);
			MemberInfo [] result = new MemberInfo [list.Count];
			list.CopyTo (result, 0);
			Timer.StopTimer (TimerType.MiscTimer);
			return result;
		}

		// ICollection

		public int Count {
			get {
				return count;
			}
		}

		public void CopyTo (MemberInfo[] array, int index)
		{
			List.CopyTo (array, index);
		}

		// IEnumerable

		public IEnumerator<MemberInfo> GetEnumerator ()
		{
			return List.GetEnumerator ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return List.GetEnumerator ();
		}

		// IList

		public bool IsFixedSize {
			get {
				return true;
			}
		}

		public bool IsReadOnly {
			get {
				return true;
			}
		}

		MemberInfo IList<MemberInfo>.this [int index] {
			get {
				return List [index];
			}

			set {
				throw new NotSupportedException ();
			}
		}

		// FIXME: try to find out whether we can avoid the cast in this indexer.
		public MemberInfo this [int index] {
			get {
				return (MemberInfo) List [index];
			}
		}

		public void Add (MemberInfo value)
		{
			throw new NotSupportedException ();
		}

		public void Clear ()
		{
			throw new NotSupportedException ();
		}

		public bool Contains (MemberInfo value)
		{
			return List.Contains (value);
		}

		public int IndexOf (MemberInfo value)
		{
			return List.IndexOf (value);
		}

		public void Insert (int index, MemberInfo value)
		{
			throw new NotSupportedException ();
		}

		public bool Remove (MemberInfo value)
		{
			throw new NotSupportedException ();
		}

		public void RemoveAt (int index)
		{
			throw new NotSupportedException ();
		}
	}

	/// <summary>
	///   This interface is used to get all members of a class when creating the
	///   member cache.  It must be implemented by all DeclSpace derivatives which
	///   want to support the member cache and by TypeHandle to get caching of
	///   non-dynamic types.
	/// </summary>
	public interface IMemberContainer {
		/// <summary>
		///   The name of the IMemberContainer.  This is only used for
		///   debugging purposes.
		/// </summary>
		string Name {
			get;
		}

		/// <summary>
		///   The type of this IMemberContainer.
		/// </summary>
		Type Type {
			get;
		}

		/// <summary>
		///   Returns the IMemberContainer of the base class or null if this
		///   is an interface or TypeManger.object_type.
		///   This is used when creating the member cache for a class to get all
		///   members from the base class.
		/// </summary>
		MemberCache BaseCache {
			get;
		}

		/// <summary>
		///   Whether this is an interface.
		/// </summary>
		bool IsInterface {
			get;
		}

		/// <summary>
		///   Returns all members of this class with the corresponding MemberTypes
		///   and BindingFlags.
		/// </summary>
		/// <remarks>
		///   When implementing this method, make sure not to return any inherited
		///   members and check the MemberTypes and BindingFlags properly.
		///   Unfortunately, System.Reflection is lame and doesn't provide a way to
		///   get the BindingFlags (static/non-static,public/non-public) in the
		///   MemberInfo class, but the cache needs this information.  That's why
		///   this method is called multiple times with different BindingFlags.
		/// </remarks>
		MemberList GetMembers (MemberTypes mt, BindingFlags bf);
	}

	/// <summary>
	///   The MemberCache is used by dynamic and non-dynamic types to speed up
	///   member lookups.  It has a member name based hash table; it maps each member
	///   name to a list of CacheEntry objects.  Each CacheEntry contains a MemberInfo
	///   and the BindingFlags that were initially used to get it.  The cache contains
	///   all members of the current class and all inherited members.  If this cache is
	///   for an interface types, it also contains all inherited members.
	///
	///   There are two ways to get a MemberCache:
	///   * if this is a dynamic type, lookup the corresponding DeclSpace and then
	///     use the DeclSpace.MemberCache property.
	///   * if this not a dynamic type, call TypeHandle.GetTypeHandle() to get a
	///     TypeHandle instance for the type and then use TypeHandle.MemberCache.
	/// </summary>
	public class MemberCache {
		public readonly IMemberContainer Container;
		protected Dictionary<string, List<CacheEntry>> member_hash;
		protected Dictionary<string, List<CacheEntry>> method_hash;

		Dictionary<string, object> locase_table;

		static List<MethodInfo> overrides = new List<MethodInfo> ();

		/// <summary>
		///   Create a new MemberCache for the given IMemberContainer `container'.
		/// </summary>
		public MemberCache (IMemberContainer container)
		{
			this.Container = container;

			Timer.IncrementCounter (CounterType.MemberCache);
			Timer.StartTimer (TimerType.CacheInit);

			// If we have a base class (we have a base class unless we're
			// TypeManager.object_type), we deep-copy its MemberCache here.
			if (Container.BaseCache != null)
				member_hash = SetupCache (Container.BaseCache);
			else
				member_hash = new Dictionary<string, List<CacheEntry>> ();

			// If this is neither a dynamic type nor an interface, create a special
			// method cache with all declared and inherited methods.
			Type type = container.Type;
			if (!(type is TypeBuilder) && !type.IsInterface &&
			    // !(type.IsGenericType && (type.GetGenericTypeDefinition () is TypeBuilder)) &&
			    !TypeManager.IsGenericType (type) && !TypeManager.IsGenericParameter (type) &&
			    (Container.BaseCache == null || Container.BaseCache.method_hash != null)) {
					method_hash = new Dictionary<string, List<CacheEntry>> ();
					AddMethods (type);
			}

			// Add all members from the current class.
			AddMembers (Container);

			Timer.StopTimer (TimerType.CacheInit);
		}

		public MemberCache (Type baseType, IMemberContainer container)
		{
			this.Container = container;
			if (baseType == null)
				this.member_hash = new Dictionary<string, List<CacheEntry>> ();
			else
				this.member_hash = SetupCache (TypeManager.LookupMemberCache (baseType));
		}

		public MemberCache (Type[] ifaces)
		{
			//
			// The members of this cache all belong to other caches.  
			// So, 'Container' will not be used.
			//
			this.Container = null;

			member_hash = new Dictionary<string, List<CacheEntry>> ();
			if (ifaces == null)
				return;

			foreach (Type itype in ifaces)
				AddCacheContents (TypeManager.LookupMemberCache (itype));
		}

		public MemberCache (IMemberContainer container, Type base_class, Type[] ifaces)
		{
			this.Container = container;

			// If we have a base class (we have a base class unless we're
			// TypeManager.object_type), we deep-copy its MemberCache here.
			if (Container.BaseCache != null)
				member_hash = SetupCache (Container.BaseCache);
			else
				member_hash = new Dictionary<string, List<CacheEntry>> ();

			if (base_class != null)
				AddCacheContents (TypeManager.LookupMemberCache (base_class));
			if (ifaces != null) {
				foreach (Type itype in ifaces) {
					MemberCache cache = TypeManager.LookupMemberCache (itype);
					if (cache != null)
						AddCacheContents (cache);
				}
			}
		}

		/// <summary>
		///   Bootstrap this member cache by doing a deep-copy of our base.
		/// </summary>
		static Dictionary<string, List<CacheEntry>> SetupCache (MemberCache base_class)
		{
			if (base_class == null)
				return new Dictionary<string, List<CacheEntry>> ();

			var hash = new Dictionary<string, List<CacheEntry>> (base_class.member_hash.Count);
			var it = base_class.member_hash.GetEnumerator ();
			while (it.MoveNext ()) {
				hash.Add (it.Current.Key, new List<CacheEntry> (it.Current.Value));
			}
                                
			return hash;
		}
		
		//
		// Converts ModFlags to BindingFlags
		//
		static BindingFlags GetBindingFlags (Modifiers modifiers)
		{
			BindingFlags bf;
			if ((modifiers & Modifiers.STATIC) != 0)
				bf = BindingFlags.Static;
			else
				bf = BindingFlags.Instance;

			if ((modifiers & Modifiers.PRIVATE) != 0)
				bf |= BindingFlags.NonPublic;
			else
				bf |= BindingFlags.Public;

			return bf;
		}		

		/// <summary>
		///   Add the contents of `cache' to the member_hash.
		/// </summary>
		void AddCacheContents (MemberCache cache)
		{
			var it = cache.member_hash.GetEnumerator ();
			while (it.MoveNext ()) {
				List<CacheEntry> list;
				if (!member_hash.TryGetValue (it.Current.Key, out list))
					member_hash [it.Current.Key] = list = new List<CacheEntry> ();

				var entries = it.Current.Value;
				for (int i = entries.Count-1; i >= 0; i--) {
					var entry = entries [i];

					if (entry.Container != cache.Container)
						break;
					list.Add (entry);
				}
			}
		}

		/// <summary>
		///   Add all members from class `container' to the cache.
		/// </summary>
		void AddMembers (IMemberContainer container)
		{
			// We need to call AddMembers() with a single member type at a time
			// to get the member type part of CacheEntry.EntryType right.
			if (!container.IsInterface) {
				AddMembers (MemberTypes.Constructor, container);
				AddMembers (MemberTypes.Field, container);
			}
			AddMembers (MemberTypes.Method, container);
			AddMembers (MemberTypes.Property, container);
			AddMembers (MemberTypes.Event, container);
			// Nested types are returned by both Static and Instance searches.
			AddMembers (MemberTypes.NestedType,
				    BindingFlags.Static | BindingFlags.Public, container);
			AddMembers (MemberTypes.NestedType,
				    BindingFlags.Static | BindingFlags.NonPublic, container);
		}

		void AddMembers (MemberTypes mt, IMemberContainer container)
		{
			AddMembers (mt, BindingFlags.Static | BindingFlags.Public, container);
			AddMembers (mt, BindingFlags.Static | BindingFlags.NonPublic, container);
			AddMembers (mt, BindingFlags.Instance | BindingFlags.Public, container);
			AddMembers (mt, BindingFlags.Instance | BindingFlags.NonPublic, container);
		}

		public void AddMember (MemberInfo mi, MemberSpec mc)
		{
			AddMember (mi.MemberType, GetBindingFlags (mc.Modifiers), Container, mi.Name, mi);
		}

		public void AddGenericMember (MemberInfo mi, InterfaceMemberBase mc)
		{
			AddMember (mi.MemberType, GetBindingFlags (mc.ModFlags), Container,
				MemberName.MakeName (mc.GetFullName (mc.MemberName), mc.MemberName.TypeArguments), mi);
		}

		public void AddNestedType (DeclSpace type)
		{
			AddMember (MemberTypes.NestedType, GetBindingFlags (type.ModFlags), (IMemberContainer) type.Parent,
				type.TypeBuilder.Name, type.TypeBuilder);
		}

		public void AddInterface (MemberCache baseCache)
		{
			if (baseCache.member_hash.Count > 0)
				AddCacheContents (baseCache);
		}

		void AddMember (MemberTypes mt, BindingFlags bf, IMemberContainer container,
				string name, MemberInfo member)
		{
			// We use a name-based hash table of ArrayList's.
			List<CacheEntry> list;
			if (!member_hash.TryGetValue (name, out list)) {
				list = new List<CacheEntry> (1);
				member_hash.Add (name, list);
			}

			// When this method is called for the current class, the list will
			// already contain all inherited members from our base classes.
			// We cannot add new members in front of the list since this'd be an
			// expensive operation, that's why the list is sorted in reverse order
			// (ie. members from the current class are coming last).
			list.Add (new CacheEntry (container, member, mt, bf));
		}

		/// <summary>
		///   Add all members from class `container' with the requested MemberTypes and
		///   BindingFlags to the cache.  This method is called multiple times with different
		///   MemberTypes and BindingFlags.
		/// </summary>
		void AddMembers (MemberTypes mt, BindingFlags bf, IMemberContainer container)
		{
			MemberList members = container.GetMembers (mt, bf);

			foreach (MemberInfo member in members) {
				string name = member.Name;

				AddMember (mt, bf, container, name, member);

				if (member is MethodInfo) {
					string gname = TypeManager.GetMethodName ((MethodInfo) member);
					if (gname != name)
						AddMember (mt, bf, container, gname, member);
				}
			}
		}

		/// <summary>
		///   Add all declared and inherited methods from class `type' to the method cache.
		/// </summary>
		void AddMethods (Type type)
		{
			AddMethods (BindingFlags.Static | BindingFlags.Public |
				    BindingFlags.FlattenHierarchy, type);
			AddMethods (BindingFlags.Static | BindingFlags.NonPublic |
				    BindingFlags.FlattenHierarchy, type);
			AddMethods (BindingFlags.Instance | BindingFlags.Public, type);
			AddMethods (BindingFlags.Instance | BindingFlags.NonPublic, type);
		}

		void AddMethods (BindingFlags bf, Type type)
		{
			MethodBase [] members = type.GetMethods (bf);

                        Array.Reverse (members);

			foreach (MethodBase member in members) {
				string name = member.Name;

				// We use a name-based hash table of ArrayList's.
				List<CacheEntry> list;
				if (!method_hash.TryGetValue (name, out list)) {
					list = new List<CacheEntry> (1);
					method_hash.Add (name, list);
				}

				MethodInfo curr = (MethodInfo) member;
				while (curr.IsVirtual && (curr.Attributes & MethodAttributes.NewSlot) == 0) {
					MethodInfo base_method = curr.GetBaseDefinition ();

					if (base_method == curr)
						// Not every virtual function needs to have a NewSlot flag.
						break;

					overrides.Add (curr);
					list.Add (new CacheEntry (null, base_method, MemberTypes.Method, bf));
					curr = base_method;
				}

				if (overrides.Count > 0) {
					for (int i = 0; i < overrides.Count; ++i)
						TypeManager.RegisterOverride ((MethodBase) overrides [i], curr);
					overrides.Clear ();
				}

				// Unfortunately, the elements returned by Type.GetMethods() aren't
				// sorted so we need to do this check for every member.
				BindingFlags new_bf = bf;
				if (member.DeclaringType == type)
					new_bf |= BindingFlags.DeclaredOnly;

				list.Add (new CacheEntry (Container, member, MemberTypes.Method, new_bf));
			}
		}

		/// <summary>
		///   Compute and return a appropriate `EntryType' magic number for the given
		///   MemberTypes and BindingFlags.
		/// </summary>
		protected static EntryType GetEntryType (MemberTypes mt, BindingFlags bf)
		{
			EntryType type = EntryType.None;

			if ((mt & MemberTypes.Constructor) != 0)
				type |= EntryType.Constructor;
			if ((mt & MemberTypes.Event) != 0)
				type |= EntryType.Event;
			if ((mt & MemberTypes.Field) != 0)
				type |= EntryType.Field;
			if ((mt & MemberTypes.Method) != 0)
				type |= EntryType.Method;
			if ((mt & MemberTypes.Property) != 0)
				type |= EntryType.Property;
			// Nested types are returned by static and instance searches.
			if ((mt & MemberTypes.NestedType) != 0)
				type |= EntryType.NestedType | EntryType.Static | EntryType.Instance;

			if ((bf & BindingFlags.Instance) != 0)
				type |= EntryType.Instance;
			if ((bf & BindingFlags.Static) != 0)
				type |= EntryType.Static;
			if ((bf & BindingFlags.Public) != 0)
				type |= EntryType.Public;
			if ((bf & BindingFlags.NonPublic) != 0)
				type |= EntryType.NonPublic;
			if ((bf & BindingFlags.DeclaredOnly) != 0)
				type |= EntryType.Declared;

			return type;
		}

		/// <summary>
		///   The `MemberTypes' enumeration type is a [Flags] type which means that it may
		///   denote multiple member types.  Returns true if the given flags value denotes a
		///   single member types.
		/// </summary>
		public static bool IsSingleMemberType (MemberTypes mt)
		{
			switch (mt) {
			case MemberTypes.Constructor:
			case MemberTypes.Event:
			case MemberTypes.Field:
			case MemberTypes.Method:
			case MemberTypes.Property:
			case MemberTypes.NestedType:
				return true;

			default:
				return false;
			}
		}

		/// <summary>
		///   We encode the MemberTypes and BindingFlags of each members in a "magic"
		///   number to speed up the searching process.
		/// </summary>
		[Flags]
		public enum EntryType {
			None		= 0x000,

			Instance	= 0x001,
			Static		= 0x002,
			MaskStatic	= Instance|Static,

			Public		= 0x004,
			NonPublic	= 0x008,
			MaskProtection	= Public|NonPublic,

			Declared	= 0x010,

			Constructor	= 0x020,
			Event		= 0x040,
			Field		= 0x080,
			Method		= 0x100,
			Property	= 0x200,
			NestedType	= 0x400,

			NotExtensionMethod	= 0x800,

			MaskType	= Constructor|Event|Field|Method|Property|NestedType
		}

		public class CacheEntry {
			public readonly IMemberContainer Container;
			public EntryType EntryType;
			public readonly MemberInfo Member;

			public CacheEntry (IMemberContainer container, MemberInfo member,
					   MemberTypes mt, BindingFlags bf)
			{
				this.Container = container;
				this.Member = member;
				this.EntryType = GetEntryType (mt, bf);
			}

			public override string ToString ()
			{
				return String.Format ("CacheEntry ({0}:{1}:{2})", Container.Name,
						      EntryType, Member);
			}
		}

		/// <summary>
		///   This is called each time we're walking up one level in the class hierarchy
		///   and checks whether we can abort the search since we've already found what
		///   we were looking for.
		/// </summary>
		protected bool DoneSearching (IList<MemberInfo> list)
		{
			//
			// We've found exactly one member in the current class and it's not
			// a method or constructor.
			//
			if (list.Count == 1 && !(list [0] is MethodBase))
				return true;

			//
			// Multiple properties: we query those just to find out the indexer
			// name
			//
			if ((list.Count > 0) && (list [0] is PropertyInfo))
				return true;

			return false;
		}

		/// <summary>
		///   Looks up members with name `name'.  If you provide an optional
		///   filter function, it'll only be called with members matching the
		///   requested member name.
		///
		///   This method will try to use the cache to do the lookup if possible.
		///
		///   Unlike other FindMembers implementations, this method will always
		///   check all inherited members - even when called on an interface type.
		///
		///   If you know that you're only looking for methods, you should use
		///   MemberTypes.Method alone since this speeds up the lookup a bit.
		///   When doing a method-only search, it'll try to use a special method
		///   cache (unless it's a dynamic type or an interface) and the returned
		///   MemberInfo's will have the correct ReflectedType for inherited methods.
		///   The lookup process will automatically restart itself in method-only
		///   search mode if it discovers that it's about to return methods.
		/// </summary>
		List<MemberInfo> global = new List<MemberInfo> ();
		bool using_global;
		
		static MemberInfo [] emptyMemberInfo = new MemberInfo [0];
		
		public MemberInfo [] FindMembers (MemberTypes mt, BindingFlags bf, string name,
						  MemberFilter filter, object criteria)
		{
			if (using_global)
				throw new Exception ();

			bool declared_only = (bf & BindingFlags.DeclaredOnly) != 0;
			bool method_search = mt == MemberTypes.Method;
			// If we have a method cache and we aren't already doing a method-only search,
			// then we restart a method search if the first match is a method.
			bool do_method_search = !method_search && (method_hash != null);

			List<CacheEntry> applicable;

			// If this is a method-only search, we try to use the method cache if
			// possible; a lookup in the method cache will return a MemberInfo with
			// the correct ReflectedType for inherited methods.
			
			if (method_search && (method_hash != null))
				method_hash.TryGetValue (name, out applicable);
			else
				member_hash.TryGetValue (name, out applicable);

			if (applicable == null)
				return emptyMemberInfo;

			//
			// 32  slots gives 53 rss/54 size
			// 2/4 slots gives 55 rss
			//
			// Strange: from 25,000 calls, only 1,800
			// are above 2.  Why does this impact it?
			//
			global.Clear ();
			using_global = true;

			Timer.StartTimer (TimerType.CachedLookup);

			EntryType type = GetEntryType (mt, bf);

			IMemberContainer current = Container;

			bool do_interface_search = current.IsInterface;

			// `applicable' is a list of all members with the given member name `name'
			// in the current class and all its base classes.  The list is sorted in
			// reverse order due to the way how the cache is initialy created (to speed
			// things up, we're doing a deep-copy of our base).

			for (int i = applicable.Count-1; i >= 0; i--) {
				CacheEntry entry = (CacheEntry) applicable [i];

				// This happens each time we're walking one level up in the class
				// hierarchy.  If we're doing a DeclaredOnly search, we must abort
				// the first time this happens (this may already happen in the first
				// iteration of this loop if there are no members with the name we're
				// looking for in the current class).
				if (entry.Container != current) {
					if (declared_only)
						break;

					if (!do_interface_search && DoneSearching (global))
						break;

					current = entry.Container;
				}

				// Is the member of the correct type ?
				if ((entry.EntryType & type & EntryType.MaskType) == 0)
					continue;

				// Is the member static/non-static ?
				if ((entry.EntryType & type & EntryType.MaskStatic) == 0)
					continue;

				// Apply the filter to it.
				if (filter (entry.Member, criteria)) {
					if ((entry.EntryType & EntryType.MaskType) != EntryType.Method) {
						do_method_search = false;
					}
					
					// Because interfaces support multiple inheritance we have to be sure that
					// base member is from same interface, so only top level member will be returned
					if (do_interface_search && global.Count > 0) {
						bool member_already_exists = false;

						foreach (MemberInfo mi in global) {
							if (mi is MethodBase)
								continue;

							if (IsInterfaceBaseInterface (TypeManager.GetInterfaces (mi.DeclaringType), entry.Member.DeclaringType)) {
								member_already_exists = true;
								break;
							}
						}
						if (member_already_exists)
							continue;
					}

					global.Add (entry.Member);
				}
			}

			Timer.StopTimer (TimerType.CachedLookup);

			// If we have a method cache and we aren't already doing a method-only
			// search, we restart in method-only search mode if the first match is
			// a method.  This ensures that we return a MemberInfo with the correct
			// ReflectedType for inherited methods.
			if (do_method_search && (global.Count > 0)){
				using_global = false;

				return FindMembers (MemberTypes.Method, bf, name, filter, criteria);
			}

			using_global = false;
			MemberInfo [] copy = new MemberInfo [global.Count];
			global.CopyTo (copy);
			return copy;
		}

		/// <summary>
		/// Returns true if iterface exists in any base interfaces (ifaces)
		/// </summary>
		static bool IsInterfaceBaseInterface (Type[] ifaces, Type ifaceToFind)
		{
			foreach (Type iface in ifaces) {
				if (iface == ifaceToFind)
					return true;

				Type[] base_ifaces = TypeManager.GetInterfaces (iface);
				if (base_ifaces.Length > 0 && IsInterfaceBaseInterface (base_ifaces, ifaceToFind))
					return true;
			}
			return false;
		}
		
		// find the nested type @name in @this.
		public Type FindNestedType (string name)
		{
			List<CacheEntry> applicable;
			if (!member_hash.TryGetValue (name, out applicable))
				return null;
			
			for (int i = applicable.Count-1; i >= 0; i--) {
				CacheEntry entry = applicable [i];
				if ((entry.EntryType & EntryType.NestedType & EntryType.MaskType) != 0)
					return (Type) entry.Member;
			}
			
			return null;
		}

		public MemberInfo FindBaseEvent (Type invocation_type, string name)
		{
			List<CacheEntry> applicable;
			if (!member_hash.TryGetValue (name, out applicable))
				return null;

			//
			// Walk the chain of events, starting from the top.
			//
			for (int i = applicable.Count - 1; i >= 0; i--) 
			{
				CacheEntry entry = applicable [i];
				if ((entry.EntryType & EntryType.Event) == 0)
					continue;
				
				EventInfo ei = (EventInfo)entry.Member;
				return ei.GetAddMethod (true);
			}

			return null;
		}

		//
		// Looks for extension methods with defined name and extension type
		//
		public List<MethodSpec> FindExtensionMethods (Assembly thisAssembly, Type extensionType, string name, bool publicOnly)
		{
			List<CacheEntry> entries;
			if (method_hash != null)
				method_hash.TryGetValue (name, out entries);
			else {
				member_hash.TryGetValue (name, out entries);
			}

			if (entries == null)
				return null;

			EntryType entry_type = EntryType.Static | EntryType.Method | EntryType.NotExtensionMethod;
			EntryType found_entry_type = entry_type & ~EntryType.NotExtensionMethod;

			List<MethodSpec> candidates = null;
			foreach (CacheEntry entry in entries) {
				if ((entry.EntryType & entry_type) == found_entry_type) {
					MethodBase mb = (MethodBase)entry.Member;

					// Simple accessibility check
					if ((entry.EntryType & EntryType.Public) == 0 && publicOnly) {
						MethodAttributes ma = mb.Attributes & MethodAttributes.MemberAccessMask;
						if (ma != MethodAttributes.Assembly && ma != MethodAttributes.FamORAssem)
							continue;
						
						if (!TypeManager.IsThisOrFriendAssembly (thisAssembly, mb.DeclaringType.Assembly))
							continue;
					}

					IMethodData md = TypeManager.GetMethod (mb);
					AParametersCollection pd = md == null ?
						TypeManager.GetParameterData (mb) : md.ParameterInfo;

					Type ex_type = pd.ExtensionMethodType;
					if (ex_type == null) {
						entry.EntryType |= EntryType.NotExtensionMethod;
						continue;
					}

					if (candidates == null)
						candidates = new List<MethodSpec> (2);
					candidates.Add (Import.CreateMethod (mb));
				}
			}

			return candidates;
		}
		
		//
		// This finds the method or property for us to override. invocation_type is the type where
		// the override is going to be declared, name is the name of the method/property, and
		// param_types is the parameters, if any to the method or property
		//
		// Because the MemberCache holds members from this class and all the base classes,
		// we can avoid tons of reflection stuff.
		//
		public MemberInfo FindMemberToOverride (Type invocation_type, string name, AParametersCollection parameters, GenericMethod generic_method, bool is_property)
		{
			List<CacheEntry> applicable;
			if (method_hash != null && !is_property)
				method_hash.TryGetValue (name, out applicable);
			else
				member_hash.TryGetValue (name, out applicable);
			
			if (applicable == null)
				return null;
			//
			// Walk the chain of methods, starting from the top.
			//
			for (int i = applicable.Count - 1; i >= 0; i--) {
				CacheEntry entry = applicable [i];
				
				if ((entry.EntryType & (is_property ? (EntryType.Property | EntryType.Field) : EntryType.Method)) == 0)
					continue;

				PropertyInfo pi = null;
				MethodInfo mi = null;
				FieldInfo fi = null;
				AParametersCollection cmp_attrs;
				
				if (is_property) {
					if ((entry.EntryType & EntryType.Field) != 0) {
						fi = (FieldInfo)entry.Member;
						cmp_attrs = ParametersCompiled.EmptyReadOnlyParameters;
					} else {
						pi = (PropertyInfo) entry.Member;
						cmp_attrs = TypeManager.GetParameterData (pi);
					}
				} else {
					mi = (MethodInfo) entry.Member;
					cmp_attrs = TypeManager.GetParameterData (mi);
				}

				if (fi != null) {
					// TODO: Almost duplicate !
					// Check visibility
					switch (fi.Attributes & FieldAttributes.FieldAccessMask) {
					case FieldAttributes.PrivateScope:
						continue;
					case FieldAttributes.Private:
						//
						// A private method is Ok if we are a nested subtype.
						// The spec actually is not very clear about this, see bug 52458.
						//
						if (!invocation_type.Equals (entry.Container.Type) &&
						    !TypeManager.IsNestedChildOf (invocation_type, entry.Container.Type))
							continue;
						break;
					case FieldAttributes.FamANDAssem:
					case FieldAttributes.Assembly:
						//
						// Check for assembly methods
						//
						if (fi.DeclaringType.Assembly != CodeGen.Assembly.Builder)
							continue;
						break;
					}
					return entry.Member;
				}

				//
				// Check the arguments
				//
				if (cmp_attrs.Count != parameters.Count)
					continue;
	
				int j;
				for (j = 0; j < cmp_attrs.Count; ++j) {
					//
					// LAMESPEC: No idea why `params' modifier is ignored
					//
					if ((parameters.FixedParameters [j].ModFlags & ~Parameter.Modifier.PARAMS) != 
						(cmp_attrs.FixedParameters [j].ModFlags & ~Parameter.Modifier.PARAMS))
						break;

					if (!TypeManager.IsEqual (parameters.Types [j], cmp_attrs.Types [j]))
						break;
				}

				if (j < cmp_attrs.Count)
					continue;

				//
				// check generic arguments for methods
				//
				if (mi != null) {
					Type [] cmpGenArgs = TypeManager.GetGenericArguments (mi);
					if (generic_method == null && cmpGenArgs != null && cmpGenArgs.Length != 0)
						continue;
					if (generic_method != null && cmpGenArgs != null && cmpGenArgs.Length != generic_method.TypeParameters.Length)
						continue;
				}

				//
				// get one of the methods because this has the visibility info.
				//
				if (is_property) {
					mi = pi.GetGetMethod (true);
					if (mi == null)
						mi = pi.GetSetMethod (true);
				}
				
				//
				// Check visibility
				//
				switch (mi.Attributes & MethodAttributes.MemberAccessMask) {
				case MethodAttributes.PrivateScope:
					continue;
				case MethodAttributes.Private:
					//
					// A private method is Ok if we are a nested subtype.
					// The spec actually is not very clear about this, see bug 52458.
					//
					if (!invocation_type.Equals (entry.Container.Type) &&
					    !TypeManager.IsNestedChildOf (invocation_type, entry.Container.Type))
						continue;
					break;
				case MethodAttributes.FamANDAssem:
				case MethodAttributes.Assembly:
					//
					// Check for assembly methods
					//
					if (!TypeManager.IsThisOrFriendAssembly (invocation_type.Assembly, mi.DeclaringType.Assembly))
						continue;
					break;
				}
				return entry.Member;
			}
			
			return null;
		}

 		/// <summary>
 		/// The method is looking for conflict with inherited symbols (errors CS0108, CS0109).
 		/// We handle two cases. The first is for types without parameters (events, field, properties).
 		/// The second are methods, indexers and this is why ignore_complex_types is here.
 		/// The latest param is temporary hack. See DoDefineMembers method for more info.
 		/// </summary>
 		public MemberInfo FindMemberWithSameName (string name, bool ignore_complex_types, MemberInfo ignore_member)
 		{
			List<CacheEntry> applicable = null;
 
 			if (method_hash != null)
				method_hash.TryGetValue (name, out applicable);
 
 			if (applicable != null) {
 				for (int i = applicable.Count - 1; i >= 0; i--) {
 					CacheEntry entry = (CacheEntry) applicable [i];
 					if ((entry.EntryType & EntryType.Public) != 0)
 						return entry.Member;
 				}
 			}
 
 			if (member_hash == null)
 				return null;

			if (member_hash.TryGetValue (name, out applicable)) {
 				for (int i = applicable.Count - 1; i >= 0; i--) {
 					CacheEntry entry = (CacheEntry) applicable [i];
 					if ((entry.EntryType & EntryType.Public) != 0 & entry.Member != ignore_member) {
 						if (ignore_complex_types) {
 							if ((entry.EntryType & EntryType.Method) != 0)
 								continue;
 
 							// Does exist easier way how to detect indexer ?
 							if ((entry.EntryType & EntryType.Property) != 0) {
 								AParametersCollection arg_types = TypeManager.GetParameterData ((PropertyInfo)entry.Member);
 								if (arg_types.Count > 0)
 									continue;
 							}
 						}
 						return entry.Member;
 					}
 				}
 			}
  			return null;
  		}


 		/// <summary>
 		/// Builds low-case table for CLS Compliance test
 		/// </summary>
		public Dictionary<string, object> GetPublicMembers ()
 		{
 			if (locase_table != null)
 				return locase_table;

			locase_table = new Dictionary<string, object> ();
 			foreach (var entry in member_hash) {
 				var members = entry.Value;
 				for (int ii = 0; ii < members.Count; ++ii) {
 					CacheEntry member_entry = members [ii];
 
 					if ((member_entry.EntryType & EntryType.Public) == 0)
 						continue;
 
 					// TODO: Does anyone know easier way how to detect that member is internal ?
 					switch (member_entry.EntryType & EntryType.MaskType) {
					case EntryType.Constructor:
						continue;
						
					case EntryType.Field:
						if ((((FieldInfo)member_entry.Member).Attributes & (FieldAttributes.Assembly | FieldAttributes.Public)) == FieldAttributes.Assembly)
							continue;
						break;
						
					case EntryType.Method:
						if ((((MethodInfo)member_entry.Member).Attributes & (MethodAttributes.Assembly | MethodAttributes.Public)) == MethodAttributes.Assembly)
							continue;
						break;
						
					case EntryType.Property:
						PropertyInfo pi = (PropertyInfo)member_entry.Member;
						if (pi.GetSetMethod () == null && pi.GetGetMethod () == null)
							continue;
						break;
						
					case EntryType.Event:
						EventInfo ei = (EventInfo)member_entry.Member;
						MethodInfo mi = ei.GetAddMethod ();
						if ((mi.Attributes & (MethodAttributes.Assembly | MethodAttributes.Public)) == MethodAttributes.Assembly)
							continue;
						break;
 					}
 					string lcase = ((string)entry.Key).ToLower (System.Globalization.CultureInfo.InvariantCulture);
 					locase_table [lcase] = member_entry.Member;
 					break;
 				}
 			}
 			return locase_table;
 		}
 
 		public IDictionary<string, List<CacheEntry>> Members {
 			get {
 				return member_hash;
 			}
 		}
 
 		/// <summary>
 		/// Cls compliance check whether methods or constructors parameters differing only in ref or out, or in array rank
 		/// </summary>
 		/// 
		// TODO: refactor as method is always 'this'
 		public static void VerifyClsParameterConflict (IList<CacheEntry> al, MethodCore method, MemberInfo this_builder, Report Report)
 		{
 			EntryType tested_type = (method is Constructor ? EntryType.Constructor : EntryType.Method) | EntryType.Public;
 
 			for (int i = 0; i < al.Count; ++i) {
 				var entry = al [i];
 		
 				// skip itself
 				if (entry.Member == this_builder)
 					continue;
 		
 				if ((entry.EntryType & tested_type) != tested_type)
 					continue;
 		
				MethodBase method_to_compare = (MethodBase)entry.Member;
				AttributeTester.Result result = AttributeTester.AreOverloadedMethodParamsClsCompliant (
					method.Parameters, TypeManager.GetParameterData (method_to_compare));

 				if (result == AttributeTester.Result.Ok)
 					continue;

				IMethodData md = TypeManager.GetMethod (method_to_compare);

				// TODO: now we are ignoring CLSCompliance(false) on method from other assembly which is buggy.
				// However it is exactly what csc does.
				if (md != null && !md.IsClsComplianceRequired ())
					continue;
 		
 				Report.SymbolRelatedToPreviousError (entry.Member);
				switch (result) {
				case AttributeTester.Result.RefOutArrayError:
					Report.Warning (3006, 1, method.Location,
							"Overloaded method `{0}' differing only in ref or out, or in array rank, is not CLS-compliant",
							method.GetSignatureForError ());
					continue;
				case AttributeTester.Result.ArrayArrayError:
					Report.Warning (3007, 1, method.Location,
							"Overloaded method `{0}' differing only by unnamed array types is not CLS-compliant",
							method.GetSignatureForError ());
					continue;
				}

				throw new NotImplementedException (result.ToString ());
 			}
  		}

		public bool CheckExistingMembersOverloads (MemberCore member, string name, ParametersCompiled parameters, Report Report)
		{
			List<CacheEntry> entries;
			if (!member_hash.TryGetValue (name, out entries))
				return true;

			int method_param_count = parameters.Count;
			for (int i = entries.Count - 1; i >= 0; --i) {
				CacheEntry ce = (CacheEntry) entries [i];

				if (ce.Container != member.Parent.PartialContainer)
					return true;

				Type [] p_types;
				AParametersCollection pd;
				if ((ce.EntryType & EntryType.Property) != 0) {
					pd = TypeManager.GetParameterData ((PropertyInfo) ce.Member);
					p_types = pd.Types;
				} else {
					MethodBase mb = (MethodBase) ce.Member;
		
					// TODO: This is more like a hack, because we are adding generic methods
					// twice with and without arity name
					if (TypeManager.IsGenericMethod (mb) && !member.MemberName.IsGeneric)
						continue;

					pd = TypeManager.GetParameterData (mb);
					p_types = pd.Types;
				}

				if (p_types.Length != method_param_count)
					continue;

				if (method_param_count > 0) {
					int ii = method_param_count - 1;
					Type type_a, type_b;
					do {
						type_a = parameters.Types [ii];
						type_b = p_types [ii];

						if (TypeManager.IsGenericParameter (type_a) && type_a.DeclaringMethod != null)
							type_a = typeof (TypeParameter);

						if (TypeManager.IsGenericParameter (type_b) && type_b.DeclaringMethod != null)
							type_b = typeof (TypeParameter);

						if ((pd.FixedParameters [ii].ModFlags & Parameter.Modifier.ISBYREF) !=
							(parameters.FixedParameters [ii].ModFlags & Parameter.Modifier.ISBYREF))
							break;

					} while (TypeManager.IsEqual (type_a, type_b) && ii-- != 0);

					if (ii >= 0)
						continue;

					//
					// Operators can differ in return type only
					//
					if (member is Operator) {
						Operator op = TypeManager.GetMethod ((MethodBase) ce.Member) as Operator;
						if (op != null && op.ReturnType != ((Operator) member).ReturnType)
							continue;
					}

					//
					// Report difference in parameter modifiers only
					//
					if (pd != null && member is MethodCore) {
						ii = method_param_count;
						while (ii-- != 0 && parameters.FixedParameters [ii].ModFlags == pd.FixedParameters [ii].ModFlags &&
							parameters.ExtensionMethodType == pd.ExtensionMethodType);

						if (ii >= 0) {
							MethodCore mc = TypeManager.GetMethod ((MethodBase) ce.Member) as MethodCore;
							Report.SymbolRelatedToPreviousError (ce.Member);
							if ((member.ModFlags & Modifiers.PARTIAL) != 0 && (mc.ModFlags & Modifiers.PARTIAL) != 0) {
								if (parameters.HasParams || pd.HasParams) {
									Report.Error (758, member.Location,
										"A partial method declaration and partial method implementation cannot differ on use of `params' modifier");
								} else {
									Report.Error (755, member.Location,
										"A partial method declaration and partial method implementation must be both an extension method or neither");
								}
							} else {
								if (member is Constructor) {
									Report.Error (851, member.Location,
										"Overloaded contructor `{0}' cannot differ on use of parameter modifiers only",
										member.GetSignatureForError ());
								} else {
									Report.Error (663, member.Location,
										"Overloaded method `{0}' cannot differ on use of parameter modifiers only",
										member.GetSignatureForError ());
								}
							}
							return false;
						}
					}
				}

				if ((ce.EntryType & EntryType.Method) != 0) {
					Method method_a = member as Method;
					Method method_b = TypeManager.GetMethod ((MethodBase) ce.Member) as Method;
					if (method_a != null && method_b != null && (method_a.ModFlags & method_b.ModFlags & Modifiers.PARTIAL) != 0) {
						const Modifiers partial_modifiers = Modifiers.STATIC | Modifiers.UNSAFE;
						if (method_a.IsPartialDefinition == method_b.IsPartialImplementation) {
							if ((method_a.ModFlags & partial_modifiers) == (method_b.ModFlags & partial_modifiers) ||
								method_a.Parent.IsUnsafe && method_b.Parent.IsUnsafe) {
								if (method_a.IsPartialImplementation) {
									method_a.SetPartialDefinition (method_b);
									entries.RemoveAt (i);
								} else {
									method_b.SetPartialDefinition (method_a);
									method_a.caching_flags |= MemberCore.Flags.PartialDefinitionExists;
								}
								continue;
							}

							if ((method_a.ModFlags & Modifiers.STATIC) != (method_b.ModFlags & Modifiers.STATIC)) {
								Report.SymbolRelatedToPreviousError (ce.Member);
								Report.Error (763, member.Location,
									"A partial method declaration and partial method implementation must be both `static' or neither");
							}

							Report.SymbolRelatedToPreviousError (ce.Member);
							Report.Error (764, member.Location,
								"A partial method declaration and partial method implementation must be both `unsafe' or neither");
							return false;
						}

						Report.SymbolRelatedToPreviousError (ce.Member);
						if (method_a.IsPartialDefinition) {
							Report.Error (756, member.Location, "A partial method `{0}' declaration is already defined",
								member.GetSignatureForError ());
						} else {
							Report.Error (757, member.Location, "A partial method `{0}' implementation is already defined",
								member.GetSignatureForError ());
						}

						return false;
					}

					Report.SymbolRelatedToPreviousError (ce.Member);
					IMethodData duplicate_member = TypeManager.GetMethod ((MethodBase) ce.Member);
					if (member is Operator && duplicate_member is Operator) {
						Report.Error (557, member.Location, "Duplicate user-defined conversion in type `{0}'",
							member.Parent.GetSignatureForError ());
						return false;
					}

					bool is_reserved_a = member is AbstractPropertyEventMethod || member is Operator;
					bool is_reserved_b = duplicate_member is AbstractPropertyEventMethod || duplicate_member is Operator;

					if (is_reserved_a || is_reserved_b) {
						Report.Error (82, member.Location, "A member `{0}' is already reserved",
							is_reserved_a ?
							TypeManager.GetFullNameSignature (ce.Member) :
							member.GetSignatureForError ());
						return false;
					}
				} else {
					Report.SymbolRelatedToPreviousError (ce.Member);
				}
				
				Report.Error (111, member.Location,
					"A member `{0}' is already defined. Rename this member or use different parameter types",
					member.GetSignatureForError ());
				return false;
			}

			return true;
		}
	}
}
