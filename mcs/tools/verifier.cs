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



		public IEnumerator GetEnumerator() {
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



	public class PublicMethods : MethodCollectionBase {

		public PublicMethods (Type type)
		       : base (type, BindingFlags.Public | BindingFlags.Instance)
		{
		}
	}

	public class PublicStaticMethods : MethodCollectionBase {

		public PublicStaticMethods (Type type)
		       : base (type, BindingFlags.Public | BindingFlags.Static)
		{
		}
	}

	public class NonPublicMethods : MethodCollectionBase {

		public NonPublicMethods (Type type)
		       : base (type, BindingFlags.NonPublic | BindingFlags.Instance)
		{
		}
	}

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








	public class ClassStuff {

		public Type type;
		public PublicMethods publicMethods;
		public PublicStaticMethods publicStaticMethods;
		public NonPublicMethods nonpublicMethods;
		public NonPublicStaticMethods nonpublicStaticMethods;

		public ClassStuff (Type type)
		{
			if (type == null)
				throw new NullReferenceException ("Invalid type.");

			this.type = type;
			publicMethods = new PublicMethods (type);
			publicStaticMethods = new PublicStaticMethods (type);
			nonpublicMethods = new NonPublicMethods (type);
			nonpublicStaticMethods = new NonPublicStaticMethods (type);
		}
	}



	public class InterfaceStuff {

		public Type type;
		public PublicMethods publicMethods;

		public InterfaceStuff (Type type)
		{
			if (type == null)
				throw new NullReferenceException ("Invalid type.");

			this.type = type;
			publicMethods = new PublicMethods (type);
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
		public delegate void Hook (TypeArray assemblyExports);

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
				TypeArray exports = TypeArray.empty;

				lock (cache) {
					if (cache.Contains (assemblyName)) {
						exports = (cache [assemblyName] as TypeArray);
					} else {
						Assembly asm = Assembly.LoadFrom (assemblyName);
						Type [] types = asm.GetExportedTypes ();
						if (types == null) types = Type.EmptyTypes;
						exports = new TypeArray (types);
						cache [assemblyName] = exports;
					}
				}
				hook (exports);
				res = true;
			} catch (ReflectionTypeLoadException rtle) {
				Type [] loaded = rtle.Types;
				for (int i = 0; i < loaded.Length; i++) {
					Verifier.log.Write ("fatal error",
					                    String.Format ("Unable to load {0}, reason - {1}", loaded [i], rtle.LoaderExceptions [i]),
					                    ImportanceLevel.LOW);
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

		public abstract void LoaderHook (TypeArray exports);


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


		public override void LoaderHook (TypeArray exports)
		{
			foreach (Type type in exports.types) {
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


		public override void LoaderHook (TypeArray exports)
		{
			foreach (Type type in exports.types) {
				if (type.IsInterface) {
					this [type.FullName] = new InterfaceStuff (type);
				}
			}
		}

	}



	public class AssemblyStuff {

		public string name;
		public bool valid;

		public ClassCollection classes;
		public InterfaceCollection interfaces;



		protected delegate bool Comparer (AssemblyStuff asm1, AssemblyStuff asm2);
		private static ArrayList comparers;

		static AssemblyStuff ()
		{
			comparers = new ArrayList ();
			comparers.Add (new Comparer (CompareNumClasses));
			comparers.Add (new Comparer (CompareNumInterfaces));
			comparers.Add (new Comparer (CompareClasses));
			comparers.Add (new Comparer (CompareInterfaces));
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
			bool ok;
			Verifier.Log.Write ("info", "Comparing classes.");
			foreach (DictionaryEntry c in asm1.classes) {
				string className = c.Key as string;
				Verifier.Log.Write ("class", className);

				ClassStuff class1 = c.Value as ClassStuff;
				ClassStuff class2 = asm2.classes [className] as ClassStuff;

				if (class2 == null) {
					Verifier.Log.Write ("error", String.Format ("There is no such class in {0}", asm2.name));
					return false;
				}

				Verifier.Log.Write ("info", "Comparing public instance methods.", ImportanceLevel.LOW);
				ok = (class1.publicMethods == class2.publicMethods);
				res &= ok;
				if (!ok && Verifier.stopOnError) return res;

				Verifier.Log.Write ("info", "Comparing public static methods.", ImportanceLevel.LOW);
				ok = (class1.publicStaticMethods == class2.publicStaticMethods);
				res &= ok;
				if (!ok && Verifier.stopOnError) return res;

				Verifier.Log.Write ("info", "Comparing non-public instance methods.", ImportanceLevel.LOW);
				ok = (class1.nonpublicMethods == class2.nonpublicMethods);
				res &= ok;
				if (!ok && Verifier.stopOnError) return res;

				Verifier.Log.Write ("info", "Comparing non-public static methods.", ImportanceLevel.LOW);
				ok = (class1.nonpublicStaticMethods == class2.nonpublicStaticMethods);
				res &= ok;
				if (!ok && Verifier.stopOnError) return res;

			}
			return res;
		}


		protected static bool CompareInterfaces (AssemblyStuff asm1, AssemblyStuff asm2)
		{
			bool res = true;
			bool ok;
			Verifier.Log.Write ("info", "Comparing interfaces.");
			foreach (DictionaryEntry ifc in asm1.interfaces) {
				string ifcName = ifc.Key as string;
				Verifier.Log.Write ("interface", ifcName);

				InterfaceStuff ifc1 = ifc.Value as InterfaceStuff;
				InterfaceStuff ifc2 = asm2.interfaces [ifcName] as InterfaceStuff;

				if (ifc2 == null) {
					Verifier.Log.Write ("error", String.Format ("There is no such interface in {0}", asm2.name));
					return false;
				}

				Verifier.Log.Write ("info", "Comparing interface methods.", ImportanceLevel.LOW);
				ok = (ifc1.publicMethods == ifc2.publicMethods);
				res &= ok;
				if (!ok && Verifier.stopOnError) return res;
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
				res = String.Format ("Asssembly {0}, valid, {1} classes, {2} interfaces.",
				             name, classes.Count, interfaces.Count);
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

				if (params1 [i].ParameterType != params2 [i].ParameterType) {
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

					if (params1 [i].IsIn != params2 [i].IsIn) {
						Verifier.Log.Write ("error", "[in] mismatch.");
						res = false;
						if (Verifier.stopOnError) break;
					}

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
						Verifier.Log.Write ("error", "[in] mismatch.");
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
				Verifier.Log.Write ("error", String.Format ("There is no such method {0}.", mi1.Name));
				return false;
			}

			Verifier.Log.Write ("method", String.Format ("{0}.", mi1.Name));
			bool res = true;
			bool ok;

			ok = mi1.ReturnType.Equals (mi2.ReturnType);
			res &= ok;
			if (!ok) {
				Verifier.Log.Write ("error", "Return types mismatch.");
				if (Verifier.stopOnError) return res;
			}

			ok = Compare.Parameters (mi1.GetParameters (), mi2.GetParameters ());
			res &= ok;
			if (!ok && Verifier.stopOnError) return res;

			ok = (mi1.CallingConvention == mi2.CallingConvention);
			res &= ok;
			if (!ok) {
				Verifier.Log.Write ("error", "Calling conventions mismatch.");
				if (Verifier.stopOnError) return res;
			}

			return res;
		}


		public static bool Fields (FieldInfo fi1, FieldInfo fi2)
		{
			if (fi2 == null) {
				Verifier.Log.Write ("error", String.Format ("There is no such field {0}.", fi1.Name));
				return false;
			}

			bool res = true;
			bool ok;

			Verifier.Log.Write ("field", String.Format ("{0}.", fi1.Name));

			ok = (fi1.IsPrivate == fi2.IsPrivate);
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

		public static Log log = new Log ();
		public static bool stopOnError = false;
		public static bool checkOptionalFlags = true;


		private Verifier ()
		{
		}

		public static Log Log {
			get {
				return log;
			}
		}



		public static void Main (String [] args) {
			if (args.Length < 2) {
				Console.WriteLine ("Usage: verifier asm1 asm2");
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

