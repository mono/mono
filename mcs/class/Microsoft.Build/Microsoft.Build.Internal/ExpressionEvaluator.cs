using System;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Build.Internal
{
	class ExpressionEvaluator
	{
		public ExpressionEvaluator (Project project, string replacementForMissingPropertyAndItem)
		{
			this.Project = project;
			ReplacementForMissingPropertyAndItem = replacementForMissingPropertyAndItem;
		}
		
		public Project Project { get; private set; }
		
		public string ReplacementForMissingPropertyAndItem { get; set; }
		
		// it is to prevent sequential property value expansion in boolean expression
		public string Wrapper {
			get { return ReplacementForMissingPropertyAndItem != null ? "'" : null; }
		}
		
		public string Evaluate (string source)
		{
			return Evaluate (source, new ExpressionParserManual (source, ExpressionValidationType.LaxString).Parse ());
		}
		
		string Evaluate (string source, ExpressionList exprList)
		{
			if (exprList == null)
				throw new ArgumentNullException ("exprList");
			return string.Concat (exprList.Select (e => e.EvaluateAsString (new EvaluationContext (this))));
		}
		
		public bool EvaluateAsBoolean (string source)
		{
			try {
				var el = new ExpressionParser ().Parse (source, ExpressionValidationType.StrictBoolean);
				if (el.Count () != 1)
					throw new InvalidProjectFileException ("Unexpected number of tokens: " + el.Count ());
				return el.First ().EvaluateAsBoolean (new EvaluationContext (this));
			} catch (yyParser.yyException ex) {
				throw new InvalidProjectFileException (string.Format ("failed to evaluate expression as boolean: '{0}': {1}", source, ex.Message), ex);
			}
		}
	}
	
	class EvaluationContext
	{
		public EvaluationContext (ExpressionEvaluator evaluator)
		{
			Evaluator = evaluator;
		}
		
		public ExpressionEvaluator Evaluator { get; private set; }
		public Project Project {
			get { return Evaluator.Project; }
		}
		public ProjectItem ContextItem { get; set; }		
		
		List<ProjectItem> items = new List<ProjectItem> ();
		List<ProjectProperty> props = new List<ProjectProperty> ();
		
		public string EvaluateItem (ProjectItem item)
		{
			if (items.Contains (item))
				throw new InvalidProjectFileException (string.Format ("Recursive reference to item '{0}' with include '{1}' was found", item.ItemType, item.UnevaluatedInclude));
			try {
				items.Add (item);
				// FIXME: needs verification if string evaluation is appropriate.
				return Evaluator.Evaluate (item.UnevaluatedInclude);
			} finally {
				items.Remove (item);
			}
		}
		
		public string EvaluateProperty (ProjectProperty prop)
		{
			if (props.Contains (prop))
				throw new InvalidProjectFileException (string.Format ("Recursive reference to property '{0}' was found", prop.Name));
			try {
				props.Add (prop);
				// FIXME: needs verification if string evaluation is appropriate.
				return Evaluator.Evaluate (prop.UnevaluatedValue);
			} finally {
				props.Remove (prop);
			}
		}
	}
	
	abstract partial class Expression
	{
		public abstract string EvaluateAsString (EvaluationContext context);
		public abstract bool EvaluateAsBoolean (EvaluationContext context);
		public abstract object EvaluateAsObject (EvaluationContext context);
		
		public bool EvaluateStringAsBoolean (string ret)
		{
			if (ret != null) {
				if (ret.Equals ("TRUE", StringComparison.InvariantCultureIgnoreCase))
					return true;
				else if (ret.Equals ("FALSE", StringComparison.InvariantCultureIgnoreCase))
					return false;
			}
			throw new InvalidProjectFileException (this.Location, null, string.Format ("String is evaluated as '{0}' and cannot be converted to boolean", ret));
		}
	}
	
	partial class BinaryExpression : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			switch (Operator) {
			case Operator.EQ:
				return Left.EvaluateAsString (context) == Right.EvaluateAsString (context);
			case Operator.NE:
				return Left.EvaluateAsString (context) != Right.EvaluateAsString (context);
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
			return EvaluateStringAsBoolean (ret);
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			var ret = EvaluateAsObject (context);
			// FIXME: this "wrapper" is kind of hack, to prevent sequential property references such as $(X)$(Y).
			return ret == null ? context.Evaluator.ReplacementForMissingPropertyAndItem : context.Evaluator.Wrapper + ret.ToString () + context.Evaluator.Wrapper;
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
				var prop = context.Project.GetProperty (Access.Name.Name);
				if (prop == null)
					return null;
				return context.EvaluateProperty (prop);
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
							throw new InvalidProjectFileException (Location, string.Format ("access to undefined method '{0}' of '{1}' at {2}", Access.Name.Name, Access.Target.EvaluateAsString (context), Location));
						return method.Invoke (null, AdjustArgsForCall (method, args));
					} else {
						var prop = type.GetProperty (Access.Name.Name);
						if (prop == null)
							throw new InvalidProjectFileException (Location, string.Format ("access to undefined property '{0}' of '{1}' at {2}", Access.Name.Name, Access.Target.EvaluateAsString (context), Location));
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
				    pl.Last ().CustomAttributes.Any (a => a.AttributeType == typeof (ParamArrayAttribute)))
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
			return EvaluateStringAsBoolean (EvaluateAsString (context));
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			var items = context.Project.GetItems (Application.Name.Name);
			if (!items.Any ())
				return context.Evaluator.ReplacementForMissingPropertyAndItem;
			if (Application.Expressions == null)
				return string.Join (";", items.Select (item => context.EvaluateItem (item)).Select (inc => !string.IsNullOrWhiteSpace (inc)));
			else
				return string.Join (";", items.Select (item => string.Concat (Application.Expressions.Select (e => e.EvaluateAsString (context)))).ToArray ());
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			return EvaluateAsString (context);
		}
	}

	partial class MetadataAccessExpression : Expression
	{
		public override string EvaluateAsString (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
		
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
	}
	partial class StringLiteral : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			var ret = EvaluateAsString (context);
			return EvaluateStringAsBoolean (ret);
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			return this.Value.Name;
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
	
	partial class FunctionCallExpression : Expression
	{
		public override string EvaluateAsString (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
		
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			throw new NotImplementedException ();
		}
	}
}

