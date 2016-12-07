//
// BuildItemDefinitionGroup.cs
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
	internal class BuildItemDefinitionGroup : IEnumerable {
		Project						project;
		XmlElement					xmlElement;
		List<BuildItemDefinition>	definitions;
		bool						evaluated;

		BuildItemDefinitionGroup ()
		{
		}

		public BuildItemDefinitionGroup (XmlElement xmlElement, Project project)
		{
			if (xmlElement == null)
				throw new ArgumentNullException ("xmlElement");

			if (project == null)
				throw new ArgumentNullException ("project");

			this.xmlElement = xmlElement;
			this.project = project;
			this.evaluated = false;
			this.definitions  = new List <BuildItemDefinition> ();

			foreach (XmlNode xn in xmlElement.ChildNodes) {
				if (xn is XmlElement)
					definitions.Add (new BuildItemDefinition ((XmlElement) xn, this));
			}

			DefinedInFileName = project != null ? project.FullFileName : null;
		}

		public IEnumerator GetEnumerator ()
		{
			return definitions.GetEnumerator ();
		}

		internal void Evaluate ()
		{
			if (evaluated)
				return;

			foreach (BuildItemDefinition bid in definitions) {
				project.CurrentItemDefinitionBeingEvaluated = bid;
				if (bid.Condition != String.Empty) {
					ConditionExpression ce = ConditionParser.ParseCondition (bid.Condition);
					if (!ce.BoolEvaluate (project))
						continue;
				}

				bid.Evaluate (project);
			}

			project.CurrentItemDefinitionBeingEvaluated = null;
			evaluated = true;
		}

		public string Condition {
			get {
				return xmlElement.GetAttribute ("Condition");
			}
		}

		internal string DefinedInFileName { get; private set; }

	}
}
