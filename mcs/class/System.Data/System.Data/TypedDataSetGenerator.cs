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
using System.Text;

namespace System.Data
{
	public class TypedDataSetGenerator
	{
		public TypedDataSetGenerator ()
		{
		}

		[MonoTODO ("See CustomDataclassGenerator.cs")]
		public static void Generate (DataSet dataSet,
			CodeNamespace codeNamespace,
			ICodeGenerator codeGen)
		{
			CustomDataClassGenerator.CreateDataSetClasses (
				dataSet, codeNamespace, codeGen, null);
		}

		public static string GenerateIdName (string name, ICodeGenerator codeGen)
		{
			return CustomDataClassGenerator.MakeSafeName (name, codeGen);
		}
	}
}
