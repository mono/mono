//
// verifier.cs: compares two assemblies and reports differences.
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//
// (C) Sergey Chaban (serge@wildwestsoftware.com)
//

using System;
using System.IO;
using System.Collections;
using System.Reflection;

namespace Mono.Verifier {



	////////////////////////////////
	// Collections
	////////////////////////////////

	public abstract class MemberCollection : IEnumerable {

		public delegate MemberInfo [] InfoQuery (Type type, BindingFlags bindings);
		public delegate bool MemberComparer (MemberInfo mi1, MemberInfo mi2);

		protected SortedList list;
		protected MemberComparer comparer;

		protected BindingFlags bindings;

		protected MemberCollection (Type type, InfoQuery query, MemberComparer comparer, BindingFlags bindings)
		{
			if (query == null)
				throw new NullReferenceException ("Invalid query delegate.");

			if (comparer == null)
				throw new NullReferenceException ("Invalid comparer.");

			this.comparer = comparer;
			this.bindings = bindings;

			this.list = new SortedList ();

			MemberInfo [] data = query (type, bindings);
			foreach (MemberInfo info in data) {
				this.list [info.Name] = info;
			}
		}



		public MemberInfo this [string name] {
			get {
				return list [name] as MemberInfo;
			}
		}


		public override int GetHashCode ()
		{
			return list.GetHashCode ();
		}


		public override bool Equals (object o)
		{
			bool res = (o is MemberCollection);
			if (res) {
				MemberCollection another = o as MemberCollection;
				IEnumerator it = GetEnumerator ();
				while (it.MoveNext () && res) {
					MemberInfo inf1 = it.Current as MemberInfo;
					MemberInfo inf2 = another [inf1.Name];
					res &= comparer (inf1, inf2);
				}
			}
			return res;
		}



		public static bool operator == (MemberCollection c1, MemberCollection c2)
		{
			return c1.Equals (c2);
		}

		public static bool operator != (MemberCollection c1, MemberCollection c2)
		{
			return !(c1 == c2);
		}



		public IEnumerator GetEnumerator()
		{
			return new Iterator (this);
		}


		internal class Iterator : IEnumerator  {
			private MemberCollection host;
			private int pos;

			internal Iterator (MemberCollection host)
			{
				this.host=host;
				this.Reset ();
			}

			/// <summary></summary>
			public object Current
			{
				get {
					if (host != null && pos >=0 && pos < host.list.Count) {
						return host.list.GetByIndex (pos);
					} else {
						return null;
					}
				}
			}

			/// <summary></summary>
			public bool MoveNext ()
			{
				if (host!=null) {
					return (++pos) < host.list.Count;
				} else {
					return false;
				}
			}

			/// <summary></summary>
			public void Reset ()
			{
				this.pos = -1;
			}
		}

	}




	//--- Method collections

	/// <summary>
	/// Abstract collection of class' methods.
	/// </summary>
	public abstract class MethodCollectionBase : MemberCollection {


		protected MethodCollectionBase (Type type, BindingFlags bindings)
		       : base (type, new InfoQuery (Query), new MemberComparer (Comparer), bindings)
		{
		}


		private static MemberInfo [] Query (Type type, BindingFlags bindings)
		{
			// returns MethodInfo []
			return type.GetMethods (bindings);
		}

		private static bool Comparer (MemberInfo mi1, MemberInfo mi2)
		{
			bool res = false;
			if (mi1 is MethodInfo && (mi2 == null || mi2 is MethodInfo)) {
				MethodInfo inf1 = mi1 as MethodInfo;
				MethodInfo inf2 = mi2 as MethodInfo;
				res = Compare.Methods (inf1, inf2);
			} else {
				Verifier.log.Write ("internal-error", "Wrong comparer arguments.", ImportanceLevel.HIGH);
			}
			return res;
		}
	}



	/// <summary>
	/// Collection of public instance methods of a class.
	/// </summary>
	public class PublicMethods : MethodCollectionBase {

		public PublicMethods (Type type)
		       : base (type, BindingFlags.Public | BindingFlags.Instance)
		{
		}
	}

	/// <summary>
	/// Collection of public static methods of a class.
	/// </summary>
	public class PublicStaticMethods : MethodCollectionBase {

		public PublicStaticMethods (Type type)
		       : base (type, BindingFlags.Public | BindingFlags.Static)
		{
		}
	}

	/// <summary>
	/// Collection of non-public instance methods of a class.
	/// </summary>
	public class NonPublicMethods : MethodCollectionBase {

		public NonPublicMethods (Type type)
		       : base (type, BindingFlags.NonPublic | BindingFlags.Instance)
		{
		}
	}

	/// <summary>
	/// Collection of non-public static methods of a class.
	/// </summary>
	public class NonPublicStaticMethods : MethodCollectionBase {

		public NonPublicStaticMethods (Type type)
		       : base (type, BindingFlags.NonPublic | BindingFlags.Static)
		{
		}
	}





	//--- Field collections

	public abstract class FieldCollectionBase : MemberCollection {


		protected FieldCollectionBase (Type type, BindingFlags bindings)
		       : base (type, new InfoQuery (Query), new MemberComparer (Comparer), bindings)
		{
		}


		private static MemberInfo [] Query (Type type, BindingFlags bindings)
		{
			// returns FieldInfo []
			return type.GetFields (bindings);
		}

		private static bool Comparer (MemberInfo mi1, MemberInfo mi2)
		{
			bool res = false;
			if (mi1 is FieldInfo && (mi2 == null || mi2 is FieldInfo)) {
				FieldInfo inf1 = mi1 as FieldInfo;
				FieldInfo inf2 = mi2 as FieldInfo;
				res = Compare.Fields (inf1, inf2);
			} else {
				Verifier.log.Write ("internal-error", "Wrong comparer arguments.", ImportanceLevel.HIGH);
			}
			return res;
		}
	}


	public class PublicFields : FieldCollectionBase {

		public PublicFields (Type type)
		       : base (type, BindingFlags.Public | BindingFlags.Instance)
		{
		}
	}

	public class PublicStaticFields : FieldCollectionBase {

		public PublicStaticFields (Type type)
		       : base (type, BindingFlags.Public | BindingFlags.Static)
		{
		}
	}

	public class NonPublicFields : FieldCollectionBase {

		public NonPublicFields (Type type)
		       : base (type, BindingFlags.NonPublic | BindingFlags.Instance)
		{
		}
	}

	public class NonPublicStaticFields : FieldCollectionBase {

		public NonPublicStaticFields (Type type)
		       : base (type, BindingFlags.NonPublic | BindingFlags.Static)
		{
		}
	}





	public abstract class AbstractTypeStuff {
		public readonly Type type;

		public AbstractTypeStuff (Type type)
		{
			if (type == null)
				throw new NullReferenceException ("Invalid type.");

			this.type = type;
		}

		public override int GetHashCode ()
		{
			return type.GetHashCode ();
		}

		public static bool operator == (AbstractTypeStuff t1, AbstractTypeStuff t2)
		{
			if ((t1 as object) == null) {
				if ((t2 as object) == null) return true;
				return false;
			}
			return t1.Equals (t2);
		}

		public static bool operator != (AbstractTypeStuff t1, AbstractTypeStuff t2)
		{
			return !(t1 == t2);
		}

		public override bool Equals (object o)
		{
			return (o is AbstractTypeStuff && CompareTypes (o as AbstractTypeStuff));
		}

		protected virtual bool CompareTypes (AbstractTypeStuff that)
		{
			Verifier.Log.Write ("info", "Comparing types.", ImportanceLevel.LOW);
			bool res;

			res = Compare.Types (this.type, that.type);

			return res;
		}

	}




	/// <summary>
	///  Represents a class.
	/// </summary>
	public class ClassStuff : AbstractTypeStuff {

		public PublicMethods publicMethods;
		public PublicStaticMethods publicStaticMethods;
		public NonPublicMethods nonpublicMethods;
		public NonPublicStaticMethods nonpublicStaticMethods;

		public PublicFields publicFields;
		public PublicStaticFields publicStaticFields;
		public NonPublicFields nonpublicFields;
		public NonPublicStaticFields nonpublicStaticFields;

		public ClassStuff (Type type) : base (type)
		{
			publicMethods = new PublicMethods (type);
			publicStaticMethods = new PublicStaticMethods (type);
			nonpublicMethods = new NonPublicMethods (type);
			nonpublicStaticMethods = new NonPublicStaticMethods (type);

			publicFields = new PublicFields (type);
			publicStaticFields = new PublicStaticFields (type);
			nonpublicFields = new NonPublicFields (type);
			nonpublicStaticFields = new NonPublicStaticFields (type);
		}


		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		private bool CompareMethods (ClassStuff that)
		{
			bool res = true;
			bool ok;

			Verifier.Log.Write ("info", "Comparing public instance methods.", ImportanceLevel.LOW);
			ok = (this.publicMethods == that.publicMethods);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			Verifier.Log.Write ("info", "Comparing public static methods.", ImportanceLevel.LOW);
			ok = (this.publicStaticMethods == that.publicStaticMethods);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			Verifier.Log.Write ("info", "Comparing non-public instance methods.", ImportanceLevel.LOW);
			ok = (this.nonpublicMethods == that.nonpublicMethods);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			Verifier.Log.Write ("info", "Comparing non-public static methods.", ImportanceLevel.LOW);
			ok = (this.nonpublicStaticMethods == that.nonpublicStaticMethods);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			return res;
		}


		private bool CompareFields (ClassStuff that)
		{
			bool res = true;
			bool ok;

			Verifier.Log.Write ("info", "Comparing public instance fields.", ImportanceLevel.LOW);
			ok = (this.publicFields == that.publicFields);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			Verifier.Log.Write ("info", "Comparing public static fields.", ImportanceLevel.LOW);
			ok = (this.publicStaticFields == that.publicStaticFields);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			Verifier.Log.Write ("info", "Comparing non-public instance fields.", ImportanceLevel.LOW);
			ok = (this.nonpublicFields == that.nonpublicFields);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			Verifier.Log.Write ("info", "Comparing non-public static fields.", ImportanceLevel.LOW);
			ok = (this.nonpublicStaticFields == that.nonpublicStaticFields);
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			return res;
		}


		public override bool Equals (object o)
		{
			bool res = (o is ClassStuff);
			if (res) {
				ClassStuff that = o as ClassStuff;

				res &= this.CompareTypes (that);
				if (!res && Verifier.stopOnError) return res;

				res &= this.CompareMethods (that);
				if (!res && Verifier.stopOnError) return res;

				res &= this.CompareFields (that);
				if (!res && Verifier.stopOnError) return res;

			}
			return res;
		}

	}



	/// <summary>
	///  Represents an interface.
	/// </summary>
	public class InterfaceStuff : AbstractTypeStuff {

		public PublicMethods publicMethods;

		public InterfaceStuff (Type type) : base (type)
		{
			publicMethods = new PublicMethods (type);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			bool res = (o is InterfaceStuff);
			if (res) {
				bool ok;
				InterfaceStuff that = o as InterfaceStuff;

				res = this.CompareTypes (that);
				if (!res && Verifier.stopOnError) return res;

				Verifier.Log.Write ("info", "Comparing interface methods.", ImportanceLevel.LOW);
				ok = (this.publicMethods == that.publicMethods);
				res &= ok;
				if (!ok && Verifier.stopOnError) return res;
			}
			return res;
		}

	}



	/// <summary>
	///  Represents an enumeration.
	/// </summary>
	public class EnumStuff : AbstractTypeStuff {

		//public FieldInfo [] members;

		public string baseType;
		public Hashtable enumTable;
		public bool isFlags;

		public EnumStuff (Type type) : base (type)
		{
			//members = type.GetFields (BindingFlags.Public | BindingFlags.Static);

			Array values = Enum.GetValues (type);
			Array names = Enum.GetNames (type);

			baseType = Enum.GetUnderlyingType (type).Name;

			enumTable = new Hashtable ();

			object [] attrs = type.GetCustomAttributes (false);
			isFlags = (attrs != null && attrs.Length > 0);
			if (isFlags) {
				foreach (object attr in attrs) {
					isFlags |= (attr is FlagsAttribute);
				}
			}

			int indx = 0;
			foreach (string id in names) {
				enumTable [id] = Convert.ToInt64(values.GetValue(indx) as Enum);
				++indx;
			}
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			bool res = (o is EnumStuff);
			bool ok;

			if (res) {
				EnumStuff that = o as EnumStuff;
				ok = this.CompareTypes (that);
				res &= ok;
				if (!ok && Verifier.stopOnError) return res;

				ok = (this.baseType == that.baseType);
				res &= ok;
				if (!ok) {
					Verifier.log.Write ("error",
						String.Format ("Underlying types mismatch [{0}, {1}].", this.baseType, that.baseType),
						ImportanceLevel.MEDIUM);
					if (Verifier.stopOnError) return res;
				}

				Verifier.Log.Write ("info", "Comparing [Flags] attribute.");
				ok = !(this.isFlags ^ that.isFlags);
				res &= ok;
				if (!ok) {
					Verifier.log.Write ("error",
						String.Format ("[Flags] attribute mismatch ({0} : {1}).", this.isFlags ? "Yes" : "No", that.isFlags ? "Yes" : "No"),
					    ImportanceLevel.MEDIUM);
					if (Verifier.stopOnError) return res;
				}

				Verifier.Log.Write ("info", "Comparing enum values.");

				ICollection names = enumTable.Keys;
				foreach (string id in names) {
					ok = that.enumTable.ContainsKey (id);
					res &= ok;
					if (!ok) {
						Verifier.log.Write ("error", String.Format("{0} absent in enumeration.", id),
							ImportanceLevel.MEDIUM);
						if (Verifier.stopOnError) return res;
					}

					if (ok) {
						long val1 = (long) this.enumTable [id];
						long val2 = (long) that.enumTable [id];
						ok = (val1 == val2);
						res &= ok;
						if (!ok) {
							Verifier.log.Write ("error",
								String.Format ("Enum values mismatch [{0}: {1} != {2}].", id, val1, val2),
								ImportanceLevel.MEDIUM);
							if (Verifier.stopOnError) return res;
						}
					}
				}
			}
			return res;
		}
	}



	public sealed class TypeArray {
		public static readonly TypeArray empty = new TypeArray (Type.EmptyTypes);

		public Type [] types;

		public TypeArray (Type [] types)
		{
			this.types = new Type [types.Length];
			for (int i = 0; i < types.Length; i++) {
				this.types.SetValue (types.GetValue (i), i);
			}
		}
	}



	public class AssemblyLoader {
		public delegate void Hook (TypeArray assemblyTypes);

		private static Hashtable cache;

		private Hook hook;

		static AssemblyLoader ()
		{
			cache = new Hashtable (11);
		}

		public AssemblyLoader (Hook hook)
		{
			if (hook == null)
				throw new NullReferenceException ("Invalid loader hook.");

			this.hook = hook;
		}


		public bool LoadFrom (string assemblyName)
		{
			bool res = false;
			try {
				TypeArray types = TypeArray.empty;

				lock (cache) {
					if (cache.Contains (assemblyName)) {
						types = (cache [assemblyName] as TypeArray);
						if (types == null) types = TypeArray.empty;
					} else {
						Assembly asm = Assembly.LoadFrom (assemblyName);
						Type [] allTypes = asm.GetTypes ();
						if (allTypes == null) allTypes = Type.EmptyTypes;
						types = new TypeArray (allTypes);
						cache [assemblyName] = types;
					}
				}
				hook (types);
				res = true;
			} catch (ReflectionTypeLoadException rtle) {
				// FIXME: Should we try to recover? Use loaded portion of types.
				Type [] loaded = rtle.Types;
				for (int i = 0, xCnt = 0; i < loaded.Length; i++) {
					if (loaded [i] == null) {
						Verifier.log.Write ("fatal error",
						    String.Format ("Unable to load {0}, reason - {1}", loaded [i], rtle.LoaderExceptions [xCnt++]),
						    ImportanceLevel.LOW);
					}
				}
			} catch (FileNotFoundException fnfe) {
					Verifier.log.Write ("fatal error", fnfe.ToString (), ImportanceLevel.LOW);
			} catch (Exception x) {
					Verifier.log.Write ("fatal error", x.ToString (), ImportanceLevel.LOW);
			}

			return res;
		}

	}




	public abstract class AbstractTypeCollection : SortedList {

		private AssemblyLoader loader;

		public AbstractTypeCollection ()
		{
			loader = new AssemblyLoader (new AssemblyLoader.Hook (LoaderHook));
		}

		public AbstractTypeCollection (string assemblyName) : this ()
		{
			LoadFrom (assemblyName);
		}

		public abstract void LoaderHook (TypeArray types);


		public bool LoadFrom (string assemblyName)
		{
			return loader.LoadFrom (assemblyName);
		}

	}



	public class ClassCollection : AbstractTypeCollection {

		public ClassCollection () : base ()
		{
		}

		public ClassCollection (string assemblyName)
		: base (assemblyName)
		{
		}


		public override void LoaderHook (TypeArray types)
		{
			foreach (Type type in types.types) {
				if (type.IsClass) {
					this [type.FullName] = new ClassStuff (type);
				}
			}
		}

	}


	public class InterfaceCollection : AbstractTypeCollection {

		public InterfaceCollection () : base ()
		{
		}

		public InterfaceCollection (string assemblyName)
		: base (assemblyName)
		{
		}


		public override void LoaderHook (TypeArray types)
		{
			foreach (Type type in types.types) {
				if (type.IsInterface) {
					this [type.FullName] = new InterfaceStuff (type);
				}
			}
		}

	}



	public class EnumCollection : AbstractTypeCollection {

		public EnumCollection () : base ()
		{
		}

		public EnumCollection (string assemblyName)
		: base (assemblyName)
		{
		}

		public override void LoaderHook (TypeArray types)
		{
			foreach (Type type in types.types) {
				if (type.IsEnum) {
					this [type.FullName] = new EnumStuff (type);
				}
			}
		}
	}



	public class AssemblyStuff {

		public string name;
		public bool valid;

		public ClassCollection classes;
		public InterfaceCollection interfaces;
		public EnumCollection enums;


		protected delegate bool Comparer (AssemblyStuff asm1, AssemblyStuff asm2);
		private static ArrayList comparers;

		static AssemblyStuff ()
		{
			comparers = new ArrayList ();
			comparers.Add (new Comparer (CompareNumClasses));
			comparers.Add (new Comparer (CompareNumInterfaces));
			comparers.Add (new Comparer (CompareClasses));
			comparers.Add (new Comparer (CompareInterfaces));
			comparers.Add (new Comparer (CompareEnums));
		}

		protected static bool CompareNumClasses (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			bool res = (asm1.classes.Count == asm2.classes.Count);
			if (!res) Verifier.Log.Write ("error", "Number of classes mismatch.", ImportanceLevel.MEDIUM);
			return res;
		}

		protected static bool CompareNumInterfaces (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			bool res = (asm1.interfaces.Count == asm2.interfaces.Count);
			if (!res) Verifier.Log.Write ("error", "Number of interfaces mismatch.", ImportanceLevel.MEDIUM);
			return res;
		}


		protected static bool CompareClasses (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			bool res = true;
			Verifier.Log.Write ("info", "Comparing classes.");

			foreach (DictionaryEntry c in asm1.classes) {
				string className = c.Key as string;

				if (Verifier.Excluded.Contains (className)) {
					Verifier.Log.Write ("info", String.Format ("Ignoring class {0}.", className), ImportanceLevel.MEDIUM);
					continue;
				}

				Verifier.Log.Write ("class", className);

				ClassStuff class1 = c.Value as ClassStuff;
				ClassStuff class2 = asm2.classes [className] as ClassStuff;

				if (class2 == null) {
					Verifier.Log.Write ("error", String.Format ("There is no such class in {0}", asm2.name));
					res = false;
					if (Verifier.stopOnError || !Verifier.ignoreMissingTypes) return res;
					continue;
				}

				res &= (class1 == class2);
				if (!res && Verifier.stopOnError) return res;
			}

			return res;
		}


		protected static bool CompareInterfaces (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			bool res = true;
			Verifier.Log.Write ("info", "Comparing interfaces.");

			foreach (DictionaryEntry ifc in asm1.interfaces) {
				string ifcName = ifc.Key as string;
				Verifier.Log.Write ("interface", ifcName);

				InterfaceStuff ifc1 = ifc.Value as InterfaceStuff;
				InterfaceStuff ifc2 = asm2.interfaces [ifcName] as InterfaceStuff;

				if (ifc2 == null) {
					Verifier.Log.Write ("error", String.Format ("There is no such interface in {0}", asm2.name));
					res = false;
					if (Verifier.stopOnError || !Verifier.ignoreMissingTypes) return res;
					continue;
				}

				res &= (ifc1 == ifc2);
				if (!res && Verifier.stopOnError) return res;

			}

			return res;
		}


		protected static bool CompareEnums (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			bool res = true;
			Verifier.Log.Write ("info", "Comparing enums.");

			foreach (DictionaryEntry e in asm1.enums) {
				string enumName = e.Key as string;
				Verifier.Log.Write ("enum", enumName);

				EnumStuff e1 = e.Value as EnumStuff;
				EnumStuff e2 = asm2.enums [enumName] as EnumStuff;

				if (e2 == null) {
					Verifier.Log.Write ("error", String.Format ("There is no such enum in {0}", asm2.name));
					res = false;
					if (Verifier.stopOnError || !Verifier.ignoreMissingTypes) return res;
					continue;
				}
				res &= (e1 == e2);
				if (!res && Verifier.stopOnError) return res;
			}

			return res;
		}



		public AssemblyStuff (string assemblyName)
		{
			this.name = assemblyName;
			valid = false;
		}

		public bool Load ()
		{
			bool res = true;
			bool ok;

			classes = new ClassCollection ();
			ok = classes.LoadFrom (name);
			res &= ok;
			if (!ok) Verifier.log.Write ("error", String.Format ("Unable to load classes from {0}.", name), ImportanceLevel.HIGH);

			interfaces = new InterfaceCollection ();
			ok = interfaces.LoadFrom (name);
			res &= ok;
			if (!ok) Verifier.log.Write ("error", String.Format ("Unable to load interfaces from {0}.", name), ImportanceLevel.HIGH);

			enums = new EnumCollection ();
			ok = enums.LoadFrom (name);
			res &= ok;
			if (!ok) Verifier.log.Write ("error", String.Format ("Unable to load enums from {0}.", name), ImportanceLevel.HIGH);

			valid = res;
			return res;
		}


		public override bool Equals (object o)
		{
			bool res = (o is AssemblyStuff);
			if (res) {
				AssemblyStuff that = o as AssemblyStuff;
				IEnumerator it = comparers.GetEnumerator ();
				while ((res || !Verifier.stopOnError) && it.MoveNext ()) {
					Comparer compare = it.Current as Comparer;
					res &= compare (this, that);
				}
			}
			return res;
		}


		public static bool operator == (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			return asm1.Equals (asm2);
		}

		public static bool operator != (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			return !(asm1 == asm2);
		}

		public override int GetHashCode ()
		{
			return classes.GetHashCode () ^ interfaces.GetHashCode ();
		}


		public override string ToString ()
		{
			string res;
			if (valid) {
				res = String.Format ("Asssembly {0}, valid, {1} classes, {2} interfaces, {3} enums.",
				             name, classes.Count, interfaces.Count, enums.Count);
			} else {
				res = String.Format ("Asssembly {0}, invalid.", name);
			}
			return res;
		}

	}




	////////////////////////////////
	// Compare
	////////////////////////////////

	public sealed class Compare {

		private Compare ()
		{
		}


		public static bool Parameters (ParameterInfo[] params1, ParameterInfo[] params2)
		{
			bool res = true;
			if (params1.Length != params2.Length) {
				Verifier.Log.Write ("Parameter count mismatch.");
				return false;
			}

			int count = params1.Length;

			for (int i = 0; i < count && res; i++) {
				if (params1 [i].Name != params2 [i].Name) {
					Verifier.Log.Write ("error", String.Format ("Parameters names mismatch {0}, {1}.", params1 [i].Name, params2 [i].Name));
					res = false;
					if (Verifier.stopOnError) break;
				}

				Verifier.Log.Write ("parameter", params1 [i].Name);

				if (!Compare.Types (params1 [i].ParameterType, params2 [i].ParameterType)) {
					Verifier.Log.Write ("error", String.Format ("Parameters types mismatch {0}, {1}.", params1 [i].ParameterType, params2 [i].ParameterType));
					res = false;
					if (Verifier.stopOnError) break;
				}


				if (Verifier.checkOptionalFlags) {
					if (params1 [i].IsIn != params2 [i].IsIn) {
						Verifier.Log.Write ("error", "[in] mismatch.");
						res = false;
						if (Verifier.stopOnError) break;
					}

					if (params1 [i].IsOut != params2 [i].IsOut) {
						Verifier.Log.Write ("error", "[out] mismatch.");
						res = false;
						if (Verifier.stopOnError) break;
					}

					if (params1 [i].IsRetval != params2 [i].IsRetval) {
						Verifier.Log.Write ("error", "[ref] mismatch.");
						res = false;
						if (Verifier.stopOnError) break;
					}

					if (params1 [i].IsOptional != params2 [i].IsOptional) {
						Verifier.Log.Write ("error", "Optional flag mismatch.");
						res = false;
						if (Verifier.stopOnError) break;
					}

				} // checkOptionalFlags


			}

			return res;
		}



		public static bool Methods (MethodInfo mi1, MethodInfo mi2)
		{
			
			if (mi2 == null) {
				Verifier.Log.Write ("error", String.Format ("There is no such method {0}.", mi1.Name), ImportanceLevel.MEDIUM);
				return false;
			}


			Verifier.Log.Flush ();
			Verifier.Log.Write ("method", String.Format ("{0}.", mi1.Name));
			bool res = true;
			bool ok;
			string expected;

			ok = Compare.Types (mi1.ReturnType, mi2.ReturnType);
			res &= ok;
			if (!ok) {
				Verifier.Log.Write ("error", "Return types mismatch.", ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}




			ok = (mi1.IsAbstract == mi2.IsAbstract);
			res &= ok;
			if (!ok) {
				expected = (mi1.IsAbstract) ? "abstract" : "non-abstract";
				Verifier.Log.Write ("error", String.Format ("Expected to be {0}.", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			ok = (mi1.IsVirtual == mi2.IsVirtual);
			res &= ok;
			if (!ok) {
				expected = (mi1.IsVirtual) ? "virtual" : "non-virtual";
				Verifier.Log.Write ("error", String.Format ("Expected to be {0}.", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			ok = (mi1.IsFinal == mi2.IsFinal);
			res &= ok;
			if (!ok) {
				expected = (mi1.IsFinal) ? "final" : "overridable";
				Verifier.Log.Write ("error", String.Format ("Expected to be {0}.", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}



			// compare access modifiers

			ok = (mi1.IsPrivate == mi2.IsPrivate);
			res &= ok;
			if (!ok) {
				expected = (mi1.IsPublic) ? "public" : "private";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}


			ok = (mi1.IsFamily == mi2.IsFamily);
			res &= ok;
			if (!ok) {
				expected = (mi1.IsFamily) ? "protected" : "!protected";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			ok = (mi1.IsAssembly == mi2.IsAssembly);
			res &= ok;
			if (!ok) {
				expected = (mi1.IsAssembly) ? "internal" : "!internal";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}


			ok = (mi1.IsStatic == mi2.IsStatic);
			res &= ok;
			if (!ok) {
				expected = (mi1.IsStatic) ? "static" : "instance";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}



			// parameters

			ok = Compare.Parameters (mi1.GetParameters (), mi2.GetParameters ());
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;


			ok = (mi1.CallingConvention == mi2.CallingConvention);
			res &= ok;
			if (!ok) {
				Verifier.Log.Write ("error", "Calling conventions mismatch.", ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}




			return res;
		}


		public static bool Fields (FieldInfo fi1, FieldInfo fi2)
		{
			if (fi2 == null) {
				Verifier.Log.Write ("error", String.Format ("There is no such field {0}.", fi1.Name), ImportanceLevel.MEDIUM);
				return false;
			}

			bool res = true;
			bool ok;
			string expected;

			Verifier.Log.Write ("field", String.Format ("{0}.", fi1.Name));

			ok = (fi1.IsPrivate == fi2.IsPrivate);
			res &= ok;
			if (!ok) {
				expected = (fi1.IsPublic) ? "public" : "private";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			ok = (fi1.IsFamily == fi2.IsFamily);
			res &= ok;
			if (!ok) {
				expected = (fi1.IsFamily) ? "protected" : "!protected";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			ok = (fi1.IsAssembly == fi2.IsAssembly);
			res &= ok;
			if (!ok) {
				expected = (fi1.IsAssembly) ? "internal" : "!internal";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			ok = (fi1.IsInitOnly == fi2.IsInitOnly);
			res &= ok;
			if (!ok) {
				expected = (fi1.IsInitOnly) ? "readonly" : "!readonly";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			ok = (fi1.IsStatic == fi2.IsStatic);
			res &= ok;
			if (!ok) {
				expected = (fi1.IsStatic) ? "static" : "instance";
				Verifier.Log.Write ("error", String.Format ("Accessibility levels mismatch (expected [{0}]).", expected), ImportanceLevel.MEDIUM);
				if (Verifier.stopOnError) return res;
			}

			return res;
		}



		public static bool Types (Type type1, Type type2)
		{
			// NOTE:
			// simply calling type1.Equals (type2) won't work,
			// types are in different assemblies hence they have
			// different (fully-qualified) names.
			int eqFlags = 0;
			eqFlags |= (type1.IsAbstract  == type2.IsAbstract)  ? 0 : 0x001;
			eqFlags |= (type1.IsClass     == type2.IsClass)     ? 0 : 0x002;
			eqFlags |= (type1.IsValueType == type2.IsValueType) ? 0 : 0x004;
			eqFlags |= (type1.IsPublic    == type2.IsPublic)    ? 0 : 0x008;
			eqFlags |= (type1.IsSealed    == type2.IsSealed)    ? 0 : 0x010;
			eqFlags |= (type1.IsEnum      == type2.IsEnum)      ? 0 : 0x020;
			eqFlags |= (type1.IsPointer   == type2.IsPointer)   ? 0 : 0x040;
			eqFlags |= (type1.IsPrimitive == type2.IsPrimitive) ? 0 : 0x080;
			bool res = (eqFlags == 0);

			if (!res) {
				// TODO: convert flags into descriptive message.
				Verifier.Log.Write ("error", "Types mismatch (0x" + eqFlags.ToString("X") + ").", ImportanceLevel.HIGH);
			}


			bool ok;

			ok = (type1.Attributes & TypeAttributes.BeforeFieldInit) ==
			     (type2.Attributes & TypeAttributes.BeforeFieldInit);
			if (!ok) {
				Verifier.Log.Write ("error", "Types attributes mismatch: BeforeFieldInit.", ImportanceLevel.HIGH);
			}
			res &= ok;

			ok = (type1.Attributes & TypeAttributes.ExplicitLayout) ==
			     (type2.Attributes & TypeAttributes.ExplicitLayout);
			if (!ok) {
				Verifier.Log.Write ("error", "Types attributes mismatch: ExplicitLayout.", ImportanceLevel.HIGH);
			}
			res &= ok;

			ok = (type1.Attributes & TypeAttributes.SequentialLayout) ==
			     (type2.Attributes & TypeAttributes.SequentialLayout);
			if (!ok) {
				Verifier.Log.Write ("error", "Types attributes mismatch: SequentialLayout.", ImportanceLevel.HIGH);
			}
			res &= ok;

			ok = (type1.Attributes & TypeAttributes.Serializable) ==
			     (type2.Attributes & TypeAttributes.Serializable);
			if (!ok) {
				Verifier.Log.Write ("error", "Types attributes mismatch: Serializable.", ImportanceLevel.HIGH);
			}
			res &= ok;

			return res;
		}

	}




	////////////////////////////////
	// Log
	////////////////////////////////

	public enum ImportanceLevel : int {
		LOW = 0, MEDIUM, HIGH
	}


	public interface ILogger {

		void Write (string tag, string msg, ImportanceLevel importance);
		void Write (string msg, ImportanceLevel level);
		void Write (string tag, string msg);
		void Write (string msg);
		ImportanceLevel DefaultImportance {get; set;}
		void Flush ();
		void Close ();
	}


	public abstract class AbstractLogger : ILogger {
		private ImportanceLevel defImportance = ImportanceLevel.MEDIUM;

		public abstract void Write (string tag, string msg, ImportanceLevel importance);
		public abstract void Write (string msg, ImportanceLevel level);

		public virtual void Write (string tag, string msg)
		{
			Write (tag, msg, DefaultImportance);
		}

		public virtual void Write (string msg)
		{
			Write (msg, DefaultImportance);
		}

		public virtual ImportanceLevel DefaultImportance {
			get {
				return defImportance;
			}
			set {
				defImportance = value < ImportanceLevel.LOW
				                 ? ImportanceLevel.LOW
				                 : value > ImportanceLevel.HIGH
				                   ? ImportanceLevel.HIGH
				                   : value;
			}
		}

		public abstract void Flush ();
		public abstract void Close ();

	}



	public class TextLogger : AbstractLogger {

		private TextWriter writer;

		public TextLogger (TextWriter writer)
		{
			if (writer == null)
				throw new NullReferenceException ();

			this.writer = writer;
		}

		private void DoWrite (string tag, string msg)
		{
			if (tag != null && tag.Length > 0) {
				writer.WriteLine ("[{0}]\t{1}", tag, msg);
			} else {
				writer.WriteLine ("\t\t" + msg);
			}
		}

		public override void Write (string tag, string msg, ImportanceLevel importance)
		{
			int v = Log.VerboseLevel;
			switch (v) {
			case 0 :
				break;
			case 1 :
				if (importance >= ImportanceLevel.HIGH) {
					DoWrite (tag, msg);
				}
				break;
			case 2 :
				if (importance >= ImportanceLevel.MEDIUM) {
					DoWrite (tag, msg);
				}
				break;
			case 3 :
				DoWrite (tag, msg);
				break;
			default:
				break;
			}
		}

		public override void Write (string msg, ImportanceLevel importance)
		{
			Write (null, msg, importance);
		}

		public override void Flush ()
		{
			Console.Out.Flush ();
		}

		public override void Close ()
		{
			if (writer != Console.Out && writer != Console.Error) {
				writer.Close ();
			}
		}
	}



	public sealed class Log {

		private static int verbose = 3;

		private ArrayList consumers;

		public Log (bool useDefault)
		{
			consumers = new ArrayList ();
			if (useDefault) AddConsumer (new TextLogger (Console.Out));
		}

		public Log () : this (true)
		{
		}


		public static int VerboseLevel {
			get {
				return verbose;
			}
			set {
				verbose = (value < 0)
				           ? 0
				           : (value > 3)
				             ? 3 : value;
			}
		}

		public void AddConsumer (ILogger consumer)
		{
			consumers.Add (consumer);
		}


		public void Write (string tag, string msg, ImportanceLevel importance)
		{
			foreach (ILogger logger in consumers) {
				if (tag == null || tag == "") {
					logger.Write (msg, importance);
				} else {
					logger.Write (tag, msg, importance);
				}
			}
		}

		public void Write (string msg, ImportanceLevel importance)
		{
			Write (null, msg, importance);
		}


		public void Write (string tag, string msg)
		{
			foreach (ILogger logger in consumers) {
				if (tag == null || tag == "") {
					logger.Write (msg);
				} else {
					logger.Write (tag, msg);
				}
			}
		}

		public void Write (string msg)
		{
			Write (null, msg);
		}


		public void Flush ()
		{
			foreach (ILogger logger in consumers) {
				logger.Flush ();
			}
		}


		public void Close ()
		{
			foreach (ILogger logger in consumers) {
				logger.Flush ();
				logger.Close ();
			}
		}

	}






	////////////////////////////////
	// Main
	////////////////////////////////

	public class Verifier {

		public static readonly Log log = new Log ();
		public static bool stopOnError = false;
		public static bool ignoreMissingTypes = true;
		public static bool checkOptionalFlags = true;

		private static readonly IList excluded;

		static Verifier ()
		{
			excluded = new ArrayList ();
			excluded.Add ("<PrivateImplementationDetails>");
		}


		private Verifier ()
		{
		}

		public static Log Log {
			get {
				return log;
			}
		}

		public static IList Excluded {
			get {
				return excluded;
			}
		}



		public static void Main (String [] args)
		{
			if (args.Length < 2) {
				Console.WriteLine ("Usage: verifier assembly1 assembly2");
			} else {
				string name1 = args [0];
				string name2 = args [1];

				bool ok = false;

				AssemblyStuff asm1 = new AssemblyStuff (name1);
				AssemblyStuff asm2 = new AssemblyStuff (name2);
				ok = asm1.Load ();
				if (!ok) {
					Console.WriteLine ("Unable to load assembly {0}.", name1);
					Environment.Exit (-1);
				}

				ok = asm2.Load ();
				if (!ok) {
					Console.WriteLine ("Unable to load assembly {0}.", name2);
					Environment.Exit (-1);
				}


				try {
					ok = (asm1 == asm2);
				} catch {
					ok = false;
				} finally {
					Log.Close ();
				}

				if (!ok) {
					Console.WriteLine ("--- not equal");
					Environment.Exit (-1);
				}
			}
		}

	}


}

