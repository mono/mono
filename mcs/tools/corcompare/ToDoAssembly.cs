// Mono.Util.CorCompare.ToDoAssembly
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents an assembly that has missing or MonoTODO classes
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class ToDoAssembly
	{
		// these types are in mono corlib, but not in the dll we are going to examine.
		static string[] ghostTypes = {"System.Object", "System.ValueType", "System.Delegate", "System.Enum"};
		ArrayList MissingTypes = new ArrayList();
		string assemblyToCompare;
		bool analyzed = false;
		ArrayList todoNameSpaces = new ArrayList();
		string name;

		public ToDoAssembly(string fileName, string friendlyName)
		{
			assemblyToCompare = fileName;
			name = friendlyName;
		}

		public string Name {
			get {
				return name;
			}
		}

		public int MissingCount {
			get {
				int sum = 0;
				foreach(ToDoNameSpace ns in todoNameSpaces) {
					sum += ns.MissingCount;
				}
				return sum;
			}
		}

		public int ReferenceTypeCount {
			get {
				int sum = 0;
				foreach(ToDoNameSpace ns in todoNameSpaces) {
					sum += ns.ReferenceTypeCount;
				}
				return sum;
			}
		}

		bool Analyze()
		{
			if (analyzed) return true;

			Type[] mscorlibTypes = GetReferenceTypes();
			if (mscorlibTypes == null)
			{
				Console.WriteLine("Could not find corlib file: {0}", assemblyToCompare);
				return false;
			}

			Type[] monocorlibTypes = GetMonoTypes();

			foreach(string ns in ToDoNameSpace.GetNamespaces(monocorlibTypes)) {
				todoNameSpaces.Add(new ToDoNameSpace(ns, monocorlibTypes, mscorlibTypes));
			}

			analyzed = true;
			return true;
/* ///////////////////////////////////////////////////////////////////////////////// stuff below here needs to be moved into classes above.

			ArrayList TypesList = new ArrayList();

			// load the classes we know should exist, but aren't in the dll
			foreach (string name in ghostTypes){
				TypesList.Add(name);
			}

			Hashtable monoMethodMap = new Hashtable();
			MethodInfo[] methods;
			ArrayList nameList = new ArrayList();
			// whether GetTypes() worked or not, we will have _some_ types here
			foreach(Type subt in monocorlibTypes)
			{
				if (null != subt && !TypesList.Contains(subt.FullName)) {
					TypesList.Add(subt.FullName);

					methods = subt.GetMethods();

					nameList = GetMungedNames(methods);

					monoMethodMap.Add(subt.FullName, nameList);
				}
			}

			// going to use BinarySearch, so sort first
			TypesList.Sort();
			
			ArrayList PartialClasses = new ArrayList();
			ArrayList MissingMethods = new ArrayList();
			bool foundit = false;

			// make list of ms types not in mono
			foreach(Type t in mscorlibTypes) {
				if (t.IsPublic) {
					foundit = (TypesList.BinarySearch(t.FullName) >= 0);
					if (foundit) 
					{
						// look for missing members
						nameList = GetMungedNames(t.GetMethods());
						foreach (string name in nameList)
						{
							ArrayList monoNames = (ArrayList)monoMethodMap[t.FullName];
							if (monoNames != null)
							{
								monoNames.Sort();
								if (monoNames.BinarySearch(name) < 0)
								{
									MissingMethods.Add(t.FullName + "::" + name);

									if (!PartialClasses.Contains(t.FullName))
									{
										PartialClasses.Add(t.FullName);
									}
									
								}

							}
						}
					}
					else
					{
						MissingTypes.Add(t.FullName);
					}
				}
			}
			foreach (string pc in PartialClasses)
			{
				int missingInClass = GetMethodCount(MissingMethods, pc);
			}
*/
		}

		public Type[] GetReferenceTypes()
		{
			// get the types in the corlib we are running with
			Assembly msAsmbl = Assembly.GetAssembly(typeof (System.Object));
			Type[] mscorlibTypes = msAsmbl.GetTypes();
			return mscorlibTypes;
		}

		public Type[] GetMonoTypes()
		{
			Type[] monocorlibTypes;
			Assembly monoAsmbl = null;
			try
			{
				monoAsmbl = Assembly.LoadFrom(assemblyToCompare);
			}
			catch(FileNotFoundException)
			{
				return null;
			}

			monocorlibTypes = monoAsmbl.GetTypes();

			return monocorlibTypes;
		}

		public string CreateClassListReport() {
			if (!Analyze() || todoNameSpaces.Count == 0) return "";

			StringBuilder output = new StringBuilder();
			foreach (ToDoNameSpace ns in todoNameSpaces)
			{
				string[] missingTypes = ns.MissingTypeNames(true);
				if (missingTypes.Length > 0) {
					string joinedNames = String.Join("\n", missingTypes);
					output.Append(joinedNames + "\n");
				}
			}
			return output.ToString();
		}

		public void CreateXMLReport(string filename) {
			bool analyzedOK = Analyze();

			XmlDocument outDoc;
			outDoc = new XmlDocument();
			outDoc.AppendChild(outDoc.CreateXmlDeclaration("1.0", null, null));
			XmlElement assembliesElem = outDoc.CreateElement("assemblies");
			outDoc.AppendChild(assembliesElem);
			XmlElement assemblyElem = outDoc.CreateElement("assembly");
			assemblyElem.SetAttribute("name", this.Name);
			assemblyElem.SetAttribute("missing", this.MissingCount.ToString());
			assemblyElem.SetAttribute("complete", (100 - 100 * this.MissingCount / this.ReferenceTypeCount).ToString());
			assembliesElem.AppendChild(assemblyElem);
			XmlElement namespacesElem = outDoc.CreateElement("namespaces");
			assemblyElem.AppendChild(namespacesElem);

			if (analyzedOK && todoNameSpaces.Count > 0) {
				XmlElement namespaceElem;
				XmlElement classesElem;
				XmlElement classElem;
				foreach (ToDoNameSpace ns in todoNameSpaces) {
					namespaceElem = outDoc.CreateElement("namespace");
					namespaceElem.SetAttribute("name", ns.name);
					MissingType[] missingTypes = ns.MissingTypes;
					classesElem = null;
					if (missingTypes.Length > 0) {
						classesElem = outDoc.CreateElement("classes");
						namespaceElem.AppendChild(classesElem);

						foreach (MissingType type in missingTypes) {
							classElem = outDoc.CreateElement("class");
							classElem.SetAttribute("name", type.Name);
							classElem.SetAttribute("status", type.Status);
							classesElem.AppendChild(classElem);
						}

						namespaceElem.SetAttribute("missing", ns.MissingCount.ToString());
					}

					ToDoType[] todoTypes = ns.ToDoTypes;
					if (todoTypes.Length > 0) {
						if (classesElem == null) {
							classesElem = outDoc.CreateElement("classes");
							namespaceElem.AppendChild(classesElem);
						}
						foreach (ToDoType type in todoTypes) {
							classElem = outDoc.CreateElement("class");
							classElem.SetAttribute("name", type.Name);
							classElem.SetAttribute("status", type.Status);
							classesElem.AppendChild(classElem);
						}
						namespaceElem.SetAttribute("todo", ns.ToDoCount.ToString());
					}
					if (ns.ReferenceTypeCount > 0) {
						namespaceElem.SetAttribute("complete", (100 - 100 * (ns.MissingCount + ns.ToDoCount) / ns.ReferenceTypeCount).ToString());
					}
					else {
						namespaceElem.SetAttribute("complete", "100");
					}
					namespacesElem.AppendChild(namespaceElem);
				}
			}
			
			outDoc.Save(filename);
		}

		static int GetMethodCount(ArrayList methods, string className) {
			int count = 0;
			foreach (string method in methods) {
				// starts with is not enough. for instance, they all start with "System"
				if (method.StartsWith(className)) {
					count++;
				}
			}
			return count;
		}
			
		static int GetClassCount(ArrayList types, string ns) {
			int count = 0;
			foreach (string type in types) {
				// starts with is not enough. for instance, they all start with "System"
				if (type.StartsWith(ns+".") && type.IndexOf(".", ns.Length+1) < 0) {
					count++;
				}
			}
			return count;
		}

		static int GetMSClassCount(Type[] types, string ns)
		{
			int count = 0;
			for (int i=0; i < types.Length; i++)
			{
				if (types[i].Namespace == ns)
				{
					count++;
				}
			}
			return count;
		}

		static ArrayList GetMungedNames(MethodInfo[] methods)
		{
			ArrayList nameList = new ArrayList();
			foreach(MethodInfo method in methods)
			{
				StringBuilder methodMungedName = new StringBuilder(method.Name + "(");
				ParameterInfo[] parameters = method.GetParameters();
				ArrayList parameterTypes = new ArrayList();
				foreach(ParameterInfo parameter in parameters)
				{
					parameterTypes.Insert(parameter.Position, parameter.ParameterType.Name);
				}
						
				foreach(string parameterTypeName in parameterTypes)
				{
					if (!methodMungedName.ToString().EndsWith("("))
						methodMungedName.Append(",");
					methodMungedName.Append(parameterTypeName);
				}

				methodMungedName.Append(")");
				nameList.Add(methodMungedName.ToString());
			}

			return nameList;
		}
	}
}
