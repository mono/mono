//
// System.Xml.XPath.BaseIterator
//
// Author:
//   Piers Haken (piersh@friskit.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002 Piers Haken
// (C) 2003 Atsushi Enomoto
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
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

#if NET_2_0
using NSResolver = System.Xml.IXmlNamespaceResolver;
#else
using NSResolver = System.Xml.XmlNamespaceManager;
#endif

namespace System.Xml.XPath
{
	internal abstract class BaseIterator : XPathNodeIterator
	{
		private NSResolver _nsm;
		protected bool _needClone = true; // TODO: use this field in practice.

		internal BaseIterator (BaseIterator other)
		{
			_nsm = other._nsm;
		}
		internal BaseIterator (NSResolver nsm)
		{
			_nsm = nsm;
		}

		public NSResolver NamespaceManager
		{
			get { return _nsm; }
			set { _nsm = value; }
		}
		
		public virtual bool ReverseAxis {
			get { return false; }
		}

		public abstract bool RequireSorting { get; }

		public virtual int ComparablePosition {
			get {
				if (ReverseAxis) {
					int diff = Count - CurrentPosition + 1;
					return diff < 1 ? 1 : diff;
				}
				else
					return CurrentPosition;
			}
		}

		public override string ToString ()
		{
			if (Current != null)
				return Current.NodeType.ToString () + "[" + CurrentPosition + "] : " + Current.Name + " = " + Current.Value;
			else
				return this.GetType().ToString () + "[" + CurrentPosition + "]";
		}
	}

	internal class WrapperIterator : BaseIterator
	{
		XPathNodeIterator iter;

		public WrapperIterator (XPathNodeIterator iter, NSResolver nsm)
			: base (nsm)
		{
			this.iter = iter;
		}

		public override XPathNodeIterator Clone ()
		{
			return new WrapperIterator (iter.Clone (), NamespaceManager);
		}

		public override bool MoveNext ()
		{
			return iter.MoveNext ();
		}

		public override XPathNavigator Current {
			get { return iter.Current; }
		}

		public override int CurrentPosition {
			get { return iter.CurrentPosition; }
		}

		public override bool RequireSorting {
			get { return true; }
		}
	}

	internal abstract class SimpleIterator : BaseIterator
	{
		protected readonly BaseIterator _iter;
		protected readonly XPathNavigator _nav;
		protected XPathNavigator _current;
		protected int _pos;

		public SimpleIterator (BaseIterator iter) : base (iter)
		{
			_iter = iter;
			_nav = iter.Current.Clone ();
			_current = _nav.Clone ();
		}
		protected SimpleIterator (SimpleIterator other) : base (other)
		{
			if (other._nav == null)
				_iter = (BaseIterator) other._iter.Clone ();
			else
				_nav = other._nav.Clone ();
			_pos = other._pos;
			_current = other._current.Clone ();
		}
		public SimpleIterator (XPathNavigator nav, NSResolver nsm) : base (nsm)
		{
			_nav = nav.Clone ();
			_current = nav.Clone ();
		}

		public override XPathNavigator Current { get { return _current; }}
		public override int CurrentPosition { get { return _pos; }}
	}

	internal class SelfIterator : SimpleIterator
	{
		public SelfIterator (BaseIterator iter) : base (iter) {}
		public SelfIterator (XPathNavigator nav, NSResolver nsm) : base (nav, nsm) {}
		protected SelfIterator (SelfIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new SelfIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				_pos = 1;
				_current = _needClone ? _nav.Clone () : _nav;
				return true;
			}
			return false;
		}

		public override bool RequireSorting { get { return false; } }
	}

	internal class NullIterator : SelfIterator
	{
		public NullIterator (BaseIterator iter) : base (iter) {}
		public NullIterator (XPathNavigator nav) : this (nav, null) {}
		public NullIterator (XPathNavigator nav, NSResolver nsm) : base (nav, nsm) {}
		protected NullIterator (NullIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new NullIterator (this); }
		public override bool MoveNext ()
		{
			return false;
		}
	}

	internal class ParensIterator : BaseIterator
	{
		BaseIterator _iter;
		public ParensIterator (BaseIterator iter) : base (iter) 
		{
			_iter = iter;
		}
		protected ParensIterator (ParensIterator other) : base (other) 
		{
			_iter = (BaseIterator) other._iter.Clone ();
		}
		public override XPathNodeIterator Clone () { return new ParensIterator (this); }
		public override bool MoveNext ()
		{
			return _iter.MoveNext ();
		}

		public override XPathNavigator Current { get { return _iter.Current; }}
		public override int CurrentPosition { get { return _iter.CurrentPosition; } }

		public override bool RequireSorting { get { return _iter.RequireSorting; } }

		public override int Count { get { return _iter.Count; } }
	}

	internal class ParentIterator : SimpleIterator
	{
		public ParentIterator (BaseIterator iter) : base (iter) {}
		protected ParentIterator (ParentIterator other) : base (other) {}
		public ParentIterator (XPathNavigator nav, NSResolver nsm) : base (nav, nsm) {}
		public override XPathNodeIterator Clone () { return new ParentIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0 && _nav.MoveToParent ())
			{
				_pos = 1;
				_current = _needClone ? _nav.Clone () : _nav;
				return true;
			}
			return false;
		}

		public override bool ReverseAxis { get { return true; } }

		public override bool RequireSorting { get { return true; } }
	}

	internal class ChildIterator : SimpleIterator
	{
		public ChildIterator (BaseIterator iter) : base (iter) {}
		protected ChildIterator (ChildIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new ChildIterator (this); }
		public override bool MoveNext ()
		{
			bool fSuccess = (_pos == 0) ? _nav.MoveToFirstChild () : _nav.MoveToNext ();
			if (fSuccess) {
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
			}
			return fSuccess;
		}

		public override bool RequireSorting { get { return false; } }
	}

	internal class FollowingSiblingIterator : SimpleIterator
	{
		public FollowingSiblingIterator (BaseIterator iter) : base (iter) {}
		protected FollowingSiblingIterator (FollowingSiblingIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new FollowingSiblingIterator (this); }
		public override bool MoveNext ()
		{
			switch (_nav.NodeType) {
			case XPathNodeType.Attribute:
			case XPathNodeType.Namespace:
				// They have no siblings.
				return false;
			}
			if (_nav.MoveToNext ())
			{
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			return false;
		}

		public override bool RequireSorting { get { return false; } }
	}

	internal class PrecedingSiblingIterator : SimpleIterator
	{
		bool finished;
		bool started;
		XPathNavigator startPosition;

		public PrecedingSiblingIterator (BaseIterator iter) : base (iter)
		{
			startPosition = iter.Current.Clone ();
			_current = startPosition.Clone ();
		}
		protected PrecedingSiblingIterator (PrecedingSiblingIterator other) : base (other) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
			_current = other._current.Clone ();
		}

		public override XPathNodeIterator Clone () { return new PrecedingSiblingIterator (this); }
		public override bool MoveNext ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
				switch (_nav.NodeType) {
				case XPathNodeType.Attribute:
				case XPathNodeType.Namespace:
					// They have no siblings.
					finished = true;
					return false;
				}

				_nav.MoveToFirst ();
				if (_nav.ComparePosition (startPosition) != XmlNodeOrder.Same) {
					_pos++;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				}
			} else {
				if (!_nav.MoveToNext ()) {
					finished = true;
					return false;
				}
			}
			if (_nav.ComparePosition (startPosition) != XmlNodeOrder.Before) {
				// Note that if _nav contains only 1 node, it won't be Same.
				finished = true;
				return false;
			} else {
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
		}
		public override bool ReverseAxis {
			get { return true; }
		}

		public override bool RequireSorting { get { return true; } }
	}

	internal class AncestorIterator : SimpleIterator
	{
		bool finished;
		bool started;
		ArrayList positions = new ArrayList ();
		XPathNavigator startPosition;
		int nextDepth;
		public AncestorIterator (BaseIterator iter) : base (iter)
		{
			startPosition = iter.Current.Clone ();
			_current = startPosition.Clone ();
		}
		protected AncestorIterator (AncestorIterator other) : base (other)
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
			positions = (ArrayList) other.positions.Clone ();
			nextDepth = other.nextDepth;
			_current = other._current.Clone ();
		}
		public override XPathNodeIterator Clone () { return new AncestorIterator (this); }
		public override bool MoveNext ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
				// This clone cannot be omitted
				XPathNavigator ancestors = startPosition.Clone ();
				ancestors.MoveToParent ();
				_nav.MoveToParent ();
				while (ancestors.NodeType != XPathNodeType.Root) {
					int i = 0;
					_nav.MoveToFirst ();
					while (_nav.ComparePosition (ancestors) == XmlNodeOrder.Before) {
						_nav.MoveToNext ();
						i++;
					}
					positions.Add (i);
					if (!ancestors.MoveToParent ())
						break; // It is for detached nodes under XmlDocumentNavigator
					_nav.MoveToParent ();
				}


				positions.Reverse ();

				if (startPosition.NodeType != XPathNodeType.Root) {
					// First time it returns Root
					_pos++;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				}
			}
			// Don't worry about node type of start position, like AncestorOrSelf.
			// It should be Element or Root.
			if (nextDepth < positions.Count) {
				int thisTimePos = (int) positions [nextDepth];
				_nav.MoveToFirstChild ();
				for (int i = 0; i < thisTimePos; i++)
					_nav.MoveToNext ();
				nextDepth++;
				_pos++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			finished = true;
			return false;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override bool RequireSorting { get { return true; } }

		public override int Count { get { return positions.Count; } }
	}

	internal class AncestorOrSelfIterator : SimpleIterator
	{
		bool finished;
		bool started;
		ArrayList positions = new ArrayList ();
		XPathNavigator startPosition;
		int nextDepth;

		public AncestorOrSelfIterator (BaseIterator iter) : base (iter)
		{
			startPosition = iter.Current.Clone ();
			_current = startPosition.Clone ();
		}
		protected AncestorOrSelfIterator (AncestorOrSelfIterator other) : base (other) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
			positions = (ArrayList) other.positions.Clone ();
			nextDepth = other.nextDepth;
			_current = other._current.Clone ();
		}
		public override XPathNodeIterator Clone () { return new AncestorOrSelfIterator (this); }
		public override bool MoveNext ()
		{
			bool initialIteration = false;
			if (finished)
				return false;
			if (!started) {
				initialIteration = true;
				started = true;
				// This clone cannot be omitted
				XPathNavigator ancestors = startPosition.Clone ();
				do {
					int i = 0;
					_nav.MoveToFirst ();
					while (_nav.ComparePosition (ancestors) == XmlNodeOrder.Before) {
						_nav.MoveToNext ();
						i++;
					}
					positions.Add (i);
					if (!ancestors.MoveToParent ())
						break; // for detached nodes under XmlDocumentNavigator.
					_nav.MoveToParent ();
				} while (ancestors.NodeType != XPathNodeType.Root);
				positions.Reverse ();
			}
			if (initialIteration && startPosition.NodeType != XPathNodeType.Root) {
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			} else if (nextDepth + 1 == positions.Count) {
				nextDepth++;
				_pos++;
				_nav.MoveTo (startPosition);
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			else if (nextDepth < positions.Count) {
				int thisTimePos = (int) positions [nextDepth];
				_nav.MoveToFirstChild ();
				for (int i = 0; i < thisTimePos; i++)
					_nav.MoveToNext ();
				nextDepth++;
				_pos++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			finished = true;
			return false;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override bool RequireSorting { get { return true; } }

		public override int Count { get { return positions.Count; } }
	}

	internal class DescendantIterator : SimpleIterator
	{
		protected int _depth;
		private bool _finished;

		public DescendantIterator (BaseIterator iter) : base (iter) {}

		protected DescendantIterator (DescendantIterator other) : base (other)
		{
			_depth = other._depth;
			_finished = other._finished;
			_current = other._current.Clone ();
		}

		public override XPathNodeIterator Clone () { return new DescendantIterator (this); }

		public override bool MoveNext ()
		{
			if (_finished)
				return false;

			if (_nav.MoveToFirstChild ())
			{
				_depth ++;
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			while (_depth != 0)
			{
				if (_nav.MoveToNext ())
				{
					_pos ++;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				}
				if (!_nav.MoveToParent ())	// should NEVER fail!
					throw new XPathException ("There seems some bugs on the XPathNavigator implementation class.");
				_depth --;
			}
			_finished = true;
			return false;
		}

		public override bool RequireSorting { get { return false; } }
	}

	internal class DescendantOrSelfIterator : SimpleIterator
	{
		protected int _depth;
		private bool _finished;

		public DescendantOrSelfIterator (BaseIterator iter) : base (iter) {}

		protected DescendantOrSelfIterator (DescendantOrSelfIterator other) : base (other)
		{
			_depth = other._depth;
			_current = other._current.Clone ();
		}

		public override XPathNodeIterator Clone () { return new DescendantOrSelfIterator (this); }

		public override bool MoveNext ()
		{
			if (_finished)
				return false;

			if (_pos == 0)
			{
				// self
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			if (_nav.MoveToFirstChild ())
			{
				_depth ++;
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			while (_depth != 0)
			{
				if (_nav.MoveToNext ())
				{
					_pos ++;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				}
				if (!_nav.MoveToParent ())	// should NEVER fail!
					throw new XPathException ("There seems some bugs on the XPathNavigator implementation class.");
				_depth --;
			}
			_finished = true;
			return false;
		}

		public override bool RequireSorting { get { return false; } }
	}

	internal class FollowingIterator : SimpleIterator
	{
		private bool _finished = false;
		public FollowingIterator (BaseIterator iter) : base (iter) {}
		protected FollowingIterator (FollowingIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new FollowingIterator (this); }
		public override bool MoveNext ()
		{
			if (_finished)
				return false;
			if (_pos == 0)
			{
				if (_nav.MoveToNext ())
				{
					_pos ++;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				} else {
					while (_nav.MoveToParent ()) {
						if (_nav.MoveToNext ()) {
							_pos ++;
							// This clone cannot be omitted
							_current = _nav.Clone ();
							return true;
						}
					}
				}
			}
			else
			{
				if (_nav.MoveToFirstChild ())
				{
					_pos ++;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				}
				do
				{
					if (_nav.MoveToNext ())
					{
						_pos ++;
						// This clone cannot be omitted
						_current = _nav.Clone ();
						return true;
					}
				}
				while (_nav.MoveToParent ());
			}
			_finished = true;
			return false;
		}

		public override bool RequireSorting { get { return false; } }
	}

	internal class PrecedingIterator : SimpleIterator
	{
		bool finished;
		bool started;
		XPathNavigator startPosition;

		public PrecedingIterator (BaseIterator iter) : base (iter) 
		{
			startPosition = iter.Current.Clone ();
			_current = startPosition.Clone ();
		}
		protected PrecedingIterator (PrecedingIterator other) : base (other) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
			_current = other._current.Clone ();
		}
		public override XPathNodeIterator Clone () { return new PrecedingIterator (this); }
		public override bool MoveNext ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
				_nav.MoveToRoot ();
			}
			bool loop = true;
			while (loop) {
				while (!_nav.MoveToFirstChild ()) {
					while (!_nav.MoveToNext ()) {
						if (!_nav.MoveToParent ()) { // Should not finish, at least before startPosition.
							finished = true;
							return false;
						}
					}
					break;
				}
				if (_nav.IsDescendant (startPosition))
					continue;
				loop = false;
				break;
			}
			if (_nav.ComparePosition (startPosition) != XmlNodeOrder.Before) {
				// Note that if _nav contains only 1 node, it won't be Same.
				finished = true;
				return false;
			} else {
				_pos ++;
				// This cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
		}
		public override bool ReverseAxis {
			get { return true; }
		}

		public override bool RequireSorting { get { return true; } }
	}

	internal class NamespaceIterator : SimpleIterator
	{
		public NamespaceIterator (BaseIterator iter) : base (iter) {}
		protected NamespaceIterator (NamespaceIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new NamespaceIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				if (_nav.MoveToFirstNamespace ())
				{
					_pos ++;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				}
			}
			else if (_nav.MoveToNextNamespace ())
			{
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			return false;
		}

		public override bool ReverseAxis { get { return true; } }
		public override bool RequireSorting { get { return false; } }
	}

	internal class AttributeIterator : SimpleIterator
	{
		public AttributeIterator (BaseIterator iter) : base (iter) {}
		protected AttributeIterator (AttributeIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new AttributeIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				if (_nav.MoveToFirstAttribute ())
				{
					_pos += 1;
					// This clone cannot be omitted
					_current = _nav.Clone ();
					return true;
				}
			}
			else if (_nav.MoveToNextAttribute ())
			{
				_pos ++;
				// This clone cannot be omitted
				_current = _nav.Clone ();
				return true;
			}
			return false;			
		}

		public override bool RequireSorting { get { return false; } }
	}

	internal class AxisIterator : BaseIterator
	{
		protected SimpleIterator _iter;
		protected NodeTest _test;
		protected int _pos;
			
		string name, ns;
		XPathNodeType matchType;

		public AxisIterator (SimpleIterator iter, NodeTest test) : base (iter)
		{
			_iter = iter;
			_test = test;
			test.GetInfo (out name, out ns, out matchType, NamespaceManager);
//			if (name != null)
//				name = Current.NameTable.Add (name);

//			if (ns != null)
//				ns = Current.NameTable.Add (ns);
		}

		protected AxisIterator (AxisIterator other) : base (other)
		{
			_iter = (SimpleIterator) other._iter.Clone ();
			_test = other._test;
			_pos = other._pos;
			name = other.name;
			ns = other.ns;
			matchType = other.matchType;
		}
		public override XPathNodeIterator Clone () { return new AxisIterator (this); }

		public override bool MoveNext ()
		{
			while (_iter.MoveNext ())
			{
				if (_test.Match (NamespaceManager, Current))
				{
					_pos ++;
					return true;
				}
			}
			return false;
		}
		public override XPathNavigator Current { get { return _iter.Current; }}
		public override int CurrentPosition { get { return _pos; }}
		
		bool Match ()
		{
			if (Current.NodeType != matchType && matchType != XPathNodeType.All)
				return false;
			
			if (ns == null)
				return name == null || (object)name == (object)Current.LocalName;
			else
				return (object)ns == (object)Current.NamespaceURI &&
					(name == null || (object)name == (object)Current.LocalName);
		}
		public override bool ReverseAxis {
			get { return _iter.ReverseAxis; }
		}

		public override bool RequireSorting { get { return _iter.RequireSorting; } }
	}

	internal class SlashIterator : BaseIterator
	{
		protected BaseIterator _iterLeft;
		protected BaseIterator _iterRight;
		protected NodeSet _expr;
		protected int _pos;
		ArrayList _navStore;
		SortedList _iterList;
		bool _finished;
		BaseIterator _nextIterRight;

		public SlashIterator (BaseIterator iter, NodeSet expr) : base (iter)
		{
			_iterLeft = iter;
			_expr = expr;
		}

		protected SlashIterator (SlashIterator other) : base (other)
		{
			_iterLeft = (BaseIterator) other._iterLeft.Clone ();
			if (other._iterRight != null)
				_iterRight = (BaseIterator) other._iterRight.Clone ();
			_expr = other._expr;
			_pos = other._pos;
			if (other._iterList != null)
				_iterList = (SortedList) other._iterList.Clone ();
			if (other._navStore != null)
				_navStore = (ArrayList) other._navStore.Clone ();
			_finished = other._finished;
			if (other._nextIterRight != null)
				_nextIterRight = (BaseIterator) other._nextIterRight.Clone ();
		}
		public override XPathNodeIterator Clone () { return new SlashIterator (this); }

		public override bool MoveNext ()
		{
			if (_finished)
				return false;
			if (RequireSorting) {
				if (_navStore == null) {
					CollectResults ();
					if (_navStore.Count == 0) {
						_finished = true;
						return false;
					}
				}
				_pos++;
				if (_navStore.Count < _pos) {
					_finished = true;
					_pos--;
					return false;
				}
				while (_navStore.Count > _pos) {
					if (((XPathNavigator) _navStore [_pos]).ComparePosition (
						(XPathNavigator) _navStore [_pos - 1]) == XmlNodeOrder.Same)
						_navStore.RemoveAt (_pos);
					else
						break;
				}

				return true;
			} else {
				if (_iterRight == null) { // First time
					if (!_iterLeft.MoveNext ())
						return false;
					_iterRight = _expr.EvaluateNodeSet (_iterLeft);
					_iterList = new SortedList (XPathIteratorComparer.Instance);
				}

				while (true) {
					while (!_iterRight.MoveNext ()) {
						if (_iterList.Count > 0) {
							int last = _iterList.Count - 1;
							BaseIterator tmpIter = (BaseIterator) _iterList.GetByIndex (last);
							_iterList.RemoveAt (last);
							switch (tmpIter.Current.ComparePosition (_iterRight.Current)) {
							case XmlNodeOrder.Same:
							case XmlNodeOrder.Before:
								_iterRight = tmpIter;
								continue;
							default:
								_iterRight = tmpIter;
								break;
							}
							break;
						} else if (_nextIterRight != null) {
							_iterRight = _nextIterRight;
							_nextIterRight = null;
							break;
						} else if (!_iterLeft.MoveNext ()) {
							_finished = true;
							return false;
						}
						else
							_iterRight = _expr.EvaluateNodeSet (_iterLeft);
					}
					bool loop = true;
					while (loop) {
						loop = false;
						if (_nextIterRight == null) {
							bool noMoreNext = false;
							while (_nextIterRight == null || !_nextIterRight.MoveNext ()) {
								if(_iterLeft.MoveNext ())
									_nextIterRight = _expr.EvaluateNodeSet (_iterLeft);
								else {
									noMoreNext = true;
									break;
								}
							}
							if (noMoreNext)
								_nextIterRight = null; // FIXME: More efficient code. Maybe making noMoreNext class scope would be better.
						}
						if (_nextIterRight != null) {
							switch (_iterRight.Current.ComparePosition (_nextIterRight.Current)) {
							case XmlNodeOrder.After:
								_iterList.Add (_iterList.Count, _iterRight);
								_iterRight = _nextIterRight;
								_nextIterRight = null;
								loop = true;
								break;
							case XmlNodeOrder.Same:
								if (!_nextIterRight.MoveNext ())
									_nextIterRight = null;

								else {
									int last = _iterList.Count;
									if (last > 0) {
										_iterList.Add (last, _nextIterRight);
										_nextIterRight = (BaseIterator) _iterList.GetByIndex (last);
										_iterList.RemoveAt (last);
									}
								}

								loop = true;
								break;
							}
						}
					}
					_pos ++;
					return true;
				}
			}
		}
		private void CollectResults ()
		{
			if (_navStore != null)
				return;
			_navStore = new ArrayList ();
			while (true) {
				while (_iterRight == null || !_iterRight.MoveNext ()) {
					if (!_iterLeft.MoveNext ()) {
						_navStore.Sort (XPathNavigatorComparer.Instance);
						return;
					}
					_iterRight = _expr.EvaluateNodeSet (_iterLeft);
				}
				XPathNavigator nav = _iterRight.Current;
				_navStore.Add (_needClone ? nav.Clone () : nav);
			}
		}

		public override XPathNavigator Current { 
			get {
				if (_pos <= 0) return null;
				if (RequireSorting) {
					return (XPathNavigator) _navStore [_pos - 1];
				} else {
					return _iterRight.Current;
				}
			}
		}
		public override int CurrentPosition { get { return _pos; }}

		public override bool RequireSorting {
			get {
				return _iterLeft.RequireSorting || _expr.RequireSorting;
			}
		}

		public override int Count { get { return _navStore == null ? base.Count : _navStore.Count; } }
	}

	internal class PredicateIterator : BaseIterator
	{
		protected BaseIterator _iter;
		protected Expression _pred;
		protected int _pos;
		protected XPathResultType resType;

		public PredicateIterator (BaseIterator iter, Expression pred) : base (iter)
		{
			_iter = iter;
			_pred = pred;
			resType = pred.GetReturnType (iter);
		}

		protected PredicateIterator (PredicateIterator other) : base (other)
		{
			_iter = (BaseIterator) other._iter.Clone ();
			_pred = other._pred;
			_pos = other._pos;
			resType = other.resType;
		}
		public override XPathNodeIterator Clone () { return new PredicateIterator (this); }

		public override bool MoveNext ()
		{
			while (_iter.MoveNext ())
			{
				switch (resType) {
					case XPathResultType.Number:
						if (_pred.EvaluateNumber (_iter) != _iter.ComparablePosition)
							continue;
						break;
					case XPathResultType.Any: {
						object result = _pred.Evaluate (_iter);
						if (result is double)
						{
							if ((double) result != _iter.ComparablePosition)
								continue;
						}
						else if (!XPathFunctions.ToBoolean (result))
							continue;
					}
						break;
					default:
						if (!_pred.EvaluateBoolean (_iter))
							continue;
						break;
				}

				_pos ++;
				return true;
			}
			return false;
		}
		public override XPathNavigator Current { get { return _iter.Current; }}
		public override int CurrentPosition { get { return _pos; }}
		public override bool ReverseAxis {
			get { return _iter.ReverseAxis; }
		}

		public override bool RequireSorting { get { return true; } }
	}

	internal class ListIterator : BaseIterator
	{
		protected IList _list;
		protected int _pos;
		bool _requireSorting;

		public ListIterator (BaseIterator iter, IList list, bool requireSorting) : base (iter)
		{
			if (!(list is ICloneable))
				throw new ArgumentException ("Target enumerator must be cloneable.");
			_list = list;
			_requireSorting = requireSorting;
		}
		
		public ListIterator (IList list, NSResolver nsm, bool requireSorting) : base (nsm)
		{
			if (!(list is ICloneable))
				throw new ArgumentException ("Target enumerator must be cloneable.");
			_list = list;
			_requireSorting = requireSorting;
		}

		protected ListIterator (ListIterator other) : base (other)
		{
			ICloneable listClone = other._list as ICloneable;
			_list = (IList) listClone.Clone ();
			_pos = other._pos;
			_requireSorting = other._requireSorting;
		}
		public override XPathNodeIterator Clone () { return new ListIterator (this); }

		public override bool MoveNext ()
		{
			if (_pos >= _list.Count)
				return false;
			_pos++;
			return true;
		}
		public override XPathNavigator Current {
			get {
				if (_list.Count == 0)
					return null;
				return (XPathNavigator) _list [_pos - 1]; 
			}
		}
		public override int CurrentPosition { get { return _pos; }}

		public override bool RequireSorting { get { return _requireSorting; } }

		public override int Count { get { return _list.Count; } }
	}

	
	internal class UnionIterator : BaseIterator
	{
		protected BaseIterator _left, _right;
		private int _pos;
		private bool keepLeft;
		private bool keepRight;
		private bool useRight;

		public UnionIterator (BaseIterator iter, BaseIterator left, BaseIterator right) : base (iter)
		{
			_left = left;
			_right = right;
		}

		protected UnionIterator (UnionIterator other) : base (other)
		{
			_left = (BaseIterator) other._left.Clone ();
			_right = (BaseIterator) other._right.Clone ();
			_pos = other._pos;
			keepLeft = other.keepLeft;
			keepRight = other.keepRight;
			useRight = other.useRight;
		}
		public override XPathNodeIterator Clone () { return new UnionIterator (this); }

		public override bool MoveNext ()
		{
			if (!keepLeft)
				keepLeft = _left.MoveNext ();
			if (!keepRight)
				keepRight = _right.MoveNext ();

			if (!keepLeft && !keepRight)
				return false;

			_pos ++;
			if (!keepRight) {
				keepLeft = useRight = false;
				return true;
			} else if (!keepLeft) {
				keepRight = false;
				useRight = true;
				return true;
			}

			switch (_left.Current.ComparePosition (_right.Current)) {
			case XmlNodeOrder.Same:
				// consume both. i.e. don't output duplicate result.
				keepLeft = keepRight = false;
				useRight = true;
				return true;
			case XmlNodeOrder.Before:
			case XmlNodeOrder.Unknown: // Maybe happen because of "document(a) | document(b)"
				keepLeft = useRight = false;
				return true;
			case XmlNodeOrder.After:
				keepRight = false;
				useRight = true;
				return true;
			default:
				throw new InvalidOperationException ("Should not happen.");
			}
		}
		public override XPathNavigator Current
		{
			get
			{
				if (_pos == 0)
					return null;
				if (useRight)
					return _right.Current;
				else
					return _left.Current;
			}
		}
		public override int CurrentPosition { get { return _pos; }}

		public override bool RequireSorting { get { return _left.RequireSorting || _right.RequireSorting; } }
	}
}
