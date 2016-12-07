//
// BuildItemDefinition.cs
//
// Author:
//   Haakon Sporsheim (haakon.sporsheim@gmail.com)
//
// (C) 2010 Haakon Sporsheim
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.BuildEngine {
	internal class BuildItemDefinition {
		BuildItemDefinitionGroup	parentGroup;
		XmlElement					xmlElement;
		IDictionary					evaluatedMetadata;
		IDictionary					unevaluatedMetadata;

		BuildItemDefinition ()
		{
			this.xmlElement = null;
			this.parentGroup = null;
			unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		public BuildItemDefinition (XmlElement xmlElement, BuildItemDefinitionGroup parentGroup)
		{
			this.xmlElement = xmlElement;
			this.parentGroup = parentGroup;
			unevaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
			evaluatedMetadata = CollectionsUtil.CreateCaseInsensitiveHashtable ();
		}

		void SetMetadata (string name, string unevaluatedValue, string evaluatedValue)
		{
			evaluatedMetadata [name] = evaluatedValue;
			unevaluatedMetadata [name] = unevaluatedValue;
		}

		public void SetAndEvaluateMetadata (string name, string value, Project project)
		{
			Expression e = new Expression ();
			e.Parse (value, ParseOptions.AllowItemsNoMetadataAndSplit);
			string evaluatedValue = (string) e.ConvertTo (project,
					typeof (string), ExpressionOptions.ExpandItemRefs);

			this.SetMetadata (name, value, evaluatedValue);

			// Also set on the evaluated project item definition

			if (!project.EvaluatedItemDefinitions.ContainsKey (Name))
				project.EvaluatedItemDefinitions [Name] = new BuildItemDefinition ();

			project.EvaluatedItemDefinitions [Name].SetMetadata (name, value, evaluatedValue);
		}

		public string GetEvaluatedMetadata (string metadataName)
		{
			if (evaluatedMetadata.Contains (metadataName))
				return (string) evaluatedMetadata [metadataName];
			else
				return String.Empty;
		}

		public string GetMetadata (string metadataName)
		{
			if (unevaluatedMetadata.Contains (metadataName))
				return (string) unevaluatedMetadata [metadataName];
			else
				return String.Empty;
		}
		
		public bool HasMetadata (string metadataName)
		{
			return evaluatedMetadata.Contains (metadataName);
		}

		internal void Evaluate (Project project)
		{
			if (project == null)
				throw new ArgumentNullException ("project");

			foreach (XmlNode xn in xmlElement.ChildNodes) {
				XmlElement xe = xn as XmlElement;
				if (xe != null && ConditionParser.ParseAndEvaluate (xe.GetAttribute ("Condition"), project))
					SetAndEvaluateMetadata (xe.Name, xe.InnerText, project);
			}
		}
 
		public string Name {
			get {
				return xmlElement.Name;
			}
		}

		public string Condition {
			get {
				return xmlElement.GetAttribute ("Condition");
			}
		}
	}
}

#endif
