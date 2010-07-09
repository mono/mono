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
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using Mono.XBuild.Framework;
using Mono.XBuild.CommandLine;

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
		List<string>			initialTargets;
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
		bool				initialTargetsBuilt;
		List<string>			builtTargetKeys;
		bool				building;
		BuildSettings			current_settings;
		Stack<Batch>			batches;
		ProjectLoadSettings		project_load_settings;


		static string extensions_path;
		static XmlNamespaceManager	manager;
		static string ns = "http://schemas.microsoft.com/developer/msbuild/2003";

		public Project ()
			: this (Engine.GlobalEngine)
		{
		}

		public Project (Engine engine) : this (engine, null)
		{
		}
		
		public Project (Engine engine, string toolsVersion)
		{
			parentEngine  = engine;
			ToolsVersion = toolsVersion;

			buildEnabled = ParentEngine.BuildEnabled;
			xmlDocument = new XmlDocument ();
			xmlDocument.PreserveWhitespace = false;
			xmlDocument.AppendChild (xmlDocument.CreateElement ("Project", XmlNamespace));
			xmlDocument.DocumentElement.SetAttribute ("xmlns", ns);
			
			fullFileName = String.Empty;
			timeOfLastDirty = DateTime.Now;
			current_settings = BuildSettings.None;
			project_load_settings = ProjectLoadSettings.None;

			encoding = null;

			builtTargetKeys = new List<string> ();
			initialTargets = new List<string> ();
			defaultTargets = new string [0];
			batches = new Stack<Batch> ();

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
				return Build ((string[]) null);
			else
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
			bool result = false;
			ParentEngine.StartProjectBuild (this, targetNames);
			string current_directory = Environment.CurrentDirectory;
			try {
				current_settings = buildFlags;
				if (!String.IsNullOrEmpty (fullFileName))
					Directory.SetCurrentDirectory (Path.GetDirectoryName (fullFileName));
				building = true;
				result = BuildInternal (targetNames, targetOutputs, buildFlags);
			} finally {
				ParentEngine.EndProjectBuild (this, result);
				current_settings = BuildSettings.None;
				Directory.SetCurrentDirectory (current_directory);
				building = false;
			}

			return result;
		}

		bool BuildInternal (string [] targetNames,
				   IDictionary targetOutputs,
				   BuildSettings buildFlags)
		{
			CheckUnloaded ();
			if (buildFlags == BuildSettings.None)
				Reevaluate ();
			
			if (targetNames == null || targetNames.Length == 0) {
				if (defaultTargets != null && defaultTargets.Length != 0)
					targetNames = defaultTargets;
				else if (firstTargetName != null)
					targetNames = new string [1] { firstTargetName};
				else
					return false;
			}

			if (!initialTargetsBuilt) {
				foreach (string target in initialTargets) {
					if (!BuildTarget (target.Trim (), targetOutputs))
						return false;
				}
				initialTargetsBuilt = true;
			}

			foreach (string target in targetNames)
				if (!BuildTarget (target.Trim (), targetOutputs))
					return false;
				
			return true;
		}

		bool BuildTarget (string target_name, IDictionary targetOutputs)
		{
			if (target_name == null)
				throw new ArgumentException ("targetNames cannot contain null strings");

			if (!targets.Exists (target_name)) {
				LogError (fullFileName, "Target named '{0}' not found in the project.", target_name);
				return false;
			}

			string key = GetKeyForTarget (target_name);
			if (!targets [target_name].Build (key))
				return false;

			ITaskItem[] outputs;
			if (ParentEngine.BuiltTargetsOutputByName.TryGetValue (key, out outputs)) {
				if (targetOutputs != null)
					targetOutputs.Add (target_name, outputs);
			}
			return true;
		}

		internal string GetKeyForTarget (string target_name)
		{
			// target name is case insensitive
			return fullFileName + ":" + target_name.ToLower () + ":" + GlobalPropertiesToString (GlobalProperties);
		}

		string GlobalPropertiesToString (BuildPropertyGroup bgp)
		{
			StringBuilder sb = new StringBuilder ();
			foreach (BuildProperty bp in bgp)
				sb.AppendFormat (" {0}:{1}", bp.Name, bp.FinalValue);
			return sb.ToString ();
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
				return new BuildItemGroup (this);
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
				return new BuildItemGroup (this);
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
			Load (projectFileName, ProjectLoadSettings.None);
		}

		public void Load (string projectFileName, ProjectLoadSettings settings)
		{
			project_load_settings = settings;
			if (String.IsNullOrEmpty (projectFileName))
				throw new ArgumentNullException ("projectFileName");

			if (!File.Exists (projectFileName))
				throw new ArgumentException (String.Format ("Project file {0} not found", projectFileName),
						"projectFileName");

			this.fullFileName = Utilities.FromMSBuildPath (Path.GetFullPath (projectFileName));

			string filename = fullFileName;
			if (String.Compare (Path.GetExtension (fullFileName), ".sln", true) == 0) {
				Project tmp_project = ParentEngine.CreateNewProject ();
				SolutionParser sln_parser = new SolutionParser ();
				sln_parser.ParseSolution (fullFileName, tmp_project, delegate (int errorNumber, string message) {
						LogWarning (filename, message);
					});
				filename = fullFileName + ".proj";
				try {
					tmp_project.Save (filename);
					ParentEngine.RemoveLoadedProject (tmp_project);
					DoLoad (new StreamReader (filename));
				} finally {
					if (Environment.GetEnvironmentVariable ("XBUILD_EMIT_SOLUTION") == null)
						File.Delete (filename);
				}
			} else {
				DoLoad (new StreamReader (filename));
			}
		}
		
		[MonoTODO ("Not tested")]
		public void Load (TextReader textReader)
		{
			Load (textReader, ProjectLoadSettings.None);
		}

		public void Load (TextReader textReader, ProjectLoadSettings projectLoadSettings)
		{
			project_load_settings = projectLoadSettings;
			fullFileName = String.Empty;
			DoLoad (textReader);
		}

		public void LoadXml (string projectXml)
		{
			LoadXml (projectXml, ProjectLoadSettings.None);
		}

		public void LoadXml (string projectXml, ProjectLoadSettings projectLoadSettings)
		{
			project_load_settings = projectLoadSettings;
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

			if (!itemToRemove.FromXml && !itemToRemove.HasParentItem)
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
			// hack to allow built targets to be removed
			building = true;
			Reevaluate ();
			building = false;
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
	
				xmlDocument.Load (textReader);

				if (xmlDocument.DocumentElement.Name == "VisualStudioProject")
					throw new InvalidProjectFileException (String.Format (
							"Project file '{0}' is a VS2003 project, which is not " +
							"supported by xbuild. You need to convert it to msbuild " +
							"format to build with xbuild.", fullFileName));

				if (SchemaFile != null) {
					xmlDocument.Schemas.Add (XmlSchema.Read (
								new StreamReader (SchemaFile), ValidationCallBack));
					xmlDocument.Validate (ValidationCallBack);
				}

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
				throw new InvalidProjectFileException (String.Format ("{0}: {1}",
							fullFileName, e.Message), e);
			} finally {
				if (textReader != null)
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
			
			string effective_tools_version = GetToolsVersionToUse ();
			taskDatabase = new TaskDatabase ();
			taskDatabase.CopyTasks (ParentEngine.GetDefaultTasks (effective_tools_version));

			initialTargets = new List<string> ();
			defaultTargets = new string [0];
			PrepareForEvaluate (effective_tools_version);
			ProcessElements (xmlDocument.DocumentElement, null);
			
			isDirty = false;
			Evaluate ();
		}

		void ProcessProjectAttributes (XmlAttributeCollection attributes)
		{
			foreach (XmlAttribute attr in attributes) {
				switch (attr.Name) {
				case "InitialTargets":
					initialTargets.AddRange (attr.Value.Split (
									new char [] {';', ' '},
									StringSplitOptions.RemoveEmptyEntries));
					break;
				case "DefaultTargets":
					// first non-empty DefaultTargets found is used
					if (defaultTargets == null || defaultTargets.Length == 0)
						defaultTargets = attr.Value.Split (new char [] {';', ' '},
							StringSplitOptions.RemoveEmptyEntries);
					EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildProjectDefaultTargets",
								DefaultTargets, PropertyType.Reserved));
					break;
				}
			}
		}

		internal void ProcessElements (XmlElement rootElement, ImportedProject ip)
		{
			ProcessProjectAttributes (rootElement.Attributes);
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
						throw new InvalidProjectFileException (String.Format ("Invalid element '{0}' in project file.", xe.Name));
					}
				}
			}
		}
		
		void PrepareForEvaluate (string effective_tools_version)
		{
			evaluatedItems = new BuildItemGroup (null, this, null, true);
			evaluatedItemsIgnoringCondition = new BuildItemGroup (null, this, null, true);
			evaluatedItemsByName = new Dictionary <string, BuildItemGroup> (StringComparer.InvariantCultureIgnoreCase);
			evaluatedItemsByNameIgnoringCondition = new Dictionary <string, BuildItemGroup> (StringComparer.InvariantCultureIgnoreCase);
			if (building && current_settings == BuildSettings.None)
				RemoveBuiltTargets ();

			InitializeProperties (effective_tools_version);
		}

		void Evaluate ()
		{
			groupingCollection.Evaluate ();

			//FIXME: UsingTasks aren't really evaluated. (shouldn't use expressions or anything)
			foreach (UsingTask usingTask in UsingTasks)
				usingTask.Evaluate ();
		}

		// Removes entries of all earlier built targets for this project
		void RemoveBuiltTargets ()
		{
			foreach (string key in builtTargetKeys)
				ParentEngine.BuiltTargetsOutputByName.Remove (key);
		}

		void InitializeProperties (string effective_tools_version)
		{
			BuildProperty bp;

			evaluatedProperties = new BuildPropertyGroup (null, null, null, true);

			foreach (BuildProperty gp in GlobalProperties) {
				bp = new BuildProperty (gp.Name, gp.Value, PropertyType.Global);
				EvaluatedProperties.AddProperty (bp);
			}
			
			foreach (BuildProperty gp in GlobalProperties)
				ParentEngine.GlobalProperties.AddProperty (gp);

			// add properties that we dont have from parent engine's
			// global properties
			foreach (BuildProperty gp in ParentEngine.GlobalProperties) {
				if (EvaluatedProperties [gp.Name] == null) {
					bp = new BuildProperty (gp.Name, gp.Value, PropertyType.Global);
					EvaluatedProperties.AddProperty (bp);
				}
			}

			foreach (DictionaryEntry de in Environment.GetEnvironmentVariables ()) {
				bp = new BuildProperty ((string) de.Key, (string) de.Value, PropertyType.Environment);
				EvaluatedProperties.AddProperty (bp);
			}

			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildProjectFile", Path.GetFileName (fullFileName),
						PropertyType.Reserved));
			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildProjectName",
						Path.GetFileNameWithoutExtension (fullFileName),
						PropertyType.Reserved));
			string toolsPath = parentEngine.Toolsets [effective_tools_version].ToolsPath;
			if (toolsPath == null)
				throw new Exception (String.Format ("Invalid tools version '{0}', no tools path set for this.", effective_tools_version));
			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildBinPath", toolsPath, PropertyType.Reserved));
			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildToolsPath", toolsPath, PropertyType.Reserved));
			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildToolsVersion", effective_tools_version, PropertyType.Reserved));
			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildExtensionsPath", ExtensionsPath, PropertyType.Reserved));
			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildExtensionsPath32", ExtensionsPath, PropertyType.Reserved));
			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildProjectDefaultTargets", DefaultTargets, PropertyType.Reserved));
			EvaluatedProperties.AddProperty (new BuildProperty ("OS", OS, PropertyType.Environment));

			// FIXME: make some internal method that will work like GetDirectoryName but output String.Empty on null/String.Empty
			string projectDir;
			if (FullFileName == String.Empty)
				projectDir = Environment.CurrentDirectory;
			else
				projectDir = Path.GetDirectoryName (FullFileName);

			EvaluatedProperties.AddProperty (new BuildProperty ("MSBuildProjectDirectory", projectDir, PropertyType.Reserved));
		}

		// precedence:
		// ToolsVersion property
		// ToolsVersion attribute on the project
		// parentEngine's DefaultToolsVersion
		string GetToolsVersionToUse ()
		{
			if (!String.IsNullOrEmpty (ToolsVersion))
				return ToolsVersion;

			if (!HasToolsVersionAttribute)
				return parentEngine.DefaultToolsVersion;

			if (parentEngine.Toolsets [DefaultToolsVersion] == null) {
				LogWarning (FullFileName, "Project has unknown ToolsVersion '{0}'. Using the default tools version '{1}' instead.",
						DefaultToolsVersion, parentEngine.DefaultToolsVersion);
				return parentEngine.DefaultToolsVersion;
			}

			return DefaultToolsVersion;
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
			// eval all the properties etc till the import
			groupingCollection.Evaluate (EvaluationType.Property);

			Import import = new Import (xmlElement, this, importingProject);
			if (!ConditionParser.ParseAndEvaluate (import.Condition, this))
				return;

			if (Imports.Contains (import)) {
				LogWarning (importingProject != null ? importingProject.FullFileName : fullFileName,
						"A circular reference was found involving the import of {0}. Only" +
						" the first import of this file will be used, ignoring others.",
						import.ProjectPath);

				return;
			}

			if (String.Compare (fullFileName, import.EvaluatedProjectPath) == 0) {
				LogWarning (importingProject != null ? importingProject.FullFileName : fullFileName,
						"The main project file was imported here, which creates a circular " +
						"reference. Ignoring this import.");

				return;
			}

			Imports.Add (import);
			import.Evaluate (project_load_settings == ProjectLoadSettings.IgnoreMissingImports);
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
				return String.Join ("; ", defaultTargets);
			}
			set {
				xmlDocument.DocumentElement.SetAttribute ("DefaultTargets", value);
				if (value != null)
					defaultTargets = value.Split (new char [] {';', ' '},
							StringSplitOptions.RemoveEmptyEntries);
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

		internal IEnumerable EvaluatedItemsByNameAsDictionaryEntries {
			get {
				if (EvaluatedItemsByName.Count == 0)
					yield break;

				foreach (KeyValuePair<string, BuildItemGroup> pair in EvaluatedItemsByName) {
					foreach (BuildItem bi in pair.Value)
						yield return new DictionaryEntry (pair.Key, bi.ConvertToITaskItem (null, ExpressionOptions.ExpandItemRefs));
				}
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

		// For batching implementation
		Dictionary<string, BuildItemGroup> perBatchItemsByName;
		Dictionary<string, BuildItemGroup> commonItemsByName;

		struct Batch {
			public Dictionary<string, BuildItemGroup> perBatchItemsByName;
			public Dictionary<string, BuildItemGroup> commonItemsByName;

			public Batch (Dictionary<string, BuildItemGroup> perBatchItemsByName, Dictionary<string, BuildItemGroup> commonItemsByName)
			{
				this.perBatchItemsByName = perBatchItemsByName;
				this.commonItemsByName = commonItemsByName;
			}
		}

		Stack<Batch> Batches {
			get { return batches; }
		}

		internal void PushBatch (Dictionary<string, BuildItemGroup> perBatchItemsByName, Dictionary<string, BuildItemGroup> commonItemsByName)
		{
			batches.Push (new Batch (perBatchItemsByName, commonItemsByName));
			SetBatchedItems (perBatchItemsByName, commonItemsByName);
		}

		internal void PopBatch ()
		{
			batches.Pop ();
			if (batches.Count > 0) {
				Batch b = batches.Peek ();
				SetBatchedItems (b.perBatchItemsByName, b.commonItemsByName);
			} else {
				SetBatchedItems (null, null);
			}
		}

		void SetBatchedItems (Dictionary<string, BuildItemGroup> perBatchItemsByName, Dictionary<string, BuildItemGroup> commonItemsByName)
		{
			this.perBatchItemsByName = perBatchItemsByName;
			this.commonItemsByName = commonItemsByName;
		}

		// Honors batching
		internal bool TryGetEvaluatedItemByNameBatched (string itemName, out BuildItemGroup group)
		{
			if (perBatchItemsByName != null && perBatchItemsByName.TryGetValue (itemName, out group))
				return true;

			if (commonItemsByName != null && commonItemsByName.TryGetValue (itemName, out group))
				return true;

			group = null;
			return EvaluatedItemsByName.TryGetValue (itemName, out group);
		}

		internal string GetMetadataBatched (string itemName, string metadataName)
		{
			BuildItemGroup group = null;
			if (itemName == null) {
				//unqualified, all items in a batch(bucket) have the
				//same metadata values
				group = GetFirst<BuildItemGroup> (perBatchItemsByName.Values);
				if (group == null)
					group = GetFirst<BuildItemGroup> (commonItemsByName.Values);
			} else {
				//qualified
				TryGetEvaluatedItemByNameBatched (itemName, out group);
			}

			if (group != null) {
				foreach (BuildItem item in group) {
					if (item.HasMetadata (metadataName))
						return item.GetEvaluatedMetadata (metadataName);
				}
			}
			return String.Empty;
		}

		internal IEnumerable<BuildItemGroup> GetAllItemGroups ()
		{
			if (perBatchItemsByName == null && commonItemsByName == null)
				foreach (BuildItemGroup group in EvaluatedItemsByName.Values)
					yield return group;

			if (perBatchItemsByName != null)
				foreach (BuildItemGroup group in perBatchItemsByName.Values)
					yield return group;

			if (commonItemsByName != null)
				foreach (BuildItemGroup group in commonItemsByName.Values)
					yield return group;
		}

		T GetFirst<T> (ICollection<T> list)
		{
			if (list == null)
				return default (T);

			foreach (T t in list)
				return t;

			return default (T);
		}

		internal void LogWarning (string filename, string message, params object[] messageArgs)
		{
			BuildWarningEventArgs bwea = new BuildWarningEventArgs (
				null, null, filename, 0, 0, 0, 0, String.Format (message, messageArgs),
				null, null);
			ParentEngine.EventSource.FireWarningRaised (this, bwea);
		}

		internal void LogError (string filename, string message,
				     params object[] messageArgs)
		{
			BuildErrorEventArgs beea = new BuildErrorEventArgs (
				null, null, filename, 0, 0, 0, 0, String.Format (message, messageArgs),
				null, null);
			ParentEngine.EventSource.FireErrorRaised (this, beea);
		}

		static string ExtensionsPath {
			get {
				if (extensions_path == null) {
					// NOTE: code from mcs/tools/gacutil/driver.cs
					PropertyInfo gac = typeof (System.Environment).GetProperty (
							"GacPath", BindingFlags.Static | BindingFlags.NonPublic);

					if (gac != null) {
						MethodInfo get_gac = gac.GetGetMethod (true);
						string gac_path = (string) get_gac.Invoke (null, null);
						extensions_path = Path.GetFullPath (Path.Combine (
									gac_path, Path.Combine ("..", "xbuild")));
					}
				}
				return extensions_path;
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

		internal IEnumerable EvaluatedPropertiesAsDictionaryEntries {
			get {
				foreach (BuildProperty bp in EvaluatedProperties)
					yield return new DictionaryEntry (bp.Name, bp.Value);
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
			get {
				return String.Join ("; ", initialTargets.ToArray ());
			}
			set {
				initialTargets.Clear ();
				xmlDocument.DocumentElement.SetAttribute ("InitialTargets", value);
				if (value != null)
					initialTargets.AddRange (value.Split (
								new char [] {';', ' '}, StringSplitOptions.RemoveEmptyEntries));
			}
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

		// corresponds to the xml attribute
		public string DefaultToolsVersion {
			get {
				if (xmlDocument != null)
					return xmlDocument.DocumentElement.GetAttribute ("ToolsVersion");
				return null;
			}
			set {
				if (xmlDocument != null)
					xmlDocument.DocumentElement.SetAttribute ("ToolsVersion", value);
			}
		}

		public bool HasToolsVersionAttribute {
			get {
				return xmlDocument != null && xmlDocument.DocumentElement.HasAttribute ("ToolsVersion");
			}
		}
		
		public string ToolsVersion {
			get; internal set;
		}

		internal List<string> BuiltTargetKeys {
			get { return builtTargetKeys; }
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

		static string OS {
			get {
				PlatformID pid = Environment.OSVersion.Platform;
				switch ((int)pid) {
				case 128:
				case 4:
					return "Unix";
				case 6:
					return "OSX";
				default:
					return "Windows_NT";
				}
			}
		}

	}
}

#endif
