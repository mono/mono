//
// Project.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2011 Leszek Ciesielski
// Copyright (C) 2011,2013 Xamarin Inc. (http://www.xamarin.com)
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
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Internal;
using Microsoft.Build.Internal.Expressions;
using Microsoft.Build.Logging;
using System.Collections;

// Basically there are two semantic Project object models and their relationship is not obvious
// (apart from Microsoft.Build.Construction.ProjectRootElement which is a "construction rule").
//
// Microsoft.Build.Evaluation.Project holds some "editable" project model, and it supports
// detailed loader API (such as Items and AllEvaluatedItems).
// ProjectPoperty holds UnevaluatedValue and gives EvaluatedValue too.
//
// Microsoft.Build.Execution.ProjectInstance holds "snapshot" of a project, and it lacks
// detailed loader API. It does not give us Unevaluated property value.
// On the other hand, it supports Targets object model. What Microsoft.Build.Evaluation.Project
// offers there is actually a list of Microsoft.Build.Execution.ProjectInstance objects.
// It should be also noted that only ProjectInstance has Evaluate() method (Project doesn't).
//
// And both API holds different set of descendant types for each and cannot really share the
// loader code. That is lame.
//
// So, can either of them be used to construct the other model? Both API models share the same
// "governor", which is Microsoft.Build.Evaluation.ProjectCollection/ Project is added to
// its LoadedProjects list, while ProjectInstance isn't. Project cannot be loaded to load
// a ProjectInstance, at least within the same ProjectCollection.
//
// On the other hand, can ProjectInstance be used to load a Project? Maybe. Since Project and
// its descendants need Microsoft.Build.Construction.ProjectElement family as its API model
// is part of the public API. Then I still have to understand how those AllEvaluatedItems/
// AllEvaluatedProperties members make sense. EvaluationCounter is another propery in question.

namespace Microsoft.Build.Evaluation
{
	[DebuggerDisplay ("{FullPath} EffectiveToolsVersion={ToolsVersion} #GlobalProperties="
	+ "{data.globalProperties.Count} #Properties={data.Properties.Count} #ItemTypes="
	+ "{data.ItemTypes.Count} #ItemDefinitions={data.ItemDefinitions.Count} #Items="
	+ "{data.Items.Count} #Targets={data.Targets.Count}")]
	public class Project
	{
		public Project (XmlReader xml)
			: this (ProjectRootElement.Create (xml))
		{
		}

		public Project (XmlReader xml, IDictionary<string, string> globalProperties,
		                              string toolsVersion)
			: this (ProjectRootElement.Create (xml), globalProperties, toolsVersion)
		{
		}

		public Project (XmlReader xml, IDictionary<string, string> globalProperties,
		                              string toolsVersion, ProjectCollection projectCollection)
			: this (ProjectRootElement.Create (xml), globalProperties, toolsVersion, projectCollection)
		{
		}

		public Project (XmlReader xml, IDictionary<string, string> globalProperties,
		                              string toolsVersion, ProjectCollection projectCollection,
		                              ProjectLoadSettings loadSettings)
			: this (ProjectRootElement.Create (xml), globalProperties, toolsVersion, projectCollection, loadSettings)
		{
		}

		public Project (ProjectRootElement xml) : this (xml, null, null)
		{
		}

		public Project (ProjectRootElement xml, IDictionary<string, string> globalProperties,
		                              string toolsVersion)
                        : this (xml, globalProperties, toolsVersion, ProjectCollection.GlobalProjectCollection)
		{
		}

		public Project (ProjectRootElement xml, IDictionary<string, string> globalProperties,
		                              string toolsVersion, ProjectCollection projectCollection)
                        : this (xml, globalProperties, toolsVersion, projectCollection, ProjectLoadSettings.Default)
		{
		}

		public Project (ProjectRootElement xml, IDictionary<string, string> globalProperties,
		                              string toolsVersion, ProjectCollection projectCollection,
		                              ProjectLoadSettings loadSettings)
		{
			if (projectCollection == null)
				throw new ArgumentNullException ("projectCollection");
			this.Xml = xml;
			this.GlobalProperties = globalProperties ?? new Dictionary<string, string> ();
			this.ToolsVersion = toolsVersion;
			this.ProjectCollection = projectCollection;
			this.load_settings = loadSettings;

			Initialize (null);
		}
		
		Project (ProjectRootElement imported, Project parent)
		{
			this.Xml = imported;
			this.GlobalProperties = parent.GlobalProperties;
			this.ToolsVersion = parent.ToolsVersion;
			this.ProjectCollection = parent.ProjectCollection;
			this.load_settings = parent.load_settings;

			Initialize (parent);
		}

		public Project (string projectFile)
			: this (projectFile, null, null)
		{
		}

		public Project (string projectFile, IDictionary<string, string> globalProperties,
				string toolsVersion)
        	: this (projectFile, globalProperties, toolsVersion, ProjectCollection.GlobalProjectCollection, ProjectLoadSettings.Default)
		{
		}

		public Project (string projectFile, IDictionary<string, string> globalProperties,
				string toolsVersion, ProjectCollection projectCollection)
        	: this (projectFile, globalProperties, toolsVersion, projectCollection, ProjectLoadSettings.Default)
		{
		}

		public Project (string projectFile, IDictionary<string, string> globalProperties,
				string toolsVersion, ProjectCollection projectCollection,
				ProjectLoadSettings loadSettings)
			: this (ProjectRootElement.Create (projectFile), globalProperties, toolsVersion, projectCollection, loadSettings)
		{
		}

		ProjectLoadSettings load_settings;

		public IDictionary<string, string> GlobalProperties { get; private set; }

		public ProjectCollection ProjectCollection { get; private set; }

		public string ToolsVersion { get; private set; }

		public ProjectRootElement Xml { get; private set; }

		string dir_path;
		Dictionary<string, ProjectItemDefinition> item_definitions;
		List<ResolvedImport> raw_imports;
		List<ProjectItem> raw_items;
		List<ProjectItem> all_evaluated_items;
		List<ProjectProperty> properties;
		Dictionary<string, ProjectTargetInstance> targets;

		void Initialize (Project parent)
		{
			dir_path = Directory.GetCurrentDirectory ();
			raw_imports = new List<ResolvedImport> ();
			item_definitions = new Dictionary<string, ProjectItemDefinition> ();
			targets = new Dictionary<string, ProjectTargetInstance> ();
			raw_items = new List<ProjectItem> ();
			
			// FIXME: this is likely hack. Test ImportedProject.Properties to see what exactly should happen.
			if (parent != null) {
				properties = parent.properties;
			} else {
				properties = new List<ProjectProperty> ();
			
				foreach (DictionaryEntry p in Environment.GetEnvironmentVariables ())
					// FIXME: this is kind of workaround for unavoidable issue that PLATFORM=* is actually given
					// on some platforms and that prevents setting default "PLATFORM=AnyCPU" property.
					if (!string.Equals ("PLATFORM", (string) p.Key, StringComparison.OrdinalIgnoreCase))
						this.properties.Add (new EnvironmentProjectProperty (this, (string)p.Key, (string)p.Value));
				foreach (var p in GlobalProperties)
					this.properties.Add (new GlobalProjectProperty (this, p.Key, p.Value));
				var tools = ProjectCollection.GetToolset (this.ToolsVersion) ?? ProjectCollection.GetToolset (this.ProjectCollection.DefaultToolsVersion);
				foreach (var p in ProjectCollection.GetReservedProperties (tools, this))
					this.properties.Add (p);
				foreach (var p in ProjectCollection.GetWellKnownProperties (this))
					this.properties.Add (p);
			}

			ProcessXml (parent);
			
			ProjectCollection.AddProject (this);
		}
		
		void ProcessXml (Project parent)
		{
			// this needs to be initialized here (regardless of that items won't be evaluated at property evaluation;
			// Conditions could incorrectly reference items and lack of this list causes NRE.
			all_evaluated_items = new List<ProjectItem> ();

			// property evaluation happens couple of times.
			// At first step, all non-imported properties are evaluated TOO, WHILE those properties are being evaluated.
			// This means, Include and IncludeGroup elements with Condition attribute MAY contain references to
			// properties and they will be expanded.
			var elements = EvaluatePropertiesAndImports (Xml.Children).ToArray (); // ToArray(): to not lazily evaluate elements.
			
			// next, evaluate items
			EvaluateItems (elements);
			
			// finally, evaluate targets and tasks
			EvaluateTargets (elements);
		}
		
		IEnumerable<ProjectElement> EvaluatePropertiesAndImports (IEnumerable<ProjectElement> elements)
		{
			// First step: evaluate Properties
			foreach (var child in elements) {
				yield return child;
				var pge = child as ProjectPropertyGroupElement;
				if (pge != null && Evaluate (pge.Condition))
					foreach (var p in pge.Properties)
						// do not allow overwriting reserved or well-known properties by user
						if (!this.properties.Any (_ => (_.IsReservedProperty || _.IsWellKnownProperty) && _.Name.Equals (p.Name, StringComparison.InvariantCultureIgnoreCase)))
							if (Evaluate (p.Condition))
								this.properties.Add (new XmlProjectProperty (this, p, PropertyType.Normal, ProjectCollection.OngoingImports.Any ()));

				var ige = child as ProjectImportGroupElement;
				if (ige != null && Evaluate (ige.Condition)) {
					foreach (var incc in ige.Imports) {
						foreach (var e in Import (incc))
							yield return e;
					}
				}
				var inc = child as ProjectImportElement;
				if (inc != null && Evaluate (inc.Condition))
					foreach (var e in Import (inc))
						yield return e;
			}
		}
		
		internal IEnumerable<T> GetAllItems<T> (string include, string exclude, Func<string,T> creator, Func<string,ITaskItem> taskItemCreator, Func<string,bool> itemTypeCheck, Action<T,string> assignRecurse)
		{
			return ProjectCollection.GetAllItems<T> (ExpandString, include, exclude, creator, taskItemCreator, DirectoryPath, assignRecurse,
				t => all_evaluated_items.Any (i => i.EvaluatedInclude == t.ItemSpec && itemTypeCheck (i.ItemType)));
		}

		void EvaluateItems (IEnumerable<ProjectElement> elements)
		{
			foreach (var child in elements) {
				var ige = child as ProjectItemGroupElement;
				if (ige != null) {
					foreach (var p in ige.Items) {
						if (!Evaluate (ige.Condition) || !Evaluate (p.Condition))
							continue;
						Func<string,ProjectItem> creator = s => new ProjectItem (this, p, s);
						foreach (var item in GetAllItems<ProjectItem> (p.Include, p.Exclude, creator, s => new ProjectTaskItem (p, s), it => string.Equals (it, p.ItemType, StringComparison.OrdinalIgnoreCase), (t, s) => t.RecursiveDir = s)) {
							raw_items.Add (item);
							all_evaluated_items.Add (item);
						}
					}
				}
				var def = child as ProjectItemDefinitionGroupElement;
				if (def != null) {
					foreach (var p in def.ItemDefinitions) {
						if (Evaluate (p.Condition)) {
							ProjectItemDefinition existing;
							if (!item_definitions.TryGetValue (p.ItemType, out existing))
								item_definitions.Add (p.ItemType, (existing = new ProjectItemDefinition (this, p.ItemType)));
							existing.AddItems (p);
						}
					}
				}
			}
			all_evaluated_items.Sort ((p1, p2) => string.Compare (p1.ItemType, p2.ItemType, StringComparison.OrdinalIgnoreCase));
		}
		
		void EvaluateTargets (IEnumerable<ProjectElement> elements)
		{
			foreach (var child in elements) {
				var te = child as ProjectTargetElement;
				if (te != null)
					this.targets.Add (te.Name, new ProjectTargetInstance (te));
			}
		}
		
		IEnumerable<ProjectElement> Import (ProjectImportElement import)
		{
			string dir = ProjectCollection.GetEvaluationTimeThisFileDirectory (() => FullPath);
			string path = WindowsCompatibilityExtensions.NormalizeFilePath (ExpandString (import.Project));
			path = Path.IsPathRooted (path) ? path : dir != null ? Path.Combine (dir, path) : Path.GetFullPath (path);
			if (ProjectCollection.OngoingImports.Contains (path)) {
				switch (load_settings) {
				case ProjectLoadSettings.RejectCircularImports:
					throw new InvalidProjectFileException (import.Location, null, string.Format ("Circular imports was detected: {0} (resolved as \"{1}\") is already on \"importing\" stack", import.Project, path));
				}
				return new ProjectElement [0]; // do not import circular references
			}
			ProjectCollection.OngoingImports.Push (path);
			try {
				using (var reader = XmlReader.Create (path)) {
					var root = ProjectRootElement.Create (reader, ProjectCollection);
					raw_imports.Add (new ResolvedImport (import, root, true));
					return this.EvaluatePropertiesAndImports (root.Children).ToArray ();
				}
			} finally {
				ProjectCollection.OngoingImports.Pop ();
			}
		}

		public ICollection<ProjectItem> GetItemsIgnoringCondition (string itemType)
		{
			return new CollectionFromEnumerable<ProjectItem> (raw_items.Where (p => p.ItemType.Equals (itemType, StringComparison.OrdinalIgnoreCase)));
		}

		public void RemoveItems (IEnumerable<ProjectItem> items)
		{
			var removal = new List<ProjectItem> (items);
			foreach (var item in removal) {
				var parent = item.Xml.Parent;
				parent.RemoveChild (item.Xml);
				if (parent.Count == 0)
					parent.Parent.RemoveChild (parent);
			}
		}

		static readonly Dictionary<string, string> empty_metadata = new Dictionary<string, string> ();

		public IList<ProjectItem> AddItem (string itemType, string unevaluatedInclude)
		{
			return AddItem (itemType, unevaluatedInclude, empty_metadata);
		}

		public IList<ProjectItem> AddItem (string itemType, string unevaluatedInclude,
				IEnumerable<KeyValuePair<string, string>> metadata)
		{
			// FIXME: needs several check that AddItemFast() does not process (see MSDN for details).

			return AddItemFast (itemType, unevaluatedInclude, metadata);
		}

		public IList<ProjectItem> AddItemFast (string itemType, string unevaluatedInclude)
		{
			return AddItemFast (itemType, unevaluatedInclude, empty_metadata);
		}

		public IList<ProjectItem> AddItemFast (string itemType, string unevaluatedInclude,
		                                                     IEnumerable<KeyValuePair<string, string>> metadata)
		{
			throw new NotImplementedException ();
		}
		
		static readonly char [] target_sep = new char[] {';'};

		public bool Build ()
		{
			return Build (Xml.DefaultTargets.Split (target_sep, StringSplitOptions.RemoveEmptyEntries));
		}

		public bool Build (IEnumerable<ILogger> loggers)
		{
			return Build (Xml.DefaultTargets.Split (target_sep, StringSplitOptions.RemoveEmptyEntries), loggers);
		}

		public bool Build (string target)
		{
			return string.IsNullOrWhiteSpace (target) ? Build () : Build (new string [] {target});
		}

		public bool Build (string[] targets)
		{
			return Build (targets, new ILogger [0]);
		}

		public bool Build (ILogger logger)
		{
			return Build (Xml.DefaultTargets.Split (target_sep, StringSplitOptions.RemoveEmptyEntries), new ILogger [] {logger});
		}

		public bool Build (string[] targets, IEnumerable<ILogger> loggers)
		{
			return Build (targets, loggers, new ForwardingLoggerRecord [0]);
		}

		public bool Build (IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers)
		{
			return Build (Xml.DefaultTargets.Split (target_sep, StringSplitOptions.RemoveEmptyEntries), loggers, remoteLoggers);
		}

		public bool Build (string target, IEnumerable<ILogger> loggers)
		{
			return Build (new string [] { target }, loggers);
		}

		public bool Build (string[] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers)
		{
			// Unlike ProjectInstance.Build(), there is no place to fill outputs by targets, so ignore them
			// (i.e. we don't use the overload with output).
			//
			// This does not check FullPath, so don't call GetProjectInstanceForBuild() directly.
			return new BuildManager ().GetProjectInstanceForBuildInternal (this).Build (targets, loggers, remoteLoggers);
		}

		public bool Build (string target, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers)
		{
			return Build (new string [] { target }, loggers, remoteLoggers);
		}

		public ProjectInstance CreateProjectInstance ()
		{
			var ret = new ProjectInstance (Xml, GlobalProperties, ToolsVersion, ProjectCollection);
			// FIXME: maybe fill other properties to the result.
			return ret;
		}
		
		bool Evaluate (string unexpandedValue)
		{
			return string.IsNullOrWhiteSpace (unexpandedValue) || new ExpressionEvaluator (this, null).EvaluateAsBoolean (unexpandedValue);
		}

		public string ExpandString (string unexpandedValue)
		{
			return ExpandString (unexpandedValue, null);
		}
		
		string ExpandString (string unexpandedValue, string replacementForMissingStuff)
		{
			return new ExpressionEvaluator (this, replacementForMissingStuff).Evaluate (unexpandedValue);
		}

		public static string GetEvaluatedItemIncludeEscaped (ProjectItem item)
		{
			return ProjectCollection.Escape (item.EvaluatedInclude);
		}

		public static string GetEvaluatedItemIncludeEscaped (ProjectItemDefinition item)
		{
			// ?? ItemDefinition does not have Include attribute. What's the point here?
			throw new NotImplementedException ();
		}

		public ICollection<ProjectItem> GetItems (string itemType)
		{
			return new CollectionFromEnumerable<ProjectItem> (Items.Where (p => p.ItemType.Equals (itemType, StringComparison.OrdinalIgnoreCase)));
		}

		public ICollection<ProjectItem> GetItemsByEvaluatedInclude (string evaluatedInclude)
		{
			return new CollectionFromEnumerable<ProjectItem> (Items.Where (p => p.EvaluatedInclude.Equals (evaluatedInclude, StringComparison.OrdinalIgnoreCase)));
		}

		public IEnumerable<ProjectElement> GetLogicalProject ()
		{
			throw new NotImplementedException ();
		}

		public static string GetMetadataValueEscaped (ProjectMetadata metadatum)
		{
			return ProjectCollection.Escape (metadatum.EvaluatedValue);
		}

		public static string GetMetadataValueEscaped (ProjectItem item, string name)
		{
			var md = item.Metadata.FirstOrDefault (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
			return md != null ? ProjectCollection.Escape (md.EvaluatedValue) : null;
		}

		public static string GetMetadataValueEscaped (ProjectItemDefinition item, string name)
		{
			var md = item.Metadata.FirstOrDefault (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
			return md != null ? ProjectCollection.Escape (md.EvaluatedValue) : null;
		}

		public string GetPropertyValue (string name)
		{
			var prop = GetProperty (name);
			return prop != null ? prop.EvaluatedValue : string.Empty;
		}

		public static string GetPropertyValueEscaped (ProjectProperty property)
		{
			// WTF happens here.
			//return ProjectCollection.Escape (property.EvaluatedValue);
			return property.EvaluatedValue;
		}

		public ProjectProperty GetProperty (string name)
		{
			return properties.FirstOrDefault (p => p.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
		}

		public void MarkDirty ()
		{
			if (!DisableMarkDirty)
				is_dirty = true;
		}

		public void ReevaluateIfNecessary ()
		{
			throw new NotImplementedException ();
		}

		public bool RemoveGlobalProperty (string name)
		{
			throw new NotImplementedException ();
		}

		public bool RemoveItem (ProjectItem item)
		{
			throw new NotImplementedException ();
		}

		public bool RemoveProperty (ProjectProperty property)
		{
			var removed = properties.FirstOrDefault (p => p.Name.Equals (property.Name, StringComparison.OrdinalIgnoreCase));
			if (removed == null)
				return false;
			properties.Remove (removed);
			return true;
		}

		public void Save ()
		{
			Xml.Save ();
		}

		public void Save (TextWriter writer)
		{
			Xml.Save (writer);
		}

		public void Save (string path)
		{
			Save (path, Encoding.Default);
		}

		public void Save (Encoding encoding)
		{
			Save (FullPath, encoding);
		}

		public void Save (string path, Encoding encoding)
		{
			using (var writer = new StreamWriter (path, false, encoding))
				Save (writer);
		}

		public void SaveLogicalProject (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		public bool SetGlobalProperty (string name, string escapedValue)
		{
			throw new NotImplementedException ();
		}

		public ProjectProperty SetProperty (string name, string unevaluatedValue)
		{
			var p = new ManuallyAddedProjectProperty (this, name, unevaluatedValue);
			properties.Add (p);
			return p;
		}

		public ICollection<ProjectMetadata> AllEvaluatedItemDefinitionMetadata {
			get { throw new NotImplementedException (); }
		}

		public ICollection<ProjectItem> AllEvaluatedItems {
			get { return all_evaluated_items; }
		}

		public ICollection<ProjectProperty> AllEvaluatedProperties {
			get { return properties; }
		}

		public IDictionary<string, List<string>> ConditionedProperties {
			get {
				// this property returns different instances every time.
				var dic = new Dictionary<string, List<string>> ();
				
				// but I dunno HOW this evaluates
				
				throw new NotImplementedException ();
			}
		}

		public string DirectoryPath {
			get { return dir_path; }
		}

		public bool DisableMarkDirty { get; set; }

		public int EvaluationCounter {
			get { throw new NotImplementedException (); }
		}

		public string FullPath {
			get { return Xml.FullPath; }
			set { Xml.FullPath = value; }
		}
		
		class ResolvedImportComparer : IEqualityComparer<ResolvedImport>
		{
			public static ResolvedImportComparer Instance = new ResolvedImportComparer ();
			
			public bool Equals (ResolvedImport x, ResolvedImport y)
			{
				return x.ImportedProject.FullPath.Equals (y.ImportedProject.FullPath);
			}
			public int GetHashCode (ResolvedImport obj)
			{
				return obj.ImportedProject.FullPath.GetHashCode ();
			}
		}

		public IList<ResolvedImport> Imports {
			get { return raw_imports.Distinct (ResolvedImportComparer.Instance).ToList (); }
		}

		public IList<ResolvedImport> ImportsIncludingDuplicates {
			get { return raw_imports; }
		}

		public bool IsBuildEnabled {
			get { return ProjectCollection.IsBuildEnabled; }
		}

		bool is_dirty;
		public bool IsDirty {
			get { return is_dirty; }
		}

		public IDictionary<string, ProjectItemDefinition> ItemDefinitions {
			get { return item_definitions; }
		}

		[MonoTODO ("should be different from AllEvaluatedItems")]
		public ICollection<ProjectItem> Items {
			get { return AllEvaluatedItems; }
		}

		public ICollection<ProjectItem> ItemsIgnoringCondition {
			get { return raw_items; }
		}

		public ICollection<string> ItemTypes {
			get { return new CollectionFromEnumerable<string> (raw_items.Select (i => i.ItemType).Distinct ()); }
		}

		[MonoTODO ("should be different from AllEvaluatedProperties")]
		public ICollection<ProjectProperty> Properties {
			get { return AllEvaluatedProperties; }
		}

		public bool SkipEvaluation { get; set; }

		#if NET_4_5
		public
		#else
		internal
		#endif
		IDictionary<string, ProjectTargetInstance> Targets {
			get { return targets; }
		}
		
		// These are required for reserved property, represents dynamically changing property values.
		// This should resolve to either the project file path or that of the imported file.
		internal string GetEvaluationTimeThisFileDirectory ()
		{
			var file = GetEvaluationTimeThisFile ();
			var dir = Path.IsPathRooted (file) ? Path.GetDirectoryName (file) : Directory.GetCurrentDirectory ();
			return dir + Path.DirectorySeparatorChar;
		}

		internal string GetEvaluationTimeThisFile ()
		{
			return ProjectCollection.OngoingImports.Count > 0 ? ProjectCollection.OngoingImports.Peek () : FullPath ?? string.Empty;
		}
		
		internal string GetFullPath (string pathRelativeToProject)
		{
			if (Path.IsPathRooted (pathRelativeToProject))
				return pathRelativeToProject;
			return Path.GetFullPath (Path.Combine (DirectoryPath, pathRelativeToProject));
		}
	}
}
