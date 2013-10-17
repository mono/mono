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
using Microsoft.Build.Internal;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using System.Collections;

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

			Initialize ();
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
		List<string> item_types;
		List<ProjectProperty> properties;
		Dictionary<string, ProjectTargetInstance> targets;

		void Initialize ()
		{
			dir_path = Directory.GetCurrentDirectory ();
			raw_imports = new List<ResolvedImport> ();
			item_definitions = new Dictionary<string, ProjectItemDefinition> ();
			item_types = new List<string> ();
			properties = new List<ProjectProperty> ();
			targets = new Dictionary<string, ProjectTargetInstance> ();
			raw_items = new List<ProjectItem> ();
			
			ProcessXml ();
		}
		
		static readonly char [] item_sep = {';'};
		
		void ProcessXml ()
		{
			foreach (DictionaryEntry p in Environment.GetEnvironmentVariables ())
				this.properties.Add (new EnvironmentProjectProperty (this, (string)p.Key, (string)p.Value));
			foreach (var p in GlobalProperties)
				this.properties.Add (new GlobalProjectProperty (this, p.Key, p.Value));

			all_evaluated_items = new List<ProjectItem> ();
			foreach (var child in Xml.Children) {
				var pge = child as ProjectPropertyGroupElement;
				if (pge != null)
					foreach (var p in pge.Properties)
						this.properties.Add (new XmlProjectProperty (this, p, PropertyType.Normal));
				var ige = child as ProjectItemGroupElement;
				if (ige != null) {
					foreach (var p in ige.Items) {
						var inc = ExpandString (p.Include);
						foreach (var each in inc.Split (item_sep, StringSplitOptions.RemoveEmptyEntries)) {
							var item = new ProjectItem (this, p, each);
							this.raw_items.Add (item);
							if (ShouldInclude (ige.Condition) && ShouldInclude (p.Condition))
								all_evaluated_items.Add (item);
						}
					}
				}
				var def = child as ProjectItemDefinitionGroupElement;
				if (def != null) {
					foreach (var p in def.ItemDefinitions) {
						if (ShouldInclude (p.Condition)) {
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

		public bool Build ()
		{
			return Build (Xml.DefaultTargets.Split (';'));
		}

		public bool Build (IEnumerable<ILogger> loggers)
		{
			return Build (Xml.DefaultTargets.Split (';'), loggers);
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
			return Build (Xml.DefaultTargets.Split (';'), new ILogger [] {logger});
		}

		public bool Build (string[] targets, IEnumerable<ILogger> loggers)
		{
			return Build (targets, loggers, new ForwardingLoggerRecord [0]);
		}

		public bool Build (IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers)
		{
			return Build (Xml.DefaultTargets.Split (';'), loggers, remoteLoggers);
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
		
		bool ShouldInclude (string unexpandedValue)
		{
			return string.IsNullOrWhiteSpace (unexpandedValue) || new ExpressionEvaluator (this).EvaluateAsBoolean (unexpandedValue);
		}

		public string ExpandString (string unexpandedValue)
		{
			return new ExpressionEvaluator (this).Evaluate (unexpandedValue);
		}

		public static string GetEvaluatedItemIncludeEscaped (ProjectItem item)
		{
			return ProjectCollection.Escape (item.EvaluatedInclude);
		}

		public static string GetEvaluatedItemIncludeEscaped (ProjectItemDefinition item)
		{
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
			var md = item.GetMetadata (name);
			return md != null ? ProjectCollection.Escape (md.EvaluatedValue) : null;
		}

		public static string GetMetadataValueEscaped (ProjectItemDefinition item, string name)
		{
			var md = item.Metadata.FirstOrDefault (m => m.Name == name);
			return md != null ? ProjectCollection.Escape (md.EvaluatedValue) : null;
		}

		public string GetPropertyValue (string name)
		{
			var prop = GetProperty (name);
			return prop != null ? prop.EvaluatedValue : string.Empty;
		}

		public static string GetPropertyValueEscaped (ProjectProperty property)
		{
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
			var removed = properties.FirstOrDefault (p => p.Name == property.Name);
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
			get { throw new NotImplementedException (); }
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

		public IList<ResolvedImport> Imports {
			get { throw new NotImplementedException (); }
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

		public ICollection<ProjectProperty> Properties {
			get { return properties; }
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
	}
}
