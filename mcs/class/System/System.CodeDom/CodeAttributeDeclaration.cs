//
// System.CodeDom CodeAttributeDeclaration Class implementation
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//   Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Ximian, Inc.
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.CodeDom 
{
	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeAttributeDeclaration 
	{
		private string name;
		private CodeAttributeArgumentCollection arguments;
#if NET_2_0
		private CodeTypeReference attribute;
#endif

		//
		// Constructors
		//
		public CodeAttributeDeclaration ()
		{
		}

		public CodeAttributeDeclaration (string name)
		{
			this.Name = name;
		}

		public CodeAttributeDeclaration (string name, params CodeAttributeArgument [] arguments)
		{
			this.Name = name;
			Arguments.AddRange (arguments);
		}

#if NET_2_0
		public CodeAttributeDeclaration (CodeTypeReference attributeType)
		{
			attribute = attributeType;
			if (attributeType != null) {
				name = attributeType.BaseType;
			}
		}

		public CodeAttributeDeclaration (CodeTypeReference attributeType, params CodeAttributeArgument [] arguments)
		{
			attribute = attributeType;
			if (attributeType != null) {
				name = attributeType.BaseType;
			}
			Arguments.AddRange (arguments);
		}
#endif

		//
		// Properties
		//
		public CodeAttributeArgumentCollection Arguments {
			get {
				if (arguments == null) {
					arguments = new CodeAttributeArgumentCollection ();
				}

				return arguments;
			}
		}

		public string Name {
			get {
				if (name == null) {
					return string.Empty;
				}
				return name;
			}
			set {
				name = value;
#if NET_2_0
				attribute = new CodeTypeReference (name);
#endif
			}
		}

#if NET_2_0
		public CodeTypeReference AttributeType {
			get { return attribute; }
		}
#endif
	}
}
