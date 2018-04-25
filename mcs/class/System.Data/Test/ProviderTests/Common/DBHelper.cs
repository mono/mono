// DBHelper.cs : Helper class for executing queries with database.
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
// 
// Copyright (c) 2004 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Data;
using System.Text;

namespace MonoTests.System.Data.Connected
{
	public sealed class DBHelper
	{
		public static Random random = new Random ( (int) DateTime.Now.Ticks);
		
		public static int ExecuteNonQuery (IDbConnection connection ,string query)
		{
			IDbCommand command = connection.CreateCommand ();
			command.CommandType = CommandType.Text;
			command.CommandText = query;
			command.CommandTimeout = 120;
			int result = -1;
			try {
				result = command.ExecuteNonQuery ();
			} catch {
				return -2;
			}
			return result;
		}

		public static int ExecuteSimpleSP (IDbConnection connection ,string proc)
		{
			IDbCommand command = connection.CreateCommand ();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = proc;
			int result = -1;
			try {
				result = command.ExecuteNonQuery ();
			} catch {
				return -2;
			}
			return result;
		}

		public static string GetRandomName (string prefix, int length)
		{
			StringBuilder s = new StringBuilder (prefix.Length + 1 + length);
			s.Append (prefix);
			s.Append ("_");
			for (int i = 0; i < length; i++) {
				s.Append (random.Next (25) + 'A');
			}
			return s.ToString ();
		}
	}
}
