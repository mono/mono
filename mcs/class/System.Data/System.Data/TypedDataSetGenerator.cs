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

		[MonoTODO]
		public static void Generate (DataSet dataSet,
			CodeNamespace codeNamespace,
			ICodeGenerator codeGen)
		{
			throw new NotImplementedException ();
		}

		public static string GenerateIdName (string name, ICodeGenerator codeGen)
		{
			if (name == null || codeGen == null)
				throw new NullReferenceException ();

			name = codeGen.CreateValidIdentifier (name);
			// ... however, in fact this method is so insufficient
			// that we have to modify the name.

			if (name.Length == 0)
				return "_";

			StringBuilder sb = null;
			if (!Char.IsLetter (name, 0) && name [0] != '_') {
				sb = new StringBuilder ();
				sb.Append ('_');
			}

			int start = 0;
			for (int i = 0; i < name.Length; i++) {
				if (!Char.IsLetterOrDigit (name, i)) {
					if (sb == null)
						sb = new StringBuilder ();
					sb.Append (name, start, i - start);
					sb.Append ('_');
					start = i + 1;
				}
			}

			if (sb != null) {
				sb.Append (name, start, name.Length - start);
				return sb.ToString ();
			}
			else
				return name;
		}
	}
}
