//
// ProjectInstance.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

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
			InitializeProperties ();
			
			throw new NotImplementedException ();
		}

		public ProjectInstance (string projectFile, IDictionary<string, string> globalProperties,
				string toolsVersion, ProjectCollection projectCollection)
		{
			InitializeProperties ();
			
			throw new NotImplementedException ();
		}
		
		void InitializeProperties ()
		{
			DefaultTargets = new List<string> ();
			InitialTargets = new List<string> ();
		}
		
		Dictionary<string, string> global_properties = new Dictionary<string, string> ();
		
		public List<string> DefaultTargets { get; private set; }
		
		public string Directory {
			get { throw new NotImplementedException (); }
		}
		
		public string FullPath {
			get { throw new NotImplementedException (); }
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
			get { throw new NotImplementedException (); }
		}
		
		public ICollection<ProjectItemInstance> Items {
			get { throw new NotImplementedException (); }
		}
		
		public ICollection<string> ItemTypes {
			get { throw new NotImplementedException (); }
		}

#if NET_4_5		
		public ElementLocation ProjectFileLocation {
			get { throw new NotImplementedException (); }
		}
#endif

		public ICollection<ProjectPropertyInstance> Properties {
			get { throw new NotImplementedException (); }
		}
		
		public IDictionary<string, ProjectTargetInstance> Targets {
			get { throw new NotImplementedException (); }
		}
		
		public string ToolsVersion {
			get { throw new NotImplementedException (); }
		}

		public ProjectItemInstance AddItem (string itemType, string evaluatedInclude)
		{
			return AddItem (itemType, evaluatedInclude, new KeyValuePair<string, string> [0]);
		}
		
		public ProjectItemInstance AddItem (string itemType, string evaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata)
		{
			throw new NotImplementedException ();
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
			return Build ((string []) null, loggers, remoteLoggers);
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
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		public string ExpandString (string unexpandedValue)
		{
			throw new NotImplementedException ();
		}

		public ICollection<ProjectItemInstance> GetItems (string itemType)
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<ProjectItemInstance> GetItemsByItemTypeAndEvaluatedInclude (string itemType, string evaluatedInclude)
		{
			throw new NotImplementedException ();
		}

		public ProjectPropertyInstance GetProperty (string name)
		{
			throw new NotImplementedException ();
		}
		
		public string GetPropertyValue (string name)
		{
			throw new NotImplementedException ();
		}
		
		public bool RemoveItem (ProjectItemInstance item)
		{
			throw new NotImplementedException ();
		}

		public bool RemoveProperty (string name)
		{
			throw new NotImplementedException ();
		}
		
		public ProjectPropertyInstance SetProperty (string name, string evaluatedValue)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		public static string GetEvaluatedItemIncludeEscaped (ProjectItemInstance item)
		{
			throw new NotImplementedException ();
		}

		public static string GetMetadataValueEscaped (ProjectMetadataInstance metadatum)
		{
			throw new NotImplementedException ();
		}
		
		public static string GetMetadataValueEscaped (ProjectItemDefinitionInstance item, string name)
		{
			throw new NotImplementedException ();
		}
		
		public static string GetMetadataValueEscaped (ProjectItemInstance item, string name)
		{
			throw new NotImplementedException ();
		}

		public static string GetPropertyValueEscaped (ProjectPropertyInstance property)
		{
			throw new NotImplementedException ();
		}
	}
}

