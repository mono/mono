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

		public int ToDoCount {
			get {
				int sum = 0;
				foreach(ToDoNameSpace ns in todoNameSpaces) {
					sum += ns.ToDoCount;
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
			assemblyElem.SetAttribute("todo", this.ToDoCount.ToString());
			assemblyElem.SetAttribute("complete", (100 - 100 * (this.MissingCount + this.ToDoCount) / this.ReferenceTypeCount).ToString());
			assembliesElem.AppendChild(assemblyElem);
			XmlElement namespacesElem = outDoc.CreateElement("namespaces");
			assemblyElem.AppendChild(namespacesElem);

			if (analyzedOK && todoNameSpaces.Count > 0) {
				XmlElement namespaceElem;
				XmlElement classesElem;
				XmlElement classElem;
				XmlElement memberElem = null;
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
							classElem.SetAttribute("missing", type.MissingCount.ToString());
							classElem.SetAttribute("todo", type.ToDoCount.ToString());
							classesElem.AppendChild(classElem);
							
							memberElem = CreateMemberCollectionElement("methods", type.MissingMethods, type.ToDoMethods, outDoc);
							if (memberElem != null) {
								classElem.AppendChild(memberElem);
							}

							memberElem = CreateMemberCollectionElement("properties", type.MissingProperties, type.ToDoProperties, outDoc);
							if (memberElem != null) {
								classElem.AppendChild(memberElem);
							}

							memberElem = CreateMemberCollectionElement("events", type.MissingEvents, type.ToDoEvents, outDoc);
							if (memberElem != null) {
								classElem.AppendChild(memberElem);
							}

							memberElem = CreateMemberCollectionElement("fields", type.MissingFields, type.ToDoFields, outDoc);
							if (memberElem != null) {
								classElem.AppendChild(memberElem);
							}

							memberElem = CreateMemberCollectionElement("constructors", type.MissingConstructors, type.ToDoConstructors, outDoc);
							if (memberElem != null) {
								classElem.AppendChild(memberElem);
							}

							memberElem = CreateMemberCollectionElement("nestedTypes", type.MissingNestedTypes, type.ToDoNestedTypes, outDoc);
							if (memberElem != null) {
								classElem.AppendChild(memberElem);
							}
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

		static XmlElement CreateMemberCollectionElement(string name, ArrayList missingList, ArrayList todoList, XmlDocument doc) {
			XmlElement element = null;
			if (missingList.Count > 0) {
				element = doc.CreateElement(name);
				foreach (IMissingMember missing in missingList) {
					element.AppendChild(CreateMissingElement(missing, doc));
				}
			}
			if (todoList.Count > 0) {
				if (element == null) {
					element = doc.CreateElement(name);
				}
				foreach (IMissingMember missing in todoList) {
					element.AppendChild(CreateMissingElement(missing, doc));
				}
			}
			if (element != null) {
				element.SetAttribute("missing", missingList.Count.ToString());
				element.SetAttribute("todo", todoList.Count.ToString());
			}
			return element;
		}

		static XmlElement CreateMissingElement(IMissingMember member, XmlDocument doc) {
			XmlElement missingElement  = doc.CreateElement(member.Type);
			missingElement.SetAttribute("name", (member.Name));
			missingElement.SetAttribute("status", (member.Status));
			return missingElement;
		}
	}
}
