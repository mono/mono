// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;

namespace System.Data.SqlClient
{
	partial class SqlException
	{
		private const string DEF_MESSAGE = "SQL Exception has occured.";

		public override string Message {
			get {
				if (Errors.Count == 0)
					return base.Message;
				StringBuilder result = new StringBuilder ();
				if (base.Message != DEF_MESSAGE) {
					result.Append (base.Message);
					result.Append ("\n");
				}
				for (int i = 0; i < Errors.Count -1; i++) {
					result.Append (Errors [i].Message);
					result.Append ("\n");
				}
				result.Append (Errors [Errors.Count - 1].Message);
				return result.ToString ();
			}
		}
	}
}