//
// System.Xml.XPath.XPathExpression support classes
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
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
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Globalization;
using Mono.Xml.XPath;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

namespace System.Xml.XPath
{
#if XPATH_DEBUG
	internal class CompiledExpression : Test.Xml.XPath.XPathExpression
#else
	internal class CompiledExpression : XPathExpression
#endif
	{
		protected NSResolver _nsm;
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
		
		public Expression ExpressionNode { get { return _expr; }}

		public override void SetContext (XmlNamespaceManager nsManager)
		{
			_nsm = nsManager;
		}

#if NET_2_0
		public override void SetContext (IXmlNamespaceResolver nsResolver)
		{
			_nsm = nsResolver;
		}
#endif

		internal NSResolver NamespaceManager { get { return _nsm; } }
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
#if false
			try
			{
#endif
				BaseIterator iterResults = (BaseIterator) _expr.EvaluateNodeSet (iter);
				if (_sorters != null)
					return _sorters.Sort (iterResults);
				return iterResults;
#if false
			}
			catch (XPathException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new XPathException ("Error during evaluation", e);
			}
#endif
		}
		public double EvaluateNumber (BaseIterator iter)
		{
#if true
			return _expr.EvaluateNumber (iter);
#else
			try
			{
				return _expr.EvaluateNumber (iter);
			}
			catch (XPathException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new XPathException ("Error during evaluation", e);
			}
#endif
		}
		public string EvaluateString (BaseIterator iter)
		{
#if true
			return _expr.EvaluateString (iter);
#else
			try
			{
				return _expr.EvaluateString (iter);
			}
			catch (XPathException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new XPathException ("Error during evaluation", e);
			}
#endif
		}
		public bool EvaluateBoolean (BaseIterator iter)
		{
#if true
			return _expr.EvaluateBoolean (iter);
#else
			try
			{
				return _expr.EvaluateBoolean (iter);
			}
			catch (XPathException)
			{
				throw;
			}
			catch (Exception e)
			{
				throw new XPathException ("Error during evaluation", e);
			}
#endif
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
				return new ListIterator (iter, rgResults, false);
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
				switch (elt1.Navigator.ComparePosition (elt2.Navigator)) {
				case XmlNodeOrder.Same:
					return 0;
				case XmlNodeOrder.After:
					return 1;
				default:
					return -1;
				}
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
						return new XPathParser ().Compile ((string)expr);
					
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
		public Expression ()
		{
		}
		public abstract XPathResultType ReturnType { get; }
		public virtual XPathResultType GetReturnType (BaseIterator iter) { return ReturnType; }
		public abstract object Evaluate (BaseIterator iter);

		public virtual BaseIterator EvaluateNodeSet (BaseIterator iter)
		{
			XPathResultType type = GetReturnType (iter);
			switch (type) {
			case XPathResultType.NodeSet:
			case XPathResultType.Any:
			case XPathResultType.Navigator: // FIXME: It may pass not-allowed use of RTF
				object o = Evaluate (iter);
				BaseIterator iterResult = o as BaseIterator;
				if (iterResult != null)
					return iterResult;
				XPathNavigator nav = o as XPathNavigator;
				if (nav != null)
					iterResult = nav.SelectChildren (XPathNodeType.All) as BaseIterator;
				if (iterResult != null)
					return iterResult;
				break;
			}
			throw new XPathException ("expected nodeset: "+ToString ());
		}

		protected static XPathResultType GetReturnType (object obj)
		{
			if (obj is string)
				return XPathResultType.String;
			if (obj is bool)
				return XPathResultType.Boolean;
			if (obj is XPathNodeIterator)
				return XPathResultType.NodeSet;
			if (obj is double || obj is int)
				return XPathResultType.Number;
			if (obj is XPathNavigator)
				return XPathResultType.Navigator;
			throw new XPathException ("invalid node type: "+obj.GetType ().ToString ());
		}

		internal virtual XPathNodeType EvaluatedNodeType {
			get { return XPathNodeType.All; }
		}

		internal virtual bool NeedAbsoluteMatching {
			get { return false; }
		}

		internal virtual bool IsPositional {
			get { return false; }
		}

		public virtual double EvaluateNumber (BaseIterator iter)
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
					return (double)result;
				case XPathResultType.Boolean:
					return ((bool) result) ? 1.0 : 0.0;
				case XPathResultType.NodeSet:
					return XPathFunctions.ToNumber (EvaluateString (iter));
				case XPathResultType.String:
					return XPathFunctions.ToNumber ((string) result);
				case XPathResultType.Navigator:
					return XPathFunctions.ToNumber (((XPathNavigator) (result)).Value);
				default:
					throw new XPathException ("invalid node type");
			}
		}

		public virtual string EvaluateString (BaseIterator iter)
		{
			object result = Evaluate (iter);
			XPathResultType type = GetReturnType (iter);
			if (type == XPathResultType.Any)
				type = GetReturnType (result);
			switch (type)
			{
				case XPathResultType.Number:
					return (string) XmlConvert.ToString ((double)result);
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
				case XPathResultType.Navigator:
					return ((XPathNavigator) result).Value;
				default:
					throw new XPathException ("invalid node type");
			}
		}

		public virtual bool EvaluateBoolean (BaseIterator iter)
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
					return (num != 0.0 && num != -0.0 && !Double.IsNaN (num));
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
				case XPathResultType.Navigator:
					return ((string) ((XPathNavigator) result).Value).Length != 0;
				default:
					throw new XPathException ("invalid node type");
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

		public virtual bool RequireSorting { get { return false; } }
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

		internal override XPathNodeType EvaluatedNodeType {
			get {
				if (_left.EvaluatedNodeType == _right.EvaluatedNodeType)
					return _left.EvaluatedNodeType;
				else
					return XPathNodeType.All;
			}
		}

		internal override bool IsPositional {
			get { return _left.IsPositional || _right.IsPositional; }
		}
	}

	internal abstract class ExprBoolean : ExprBinary
	{
		public ExprBoolean (Expression left, Expression right) : base (left, right) {}
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		public override object Evaluate (BaseIterator iter)
		{
			return EvaluateBoolean (iter);
		}
		public override double EvaluateNumber (BaseIterator iter)
		{
			return EvaluateBoolean (iter) ? 1 : 0;
		}
		
		public override string EvaluateString (BaseIterator iter)
		{
			return EvaluateBoolean (iter) ? "true" : "false";
		}
	}

	internal class ExprOR : ExprBoolean
	{
		public ExprOR (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "or"; }}
		public override bool EvaluateBoolean (BaseIterator iter)
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
		public override bool EvaluateBoolean (BaseIterator iter)
		{
			if (!_left.EvaluateBoolean (iter))
				return false;
			return _right.EvaluateBoolean (iter);
		}
	}

	internal abstract class EqualityExpr : ExprBoolean
	{
		bool trueVal;
		public EqualityExpr (Expression left, Expression right, bool trueVal) : base (left, right)
		{
			this.trueVal = trueVal;
		}

		[MonoTODO ("Avoid extraneous evaluation")]
		public override bool EvaluateBoolean (BaseIterator iter)
		{
			XPathResultType typeL = _left.GetReturnType (iter);
			XPathResultType typeR = _right.GetReturnType (iter);

			// TODO: avoid double evaluations
			if (typeL == XPathResultType.Any)
				typeL = GetReturnType (_left.Evaluate (iter));
			if (typeR == XPathResultType.Any)
				typeR = GetReturnType (_right.Evaluate (iter));

			// Regard RTF as nodeset
			if (typeL == XPathResultType.Navigator)
				typeL = XPathResultType.NodeSet;
			if (typeR == XPathResultType.Navigator)
				typeR = XPathResultType.NodeSet;

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
					return left.EvaluateBoolean (iter) == right.EvaluateBoolean (iter) == trueVal;
				}
				else
				{
					BaseIterator iterL = left.EvaluateNodeSet (iter);
					if (typeR == XPathResultType.Number)
					{
						double dR = right.EvaluateNumber (iter);
						while (iterL.MoveNext ())
							if (XPathFunctions.ToNumber (iterL.Current.Value) == dR == trueVal)
								return true;
					}
					else if (typeR == XPathResultType.String)
					{
						string strR = right.EvaluateString (iter);
						while (iterL.MoveNext ())
							if (iterL.Current.Value == strR == trueVal)
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
							for (int l = 0; l < rgNodesL.Count; l++)
								if ((strR == (string) rgNodesL [l]) == trueVal)
									return true;
						}
					}
					return false;
				}
			}
			else if (typeL == XPathResultType.Boolean || typeR == XPathResultType.Boolean)
				return _left.EvaluateBoolean (iter) == _right.EvaluateBoolean (iter) == trueVal;
			else if (typeL == XPathResultType.Number || typeR == XPathResultType.Number)
				return _left.EvaluateNumber (iter) == _right.EvaluateNumber (iter) == trueVal;
			else
				return _left.EvaluateString (iter) == _right.EvaluateString (iter) == trueVal;
		}
	}
	
	internal class ExprEQ : EqualityExpr
	{
		public ExprEQ (Expression left, Expression right) : base (left, right, true) {}
		protected override String Operator { get { return "="; }}
	}

	internal class ExprNE : EqualityExpr
	{
		public ExprNE (Expression left, Expression right) : base (left, right, false) {}
		protected override String Operator { get { return "!="; }}
	}

	internal abstract class RelationalExpr : ExprBoolean
	{
		public RelationalExpr (Expression left, Expression right) : base (left, right) {}
		[MonoTODO ("Avoid extraneous evaluation.")]
		public override bool EvaluateBoolean (BaseIterator iter)
		{
			XPathResultType typeL = _left.GetReturnType (iter);
			XPathResultType typeR = _right.GetReturnType (iter);

			if (typeL == XPathResultType.Any)
				typeL = GetReturnType (_left.Evaluate (iter));
			if (typeR == XPathResultType.Any)
				typeR = GetReturnType (_right.Evaluate (iter));

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
							for (int l = 0; l < rgNodesL.Count; l++)
								if (Compare (numR, (double) rgNodesL [l]))
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
		
		public override object Evaluate (BaseIterator iter)
		{
			return EvaluateNumber (iter);
		}
	}

	internal class ExprPLUS : ExprNumeric
	{
		public ExprPLUS (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "+"; }}
		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) + _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMINUS : ExprNumeric
	{
		public ExprMINUS (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "-"; }}
		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) - _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMULT : ExprNumeric
	{
		public ExprMULT (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "*"; }}
		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) * _right.EvaluateNumber (iter);
		}
	}

	internal class ExprDIV : ExprNumeric
	{
		public ExprDIV (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return " div "; }}
		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) / _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMOD : ExprNumeric
	{
		public ExprMOD (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "%"; }}

		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) % _right.EvaluateNumber (iter);
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
		
		public override double EvaluateNumber (BaseIterator iter)
		{
			return - _expr.EvaluateNumber (iter);
		}

		internal override bool IsPositional {
			get { return _expr.IsPositional; }
		}
	}


	internal abstract class NodeSet : Expression
	{
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
	}

	internal class ExprUNION : NodeSet
	{
		public readonly Expression left, right;
		public ExprUNION (Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}
		public override String ToString () { return left.ToString ()+ " | " + right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterLeft = left.EvaluateNodeSet (iter);
			BaseIterator iterRight = right.EvaluateNodeSet (iter);
			return new UnionIterator (iter, iterLeft, iterRight);
		}

		internal override bool NeedAbsoluteMatching {
			get { return left.NeedAbsoluteMatching || right.NeedAbsoluteMatching; }
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return left.EvaluatedNodeType == right.EvaluatedNodeType ? left.EvaluatedNodeType : XPathNodeType.All; }
		}

		internal override bool IsPositional {
			get { return left.IsPositional || right.IsPositional; }
		}
	}

	internal class ExprSLASH : NodeSet
	{
		public readonly Expression left;
		public readonly NodeSet right;
		public ExprSLASH (Expression left, NodeSet right)
		{
			this.left = left;
			this.right = right;
		}
		public override String ToString () { return left.ToString ()+ "/" + right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterLeft = left.EvaluateNodeSet (iter);
			return new SlashIterator (iterLeft, right);
		}

		public override bool RequireSorting { get { return left.RequireSorting || right.RequireSorting; } }

		internal override bool NeedAbsoluteMatching {
			get { return true; }
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return right.EvaluatedNodeType; }
		}

		internal override bool IsPositional {
			get { return left.IsPositional || right.IsPositional; }
		}
	}
	
	internal class ExprSLASH2 : NodeSet {
		public readonly Expression left;
		public readonly NodeSet right;
			
		static NodeTest DescendantOrSelfStar = new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All);

		public ExprSLASH2 (Expression left, NodeSet right)
		{
			this.left = left;
			this.right = right;
		}
		public override String ToString () { return left.ToString ()+ "//" + right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			return new SlashIterator (
				new SlashIterator (
					left.EvaluateNodeSet (iter),
					DescendantOrSelfStar
				),
				right
			);
		}

		public override bool RequireSorting { get { return left.RequireSorting || right.RequireSorting; } }

		internal override bool NeedAbsoluteMatching {
			get { return true; }
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return right.EvaluatedNodeType; }
		}

		internal override bool IsPositional {
			get { return left.IsPositional || right.IsPositional; }
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

		internal override bool NeedAbsoluteMatching {
			get { return false; }
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return XPathNodeType.Root; }
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
		public virtual SimpleIterator Evaluate (BaseIterator iter)
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

	internal abstract class NodeTest : NodeSet
	{
		protected AxisSpecifier _axis;
		public NodeTest (Axes axis)
		{
			_axis = new AxisSpecifier (axis);
		}
		public abstract bool Match (NSResolver nsm, XPathNavigator nav);
		public AxisSpecifier Axis { get { return _axis; }}
		public override object Evaluate (BaseIterator iter)
		{
			SimpleIterator iterAxis = _axis.Evaluate (iter);
			return new AxisIterator (iterAxis, this);
		}
		
		public abstract void GetInfo (out string name, out string ns, out XPathNodeType nodetype, NSResolver nsm);

		public override bool RequireSorting {
			get {
				switch (_axis.Axis) {
				case Axes.Ancestor:
				case Axes.AncestorOrSelf:
				case Axes.Preceding:
				case Axes.PrecedingSibling:
				case Axes.Namespace:
					return true;
				default:
					return false;
				}
			}

		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return _axis.NodeType; }
		}
	}

	internal class NodeTypeTest : NodeTest
	{
		public readonly XPathNodeType type;
		protected String _param;
		public NodeTypeTest (Axes axis) : base (axis)
		{
			this.type = _axis.NodeType;
		}
		public NodeTypeTest (Axes axis, XPathNodeType type) : base (axis)
		{
			this.type = type;
		}
		[MonoTODO ("Better description.")]
		public NodeTypeTest (Axes axis, XPathNodeType type, String param) : base (axis)
		{
			this.type = type;
			_param = param;
			if (param != null && type != XPathNodeType.ProcessingInstruction)
				throw new XPathException ("No argument allowed for "+ToString (type)+"() test");	// TODO: better description
		}

		public override String ToString ()
		{
			String strType = ToString (type);
			if (type == XPathNodeType.ProcessingInstruction && _param != null)
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
					return "node-type [" + type.ToString () + "]";
			}
		}

		public override bool Match (NSResolver nsm, XPathNavigator nav)
		{
			XPathNodeType nodeType = nav.NodeType;
			switch (type)
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
					return type == nodeType;
			}
		}
		
		public override void GetInfo (out string name, out string ns, out XPathNodeType nodetype, NSResolver nsm)
		{
			name = _param;
			ns = null;
			nodetype = type;
		}
	}

	internal class NodeNameTest : NodeTest
	{
		protected XmlQualifiedName _name;
		protected readonly bool resolvedName = false;
		public NodeNameTest (Axes axis, XmlQualifiedName name, IStaticXsltContext ctx) : base (axis)
		{
			if (ctx != null) {
				name = ctx.LookupQName (name.ToString ());
				resolvedName = true;
			}
			_name = name;
		}
		
		public NodeNameTest (Axes axis, XmlQualifiedName name, bool resolvedName) : base (axis)
		{
			_name = name;
			resolvedName = resolvedName;
		}
		public override String ToString () { return _axis.ToString () + "::" + _name.ToString (); }
		
		public XmlQualifiedName Name { get { return _name; } }

		public override bool Match (NSResolver nsm, XPathNavigator nav)
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
				if (resolvedName)
					strURI1 = _name.Namespace;
				else
					strURI1 = nsm.LookupNamespace (_name.Namespace, false);
				if (strURI1 == null)
					throw new XPathException ("Invalid namespace prefix: "+_name.Namespace);
			}

			string strURI = nav.NamespaceURI;

			// test the prefixes
			return strURI1 == nav.NamespaceURI;
		}
		
		public override void GetInfo (out string name, out string ns, out XPathNodeType nodetype, NSResolver nsm)
		{
			// must be the correct node type
			nodetype = _axis.NodeType;
			
			if (_name.Name != "")
				name = _name.Name;
			else
				name = null;
			ns = "";
			if (nsm != null && _name.Namespace != "") {
				if (resolvedName)
					ns = _name.Namespace;
				else
					ns = nsm.LookupNamespace (_name.Namespace, false);	// TODO: check to see if this returns null or ""
				if (ns == null)
					throw new XPathException ("Invalid namespace prefix: "+_name.Namespace);
			}
		}
	}

	internal class ExprFilter : NodeSet
	{
		public readonly Expression expr, pred;
		
		public ExprFilter (Expression expr, Expression pred)
		{
			this.expr = expr;
			this.pred = pred;
		}
		
		internal Expression LeftHandSide {get{return expr;}}
		public override String ToString () { return "(" + expr.ToString () + ")[" + pred.ToString () + "]"; }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterExpr = expr.EvaluateNodeSet (iter);
			return new PredicateIterator (iterExpr, pred);
		}

		internal override bool NeedAbsoluteMatching {
			get { return expr.NeedAbsoluteMatching; }
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return expr.EvaluatedNodeType; }
		}

		internal override bool IsPositional {
			get {
				if (pred.ReturnType == XPathResultType.Number)
					return true;
				return expr.IsPositional || pred.IsPositional;
			}
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
		
		public override double EvaluateNumber (BaseIterator iter)
		{
			return _value;
		}

		internal override bool IsPositional {
			get { return false; }
		}
	}

	internal class ExprLiteral : Expression
	{
		protected String _value;
		public ExprLiteral (String value)
		{
			_value = value;
		}
		public string Value { get { return _value; } }
		public override String ToString () { return "'" + _value + "'"; }
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			return _value;
		}
		
		public override string EvaluateString (BaseIterator iter)
		{
			return _value;
		}
	}

	internal class ExprVariable : Expression
	{
		protected XmlQualifiedName _name;
		protected bool resolvedName = false;
		public ExprVariable (XmlQualifiedName name, IStaticXsltContext ctx)
		{
			if (ctx != null) {
				name = ctx.LookupQName (name.ToString ());
				resolvedName = true;
			}
			
			_name = name;
		}
		public override String ToString () { return "$" + _name.ToString (); }
		public override XPathResultType ReturnType { get { return XPathResultType.Any; }}
		public override XPathResultType GetReturnType (BaseIterator iter)
		{
			return XPathResultType.Any;
		}
		
		public override object Evaluate (BaseIterator iter)
		{
			IXsltContextVariable var = null;
			
			XsltContext context = iter.NamespaceManager as XsltContext;
			if (context != null) {
				if (resolvedName)
					var = context.ResolveVariable (_name);
				else
					var = context.ResolveVariable (new XmlQualifiedName (_name.Name, _name.Namespace));
			}
			
			if (var == null)
				throw new XPathException ("variable "+_name.ToString ()+" not found");
			object objResult = var.Evaluate (context);
			XPathNodeIterator iterResult = objResult as XPathNodeIterator;
			if (iterResult != null)
				return new WrapperIterator (iterResult.Clone (), iter.NamespaceManager);
			return objResult;
		}
	}

	internal class ExprParens : Expression
	{
		protected Expression _expr;
		public ExprParens (Expression expr)
		{
			_expr = expr;
		}
		public override String ToString () { return "(" + _expr.ToString () + ")"; }
		public override XPathResultType ReturnType { get { return _expr.ReturnType; }}
		public override object Evaluate (BaseIterator iter)
		{
			object o = (_expr.Evaluate (iter));
			BaseIterator predBase = o as BaseIterator;
			if (predBase != null)
				return new ParensIterator (predBase);
			else
				return o;
		}

		internal override bool NeedAbsoluteMatching {
			get { return _expr.NeedAbsoluteMatching; }
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return _expr.EvaluatedNodeType; }
		}

		internal override bool IsPositional {
			get { return _expr.IsPositional; }
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
		
		public void ToArrayList (ArrayList a)
		{
			FunctionArguments cur = this;
			
			do {
				a.Add (cur._arg);
				cur = cur._tail;
			} while (cur != null);
			
		}
	}

	internal class ExprFunctionCall : Expression
	{
		protected readonly XmlQualifiedName _name;
		protected readonly bool resolvedName = false;
		protected readonly ArrayList _args = new ArrayList ();
		public ExprFunctionCall (XmlQualifiedName name, FunctionArguments args, IStaticXsltContext ctx)
		{
			if (ctx != null) {
				name = ctx.LookupQName (name.ToString ());
				resolvedName = true;
			}
			
			_name = name;
			if (args != null)
				args.ToArrayList (_args);
		}
		
		public static Expression Factory (XmlQualifiedName name, FunctionArguments args, IStaticXsltContext ctx)
		{
			if (name.Namespace != null && name.Namespace != "")
				return new ExprFunctionCall (name, args, ctx);
			
			switch (name.Name) {
				case "last": return new XPathFunctionLast (args);
				case "position": return new XPathFunctionPosition (args);
				case "count": return new XPathFunctionCount (args);
				case "id": return new XPathFunctionId (args);
				case "local-name": return new XPathFunctionLocalName (args);
				case "namespace-uri": return new XPathFunctionNamespaceUri (args);
				case "name": return new XPathFunctionName (args);
				case "string": return new XPathFunctionString (args);
				case "concat": return new XPathFunctionConcat (args);
				case "starts-with": return new XPathFunctionStartsWith (args);
				case "contains": return new XPathFunctionContains (args);
				case "substring-before": return new XPathFunctionSubstringBefore (args);
				case "substring-after": return new XPathFunctionSubstringAfter (args);
				case "substring": return new XPathFunctionSubstring (args);
				case "string-length": return new XPathFunctionStringLength (args);
				case "normalize-space": return new XPathFunctionNormalizeSpace (args);
				case "translate": return new XPathFunctionTranslate (args);
				case "boolean": return new XPathFunctionBoolean (args);
				case "not": return new XPathFunctionNot (args);
				case "true": return new XPathFunctionTrue (args);
				case "false": return new XPathFunctionFalse (args);
				case "lang": return new XPathFunctionLang (args);
				case "number": return new XPathFunctionNumber (args);
				case "sum": return new XPathFunctionSum (args);
				case "floor": return new XPathFunctionFloor (args);
				case "ceiling": return new XPathFunctionCeil (args);
				case "round": return new XPathFunctionRound (args);
			}
			return new ExprFunctionCall (name, args, ctx);
		}
		
		public override String ToString ()
		{
			String strArgs = "";
			for (int i = 0; i < _args.Count; i++) {
				Expression arg = (Expression) _args [i];
				if (strArgs != "")
					strArgs += ", ";
				strArgs += arg.ToString ();
			}
			return _name.ToString () + '(' + strArgs + ')';
		}
		public override XPathResultType ReturnType { get { return XPathResultType.Any; }}
		public override XPathResultType GetReturnType (BaseIterator iter)
		{
			return XPathResultType.Any;
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
			XPathResultType [] rgTypes = GetArgTypes (iter);
			IXsltContextFunction func = null;
			XsltContext context = iter.NamespaceManager as XsltContext;
			if (context != null) {
				if (resolvedName)
					func = context.ResolveFunction (_name, rgTypes);
				else
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
