//
// XPathSequence.cs - represents XPath sequence iterator
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Query;
using System.Xml.XPath;

namespace Mono.Xml.XPath2
{
	public abstract class XPathSequence : IEnumerable, ICloneable
	{
		XQueryContext ctx;
		int countCache = -1;
		int position = 0;

		internal XPathSequence (XQueryContext ctx)
		{
			this.ctx = ctx;
		}

		internal XPathSequence (XPathSequence original)
		{
			ctx = original.ctx;
			position = original.position;
		}

		internal XQueryContext Context {
			get { return ctx; }
		}

		public virtual int Count {
			get {
				if (countCache >= 0)
					return countCache;
				XPathSequence clone = Clone ();
				countCache = 0;
				while (clone.MoveNext ())
					countCache++;
				return countCache;
			}
		}

		public XPathItem Current {
			get {
				if (Position == 0)
					throw new InvalidOperationException ("XQuery internal error (should not happen)");
				return CurrentCore;
			}
		}

		public abstract XPathItem CurrentCore { get; }

		// Returns 0 if not started, otherwise returns XPath positional integer.
		public int Position {
			get { return position; }
		}

		public bool MoveNext ()
		{
			if (!MoveNextCore ())
				return false;
			position++;
			return true;
		}

		public abstract bool MoveNextCore ();

		public abstract XPathSequence Clone ();

		object ICloneable.Clone ()
		{
			return this.Clone ();
		}

		public virtual IEnumerator GetEnumerator ()
		{
			while (MoveNext ())
				yield return CurrentCore;
		}

	}

	// empty iterator (still required since it contains XQueryContext)
	class XPathEmptySequence : XPathSequence
	{
		internal XPathEmptySequence (XPathSequence iter)
			: base (iter.Context)
		{
		}

		public override int Count {
			get { return 0; }
		}

		public override bool MoveNextCore ()
		{
			return false;
		}

		public override XPathItem CurrentCore {
			get { throw new InvalidOperationException ("Should not happen. In XPathEmptySequence.Current."); }
		}

		// Don't return clone
		public override XPathSequence Clone ()
		{
			return this;
		}
	}

	// single item iterator

	internal class SingleItemIterator : XPathSequence
	{
		XPathItem item;
		XPathItem current;

		public SingleItemIterator (XPathItem item, XPathSequence iter)
			: this (item, iter.Context)
		{
		}

		// for XQuery execution start point
		internal SingleItemIterator (XPathItem item, XQueryContext ctx)
			: base (ctx)
		{
			this.item = item;
		}

		private SingleItemIterator (SingleItemIterator other)
			: base (other)
		{
			this.item = other.item;
			this.current = other.current;
		}

		public override XPathSequence Clone ()
		{
			return new SingleItemIterator (this);
		}

		public override bool MoveNextCore ()
		{
			if (item != null) {
				current = item;
				item = null;
				return true;
			}
			return false;
		}

		public override XPathItem CurrentCore {
			get {
				return current;
			}
		}
	}

	// RangeExpr iterator

	internal class IntegerRangeIterator : XPathSequence
	{
		static XmlSchemaSimpleType intType = XmlSchemaType.GetBuiltInSimpleType (new XmlQualifiedName ("int", XmlSchema.Namespace));

		int start;
		int end;
		int next;
		XPathItem current;

		public IntegerRangeIterator (XPathSequence iter, int start, int end)
			: base (iter.Context)
		{
			this.start = start;
			this.end = end;
		}

		private IntegerRangeIterator (IntegerRangeIterator other)
			: base (other)
		{
			this.start = other.start;
			this.end = other.end;
			this.next = other.next;
			this.current = other.current;
		}

		public override XPathSequence Clone ()
		{
			return new IntegerRangeIterator (this);
		}

		public override bool MoveNextCore ()
		{
			if (current == null)
				next = start;
			if (next > end)
				return false;
			current = new XPathAtomicValue (next++, intType);
			return true;
		}

		public override XPathItem CurrentCore {
			get {
				return current;
			}
		}
	}

	// Slash iterator
	internal class ChildPathIterator : XPathSequence
	{
		XPathSequence left;
		ExprSingle child;

		public ChildPathIterator (XPathSequence iter, PathChildExpr source)
			: base (iter.Context)
		{
			left = source.Left.Evaluate (iter);
			child = source.Next;
		}

		private ChildPathIterator (ChildPathIterator other)
			: base (other)
		{
			left = other.left.Clone ();
			child = other.child;
		}

		public override XPathSequence Clone ()
		{
			return new ChildPathIterator (this);
		}

		public override bool MoveNextCore ()
		{
			throw new NotImplementedException ();
		}

		public override XPathItem CurrentCore {
			get { return left.Current; }
		}
	}

	// Slash2 iterator
	internal class DescendantPathIterator : XPathSequence
	{
		XPathSequence left;
		ExprSingle descendant;

		public DescendantPathIterator (XPathSequence iter, PathDescendantExpr source)
			: base (iter.Context)
		{
			left = source.Left.Evaluate (iter);
			descendant = source.Descendant;
		}

		private DescendantPathIterator (DescendantPathIterator other)
			: base (other)
		{
			left = other.left.Clone ();
			descendant = other.descendant;
		}

		public override XPathSequence Clone ()
		{
			return new DescendantPathIterator (this);
		}

		public override bool MoveNextCore ()
		{
			throw new NotImplementedException ();
		}

		public override XPathItem CurrentCore {
			get { return left.Current; }
		}
	}

	// Filter step iterator
	internal class FilteredIterator : XPathSequence
	{
		XPathSequence left;
		PredicateList filter;

		public FilteredIterator (XPathSequence iter, FilterStepExpr source)
			: base (iter.Context)
		{
			left = source.Expr.Evaluate (iter);
			filter = source.Predicates;
		}

		private FilteredIterator (FilteredIterator other)
			: base (other)
		{
			left = other.left.Clone ();
			filter = other.filter;
		}

		public override XPathSequence Clone ()
		{
			return new FilteredIterator (this);
		}

		public override bool MoveNextCore ()
		{
			throw new NotImplementedException ();
		}

		public override XPathItem CurrentCore {
			get { return left.Current; }
		}
	}

	// AxisIterator
	internal class AxisIterator : XPathSequence
	{
		XPathSequence iter;
		AxisStepExpr source;

		public AxisIterator (XPathSequence iter, AxisStepExpr source)
			: base (iter.Context)
		{
			this.iter = iter;
			this.source = source;
		}

		private AxisIterator (AxisIterator other)
			: base (other)
		{
			iter = other.iter.Clone ();
			source = other.source;
		}

		public override XPathSequence Clone ()
		{
			return new AxisIterator (this);
		}

		public override bool MoveNextCore ()
		{
			throw new NotImplementedException ();
		}

		public override XPathItem CurrentCore {
			get { throw new NotImplementedException (); }
		}
	}

/*
	// Node test iterator
	internal class NodeKindTestIterator : XPathSequence
	{
		XPathSequence iter;
		NodeKindTestExpression source;

		public NodeKindTestIterator (XPathSequence iter, NodeKindTestExpression source)
			: base (iter.Context)
		{
			this.iter = iter;
			this.source = source;
		}

		private NodeKindTestIterator (NodeKindTestIterator other)
			: base (other)
		{
			iter = other.iter.Clone ();
			source = other.source;
		}

		public override XPathSequence Clone ()
		{
			return new NodeKindTestIterator (this);
		}

		public override bool MoveNextCore ()
		{
			throw new NotImplementedException ();
		}

		public override XPathItem CurrentCore {
			get { throw new NotImplementedException (); }
		}
	}
*/

	internal class ExprSequenceIterator : XPathSequence
	{
		XPathSequence contextSequence;
		XPathSequence iter;
		ExprSequence expr;
		int currentExprIndex;

		public ExprSequenceIterator (XPathSequence iter, ExprSequence expr)
			: base (iter.Context)
		{
			contextSequence = iter;
			this.expr = expr;
		}

		private ExprSequenceIterator (ExprSequenceIterator other)
			: base (other)
		{
			if (other.iter != null)
				iter = other.iter.Clone ();
			expr = other.expr;
			currentExprIndex = other.currentExprIndex;
		}

		public override XPathSequence Clone ()
		{
			return new ExprSequenceIterator (this);
		}

		public override bool MoveNextCore ()
		{
			if (iter != null && iter.MoveNext ())
				return true;
			while (currentExprIndex < expr.Count) {
				iter = expr [currentExprIndex++].Evaluate (contextSequence);
				if (iter.MoveNext ())
					return true;
			}
			return false;
		}

		public override XPathItem CurrentCore {
			get { return iter.Current; }
		}
	}

	internal class FLWORIterator : XPathSequence
	{
		XPathSequence contextSequence;
		FLWORExpr expr;

		public FLWORIterator (XPathSequence iter, FLWORExpr expr)
			: base (iter.Context)
		{
			this.contextSequence = iter;
			this.expr = expr;
		}

		private FLWORIterator (FLWORIterator other)
			: base (other)
		{
			contextSequence = other.contextSequence;
			expr = other.expr;
			throw new NotImplementedException ();
		}

		public override XPathSequence Clone ()
		{
			return new FLWORIterator (this);
		}

		public override bool MoveNextCore ()
		{
			throw new NotImplementedException ();
		}

		public override XPathItem CurrentCore {
			get { throw new NotImplementedException (); }
		}
	}

	internal class AtomizingIterator : XPathSequence
	{
		XPathSequence iter;

		public AtomizingIterator (XPathSequence iter)
			: base (iter.Context)
		{
			this.iter = iter;
		}

		private AtomizingIterator (AtomizingIterator other)
			: base (other)
		{
			iter = other.iter.Clone ();
		}

		public override XPathSequence Clone ()
		{
			return new AtomizingIterator (this);
		}

		public override bool MoveNextCore ()
		{
			return iter.MoveNext ();
		}

		public override XPathItem CurrentCore {
			get {
				XPathNavigator nav = iter.Current as XPathNavigator;
				if (nav == null)
					return (XPathAtomicValue) iter.Current;
				if (nav.SchemaInfo != null)
					return new XPathAtomicValue (
						nav.TypedValue,
						nav.SchemaInfo.SchemaType);
				else
					return new XPathAtomicValue (nav.Value, null);
			}
		}
	}

	internal class ConvertingIterator : XPathSequence
	{
		XPathSequence iter;
		SequenceType type;

		public ConvertingIterator (XPathSequence iter, SequenceType type)
			: base (iter.Context)
		{
			this.iter = iter;
			type = type;
		}

		private ConvertingIterator (ConvertingIterator other)
			: base (other)
		{
			iter = other.iter.Clone ();
			type = other.type;
		}

		public override XPathSequence Clone ()
		{
			return new ConvertingIterator (this);
		}

		public override bool MoveNextCore ()
		{
			return iter.MoveNext ();
		}

		public override XPathItem CurrentCore {
			get { return type.Convert (iter.Current); }
		}
	}

	internal class TracingIterator : XPathSequence
	{
		XPathSequence iter;
		string format;

		public TracingIterator (XPathSequence iter, string format)
			: base (iter.Context)
		{
			this.iter = iter;
			this.format = format;
		}

		private TracingIterator (TracingIterator other)
			: base (other)
		{
			iter = other.iter.Clone ();
			format = other.format;
		}

		public override XPathSequence Clone ()
		{
			return new TracingIterator (this);
		}

		public override bool MoveNextCore ()
		{
			if (!iter.MoveNext ())
				return false;
			// FIXME: use OnMessageEvent
//			Context.ErrorOutput.Write (format, iter.Current.TypedValue);
			throw new NotImplementedException ();
			return true;
		}

		public override XPathItem CurrentCore {
			get { return iter.Current; }
		}
	}

	internal class ListIterator : XPathSequence
	{
		IList list;

		public ListIterator (XPathSequence iter, IList list)
			: base (iter.Context)
		{
			if (list is ICloneable)
				this.list = list;
			else
				throw new InvalidOperationException (String.Format ("XQuery internal error: target list is not cloneable. List is {0}.", list != null ? list.GetType ().ToString () : "null argument"));
		}

		private ListIterator (ListIterator other)
			: base (other)
		{
			this.list = (IList) ((ICloneable) other.list).Clone ();
		}

		public override XPathSequence Clone ()
		{
			return new ListIterator (this);
		}

		public override bool MoveNextCore ()
		{
			return (Position < list.Count);
		}

		public override XPathItem CurrentCore {
			get { return (XPathItem) list [Position - 1]; }
		}
	}
}

#endif
