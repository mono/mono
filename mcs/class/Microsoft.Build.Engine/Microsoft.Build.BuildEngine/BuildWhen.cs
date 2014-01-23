//
// BuildWhen.cs: Represents <When>.
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

using System;
using System.Collections;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal class BuildWhen {
		//Project			parentProject;
		GroupingCollection	groupingCollection;
		XmlElement		whenElement;
	
		public BuildWhen (XmlElement whenElement, Project parentProject)
		{
			//this.parentProject = parentProject;
			this.groupingCollection = new GroupingCollection (parentProject);
			if (whenElement == null)
				throw new ArgumentNullException ("whenElement");
			this.whenElement = whenElement;
			foreach (XmlElement xe in whenElement.ChildNodes) {
				switch (xe.Name) {
					case "ItemGroup":
						BuildItemGroup big = new BuildItemGroup (xe, parentProject, null, true);
						//big.BindToXml (xe);
						groupingCollection.Add (big);
						break;
					case "PropertyGroup":
						BuildPropertyGroup bpg = new BuildPropertyGroup (xe, parentProject, null, true);
						//bpg.BindToXml (xe);
						groupingCollection.Add (bpg);
						break;
					case "Choose":
						BuildChoose bc = new BuildChoose (xe, parentProject);
						groupingCollection.Add (bc);
						break;
					default:
						throw new InvalidProjectFileException ( string.Format ("Invalid element '{0}' in When.", xe.Name));
				}
			}
		}

		public void Evaluate()
		{
			groupingCollection.Evaluate ();
		}
		
		public string Condition {
			get { return whenElement.GetAttribute ("Condition"); }
			set { whenElement.SetAttribute ("Condition", value); }
		}
		
		public GroupingCollection GroupingCollection {
			get { return groupingCollection; }
			set { groupingCollection = value; }
		}
	}
}
