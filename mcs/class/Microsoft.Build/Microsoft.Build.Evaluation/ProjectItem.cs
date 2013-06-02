//
// ProjectItem.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// (C) 2011 Leszek Ciesielski
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
using System.Diagnostics;

using Microsoft.Build.Construction;

namespace Microsoft.Build.Evaluation
{
        [DebuggerDisplay("{ItemType}={EvaluatedInclude} [{UnevaluatedInclude}] #DirectMetadata={DirectMetadataCount}")]
        public class ProjectItem
        {
                internal ProjectItem (ProjectItemElement xml)
                {
                        Xml = xml;
                }

                public ProjectMetadata GetMetadata (string name)
                {
                        throw new NotImplementedException ();
                }

                public string GetMetadataValue (string name)
                {
                        throw new NotImplementedException ();
                }

                public bool HasMetadata (string name)
                {
                        throw new NotImplementedException ();
                }

                public bool RemoveMetadata (string name)
                {
                        throw new NotImplementedException ();
                }

                public void Rename (string name)
                {
                        throw new NotImplementedException ();
                }

                public void SetMetadataValue (string name, string unevaluatedValue)
                {
                        throw new NotImplementedException ();
                }

                public IEnumerable<ProjectMetadata> DirectMetadata {
                        get { throw new NotImplementedException (); }
                }

                public int DirectMetadataCount {
                        get { throw new NotImplementedException (); }
                }

                public string EvaluatedInclude {
                        get { throw new NotImplementedException (); }
                }

                public bool IsImported {
                        get { throw new NotImplementedException (); }
                }

                public string ItemType {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public ICollection<ProjectMetadata> Metadata {
                        get { throw new NotImplementedException (); }
                }

                public int MetadataCount {
                        get { throw new NotImplementedException (); }
                }

                public Project Project {
                        get { throw new NotImplementedException (); }
                }

                public string UnevaluatedInclude {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public ProjectItemElement Xml { get; private set; }

        }
}
