//
// TypedDataSetGenerator.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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


using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.IO;

using T = System.Data.TypedDataSetGenerator;

namespace System.Data.Design
{
	// It is likely replaced by System.Data.TypedDataSetGenerator-based
	// implementation.

	public sealed class TypedDataSetGenerator
	{
		private TypedDataSetGenerator ()
		{
		}

		[Flags]
		public enum GenerateOption
		{
			None = 0,
			HierarchicalUpdate = 1,
			LinqOverTypedDatasets = 2
		}

		[MonoTODO]
		public static ICollection<Assembly> ReferencedAssemblies {
			get { throw new NotImplementedException (); }
		}

		public static string Generate (DataSet dataSet, CodeNamespace codeNamespace, CodeDomProvider codeProvider)
		{
			T.Generate (dataSet, codeNamespace, codeProvider.CreateGenerator ());
			return null;
		}

		public static string Generate (string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider)
		{
			var dataSet = new DataSet ();
			dataSet.ReadXmlSchema (inputFileContent);
			T.Generate (dataSet, mainNamespace, codeProvider.CreateGenerator ());
			return null;
		}

		[MonoTODO]
		public static void Generate (string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, Hashtable customDBProviders)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Generate (string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, DbProviderFactory specifiedFactory)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string Generate (string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, GenerateOption option)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Generate (string inputFileContent, CodeCompileUnit compileUnit, CodeNamespace mainNamespace, CodeDomProvider codeProvider, Hashtable customDBProviders, GenerateOption option)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetProviderName (string inputFileContent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GetProviderName (string inputFileContent, string tableName)
		{
			throw new NotImplementedException ();
		}
	}
}

