//
// Project.cs: Project class
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using Mono.XBuild.Framework;

namespace Microsoft.Build.BuildEngine {
	public class Project {
		static string separator = ";";
	
		bool				buildEnabled;
		IDictionary			conditionedProperties;
		string[]			defaultTargets;
		IList				directlyImportedProjects;
		Encoding			encoding;
		BuildPropertyGroup		environmentProperties;
		BuildItemGroup			evaluatedItems;
		BuildItemGroup			evaluatedItemsIgnoringCondition;
		IDictionary			evaluatedItemsByName;
		IDictionary			evaluatedItemsByNameIgnoringCondition;
		BuildPropertyGroup		evaluatedProperties;
		string				firstTargetName;
		string				fullFileName;
		BuildPropertyGroup		globalProperties;
		GroupingCollection		groups;
		bool				isDirty;
		bool				isValidated;
		bool				isReset;
		BuildItemGroupCollection	itemGroups;
		IDictionary			importedProjects;
		Engine				parentEngine;
		BuildPropertyGroupCollection	propertyGroups;
		BuildPropertyGroup		reservedProperties;
		string				schemaFile;
		TaskDatabase			taskDatabase;
		TargetCollection		targets;
		DateTime			timeOfLastDirty;
		IList				usingTaskElements;
		XmlDocument			xmlDocument;
		XmlElement			xmlElement;

		public Project ()
			: this (null)
		{
		}

		public Project (Engine engine)
		{
			parentEngine  = engine;
			xmlDocument = new XmlDocument ();
			evaluatedItems = new BuildItemGroup (this);
			evaluatedItemsByName = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			evaluatedItemsByNameIgnoringCondition = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			evaluatedItemsIgnoringCondition = new BuildItemGroup (this);
			evaluatedProperties = new BuildPropertyGroup (false, null);
			groups = new GroupingCollection ();
			itemGroups = new BuildItemGroupCollection (groups);
			propertyGroups = new BuildPropertyGroupCollection (groups);
			targets = new TargetCollection (this);
			usingTaskElements = new ArrayList ();
			taskDatabase = new TaskDatabase ();
		}

		[MonoTODO]
		public void AddNewImport (string importLocation,
					  string importCondition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BuildItem AddNewItem (string itemName,
					     string itemInclude)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BuildItemGroup AddNewItemGroup ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public BuildPropertyGroup AddNewPropertyGroup (bool insertAtEndOfProject)
		{
			throw new NotImplementedException ();
		}

		public bool Build (string[] targetNamesToBuild,
				   IDictionary targetOutputs)
		{
			if (targetNamesToBuild.Length == 0) {
				if (defaultTargets.Length != 0) {
					targetNamesToBuild = defaultTargets;
				}
				else if (firstTargetName != null) {
					targetNamesToBuild = new string [1] { firstTargetName};
				}
				else
					return false;
			}
			foreach (string target in targetNamesToBuild) {
				if (BuildTarget (target, targetOutputs) == false) {
					return false;
				}
			}
			return true;
		}

		public bool BuildTarget (string targetName,
					 IDictionary targetOutputs)
		{
			return BuildTargetWithFlags (targetName, targetOutputs, BuildSettings.None);
		}

		public bool BuildTargetWithFlags (string targetName,
						  IDictionary targetOutputs,
						  BuildSettings buildFlags)
		{
			if (targets.Exists (targetName) == false)
				throw new Exception ("Target specified to build does not exist.");
			
			this.targets [targetName].Build ();
			return true;
		}
		
		public string[] GetConditionedPropertyValues (string propertyName)
		{
			StringCollection sc = (StringCollection) conditionedProperties [propertyName];
			string[] propertyValues = new string [sc.Count];
			int i  = 0;
			foreach (string propertyValue in sc)
				propertyValues [i++] = propertyValue;
			return propertyValues;
		}

		public string[] GetDirectlyImportedProjects ()
		{
			string[] dip = new string [directlyImportedProjects.Count];
			int i = 0;
			foreach (string importedProject in directlyImportedProjects)
				dip [i++] = importedProject;
			return dip;
		}

		public BuildItemGroup GetEvaluatedItemsByName (string itemName)
		{
			return (BuildItemGroup) evaluatedItemsByName [itemName];
		}

		public BuildItemGroup GetEvaluatedItemsByNameIgnoringCondition (string itemName)
		{
			return (BuildItemGroup) evaluatedItemsByNameIgnoringCondition [itemName];
		}

		public string GetEvaluatedProperty (string propertyName)
		{
			return evaluatedProperties [propertyName];
		}

		[MonoTODO]
		public string[] GetNonImportedItemNames ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string[] GetNonImportedPropertyNames ()
		{
			throw new NotImplementedException ();
		}

		public string[] GetNonImportedTargetNames ()
		{
			ArrayList temporaryNonImportedTargets = new ArrayList ();
			foreach (Target target in targets)
				if (target.IsImported == false)
					temporaryNonImportedTargets.Add (target);
			string[] nonImportedTargetNames = new string [temporaryNonImportedTargets.Count];
			int i = 0;
			foreach (Target target in temporaryNonImportedTargets)
				nonImportedTargetNames [i++] = target.Name;
			return nonImportedTargetNames;
		}

		[MonoTODO]
		public string[] GetNonImportedUsingTasks ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetProjectExtensions (string id)
		{
			throw new NotImplementedException ();
		}

		public void LoadFromFile (string projectFileName)
		{
			this.fullFileName = Path.GetFullPath (projectFileName);
			XmlSchemaCollection xmlSchemaCollection = null;
			XmlTextReader xmlTextReader = null;
			XmlValidatingReader xmlValidatingReader = null;
			
			if (this.schemaFile != null) {
				xmlSchemaCollection = new XmlSchemaCollection ();
				xmlSchemaCollection.ValidationEventHandler += new ValidationEventHandler (ValidationCallBack);
				xmlSchemaCollection.Add (null, this.schemaFile);
				if (xmlSchemaCollection.Count > 0) {
					xmlTextReader = new XmlTextReader (projectFileName);
					xmlValidatingReader = new XmlValidatingReader (xmlTextReader);
					xmlValidatingReader.ValidationType = ValidationType.Schema;
					xmlValidatingReader.Schemas.Add (xmlSchemaCollection);
					xmlValidatingReader.ValidationEventHandler += new ValidationEventHandler (ValidationCallBack);
				}
			} else {
				xmlTextReader = new XmlTextReader (projectFileName);
			}
			if (xmlValidatingReader != null)
				xmlDocument.Load (xmlValidatingReader);
			else if (xmlTextReader != null)
				xmlDocument.Load (xmlTextReader);
			else
				throw new Exception ();
			xmlElement = xmlDocument.DocumentElement;
			if (xmlElement.Name != "Project")
				throw new InvalidProjectFileException ("Invalid root element.");
			if (xmlElement.GetAttributeNode ("DefaultTargets") != null)
				defaultTargets = xmlElement.GetAttribute ("DefaultTargets").Split (';');
			else
				defaultTargets = new string [0];
			
			ProcessElements (xmlElement, null);
			
			isDirty = false;
		}

		public void LoadFromXml (XmlDocument projectXml)
		{
			fullFileName = "";
			xmlDocument = projectXml;
			xmlElement = xmlDocument.DocumentElement;
			if (xmlElement.Name != "Project")
				throw new InvalidProjectFileException ("Invalid root element.");
			if (xmlElement.GetAttributeNode ("DefaultTargets") != null)
				defaultTargets = xmlElement.GetAttribute ("DefaultTargets").Split (';');
			else
				defaultTargets = new string [0];
			
			ProcessElements (xmlElement, null);
			
			isDirty = false;
		}

		public void MarkProjectAsDirty ()
		{
			isDirty = true;
		}

		[MonoTODO]
		public void RemoveAllItemGroups ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAllItemsGroupsByCondition (string condition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAllPropertyGroups ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAllPropertyGroupsByCondition (string condition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveItem (BuildItem itemToRemove)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveItemGroup (BuildItemGroup itemGroupToRemove)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveItemsByName (string itemName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemovePropertyGroup (BuildPropertyGroup propertyGroupToRemove)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetBuildStatus ()
		{
			throw new NotImplementedException ();
		}

		public void SaveToFile (string projectFileName)
		{
			xmlDocument.Save (projectFileName);
		}

		public void SaveToFile (string projectFileName,
					Encoding encoding)
		{
			SaveToFile (projectFileName);
		}

		public void SaveToTextWriter (TextWriter outTextWriter)
		{
			xmlDocument.Save (outTextWriter);
		}

		[MonoTODO]
		public void SetImportedProperty (string propertyName,
						 string propertyValue,
						 string condition,
						 Project importProject)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetImportedPropertyAt (string propertyName,
						   string propertyValue,
						   string condition,
						   Project importedProject,
						   PropertyPosition position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetProjectExtensions (string id, string xmlText)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetProperty (string propertyName,
					 string propertyValue,
					 string condition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetPropertyAt (string propertyName,
					   string propertyValue,
					   string condition,
					   PropertyPosition position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Unload ()
		{
			throw new NotImplementedException ();
		}
		
		private void ProcessElements (XmlElement rootElement, ImportedProject ip)
		{
			foreach (XmlNode xn in rootElement.ChildNodes) {
				if (xn is XmlElement) {
					XmlElement xe = (XmlElement) xn;
					switch (xe.Name) {
					case "ProjectExtensions":
						AddProjectExtensions (xe);
						break;
					case "Warning":
					case "Message":
					case "Error":
						AddMessage (xe);
						break;
					case "Target":
						AddTarget (xe, ip);
						break;
					case "UsingTask":
						AddUsingTask (xe, ip);
						break;
					case "Import":
						AddImport (xe, ip);
						break;
					case "ItemGroup":
						AddItemGroup (xe);
						break;
					case "PropertyGroup":
						AddPropertyGroup (xe);
						break;
					case  "Choose":
						AddChoose (xe);
						break;
					default:
						throw new InvalidProjectFileException ("Invalid element in project file.");
					}
				}
			}
		}
		
		private void AddProjectExtensions (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
		}
		
		private void AddMessage (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
		}
		
		private void AddTarget (XmlElement xmlElement, ImportedProject importedProject)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			Target target = targets.AddNewTarget (xmlElement.GetAttribute ("Name"));
			target.BindToXml (xmlElement);
			if (importedProject == null) {
				target.IsImported = false;
				if (firstTargetName == null)
					firstTargetName = target.Name;
			} else
				target.IsImported = true;
		}
		
		private void AddUsingTask (XmlElement xmlElement, ImportedProject importedProject)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
				
			if (xmlElement.GetAttribute ("TaskName") == String.Empty)
				throw new InvalidProjectFileException ("TaskName attribute must be specified.");

			usingTaskElements.Add (xmlElement);

			AssemblyLoadInfo loadInfo = null;
			string filename = null;
			string name = null;
			string taskName = xmlElement.GetAttribute ("TaskName");
			
			if (xmlElement.GetAttribute ("AssemblyName") != String.Empty) {
				name  = xmlElement.GetAttribute ("AssemblyName");
				loadInfo  = new AssemblyLoadInfo (name, taskName);
				taskDatabase.RegisterTask (taskName, loadInfo);
			} else if (xmlElement.GetAttribute ("AssemblyFile") != String.Empty) {
				filename = xmlElement.GetAttribute ("AssemblyFile");
				if (Path.IsPathRooted (filename) == false) {
					if (importedProject == null)
						filename = Path.Combine (Path.GetDirectoryName (fullFileName), filename);
					else
						filename = Path.Combine (Path.GetDirectoryName (importedProject.FullFileName), filename);
				}
				loadInfo  = new AssemblyLoadInfo (LoadInfoType.AssemblyFilename, filename, null, null, null, null, taskName);
				taskDatabase.RegisterTask (taskName, loadInfo);
			} else
				throw new InvalidProjectFileException ("AssemblyName or AssemblyFile attribute must be specified.");
		}
		
		private void AddImport (XmlElement xmlElement, ImportedProject importingProject)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			
			string importedFile;
			Expression importedFileExpr;
			ImportedProject ImportedProject;

			importedFileExpr = new Expression (this, xmlElement.GetAttribute ("Project"));
			importedFile = (string) importedFileExpr.ToNonArray (typeof (string));
			
			if (importedFile == String.Empty)
				throw new InvalidProjectFileException ("Project attribute must be specified.");
			
			if (Path.IsPathRooted (importedFile) == false) {
				if (importingProject == null)
					importedFile = Path.Combine (Path.GetDirectoryName (fullFileName), importedFile);
				else
					importedFile = Path.Combine (Path.GetDirectoryName (importingProject.FullFileName), importedFile);
			}
			
			ImportedProject importedProject = new ImportedProject ();
			try {
				importedProject.Load (importedFile);
				ProcessElements (importedProject.XmlDocument.DocumentElement, importedProject);
			}
			catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
		
		private void AddItemGroup (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			BuildItemGroup big = new BuildItemGroup (this);
			big.BindToXml (xmlElement);
			itemGroups.Add (big);
		}
		
		private void AddPropertyGroup (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
			BuildPropertyGroup bpg = new BuildPropertyGroup (true, this);
			bpg.BindToXml (xmlElement);
			propertyGroups.Add (bpg);
		}
		
		private void AddChoose (XmlElement xmlElement)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");
		}
		
		private static void ValidationCallBack (object sender, ValidationEventArgs e)
		{
			Console.WriteLine ("Validation Error: {0}", e.Message);
		}
		
		public bool BuildEnabled {
			get {
				return buildEnabled;
			}
			set {
				buildEnabled = value;
			}
		}

		public Encoding Encoding {
			get { return encoding; }
		}

		public string DefaultTargets {
			get { return xmlElement.GetAttribute ("DefaultTargets"); }
			set {
				xmlElement.SetAttribute ("DefaultTargets",value);
				defaultTargets = value.Split (';');
			}
		}

		public BuildItemGroup EvaluatedItems {
			get { return evaluatedItems; }
		}

		public BuildItemGroup EvaluatedItemsIgnoringCondition {
			get { return evaluatedItemsIgnoringCondition; }
		}
		
		internal IDictionary EvaluatedItemsByName {
			get { return evaluatedItemsByName; }
		}
		
		internal IDictionary EvaluatedItemsByNameIgnoringCondition {
			get { return evaluatedItemsByNameIgnoringCondition; }
		}

		public BuildPropertyGroup EvaluatedProperties {
			get { return evaluatedProperties; }
		}

		public string FullFileName {
			get { return fullFileName; }
			set { fullFileName = value; }
		}

		public BuildPropertyGroup GlobalProperties {
			get { return globalProperties; }
			set {
				globalProperties = value;
				foreach (BuildProperty bp in globalProperties)
					evaluatedProperties.AddFromExistingProperty (bp);
			}
		}

		public bool IsDirty {
			get { return isDirty; }
		}

		public bool IsValidated {
			get { return isValidated; }
			set { isValidated = value; }
		}

		public BuildItemGroupCollection ItemGroups {
			get { return itemGroups; }
		}

		public Engine ParentEngine {
			get { return parentEngine; }
		}

		public BuildPropertyGroupCollection PropertyGroups {
			get { return propertyGroups; }
		}

		public string SchemaFile {
			get { return schemaFile; }
			set { schemaFile = value; }
		}

		public TargetCollection Targets {
			get { return targets; }
		}

		public DateTime TimeOfLastDirty {
			get { return timeOfLastDirty; }
		}

		public XmlDocument Xml {
			get { return xmlDocument; }
		}
		
		internal TaskDatabase TaskDatabase {
			get { return taskDatabase; }
		}
		
		internal BuildPropertyGroup EnvironmentProperties {
			set {
				environmentProperties = value;
				foreach (BuildProperty bp in environmentProperties)
					evaluatedProperties.AddFromExistingProperty (bp);
			}
		}
		
		internal BuildPropertyGroup ReservedProperties {
			set {
				reservedProperties = value;
				foreach (BuildProperty bp in reservedProperties)
					evaluatedProperties.AddFromExistingProperty (bp);
			}
		}
	}
}

#endif