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
				while (clone.MoveNext ())
					;
				countCache = clone.Position;
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
		public virtual int Position {
			get { return position; }
		}

		public virtual bool MoveNext ()
		{
			if (!MoveNextCore ())
				return false;
			position++;
			return true;
		}

		protected abstract bool MoveNextCore ();

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
		internal XPathEmptySequence (XQueryContext ctx)
			: base (ctx)
		{
		}

		public override int Count {
			get { return 0; }
		}

		protected override bool MoveNextCore ()
		{
			return false;
		}

		public override XPathItem CurrentCore {
			get { throw new InvalidOperationException ("Should not happen. In XPathEmptySequence.Current."); }
		}

		// Don't return clone. It's waste of resource.
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

		protected override bool MoveNextCore ()
		{
			if (current == null) {
				current = item;
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

		protected override bool MoveNextCore ()
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
	// <copy original='System.Xml.XPath/Iterator.cs,
	//	System.Xml.XPath/XPathComparer.cs'>
	internal class PathStepIterator : XPathSequence
	{
		XPathSequence left;
		XPathSequence right;
		PathStepExpr step;
		ArrayList nodeStore;
		SortedList storedIterators;
		bool finished;
		XPathSequence nextRight;

		public PathStepIterator (XPathSequence iter, PathStepExpr source)
			: base (iter.Context)
		{
			left = iter;
			step = source;
		}

		private PathStepIterator (PathStepIterator other)
			: base (other)
		{
			left = other.left.Clone ();
			step = other.step;
			if (other.right != null)
				right = other.right.Clone ();
			if (other.nodeStore != null)
				nodeStore = (ArrayList) other.nodeStore.Clone ();
			if (other.storedIterators != null)
				storedIterators = (SortedList) other.storedIterators.Clone ();
			if (other.nextRight != null)
				nextRight = other.nextRight.Clone ();
		}

		public override XPathSequence Clone ()
		{
			return new PathStepIterator (this);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;
//			if (RequireSorting) {
			if (step.RequireSorting) {
				// Mainly '//' ('/descendant-or-self::node()/')
				if (nodeStore == null) {
					CollectResults ();
					if (nodeStore.Count == 0) {
						finished = true;
						return false;
					}
				}
				if (nodeStore.Count == Position) {
					finished = true;
					return false;
				}
				while (nodeStore.Count > Position) {
					if (((XPathNavigator) nodeStore [Position]).ComparePosition (
						(XPathNavigator) nodeStore [Position - 1]) == XmlNodeOrder.Same)
						nodeStore.RemoveAt (Position);
					else
						break;
				}

				return true;
			} else { // Sorting not required
				if (right == null) { // First time
					if (!left.MoveNext ())
						return false;
					right = step.Next.Evaluate (left);
					storedIterators = new SortedList (XPathSequenceComparer.Instance);
				}

				while (true) {
					while (!right.MoveNext ()) {
						if (storedIterators.Count > 0) {
							int last = storedIterators.Count - 1;
							XPathSequence tmpIter = (XPathSequence) storedIterators.GetByIndex (last);
							storedIterators.RemoveAt (last);
							switch (((XPathNavigator) tmpIter.Current).ComparePosition ((XPathNavigator) right.Current)) {
							case XmlNodeOrder.Same:
							case XmlNodeOrder.Before:
								right = tmpIter;
								continue;
							default:
								right = tmpIter;
								break;
							}
							break;
						} else if (nextRight != null) {
							right = nextRight;
							nextRight = null;
							break;
						} else if (!left.MoveNext ()) {
							finished = true;
							return false;
						}
						else
							right = step.Next.Evaluate (left);
					}
					bool loop = true;
					while (loop) {
						loop = false;
						if (nextRight == null) {
							bool noMoreNext = false;
							while (nextRight == null || !nextRight.MoveNext ()) {
								if(left.MoveNext ())
									nextRight = step.Next.Evaluate (left);
								else {
									noMoreNext = true;
									break;
								}
							}
							if (noMoreNext)
								nextRight = null; // FIXME: More efficient code. Maybe making noMoreNext class scope would be better.
						}
						if (nextRight != null) {
							switch (((XPathNavigator) right.Current).ComparePosition ((XPathNavigator) nextRight.Current)) {
							case XmlNodeOrder.After:
								storedIterators.Add (storedIterators.Count, right);
								right = nextRight;
								nextRight = null;
								loop = true;
								break;
							case XmlNodeOrder.Same:
								if (!nextRight.MoveNext ())
									nextRight = null;

								else {
									int last = storedIterators.Count;
									if (last > 0) {
										storedIterators.Add (last, nextRight);
										nextRight = (XPathSequence) storedIterators.GetByIndex (last);
										storedIterators.RemoveAt (last);
									}
								}

								loop = true;
								break;
							}
						}
					}
					return true;
				}
			}
		}

		private void CollectResults ()
		{
			if (nodeStore != null)
				return;
			nodeStore = new ArrayList ();
			while (true) {
				while (right == null || !right.MoveNext ()) {
					if (!left.MoveNext ()) {
						nodeStore.Sort (XPathNavigatorComparer2.Instance);
						return;
					}
					right = step.Next.Evaluate (left);
				}
				XPathNavigator nav = (XPathNavigator) right.Current;
				nodeStore.Add (nav);
			}
		}

		public override XPathItem CurrentCore { 
			get {
				if (Position <= 0) return null;
//				if (RequireSorting) {
				if (step.RequireSorting) {
					return (XPathNavigator) nodeStore [Position - 1];
				} else {
					return right.Current;
				}
			}
		}

/*
		public override bool RequireSorting {
			get {
				return left.RequireSorting || step.Next.RequireSorting;
			}
		}
*/

		public override int Count {
			get {
				if (nodeStore == null)
					return base.Count;
				else
					return nodeStore.Count;
			}
		}


		internal class XPathSequenceComparer : IComparer
		{
			public static XPathSequenceComparer Instance = new XPathSequenceComparer ();
			private XPathSequenceComparer ()
			{
			}

			public int Compare (object o1, object o2)
			{
				XPathSequence nav1 = o1 as XPathSequence;
				XPathSequence nav2 = o2 as XPathSequence;
				if (nav1 == null)
					return -1;
				if (nav2 == null)
					return 1;
				switch (((XPathNavigator) nav1.Current).ComparePosition ((XPathNavigator) nav2.Current)) {
				case XmlNodeOrder.Same:
					return 0;
				case XmlNodeOrder.After:
					return -1;
				default:
					return 1;
				}
			}
		}

		internal class XPathNavigatorComparer2 : IComparer
		{
			public static XPathNavigatorComparer2 Instance = new XPathNavigatorComparer2 ();
			private XPathNavigatorComparer2 ()
			{
			}

			public int Compare (object o1, object o2)
			{
				XPathNavigator nav1 = o1 as XPathNavigator;
				XPathNavigator nav2 = o2 as XPathNavigator;
				if (nav1 == null)
					return -1;
				if (nav2 == null)
					return 1;
				switch (nav1.ComparePosition (nav2)) {
				case XmlNodeOrder.Same:
					return 0;
				case XmlNodeOrder.After:
					return 1;
				default:
					return -1;
				}
			}
		}
	}
	// </copy>

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

		protected override bool MoveNextCore ()
		{
			while (left.MoveNext ()) {
				bool skipThisItem = false;
				// Examine all filters
				foreach (ExprSequence expr in filter) {
					bool doesntPass = true;
					// Treat as OK if any of expr passed.
					// FIXME: handle numeric predicate.
					foreach (ExprSingle single in expr) {
						if (single.EvaluateAsBoolean (left)) {
							doesntPass = false;
							break;
						}
					}
					if (doesntPass) {
						skipThisItem = true;
						break;
					}
				}
				if (skipThisItem)
					continue;
				return true;
			}
			return false;
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

		protected override bool MoveNextCore ()
		{
			while (iter.MoveNext ()) {
				if (source.Matches (iter.Current as XPathNavigator))
					return true;
			}
			return false;
		}

		public override XPathItem CurrentCore {
			get { return iter.Current; }
		}
	}

	internal abstract class NodeIterator : XPathSequence
	{
		XPathNavigator node;
		XPathNavigator current;
		bool emptyInput;

		public NodeIterator (XPathSequence iter)
			: base (iter.Context)
		{
			if (iter.Position == 0) {
				if (!iter.MoveNext ()) {
					emptyInput = true;
					return;
				}
			}
			XPathItem item = iter.Current;
			node = item as XPathNavigator;
			if (node == null)
				throw new XmlQueryException (String.Format ("Current item is expected to be a node, but it is {0} ({1}).", item.XmlType.QualifiedName, item.XmlType.TypeCode));
			node = node.Clone ();
		}

		internal NodeIterator (NodeIterator other, bool cloneFlag)
			: base (other)
		{
			if (other.emptyInput)
				emptyInput = true;
			else
				node = other.node.Clone ();
		}

		internal XPathNavigator Node {
			get { return node; }
		}

		public override bool MoveNext ()
		{
			if (emptyInput)
				return false;
			if (!base.MoveNext ())
				return false;
			current = null;
			return true;
		}

		public override XPathItem CurrentCore {
			get {
				if (current == null)
					current = node.Clone ();
				return current;
			}
		}

		public virtual bool ReverseAxis {
			get { return false; }
		}

//		public override bool RequireSorting {
//			get { return ReverseAxis; }
//		}
	}

	// <copy original='System.Xml.XPath/Iterator.cs'>

	internal class ParentIterator : NodeIterator
	{
		public ParentIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private ParentIterator (ParentIterator other, bool cloneFlag) 
			: base (other, true)
		{
		}

		public override XPathSequence Clone ()
		{
			return new ParentIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (Position == 0 && Node.MoveToParent ())
				return true;
			return false;
		}

		public override bool ReverseAxis {
			get { return true; }
		}
	}

	internal class ChildIterator : NodeIterator
	{
		public ChildIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private ChildIterator (ChildIterator other, bool cloneFlag) 
			: base (other, true)
		{
		}

		public override XPathSequence Clone ()
		{
			return new ChildIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (Position == 0)
				return Node.MoveToFirstChild ();
			else
				return Node.MoveToNext ();
		}
	}

	internal class FollowingSiblingIterator : NodeIterator
	{
		public FollowingSiblingIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private FollowingSiblingIterator (FollowingSiblingIterator other, bool cloneFlag) 
			: base (other, true)
		{
		}

		public override XPathSequence Clone ()
		{
			return new FollowingSiblingIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			return Node.MoveToNext ();
		}
	}

	internal class PrecedingSiblingIterator : NodeIterator
	{
		bool finished;
		bool started;
		XPathNavigator startPosition;

		public PrecedingSiblingIterator (XPathSequence iter)
			: base (iter)
		{
			startPosition = Node.Clone ();
		}

		private PrecedingSiblingIterator (PrecedingSiblingIterator other, bool cloneFlag) 
			: base (other, true)
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
		}

		public override XPathSequence Clone ()
		{
			return new PrecedingSiblingIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
				Node.MoveToFirst ();
			} else {
				Node.MoveToNext ();
			}
			if (Node.ComparePosition (startPosition) == XmlNodeOrder.Same) {
				finished = true;
				return false;
			}
			else
				return true;
		}

		public override bool ReverseAxis {
			get { return true; }
		}
	}

	internal class AncestorIterator : NodeIterator
	{
		bool finished;
		ArrayList nodes = new ArrayList ();

		public AncestorIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private AncestorIterator (AncestorIterator other, bool cloneFlag) 
			: base (other, true)
		{
			finished = other.finished;
			nodes = other.nodes;
		}

		public override XPathSequence Clone ()
		{
			return new AncestorIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;
			if (nodes != null) {
				nodes = new ArrayList ();
				while (Node.MoveToParent () && Node.NodeType != XPathNodeType.Root)
					nodes.Add (Node.Clone ());
				nodes.Reverse ();
			}
			if (nodes.Count >= Position)
				return false;
			Node.MoveTo (nodes [Position] as XPathNavigator);
			return true;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override int Count {
			get {
				if (Position == 0)
					return base.Count;
				return nodes.Count;
			}
		}
	}

	internal class AncestorOrSelfIterator : NodeIterator
	{
		bool finished;
		ArrayList nodes = new ArrayList ();

		public AncestorOrSelfIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private AncestorOrSelfIterator (AncestorOrSelfIterator other, bool cloneFlag) 
			: base (other, true)
		{
			finished = other.finished;
			nodes = other.nodes;
		}

		public override XPathSequence Clone ()
		{
			return new AncestorOrSelfIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;
			if (nodes != null) {
				nodes = new ArrayList ();
				do {
					nodes.Add (Node.Clone ());
				} while (Node.MoveToParent () && Node.NodeType != XPathNodeType.Root);
				nodes.Reverse ();
			}
			if (nodes.Count >= Position)
				return false;
			Node.MoveTo (nodes [Position] as XPathNavigator);
			return true;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override int Count {
			get {
				if (Position == 0)
					return base.Count;
				return nodes.Count;
			}
		}
	}

	internal class DescendantIterator : NodeIterator
	{
		private int depth;
		private bool finished;

		public DescendantIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private DescendantIterator (DescendantIterator other, bool cloneFlag) 
			: base (other, true)
		{
			finished = other.finished;
			depth = other.depth;
		}

		public override XPathSequence Clone ()
		{
			return new DescendantIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;

			if (Node.MoveToFirstChild ()) {
				depth ++;
				return true;
			}
			while (depth != 0) {
				if (Node.MoveToNext ())
					return true;

				if (!Node.MoveToParent ())	// should NEVER fail!
					throw new XmlQueryException ("There seems some bugs on the XPathNavigator implementation class.");
				depth --;
			}
			finished = true;
			return false;
		}
	}

	internal class DescendantOrSelfIterator : NodeIterator
	{
		protected int depth;
		private bool finished;

		public DescendantOrSelfIterator (XPathSequence iter) 
			: base (iter)
		{
		}

		protected DescendantOrSelfIterator (DescendantOrSelfIterator other, bool cloneFlag) 
			: base (other, true)
		{
			depth = other.depth;
			finished = other.finished;
		}

		public override XPathSequence Clone ()
		{
			return new DescendantOrSelfIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;

			if (Position == 0)
				return true; // Self


			if (Node.MoveToFirstChild ()) {
				depth ++;
				return true;
			}
			while (depth != 0) {
				if (Node.MoveToNext ())
					return true;

				if (!Node.MoveToParent ())	// should NEVER fail!
					throw new XmlQueryException ("There seems some bugs on the XPathNavigator implementation class.");
				depth --;
			}
			finished = true;
			return false;
		}
	}

	internal class FollowingIterator : NodeIterator
	{
		private bool finished;

		public FollowingIterator (XPathSequence iter) 
			: base (iter)
		{
		}

		protected FollowingIterator (FollowingIterator other, bool cloneFlag) 
			: base (other, true)
		{
			finished = other.finished;
		}

		public override XPathSequence Clone ()
		{
			return new FollowingIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;
			if (Position == 0) {
				// At first, it should not iterate children.
				if (Node.MoveToNext ())
					return true;
				else {
					while (Node.MoveToParent ())
						if (Node.MoveToNext ())
							return true;
				}
			} else {
				if (Node.MoveToFirstChild ())
					return true;
				do {
					if (Node.MoveToNext ())
						return true;
				} while (Node.MoveToParent ());
			}
			finished = true;
			return false;
		}
	}

	internal class PrecedingIterator : NodeIterator
	{
		bool finished;
		bool started;
		XPathNavigator startPosition;

		public PrecedingIterator (XPathSequence iter)
			: base (iter) 
		{
			startPosition = Node.Clone ();
		}

		private PrecedingIterator (PrecedingIterator other, bool cloneFlag) 
			: base (other, true)
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
		}

		public override XPathSequence Clone ()
		{
			return new PrecedingIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
				Node.MoveToRoot ();
			}
			bool loop = true;
			while (loop) {
				while (!Node.MoveToFirstChild ()) {
					while (!Node.MoveToNext ()) {
						if (!Node.MoveToParent ()) { // Should not finish, at least before startPosition.
							finished = true;
							return false;
						}
					}
					break;
				}
				if (Node.IsDescendant (startPosition))
					continue;
				loop = false;
				break;
			}
			if (Node.ComparePosition (startPosition) != XmlNodeOrder.Before) {
				// Note that if _nav contains only 1 node, it won't be Same.
				finished = true;
				return false;
			}
			else
				return true;
		}

		public override bool ReverseAxis {
			get { return true; }
		}
	}

	internal class NamespaceIterator : NodeIterator
	{
		public NamespaceIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private NamespaceIterator (NamespaceIterator other, bool cloneFlag) 
			: base (other, true)
		{
		}

		public override XPathSequence Clone ()
		{
			return new NamespaceIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (Position == 0) {
				if (Node.MoveToFirstNamespace ())
					return true;
			}
			else if (Node.MoveToNextNamespace ())
				return true;
			return false;
		}

		public override bool ReverseAxis { get { return true; } }
	}

	internal class AttributeIterator : NodeIterator
	{
		public AttributeIterator (XPathSequence iter)
			: base (iter)
		{
		}

		private AttributeIterator (AttributeIterator other, bool cloneFlag) 
			: base (other, true)
		{
		}

		public override XPathSequence Clone ()
		{
			return new AttributeIterator (this, true);
		}

		protected override bool MoveNextCore ()
		{
			if (Position == 0) {
				if (Node.MoveToFirstAttribute ())
					return true;
			}
			else if (Node.MoveToNextAttribute ())
				return true;
			return false;
		}
	}

	// </copy>

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
			contextSequence = other.contextSequence;
			currentExprIndex = other.currentExprIndex;
		}

		public override XPathSequence Clone ()
		{
			return new ExprSequenceIterator (this);
		}

		protected override bool MoveNextCore ()
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

	// FLWOR - Order By
	internal class FLWORIterator : XPathSequence
	{
		XPathSequence contextSequence;
		FLWORExpr expr;
		ArrayList forStack = new ArrayList ();
		IEnumerator en;
		bool finished;

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
			forStack = other.forStack.Clone () as ArrayList;
			if (en != null)
				en = ((ICloneable) other.en).Clone () as IEnumerator;
			finished = other.finished;
		}

		public override XPathSequence Clone ()
		{
			return new FLWORIterator (this);
		}

#if false
		protected override bool MoveNextCore ()
		{
			throw new NotImplementedException ();
		}
#else
		protected override bool MoveNextCore ()
		{
			if (en == null)
				en = GetEnumerator ();
			return en.MoveNext ();
		}

		public override IEnumerator GetEnumerator ()
		{
			IEnumerator forLetEnum = expr.ForLetClauses.GetEnumerator ();
			// FIXME: this invokation seems to result in an Invalid IL error.
			return EvaluateRemainingForLet (forLetEnum);
		}
		
		private IEnumerator EvaluateRemainingForLet (IEnumerator forLetEnum)
		{
			// Prepare iteration stack
			if (forLetEnum.MoveNext ()) {
				ForLetClause flc = (ForLetClause) forLetEnum.Current;
				IEnumerator flsb = flc.GetEnumerator ();
				IEnumerator items = EvaluateRemainingSingleItem (forLetEnum, flsb);
				while (items.MoveNext ())
					yield return items.Current;
				yield break;
			}

			bool passedFilter = expr.WhereClause == null;
			if (!passedFilter)
				passedFilter = expr.WhereClause.EvaluateAsBoolean (contextSequence);
			if (passedFilter) {
				foreach (XPathItem item in expr.ReturnExpr.Evaluate (contextSequence))
					yield return item;
			}
		}

		private IEnumerator EvaluateRemainingSingleItem (IEnumerator forLetClauses, IEnumerator singleBodies)
		{
			if (singleBodies.MoveNext ()) {
				ForLetSingleBody sb = singleBodies.Current as ForLetSingleBody;
				ForSingleBody fsb = sb as ForSingleBody;
				if (fsb != null) {
					XPathSequence backup = contextSequence;
					Context.ContextManager.PushCurrentSequence (sb.Expression.Evaluate (Context.CurrentSequence));
					foreach (XPathItem forItem in Context.CurrentSequence) {
						Context.PushVariable (fsb.PositionalVar, Context.CurrentSequence.Position);
						Context.PushVariable (sb.VarName, forItem);
						// recurse here (including following bindings)
						IEnumerator items = 
EvaluateRemainingSingleItem (forLetClauses, singleBodies);
						while (items.MoveNext ())
							yield return (XPathItem) items.Current;
						Context.PopVariable ();
						Context.PopVariable ();
					}
					Context.ContextManager.PopCurrentSequence ();
					contextSequence = backup;
				} else {
					Context.PushVariable (sb.VarName, sb.Expression.Evaluate (contextSequence));
					// recurse here (including following bindings)
					IEnumerator items = EvaluateRemainingSingleItem (forLetClauses, singleBodies);
					while (items.MoveNext ())
						yield return (XPathItem) items.Current;
					Context.PopVariable ();
				}
			} else {
				// examine next binding
				IEnumerator items = EvaluateRemainingForLet (forLetClauses);
				while (items.MoveNext ())
					yield return (XPathItem) items.Current;
			}
		}
#endif

		public override XPathItem CurrentCore {
			get { return (XPathItem) en.Current; }
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

		protected override bool MoveNextCore ()
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
			this.type = type;
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

		protected override bool MoveNextCore ()
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

		protected override bool MoveNextCore ()
		{
			if (!iter.MoveNext ())
				return false;
			// FIXME: use OnMessageEvent
			string output = String.Format (format, iter.Current.TypedValue);
			Context.StaticContext.OnMessageEvent (iter.Current, new TraceEventArgs (output));
			return true;
		}

		public override XPathItem CurrentCore {
			get { return iter.Current; }
		}

		internal class TraceEventArgs : QueryEventArgs
		{
			string message;

			internal TraceEventArgs (string message)
			{
				this.message = message;
			}

			public override string Message {
				get { return message; }
			}
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

		protected override bool MoveNextCore ()
		{
			return (Position < list.Count);
		}

		public override XPathItem CurrentCore {
			get { return (XPathItem) list [Position - 1]; }
		}
	}

	internal class EnumeratorIterator : XPathSequence
	{
		IEnumerator list;

		public EnumeratorIterator (XQueryContext ctx, IEnumerable en)
			: base (ctx)
		{
			list = en.GetEnumerator ();
			if (list is ICloneable)
				this.list = list;
			else
				throw new InvalidOperationException (String.Format ("XQuery internal error: target list's enumerator is not cloneable. List is {0}.", en != null ? en.GetType ().ToString () : "null argument"));
		}

		private EnumeratorIterator (EnumeratorIterator other)
			: base (other)
		{
			this.list = (IEnumerator) ((ICloneable) other.list).Clone ();
		}

		public override XPathSequence Clone ()
		{
			return new EnumeratorIterator (this);
		}

		protected override bool MoveNextCore ()
		{
			return list.MoveNext ();
		}

		public override XPathItem CurrentCore {
			get { return (XPathItem) list.Current; }
		}
	}
}

#endif
