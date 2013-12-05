//
// ProjectInstance.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2011,2013 Xamarin Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Internal.Expressions;
using Microsoft.Build.Logging;

//
// It is not always consistent to reuse Project and its evaluation stuff mostly because
// both BuildParameters.ctor() and Project.ctor() takes arbitrary ProjectCollection, which are not very likely eqivalent
// (as BuildParameters.ctor(), unlike Project.ctor(...), is known to create a new ProjectCollection instance).
//
// However, that inconsistency could happen even if you only use ProjectInstance and BuildParameters.
// They both have constructors that take ProjectCollection and there is no guarantee that the arguments are the same.
// BuildManager.Build() does not fail because of inconsistent ProjectCollection instance on .NET.
//
// Anyhow, I'm not going to instantiate Project within ProjectInstance code for another reason:
// ProjectCollection.GetLoadedProject() does not return any Project instnace for corresponding ProjectInstance
// (or I should say, ProjectRootElement for both).
using Microsoft.Build.Internal;
using System.Xml;
using Microsoft.Build.Exceptions;
using System.IO;


namespace Microsoft.Build.Execution
{
	public class ProjectInstance
	{
		// instance members
		
		public ProjectInstance (ProjectRootElement xml)
			: this (xml, null, null, ProjectCollection.GlobalProjectCollection)
		{
		}

		public ProjectInstance (string projectFile)
			: this (projectFile, null, null, ProjectCollection.GlobalProjectCollection)
		{
		}

		public ProjectInstance (string projectFile, IDictionary<string, string> globalProperties,
				string toolsVersion)
			: this (projectFile, globalProperties, toolsVersion, ProjectCollection.GlobalProjectCollection)
		{
		}

		public ProjectInstance (ProjectRootElement xml, IDictionary<string, string> globalProperties,
				string toolsVersion, ProjectCollection projectCollection)
		{
			projects = projectCollection;
			global_properties = globalProperties ?? new Dictionary<string, string> ();
			tools_version = !string.IsNullOrEmpty (toolsVersion) ? toolsVersion :
				!string.IsNullOrEmpty (xml.ToolsVersion) ? xml.ToolsVersion :
				projects.DefaultToolsVersion;
			InitializeProperties (xml, null);
		}

		public ProjectInstance (string projectFile, IDictionary<string, string> globalProperties,
				string toolsVersion, ProjectCollection projectCollection)
			: this (ProjectRootElement.Create (projectFile), globalProperties, toolsVersion, projectCollection)
		{
		}

		ProjectCollection projects;
		IDictionary<string, string> global_properties;
		
		string full_path, directory;
		#if NET_4_5
		ElementLocation location;
		#endif
		
		Dictionary<string, ProjectItemDefinitionInstance> item_definitions;
		List<ResolvedImport> raw_imports; // maybe we don't need this...
		List<ProjectItemInstance> all_evaluated_items;
		List<ProjectItemInstance> raw_items;
		List<ProjectPropertyInstance> properties;
		Dictionary<string, ProjectTargetInstance> targets;
		string tools_version;
		
		List<string> GetDefaultTargets (ProjectRootElement xml)
		{
			var ret = xml.DefaultTargets.Split (item_target_sep, StringSplitOptions.RemoveEmptyEntries).Select (s => s.Trim ()).ToList ();
			if (ret.Count == 0 && xml.Targets.Any ())
				ret.Add (xml.Targets.First ().Name);
			return ret;
		}

		void InitializeProperties (ProjectRootElement xml, ProjectInstance parent)
		{
			#if NET_4_5
			location = xml.Location;
			#endif
			full_path = xml.FullPath;
			directory = string.IsNullOrWhiteSpace (xml.DirectoryPath) ? System.IO.Directory.GetCurrentDirectory () : xml.DirectoryPath;
			DefaultTargets = GetDefaultTargets (xml);
			InitialTargets = xml.InitialTargets.Split (item_target_sep, StringSplitOptions.RemoveEmptyEntries).Select (s => s.Trim ()).ToList ();

			raw_imports = new List<ResolvedImport> ();
			item_definitions = new Dictionary<string, ProjectItemDefinitionInstance> ();
			targets = new Dictionary<string, ProjectTargetInstance> ();
			raw_items = new List<ProjectItemInstance> ();
			
			// FIXME: this is likely hack. Test ImportedProject.Properties to see what exactly should happen.
			if (parent != null) {
				properties = parent.properties;
			} else {
				properties = new List<ProjectPropertyInstance> ();
			
				foreach (DictionaryEntry p in Environment.GetEnvironmentVariables ())
					// FIXME: this is kind of workaround for unavoidable issue that PLATFORM=* is actually given
					// on some platforms and that prevents setting default "PLATFORM=AnyCPU" property.
					if (!string.Equals ("PLATFORM", (string) p.Key, StringComparison.OrdinalIgnoreCase))
						this.properties.Add (new ProjectPropertyInstance ((string) p.Key, false, (string) p.Value));
				foreach (var p in global_properties)
					this.properties.Add (new ProjectPropertyInstance (p.Key, false, p.Value));
				var tools = projects.GetToolset (tools_version) ?? projects.GetToolset (projects.DefaultToolsVersion);
				foreach (var p in projects.GetReservedProperties (tools, this, xml))
					this.properties.Add (p);
				foreach (var p in ProjectCollection.GetWellKnownProperties (this))
					this.properties.Add (p);
			}

			ProcessXml (parent, xml);
		}
		
		static readonly char [] item_target_sep = {';'};
		
		void ProcessXml (ProjectInstance parent, ProjectRootElement xml)
		{
			TaskDatabase = new BuildTaskDatabase (this, xml);
			
			// this needs to be initialized here (regardless of that items won't be evaluated at property evaluation;
			// Conditions could incorrectly reference items and lack of this list causes NRE.
			all_evaluated_items = new List<ProjectItemInstance> ();

			// property evaluation happens couple of times.
			// At first step, all non-imported properties are evaluated TOO, WHILE those properties are being evaluated.
			// This means, Include and IncludeGroup elements with Condition attribute MAY contain references to
			// properties and they will be expanded.
			var elements = EvaluatePropertiesAndImports (xml.Children).ToArray (); // ToArray(): to not lazily evaluate elements.
			
			// next, evaluate items
			EvaluateItems (xml, elements);
			
			// finally, evaluate targets and tasks
			EvaluateTasks (elements);			
		}
		
		IEnumerable<ProjectElement> EvaluatePropertiesAndImports (IEnumerable<ProjectElement> elements)
		{
			// First step: evaluate Properties
			foreach (var child in elements) {
				yield return child;
				var pge = child as ProjectPropertyGroupElement;
				if (pge != null && EvaluateCondition (pge.Condition))
					foreach (var p in pge.Properties)
						// do not allow overwriting reserved or well-known properties by user
						if (!this.properties.Any (_ => (_.IsImmutable) && _.Name.Equals (p.Name, StringComparison.InvariantCultureIgnoreCase)))
							if (EvaluateCondition (p.Condition))
								this.properties.Add (new ProjectPropertyInstance (p.Name, false, ExpandString (p.Value)));

				var ige = child as ProjectImportGroupElement;
				if (ige != null && EvaluateCondition (ige.Condition)) {
					foreach (var incc in ige.Imports) {
						foreach (var e in Import (incc))
							yield return e;
					}
				}
				var inc = child as ProjectImportElement;
				if (inc != null && EvaluateCondition (inc.Condition))
					foreach (var e in Import (inc))
						yield return e;
			}
		}
		
		internal IEnumerable<T> GetAllItems<T> (string include, string exclude, Func<string,T> creator, Func<string,ITaskItem> taskItemCreator, Func<string,bool> itemTypeCheck, Action<T,string> assignRecurse)
		{
			return ProjectCollection.GetAllItems<T> (ExpandString, include, exclude, creator, taskItemCreator, Directory, assignRecurse,
				t => all_evaluated_items.Any (i => i.EvaluatedInclude == t.ItemSpec && itemTypeCheck (i.ItemType)));
		}

		void EvaluateItems (ProjectRootElement xml, IEnumerable<ProjectElement> elements)
		{
			foreach (var child in elements) {
				var ige = child as ProjectItemGroupElement;
				if (ige != null) {
					foreach (var p in ige.Items) {
						if (!EvaluateCondition (ige.Condition) || !EvaluateCondition (p.Condition))
							continue;
						Func<string,ProjectItemInstance> creator = s => new ProjectItemInstance (this, p.ItemType, p.Metadata.Select (m => new KeyValuePair<string,string> (m.Name, m.Value)).ToList (), s);
						foreach (var item in GetAllItems (p.Include, p.Exclude, creator, s => new ProjectTaskItem (p, s), it => string.Equals (it, p.ItemType, StringComparison.OrdinalIgnoreCase), (t, s) => t.RecursiveDir = s)) {
							raw_items.Add (item);
							all_evaluated_items.Add (item);
						}
					}
				}
				var def = child as ProjectItemDefinitionGroupElement;
				if (def != null) {
					foreach (var p in def.ItemDefinitions) {
						if (EvaluateCondition (p.Condition)) {
							ProjectItemDefinitionInstance existing;
							if (!item_definitions.TryGetValue (p.ItemType, out existing))
								item_definitions.Add (p.ItemType, (existing = new ProjectItemDefinitionInstance (p)));
							existing.AddItems (p);
						}
					}
				}
			}
			all_evaluated_items.Sort ((p1, p2) => string.Compare (p1.ItemType, p2.ItemType, StringComparison.OrdinalIgnoreCase));
		}
		
		void EvaluateTasks (IEnumerable<ProjectElement> elements)
		{
			foreach (var child in elements) {
				var te = child as ProjectTargetElement;
				if (te != null)
					this.targets.Add (te.Name, new ProjectTargetInstance (te));
			}
		}
		
		IEnumerable<ProjectElement> Import (ProjectImportElement import)
		{
			string dir = projects.GetEvaluationTimeThisFileDirectory (() => FullPath);
			string path = WindowsCompatibilityExtensions.NormalizeFilePath (ExpandString (import.Project));
			path = Path.IsPathRooted (path) ? path : dir != null ? Path.Combine (dir, path) : Path.GetFullPath (path);
			if (projects.OngoingImports.Contains (path))
				throw new InvalidProjectFileException (import.Location, null, string.Format ("Circular imports was detected: {0} is already on \"importing\" stack", path));
			projects.OngoingImports.Push (path);
			try {
				using (var reader = XmlReader.Create (path)) {
					var root = ProjectRootElement.Create (reader, projects);
					if (DefaultTargets.Count == 0)
						DefaultTargets.AddRange (GetDefaultTargets (root));
					raw_imports.Add (new ResolvedImport (import, root, true));
					return this.EvaluatePropertiesAndImports (root.Children).ToArray ();
				}
			} finally {
				projects.OngoingImports.Pop ();
			}
		}

		internal IEnumerable<ProjectItemInstance> AllEvaluatedItems {
			get { return all_evaluated_items; }
		}

		public List<string> DefaultTargets { get; private set; }
		
		public string Directory {
			get { return directory; }
		}
		
		public string FullPath {
			get { return full_path; }
		}
		
		public IDictionary<string, string> GlobalProperties {
			get { return global_properties; }
		}
		
		public List<string> InitialTargets { get; private set; }
		
#if NET_4_5		
		public bool IsImmutable {
			get { throw new NotImplementedException (); }
		}
#endif
		
		public IDictionary<string, ProjectItemDefinitionInstance> ItemDefinitions {
			get { return item_definitions; }
		}
		
		public ICollection<ProjectItemInstance> Items {
			get { return all_evaluated_items; }
		}
		
		public ICollection<string> ItemTypes {
			get { return all_evaluated_items.Select (i => i.ItemType).Distinct ().ToArray (); }
		}

#if NET_4_5		
		public ElementLocation ProjectFileLocation {
			get { return location; }
		}
#endif

		public ICollection<ProjectPropertyInstance> Properties {
			get { return properties; }
		}
		
		#if NET_4_5
		public
		#else
		internal
		#endif
		IDictionary<string, ProjectTargetInstance> Targets {
			get { return targets; }
		}
		
		public string ToolsVersion {
			get { return tools_version; }
		}

		public ProjectItemInstance AddItem (string itemType, string evaluatedInclude)
		{
			return AddItem (itemType, evaluatedInclude, new KeyValuePair<string, string> [0]);
		}
		
		public ProjectItemInstance AddItem (string itemType, string evaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata)
		{
			var item = new ProjectItemInstance (this, itemType, metadata, evaluatedInclude);
			raw_items.Add (item);
			all_evaluated_items.Add (item);
			return item;
		}

		public bool Build ()
		{
			return Build (new ILogger [0]);
		}

		public bool Build (IEnumerable<ILogger> loggers)
		{
			return Build (loggers, new ForwardingLoggerRecord [0]);
		}
		
		public bool Build (IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers)
		{
			return Build (DefaultTargets.ToArray (), loggers, remoteLoggers);
		}

		public bool Build (string target, IEnumerable<ILogger> loggers)
		{
			return Build (target, loggers, new ForwardingLoggerRecord [0]);
		}

		public bool Build (string [] targets, IEnumerable<ILogger> loggers)
		{
			return Build (targets, loggers, new ForwardingLoggerRecord [0]);
		}
		
		public bool Build (string target, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers)
		{
			return Build (new string [] {target}, loggers, remoteLoggers);
		}
		
		public bool Build (string [] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers)
		{
			IDictionary<string, TargetResult> outputs;
			return Build (targets, loggers, remoteLoggers, out outputs);
		}

		public bool Build (string[] targets, IEnumerable<ILogger> loggers, out IDictionary<string, TargetResult> targetOutputs)
		{
			return Build (targets, loggers, new ForwardingLoggerRecord [0], out targetOutputs);
		}

		public bool Build (string[] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers, out IDictionary<string, TargetResult> targetOutputs)
		{
			var manager = new BuildManager ();
			var parameters = new BuildParameters (projects) {
				ForwardingLoggers = remoteLoggers,
				Loggers = loggers,
			};
			var requestData = new BuildRequestData (this, targets);
			var result = manager.Build (parameters, requestData);
			targetOutputs = result.ResultsByTarget;
			return result.OverallResult == BuildResultCode.Success;
		}
		
		public ProjectInstance DeepCopy ()
		{
			return DeepCopy (false);
		}
		
		public ProjectInstance DeepCopy (bool isImmutable)
		{
			throw new NotImplementedException ();
		}
		
		public bool EvaluateCondition (string condition)
		{
			return string.IsNullOrWhiteSpace (condition) || new ExpressionEvaluator (this, null).EvaluateAsBoolean (condition);
		}

		public string ExpandString (string unexpandedValue)
		{
			return ExpandString (unexpandedValue, null);
		}
		
		string ExpandString (string unexpandedValue, string replacementForMissingStuff)
		{
			return new ExpressionEvaluator (this, replacementForMissingStuff).Evaluate (unexpandedValue);
		}

		public ICollection<ProjectItemInstance> GetItems (string itemType)
		{
			return new CollectionFromEnumerable<ProjectItemInstance> (Items.Where (p => p.ItemType.Equals (itemType, StringComparison.OrdinalIgnoreCase)));
		}

		public IEnumerable<ProjectItemInstance> GetItemsByItemTypeAndEvaluatedInclude (string itemType, string evaluatedInclude)
		{
			throw new NotImplementedException ();
		}

		public ProjectPropertyInstance GetProperty (string name)
		{
			return properties.FirstOrDefault (p => p.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
		}
		
		public string GetPropertyValue (string name)
		{
			var prop = GetProperty (name);
			return prop != null ? prop.EvaluatedValue : string.Empty;
		}
		
		public bool RemoveItem (ProjectItemInstance item)
		{
			// yeah, this raw_items should vanish...
			raw_items.Remove (item);
			return all_evaluated_items.Remove (item);
		}

		public bool RemoveProperty (string name)
		{
			var removed = properties.FirstOrDefault (p => p.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
			if (removed == null)
				return false;
			properties.Remove (removed);
			return true;
		}
		
		public ProjectPropertyInstance SetProperty (string name, string evaluatedValue)
		{
			var p = new ProjectPropertyInstance (name, false, evaluatedValue);
			properties.Add (p);
			return p;
		}
		
		public ProjectRootElement ToProjectRootElement ()
		{
			throw new NotImplementedException ();
		}
		
#if NET_4_5
		public void UpdateStateFrom (ProjectInstance projectState)
		{
			throw new NotImplementedException ();
		}
#endif
		
		// static members		

		public static string GetEvaluatedItemIncludeEscaped (ProjectItemDefinitionInstance item)
		{
			// ?? ItemDefinition does not have Include attribute. What's the point here?
			throw new NotImplementedException ();
		}

		public static string GetEvaluatedItemIncludeEscaped (ProjectItemInstance item)
		{
			return ProjectCollection.Escape (item.EvaluatedInclude);
		}

		public static string GetMetadataValueEscaped (ProjectMetadataInstance metadatum)
		{
			return ProjectCollection.Escape (metadatum.EvaluatedValue);
		}
		
		public static string GetMetadataValueEscaped (ProjectItemDefinitionInstance item, string name)
		{
			var md = item.Metadata.FirstOrDefault (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
			return md != null ? ProjectCollection.Escape (md.EvaluatedValue) : null;
		}
		
		public static string GetMetadataValueEscaped (ProjectItemInstance item, string name)
		{
			var md = item.Metadata.FirstOrDefault (m => m.Name.Equals (name, StringComparison.OrdinalIgnoreCase));
			return md != null ? ProjectCollection.Escape (md.EvaluatedValue) : null;
		}

		public static string GetPropertyValueEscaped (ProjectPropertyInstance property)
		{
			// WTF happens here.
			//return ProjectCollection.Escape (property.EvaluatedValue);
			return property.EvaluatedValue;
		}

		internal BuildTaskDatabase TaskDatabase { get; private set; }
		
		internal string GetFullPath (string pathRelativeToProject)
		{
			if (Path.IsPathRooted (pathRelativeToProject))
				return pathRelativeToProject;
			return Path.GetFullPath (Path.Combine (Directory, pathRelativeToProject));
		}
	}
}

