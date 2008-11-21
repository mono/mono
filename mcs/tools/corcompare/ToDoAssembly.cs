// Mono.Util.CorCompare.ToDoAssembly
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using Mono.Cecil;

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
		ArrayList rgNamespaces = new ArrayList();
		string strName;
		AssemblyDefinition assMono;
		AssemblyDefinition assMS;
		TypeDefinitionCollection rgTypesMono;
		TypeDefinitionCollection rgTypesMS;

		protected static Hashtable htGhostTypes;
		private static string[] rgstrGhostTypes = {"System.Object", "System.ValueType", "System.Delegate", "System.Enum"};


		static ToDoAssembly ()
		{
			htGhostTypes = new Hashtable ();

			foreach (string strGhostType in rgstrGhostTypes)
			{
				htGhostTypes.Add (strGhostType, null);
			}
		}

		public static ToDoAssembly Load (string strFileMono, string strName, string strNameMS)
		{
			AssemblyDefinition assemblyMono = null;//TODO AssemblyDefinition.LoadFrom (strFileMono);
			AssemblyDefinition assemblyMS = null;//TODO AssemblyDefinition.LoadWithPartialName (strNameMS);

			return new ToDoAssembly (strName, assemblyMono, assemblyMS);
		}

		public ToDoAssembly (string _strName, AssemblyDefinition _assMono, AssemblyDefinition _assMS)
		{
			strName = _strName;
			assMono = _assMono;
			assMS = _assMS;

			rgTypesMono = assMono.MainModule.Types;
			rgTypesMS = assMS.MainModule.Types;
			m_nodeStatus = new NodeStatus (_assMono, _assMS);
		}

		public override string Name {
			get {
				return strName;
			}
		}

		public override string Type
		{
			get { return "assembly"; }
		}

		private Hashtable GetNamespaceMap (TypeDefinitionCollection rgTypes)
		{
			Hashtable mapTypes = new Hashtable ();
			foreach (TypeDefinition t in rgTypes)
			{
				if (t != null)
				{
					string strName = t.FullName;
					string strNamespace = t.Namespace;
					if (strNamespace != null && strNamespace.Length > 0 &&
						strName != null && strName.Length > 0 &&
						!htGhostTypes.Contains (strName))
					{
						TypeDefinitionCollection rgContainedTypes = (TypeDefinitionCollection) mapTypes [strNamespace];
						if (rgContainedTypes == null)
						{
							rgContainedTypes = new TypeDefinitionCollection (t.Module);
							mapTypes [strNamespace] = rgContainedTypes;
						}
						rgContainedTypes.Add (t);
					}
				}
			}
			return mapTypes;
		}

		public override NodeStatus Analyze ()
		{
			Hashtable mapTypesMono = GetNamespaceMap (rgTypesMono);
			Hashtable mapTypesMS = GetNamespaceMap (rgTypesMS);

			foreach (string strNamespaceMS in mapTypesMS.Keys)
			{
				if (strNamespaceMS != null)
				{
					TypeDefinitionCollection rgContainedTypesMS = (TypeDefinitionCollection) mapTypesMS [strNamespaceMS];
					TypeDefinitionCollection rgContainedTypesMono = (TypeDefinitionCollection) mapTypesMono [strNamespaceMS];
					MissingNameSpace mns = new MissingNameSpace (strNamespaceMS, rgContainedTypesMono, rgContainedTypesMS);
					NodeStatus nsNamespace = mns.Analyze ();
					m_nodeStatus.AddChildren (nsNamespace);
					if (rgTypesMono != null)
						mapTypesMono.Remove (strNamespaceMS);
					rgNamespaces.Add (mns);
				}
			}
			foreach (string strNamespaceMono in mapTypesMono.Keys)
			{
				if (strNamespaceMono != null)
				{
					TypeDefinitionCollection rgContainedTypesMono = (TypeDefinitionCollection) mapTypesMono [strNamespaceMono];
					MissingNameSpace mns = new MissingNameSpace (strNamespaceMono, rgContainedTypesMono, null);
					NodeStatus nsNamespace = mns.Analyze ();
					m_nodeStatus.AddChildren (nsNamespace);
					rgNamespaces.Add (mns);
				}
			}

			rgAttributes = new ArrayList ();
			NodeStatus nsAttributes = MissingAttribute.AnalyzeAttributes (
				assMono.CustomAttributes,
				assMS.CustomAttributes,
				rgAttributes);
			m_nodeStatus.Add (nsAttributes);

			return m_nodeStatus;
		}


		public string CreateClassListReport() {
			Analyze ();
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
			Analyze();

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
