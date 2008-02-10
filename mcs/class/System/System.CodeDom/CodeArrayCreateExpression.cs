//
// System.CodeDom CodeArrayCreateExpression Class implementation
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
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

namespace System.CodeDom {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	public class CodeArrayCreateExpression
		: CodeExpression 
	{
		private CodeTypeReference createType;
		private CodeExpressionCollection initializers;
		private CodeExpression sizeExpression;
		private int size;
		
		//
		// Constructors
		//
		public CodeArrayCreateExpression ()
		{
		}

		public CodeArrayCreateExpression (CodeTypeReference createType, 
						  CodeExpression size )
		{
			this.createType = createType;
			this.sizeExpression = size;
		}

		public CodeArrayCreateExpression (CodeTypeReference createType, 
						  params CodeExpression[] initializers )
		{
			this.createType = createType;
			this.Initializers.AddRange( initializers );
		}

		public CodeArrayCreateExpression (CodeTypeReference createType, 
						  int size)
		{
			this.createType = createType;
			this.size = size;
		}


		public CodeArrayCreateExpression (string createType, 
						  CodeExpression size)
		{
			this.createType = new CodeTypeReference( createType );
			this.sizeExpression = size;
		}

		public CodeArrayCreateExpression (string createType, 
						  params CodeExpression[] initializers)
		{
			this.createType = new CodeTypeReference( createType );
			this.Initializers.AddRange( initializers );
		}

		public CodeArrayCreateExpression (string createType, 
						  int size)
		{
			this.createType = new CodeTypeReference( createType );
			this.size = size;
		}


		public CodeArrayCreateExpression (Type createType, 
						  CodeExpression size)
		{
			this.createType = new CodeTypeReference( createType );
			this.sizeExpression = size;
		}
			
		public CodeArrayCreateExpression (Type createType, 
						  params CodeExpression[] initializers)
		{
			this.createType = new CodeTypeReference( createType );
			this.Initializers.AddRange( initializers );
		}

		public CodeArrayCreateExpression (Type createType, 
						  int size)
		{
			this.createType = new CodeTypeReference( createType );
			this.size = size;
		}

		//
		// Properties
		//
		public CodeTypeReference CreateType {
			get {
				if (createType == null) {
					createType = new CodeTypeReference (typeof (void));
				}
				return createType;
			}
			set {
				createType = value;
			}
		}

		public CodeExpressionCollection Initializers {
			get {
				if ( initializers == null )
					initializers = new CodeExpressionCollection();
					
				return initializers;
			}
		}

		public CodeExpression SizeExpression {
			get {
				return sizeExpression;
			}
			set {
				sizeExpression = value;
			}
		}

		public int Size {
			get {
				return size;
			}
			set {
				size = value;
				// NOTE: Setting Size in ms.Net does
				// not supersede SizeExpression
				// values. Instead, the CodeGenerator
				// seems to always prefer
				// SizeExpression if set to a value !=
				// null.
			}
		}

		//
		// ICodeDomVisitor method
		//
		internal override void Accept (ICodeDomVisitor visitor)
		{
			visitor.Visit (this);
		}
	}
}

