using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;


namespace AsmDiff
{
	class AsmMain
	{
		private static string    DEFAULT_FILE    = "out.xml";
		private static string    INTERNAL_PREFIX = "int";


		static void Main(string[] args)
		{
			if (args.Length < 1) {
				Usage();
				return;
			}
			
			string outFile     = args.Length > 2 ? args[2] : DEFAULT_FILE;
			Assembly monoAssem = LoadAssemblyFromString(args[0]);

			CreateTODOXml(monoAssem);
		}


		private static void CreateTODOXml(Assembly monoAssem)
		{
			BindingFlags flags =
				BindingFlags.Instance     |
				BindingFlags.Static       |
				BindingFlags.Public       |
				BindingFlags.NonPublic    |
				BindingFlags.DeclaredOnly;
			
			foreach (Type t in monoAssem.GetTypes()) {
				int       totalMembers      = 0;
				ArrayList unfinishedMembers = new ArrayList();

				foreach (MemberInfo m in t.GetMembers(flags)) {
					totalMembers++;
					
					// HACK: load and check MonoTODO attrs with reflection
					// and introspection to make compilation easier.
					// this way, our corlib doesn't have to be referenced.

					ArrayList todos = new ArrayList();
					object[]  attrs = m.GetCustomAttributes(false);

					foreach (object attr in attrs) {
						Type attrType = attr.GetType();
						if (attrType.FullName == "System.MonoTODOAttribute") {
							PropertyInfo pi = attrType.GetProperty("Comment");
							
							todos.Add(pi == null ? "Null comment" : "Comment exists");
						}
					}

					// if the member has todos, add it to the unfinishedMembers
					if (todos.Count > 0) {
						UnfinishedMember mem = new UnfinishedMember(GetNameForMemberInfo(m, false));
						string[] todoMsgs    = new string[todos.Count];
						
						todos.CopyTo(todoMsgs);
						mem.Todo = todoMsgs;
						
						mem.Type = GetTypeNameForMemberInfo(m);
						unfinishedMembers.Add(mem);
					}
				}

				if (unfinishedMembers.Count > 0) {
					int pctComplete = Convert.ToInt32(
						((float) (totalMembers - unfinishedMembers.Count)
						/ (float) totalMembers) * 100f);
					
					
					Console.WriteLine("Type: {0} ({1}% complete)", t.Name, pctComplete);

					foreach (UnfinishedMember mem in unfinishedMembers) {
						Console.WriteLine("    Unfinished {0}: {1}", mem.Type, mem.Name);
						foreach (string todo in mem.Todo) {
							if (todo != null) {
								Console.WriteLine("        TODO: {0}", todo);
							}
						}
					}
				}
			}
		}


		private static Assembly LoadAssemblyFromString(string assemblyString)
		{
			// allows file names and int:assemblyname to load from memory
			Assembly a;

			if (assemblyString == null || assemblyString == string.Empty) {
				throw new ApplicationException("Syntax error.");
			}

			if (assemblyString.StartsWith(INTERNAL_PREFIX))  {
				// load it from the AppDomain
				string[] split = assemblyString.Split(new char[] {':'});

				if (split.Length != 2) {
					throw new ApplicationException("Syntax error: " + assemblyString);
				}

				string name = split[1];

				Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
				a = null;

				foreach (Assembly la in loaded) {
					if (la.GetName().Name == name) {
						a = la;
						break;
					}
				}

				if (a == null) {
					throw new ApplicationException("Could not load " + name + " from the current AppDomain.");
				}
			} else {
				// load it from a file
				try {
					a = Assembly.LoadFrom(assemblyString);
				} catch (FileNotFoundException) {
					throw new ApplicationException("Could not load assembly: " + assemblyString);
				}
			}

			return a;
		}


		private static string GetNameForMemberInfo(MemberInfo m, bool fullName)
		{
			StringBuilder name = new StringBuilder();
			name.Append((fullName ? m.DeclaringType.FullName + "." : "") + m.Name.Replace('.', '#'));
		
			// for methods and constructors, params are part of the name
			if (m is MethodBase) {
				MethodBase      method     = m as MethodBase;
				ParameterInfo[] parameters = method.GetParameters();
				
				name.Append("(");
				if (parameters.Length > 0) {
					bool first = true;
					foreach (ParameterInfo p in parameters) {
						if (!first) name.Append(",");
						name.Append(p.ParameterType.FullName);
						first = false;
					}
				}
				name.Append(")");
			}
			
			return name.ToString();
		}


		private static string GetTypeNameForMemberInfo(MemberInfo m)
		{
			if      (m is EventInfo)       return "event";
			else if (m is FieldInfo)       return "field";
			else if (m is MethodInfo)      return "method";
			else if (m is ConstructorInfo) return "constructor";
			else if (m is PropertyInfo)    return "property";
			else if (m is Type)            return "nested_type";
			else                           return "UNKNOWN";
		}

		
		private static void Usage()
		{
			// TODO: MEM:<assemblyName> loads the in-memory copy if available.
			Console.WriteLine("AsmDiff <monoAssembly> [outputFile]");
			Console.WriteLine();
		}


		private struct UnfinishedMember
		{
			public string   Name;
			public string   Type;
			public string[] Todo;


			public UnfinishedMember(string name)
			{
				this.Name = name;
				this.Type = null;
				this.Todo = null;
			}
		}
	}
}
