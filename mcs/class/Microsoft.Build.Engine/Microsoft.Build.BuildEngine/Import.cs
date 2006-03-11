//
// Import.cs: Represents a single Import element in an MSBuild project.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public class Import {
	
		XmlAttribute	condition;
		string		evaluatedProjectPath;
		XmlElement	importElement;
		bool		isImported;
		XmlAttribute	projectPath;
		
	
		internal Import (XmlElement importElement, bool isImported)
		{
			if (importElement == null)
				throw new ArgumentNullException ("importElement");
		
			this.importElement = importElement;
			this.isImported = isImported;
			this.condition = importElement.GetAttributeNode ("Condition");
			this.projectPath = importElement.GetAttributeNode ("ProjectPath");
			// FIXME: evaluate this
			this.evaluatedProjectPath = projectPath.Value;
		}
		
		public string Condition {
			get { return condition.Value; }
		}
		
		public string EvaluatedProjectPath {
			get { return evaluatedProjectPath; }
		}
		
		public bool IsImported {
			get { return isImported; }
		}
		
		public string ProjectPath {
			get { return evaluatedProjectPath; }
		}
	}
}

#endif