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
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//

using System.Collections.Generic;

namespace System.Data.Linq
{
    class XmlTypeMapping
    {
        #region .ctor
        public XmlTypeMapping(string name)
        {
            this.name = name;
        }
        #endregion

        #region Fields
        private string name;
        private string inheritanceCode;
        private bool inheritanceDefault;
        private List<XmlTypeMapping> derived = new List<XmlTypeMapping>();
        private List<XmlMemberMapping> members = new List<XmlMemberMapping>();
        #endregion

        #region Properties
        public string TypeName
        {
            get { return name; }
        }

        public string InheritanceCode
        {
            get { return inheritanceCode; }
            set { inheritanceCode = value; }
        }

        public bool IsInheritanceDefault
        {
            get { return inheritanceDefault; }
            set { inheritanceDefault = value; }
        }

        public List<XmlTypeMapping> Derived
        {
            get { return derived; }
        }

        public List<XmlMemberMapping> Members
        {
            get { return members; }
        }
        #endregion
    }
}