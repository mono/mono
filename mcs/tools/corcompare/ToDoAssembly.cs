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
	class ToDoAssembly : MissingBase
	{
		// these types are in mono corlib, but not in the dll we are going to examine.
		ArrayList MissingTypes = new ArrayList();
		string assemblyToCompare, assemblyToCompareWith;
		bool analyzed = false;
		ArrayList rgNamespaces = new ArrayList();
		string name;

		CompletionInfo ci;

		public ToDoAssembly(string fileName, string friendlyName, string strMSName)
		{
			assemblyToCompare = fileName;
			name = friendlyName;
			assemblyToCompareWith = strMSName;
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override string Type
		{
			get { return "assembly"; }
		}

		public override CompletionTypes Completion
		{
			get { return CompletionTypes.Todo; }
		}

		public CompletionInfo Analyze ()
		{
			if (analyzed)
				return ci;

			Type[] mscorlibTypes = GetReferenceTypes();
			if (mscorlibTypes == null)
				throw new Exception ("Could not find corlib file: " + name);

			Type[] monocorlibTypes = GetMonoTypes();
			if (monocorlibTypes == null)
				throw new Exception ("Failed to load Mono assembly: " + assemblyToCompare);

			ArrayList rgNamespacesMono = ToDoNameSpace.GetNamespaces(monocorlibTypes);
			if (rgNamespacesMono == null)
				throw new Exception ("Failed to get namespaces from Mono assembly: " + assemblyToCompare);

			foreach(string ns in rgNamespacesMono) {
				if (ns != null && ns.Length != 0)
				{
					ToDoNameSpace tdns = new ToDoNameSpace(ns, mscorlibTypes, monocorlibTypes);
					rgNamespaces.Add (tdns);
					CompletionInfo ciNS = tdns.Analyze ();
					ci.cComplete += ciNS.cComplete;
					ci.cMissing += ciNS.cMissing;
					ci.cTodo += ciNS.cTodo;
				}
			}

			ArrayList rgNamespacesMS = ToDoNameSpace.GetNamespaces(mscorlibTypes);
			if (rgNamespacesMS == null)
				throw new Exception ("Failed to get namespaces from Microsoft assembly: " + assemblyToCompareWith);

			foreach (string nsMS in rgNamespacesMS)
			{
				if (nsMS != null && nsMS.Length != 0)
				{
					bool fMissing = true;
					foreach (string nsMono in rgNamespacesMono)
					{
						if (nsMono == nsMS)
						{
							fMissing = false;
							break;
						}
					}
					if (fMissing && !nsMS.StartsWith ("Microsoft."))
					{
						MissingNameSpace mns = new MissingNameSpace (nsMS, mscorlibTypes);
						rgNamespaces.Add (mns);
					}
				}
			}

			analyzed = true;
			return ci;
		}

		public Type[] GetReferenceTypes()
		{
			try
			{
				// get the types in the corlib we are running with
				//Assembly msAsmbl = Assembly.GetAssembly(typeof (System.Object));
				Assembly assembly = Assembly.Load (assemblyToCompareWith);
				Type[] mscorlibTypes = assembly.GetTypes();
				return mscorlibTypes;
			}
			catch (Exception)
			{
				return null;
			}
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
			Analyze ();	// TODO: catch exception
			if (rgNamespaces.Count == 0) return "";

			StringBuilder output = new StringBuilder();
			foreach (MissingNameSpace ns in rgNamespaces)
			{
				string[] missingTypes = ns.MissingTypeNames(true);
				if (missingTypes != null && missingTypes.Length > 0) {
					string joinedNames = String.Join("\n", missingTypes);
					output.Append(joinedNames + "\n");
				}
			}
			return output.ToString();
		}

		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement assemblyElem = base.CreateXML (doc);
			ci.SetAttributes (assemblyElem);

			if (rgNamespaces.Count > 0)
			{
				XmlElement eltNamespaces = doc.CreateElement ("namespaces");
				assemblyElem.AppendChild (eltNamespaces);

				foreach (MissingNameSpace ns in rgNamespaces)
				{
					XmlElement eltNameSpace = ns.CreateXML (doc);
					if (eltNameSpace != null)
						eltNamespaces.AppendChild (eltNameSpace);
				}
			}
			return assemblyElem;
		}

		public void CreateXMLReport(string filename) {
			Analyze();	// TODO: catch exception

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
