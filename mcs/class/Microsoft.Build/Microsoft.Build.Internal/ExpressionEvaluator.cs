using System;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Exceptions;
using System.Collections.Generic;

namespace Microsoft.Build.Internal
{
	class ExpressionEvaluator
	{
		public ExpressionEvaluator (Project project)
		{
			this.Project = project;
		}
		
		public Project Project { get; private set; }
		
		public string Evaluate (string source)
		{
			return Evaluate (source, new ExpressionParserManual ().Parse (source, ExpressionValidationType.LaxString));
			//return Evaluate (new ExpressionParser ().Parse (source, validationType));
		}
		
		public string Evaluate (string source, ExpressionList exprList)
		{
			if (exprList == null)
				throw new ArgumentNullException ("exprList");
			return string.Concat (exprList.Select (e => e.EvaluateAsString (new EvaluationContext (this))));
		}
		
		public bool EvaluateAsBoolean (string source)
		{
			var el = new ExpressionParserManual ().Parse (source, ExpressionValidationType.StrictBoolean);
			if (el.Count () != 1)
				throw new InvalidProjectFileException ("Unexpected number of tokens");
			return el.First ().EvaluateAsBoolean (new EvaluationContext (this));
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
			return ret == null ? null : ret.ToString ();
		}
		
		public override object EvaluateAsObject (EvaluationContext context)
		{
			if (Access.Target == null) {
				var prop = context.Project.GetProperty (Access.Name.Name);
				if (prop == null)
					return null;
				return context.EvaluateProperty (prop);
			} else {
				var obj = EvaluateAsObject (context);
				if (obj == null)
					return null;
				var prop = obj.GetType ().GetProperty (Access.Name.Name);
				if (prop == null)
					throw new InvalidProjectFileException (string.Format ("access to undefined property '{0}' at {1}", Access.Name, Location));
				return prop.GetValue (obj, null);
			}
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
	partial class StringLiteralExpression : Expression
	{
		public override bool EvaluateAsBoolean (EvaluationContext context)
		{
			var ret = EvaluateAsString (context);
			return EvaluateStringAsBoolean (ret);
		}
		
		public override string EvaluateAsString (EvaluationContext context)
		{
			return string.Concat (Contents.Select (e => e.EvaluateAsString (context)));
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

