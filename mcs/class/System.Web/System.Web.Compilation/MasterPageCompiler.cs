//
// System.Web.Compilation.MasterPageCompiler
//
// Authors:
//	Joel W. Reed (joelwreed@gmail.com)
//
//

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
using System.CodeDom;
using System.Web.UI;

namespace System.Web.Compilation
{
	class MasterPageCompiler : UserControlCompiler
	{
		MasterPageParser parser;

		public MasterPageCompiler (MasterPageParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		protected internal override void CreateMethods ()
		{
			base.CreateMethods ();

			Type type = parser.MasterType;
			if (type != null) {
				CodeMemberProperty mprop = new CodeMemberProperty ();
				mprop.Name = "Master";
				mprop.Type = new CodeTypeReference (parser.MasterType);
				mprop.Attributes = MemberAttributes.Public | MemberAttributes.New;
				CodeExpression prop = new CodePropertyReferenceExpression (new CodeBaseReferenceExpression (), "Master");
				prop = new CodeCastExpression (parser.MasterType, prop);
				mprop.GetStatements.Add (new CodeMethodReturnStatement (prop));
				mainClass.Members.Add (mprop);
				AddReferencedAssembly (type.Assembly);
			}
		}
	}
}



