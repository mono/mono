using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;


namespace Mono.Utils.Todo
{
	class TodoMain
	{
		private static string DEFAULT_FILE    = "out.xml";
		private static string INTERNAL_PREFIX = "int";


		static void Main(string[] args)
		{
			int argc = args.Length;

			if (argc < 1) {
				Usage();
				return;
			}
			
			string maintainersFile  = null;
			string outFile          = null;
			string assembly         = null;
			string diffAssemblyName = null;

			for (int i = 0; i < args.Length; i++) {
				string arg = args[i];
				
				if (arg.StartsWith("-")) {
					switch (arg) {
						case "--maint":
							if ((i + 1) >= argc) {
								Usage();
								return;
							}
							
							maintainersFile = args[++i];
							continue;
						
						case "--out":
							if ((i + 1) >= argc) {
								Usage();
								return;
							}

							outFile = args[++i];
							continue;
						
						case "--diff":
							if ((i + 1) >= argc) {
								Usage();
								return;
							}

							diffAssemblyName = args[++i];
							continue;

						default:
							Usage();
							return;
					}
				}

				assembly = arg;
				break;
			}
			
			if (outFile == null) {
				outFile = DEFAULT_FILE;
			}

			if (assembly == null) {
				Usage();
				return;
			}

			Hashtable maintainers = null;

			if (maintainersFile != null) {
				maintainers = LoadMaintainersFromXml(maintainersFile);
			}

			try {
				Assembly monoAssem = LoadAssemblyFromString(assembly);
				Assembly diffAssem = null;

				if (diffAssemblyName != null) {
					diffAssem = LoadAssemblyFromString(diffAssemblyName);
				}

				CreateTODOXml(monoAssem, diffAssem, maintainers, outFile);
			} catch (ApplicationException ae) {
				Console.WriteLine("ERROR: " + ae.Message);
				return;
			}
		}


		private static void CreateTODOXml(
			Assembly  monoAssem,
			Assembly  diffAssem,
			Hashtable maint, 
			string    file)
		{
			BindingFlags flags =
				BindingFlags.Instance     |
				BindingFlags.Static       |
				BindingFlags.Public       |
				BindingFlags.NonPublic    |
				BindingFlags.DeclaredOnly;
			
			// output the XML
			XmlTextWriter w = null;

			try {
				w = new XmlTextWriter(new StreamWriter(file));
				w.Formatting  = Formatting.Indented;
				w.Indentation = 4;

				w.WriteStartDocument();

				// <assembly>
				w.WriteStartElement("assembly");
				w.WriteAttributeString("name", monoAssem.GetName().Name);
				
				// <incomplete-types>
				w.WriteStartElement("incomplete-types");

				ArrayList monoTypeNames = new ArrayList();

				foreach (Type t in monoAssem.GetTypes()) {
					int       totalMembers      = 0;
					ArrayList unfinishedMembers = new ArrayList();

					monoTypeNames.Add(t.FullName);

					foreach (MemberInfo m in t.GetMembers(flags)) {
						totalMembers++;
						
						// FIXME:
						// due to the fact that this utility can't be compiled
						// without Microsoft's corlib, we can't reference the
						// MonoTODOAttribute class in Mono corlib.
						// So, while we can see that a member has TODO
						// attributes, we can't actually retrieve the comments.

						object[]  attrs      = m.GetCustomAttributes(false);
						bool      incomplete = false;

						foreach (object attr in attrs) {
							Type attrType = attr.GetType();
							if (attrType.FullName == "System.MonoTODOAttribute") {
								incomplete = true;
								break;
							}
						}

						// if the member has todos, add it to the unfinishedMembers
						if (incomplete) {
							UnfinishedMember mem = new UnfinishedMember(GetNameForMemberInfo(m, false));
							mem.Type             = GetTypeNameForMemberInfo(m);
							unfinishedMembers.Add(mem);
						}
					}


					if (unfinishedMembers.Count > 0) {
						// <type>
						w.WriteStartElement("type");
						w.WriteAttributeString("name", t.Name);
						w.WriteAttributeString("namespace", t.Namespace);
						
						int pctComplete = Convert.ToInt32(
							((float) (totalMembers - unfinishedMembers.Count)
							/(float) totalMembers) * 100f);
						
						w.WriteAttributeString("percent", pctComplete.ToString());

						if (maint != null) {
							// <maintainers>
							w.WriteStartElement("maintainers");

							ArrayList maints = (ArrayList) maint[t.FullName];

							if (maints != null) {
								foreach (string email in maints) {
									// <maintainer>
									w.WriteStartElement("maintainer");
									w.WriteString(email);
									w.WriteEndElement();
									// </maintainers>
								}
							}

							w.WriteEndElement();
							// </maintainers>
						}

						// <todo>
						w.WriteStartElement("todo");

						foreach (UnfinishedMember mem in unfinishedMembers) {
							// <member>
							w.WriteStartElement("member");
							w.WriteAttributeString("name", mem.Name);
							w.WriteAttributeString("type", mem.Type);
							w.WriteEndElement();
							// </member>
						}
						
						w.WriteEndElement();
						// </todo>

						w.WriteEndElement();
						// </type>
					}
				}
			
				w.WriteEndElement();
				// </incomplete-types>

				if (diffAssem != null) {
					monoTypeNames.Sort();

					// <missing-types>
					w.WriteStartElement("missing-types");

					foreach (Type t in diffAssem.GetTypes()) {
						if (t.IsPublic && !(monoTypeNames.BinarySearch(t.FullName) >= 0)) {
							// <missing>
							w.WriteStartElement("missing");
							w.WriteString(t.FullName);
							w.WriteEndElement();
							// </missing>
						}
					}

					w.WriteEndElement();
					// </missing-types>
				}

				w.WriteEndElement();
				// </assembly>

				w.WriteEndDocument();
				w.Close();
			} catch {
				throw new ApplicationException("Problem writing to output file " + file);
			}
		}


		private static Hashtable LoadMaintainersFromXml(string file)
		{
			if (file == null) {
				// can't happen
				return null;
			}

			Hashtable h = new Hashtable();

			try {
				XmlDocument doc = new XmlDocument();
				doc.Load(file);

				XmlElement root    = doc.DocumentElement;
				XmlNodeList people = root.GetElementsByTagName("person");

				foreach (XmlElement person in people) {
					string email     = person.Attributes["email"].Value;
					XmlElement types = (XmlElement) person.GetElementsByTagName("types")[0];

					foreach (XmlElement type in types.GetElementsByTagName("type")) {
						string typeName = type.InnerText;

						if (h[typeName] == null) {
							h[typeName] = new ArrayList();
						}

						((ArrayList) h[typeName]).Add(email);
					}
				}
			} catch {
				Console.WriteLine("WARNING: Problem reading maintainers file '{0}'.", file);
				Console.WriteLine("         Continuing without maintainers.");
				return null;
			}
			return h;
		}
		
		
		private static Assembly LoadAssemblyFromString(string assemblyString)
		{
			// allows file names and int:assemblyname to load from memory
			Assembly a;

			if (assemblyString == null || assemblyString == string.Empty) {
				Usage();
				throw new ApplicationException("Syntax error.");
			}

			if (assemblyString.StartsWith(INTERNAL_PREFIX))  {
				// load it from the AppDomain
				string[] split = assemblyString.Split(new char[] {':'});

				if (split.Length != 2) {
					Usage();
					throw new ApplicationException("Syntax error: " + assemblyString);
				}

				string name       = split[1];
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
			Console.WriteLine(
				"Mono Assembly Todo-List Generator, Copyright (c) 2001 John Barnette\n" + 
				"todo [options] assemblyFile\n" +
				"   --diff ASSEMBLY  Compare with ASSEMBLY and document missing\n" +
				"                    types.  ASSEMBLY is either a file, or, if\n" +
				"                    specified in the format '{0}:ASSEMBLY', an\n" +
				"                    assembly to be loaded from the current AppDomain.\n" +
				"   --maint FILE     Load maintainer information from XML file.\n" +
				"   --out FILE       Specifies XML output file.  Default is {1}.\n\n",
				INTERNAL_PREFIX, DEFAULT_FILE
			);
		}


		private struct UnfinishedMember
		{
			public string   Name;
			public string   Type;


			public UnfinishedMember(string name)
			{
				this.Name = name;
				this.Type = null;
			}
		}
	}
}