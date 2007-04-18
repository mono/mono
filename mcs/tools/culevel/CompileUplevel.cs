//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.CSharp;

namespace Mono.Tools
{
	class Position
	{
		int start = -1;
		int end = -1;
    
		public int Start {
			get { return start; }
		}

		public int End {
			get { return end; }
		}

		public int RequiredUALength {
			get {
				if (end == -1)
					return start;
				return end;
			}
		}
    
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ("Position {");
			sb.AppendFormat ("{0}", start);
			if (end != -1)
				sb.AppendFormat (",{0}}}", end);
			else
				sb.Append ("}");

			return sb.ToString ();
		}
    
		public Position (string positions)
		{
			if (positions == null || positions.Length == 0)
				throw new ArgumentException ("'positions' must not be null or empty");
      
			string[] pa = positions.Split ('-');
      
			if (pa.Length > 2)
				throw new ApplicationException ("Syntax error in the positions attribute - only one dash can be present");

			try {
				start = Int32.Parse (pa [0]);
			} catch (Exception) {
				throw new ApplicationException ("The 'positions' attribute has invalid syntax");
			}

			if (start < 0)
				throw new ApplicationException ("Start must be 0 or more.");
      
			if (pa.Length == 2) {
				try {
					end = Int32.Parse (pa [1]);
				} catch (Exception) {
					throw new ApplicationException ("The 'positions' attribute has invalid syntax");
				}

				if (end < start)
					throw new ApplicationException ("End of range must not be smaller than its start");

				if (end == start)
					end = -1;
			}
		}

		public CodeBinaryOperatorExpression GetExpression (string match)
		{
			int poslen;

			if (end == -1)
				poslen = 0;
			else
				poslen = end - start;
			poslen++;
      
			if (match.Length != poslen)
				throw new ApplicationException (
					String.Format ("Match string '{0}' has incorrect length (expected {1})", match, poslen));
      
			CodeBinaryOperatorExpression expr = new CodeBinaryOperatorExpression ();

			if (poslen == 1) {
				expr.Left = new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("ua"),
									    new CodePrimitiveExpression (Start));
				expr.Operator = CodeBinaryOperatorType.ValueEquality;
				expr.Right = new CodePrimitiveExpression (match [0]);
			} else {
				CodeBinaryOperatorExpression cur = expr, prev = expr, tmp;
				int i, pos;
	
				for (i = 0, pos = Start; i < poslen; i++, pos++) {
					tmp = new CodeBinaryOperatorExpression ();
					tmp.Left = new CodeArrayIndexerExpression (new CodeVariableReferenceExpression ("ua"),
										   new CodePrimitiveExpression (pos));
					tmp.Operator = CodeBinaryOperatorType.ValueEquality;
					tmp.Right = new CodePrimitiveExpression (match [i]);

					if (i + 1 < poslen) {
						cur.Left = tmp;
						cur.Operator = CodeBinaryOperatorType.BooleanAnd;
						prev.Right = cur;
						prev = cur;
						cur = new CodeBinaryOperatorExpression ();
					} else
						prev.Right = tmp;
				}
			}
      
			return expr;
		}
	}
  
	class GroupDefinition
	{
		readonly CodeMethodReturnStatement returnTrue = new CodeMethodReturnStatement (new CodePrimitiveExpression (true));
		readonly CodeMethodReturnStatement returnFalse = new CodeMethodReturnStatement (new CodePrimitiveExpression (false));
    
		ArrayList positions = null;
		ArrayList matches = null;
		ArrayList childGroups;
		ArrayList exceptions;
    
		bool defaultJS = true;
		int scanfrom = -1;
		int skip = -1;
		int level = 0;
		int groupId = 0;
    
		public Position[] Positions {
			get {
				if (positions == null)
					return null;
				return (Position[]) positions.ToArray (typeof (Position));
			}
		}

		public string[] Matches {
			get {
				if (matches == null)
					return null;
				return (string[]) matches.ToArray (typeof (string));
			}
		}

		public ArrayList ChildGroups {
			get { return childGroups; }
		}
    
		public bool DefaultJS {
			get { return defaultJS; }
		}

		public int ScanFrom {
			get { return scanfrom; }
		}

		public int Skip {
			get { return skip; }
		}

		public bool Positional {
			get { return positions != null; }
		}

		public bool GroupZero {
			get { return positions == null && matches == null && scanfrom == -1 && skip == -1; }
		}

		public int Level {
			get { return level; }
			set { level = value; }
		}

		public int GroupId {
			get { return groupId; }
			set { groupId = value; }
		}
    
		public override string ToString ()
		{
			if (GroupZero)
				return "GroupZero";
      
			StringBuilder sb = new StringBuilder ("Group: ");
			if (Positional) {
				sb.Append ("positions =");
				foreach (Position p in positions)
					sb.AppendFormat (" [{0}]", p.ToString ());
			} else {
				sb.AppendFormat ("scanfrom {0}, skip {1}", scanfrom, skip);
			}

			sb.Append ("; matches =");
			foreach (string m in matches)
				sb.AppendFormat (" [{0}]", m);
      
			return sb.ToString ();
		}

		public GroupDefinition ()
		{
			childGroups = new ArrayList ();
		}
      
		public GroupDefinition (XmlReader reader)
		{
			childGroups = new ArrayList ();

			string positions = reader.GetAttribute ("positions");
			string scanfrom = reader.GetAttribute ("scanfrom");
      
			if (positions != null && scanfrom != null)
				throw new ApplicationException ("The 'positions' and 'scanfrom' attributes are mutually exclusive");
			if ((positions == null || positions.Length == 0) && (scanfrom == null || scanfrom.Length == 0))
				throw new ApplicationException ("Exactly one of the 'positions' or 'scanfrom' attributes must be present and have a value");
      
			if (positions != null)
				InitPositions (reader, positions);
			else
				InitScanfrom (reader, scanfrom);

			string javascript = reader.GetAttribute ("javascript");
			if (javascript != null && javascript.Length > 0) {
				try {
					defaultJS = Boolean.Parse (javascript);
				} catch (Exception) {
					throw new ApplicationException ("Invalid value of the 'javascript' attribute. Must be a valid boolean value (true or false)");
				}
			}

			string match = reader.GetAttribute ("match");
			if (match == null || match.Length == 0)
				throw new ApplicationException ("Missing the 'match' attribute");

			InitMatches (match);
		}

		public void AddExcept (XmlReader reader)
		{
			if (exceptions == null)
				exceptions = new ArrayList ();

			exceptions.Add (new GroupDefinition (reader));
		}
    
		void InitMatches (string match)
		{
			if (positions != null) {
				string[] ma = match.Split (',');
      
				if (ma.Length != positions.Count)
					throw new ApplicationException ("Number of matches provided in the 'match' attribute is different that the number of positions.");

				matches = new ArrayList (ma.Length);
				foreach (string m in ma)
					matches.Add (m);
			} else {
				matches = new ArrayList (1);
				matches.Add (match);
			}
		}
    
		void InitPositions (XmlReader reader, string positions)
		{
			string[] pa = positions.Split (',');
			this.positions = new ArrayList (pa.Length);
			foreach (string p in pa)
				this.positions.Add (new Position (p.Trim ()));
		}

		void InitScanfrom (XmlReader reader, string scanfrom)
		{
			string skip = reader.GetAttribute ("skip");
      
			if (skip == null || skip.Length == 0)
				this.skip = 0;
			else {
				try {
					this.skip = Int32.Parse (skip);
				} catch (Exception) {
					throw new ApplicationException ("Invalid value of the 'skip' attribute. Must be an integer.");
				}
			}

			try {
				this.scanfrom = Int32.Parse (scanfrom);
			} catch (Exception) {
				throw new ApplicationException ("Invalid value of the 'scanfrom' attribute. Must be an integer.");
			}
		}

		public CodeCompileUnit GenerateCode ()
		{
			if (!GroupZero)
				throw new ApplicationException ("Code can be generated only by GroupZero");
      
			CodeCompileUnit unit = new CodeCompileUnit ();
			CodeNamespace ns = new CodeNamespace ("System.Web");
			unit.Namespaces.Add (ns);
      
			ns.Imports.Add (new CodeNamespaceImport ("System"));

			CodeTypeDeclaration mainClass = new CodeTypeDeclaration ("UplevelHelper");
			mainClass.TypeAttributes = TypeAttributes.Class |
				TypeAttributes.Sealed |
				TypeAttributes.NotPublic |
				TypeAttributes.NestedAssembly;
			ns.Types.Add (mainClass);

			GenerateMethod (mainClass);
			return unit;
		}

		CodeMemberMethod GetMainMethod ()
		{
			CodeMemberMethod mainMethod = new CodeMemberMethod ();
			mainMethod.Name = "IsUplevel";
			mainMethod.ReturnType = new CodeTypeReference (typeof (bool));
			mainMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "ua"));
			mainMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static | MemberAttributes.Final;

			// if (ua == null)
			//    return false;
			CodeBinaryOperatorExpression uaNull = new CodeBinaryOperatorExpression ();
			uaNull.Left = new CodeArgumentReferenceExpression ("ua");
			uaNull.Operator = CodeBinaryOperatorType.ValueEquality;
			uaNull.Right = new CodePrimitiveExpression (null);
			mainMethod.Statements.Add (new CodeConditionStatement (uaNull, returnFalse));
      
			// int ualength = ua.Length;
			mainMethod.Statements.Add (
				new CodeVariableDeclarationStatement (
					typeof (int),
					"ualength",
					new CodePropertyReferenceExpression (new CodeArgumentReferenceExpression ("ua"), "Length"))
			);

			// if (ualength == 0)
			//    return false;
			CodeBinaryOperatorExpression uaEmpty = new CodeBinaryOperatorExpression ();
			uaEmpty.Left = new CodeVariableReferenceExpression ("ualength");
			uaEmpty.Operator = CodeBinaryOperatorType.ValueEquality;
			uaEmpty.Right = new CodePrimitiveExpression (0);
			mainMethod.Statements.Add (new CodeConditionStatement (uaEmpty, returnFalse));

			// bool hasJavaScript = false;
			mainMethod.Statements.Add (
				new CodeVariableDeclarationStatement (typeof (bool), "hasJavaScript",
								      new CodePrimitiveExpression (false)));
      
			return mainMethod;
		}

		CodeMemberMethod GetGroupMethod ()
		{
			CodeMemberMethod groupMethod = new CodeMemberMethod ();
			groupMethod.Name = String.Format ("DetermineUplevel_{0}_{1}", level, groupId);
			groupMethod.ReturnType = new CodeTypeReference (typeof (bool));
			groupMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "ua"));
			CodeParameterDeclarationExpression hasJavaScript =
				new CodeParameterDeclarationExpression (typeof (bool), "hasJavaScript");
			
			hasJavaScript.Direction = FieldDirection.Out;
			groupMethod.Parameters.Add (hasJavaScript);
			groupMethod.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "ualength"));
			groupMethod.Attributes = MemberAttributes.Private | MemberAttributes.Static | MemberAttributes.Final;

			// hasJavaScript = <valueOf_DefaultJS>;
			CodeAssignStatement assign = new CodeAssignStatement (new CodeVariableReferenceExpression ("hasJavaScript"),
									      new CodePrimitiveExpression (DefaultJS));
			groupMethod.Statements.Add (assign);
      
			return groupMethod;
		}

		ArrayList GenerateExceptions (CodeTypeDeclaration mainClass, bool assignHasJavaScript)
		{
			if (exceptions == null || exceptions.Count == 0)
				return null;

			ArrayList matches = new ArrayList (exceptions.Count);
			CodeConditionStatement match;
      
			foreach (GroupDefinition gd in exceptions) {
				match = gd.GenerateConditionStatement (mainClass);
				matches.Add (match);

				if (assignHasJavaScript && gd.Positional)
					match.TrueStatements.Add (new CodeAssignStatement (
									  new CodeVariableReferenceExpression ("hasJavaScript"),
									  new CodePrimitiveExpression (false)));
				if (!assignHasJavaScript || GroupZero)
					match.TrueStatements.Add (returnFalse);
				else
					match.TrueStatements.Add (returnTrue);
			}

			return matches;
		}
    
		CodeMemberMethod GenerateMethod (CodeTypeDeclaration mainClass)
		{
			CodeMemberMethod method;

			if (GroupZero)
				method = GetMainMethod ();
			else
				method = GetGroupMethod ();

			mainClass.Members.Add (method);
			CodeConditionStatement matches, subMatches;
			ArrayList reverseMatches;
			CodeMemberMethod childMethod;
			CodeExpression hasJSRef = GroupZero ?
				(CodeExpression) new CodeVariableReferenceExpression ("hasJavaScript") :
				(CodeExpression) new CodeArgumentReferenceExpression ("hasJavaScript");

			reverseMatches = GenerateExceptions (mainClass, !GroupZero);
			if (reverseMatches != null && reverseMatches.Count > 0)
				foreach (CodeConditionStatement ccs in reverseMatches)
					method.Statements.Add (ccs);
      
			if (childGroups.Count > 0) {
				CodeDirectionExpression hasJavaScript = new CodeDirectionExpression (FieldDirection.Out, hasJSRef);
				CodeExpression ualengthRef = GroupZero ?
					(CodeExpression) new CodeVariableReferenceExpression ("ualength") :
					(CodeExpression) new CodeArgumentReferenceExpression ("ualength");

				int groupId = 0;
	
				CodeMethodReturnStatement returnHasJS = new CodeMethodReturnStatement (
					new CodeVariableReferenceExpression ("hasJavaScript"));
				
				foreach (GroupDefinition gd in childGroups) {
					matches = gd.GenerateConditionStatement (mainClass);

					if (gd.ChildGroups.Count > 0) {
						childMethod = gd.GenerateMethod (mainClass);

						subMatches = new CodeConditionStatement ();
						subMatches.Condition = new CodeMethodInvokeExpression (
							new CodeMethodReferenceExpression (
								new CodeTypeReferenceExpression ("UplevelHelper"), childMethod.Name),
							new CodeExpression[] {new CodeArgumentReferenceExpression ("ua"),
									      hasJavaScript, ualengthRef}
						);
						subMatches.TrueStatements.Add (returnHasJS);
						subMatches.FalseStatements.Add (new CodeMethodReturnStatement (
											new CodePrimitiveExpression (false))
						);
						
						matches.TrueStatements.Add (subMatches);
					} else {
						reverseMatches = gd.GenerateExceptions (mainClass, !GroupZero);
						if (reverseMatches != null && reverseMatches.Count > 0)
							foreach (CodeConditionStatement ccs in reverseMatches)
								matches.TrueStatements.Add (ccs);
	    
						if (!GroupZero && gd.Positional)
							matches.TrueStatements.Add (
								new CodeAssignStatement (
									new CodeVariableReferenceExpression ("hasJavaScript"),
									new CodePrimitiveExpression (true))
							);
						matches.TrueStatements.Add (returnTrue);
					}
					method.Statements.Add (matches);
					groupId++;
				}

				// return false;
				method.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (false)));
			} else 
				// return <valueOf_DefaultJS>
				method.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (DefaultJS)));
      
			return method;
		}

		CodeConditionStatement GenerateConditionStatement (CodeTypeDeclaration mainClass)
		{
			CodeConditionStatement ret = new CodeConditionStatement ();

			if (Positional)
				ret.Condition = GeneratePositionalExpression ();
			else
				ret.Condition = GenerateScanfromExpression (mainClass);

			return ret;
		}

		CodeExpression GeneratePositionalExpression ()
		{
			Position[] positions = Positions;
			string[] matches = Matches;
			ArrayList components = new ArrayList ();

			int i, reqLength = 0;
			Position p;
      
			for (i = 0; i < positions.Length; i++) {
				p = positions [i];
				components.Add (p.GetExpression (matches [i]));
				if (p.RequiredUALength > reqLength)
					reqLength = p.RequiredUALength;
			}
      
			CodeBinaryOperatorExpression expr = null;

			int complen = components.Count;
			i = 0;

			if (complen == 1)
				expr = components [0] as CodeBinaryOperatorExpression;
			else {
				expr = new CodeBinaryOperatorExpression ();
				CodeBinaryOperatorExpression cur = expr, prev = expr;
				foreach (CodeBinaryOperatorExpression op in components) {
					if (i + 1 < complen) {
						cur.Left = op;
						cur.Operator = CodeBinaryOperatorType.BooleanAnd;
						prev.Right = cur;
						prev = cur;
						cur = new CodeBinaryOperatorExpression ();
					} else {
						prev.Right = op;
					}
					i++;
				}
			}

			CodeBinaryOperatorExpression sizeCheck = new CodeBinaryOperatorExpression ();
			sizeCheck.Left = GroupZero ?
				(CodeExpression) new CodeVariableReferenceExpression ("ualength") :
				(CodeExpression) new CodeArgumentReferenceExpression ("ualength");
			sizeCheck.Operator = CodeBinaryOperatorType.GreaterThan;
			sizeCheck.Right = new CodePrimitiveExpression (reqLength);

			CodeBinaryOperatorExpression ret = new CodeBinaryOperatorExpression ();
			ret.Left = sizeCheck;
			ret.Operator = CodeBinaryOperatorType.BooleanAnd;
			ret.Right = expr;
      
			return ret;
		}

		CodeMemberMethod GenerateScanMethod ()
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = String.Format ("ScanForMatch_{0}_{1}", level, groupId);

			method.ReturnType = new CodeTypeReference (typeof (bool));
			method.Parameters.Add (new CodeParameterDeclarationExpression (typeof (string), "ua"));
			CodeParameterDeclarationExpression hasJavaScript =
				new CodeParameterDeclarationExpression (typeof (bool), "hasJavaScript");
			
			hasJavaScript.Direction = FieldDirection.Out;
			method.Parameters.Add (hasJavaScript);
			method.Parameters.Add (new CodeParameterDeclarationExpression (typeof (int), "ualength"));
			method.Attributes = MemberAttributes.Private | MemberAttributes.Static | MemberAttributes.Final;

			// hasJavaScript = <valueOf_DefaultJS>;
			CodeAssignStatement assign = new CodeAssignStatement (new CodeVariableReferenceExpression ("hasJavaScript"),
									      new CodePrimitiveExpression (DefaultJS));

			method.Statements.Add (assign);
			return method;
		}

		CodeBinaryOperatorExpression GenerateScanCondition (string match, int matchLength, int startPosition)
		{
			CodeBinaryOperatorExpression ret = new CodeBinaryOperatorExpression ();

			int endPosition = startPosition + matchLength - 1;
			int matchStartPosition = 0;
			int matchEndPosition = matchLength - 1;
			CodeArgumentReferenceExpression uaRef = new CodeArgumentReferenceExpression ("ua");
      
			if (matchLength == 1) {
				ret.Left = new CodeArrayIndexerExpression (uaRef, new CodePrimitiveExpression (startPosition));
				ret.Operator = CodeBinaryOperatorType.ValueEquality;
				ret.Right = new CodePrimitiveExpression (match [matchStartPosition]);
				return ret;
			}

			CodeBinaryOperatorExpression cur = ret, prev = null, lhs, rhs, tmp;
      
			if (matchLength == 2) {
				lhs = new CodeBinaryOperatorExpression ();
				lhs.Left = new CodeArrayIndexerExpression (uaRef, new CodePrimitiveExpression (startPosition++));
				lhs.Operator = CodeBinaryOperatorType.ValueEquality;
				lhs.Right = new CodePrimitiveExpression (match [matchStartPosition]);

				rhs = new CodeBinaryOperatorExpression ();
				rhs.Left = new CodeArrayIndexerExpression (uaRef, new CodePrimitiveExpression (endPosition--));
				rhs.Operator = CodeBinaryOperatorType.ValueEquality;
				rhs.Right = new CodePrimitiveExpression (match [matchEndPosition]);

				ret.Left = lhs;
				ret.Operator = CodeBinaryOperatorType.BooleanAnd;
				ret.Right = rhs;

				return ret;
			}

			bool matchOdd = matchLength % 2 != 0;
      
			while (matchLength >= 0) {
				matchLength--;
				if (!matchOdd || (matchOdd && (matchLength - 1) > 0)) {
					lhs = new CodeBinaryOperatorExpression ();
					lhs.Left = new CodeArrayIndexerExpression (
						uaRef,
						new CodeBinaryOperatorExpression (
							new CodeVariableReferenceExpression ("startPosition"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression (matchStartPosition))
					);
					lhs.Operator = CodeBinaryOperatorType.ValueEquality;
					lhs.Right = new CodePrimitiveExpression (match [matchStartPosition]);

					rhs = new CodeBinaryOperatorExpression ();
					rhs.Left = new CodeArrayIndexerExpression (
						uaRef,
						new CodeBinaryOperatorExpression (
							new CodeVariableReferenceExpression ("endPosition"),
							CodeBinaryOperatorType.Subtract,
							new CodePrimitiveExpression (matchEndPosition - matchLength))
					);
	  
					rhs.Operator = CodeBinaryOperatorType.ValueEquality;
					rhs.Right = new CodePrimitiveExpression (match [matchEndPosition]);

					tmp = new CodeBinaryOperatorExpression (lhs, CodeBinaryOperatorType.BooleanAnd, rhs);
					matchLength--;
				} else {
					tmp = new CodeBinaryOperatorExpression ();
					tmp.Left = new CodeArrayIndexerExpression (
						uaRef,
						new CodeBinaryOperatorExpression (
							new CodeVariableReferenceExpression ("startPosition"),
							CodeBinaryOperatorType.Add,
							new CodePrimitiveExpression (matchStartPosition - 1))
					);
					tmp.Operator = CodeBinaryOperatorType.ValueEquality;
					tmp.Right = new CodePrimitiveExpression (match [matchStartPosition - 1]);
				}
	
				if (matchLength - 1 >= 0) {
					cur.Left = tmp;
					cur.Operator = CodeBinaryOperatorType.BooleanAnd;
					if (prev != null)
						prev.Right = cur;
					prev = cur;
					cur = new CodeBinaryOperatorExpression ();
				} else
					prev.Right = tmp;

				matchStartPosition++;
				matchEndPosition--;
			}
      
			return ret;
		}
    
		CodeExpression GenerateScanfromExpression (CodeTypeDeclaration mainClass)
		{
			CodeMemberMethod method = GenerateScanMethod ();

			int startPosition = scanfrom + skip;
			string match = matches [0] as string;
			int matchLength = match.Length;
			int minsize = startPosition + matchLength + 1;
      
			// if (ualength < minsize)
			//    return false;
			CodeBinaryOperatorExpression uaSizeCheck = new CodeBinaryOperatorExpression ();
			uaSizeCheck.Left = new CodeArgumentReferenceExpression ("ualength");
			uaSizeCheck.Operator = CodeBinaryOperatorType.LessThan;
			uaSizeCheck.Right = new CodePrimitiveExpression (minsize);
			method.Statements.Add (
				new CodeConditionStatement (uaSizeCheck,
							    new CodeMethodReturnStatement (new CodePrimitiveExpression (false))));      

			// int startPosition = 0;
			method.Statements.Add (
				new CodeVariableDeclarationStatement (typeof (int), "startPosition",
								      new CodePrimitiveExpression (0)));

			// int endPosition = startPosition + matchLength;
			method.Statements.Add (
				new CodeVariableDeclarationStatement (
					typeof (int), "endPosition",
					new CodeBinaryOperatorExpression (
						new CodeVariableReferenceExpression ("startPosition"),
						CodeBinaryOperatorType.Add,
						new CodePrimitiveExpression (matchLength - 1))
				)
			);

			// for (int ualeft = ualength; ualeft >= matchlen; ualeft--) {
			//   if (<condition>) {
			//      hasJavaScript = true;
			//      return true;
			//   }
			//   startPosition++;
			//   endPosition++;
			// }
			CodeIterationStatement iter = new CodeIterationStatement ();
			iter.InitStatement = new CodeVariableDeclarationStatement (
				typeof (int), "ualeft", new CodeArgumentReferenceExpression ("ualength"));
			iter.IncrementStatement = new CodeAssignStatement (
				new CodeVariableReferenceExpression ("ualeft"),
				new CodeBinaryOperatorExpression (new CodeVariableReferenceExpression ("ualeft"),
								  CodeBinaryOperatorType.Subtract,
								  new CodePrimitiveExpression (1))
			);
			iter.TestExpression = new CodeBinaryOperatorExpression (
				new CodeVariableReferenceExpression ("ualeft"),
				CodeBinaryOperatorType.GreaterThanOrEqual,
				new CodePrimitiveExpression (matchLength)
			);

			CodeConditionStatement cond = new CodeConditionStatement (
				GenerateScanCondition (match, matchLength, startPosition));
			
			cond.TrueStatements.Add (
				new CodeAssignStatement (new CodeArgumentReferenceExpression ("hasJavaScript"),
							 new CodePrimitiveExpression (true)));
			cond.TrueStatements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (true)));
			iter.Statements.Add (cond);
			iter.Statements.Add (
				new CodeAssignStatement (new CodeVariableReferenceExpression ("startPosition"),
							 new CodeBinaryOperatorExpression (
								 new CodeVariableReferenceExpression ("startPosition"),
								 CodeBinaryOperatorType.Add,
								 new CodePrimitiveExpression (1)))
			);
			iter.Statements.Add (
				new CodeAssignStatement (new CodeVariableReferenceExpression ("endPosition"),
							 new CodeBinaryOperatorExpression (
								 new CodeVariableReferenceExpression ("endPosition"),
								 CodeBinaryOperatorType.Add,
								 new CodePrimitiveExpression (1)))
			);
			method.Statements.Add (iter);
			method.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (false)));
      
			mainClass.Members.Add (method);

			return new CodeMethodInvokeExpression (
				new CodeMethodReferenceExpression (new CodeTypeReferenceExpression ("UplevelHelper"), method.Name),
				new CodeExpression[] {new CodeArgumentReferenceExpression ("ua"),
						      new CodeDirectionExpression (
							      FieldDirection.Out,
							      new CodeArgumentReferenceExpression ("hasJavaScript")),
						      new CodeArgumentReferenceExpression ("ualength")}
			);
		}
	}

	public class CompileUplevel
	{
		public static void Main (string[] args)
		{
			try {
				CompileUplevel cu = new CompileUplevel ();
				cu.Run (args);
			} catch (Exception ex) {
				Console.Error.WriteLine ("Exception caught while generating UplevelHelper code:");
				Console.Error.Write (ex);
				Console.Error.WriteLine ();
			}
		}

		void Usage (string format, params object[] parms)
		{
			if (format != null && format.Length > 0) {
				Console.Error.WriteLine (format, parms);
				Environment.Exit (1);
			}

			Console.Error.WriteLine (@"Usage: culevel [OPTIONS] INPUT_FILE
Options:
    -o|--o|-output|--output    file to write the generated code to.
                               If not specified, output goes to the console
    -h|--h|-help|--help        show this usage information.
");
			Environment.Exit (0);
		}

		void DumpGroup (GroupDefinition gd, int indent)
		{
			Console.WriteLine ("{0}{1}", new String (' ', indent), gd.ToString ());
			foreach (GroupDefinition gd2 in gd.ChildGroups)
				DumpGroup (gd2, indent + 1);
		}
    
		void Run (string[] args)
		{
			if (args.Length < 1)
				Usage ("Invalid number of parameters");
      
			Stack context = new Stack ();
			GroupDefinition groupZero = new GroupDefinition ();
			GroupDefinition group, current;
			XmlReader reader = null;
			string outfile = null, infile = null;
			string a;
      
			for (int i = 0; i < args.Length; i++) {
				a = args [i];
				if (a [0] == '-' && a.Length > 1) {
					a = a.Substring (1).Trim ();
	  
					switch (a.ToLower ()) {
						case "o":
						case "output":
						case "-output":
							i++;
							if (i > args.Length)
								Usage ("Missing output file name");
							outfile = args [i];
							break;

						case "h":
						case "help":
						case "-help":
							Usage (null);
							break;
	      
						default:
							Usage ("Unknown command line option: '{0}'", a);
							break;
					}
				} else if (infile == null)
					infile = args [i];
			}

			if (infile == null)
				Usage ("Missing input file on the command line.");
      
			try {
				XmlNodeType nodeType;
				int level = 1;
				bool ingroup = false;
	
				reader = new XmlTextReader (infile);
				while (reader.Read ()) {
					nodeType = reader.NodeType;
					if (nodeType != XmlNodeType.Element && nodeType != XmlNodeType.EndElement)
						continue;

					current = context.Count > 0 ? context.Peek () as GroupDefinition : null;
					if (ingroup && reader.LocalName == "except") {
						if (current == null)
							throw new ApplicationException ("Inside a group but there is no group on the stack");

						current.AddExcept (reader);
						continue;
					}
	    
					if (reader.LocalName != "group")
						continue;
	    
					if (reader.NodeType == XmlNodeType.EndElement) {
						if (current == null)
							throw new ApplicationException ("Found group end, but no current group on stack");
						context.Pop ();
						if (context.Count == 0) {
							groupZero.ChildGroups.Add (current);
							current.GroupId = groupZero.ChildGroups.Count;
						}
						level--;
						ingroup = false;
						continue;
					}
	    
					group = new GroupDefinition (reader);
					group.Level = level++;
	    
					if (current != null) {
						current.ChildGroups.Add (group);
						group.GroupId = current.ChildGroups.Count;
					}
	    
					context.Push (group);
					ingroup = true;
				}
			} catch (Exception) {
				throw;
			} finally {
				if (reader != null)
					reader.Close();
			}

			CodeCompileUnit unit = groupZero.GenerateCode ();
			if (unit == null)
				Environment.Exit (1);
      
			CodeDomProvider provider = new CSharpCodeProvider ();
			ICodeGenerator gen = provider.CreateGenerator ();

			TextWriter tw;
			if (outfile == null)
				tw = Console.Out;
			else
				tw = new IndentedTextWriter (new StreamWriter (outfile, false), "\t");
			gen.GenerateCodeFromCompileUnit (unit, tw, new CodeGeneratorOptions ());
			if (outfile != null)
				tw.Close ();
		}
	}
}
