//
// ProjectTargetElement.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//
// (C) 2011 Leszek Ciesielski
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

using System.Collections.Generic;
using System;

namespace Microsoft.Build.Construction
{
        public class ProjectTargetElement : ProjectElementContainer
        {
                public string AfterTargets { get; set; }
                public string BeforeTargets { get; set; }
                public string DependsOnTargets { get; set; }
                public string Inputs { get; set; }
                public ICollection<ProjectItemGroupElement> ItemGroups {
                        get {
                                throw new NotImplementedException ();
                        }
                }
                public string KeepDuplicateOutputs { get; set; }
                public string Name { get; set; }
                public ICollection<ProjectOnErrorElement> OnErrors {
                        get {
                                throw new NotImplementedException ();
                        }
                }
                public string Outputs { get; set; }
                public ICollection<ProjectPropertyGroupElement> PropertyGroups {
                        get {
                                throw new NotImplementedException ();
                        }
                }
                public string Returns { get; set; }
                public ICollection<ProjectTaskElement> Tasks {
                        get {
                                throw new NotImplementedException ();
                        }
                }
                public ProjectItemGroupElement AddItemGroup ()
                {
                        throw new NotImplementedException ();
                }
                public ProjectPropertyGroupElement AddPropertyGroup ()
                {
                        throw new NotImplementedException ();
                }
                public ProjectTaskElement AddTask (string taskName)
                {
                        throw new NotImplementedException ();
                }
                internal override string XmlName {
                        get {
                                throw new NotImplementedException ();
                        }
                }
        }
}
