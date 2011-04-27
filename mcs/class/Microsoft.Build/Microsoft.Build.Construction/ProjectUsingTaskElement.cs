﻿//
// ProjectUsingTaskElement.cs
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

using System;

namespace Microsoft.Build.Construction
{
        public class ProjectUsingTaskElement : ProjectElementContainer
        {
                public string AssemblyFile { get; set; }
                public string AssemblyName { get; set; }
                public UsingTaskParameterGroupElement ParameterGroup {
                        get {
                                throw new NotImplementedException ();
                        }
                }
                public ProjectUsingTaskBodyElement TaskBody {
                        get {
                                throw new NotImplementedException ();
                        }
                }
                public string TaskFactory { get; set; }
                public string TaskName { get; set; }

                public UsingTaskParameterGroupElement AddParameterGroup ()
                {
                        throw new NotImplementedException ();
                }

                public ProjectUsingTaskBodyElement AddUsingTaskBody (string evaluate, string taskBody)
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
