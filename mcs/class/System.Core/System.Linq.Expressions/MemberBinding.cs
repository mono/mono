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
//        Federico Di Gregorio  <fog@initd.org>
//

using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    public abstract class MemberBinding
    {
        #region .ctor
        protected MemberBinding(MemberBindingType type, MemberInfo member)
        {
            this.type = type;
            this.member = member;
        }
        #endregion

        #region Fields
        private MemberBindingType type;
        private MemberInfo member;
        #endregion

        #region Properties
        public MemberBindingType BindingType {
            get { return type; }
        }

        public MemberInfo Member {
            get { return member; }
        }
        #endregion

        #region Internal Methods
        internal abstract void BuildString(StringBuilder builder);
        #endregion

        #region ToString
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder ();
            BuildString (builder);
            return builder.ToString ();
        }
        #endregion
    }
}
