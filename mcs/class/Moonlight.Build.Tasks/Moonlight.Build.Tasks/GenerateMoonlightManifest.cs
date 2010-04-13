//
// GenerateMoonlightManifest.cs
//
// Author:
//	Michael Hutchinson <mhutchinson@novell.com>
//	Ankit Jain <jankit@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;

using Microsoft.CSharp;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Moonlight.Build.Tasks {
	public class GenerateMoonlightManifest : Task {

		public override bool Execute ()
		{
			return GenerateManifest ();
		}

		bool GenerateManifest ()
		{
			const string depNS = "http://schemas.microsoft.com/client/2007/deployment";

			string template = null;
			var manifest = ManifestFile.ItemSpec;
			Log.LogMessage (MessageImportance.Normal, "Generating manifest file {0}", manifest);

			if (SilverlightManifestTemplate != null)
				template = String.IsNullOrEmpty (SilverlightManifestTemplate.ItemSpec) ?
							null :
							SilverlightManifestTemplate.GetMetadata ("FullPath");

			XmlDocument doc = new XmlDocument ();
			if (template != null) {
				if (!File.Exists (template)) {
					Log.LogError ("Could not find manifest template '" +  template + "'.");
					return false;
				}

				try {
					doc.Load (template);
				} catch (XmlException ex) {
					Log.LogError (null, null, null, template, ex.LineNumber, ex.LinePosition, 0, 0,
							"Error loading manifest template '" + ex.Source);
					return false;
				} catch (Exception ex) {
					Log.LogError ("Could not load manifest template '" +  template + "'.");
					Log.LogMessage (MessageImportance.Low, "Could not load manifest template '" +  template + "': " + ex.ToString ());
					return false;
				}

			} else {
				doc.LoadXml (@"<Deployment xmlns=""http://schemas.microsoft.com/client/2007/deployment"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""></Deployment>");
			}

			try {
				XmlNode deploymentNode = doc.DocumentElement;
				if (deploymentNode == null || deploymentNode.Name != "Deployment" || deploymentNode.NamespaceURI != depNS) {
					Log.LogError ("Missing or invalid root <Deployment> element in manifest template '" +  template + "'.");
					return false;
				}
				if (deploymentNode.Attributes["EntryPointAssembly"] == null)
					deploymentNode.Attributes.Append (doc.CreateAttribute ("EntryPointAssembly")).Value =
						EntryPointAssembly.GetMetadata ("Filename");

				if (!String.IsNullOrEmpty (SilverlightAppEntry) && deploymentNode.Attributes["EntryPointType"] == null)
					deploymentNode.Attributes.Append (doc.CreateAttribute ("EntryPointType")).Value = SilverlightAppEntry;

				if (deploymentNode.Attributes["RuntimeVersion"] == null) {
					//FIXME:
					/*string fxVersion = MoonlightFrameworkBackend.GetFxVersion (proj.TargetFramework);

					if (proj.TargetRuntime is MonoDevelop.Core.Assemblies.MonoTargetRuntime) {
						var package = proj.TargetRuntime.RuntimeAssemblyContext.GetPackage ("moonlight-web-" + fxVersion);
						if (package != null && package.IsFrameworkPackage) {
							runtimeVersion = package.Version;
						} else {
							LoggingService.LogWarning ("Moonlight core framework package not found, cannot determine " +
								"runtime version string. Falling back to default value.");
						}
					}*/

					deploymentNode.Attributes.Append (doc.CreateAttribute ("RuntimeVersion")).Value =
							String.IsNullOrEmpty (RuntimeVersion) ? "2.0.31005.0" : RuntimeVersion;
				}

				XmlNamespaceManager mgr = new XmlNamespaceManager (doc.NameTable);
				mgr.AddNamespace ("dep", depNS);
				XmlNode partsNode = deploymentNode.SelectSingleNode ("dep:Deployment.Parts", mgr);
				if (partsNode == null)
					partsNode = deploymentNode.AppendChild (doc.CreateElement ("Deployment.Parts", depNS));

				AddAssemblyPart (doc, partsNode, EntryPointAssembly);

				foreach (ITaskItem ref_item in References)
					AddAssemblyPart (doc, partsNode, ref_item);
			} catch (XmlException ex) {
				Log.LogError (null, null, null, template, ex.LineNumber, ex.LinePosition, 0, 0,
						"Error processing manifest template: '" + ex.Source);
				return false;
			}

			doc.Save (manifest);

			return true;
		}

		static void AddAssemblyPart (XmlDocument doc, XmlNode partsNode, ITaskItem filename)
		{
			XmlNode child = doc.CreateElement ("AssemblyPart", "http://schemas.microsoft.com/client/2007/deployment");
			child.Attributes.Append (doc.CreateAttribute (
						"Name", "http://schemas.microsoft.com/winfx/2006/xaml")).Value = filename.GetMetadata ("Filename");
			string subdir = filename.GetMetadata ("DestinationSubdirectory");
			child.Attributes.Append (doc.CreateAttribute ("Source")).Value = Path.Combine (subdir ?? String.Empty, Path.GetFileName (filename.ItemSpec));
			partsNode.AppendChild (child);
		}

		[Required]
		[Output]
		public ITaskItem ManifestFile {
			get; set;
		}

		[Required]
		// with extension
		public ITaskItem EntryPointAssembly {
			get; set;
		}

		[Required]
		public ITaskItem[] References {
			get; set;
		}

		public ITaskItem SilverlightManifestTemplate {
			get; set;
		}

		public string SilverlightAppEntry {
			get; set;
		}

		public string RuntimeVersion {
			get; set;
		}
	}

}
