// 
// System.Xml.Serialization.SchemaImporterExtension.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
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

#if NET_2_0

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Xml.Schema;

namespace System.Xml.Serialization.Advanced
{
	public abstract class SchemaImporterExtension
	{
		protected SchemaImporterExtension ()
		{
		}

		public virtual string ImportAnyElement (
			XmlSchemaAny any, 
			bool mixed, 
			XmlSchemas schemas, 
			XmlSchemaImporter importer, 
			CodeCompileUnit compileUnit, 
			CodeNamespace mainNamespace, 
			CodeGenerationOptions options, 
			CodeDomProvider codeProvider
		)
		{
			return null;
		}
		
		public virtual CodeExpression ImportDefaultValue (string value, string type)
		{
			return null;
		}
		
		public virtual string ImportSchemaType (
			XmlSchemaType type, 
			XmlSchemaObject context, 
			XmlSchemas schemas, 
			XmlSchemaImporter importer, 
			CodeCompileUnit compileUnit, 
			CodeNamespace mainNamespace, 
			CodeGenerationOptions options, 
			CodeDomProvider codeProvider
		)
		{
			return null;
		}
		
		public virtual string ImportSchemaType (
			string name, 
			string ns, 
			XmlSchemaObject context, 
			XmlSchemas schemas, 
			XmlSchemaImporter importer, 
			CodeCompileUnit compileUnit, 
			CodeNamespace mainNamespace, 
			CodeGenerationOptions options, 
			CodeDomProvider codeProvider
		)
		{
			return null;
		}
	}
}

#endif
