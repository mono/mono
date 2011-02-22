//
// BuildChoose.cs: Represents <Choose>.
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
using System.Collections.Generic;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal class BuildChoose {
		
		BuildWhen	otherwise;
		Project		project;
		ImportedProject	importedProject;
		XmlElement	xmlElement;
		List <BuildWhen>	whens;
		
		public BuildChoose (XmlElement xmlElement, Project project)
			: this (xmlElement, project, null)
		{
		}

		internal BuildChoose (XmlElement xmlElement, Project project, ImportedProject importedProject)
		{
			this.xmlElement = xmlElement;
			this.project = project;
			this.importedProject = importedProject;
			this.whens = new List <BuildWhen> ();

			foreach (XmlNode xn in xmlElement.ChildNodes) {
				if (!(xn is XmlElement))
					continue;

				XmlElement xe = (XmlElement)xn;

				if (xe.Name == "When") {
					if (otherwise != null)
						throw new InvalidProjectFileException ("The 'Otherwise' element must be last in a 'Choose' element.");
					if (xe.Attributes.GetNamedItem ("Condition") == null)
						throw new InvalidProjectFileException ("The 'When' element requires a 'Condition' attribute.");
					BuildWhen bw = new BuildWhen (xe, project);
					whens.Add (bw);
				} else if (xe.Name == "Otherwise") {
					if (this.whens.Count == 0)
						throw new InvalidProjectFileException ("At least one 'When' element must occur in a 'Choose' element.");
					
					otherwise = new BuildWhen (xe, project);
				}
			}

			DefinedInFileName = importedProject != null ? importedProject.FullFileName :
						project != null ? project.FullFileName : null;
		}
		
		public void Evaluate ()
		{
		}
		
		//public bool IsImported {
		//	get { return isImported; }
		//	set { isImported = value; }
		//}
		
		public BuildWhen Otherwise {
			get { return otherwise; }
			set { otherwise = value; }
		}
		
		public List <BuildWhen> Whens {
			get { return whens; }
			set { whens = value; }
		}

		internal string DefinedInFileName { get; private set; }
	}
}

#endif
