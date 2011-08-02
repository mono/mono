//
// ConditionFunctionExpression.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2006 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal sealed class ConditionFunctionExpression : ConditionExpression {
	
		List <ConditionFactorExpression> 	args;
		string					name;
		
		static Dictionary <string, MethodInfo>	functions;
		
		static ConditionFunctionExpression ()
		{
			Type t = typeof (ConditionFunctionExpression);
			string [] names = new string [] { "Exists", "HasTrailingSlash" };
		
			functions = new Dictionary <string, MethodInfo> (StringComparer.InvariantCultureIgnoreCase);
			foreach (string name in names)
				functions.Add (name, t.GetMethod (name, BindingFlags.NonPublic | BindingFlags.Static));
		}
	
		public ConditionFunctionExpression (string name, List <ConditionFactorExpression> args)
		{
			this.args = args;
			this.name = name;
		}
		
		public override  bool BoolEvaluate (Project context)
		{
			if (!functions.ContainsKey (name))
				throw new InvalidOperationException ("Unknown function named: " + name);
			
			if (functions [name] == null)
				throw new InvalidOperationException ("Unknown function named: " + name);
				
			MethodInfo mi = functions [name];
			object [] argsArr = new object [args.Count + 1];
			int i = 0;
			foreach (ConditionFactorExpression cfe in args)
				argsArr [i++] = cfe.StringEvaluate (context);
			argsArr [i] = context;
				
			return (bool) mi.Invoke (null, argsArr);
		}
		
		public override float NumberEvaluate (Project context)
		{
			throw new NotSupportedException ();
		}
		
		public override string StringEvaluate (Project context)
		{
			throw new NotSupportedException ();
		}
		
		public override bool CanEvaluateToBool (Project context)
		{
			return functions.ContainsKey (name);
		}
		
		public override bool CanEvaluateToNumber (Project context)
		{
			return false;
		}
		
		public override bool CanEvaluateToString (Project context)
		{
			return false;
		}

#pragma warning disable 0169
#region Functions
		// FIXME imported projects
		static bool Exists (string file, Project context)
		{
			string directory  = null;
			
			if (context.FullFileName != String.Empty)
				directory = Path.GetDirectoryName (context.FullFileName);
				
			if (!Path.IsPathRooted (file) && directory != null && directory != String.Empty)
				file = Path.Combine (directory, file);
		
			return File.Exists (file) || Directory.Exists (file);
		}

		static bool HasTrailingSlash (string file, Project context)
		{
			if (file == null)
				return false;

			file = file.Trim ();

			int len = file.Length;
			if (len == 0)
				return false;

			return file [len - 1] == '\\' || file [len - 1] == '/';
		}

#endregion
#pragma warning restore 0169

	}
}

#endif
