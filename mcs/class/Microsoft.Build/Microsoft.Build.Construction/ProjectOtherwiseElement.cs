﻿//
// ProjectOtherwiseElement.cs
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
using System.Linq;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Internal;

namespace Microsoft.Build.Construction
{
        [System.Diagnostics.DebuggerDisplayAttribute ("#Children={Count}")]
        public class ProjectOtherwiseElement : ProjectElementContainer
        {
                internal ProjectOtherwiseElement (ProjectRootElement containingProject)
                {
                        ContainingProject = containingProject;
                }
                public ICollection<ProjectChooseElement> ChooseElements {
                        get { return new CollectionFromEnumerable<ProjectChooseElement> (
                                new FilteredEnumerable<ProjectChooseElement> (Children)); }
                }
                public override string Condition {
                        get { return null; }
                        set {
                                throw new InvalidOperationException ("Can not set Condition.");
                        }
                }
                public ICollection<ProjectItemGroupElement> ItemGroups {
                        get { return new CollectionFromEnumerable<ProjectItemGroupElement> (
                                new FilteredEnumerable<ProjectItemGroupElement> (Children)); }
                }
                public ICollection<ProjectPropertyGroupElement> PropertyGroups {
                        get { return new CollectionFromEnumerable<ProjectPropertyGroupElement> (
                                new FilteredEnumerable<ProjectPropertyGroupElement> (Children)); }
                }
                internal override string XmlName {
                        get { return "Otherwise"; }
                }
                internal override ProjectElement LoadChildElement (string name)
                {
                        switch (name) {
                        case "PropertyGroup":
                                var property = ContainingProject.CreatePropertyGroupElement ();
                                AppendChild (property);
                                return property;
                        case "ItemGroup":
                                var item = ContainingProject.CreateItemGroupElement ();
                                AppendChild (item);
                                return item;
                        case "When":
                                var when = ContainingProject.CreateWhenElement (null);
                                AppendChild (when);
                                return when;
                        default:
                                throw new InvalidProjectFileException (string.Format (
                                        "Child \"{0}\" is not a known node type.", name));
                        }
                }
        }
}
