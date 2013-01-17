//
// ProjectItemInstance.cs
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

using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;

namespace Microsoft.Build.Execution
{
        public class ProjectItemInstance
                : ITaskItem2
        {
                private ProjectItemInstance ()
                {
                        throw new NotImplementedException ();
                }

                public ProjectMetadataInstance GetMetadata (string name)
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

                public void RemoveMetadata (string metadataName)
                {
                        throw new NotImplementedException ();
                }

                public void SetMetadata (IEnumerable<KeyValuePair<string, string>> metadataDictionary)
                {
                        throw new NotImplementedException ();
                }

                public ProjectMetadataInstance SetMetadata (string name, string evaluatedValue)
                {
                        throw new NotImplementedException ();
                }

                public int DirectMetadataCount {
                        get { throw new NotImplementedException (); }
                }

                public string EvaluatedInclude {
                        get { throw new NotImplementedException (); }
                        set { throw new NotImplementedException (); }
                }

                public string ItemType {
                        get { throw new NotImplementedException (); }
                }

                public IEnumerable<ProjectMetadataInstance> Metadata {
                        get { throw new NotImplementedException (); }
                }

                public int MetadataCount {
                        get { throw new NotImplementedException (); }
                }

                public ICollection<string> MetadataNames {
                        get { throw new NotImplementedException (); }
                }

                public ProjectInstance Project {
                        get { throw new NotImplementedException (); }
                }

                #region ITaskItem2 implementation
                string ITaskItem2.GetMetadataValueEscaped (string metadataName)
                {
                        throw new NotImplementedException ();
                }

                void ITaskItem2.SetMetadataValueLiteral (string metadataName, string metadataValue)
                {
                        throw new NotImplementedException ();
                }

                System.Collections.IDictionary ITaskItem2.CloneCustomMetadataEscaped ()
                {
                        throw new NotImplementedException ();
                }

                string ITaskItem2.EvaluatedIncludeEscaped {
                        get {
                                throw new NotImplementedException ();
                        }
                        set {
                                throw new NotImplementedException ();
                        }
                }
                #endregion

                #region ITaskItem implementation
                System.Collections.IDictionary ITaskItem.CloneCustomMetadata ()
                {
                        throw new NotImplementedException ();
                }

                void ITaskItem.CopyMetadataTo (ITaskItem destinationItem)
                {
                        throw new NotImplementedException ();
                }

                string ITaskItem.GetMetadata (string metadataName)
                {
                        throw new NotImplementedException ();
                }

                void ITaskItem.RemoveMetadata (string metadataName)
                {
                        throw new NotImplementedException ();
                }

                void ITaskItem.SetMetadata (string metadataName, string metadataValue)
                {
                        throw new NotImplementedException ();
                }

                string ITaskItem.ItemSpec {
                        get {
                                throw new NotImplementedException ();
                        }
                        set {
                                throw new NotImplementedException ();
                        }
                }

                int ITaskItem.MetadataCount {
                        get {
                                throw new NotImplementedException ();
                        }
                }

                System.Collections.ICollection ITaskItem.MetadataNames {
                        get {
                                throw new NotImplementedException ();
                        }
                }
                #endregion
        }
}

