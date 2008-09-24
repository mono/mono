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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using Mono.XBuild.Framework;

namespace Microsoft.Build.BuildEngine {
	public class Project {
	
		bool				buildEnabled;
		Dictionary <string, List <string>>	conditionedProperties;
		string[]			defaultTargets;
		Encoding			encoding;
		BuildItemGroup			evaluatedItems;
		BuildItemGroup			evaluatedItemsIgnoringCondition;
		Dictionary <string, BuildItemGroup>	evaluatedItemsByName;
		Dictionary <string, BuildItemGroup>	evaluatedItemsByNameIgnoringCondition;
		BuildPropertyGroup		evaluatedProperties;
		string				firstTargetName;
		string				fullFileName;
		BuildPropertyGroup		globalProperties;
		GroupingCollection		groupingCollection;
		bool				isDirty;
		bool				isValidated;
		BuildItemGroupCollection	itemGroups;
		ImportCollection		imports;
		string				initialTargets;
		Dictionary <string, BuildItemGroup> last_item_group_containing;
		bool				needToReevaluate;
		Engine				parentEngine;
		BuildPropertyGroupCollection	propertyGroups;
		string				schemaFile;
		TaskDatabase			taskDatabase;
		TargetCollection		targets;
		DateTime			timeOfLastDirty;
		UsingTaskCollection		usingTasks;
		XmlDocument			xmlDocument;
		bool				unloaded;

		static XmlNamespaceManager	manager;
		static string ns = "http://schemas.microsoft.com/developer/msbuild/2003";

		public Project ()
			: this (Engine.GlobalEngine)
		{
		}

		public Project (Engine engine)
		{
			parentEngine  = engine;

			buildEnabled = ParentEngine.BuildEnabled;
			xmlDocument = new XmlDocument ();
			xmlDocument.PreserveWhitespace = false;
			xmlDocument.AppendChild (xmlDocument.CreateElement ("Project", XmlNamespace));
			xmlDocument.DocumentElement.SetAttribute ("xmlns", ns);
			
			fullFileName = String.Empty;

			globalProperties = new BuildPropertyGroup (null, this, null, false);
			foreach (BuildProperty bp in parentEngine.GlobalProperties)
				GlobalProperties.AddProperty (bp.Clone (true));
			
			ProcessXml ();
		}

		[MonoTODO ("Not tested")]
		public void AddNewImport (string importLocation,
					  string importCondition)
		{
			if (importLocation == null)
				throw new ArgumentNullException ("importLocation");

			XmlElement importElement = xmlDocument.CreateElement ("Import", XmlNamespace);
			xmlDocument.DocumentElement.AppendChild (importElement);
			importElement.SetAttribute ("Project", importLocation);
			if (!String.IsNullOrEmpty (importCondition))
				importElement.SetAttribute ("Condition", importCondition);

			Import import = new Import (importElement, this, null);
			imports.Add (import);
			MarkProjectAsDirty ();
			NeedToReevaluate ();
		}

		public BuildItem AddNewItem (string itemName,
					     string itemInclude)
		{
			return AddNewItem (itemName, itemInclude, false);
		}
		
		[MonoTODO ("Adds item not in the same place as MS")]
		public BuildItem AddNewItem (string itemName,
					     string itemInclude,
					     bool treatItemIncludeAsLiteral)
		{
			BuildItemGroup big;

			if (itemGroups.Count == 0)
				big = AddNewItemGroup ();
			else {
				if (last_item_group_containing.ContainsKey (itemName)) {
					big = last_item_group_containing [itemName];
				} else {
					// FIXME: not tested
					BuildItemGroup [] groups = new BuildItemGroup [itemGroups.Count];
					itemGroups.CopyTo (groups, 0);
					big = groups [0];
				}
			}

			BuildItem item = big.AddNewItem (itemName, itemInclude, treatItemIncludeAsLiteral);
				
			MarkProjectAsDirty ();
			NeedToReevaluate ();

			return item;
		}

		[MonoTODO ("Not tested")]
		public BuildItemGroup AddNewItemGroup ()
		{
			XmlElement element = xmlDocument.CreateElement ("ItemGroup", XmlNamespace);
			xmlDocument.DocumentElement.AppendChild (element);

			BuildItemGroup big = new BuildItemGroup (element, this, null, false);
			itemGroups.Add (big);
			MarkProjectAsDirty ();
			NeedToReevaluate ();

			return big;
		}

		[MonoTODO ("Ignores insertAtEndOfProject")]
		public BuildPropertyGroup AddNewPropertyGroup (bool insertAtEndOfProject)
		{
			XmlElement element = xmlDocument.CreateElement ("PropertyGroup", XmlNamespace);
			xmlDocument.DocumentElement.AppendChild (element);

			BuildPropertyGroup bpg = new BuildPropertyGroup (element, this, null, false);
			propertyGroups.Add (bpg);
			MarkProjectAsDirty ();
			NeedToReevaluate ();

			return bpg;
		}
		
		[MonoTODO ("Not tested, isn't added to TaskDatabase (no reevaluation)")]
		public void AddNewUsingTaskFromAssemblyFile (string taskName,
							     string assemblyFile)
		{
			if (taskName == null)
				throw new ArgumentNullException ("taskName");
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");

			XmlElement element = xmlDocument.CreateElement ("UsingTask", XmlNamespace);
			xmlDocument.DocumentElement.AppendChild (element);
			element.SetAttribute ("TaskName", taskName);
			element.SetAttribute ("AssemblyFile", assemblyFile);

			UsingTask ut = new UsingTask (element, this, null);
			usingTasks.Add (ut);
			MarkProjectAsDirty ();
		}
		
		[MonoTODO ("Not tested, isn't added to TaskDatabase (no reevaluation)")]
		public void AddNewUsingTaskFromAssemblyName (string taskName,
							     string assemblyName)
		{
			if (taskName == null)
				throw new ArgumentNullException ("taskName");
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			XmlElement element = xmlDocument.CreateElement ("UsingTask", XmlNamespace);
			xmlDocument.DocumentElement.AppendChild (element);
			element.SetAttribute ("TaskName", taskName);
			element.SetAttribute ("AssemblyName", assemblyName);

			UsingTask ut = new UsingTask (element, this, null);
			usingTasks.Add (ut);
			MarkProjectAsDirty ();
		}
		
		[MonoTODO ("Not tested")]
		public bool Build ()
		{
			return Build (new string [0]);
		}
		
		[MonoTODO ("Not tested")]
		public bool Build (string targetName)
		{
			if (targetName == null)
				return false;
			return Build (new string [1] { targetName });
		}
		
		[MonoTODO ("Not tested")]
		public bool Build (string [] targetNames)
		{
			return Build (targetNames, null);
		}
		
		[MonoTODO ("Not tested")]
		public bool Build (string [] targetNames,
				   IDictionary targetOutputs)
		{
			return Build (targetNames, targetOutputs, BuildSettings.None);
		}
		
		[MonoTODO ("Not tested")]
		public bool Build (string [] targetNames,
				   IDictionary targetOutputs,
				   BuildSettings buildFlags)
		
		{
			if (targetNames == null)
				return false;

			CheckUnloaded ();
			ParentEngine.StartBuild ();
			NeedToReevaluate ();
			
			if (targetNames.Length == 0) {
				if (defaultTargets != null && defaultTargets.Length != 0)
					targetNames = defaultTargets;
				else if (firstTargetName != null)
					targetNames = new string [1] { firstTargetName};
				else
					return false;
			}
			
			foreach (string target in targetNames) {
				if (target == null)
					throw new ArgumentException ("targetNames cannot contain null strings");

				if (!targets.Exists (target))
					// FIXME: test if it's logged
					return false;
				
				if (!targets [target].Build ())
					return false;

				if (targetOutputs != null)
					targetOutputs.Add (target, targets [target].Outputs);
			}
				
			return true;
		}

		[MonoTODO]
		public string [] GetConditionedPropertyValues (string propertyName)
		{
			if (conditionedProperties.ContainsKey (propertyName))
				return conditionedProperties [propertyName].ToArray ();
			else
				return new string [0];
		}

		public BuildItemGroup GetEvaluatedItemsByName (string itemName)
		{			
			if (needToReevaluate) {
				needToReevaluate = false;
				Reevaluate ();
			}

			if (evaluatedItemsByName.ContainsKey (itemName))
				return evaluatedItemsByName [itemName];
			else
				return new BuildItemGroup ();
		}

		public BuildItemGroup GetEvaluatedItemsByNameIgnoringCondition (string itemName)
		{
			if (needToReevaluate) {
				needToReevaluate = false;
				Reevaluate ();
			}

			if (evaluatedItemsByNameIgnoringCondition.ContainsKey (itemName))
				return evaluatedItemsByNameIgnoringCondition [itemName];
			else
				return new BuildItemGroup ();
		}

		public string GetEvaluatedProperty (string propertyName)
		{
			if (needToReevaluate) {
				needToReevaluate = false;
				Reevaluate ();
			}

			if (propertyName == null)
				throw new ArgumentNullException ("propertyName");

			BuildProperty bp = evaluatedProperties [propertyName];

			return bp == null ? null : (string) bp;
		}

		[MonoTODO ("We should remember that node and not use XPath to get it")]
		public string GetProjectExtensions (string id)
		{
			if (id == null || id == String.Empty)
				return String.Empty;

			XmlNode node = xmlDocument.SelectSingleNode (String.Format ("/tns:Project/tns:ProjectExtensions/tns:{0}", id), XmlNamespaceManager);

			if (node == null)
				return String.Empty;
			else
				return node.InnerXml;
		}


		public void Load (string projectFileName)
		{
			this.fullFileName = Path.GetFullPath (projectFileName);
			DoLoad (new StreamReader (projectFileName));
		}
		
		[MonoTODO ("Not tested")]
		public void Load (TextReader textReader)
		{
			fullFileName = String.Empty;
			DoLoad (textReader);
		}

		public void LoadXml (string projectXml)
		{
			fullFileName = String.Empty;
			DoLoad (new StringReader (projectXml));
			MarkProjectAsDirty ();
		}


		public void MarkProjectAsDirty ()
		{
			isDirty = true;
			timeOfLastDirty = DateTime.Now;
		}

		[MonoTODO ("Not tested")]
		public void RemoveAllItemGroups ()
		{
			int length = ItemGroups.Count;
			BuildItemGroup [] groups = new BuildItemGroup [length];
			ItemGroups.CopyTo (groups, 0);

			for (int i = 0; i < length; i++)
				RemoveItemGroup (groups [i]);

			MarkProjectAsDirty ();
			NeedToReevaluate ();
		}

		[MonoTODO ("Not tested")]
		public void RemoveAllPropertyGroups ()
		{
			int length = PropertyGroups.Count;
			BuildPropertyGroup [] groups = new BuildPropertyGroup [length];
			PropertyGroups.CopyTo (groups, 0);

			for (int i = 0; i < length; i++)
				RemovePropertyGroup (groups [i]);

			MarkProjectAsDirty ();
			NeedToReevaluate ();
		}

		[MonoTODO]
		public void RemoveItem (BuildItem itemToRemove)
		{
			if (itemToRemove == null)
				throw new ArgumentNullException ("itemToRemove");

			if (!itemToRemove.FromXml && !itemToRemove.HasParent)
				throw new InvalidOperationException ("The object passed in is not part of the project.");

			BuildItemGroup big = itemToRemove.ParentItemGroup;

			if (big.Count == 1) {
				// ParentItemGroup for items from xml and that have parent is the same
				groupingCollection.Remove (big);
			} else {
				if (big.ParentProject != this)
					throw new InvalidOperationException ("The object passed in is not part of the project.");

				if (itemToRemove.FromXml)
					big.RemoveItem (itemToRemove);
				else
					big.RemoveItem (itemToRemove.ParentItem);
			}

			MarkProjectAsDirty ();
			NeedToReevaluate ();
		}

		[MonoTODO ("Not tested")]
		public void RemoveItemGroup (BuildItemGroup itemGroupToRemove)
		{
			if (itemGroupToRemove == null)
				throw new ArgumentNullException ("itemGroupToRemove");

			groupingCollection.Remove (itemGroupToRemove);
			MarkProjectAsDirty ();
		}
		
		[MonoTODO]
		// NOTE: does not modify imported projects
		public void RemoveItemGroupsWithMatchingCondition (string matchingCondition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveItemsByName (string itemName)
		{
			if (itemName == null)
				throw new ArgumentNullException ("itemName");

			throw new NotImplementedException ();
		}

		[MonoTODO ("Not tested")]
		public void RemovePropertyGroup (BuildPropertyGroup propertyGroupToRemove)
		{
			if (propertyGroupToRemove == null)
				throw new ArgumentNullException ("propertyGroupToRemove");

			groupingCollection.Remove (propertyGroupToRemove);
			MarkProjectAsDirty ();
		}
		
		[MonoTODO]
		// NOTE: does not modify imported projects
		public void RemovePropertyGroupsWithMatchingCondition (string matchCondition)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetBuildStatus ()
		{
			throw new NotImplementedException ();
		}

		public void Save (string projectFileName)
		{
			Save (projectFileName, Encoding.Default);
			isDirty = false;
		}

		[MonoTODO ("Ignores encoding")]
		public void Save (string projectFileName, Encoding encoding)
		{
			xmlDocument.Save (projectFileName);
			isDirty = false;
		}

		public void Save (TextWriter outTextWriter)
		{
			xmlDocument.Save (outTextWriter);
			isDirty = false;
		}

		public void SetImportedProperty (string propertyName,
						 string propertyValue,
						 string condition,
						 Project importProject)
		{
			SetImportedProperty (propertyName, propertyValue, condition, importProject,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup);
		}

		public void SetImportedProperty (string propertyName,
						 string propertyValue,
						 string condition,
						 Project importedProject,
						 PropertyPosition position)
		{
			SetImportedProperty (propertyName, propertyValue, condition, importedProject,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup, false);
		}

		[MonoTODO]
		public void SetImportedProperty (string propertyName,
						 string propertyValue,
						 string condition,
						 Project importedProject,
						 PropertyPosition position,
						 bool treatPropertyValueAsLiteral)
		{
			throw new NotImplementedException ();
		}

		public void SetProjectExtensions (string id, string xmlText)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			if (xmlText == null)
				throw new ArgumentNullException ("xmlText");

			XmlNode projectExtensions, node;

			projectExtensions = xmlDocument.SelectSingleNode ("/tns:Project/tns:ProjectExtensions", XmlNamespaceManager);
			
			if (projectExtensions == null) {
				projectExtensions = xmlDocument.CreateElement ("ProjectExtensions", XmlNamespace);
				xmlDocument.DocumentElement.AppendChild (projectExtensions);

				node = xmlDocument.CreateElement (id, XmlNamespace);
				node.InnerXml = xmlText;
				projectExtensions.AppendChild (node);
			} else {
				node = xmlDocument.SelectSingleNode (String.Format ("/tns:Project/tns:ProjectExtensions/tns:{0}", id), XmlNamespaceManager);

				if (node == null) {
					node = xmlDocument.CreateElement (id, XmlNamespace);
					projectExtensions.AppendChild (node);
				}
				
				node.InnerXml = xmlText;
				
			}

			MarkProjectAsDirty ();
		}
		
		public void SetProperty (string propertyName,
					 string propertyValue)
		{
			SetProperty (propertyName, propertyValue, "true",
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup, false);
		}

		public void SetProperty (string propertyName,
					 string propertyValue,
					 string condition)
		{
			SetProperty (propertyName, propertyValue, condition,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup);
		}

		public void SetProperty (string propertyName,
					 string propertyValue,
					 string condition,
					 PropertyPosition position)
		{
			SetProperty (propertyName, propertyValue, condition,
				PropertyPosition.UseExistingOrCreateAfterLastPropertyGroup, false);
		}

		[MonoTODO]
		public void SetProperty (string propertyName,
					 string propertyValue,
					 string condition,
					 PropertyPosition position,
					 bool treatPropertyValueAsLiteral)
		{
			throw new NotImplementedException ();
		}

		internal void Unload ()
		{
			unloaded = true;
		}

		internal void CheckUnloaded ()
		{
			if (unloaded)
				throw new InvalidOperationException ("This project object has been unloaded from the MSBuild engine and is no longer valid.");
		}

		internal void NeedToReevaluate ()
		{
			needToReevaluate = true;
		}
				
		// Does the actual loading.
		void DoLoad (TextReader textReader)
		{
			try {
				ParentEngine.RemoveLoadedProject (this);
	
				XmlReaderSettings settings = new XmlReaderSettings ();
	
				if (SchemaFile != null) {
					settings.Schemas.Add (null, SchemaFile);
					settings.ValidationType = ValidationType.Schema;
					settings.ValidationEventHandler += new ValidationEventHandler (ValidationCallBack);
				}
	
				XmlReader xmlReader = XmlReader.Create (textReader, settings);
				xmlDocument.Load (xmlReader);

				if (xmlDocument.DocumentElement.Name != "Project") {
					throw new InvalidProjectFileException (String.Format (
						"The element <{0}> is unrecognized, or not supported in this context.", xmlDocument.DocumentElement.Name));
				}
	
				if (xmlDocument.DocumentElement.GetAttribute ("xmlns") != ns) {
					throw new InvalidProjectFileException (
						@"The default XML namespace of the project must be the MSBuild XML namespace." + 
						" If the project is authored in the MSBuild 2003 format, please add " +
						"xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\" to the <Project> element. " +
						"If the project has been authored in the old 1.0 or 1.2 format, please convert it to MSBuild 2003 format.  ");
				}
				ProcessXml ();
				ParentEngine.AddLoadedProject (this);
			} catch (Exception e) {
				throw new InvalidProjectFileException (e.Message, e);
			} finally {
				textReader.Close ();
			}
		}

		void Reevaluate ()
		{
			ProcessXml ();
		}

		void ProcessXml ()
		{
			groupingCollection = new GroupingCollection (this);
			imports = new ImportCollection (groupingCollection);
			usingTasks = new UsingTaskCollection (this);
			itemGroups = new BuildItemGroupCollection (groupingCollection);
			propertyGroups = new BuildPropertyGroupCollection (groupingCollection);
			targets = new TargetCollection (this);
			last_item_group_containing = new Dictionary <string, BuildItemGroup> ();
			
			taskDatabase = new TaskDatabase ();
			if (ParentEngine.DefaultTasksRegistered)
				taskDatabase.CopyTasks (ParentEngine.DefaultTasks);	

			if (xmlDocument.DocumentElement.GetAttributeNode ("DefaultTargets") != null)
				defaultTargets = xmlDocument.DocumentElement.GetAttribute ("DefaultTargets").Split (';');
			else
				defaultTargets = new string [0];
			
			ProcessElements (xmlDocument.DocumentElement, null);
			
			isDirty = false;
			Evaluate ();
		}
		
		internal void ProcessElements (XmlElement rootElement, ImportedProject ip)
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
						AddItemGroup (xe, ip);
						break;
					case "PropertyGroup":
						AddPropertyGroup (xe, ip);
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
		
		void Evaluate ()
		{
			evaluatedItems = new BuildItemGroup (null, this, null, true);
			evaluatedItemsIgnoringCondition = new BuildItemGroup (null, this, null, true);
			evaluatedItemsByName = new Dictionary <string, BuildItemGroup> (StringComparer.InvariantCultureIgnoreCase);
			evaluatedItemsByNameIgnoringCondition = new Dictionary <string, BuildItemGroup> (StringComparer.InvariantCultureIgnoreCase);
			evaluatedProperties = new BuildPropertyGroup (null, null, null, true);

			InitializeProperties ();

			groupingCollection.Evaluate ();

			//FIXME: UsingTasks aren't really evaluated. (shouldn't use expressions or anything)
			foreach (UsingTask usingTask in UsingTasks)
				usingTask.Evaluate ();
		}

		void InitializeProperties ()
		{
			BuildProperty bp;

			foreach (BuildProperty gp in GlobalProperties) {
				bp = new BuildProperty (gp.Name, gp.Value, PropertyType.Global);
				EvaluatedProperties.AddProperty (bp);
			}
			
			foreach (DictionaryEntry de in Environment.GetEnvironmentVariables ()) {
				bp = new BuildProperty ((string) de.Key, (string) de.Value, PropertyType.Environment);
				EvaluatedProperties.AddProperty (bp);
			}

			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildBinPath", parentEngine.BinPath, PropertyType.Reserved));

			// FIXME: make some internal method that will work like GetDirectoryName but output String.Empty on null/String.Empty
			string projectDir;
			if (FullFileName == String.Empty)
				projectDir = Environment.CurrentDirectory;
			else
				projectDir = Path.GetDirectoryName (FullFileName);

			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildProjectDirectory", projectDir, PropertyType.Reserved));
		}
		
		void AddProjectExtensions (XmlElement xmlElement)
		{
		}
		
		void AddMessage (XmlElement xmlElement)
		{
		}
		
		void AddTarget (XmlElement xmlElement, ImportedProject importedProject)
		{
			Target target = new Target (xmlElement, this, importedProject);
			targets.AddTarget (target);
			
			if (firstTargetName == null)
				firstTargetName = target.Name;
		}
		
		void AddUsingTask (XmlElement xmlElement, ImportedProject importedProject)
		{
			UsingTask usingTask;

			usingTask = new UsingTask (xmlElement, this, importedProject);
			UsingTasks.Add (usingTask);
		}
		
		void AddImport (XmlElement xmlElement, ImportedProject importingProject)
		{
			Import import;
			
			import = new Import (xmlElement, this, importingProject);
			Imports.Add (import);
		}
		
		void AddItemGroup (XmlElement xmlElement, ImportedProject importedProject)
		{
			BuildItemGroup big = new BuildItemGroup (xmlElement, this, importedProject, false);
			ItemGroups.Add (big);
		}
		
		void AddPropertyGroup (XmlElement xmlElement, ImportedProject importedProject)
		{
			BuildPropertyGroup bpg = new BuildPropertyGroup (xmlElement, this, importedProject, false);
			PropertyGroups.Add (bpg);
		}
		
		void AddChoose (XmlElement xmlElement)
		{
			BuildChoose bc = new BuildChoose (xmlElement, this);
			groupingCollection.Add (bc);
		}
		
		static void ValidationCallBack (object sender, ValidationEventArgs e)
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

		[MonoTODO]
		public Encoding Encoding {
			get { return encoding; }
		}

		public string DefaultTargets {
			get {
				return xmlDocument.DocumentElement.GetAttribute ("DefaultTargets");
			}
			set {
				xmlDocument.DocumentElement.SetAttribute ("DefaultTargets", value);
				defaultTargets = value.Split (';');
			}
		}

		public BuildItemGroup EvaluatedItems {
			get {
				if (needToReevaluate) {
					needToReevaluate = false;
					Reevaluate ();
				}
				return evaluatedItems;
			}
		}

		public BuildItemGroup EvaluatedItemsIgnoringCondition {
			get {
				if (needToReevaluate) {
					needToReevaluate = false;
					Reevaluate ();
				}
				return evaluatedItemsIgnoringCondition;
			}
		}
		
		internal IDictionary <string, BuildItemGroup> EvaluatedItemsByName {
			get {
				// FIXME: do we need to do this here?
				if (needToReevaluate) {
					needToReevaluate = false;
					Reevaluate ();
				}
				return evaluatedItemsByName;
			}
		}
		
		internal IDictionary <string, BuildItemGroup> EvaluatedItemsByNameIgnoringCondition {
			get {
				// FIXME: do we need to do this here?
				if (needToReevaluate) {
					needToReevaluate = false;
					Reevaluate ();
				}
				return evaluatedItemsByNameIgnoringCondition;
			}
		}

		public BuildPropertyGroup EvaluatedProperties {
			get {
				if (needToReevaluate) {
					needToReevaluate = false;
					Reevaluate ();
				}
				return evaluatedProperties;
			}
		}

		public string FullFileName {
			get { return fullFileName; }
			set { fullFileName = value; }
		}

		public BuildPropertyGroup GlobalProperties {
			get { return globalProperties; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				if (value.FromXml)
					throw new InvalidOperationException ("GlobalProperties can not be set to persisted property group.");
				
				globalProperties = value;
				NeedToReevaluate ();
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
		
		public ImportCollection Imports {
			get { return imports; }
		}
		
		public string InitialTargets {
			get { return initialTargets; }
			set { initialTargets = value; }
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
		
		public UsingTaskCollection UsingTasks {
			get { return usingTasks; }
		}

		[MonoTODO]
		public string Xml {
			get { return xmlDocument.InnerXml; }
		}

		internal Dictionary <string, BuildItemGroup> LastItemGroupContaining {
			get { return last_item_group_containing; }
		}
		
		internal static XmlNamespaceManager XmlNamespaceManager {
			get {
				if (manager == null) {
					manager = new XmlNamespaceManager (new NameTable ());
					manager.AddNamespace ("tns", ns);
				}
				
				return manager;
			}
		}
		
		internal TaskDatabase TaskDatabase {
			get { return taskDatabase; }
		}
		
		internal XmlDocument XmlDocument {
			get { return xmlDocument; }
		}
		
		internal static string XmlNamespace {
			get { return ns; }
		}
	}
}

#endif
