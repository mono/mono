using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using Mono.Doc.Utils;

namespace Mono.Doc.StatusGen
{
	public class StatusGenerator
	{
		private const string DEFAULT_MAINTAINER = "mono-list@ximian.com";

		private Assembly    statusAssem;
		private Assembly    diffAssem;
		private XmlDocument outDoc;
		private XmlDocument classDoc;
		private string      outFile;

		public StatusGenerator(string statusAssemName, string diffAssemName, string classFile, string outFile)
		{
			if (statusAssemName == null) {
				throw new ArgumentNullException(
					"statusAssemName",
					"An assembly to process must be specified."
				);
			}

			if (outFile == null) {
				throw new ArgumentNullException(
					"outFile",
					"An XML output file must be specified."
				);
			}

			if (classFile == null) {
				throw new ArgumentNullException(
					"classFile",
					"An XML file with maintainer information must be specified."
				);
			}

			this.statusAssem = AssemblyLoader.Load(statusAssemName);
			this.outFile     = outFile;
			outDoc           = new XmlDocument();
			classDoc         = new XmlDocument();

			try {
				classDoc.Load(classFile);
			} catch {
				throw new ArgumentException(
					"classFile",
					"Unable to load class information XML file: " + classFile
				);
			}

			if (diffAssemName != null) {
				this.diffAssem = AssemblyLoader.Load(diffAssemName);
			}
		}


		public void Create()
		{
			outDoc.AppendChild(outDoc.CreateXmlDeclaration("1.0", null, null));

			// <assembly>
			XmlElement assemblyElem = outDoc.CreateElement("assembly");
			assemblyElem.SetAttribute("name", statusAssem.GetName().Name);
			outDoc.AppendChild(assemblyElem);

			// <types>
			XmlElement typesElem = outDoc.CreateElement("types");
			assemblyElem.AppendChild(typesElem);

			// <type>
			Type[] types = statusAssem.GetTypes();
			foreach (Type type in types) {
				if (type.IsPublic) {
					XmlElement typeElem = CreateTypeElement(type);
					typesElem.AppendChild(typeElem);
				}
			}

			// <missing-types>
			XmlElement missingTypesElem = CreateMissingTypesElement();
			if (missingTypesElem != null) {
				assemblyElem.AppendChild(missingTypesElem);
			}

			// save
			try {
				outDoc.Save(new StreamWriter(outFile));
			} catch (Exception e) {
				throw new ApplicationException("Unable to write to '" + outFile + "'.", e);
			}
		}


		#region Private Implementation Methods

		private XmlElement CreateTypeElement(Type t)
		{
			XmlElement typeElem = outDoc.CreateElement("type");
			typeElem.SetAttribute("name", t.FullName);
			typeElem.SetAttribute("kind", NameGenerator.GetKindNameForType(t));

			// information from class info file
			XmlNode   classNode    = classDoc.SelectSingleNode("/classes/class[@name='"+ t.FullName + "']");
			ArrayList maintainers  = new ArrayList();
			bool      hasTestSuite = false;

			if (classNode != null) {
				XmlNodeList maintNodes = classNode.SelectNodes("maintainers/maintainer");
				foreach (XmlNode maintNode in maintNodes) {
					maintainers.Add(maintNode.InnerText);
				}

				XmlNode testSuiteNode = classNode.SelectSingleNode("test-suite");
				if (testSuiteNode != null) {
					string testText = testSuiteNode.InnerText;
					hasTestSuite = testText == "yes" ? true : false;
				} else {
					// redundant
					hasTestSuite = false;
				}
			}

			if (maintainers.Count == 0) {
				maintainers.Add(DEFAULT_MAINTAINER);
			}

			// <extended-info>
			XmlElement infoElem = outDoc.CreateElement("extended-info");
			typeElem.PrependChild(infoElem);

			foreach (string maintainer in maintainers) {
				infoElem.AppendChild(CreateItem("maintainer", maintainer));
			}

			infoElem.AppendChild(CreateItem("test-suite", hasTestSuite.ToString().ToLower()));

			// todo
			bool complete = true;

			// class may be marked
			if (HasTodo(t)) {
				complete = false;
			}

			// now check members
			BindingFlags flags    =
				BindingFlags.Instance     | 
				BindingFlags.Static       | 
				BindingFlags.Public       | 
				BindingFlags.DeclaredOnly;

			ArrayList incompleteMembers = new ArrayList();
			int       total             = 0;

			// methods
			MethodInfo[] methods = t.GetMethods(flags);

			foreach (MethodInfo method in methods) {
				total++;

				if (HasTodo(method)) {
					incompleteMembers.Add(
						NameGenerator.GetNameForMemberInfo(method, NamingFlags.ForceMethodParams)
					);
				}
			}

			// write out all the incomplete members
			if (incompleteMembers.Count > 0) {
				complete            = false;
				XmlElement todoElem = outDoc.CreateElement("todo");

				int pctComplete = Convert.ToInt32(
					((float) (total - incompleteMembers.Count) / (float) total) *100f);

				todoElem.SetAttribute("percent", pctComplete.ToString());

				incompleteMembers.Sort();
			
				foreach (string memberName in incompleteMembers) {
					XmlElement memberElem = outDoc.CreateElement("member");
					memberElem.SetAttribute("name", memberName);
					todoElem.AppendChild(memberElem);
				}

				typeElem.AppendChild(todoElem);
			}

			typeElem.SetAttribute("complete", complete.ToString().ToLower());

			return typeElem;
		}

		
		private bool HasTodo(MemberInfo m) {
			object[] attrs = null;

			try {
				attrs = m.GetCustomAttributes(false);
			} catch (CustomAttributeFormatException) {
				Console.Error.WriteLine("WARNING: Unable to look for MonoTODO in " +
					NameGenerator.GetNameForMemberInfo(m,
						NamingFlags.TypeSpecifier | NamingFlags.FullName));
				return false;
			}

			foreach (object attr in attrs) {
				if (attr.GetType().FullName == "System.MonoTODOAttribute") {
					return true;
				}
			}

			return false;
		}

		
		private XmlElement CreateItem(string key, string value)
		{
			XmlElement itemElem = outDoc.CreateElement("item");
			itemElem.SetAttribute("key", key);
			itemElem.SetAttribute("value", value);

			return itemElem;
		}


		private XmlElement CreateMissingTypesElement()
		{
			if (diffAssem != null) {
				Type[] diffTypes            = diffAssem.GetTypes();
				ArrayList missingTypes      = new ArrayList();
				XmlElement missingTypesElem = null;

				foreach (Type type in diffTypes) {
					if (type.IsPublic) {
						string name = type.FullName;
						if (statusAssem.GetType(name) == null) {
							missingTypes.Add(name);
						}
					}
				}

				if (missingTypes.Count > 0) {
					missingTypesElem = outDoc.CreateElement("missing-types");
					missingTypes.Sort();

					foreach (string typeName in missingTypes) {
						XmlElement typeElem = outDoc.CreateElement("type");
						typeElem.SetAttribute("name", typeName);
						missingTypesElem.AppendChild(typeElem);
					}
				}

				return missingTypesElem;
			} else {
				return null;
			}
		}

		#endregion
	}
}
