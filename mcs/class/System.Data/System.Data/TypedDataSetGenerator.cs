//
// TypedDataSetGenerator.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Data;

namespace System.Data
{
	public class TypedDataSetGenerator
	{
		public TypedDataSetGenerator ()
		{
		}

		[MonoTODO]
		public static void Generate (DataSet dataSet,
			CodeNamespace codeNamespace,
			ICodeGenerator codeGen)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string GenerateIdName (string name, ICodeGenerator codeGen)
		{
			throw new NotImplementedException ();
		}
	}
}
