//
// System.Xml.XPath.XPathExpression support classes
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
//
using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Globalization;
using Mono.Xml.XPath;

namespace System.Xml.XPath
{
#if XPATH_DEBUG
	internal class CompiledExpression : Test.Xml.XPath.XPathExpression
#else
	internal class CompiledExpression : XPathExpression
#endif
	{
		protected XmlNamespaceManager _nsm;
		protected Expression _expr;
		XPathSorters _sorters;


		public CompiledExpression (Expression expr)
		{
			_expr = expr;
		}
		private CompiledExpression (CompiledExpression other)
		{
			_nsm = other._nsm;
			_expr = other._expr;
		}
#if XPATH_DEBUG
		public override Test.Xml.XPath.XPathExpression Clone () { return new CompiledExpression (this); }
#else
		public override XPathExpression Clone () { return new CompiledExpression (this); }
#endif

		public override void SetContext (XmlNamespaceManager nsManager)
		{
			_nsm = nsManager;
		}
		internal XmlNamespaceManager NamespaceManager { get { return _nsm; } }
		public override String Expression { get { return _expr.ToString (); }}
		public override XPathResultType ReturnType { get { return _expr.ReturnType; }}

		public object Evaluate (BaseIterator iter)
		{
			if (_sorters != null)
				return EvaluateNodeSet (iter);

			try
			{
				return _expr.Evaluate (iter);
			}
			catch (XPathException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new XPathException ("Error during evaluation", e);
			}
		}
		public XPathNodeIterator EvaluateNodeSet (BaseIterator iter)
		{
			try
			{
				BaseIterator iterResults = (BaseIterator) _expr.EvaluateNodeSet (iter);
				if (_sorters != null)
					return _sorters.Sort (iterResults);
				return iterResults;
			}
			catch (XPathException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new XPathException ("Error during evaluation", e);
			}
		}

		public override void AddSort (Object obj, IComparer cmp)
		{
			if (_sorters == null)
				_sorters = new XPathSorters ();
			_sorters.Add (obj, cmp);
		}
		public override void AddSort(object expr, XmlSortOrder orderSort, XmlCaseOrder orderCase, string lang, XmlDataType dataType)
		{
			if (_sorters == null)
				_sorters = new XPathSorters ();
			_sorters.Add (expr, orderSort, orderCase, lang, dataType);
		}

		class XPathSorters : IComparer
		{
			readonly ArrayList _rgSorters = new ArrayList ();

			public void Add (object expr, IComparer cmp)
			{
				_rgSorters.Add (new XPathSorter (expr, cmp));
			}

			public void Add (object expr, XmlSortOrder orderSort, XmlCaseOrder orderCase, string lang, XmlDataType dataType)
			{
				_rgSorters.Add (new XPathSorter (expr, orderSort, orderCase, lang, dataType));
			}

			public BaseIterator Sort (BaseIterator iter)
			{
				ArrayList rgElts = new ArrayList ();
				int cSorters = _rgSorters.Count;
				while (iter.MoveNext ())
				{
					XPathSortElement elt = new XPathSortElement ();
					elt.Navigator = iter.Current.Clone ();
					elt.Values = new object [cSorters];
					for (int iSorter = 0; iSorter < _rgSorters.Count; ++iSorter)
					{
						XPathSorter sorter = (XPathSorter) _rgSorters [iSorter];
						elt.Values [iSorter] = sorter.Evaluate (iter);
					}
					rgElts.Add (elt);
				}
				rgElts.Sort (this);
				XPathNavigator [] rgResults = new XPathNavigator [rgElts.Count];
				for (int iResult = 0; iResult < rgElts.Count; ++iResult)
				{
					XPathSortElement elt = (XPathSortElement) rgElts [iResult];
					rgResults [iResult] = elt.Navigator;
				}
				return new EnumeratorIterator (iter, rgResults.GetEnumerator ());
			}

			class XPathSortElement
			{
				public XPathNavigator Navigator;
				public object [] Values;
			}

			int IComparer.Compare (object o1, object o2)
			{
				XPathSortElement elt1 = (XPathSortElement) o1;
				XPathSortElement elt2 = (XPathSortElement) o2;
				for (int iSorter = 0; iSorter < _rgSorters.Count; ++iSorter)
				{
					XPathSorter sorter = (XPathSorter) _rgSorters [iSorter];
					int cmp = sorter.Compare (elt1.Values [iSorter], elt2.Values [iSorter]);
					if (cmp != 0)
						return cmp;
				}
				return 0;
			}

			class XPathSorter
			{
				readonly Expression _expr;
				readonly IComparer _cmp;
				readonly XmlDataType _type;

				public XPathSorter (object expr, IComparer cmp)
				{
					_expr = ExpressionFromObject (expr);
					_cmp = cmp;
					_type = XmlDataType.Text;
				}

				public XPathSorter (object expr, XmlSortOrder orderSort, XmlCaseOrder orderCase, string lang, XmlDataType dataType)
				{
					_expr = ExpressionFromObject (expr);
					_type = dataType;
					if (dataType == XmlDataType.Number)
						_cmp = new XPathNumberComparer (orderSort);
					else
						_cmp = new XPathTextComparer (orderSort, orderCase, lang);
				}

				static Expression ExpressionFromObject (object expr)
				{
					if (expr is CompiledExpression)
						return ((CompiledExpression) expr)._expr;
					if (expr is string)
					{
						Tokenizer tokenizer = new Tokenizer ((string) expr);
						XPathParser parser = new XPathParser ();
						return (Expression) parser.yyparseSafe (tokenizer);
					}
					throw new XPathException ("Invalid query object");
				}

				public object Evaluate (BaseIterator iter)
				{
					if (_type == XmlDataType.Number)
						return _expr.EvaluateNumber (iter);
					return _expr.EvaluateString (iter);
				}

				public int Compare (object o1, object o2)
				{
					return _cmp.Compare (o1, o2);
				}

				class XPathNumberComparer : IComparer
				{
					int _nMulSort;

					public XPathNumberComparer (XmlSortOrder orderSort)
					{
						_nMulSort = (orderSort == XmlSortOrder.Ascending) ? 1 : -1;
					}

					int IComparer.Compare (object o1, object o2)
					{
						double num1 = (double) o1;
						double num2 = (double) o2;
						if (num1 < num2)
							return -_nMulSort;
						if (num1 > num2)
							return _nMulSort;
						if (num1 == num2)
							return 0;
						if (double.IsNaN (num1))
							return (double.IsNaN (num2)) ? 0 : -_nMulSort;
						return _nMulSort;
					}
				}

				class XPathTextComparer : IComparer
				{
					int _nMulSort;
					int _nMulCase;
					XmlCaseOrder _orderCase;
					CultureInfo _ci;

					public XPathTextComparer (XmlSortOrder orderSort, XmlCaseOrder orderCase, string strLang)
					{
						_orderCase = orderCase;
						_nMulCase = (orderCase == XmlCaseOrder.UpperFirst) ? -1 : 1;

						_nMulSort = (orderSort == XmlSortOrder.Ascending) ? 1 : -1;

						if (strLang == null || strLang == "")
							_ci = CultureInfo.CurrentCulture;	// TODO: defer until evaluation?
						else
							_ci = new CultureInfo (strLang);
					}

					int IComparer.Compare (object o1, object o2)
					{
						string str1 = (string) o1;
						string str2 = (string) o2;
						int cmp = String.Compare (str1, str2, true, _ci);
						if (cmp != 0 || _orderCase == XmlCaseOrder.None)
							return cmp * _nMulSort;
						return _nMulSort * _nMulCase * String.Compare (str1, str2, false, _ci);
					}
				}
			}
		}
	}


	/// <summary>
	/// Summary description for Expression.
	/// </summary>
	internal abstract class Expression
	{
		private static XsltContext _ctxDefault = new DefaultContext ();
		public Expression ()
		{
		}
		public abstract XPathResultType ReturnType { get; }
		public virtual XPathResultType GetReturnType (BaseIterator iter) { return ReturnType; }
		public abstract object Evaluate (BaseIterator iter);

		public BaseIterator EvaluateNodeSet (BaseIterator iter)
		{
			XPathResultType type = GetReturnType (iter);
			if (type == XPathResultType.NodeSet ||
				type == XPathResultType.Any)
			{
				BaseIterator iterResult = Evaluate (iter) as BaseIterator;
				if (iterResult != null)
					return iterResult;
			}
			throw new XPathException ("expected nodeset: "+ToString ());
		}
		protected static XsltContext DefaultContext { get { return _ctxDefault; } }

		static XPathResultType GetReturnType (object obj)
		{
			if (obj is string)
				return XPathResultType.String;
			if (obj is bool)
				return XPathResultType.Boolean;
			if (obj is XPathNodeIterator)
				return XPathResultType.NodeSet;
			if (obj is double || obj is int)
				return XPathResultType.Number;
			throw new XPathException ("invalid node type: "+obj.GetType ().ToString ());
		}
		[MonoTODO]
		public double EvaluateNumber (BaseIterator iter)
		{
			object result;
			XPathResultType type = GetReturnType (iter);
			if (type == XPathResultType.NodeSet)
			{
				result = EvaluateString (iter);
				type = XPathResultType.String;
			}
			else
				result = Evaluate (iter);

			if (type == XPathResultType.Any)
				type = GetReturnType (result);

			switch (type)
			{
				case XPathResultType.Number:
					return Convert.ToDouble (result);
				case XPathResultType.Boolean:
					return Convert.ToDouble ((bool) result);
				case XPathResultType.NodeSet:
					return XPathFunctions.ToNumber (EvaluateString (iter));
				case XPathResultType.String:
					return XPathFunctions.ToNumber ((string) result);	// TODO: spec? convert string to number
				default:
					throw new XPathException ("invalid node type"); // TODO: handle other types
			}
		}
		[MonoTODO]
		public string EvaluateString (BaseIterator iter)
		{
			object result = Evaluate (iter);
			XPathResultType type = GetReturnType (iter);
			if (type == XPathResultType.Any)
				type = GetReturnType (result);
			switch (type)
			{
				case XPathResultType.Number:
					return (string) XmlConvert.ToString (Convert.ToDouble (result));	// TODO: spec? convert number to string
				case XPathResultType.Boolean:
					return ((bool) result) ? "true" : "false";
				case XPathResultType.String:
					return (string) result;
				case XPathResultType.NodeSet:
				{
					BaseIterator iterResult = (BaseIterator) result;
					if (iterResult == null || !iterResult.MoveNext ())
						return "";
					return iterResult.Current.Value;
				}
				default:
					throw new XPathException ("invalid node type"); // TODO: handle other types
			}
		}
		[MonoTODO]
		public bool EvaluateBoolean (BaseIterator iter)
		{
			object result = Evaluate (iter);
			XPathResultType type = GetReturnType (iter);
			if (type == XPathResultType.Any)
				type = GetReturnType (result);
			switch (type)
			{
				case XPathResultType.Number:
				{
					double num = Convert.ToDouble (result);
					return (num != 0.0 && num != -0.0 && num != Double.NaN);
				}
				case XPathResultType.Boolean:
					return (bool) result;
				case XPathResultType.String:
					return ((string) result).Length != 0;
				case XPathResultType.NodeSet:
				{
					BaseIterator iterResult = (BaseIterator) result;
					return (iterResult != null && iterResult.MoveNext ());
				}
				default:
					throw new XPathException ("invalid node type"); // TODO: handle other types
			}
		}
		public object EvaluateAs (BaseIterator iter, XPathResultType type)
		{
			switch (type)
			{
			case XPathResultType.Boolean:
				return EvaluateBoolean (iter);
			case XPathResultType.NodeSet:
				return EvaluateNodeSet (iter);
			case XPathResultType.String:
				return EvaluateString (iter);
			case XPathResultType.Number:
				return EvaluateNumber (iter);
			}
			return Evaluate (iter);
		}
	}

	internal abstract class ExprBinary : Expression
	{
		protected Expression _left, _right;

		public ExprBinary (Expression left, Expression right)
		{
			_left = left;
			_right = right;
		}
		public override String ToString ()
		{
			return _left.ToString () + ' ' + Operator + ' ' + _right.ToString ();
		}
		protected abstract String Operator { get; }
	}

	internal abstract class ExprBoolean : ExprBinary
	{
		public ExprBoolean (Expression left, Expression right) : base (left, right) {}
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
	}

	internal class ExprOR : ExprBoolean
	{
		public ExprOR (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "or"; }}
		public override object Evaluate (BaseIterator iter)
		{
			if (_left.EvaluateBoolean (iter))
				return true;
			return _right.EvaluateBoolean (iter);
		}
	}

	internal class ExprAND : ExprBoolean
	{
		public ExprAND (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "and"; }}
		public override object Evaluate (BaseIterator iter)
		{
			if (!_left.EvaluateBoolean (iter))
				return false;
			return _right.EvaluateBoolean (iter);
		}
	}

	internal abstract class EqualityExpr : ExprBoolean
	{
		public EqualityExpr (Expression left, Expression right) : base (left, right) {}
		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			XPathResultType typeL = _left.GetReturnType (iter);
			XPathResultType typeR = _right.GetReturnType (iter);
			if (typeL == XPathResultType.NodeSet || typeR == XPathResultType.NodeSet)
			{
				Expression left, right;
				if (typeL != XPathResultType.NodeSet)
				{
					left = _right;
					right = _left;
					XPathResultType typeTmp = typeL;
					typeL = typeR;
					typeR = typeTmp;
				}
				else
				{
					left = _left;
					right = _right;
				}
				if (typeR == XPathResultType.Boolean)
				{
					bool fL = left.EvaluateBoolean (iter);
					bool fR = right.EvaluateBoolean (iter);
					return Compare (Convert.ToDouble (fL), Convert.ToDouble (fR));
				}
				else
				{
					BaseIterator iterL = left.EvaluateNodeSet (iter);
					if (typeR == XPathResultType.Number)
					{
						double dR = right.EvaluateNumber (iter);
						while (iterL.MoveNext ())
							if (Compare (XPathFunctions.ToNumber (iterL.Current.Value), dR))
								return true;
					}
					else if (typeR == XPathResultType.String)
					{
						string strR = right.EvaluateString (iter);
						while (iterL.MoveNext ())
							if (Compare (iterL.Current.Value, strR))
								return true;
					}
					else if (typeR == XPathResultType.NodeSet)
					{
						BaseIterator iterR = right.EvaluateNodeSet (iter);
						ArrayList rgNodesL = new ArrayList ();
						while (iterL.MoveNext ())
							rgNodesL.Add (XPathFunctions.ToString (iterL.Current.Value));
						while (iterR.MoveNext ())
						{
							string strR = XPathFunctions.ToString (iterR.Current.Value);
							foreach (string strL in rgNodesL)
								if (Compare (strL, strR))
									return true;
						}
					}
					return false;
				}
			}
			else if (typeL == XPathResultType.Boolean || typeR == XPathResultType.Boolean)
				return Compare (_left.EvaluateBoolean (iter), _right.EvaluateBoolean (iter));
			else if (typeL == XPathResultType.Number || typeR == XPathResultType.Number)
				return Compare (_left.EvaluateNumber (iter), _right.EvaluateNumber (iter));
			else
				return Compare (_left.EvaluateString (iter), _right.EvaluateString (iter));
		}
		[MonoTODO]
		public abstract bool Compare (object arg1, object arg2);	// TODO: should probably have type-safe methods here
	}
	
	internal class ExprEQ : EqualityExpr
	{
		public ExprEQ (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "="; }}
		public override bool Compare (object arg1, object arg2)
		{
			return arg1.Equals (arg2);
		}
	}

	internal class ExprNE : EqualityExpr
	{
		public ExprNE (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "!="; }}
		public override bool Compare (object arg1, object arg2)
		{
			return !arg1.Equals (arg2);
		}
	}

	internal abstract class RelationalExpr : ExprBoolean
	{
		public RelationalExpr (Expression left, Expression right) : base (left, right) {}
		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			XPathResultType typeL = _left.GetReturnType (iter);
			XPathResultType typeR = _right.GetReturnType (iter);
			if (typeL == XPathResultType.NodeSet || typeR == XPathResultType.NodeSet)
			{
				bool fReverse = false;
				Expression left, right;
				if (typeL != XPathResultType.NodeSet)
				{
					fReverse = true;
					left = _right;
					right = _left;
					XPathResultType typeTmp = typeL;
					typeL = typeR;
					typeR = typeTmp;
				}
				else
				{
					left = _left;
					right = _right;
				}
				if (typeR == XPathResultType.Boolean)
				{
					bool fL = left.EvaluateBoolean (iter);
					bool fR = right.EvaluateBoolean (iter);
					return Compare (Convert.ToDouble (fL), Convert.ToDouble (fR), fReverse);
				}
				else
				{
					BaseIterator iterL = left.EvaluateNodeSet (iter);
					if (typeR == XPathResultType.Number || typeR == XPathResultType.String)
					{
						double dR = right.EvaluateNumber (iter);
						while (iterL.MoveNext ())
							if (Compare (XPathFunctions.ToNumber (iterL.Current.Value), dR, fReverse))
								return true;
					}
					else if (typeR == XPathResultType.NodeSet)
					{
						BaseIterator iterR = right.EvaluateNodeSet (iter);
						ArrayList rgNodesL = new ArrayList ();
						while (iterL.MoveNext ())
							rgNodesL.Add (XPathFunctions.ToNumber (iterL.Current.Value));
						while (iterR.MoveNext ())
						{
							double numR = XPathFunctions.ToNumber (iterR.Current.Value);
							foreach (double numL in rgNodesL)
								if (Compare (numL, numR))
									return true;
						}
					}
					return false;
				}
			}
			else
				return Compare (_left.EvaluateNumber (iter), _right.EvaluateNumber (iter));
		}
		public abstract bool Compare (double arg1, double arg2);
		public bool Compare (double arg1, double arg2, bool fReverse)
		{
			if (fReverse)
				return Compare (arg2, arg1);
			else
				return Compare (arg1, arg2);
		}
	}

	internal class ExprGT : RelationalExpr
	{
		public ExprGT (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return ">"; }}
		public override bool Compare (double arg1, double arg2)
		{
			return arg1 > arg2;
		}
	}

	internal class ExprGE : RelationalExpr
	{
		public ExprGE (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return ">="; }}
		public override bool Compare (double arg1, double arg2)
		{
			return arg1 >= arg2;
		}
	}

	internal class ExprLT : RelationalExpr
	{
		public ExprLT (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "<"; }}
		public override bool Compare (double arg1, double arg2)
		{
			return arg1 < arg2;
		}
	}

	internal class ExprLE : RelationalExpr
	{
		public ExprLE (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "<="; }}
		public override bool Compare (double arg1, double arg2)
		{
			return arg1 <= arg2;
		}
	}

	internal abstract class ExprNumeric : ExprBinary
	{
		public ExprNumeric (Expression left, Expression right) : base (left, right) {}
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
	}

	internal class ExprPLUS : ExprNumeric
	{
		public ExprPLUS (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "+"; }}
		public override object Evaluate (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) + _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMINUS : ExprNumeric
	{
		public ExprMINUS (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "-"; }}
		public override object Evaluate (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) - _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMULT : ExprNumeric
	{
		public ExprMULT (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "*"; }}
		public override object Evaluate (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) * _right.EvaluateNumber (iter);
		}
	}

	internal class ExprDIV : ExprNumeric
	{
		public ExprDIV (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return " div "; }}
		public override object Evaluate (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) / _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMOD : ExprNumeric
	{
		public ExprMOD (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "%"; }}
		[MonoTODO]
		public override object Evaluate (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) % _right.EvaluateNumber (iter);	// TODO: spec?
		}
	}

	internal class ExprNEG : Expression
	{
		Expression _expr;
		public ExprNEG (Expression expr)
		{
			_expr = expr;
		}
		public override String ToString () { return "- " + _expr.ToString (); }
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override object Evaluate (BaseIterator iter)
		{
			return - _expr.EvaluateNumber (iter);
		}
	}


	internal abstract class NodeSet : Expression
	{
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
	}
	internal class ExprUNION : NodeSet
	{
		protected Expression _left, _right;
		public ExprUNION (Expression left, Expression right)
		{
			_left = left;
			_right = right;
		}
		public override String ToString () { return _left.ToString ()+ " | " + _right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterLeft = _left.EvaluateNodeSet (iter);
			BaseIterator iterRight = _right.EvaluateNodeSet (iter);
			return new UnionIterator (iter, iterLeft, iterRight);
		}
	}

	internal class ExprSLASH : NodeSet
	{
		protected Expression _left, _right;
		public ExprSLASH (Expression left, NodeSet right)
		{
			_left = left;
			_right = right;
		}
		public override String ToString () { return _left.ToString ()+ "/" + _right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterLeft = _left.EvaluateNodeSet (iter);
			return new SlashIterator (iterLeft, _right);
		}
	}

	internal class ExprRoot : NodeSet
	{
		public override String ToString () { return ""; }
		public override object Evaluate (BaseIterator iter)
		{
			XPathNavigator navRoot = iter.Current.Clone ();
			navRoot.MoveToRoot ();
			return new SelfIterator (navRoot, iter.NamespaceManager);
		}
	}

	internal enum Axes
	{
		Ancestor,
		AncestorOrSelf,
		Attribute,
		Child,
		Descendant,
		DescendantOrSelf,
		Following,
		FollowingSibling,
		Namespace,
		Parent,
		Preceding,
		PrecedingSibling,
		Self,
	}

	internal class AxisSpecifier
	{
		protected Axes _axis;
		public AxisSpecifier (Axes axis)
		{
			_axis = axis;
		}
		public XPathNodeType NodeType
		{
			get
			{
				switch (_axis)
				{
					case Axes.Namespace:
						return XPathNodeType.Namespace;
					case Axes.Attribute:
						return XPathNodeType.Attribute;
					default:
						return XPathNodeType.Element;
				}
			}
		}
		public override string ToString ()
		{
			switch (_axis)
			{
				case Axes.Ancestor:
					return "ancestor";
				case Axes.AncestorOrSelf:
					return "ancestor-or-self";
				case Axes.Attribute:
					return "attribute";
				case Axes.Child:
					return "child";
				case Axes.Descendant:
					return "descendant";
				case Axes.DescendantOrSelf:
					return "descendant-or-self";
				case Axes.Following:
					return "following";
				case Axes.FollowingSibling:
					return "following-sibling";
				case Axes.Namespace:
					return "namespace";
				case Axes.Parent:
					return "parent";
				case Axes.Preceding:
					return "preceeding";
				case Axes.PrecedingSibling:
					return "preceeding-sibling";
				case Axes.Self:
					return "self";
				default:
					throw new IndexOutOfRangeException ();
			}
		}
		public Axes Axis { get { return _axis; }}
		public virtual BaseIterator Evaluate (BaseIterator iter)
		{
			switch (_axis)
			{
				case Axes.Ancestor:
					return new AncestorIterator (iter);
				case Axes.AncestorOrSelf:
					return new AncestorOrSelfIterator (iter);
				case Axes.Attribute:
					return new AttributeIterator (iter);
				case Axes.Child:
					return new ChildIterator (iter);
				case Axes.Descendant:
					return new DescendantIterator (iter);
				case Axes.DescendantOrSelf:
					return new DescendantOrSelfIterator (iter);
				case Axes.Following:
					return new FollowingIterator (iter);
				case Axes.FollowingSibling:
					return new FollowingSiblingIterator (iter);
				case Axes.Namespace:
					return new NamespaceIterator (iter);
				case Axes.Parent:
					return new ParentIterator (iter);
				case Axes.Preceding:
					return new PrecedingIterator (iter);
				case Axes.PrecedingSibling:
					return new PrecedingSiblingIterator (iter);
				case Axes.Self:
					return new SelfIterator (iter);
				default:
					throw new IndexOutOfRangeException ();
			}
		}
	}

	internal abstract class NodeTest
	{
		protected AxisSpecifier _axis;
		public NodeTest (AxisSpecifier axis)
		{
			_axis = axis;
		}
		public NodeTest (Axes axis)
		{
			_axis = new AxisSpecifier (axis);
		}
		public abstract bool Match (XmlNamespaceManager nsm, XPathNavigator nav);
		public AxisSpecifier Axis { get { return _axis; }}
		public virtual BaseIterator Evaluate (BaseIterator iter)
		{
			BaseIterator iterAxis = _axis.Evaluate (iter);
			return new AxisIterator (iterAxis, this);
		}
	}

	internal class NodeTypeTest : NodeTest
	{
		protected XPathNodeType _type;
		protected String _param;
		public NodeTypeTest (Axes axis) : base (axis)
		{
			_type = _axis.NodeType;
		}
		public NodeTypeTest (Axes axis, XPathNodeType type) : base (axis)
		{
			_type = type;
		}
		[MonoTODO]
		public NodeTypeTest (Axes axis, XPathNodeType type, String param) : base (axis)
		{
			_type = type;
			_param = param;
			if (param != null && type != XPathNodeType.ProcessingInstruction)
				throw new XPathException ("No argument allowed for "+ToString (_type)+"() test");	// TODO: better description
		}
		public override String ToString ()
		{
			String strType = ToString (_type);
			if (_type == XPathNodeType.ProcessingInstruction && _param != null)
				strType += "('" + _param + "')";
			else
				strType += "()";

			return _axis.ToString () + "::" + strType;
		}
		private static String ToString (XPathNodeType type)
		{
			switch (type)
			{
				case XPathNodeType.Comment:
					return "comment";
				case XPathNodeType.Text:
					return "text";
				case XPathNodeType.ProcessingInstruction:
					return "processing-instruction";
				case XPathNodeType.All:
				case XPathNodeType.Attribute:
				case XPathNodeType.Element:
				case XPathNodeType.Namespace:
					return "node";
				default:
					throw new NotImplementedException ();
			}
		}
		public override bool Match (XmlNamespaceManager nsm, XPathNavigator nav)
		{
			XPathNodeType nodeType = nav.NodeType;
			switch (_type)
			{
				case XPathNodeType.All:
					return true;

				case XPathNodeType.ProcessingInstruction:
					if (nodeType != XPathNodeType.ProcessingInstruction)
						return false;
					if (_param != null && nav.Name != _param)
						return false;
					return true;
				
				default:
					return _type == nodeType;
			}
		}
	}

	internal class NodeNameTest : NodeTest
	{
		protected XmlQualifiedName _name;
		public NodeNameTest (Axes axis, XmlQualifiedName name) : base (axis)
		{
			_name = name;
		}
		public override String ToString () { return _axis.ToString () + "::" + _name.ToString (); }
		[MonoTODO]
		public override bool Match (XmlNamespaceManager nsm, XPathNavigator nav)
		{
			// must be the correct node type
			if (nav.NodeType != _axis.NodeType)
				return false;

			if (_name.Name != "")
			{
				// test the local part of the name first
				if (_name.Name != nav.LocalName)
					return false;
			}

			// get the prefix for the given name
			String strURI1 = "";
			if (nsm != null && _name.Namespace != "")
			{
				strURI1 = nsm.LookupNamespace (_name.Namespace);	// TODO: check to see if this returns null or ""
				if (strURI1 == null)
					throw new XPathException ("Invalid namespace prefix: "+_name.Namespace);
			}

			string strURI = nav.NamespaceURI;
			if (strURI == null && strURI1 == "")	// TODO: remove when bug #26855 fixed
				return true;

			// test the prefixes
			return strURI1 == nav.NamespaceURI;
		}
	}

	internal class ExprStep : NodeSet
	{
		protected NodeTest _test;
		protected Expression [] _preds;
		public ExprStep (NodeTest test, ExprPredicates preds)
		{
			_test = test;
			if (preds != null)
				_preds = preds.GetPredicates ();
		}
		public ExprStep (NodeTest test)
		{
			_test = test;
		}
		public override String ToString ()
		{
			String strExpr = _test.ToString ();
			if (_preds != null)
				foreach (Expression pred in _preds)
					strExpr += '[' + pred.ToString () + ']';
			return strExpr;
		}
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterStep = _test.Evaluate (iter);
			if (_preds == null)
				return iterStep;
			return new PredicateIterator (iterStep, _preds);
		}
	}


	internal class ExprPredicates
	{
		protected Expression _pred;
		protected ExprPredicates _tail;
		public ExprPredicates (Expression pred, ExprPredicates tail)
		{
			_pred = pred;
			_tail = tail;
		}
		public ExprPredicates (Expression pred)
		{
			_pred = pred;
		}
		public Expression [] GetPredicates ()
		{
			ArrayList lstPreds = new ArrayList ();
			ExprPredicates curr = this;
			while (curr != null)
			{
				lstPreds.Add (curr._pred);
				curr = curr._tail;
			}
			return (Expression []) lstPreds.ToArray (typeof (Expression));
		}
	}

	internal class ExprFilter : Expression
	{
		protected Expression _expr;
		protected Expression [] _preds = new Expression [1];
		public ExprFilter (Expression expr, Expression pred)
		{
			_expr = expr;
			_preds [0] = pred;
		}
		public override String ToString () { return "(" + _expr.ToString () + ")[" + _preds [0].ToString () + "]"; }
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterExpr = _expr.EvaluateNodeSet (iter);
			return new PredicateIterator (iterExpr, _preds);
		}
	}

	internal class ExprNumber : Expression
	{
		protected double _value;
		public ExprNumber (double value)
		{
			_value = value;
		}
		public override String ToString () { return _value.ToString (); }
		public override XPathResultType ReturnType { get { return XPathResultType.Number; }}
		public override object Evaluate (BaseIterator iter)
		{
			return _value;
		}
	}

	internal class ExprLiteral : Expression
	{
		protected String _value;
		public ExprLiteral (String value)
		{
			_value = value;
		}
		public override String ToString () { return "'" + _value + "'"; }
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			return _value;
		}
	}

	internal class ExprVariable : Expression
	{
		protected XmlQualifiedName _name;
		public ExprVariable (XmlQualifiedName name)
		{
			_name = name;
		}
		public override String ToString () { return "$" + _name.ToString (); }
		public override XPathResultType ReturnType { get { return XPathResultType.Any; }}
		public override XPathResultType GetReturnType (BaseIterator iter)
		{
			IXsltContextVariable var = null;
			XsltContext context = iter.NamespaceManager as XsltContext;
			if (context != null)
				var = context.ResolveVariable (_name.Namespace, _name.Name);
			if (var == null)
				throw new XPathException ("variable "+_name.ToString ()+" not found");
			return var.VariableType;
		}
		public override object Evaluate (BaseIterator iter)
		{
			IXsltContextVariable var = null;
			XsltContext context = iter.NamespaceManager as XsltContext;
			if (context != null)
				var = context.ResolveVariable (_name.Namespace, _name.Name);
			if (var == null)
				throw new XPathException ("variable "+_name.ToString ()+" not found");
			object objResult = var.Evaluate (context);
			XPathNodeIterator iterResult = objResult as XPathNodeIterator;
			if (iterResult != null)
				return iterResult.Clone ();
			return objResult;
		}
	}

	internal class FunctionArguments
	{
		protected Expression _arg;
		protected FunctionArguments _tail;
		public FunctionArguments (Expression arg, FunctionArguments tail)
		{
			_arg = arg;
			_tail = tail;
		}
		public Expression Arg
		{
			get { return _arg; }
		}
		public FunctionArguments Tail
		{
			get { return _tail; }
		}
	}

	internal class ExprFunctionCall : Expression
	{
		protected readonly XmlQualifiedName _name;
		protected readonly ArrayList _args = new ArrayList ();
		public ExprFunctionCall (XmlQualifiedName name, FunctionArguments args)
		{
			_name = name;
			while (args != null)
			{
				_args.Add (args.Arg);
				args = args.Tail;
			}
		}
		public override String ToString ()
		{
			String strArgs = "";
			foreach (Expression arg in _args)
			{
				if (strArgs != "")
					strArgs += ", ";
				strArgs += arg.ToString ();
			}
			return _name.ToString () + '(' + strArgs + ')';
		}
		public override XPathResultType ReturnType { get { return XPathResultType.Any; }}
		public override XPathResultType GetReturnType (BaseIterator iter)
		{
			IXsltContextFunction func = null;
			XsltContext context = iter.NamespaceManager as XsltContext;
			if (context != null)
				func = context.ResolveFunction (_name.Namespace, _name.Name, GetArgTypes (iter));
			if (func == null)
				func = DefaultContext.ResolveFunction (_name.Namespace, _name.Name, GetArgTypes (iter));
			if (func == null)
				throw new XPathException ("function "+_name.ToString ()+" not found");
			return func.ReturnType;
		}
		private XPathResultType [] GetArgTypes (BaseIterator iter)
		{
			// TODO: can we cache these? what if the types depend on the nsm?
			XPathResultType [] rgArgs = new XPathResultType [_args.Count];
			for (int iArg = 0; iArg < _args.Count; iArg++)
				rgArgs [iArg] = ((Expression) _args [iArg]).GetReturnType (iter);
			return rgArgs;
		}
		public override object Evaluate (BaseIterator iter)
		{
			//special-case the 'last' and 'position' functions
			if (_args.Count == 0 && _name.Namespace == "")
			{
				if (_name.Name == "last")
				{
					return (double) iter.Count;
				}
				else if (_name.Name == "position")
				{
					return (double) iter.CurrentPosition;
				}
			}

			XPathResultType [] rgTypes = GetArgTypes (iter);
			IXsltContextFunction func = null;
			XsltContext context = iter.NamespaceManager as XsltContext;
			if (context != null)
				func = context.ResolveFunction (_name.Namespace, _name.Name, rgTypes);
			if (func == null)
			{
				context = DefaultContext;
				func = context.ResolveFunction (_name.Namespace, _name.Name, rgTypes);
			}
			if (func == null)
				throw new XPathException ("function "+_name.ToString ()+" not found");

			object [] rgArgs = new object [_args.Count];
			if (func.Maxargs != 0)
			{
				XPathResultType [] rgFuncTypes = func.ArgTypes;
				for (int iArg = 0; iArg < _args.Count; iArg ++)
				{
					XPathResultType typeArg;
					if (rgFuncTypes == null)
						typeArg = XPathResultType.Any;
					else if (iArg < rgFuncTypes.Length)
						typeArg = rgFuncTypes [iArg];
					else
						typeArg = rgFuncTypes [rgFuncTypes.Length - 1];

					Expression arg = (Expression) _args [iArg];
					object result = arg.EvaluateAs (iter, typeArg);
					rgArgs [iArg] = result;
				}
			}
			return func.Invoke (context, rgArgs, iter.Current);
		}
	}
}
