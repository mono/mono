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

		public XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement assemblyElem = doc.CreateElement("assembly");
			assemblyElem.SetAttribute("name", this.Name);
			assemblyElem.SetAttribute("missing", this.MissingCount.ToString());
			assemblyElem.SetAttribute("todo", this.ToDoCount.ToString());
			assemblyElem.SetAttribute("complete", (100 - 100 * (this.MissingCount + this.ToDoCount) / this.ReferenceTypeCount).ToString());

			if (todoNameSpaces.Count > 0)
			{
				XmlElement eltNamespaces = doc.CreateElement ("namespaces");
				assemblyElem.AppendChild (eltNamespaces);

				foreach (ToDoNameSpace ns in todoNameSpaces)
				{
					XmlElement eltNameSpace = ns.CreateXML (doc);
					if (eltNameSpace != null)
						eltNamespaces.AppendChild (eltNameSpace);
				}
			}
			return assemblyElem;
		}

		public void CreateXMLReport(string filename) {
			bool analyzedOK = Analyze();

			XmlDocument outDoc;
			outDoc = new XmlDocument();
			outDoc.AppendChild(outDoc.CreateXmlDeclaration("1.0", null, null));
			XmlElement assembliesElem = outDoc.CreateElement("assemblies");
			outDoc.AppendChild(assembliesElem);
			XmlElement assemblyElem = CreateXML (outDoc);
			assembliesElem.AppendChild(assemblyElem);
			outDoc.Save(filename);
		}
	}
}
