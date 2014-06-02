//
// ExpressionEvaluator.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using System.IO;

namespace Microsoft.Build.Internal.Expressions
{
	class ExpressionEvaluator
	{
		public ExpressionEvaluator (Project project)
		{
			Project = project;
		}
		
		public ExpressionEvaluator (ProjectInstance project)
		{
			ProjectInstance = project;
		}
		
		EvaluationContext CreateContext (string source)
		{
			return new EvaluationContext (source, this);
		}
		
		public Project Project { get; private set; }
		public ProjectInstance ProjectInstance { get; set; }

		List<ITaskItem> evaluated_task_items = new List<ITaskItem> ();

		public IList<ITaskItem> EvaluatedTaskItems {
			get { return evaluated_task_items; }
		}

		public string Evaluate (string source)
		{
			return Evaluate (source, new ExpressionParserManual (source ?? string.Empty, ExpressionValidationType.LaxString).Parse ());
		}
		
		string Evaluate (string source, ExpressionList exprList)
		{
			if (exprList == null)
				throw new ArgumentNullException ("exprList");
			return string.Concat (exprList.Select (e => e.EvaluateAsString (CreateContext (source))));
		}
		
		public bool EvaluateAsBoolean (string source)
		{
			try {
				var el = new ExpressionParser ().Parse (source, ExpressionValidationType.StrictBoolean);
				if (el.Count () != 1)
					throw new InvalidProjectFileException ("Unexpected number of tokens: " + el.Count ());
				return el.First ().EvaluateAsBoolean (CreateContext (source));
			} catch (yyParser.yyException ex) {
				throw new InvalidProjectFileException (string.Format ("failed to evaluate expression as boolean: '{0}': {1}", source, ex.Message), ex);
			}
		}
	}
	
	class EvaluationContext
	{
		public EvaluationContext (string source, ExpressionEvaluator evaluator)
		{
			Source = source;
			Evaluator = evaluator;
		}

		public string Source { get; private set; }
		
		public ExpressionEvaluator Evaluator { get; private set; }
		public object ContextItem { get; set; }
		
		Stack<object> evaluating_items = new Stack<object> ();
		Stack<object> evaluating_props = new Stack<object> ();

		public IEnumerable<object> GetItems (string name)
		{
			if (Evaluator.Project != null)
				return Evaluator.Project.GetItems (name);
			else
				return Evaluator.ProjectInstance.GetItems (name);
		}

		public IEnumerable<object> GetAllItems ()
		{
			if (Evaluator.Project != null)
				return Evaluator.Project.AllEvaluatedItems;
			else
				return Evaluator.ProjectInstance.AllEvaluatedItems;
		}
		
		public string EvaluateItem (string itemType, object item)
		{
			if (evaluating_items.Contains (item))
				throw new InvalidProjectFileException (string.Format ("Recursive reference to item '{0}' was found", itemType));
			try {
				evaluating_items.Push (item);
				var eval = item as ProjectItem;
				if (eval != null)
					return Evaluator.Evaluate (eval.EvaluatedInclude);
				else {
					var inst = (ProjectItemInstance) item;
					if (!Evaluator.EvaluatedTaskItems.Contains (inst))
						Evaluator.EvaluatedTaskItems.Add (inst);
					return Evaluator.Evaluate (inst.EvaluatedInclude);
				}
			} finally {
				evaluating_items.Pop ();
			}
		}
				
		public string EvaluateProperty (string name)
		{
			if (Evaluator.Project != null) {
				var prop = Evaluator.Project.GetProperty (name);
				if (prop == null)
					return null;
				return EvaluateProperty (prop, prop.Name, prop.EvaluatedValue);
			} else {
				var prop = Evaluator.ProjectInstance.GetProperty (name);
				if (prop == null)
					return null;
				return EvaluateProperty (prop, prop.Name, prop.EvaluatedValue);
			}
		}
		
		public string EvaluateProperty (object prop, string name, string value)
		{
			if (evaluating_props.Contains (prop))
				throw new InvalidProjectFileException (string.Format ("Recursive reference to property '{0}' was found", name));
			try {
				evaluating_props.Push (prop);
				// FIXME: needs verification on whether string evaluation is appropriate or not.
				return Evaluator.Evaluate (value);
			} finally {
				evaluating_props.Pop ();
			}
		}
	}
	
	abstract partial class Expression
	{
		public abstract string ExpressionString { get; }
		public abstract string EvaluateAsString (EvaluationContext context);
		public abstract bool EvaluateAsBoolean (EvaluationContext context);
		public abstract object EvaluateAsObject (EvaluationContext context);

		public bool EvaluateStringAsBoolean (EvaluationContext context, string ret)
		{
			if (ret != null) {
				if (ret.Equals ("TRUE", StringComparison.InvariantCultureIgnoreCase))
					return true;
				else if (ret.Equals ("FALSE", StringComparison.InvariantCultureIgnoreCase))
					return false;
			}
			throw new InvalidProjectFileException (this.Location, string.Format ("Part of condition '{0}' is evaluated as '{1}' and cannot be converted to boolean", context.Source, ret));
		}
	}
	
	partial class BinaryExpression : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			switch (Operator) {
			case Operator.EQ:
				return string.Equals (StripStringWrap (Left.EvaluateAsString (context)), StripStringWrap (Right.EvaluateAsString (context)), StringComparison.OrdinalIgnoreCase);
			case Operator.NE:
				return !string.Equals (StripStringWrap (Left.EvaluateAsString (context)), StripStringWrap (Right.EvaluateAsString (context)), StringComparison.OrdinalIgnoreCase);
			case Operator.And:
			case Operator.Or:
				// evaluate first, to detect possible syntax error on right expr.
				var lb = Left.EvaluateAsBoolean (context);
				var rb = Right.EvaluateAsBoolean (context);
				return Operator == Operator.And ? (lb && rb) : (lb || rb);
			}
			// comparison expressions - evaluate comparable first, then compare values.
			var left = Left.EvaluateAsObject (context);
			var right = Right.EvaluateAsObject (context);
			if (!(left is IComparable && right is IComparable))
				throw new InvalidProjectFileException ("expression cannot be evaluated as boolean");
			var result = ((IComparable) left).CompareTo (right);
			switch (Operator) {
			case Operator.GE:
				return result >= 0;
			case Operator.GT:
				return result > 0;
			case Operator.LE:
				return result <= 0;
			case Operator.LT:
				return result < 0;
			}
			throw new InvalidOperationException ();
		}
		
		string StripStringWrap (string s)
		{
			if (s == null)
				return string.Empty;
			s = s.Trim ();
			if (s.Length > 1 && s [0] == '"' && s [s.Length - 1] == '"')
				return s.Substring (1, s.Length - 2);
			else if (s.Length > 1 && s [0] == '\'' && s [s.Length - 1] == '\'')
				return s.Substring (1, s.Length - 2);
			return s;
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
		
		static readonly Dictionary<Operator,string> strings = new Dictionary<Operator, string> () {
			{Operator.EQ, " == "},
			{Operator.NE, " != "},
			{Operator.LT, " < "},
			{Operator.LE, " <= "},
			{Operator.GT, " > "},
			{Operator.GE, " >= "},
			{Operator.And, " And "},
			{Operator.Or, " Or "},
		};
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			return Left.EvaluateAsString (context) + strings [Operator] + Right.EvaluateAsString (context);
		}
	}
	
	partial class BooleanLiteral : Expression
	{
		public override string EvaluateAsString (EvaluationContext context)
		{
			return Value ? "True" : "False";
		}
		
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			return Value;
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			return Value;
		}
	}

	partial class NotExpression : Expression
	{
		public override string EvaluateAsString (EvaluationContext context)
		{
			// no negation for string
			return "!" + Negated.EvaluateAsString (context);
		}
		
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			return !Negated.EvaluateAsBoolean (context);
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			return EvaluateAsString (context);
		}
	}

	partial class PropertyAccessExpression : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			var ret = EvaluateAsString (context);
			return EvaluateStringAsBoolean (context, ret);
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			var ret = EvaluateAsObject (context);
			return ret == null ? null : ret.ToString ();
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			try {
				return DoEvaluateAsObject (context);
			} catch (TargetInvocationException ex) {
				throw new InvalidProjectFileException ("Access to property caused an error", ex);
			}
		}
		
		object DoEvaluateAsObject (EvaluationContext context)
		{
			if (Access.Target == null) {
				return context.EvaluateProperty (Access.Name.Name);
			} else {
				if (this.Access.TargetType == PropertyTargetType.Object) {
					var obj = Access.Target.EvaluateAsObject (context);
					if (obj == null)
						return null;
					if (Access.Arguments != null) {
						var args = Access.Arguments.Select (e => e.EvaluateAsObject (context)).ToArray ();
						var method = FindMethod (obj.GetType (), Access.Name.Name, args);
						if (method == null)
							throw new InvalidProjectFileException (Location, string.Format ("access to undefined method '{0}' of '{1}' at {2}", Access.Name.Name, Access.Target.EvaluateAsString (context), Location));
						return method.Invoke (obj, AdjustArgsForCall (method, args));
					} else {
						var prop = obj.GetType ().GetProperty (Access.Name.Name);
						if (prop == null)
							throw new InvalidProjectFileException (Location, string.Format ("access to undefined property '{0}' of '{1}' at {2}", Access.Name.Name, Access.Target.EvaluateAsString (context), Location));
						return prop.GetValue (obj, null);
					}
				} else {
					var type = Type.GetType (Access.Target.EvaluateAsString (context));
					if (type == null)
						throw new InvalidProjectFileException (Location, string.Format ("specified type '{0}' was not found", Access.Target.EvaluateAsString (context)));
					if (Access.Arguments != null) {
						var args = Access.Arguments.Select (e => e.EvaluateAsObject (context)).ToArray ();
						var method = FindMethod (type, Access.Name.Name, args);
						if (method == null)
							throw new InvalidProjectFileException (Location, string.Format ("access to undefined static method '{0}' of '{1}' at {2}", Access.Name.Name, type, Location));
						return method.Invoke (null, AdjustArgsForCall (method, args));
					} else {
						var prop = type.GetProperty (Access.Name.Name);
						if (prop == null)
							throw new InvalidProjectFileException (Location, string.Format ("access to undefined static property '{0}' of '{1}' at {2}", Access.Name.Name, type, Location));
						return prop.GetValue (null, null);
					}
				}
			}
		}
	
		MethodInfo FindMethod (Type type, string name, object [] args)
		{
			var methods = type.GetMethods ().Where (m => {
				if (m.Name != name)
					return false;
				var pl = m.GetParameters ();
				if (pl.Length == args.Length)
					return true;
				// calling String.Format() with either set of arguments is valid:
				// - three strings (two for varargs)
				// - two strings (happen to be exact match)
				// - one string (no varargs)
				if (pl.Length > 0 && pl.Length - 1 <= args.Length &&
				    pl.Last ().GetCustomAttributesData ().Any (a => a.Constructor.DeclaringType == typeof (ParamArrayAttribute)))
					return true;
				return false;
				});
			if (methods.Count () == 1)
				return methods.First ();
			return args.Any (a => a == null) ? 
				type.GetMethod (name) :
				type.GetMethod (name, args.Select (o => o.GetType ()).ToArray ());
		}
		
		object [] AdjustArgsForCall (MethodInfo m, object[] args)
		{
			if (m.GetParameters ().Length == args.Length + 1)
				return args.Concat (new object[] {Array.CreateInstance (m.GetParameters ().Last ().ParameterType.GetElementType (), 0)}).ToArray ();
			else
				return args;
		}
	}

	partial class ItemAccessExpression : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			return EvaluateStringAsBoolean (context, EvaluateAsString (context));
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			string itemType = Application.Name.Name;
			var items = context.GetItems (itemType);
			if (!items.Any ())
				return null;
			if (Application.Expressions == null)
				return string.Join (";", items.Select (item => Unwrap (context.EvaluateItem (itemType, item))));
			else
				return string.Join (";", items.Select (item => {
					context.ContextItem = item;
					var ret = Unwrap (string.Concat (Application.Expressions.Select (e => e.EvaluateAsString (context))));
					context.ContextItem = null;
					return ret;
				}));
		}

		static string Unwrap (string ret)
		{
			if (ret.Length < 2 || ret [0] != ret [ret.Length - 1] || ret [0] != '"' && ret [0] != '\'')
				return ret;
			return ret.Substring (1, ret.Length - 2);
		}

		public override object EvaluateAsObject (EvaluationContext context)
		{
			return EvaluateAsString (context);
		}
	}

	partial class MetadataAccessExpression : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			return EvaluateStringAsBoolean (context, EvaluateAsString (context));
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			string itemType = this.Access.ItemType != null ? this.Access.ItemType.Name : null;
			string metadataName = Access.Metadata.Name;
			IEnumerable<object> items;
			if (this.Access.ItemType != null)
				items = context.GetItems (itemType);
			else if (context.ContextItem != null)
				items = new Object [] { context.ContextItem };
			else
				items = context.GetAllItems ();
			
			var values = items.Select (i => (i is ProjectItem) ? ((ProjectItem) i).GetMetadataValue (metadataName) : ((ProjectItemInstance) i).GetMetadataValue (metadataName)).Where (s => !string.IsNullOrEmpty (s));
			return string.Join (";", values);
		}

		public override object EvaluateAsObject (EvaluationContext context)
		{
			return EvaluateAsString (context);
		}
	}
	partial class StringLiteral : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			var ret = EvaluateAsString (context);
			return EvaluateStringAsBoolean (context, ret);
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			return context.Evaluator.Evaluate (this.Value.Name);
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			return EvaluateAsString (context);
		}
	}
	partial class RawStringLiteral : Expression
	{
		public override string EvaluateAsString (EvaluationContext context)
		{
			return Value.Name;
		}
		
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			throw new InvalidProjectFileException ("raw string literal cannot be evaluated as boolean");
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			return EvaluateAsString (context);
		}
	}

	partial class QuotedExpression : Expression
	{
		public override string EvaluateAsString (EvaluationContext context)
		{
			return QuoteChar + EvaluateAsStringWithoutQuote (context) + QuoteChar;
		}

		public string EvaluateAsStringWithoutQuote (EvaluationContext context)
		{
			return string.Concat (Contents.Select (e => e.EvaluateAsString (context)));
		}

		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			var ret = EvaluateAsStringWithoutQuote (context);
			return EvaluateStringAsBoolean (context, ret);
		}

		public override object EvaluateAsObject (EvaluationContext context)
		{
			return EvaluateAsStringWithoutQuote (context);
		}
	}
	
	partial class FunctionCallExpression : Expression
	{
		public override string EvaluateAsString (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
		
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			if (string.Equals (Name.Name, "Exists", StringComparison.OrdinalIgnoreCase)) {
				if (Arguments.Count != 1)
					throw new InvalidProjectFileException (Location, "Function 'Exists' expects 1 argument");
				string val = Arguments.First ().EvaluateAsString (context);
				val = WindowsCompatibilityExtensions.FindMatchingPath (val);
				return Directory.Exists (val) || System.IO.File.Exists (val);
			}
			if (string.Equals (Name.Name, "HasTrailingSlash", StringComparison.OrdinalIgnoreCase)) {
				if (Arguments.Count != 1)
					throw new InvalidProjectFileException (Location, "Function 'HasTrailingSlash' expects 1 argument");
				string val = Arguments.First ().EvaluateAsString (context);
				return val.LastOrDefault () == '\\' || val.LastOrDefault () == '/';
			}
			throw new InvalidProjectFileException (Location, string.Format ("Unsupported function '{0}'", Name));
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
	}
}

