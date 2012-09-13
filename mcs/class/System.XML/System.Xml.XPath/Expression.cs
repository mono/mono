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

using NSResolver = System.Xml.IXmlNamespaceResolver;

namespace System.Xml.XPath
{
	internal static class ExpressionCache
	{
		static readonly Hashtable table_per_ctx = new Hashtable ();
		static object dummy = new object ();
		static object cache_lock = new object ();

		public static XPathExpression Get (string xpath, IStaticXsltContext ctx)
		{
			object ctxkey = ctx != null ? ctx : dummy;

			lock (cache_lock) {
				WeakReference wr = table_per_ctx [ctxkey] as WeakReference;
				if (wr == null)
					return null;
				Hashtable table = wr.Target as Hashtable;
				if (table == null) {
					table_per_ctx [ctxkey] = null;
					return null;
				}

				wr = table [xpath] as WeakReference;
				if (wr != null) {
					XPathExpression e = wr.Target as XPathExpression;
					if (e != null)
						return e;
					table [xpath] = null;
				}
			}
			return null;
		}

		public static void Set (string xpath, IStaticXsltContext ctx, XPathExpression exp)
		{
			object ctxkey = ctx != null ? ctx : dummy;

			Hashtable table = null;
			lock (cache_lock) {
				WeakReference wr = table_per_ctx [ctxkey] as WeakReference;
				if (wr != null && wr.IsAlive)
					table = (Hashtable) wr.Target;
				if (table == null) {
					table = new Hashtable ();
					table_per_ctx [ctxkey] = new WeakReference (table);
				}
				table [xpath] = new WeakReference (exp);
			}
		}
	}

#if XPATH_DEBUG
	internal class CompiledExpression : Test.Xml.XPath.XPathExpression
#else
	internal class CompiledExpression : XPathExpression
#endif
	{
		protected NSResolver _nsm;
		protected Expression _expr;
		XPathSorters _sorters;
		string rawExpression;

		public CompiledExpression (string raw, Expression expr)
		{
			_expr = expr.Optimize ();
			rawExpression = raw;
		}
		private CompiledExpression (CompiledExpression other)
		{
			_nsm = other._nsm;
			_expr = other._expr;
			rawExpression = other.rawExpression;
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

		public override void SetContext (IXmlNamespaceResolver nsResolver)
		{
			_nsm = nsResolver;
		}

		internal NSResolver NamespaceManager { get { return _nsm; } }
		public override String Expression { get { return rawExpression; }}
		public override XPathResultType ReturnType { get { return _expr.ReturnType; }}

		public object Evaluate (BaseIterator iter)
		{
			if (_sorters != null)
				return EvaluateNodeSet (iter);

#if false
			return _expr.Evaluate (iter);
#else
			try {
				return _expr.Evaluate (iter);
			}
			catch (XPathException) {
				throw;
			}
			catch (XsltException) {
				throw;
			}
			catch (Exception e) {
				throw new XPathException ("Error during evaluation", e);
			}
#endif
		}
		public XPathNodeIterator EvaluateNodeSet (BaseIterator iter)
		{
#if false
			try
			{
#endif
				BaseIterator iterResults = _expr.EvaluateNodeSet (iter);
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

	}

	class XPathSortElement
	{
		public XPathNavigator Navigator;
		public object [] Values;
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

		public void CopyFrom (XPathSorter [] sorters)
		{
			_rgSorters.Clear ();
			_rgSorters.AddRange (sorters);
		}

		public BaseIterator Sort (BaseIterator iter)
		{
			ArrayList rgElts = ToSortElementList (iter);
			return Sort (rgElts, iter.NamespaceManager);
		}

		ArrayList ToSortElementList (BaseIterator iter)
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
			return rgElts;
		}

		public BaseIterator Sort (ArrayList rgElts, NSResolver nsm)
		{
			rgElts.Sort (this);
			XPathNavigator [] rgResults = new XPathNavigator [rgElts.Count];
			for (int iResult = 0; iResult < rgElts.Count; ++iResult)
			{
				XPathSortElement elt = (XPathSortElement) rgElts [iResult];
				rgResults [iResult] = elt.Navigator;
			}
			return new ListIterator (rgResults, nsm);
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
				return ((CompiledExpression) expr).ExpressionNode;
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
				// FIXME: We have to set this in
				// reverse order since currently
				// we don't support collation.
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

		public virtual Expression Optimize ()
		{
			return this;
		}

		public virtual bool HasStaticValue {
			get { return false; }
		}

		public virtual object StaticValue {
			get {
				switch (ReturnType) {
				case XPathResultType.String:
					return StaticValueAsString;
				case XPathResultType.Number:
					return StaticValueAsNumber;
				case XPathResultType.Boolean:
					return StaticValueAsBoolean;
				}
				return null;
			}
		}

		public virtual string StaticValueAsString {
			get { return HasStaticValue ? XPathFunctions.ToString (StaticValue) : null; }
		}

		public virtual double StaticValueAsNumber {
			get { return HasStaticValue ? XPathFunctions.ToNumber (StaticValue) : 0; }
		}

		public virtual bool StaticValueAsBoolean {
			get { return HasStaticValue ? XPathFunctions.ToBoolean (StaticValue) : false; }
		}

		public virtual XPathNavigator StaticValueAsNavigator {
			get { return StaticValue as XPathNavigator; }
		}

		public abstract object Evaluate (BaseIterator iter);

		public virtual BaseIterator EvaluateNodeSet (BaseIterator iter)
		{
			XPathResultType type = GetReturnType (iter);
			switch (type) {
			case XPathResultType.NodeSet:
			case XPathResultType.Any:
			case XPathResultType.Navigator: // FIXME: It may pass not-allowed use of RTF
				object o = Evaluate (iter);
				XPathNodeIterator xi = o as XPathNodeIterator;
				BaseIterator iterResult = null;
				if (xi != null) {
					iterResult = xi as BaseIterator;
					if (iterResult == null)
						iterResult = new WrapperIterator (xi, iter.NamespaceManager);
					return iterResult;
				}
				XPathNavigator nav = o as XPathNavigator;
				if (nav != null) {
					XPathNodeIterator xiter = nav.SelectChildren (XPathNodeType.All);
					iterResult = xiter as BaseIterator;
					if (iterResult == null && xiter != null)
						iterResult = new WrapperIterator (xiter, iter.NamespaceManager);
				}
				if (iterResult != null)
					return iterResult;
				if (o == null)
					return new NullIterator (iter);
				type = GetReturnType (o);
				break;
			}
			throw new XPathException (String.Format ("expected nodeset but was {1}: {0}", ToString (), type));
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

		internal virtual bool IsPositional {
			get { return false; }
		}

		// For "peer and subtree" optimization. see:
		// http://idealliance.org/papers/dx_xmle04/papers/02-03-02/02-03-02.html
		internal virtual bool Peer {
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

			switch (type) {
			case XPathResultType.Number:
				if (result is double)
					return (double)result;
				else if (result is IConvertible)
					return ((IConvertible) result).ToDouble (CultureInfo.InvariantCulture);
				else
					return (double) result; // most likely invalid cast
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
			switch (type) {
			case XPathResultType.Number:
				double d = (double) result;
				return XPathFunctions.ToString (d);
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
			switch (type) {
			case XPathResultType.Number:
				double num = Convert.ToDouble (result);
				return (num != 0.0 && num != -0.0 && !Double.IsNaN (num));
			case XPathResultType.Boolean:
				return (bool) result;
			case XPathResultType.String:
				return ((string) result).Length != 0;
			case XPathResultType.NodeSet:
				BaseIterator iterResult = (BaseIterator) result;
				return (iterResult != null && iterResult.MoveNext ());
			case XPathResultType.Navigator:
				return (((XPathNavigator) result).HasChildren);
			default:
				throw new XPathException ("invalid node type");
			}
		}

		public object EvaluateAs (BaseIterator iter, XPathResultType type)
		{
			switch (type) {
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

		public override Expression Optimize ()
		{
			_left = _left.Optimize ();
			_right = _right.Optimize ();
			return this;
		}

		public override bool HasStaticValue {
			get { return _left.HasStaticValue && _right.HasStaticValue; }
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

		internal override bool Peer {
			get { return _left.Peer && _right.Peer; }
		}
	}

	internal abstract class ExprBoolean : ExprBinary
	{
		public ExprBoolean (Expression left, Expression right) : base (left, right) {}

		public override Expression Optimize ()
		{
			base.Optimize ();
			if (!HasStaticValue)
				return this;
			else if (StaticValueAsBoolean)
				return new XPathFunctionTrue (null);
			else
				return new XPathFunctionFalse (null);
		}

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

		public override bool StaticValueAsBoolean {
			get { return HasStaticValue ? _left.StaticValueAsBoolean || _right.StaticValueAsBoolean : false; }
		}

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

		public override bool StaticValueAsBoolean {
			get { return HasStaticValue ? _left.StaticValueAsBoolean && _right.StaticValueAsBoolean : false; }
		}

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

		public override bool StaticValueAsBoolean {
			get {
				if (!HasStaticValue)
					return false;
				if ((_left.ReturnType == XPathResultType.Navigator || _right.ReturnType == XPathResultType.Navigator) && _left.ReturnType == _right.ReturnType)
					return (_left.StaticValueAsNavigator.IsSamePosition (
					_right.StaticValueAsNavigator))
					 == trueVal;
				if (_left.ReturnType == XPathResultType.Boolean | _right.ReturnType == XPathResultType.Boolean)
					return (_left.StaticValueAsBoolean == _right.StaticValueAsBoolean) == trueVal;
				if (_left.ReturnType == XPathResultType.Number | _right.ReturnType == XPathResultType.Number)
					return (_left.StaticValueAsNumber == _right.StaticValueAsNumber) == trueVal;
				if (_left.ReturnType == XPathResultType.String | _right.ReturnType == XPathResultType.String)
					return (_left.StaticValueAsString == _right.StaticValueAsString) == trueVal;
				return _left.StaticValue == _right.StaticValue == trueVal;
			}
		}

		// FIXME: Avoid extraneous evaluation
		public override bool EvaluateBoolean (BaseIterator iter)
		{
			XPathResultType typeL = _left.GetReturnType (iter);
			XPathResultType typeR = _right.GetReturnType (iter);

			// TODO: avoid double evaluations
			if (typeL == XPathResultType.Any)
				typeL = GetReturnType (_left.Evaluate (iter));
			if (typeR == XPathResultType.Any)
				typeR = GetReturnType (_right.Evaluate (iter));

			// Regard RTF as string
			if (typeL == XPathResultType.Navigator)
				typeL = XPathResultType.String;
			if (typeR == XPathResultType.Navigator)
				typeR = XPathResultType.String;

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

		public override bool StaticValueAsBoolean {
			get { return HasStaticValue ? Compare (_left.StaticValueAsNumber, _right.StaticValueAsNumber) : false; }
		}

		// FIXME: Avoid extraneous evaluation.
		public override bool EvaluateBoolean (BaseIterator iter)
		{
			XPathResultType typeL = _left.GetReturnType (iter);
			XPathResultType typeR = _right.GetReturnType (iter);

			if (typeL == XPathResultType.Any)
				typeL = GetReturnType (_left.Evaluate (iter));
			if (typeR == XPathResultType.Any)
				typeR = GetReturnType (_right.Evaluate (iter));

			// Regard RTF as string
			if (typeL == XPathResultType.Navigator)
				typeL = XPathResultType.String;
			if (typeR == XPathResultType.Navigator)
				typeR = XPathResultType.String;

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
								if (Compare ((double) rgNodesL [l], numR))
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

		public override Expression Optimize ()
		{
			base.Optimize ();
			return !HasStaticValue ?
				(Expression) this :
				new ExprNumber (StaticValueAsNumber);
		}

		public override object Evaluate (BaseIterator iter)
		{
			return EvaluateNumber (iter);
		}
	}

	internal class ExprPLUS : ExprNumeric
	{
		public ExprPLUS (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "+"; }}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? _left.StaticValueAsNumber + _right.StaticValueAsNumber: 0; }
		}

		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) + _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMINUS : ExprNumeric
	{
		public ExprMINUS (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "-"; }}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? _left.StaticValueAsNumber - _right.StaticValueAsNumber: 0; }
		}

		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) - _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMULT : ExprNumeric
	{
		public ExprMULT (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "*"; }}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? _left.StaticValueAsNumber * _right.StaticValueAsNumber: 0; }
		}

		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) * _right.EvaluateNumber (iter);
		}
	}

	internal class ExprDIV : ExprNumeric
	{
		public ExprDIV (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return " div "; }}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? _left.StaticValueAsNumber / _right.StaticValueAsNumber: 0; }
		}

		public override double EvaluateNumber (BaseIterator iter)
		{
			return _left.EvaluateNumber (iter) / _right.EvaluateNumber (iter);
		}
	}

	internal class ExprMOD : ExprNumeric
	{
		public ExprMOD (Expression left, Expression right) : base (left, right) {}
		protected override String Operator { get { return "%"; }}

		public override double StaticValueAsNumber {
			get { return HasStaticValue ? _left.StaticValueAsNumber % _right.StaticValueAsNumber: 0; }
		}

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

		public override Expression Optimize ()
		{
			_expr = _expr.Optimize ();
			return !HasStaticValue ?
				(Expression) this :
				new ExprNumber (StaticValueAsNumber);
		}

		internal override bool Peer {
			get { return _expr.Peer; }
		}

		public override bool HasStaticValue {
			get { return _expr.HasStaticValue; }
		}

		public override double StaticValueAsNumber {
			get { return _expr.HasStaticValue ? -1 * _expr.StaticValueAsNumber : 0; }
		}

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

		// For "peer and subtree" optimization. see:
		// http://idealliance.org/papers/dx_xmle04/papers/02-03-02/02-03-02.html
		internal abstract bool Subtree { get; }
	}

	internal class ExprUNION : NodeSet
	{
		internal Expression left, right;
		public ExprUNION (Expression left, Expression right)
		{
			this.left = left;
			this.right = right;
		}

		public override Expression Optimize ()
		{
			left = left.Optimize ();
			right = right.Optimize ();
			return this;
		}

		public override String ToString () { return left.ToString ()+ " | " + right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterLeft = left.EvaluateNodeSet (iter);
			BaseIterator iterRight = right.EvaluateNodeSet (iter);
			return new UnionIterator (iter, iterLeft, iterRight);
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return left.EvaluatedNodeType == right.EvaluatedNodeType ? left.EvaluatedNodeType : XPathNodeType.All; }
		}

		internal override bool IsPositional {
			get { return left.IsPositional || right.IsPositional; }
		}

		internal override bool Peer {
			get { return left.Peer && right.Peer; }
		}

		internal override bool Subtree {
			get {
				NodeSet nl = left as NodeSet;
				NodeSet nr = right as NodeSet;
				return nl != null && nr != null && nl.Subtree && nr.Subtree;
			}
		}
	}

	internal class ExprSLASH : NodeSet
	{
		public Expression left;
		public NodeSet right;
		public ExprSLASH (Expression left, NodeSet right)
		{
			this.left = left;
			this.right = right;
		}

		public override Expression Optimize ()
		{
			left = left.Optimize ();
			right = (NodeSet) right.Optimize ();
			return this;
		}

		public override String ToString () { return left.ToString ()+ "/" + right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			// Peer and subtree optimization. see
			// http://idealliance.org/papers/dx_xmle04/papers/02-03-02/02-03-02.html
			BaseIterator iterLeft = left.EvaluateNodeSet (iter);
			if (left.Peer && right.Subtree)
				return new SimpleSlashIterator (iterLeft, right);
			BaseIterator si = new SlashIterator (iterLeft, right);
			return new SortedIterator (si);
		}

		public override bool RequireSorting { get { return left.RequireSorting || right.RequireSorting; } }

		internal override XPathNodeType EvaluatedNodeType {
			get { return right.EvaluatedNodeType; }
		}

		internal override bool IsPositional {
			get { return left.IsPositional || right.IsPositional; }
		}

		internal override bool Peer {
			get { return left.Peer && right.Peer; }
		}

		internal override bool Subtree {
			get {
				NodeSet n = left as NodeSet;
				return n != null && n.Subtree && right.Subtree;
			}
		}
	}
	
	internal class ExprSLASH2 : NodeSet {
		public Expression left;
		public NodeSet right;
			
		static NodeTest DescendantOrSelfStar = new NodeTypeTest (Axes.DescendantOrSelf, XPathNodeType.All);

		public ExprSLASH2 (Expression left, NodeSet right)
		{
			this.left = left;
			this.right = right;
		}

		public override Expression Optimize ()
		{
			left = left.Optimize ();
			right = (NodeSet) right.Optimize ();
			// Path A//B is equal to 
			// A/descendant-or-self::node()/child::B, which is
			// equivalent to A/descendant::B. Unlike '//', '/'
			// could be optimized by SimpleSlashIterator.
			NodeTest rnt = right as NodeTest;
			if (rnt != null && rnt.Axis.Axis == Axes.Child) {
				NodeNameTest nameTest = rnt as NodeNameTest;
				if (nameTest != null)
					return new ExprSLASH (left,
						new NodeNameTest (nameTest, Axes.Descendant));
				NodeTypeTest typeTest = rnt as NodeTypeTest;
				if (typeTest != null)
					return new ExprSLASH (left,
						new NodeTypeTest (typeTest, Axes.Descendant));
			}
			return this;
		}

		public override String ToString () { return left.ToString ()+ "//" + right.ToString (); }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator il = left.EvaluateNodeSet (iter);
			if (left.Peer && !left.RequireSorting)
				il = new SimpleSlashIterator (
					il, DescendantOrSelfStar);
			else {
				BaseIterator bb = new SlashIterator (il, DescendantOrSelfStar);
				il = left.RequireSorting ? new SortedIterator (bb) : bb;
			}

			// FIXME: there could be chances to introduce sort-less
			// iterator, but no one could do it yet.
			SlashIterator b = new SlashIterator (il, right);
			return new SortedIterator (b);
		}

		public override bool RequireSorting { get { return left.RequireSorting || right.RequireSorting; } }

		internal override XPathNodeType EvaluatedNodeType {
			get { return right.EvaluatedNodeType; }
		}

		internal override bool IsPositional {
			get { return left.IsPositional || right.IsPositional; }
		}

		internal override bool Peer {
			get { return false; }
		}

		internal override bool Subtree {
			get {
				NodeSet n = left as NodeSet;
				return n != null && n.Subtree && right.Subtree;
			}
		}
	}

	internal class ExprRoot : NodeSet
	{
		public override String ToString () { return ""; }
		public override object Evaluate (BaseIterator iter)
		{
			if (iter.CurrentPosition == 0) {
				iter = (BaseIterator) iter.Clone ();
				iter.MoveNext ();
			}
			XPathNavigator navRoot = iter.Current.Clone ();
			navRoot.MoveToRoot ();
			return new SelfIterator (navRoot, iter.NamespaceManager);
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return XPathNodeType.Root; }
		}

		internal override bool Peer {
			get { return true; }
		}

		internal override bool Subtree {
			get { return false; }
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
				switch (_axis) {
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
			switch (_axis) {
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
				return "preceding";
			case Axes.PrecedingSibling:
				return "preceding-sibling";
			case Axes.Self:
				return "self";
			default:
				throw new IndexOutOfRangeException ();
			}
		}
		public Axes Axis { get { return _axis; }}
		public BaseIterator Evaluate (BaseIterator iter)
		{
			switch (_axis) {
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
			BaseIterator iterAxis = _axis.Evaluate (iter);
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
				case Axes.Attribute:
				case Axes.Namespace:
					return true;
				default:
					return false;
				}
			}

		}

		internal override bool Peer {
			get {
				switch (_axis.Axis) {
				case Axes.Ancestor:
				case Axes.AncestorOrSelf:
				case Axes.DescendantOrSelf:
				case Axes.Descendant:
				case Axes.Preceding:
				case Axes.Following:
					return false;
				default:
					return true;
				}
			}
		}

		internal override bool Subtree {
			get {
				switch (_axis.Axis) {
				case Axes.Parent:
				case Axes.Ancestor:
				case Axes.AncestorOrSelf:
				case Axes.Preceding:
				case Axes.PrecedingSibling:
				case Axes.Following:
				case Axes.FollowingSibling:
					return false;
				default:
					return true;
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
		// FIXME: Better description
		public NodeTypeTest (Axes axis, XPathNodeType type, String param) : base (axis)
		{
			this.type = type;
			_param = param;
			if (param != null && type != XPathNodeType.ProcessingInstruction)
				throw new XPathException ("No argument allowed for "+ToString (type)+"() test");	// TODO: better description
		}

		// for optimizer use
		internal NodeTypeTest (NodeTypeTest other, Axes axis)
			: base (axis)
		{
			type = other.type;
			_param = other._param;
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
			switch (type) {
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
			switch (type) {
			case XPathNodeType.All:
				return true;

			case XPathNodeType.ProcessingInstruction:
				if (nodeType != XPathNodeType.ProcessingInstruction)
					return false;
				if (_param != null && nav.Name != _param)
					return false;
				return true;
			
			case XPathNodeType.Text:
				switch (nodeType) {
				case XPathNodeType.Text:
				case XPathNodeType.Whitespace:
				case XPathNodeType.SignificantWhitespace:
					return true;
				default:
					return false;
				}
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
			this.resolvedName = resolvedName;
		}

		// for optimized path rewrite
		internal NodeNameTest (NodeNameTest source, Axes axis)
			: base (axis)
		{
			_name = source._name;
			resolvedName = source.resolvedName;
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
			if (_name.Namespace != "")
			{
				if (resolvedName)
					strURI1 = _name.Namespace;
				else if (nsm != null)
					strURI1 = nsm.LookupNamespace (_name.Namespace);
				if (strURI1 == null)
					throw new XPathException ("Invalid namespace prefix: "+_name.Namespace);
			}

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
					ns = nsm.LookupNamespace (_name.Namespace);	// TODO: check to see if this returns null or ""
				if (ns == null)
					throw new XPathException ("Invalid namespace prefix: "+_name.Namespace);
			}
		}
	}

	internal class ExprFilter : NodeSet
	{
		internal Expression expr, pred;
		
		public ExprFilter (Expression expr, Expression pred)
		{
			this.expr = expr;
			this.pred = pred;
		}

		public override Expression Optimize ()
		{
			expr = expr.Optimize ();
			pred = pred.Optimize ();
			return this;
		}
		
		internal Expression LeftHandSide {get{return expr;}}
		public override String ToString () { return "(" + expr.ToString () + ")[" + pred.ToString () + "]"; }
		public override object Evaluate (BaseIterator iter)
		{
			BaseIterator iterExpr = expr.EvaluateNodeSet (iter);
			return new PredicateIterator (iterExpr, pred);
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

		internal override bool Peer {
			get { return expr.Peer && pred.Peer; }
		}

		internal override bool Subtree {
			get {
				NodeSet n = expr as NodeSet;
				return n != null && n.Subtree;
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

		internal override bool Peer {
			get { return true; }
		}

		public override bool HasStaticValue {
			get { return true; }
		}

		public override double StaticValueAsNumber {
			get { return XPathFunctions.ToNumber (_value); }
		}

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

	internal class BooleanConstant : Expression
	{
		bool _value;

		public BooleanConstant (bool value)
		{
			_value = value;
		}

		public override String ToString () { return _value ? "true()" : "false()"; }
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		internal override bool Peer {
			get { return true; }
		}

		public override bool HasStaticValue {
			get { return true; }
		}

		public override bool StaticValueAsBoolean {
			get { return _value; }
		}

		public override object Evaluate (BaseIterator iter)
		{
			return _value;
		}
		
		public override bool EvaluateBoolean (BaseIterator iter)
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
		public string Value { get { return _value; } }
		public override String ToString () { return "'" + _value + "'"; }
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}

		internal override bool Peer {
			get { return true; }
		}

		public override bool HasStaticValue {
			get { return true; }
		}

		public override string StaticValueAsString {
			get { return _value; }
		}

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
			else
				throw new XPathException (String.Format ("XSLT context is required to resolve variable. Current namespace manager in current node-set '{0}' is '{1}'", iter.GetType (), iter.NamespaceManager != null ? iter.NamespaceManager.GetType () : null));
			
			if (var == null)
				throw new XPathException ("variable "+_name.ToString ()+" not found");
			object objResult = var.Evaluate (context);
			XPathNodeIterator iterResult = objResult as XPathNodeIterator;
			if (iterResult != null)
				return iterResult is BaseIterator ? iterResult : new WrapperIterator (iterResult, iter.NamespaceManager);
			return objResult;
		}

		internal override bool Peer {
			get { return false; }
		}
	}

	internal class ExprParens : Expression
	{
		protected Expression _expr;
		public ExprParens (Expression expr)
		{
			_expr = expr;
		}

		public override Expression Optimize ()
		{
			_expr.Optimize ();
			return this;
		}

		public override bool HasStaticValue {
			get { return _expr.HasStaticValue; }
		}

		public override object StaticValue {
			get { return _expr.StaticValue; }
		}

		public override string StaticValueAsString {
			get { return _expr.StaticValueAsString; }
		}

		public override double StaticValueAsNumber {
			get { return _expr.StaticValueAsNumber; }
		}

		public override bool StaticValueAsBoolean {
			get { return _expr.StaticValueAsBoolean; }
		}

		public override String ToString () { return "(" + _expr.ToString () + ")"; }
		public override XPathResultType ReturnType { get { return _expr.ReturnType; }}
		public override object Evaluate (BaseIterator iter)
		{
			object o = (_expr.Evaluate (iter));
			XPathNodeIterator xi = o as XPathNodeIterator;
			BaseIterator predBase = xi as BaseIterator;
			if (predBase == null && xi != null)
				predBase = new WrapperIterator (xi, iter.NamespaceManager);
			if (predBase != null)
				return new ParensIterator (predBase);
			else
				return o;
		}

		internal override XPathNodeType EvaluatedNodeType {
			get { return _expr.EvaluatedNodeType; }
		}

		internal override bool IsPositional {
			get { return _expr.IsPositional; }
		}

		internal override bool Peer {
			get { return _expr.Peer; }
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

		internal override bool Peer {
			get { return false; }
		}
	}
}
