//
// verifier.cs: compares two assemblies and reports differences.
//
// Author:
//   Sergey (serge@wildwestsoftware.com)
//
// (C) Sergey (serge@wildwestsoftware.com)
//
using System;
using System.IO;
using System.Collections;
using System.Reflection;

namespace Mono.Verifier {

	public class MethodCollection : IEnumerable {

		private SortedList methods;

		public MethodCollection (Type type)
		{
			this.methods = new SortedList ();

			MethodInfo [] methods = type.GetMethods ();
			foreach (MethodInfo mi in methods) {
				this.methods [mi.Name] = mi;
			}
		}


		public IEnumerator GetEnumerator() {
			return new Iterator (this);
		}


		public MethodInfo this [string name] {
			get {
				return methods [name] as MethodInfo;
			}
		}

		internal class Iterator : IEnumerator  {
			private MethodCollection host;
			private int pos;

			internal Iterator (MethodCollection host)
			{
				this.host=host;
				this.Reset ();
			}

			/// <summary></summary>
			public Object Current
			{
				get {
					if (host != null && pos >=0 && pos < host.methods.Count) {
						return host.methods.GetByIndex (pos);
					} else {
						return null;
					}
				}
			}

			/// <summary></summary>
			public bool MoveNext ()
			{
				if (host!=null) {
					return (++pos) < host.methods.Count;
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

	public class ClassCollection : SortedList {

		public ClassCollection ()
		{
		}

		public ClassCollection (string assemblyName) : this ()
		{
			LoadFrom (assemblyName);
		}

		public void LoadFrom (string assemblyName)
		{
			try {
				Assembly asm = Assembly.LoadFrom (assemblyName);
				Type [] publics = asm.GetExportedTypes ();
				foreach (Type type in publics) {
					if (type.IsClass) {
						this [type.FullName] = new MethodCollection (type);
					}
				}
			} catch (ReflectionTypeLoadException rtle) {
				Type [] loaded = rtle.Types;
				for (int i = 0; i < loaded.Length; i++) {
					Console.Error.WriteLine ("Unable to load {0}, reason - {1}", loaded [i], rtle.LoaderExceptions [i]);
				}
			} catch (FileNotFoundException fnfe) {
					Console.Error.WriteLine (fnfe);
			} catch (Exception x) {
					Console.Error.WriteLine (x);
			}
		}



		private static bool CompareParameters (ParameterInfo[] params1, ParameterInfo[] params2)
		{
			bool res = true;
			if (params1.Length != params2.Length) {
				Console.WriteLine ("Parameter count mismatch.");
				return false;
			}

			int count = params1.Length;

			for (int i = 0; i < count && res; i++) {
				if (params1 [i].Name != params2 [i].Name) {
					Console.WriteLine ("Parameters names mismatch {0}, {1}.", params1 [i].Name, params2 [i].Name);
					res = false;
					break;
				}

				if (params1 [i].ParameterType != params2 [i].ParameterType) {
					Console.WriteLine ("Parameters types mismatch {0}, {1}.", params1 [i].ParameterType, params2 [i].ParameterType);
					res = false;
					break;
				}
			}

			return res;
		}



		private static void Verify (string assembly1, string assembly2)
		{
			ClassCollection classes1 = new ClassCollection (assembly1);
			ClassCollection classes2 = new ClassCollection (assembly2);

			foreach (DictionaryEntry c in classes1) {
				string className = c.Key as string;
				Console.WriteLine ("class: " + className);
				if (classes2 [className] == null) {
					Console.WriteLine ("There is no such class in {0}", assembly2);
					Environment.Exit (-1);
				}

				MethodCollection methods2 = classes2 [className] as MethodCollection;

				foreach (MethodInfo method in (c.Value as MethodCollection)) {
					Console.WriteLine ("\t method: " + method);
					MethodInfo m2 = methods2 [method.Name];
					if (m2 == null) {
						Console.WriteLine ("There is no such method in {0} ({1})", className, assembly2);
						Environment.Exit (-1);
					}
					bool paramsIndentical = CompareParameters (method.GetParameters (), m2.GetParameters ());
					if (!paramsIndentical) {
						Environment.Exit (-1);
					}
				}
			}
		}



		public static void Main (String [] args) {
			if (args.Length < 2) {
				Console.WriteLine ("Usage: verifier asm1 asm2");
			} else {
				string asm1 = args [0];
				string asm2 = args [1];
				Verify (asm1, asm2);
			}
		}

	}

}

