//
// System.Xml.XPath.BaseIterator
//
// Author:
//   Piers Haken (piersh@friskit.com)
//
// (C) 2002 Piers Haken
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace System.Xml.XPath
{
	internal abstract class BaseIterator : XPathNodeIterator
	{
		private XmlNamespaceManager _nsm;

		internal BaseIterator (BaseIterator other)
		{
			_nsm = other._nsm;
		}
		internal BaseIterator (XmlNamespaceManager nsm)
		{
			_nsm = nsm;
		}

		public XmlNamespaceManager NamespaceManager
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

	internal class MergedIterator : BaseIterator
	{
		protected ArrayList _iters = new ArrayList ();
		protected int _pos;
		protected int _index;

		public MergedIterator (BaseIterator iter ) : base (iter) {}
		protected MergedIterator (MergedIterator other) : base (other)
		{
			foreach (XPathNodeIterator iter in other._iters)
				_iters.Add (iter.Clone ());
			_pos = other._pos;
			_index = other._index;
		}
		public override XPathNodeIterator Clone () { return new MergedIterator (this); }

		public void Add (BaseIterator iter)
		{
			_iters.Add (iter);
		}

		public override bool MoveNext ()
		{
			while (_index < _iters.Count)
			{
				BaseIterator iter = (BaseIterator) _iters [_index];
				if (iter.MoveNext ())
				{
					_pos ++;
					return true;
				}
				_index ++;
			}
			return false;
		}
		public override XPathNavigator Current
		{
			get
			{
				if (_index >= _iters.Count)
					return null;
				BaseIterator iter = (BaseIterator) _iters [_index];
				return iter.Current;
			}
		}
		public override int CurrentPosition { get { return _pos; }}

		public override bool RequireSorting { get { return true; } }
	}

	internal abstract class SimpleIterator : BaseIterator
	{
		protected readonly BaseIterator _iter;
		protected readonly XPathNavigator _nav;
		protected int _pos;

		public SimpleIterator (BaseIterator iter) : base (iter)
		{
			_iter = iter;
			if (iter.Current != null)
				_nav = iter.Current.Clone ();
		}
		protected SimpleIterator (SimpleIterator other) : base (other)
		{
			if (other._nav == null)
				_iter = (BaseIterator) other._iter.Clone ();
			else
				_nav = other._nav.Clone ();
			_pos = other._pos;
		}
		public SimpleIterator (XPathNavigator nav, XmlNamespaceManager nsm) : base (nsm)
		{
			_nav = nav.Clone ();
		}

		public override XPathNavigator Current { get { return _nav; }}
		public override int CurrentPosition { get { return _pos; }}
	}

	internal class SelfIterator : SimpleIterator
	{
		public SelfIterator (BaseIterator iter) : base (iter) {}
		public SelfIterator (XPathNavigator nav, XmlNamespaceManager nsm) : base (nav, nsm) {}
		protected SelfIterator (SelfIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new SelfIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0)
			{
				_pos = 1;
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
		public NullIterator (XPathNavigator nav, XmlNamespaceManager nsm) : base (nav, nsm) {}
		protected NullIterator (NullIterator other) : base (other) {}
		public override XPathNodeIterator Clone () { return new NullIterator (this); }
		public override bool MoveNext ()
		{
			return false;
		}
	}

	internal class ParensIterator : SimpleIterator
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
	}

	internal class ParentIterator : SimpleIterator
	{
		public ParentIterator (BaseIterator iter) : base (iter) {}
		protected ParentIterator (ParentIterator other) : base (other) {}
		public ParentIterator (XPathNavigator nav, XmlNamespaceManager nsm) : base (nav, nsm) {}
		public override XPathNodeIterator Clone () { return new ParentIterator (this); }
		public override bool MoveNext ()
		{
			if (_pos == 0 && _nav.MoveToParent ())
			{
				_pos = 1;
				return true;
			}
			return false;
		}

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
			if (fSuccess)
				_pos ++;
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
		}
		protected PrecedingSiblingIterator (PrecedingSiblingIterator other) : base (other) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
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
				if (_nav.ComparePosition (startPosition) == XmlNodeOrder.Same) {
					_pos++;
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
		}
		protected AncestorIterator (AncestorIterator other) : base (other)
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
			positions = (ArrayList) other.positions.Clone ();
			nextDepth = other.nextDepth;
		}
		public override XPathNodeIterator Clone () { return new AncestorIterator (this); }
		public override bool MoveNext ()
		{
			if (finished)
				return false;
			if (!started) {
				started = true;
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
					ancestors.MoveToParent ();
					_nav.MoveToParent ();
				}


				positions.Reverse ();

				if (startPosition.NodeType != XPathNodeType.Root) {
					// First time it returns Root
					_pos++;
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
				return true;
			}
			finished = true;
			return false;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override bool RequireSorting { get { return true; } }
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
		}
		protected AncestorOrSelfIterator (AncestorOrSelfIterator other) : base (other) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
			positions = (ArrayList) other.positions.Clone ();
			nextDepth = other.nextDepth;
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
				XPathNavigator ancestors = startPosition.Clone ();
				do {
					int i = 0;
					_nav.MoveToFirst ();
					while (_nav.ComparePosition (ancestors) == XmlNodeOrder.Before) {
						_nav.MoveToNext ();
						i++;
					}
					positions.Add (i);
					ancestors.MoveToParent ();
					_nav.MoveToParent ();
				} while (ancestors.NodeType != XPathNodeType.Root);
				positions.Reverse ();
			}
			if (initialIteration && startPosition.NodeType != XPathNodeType.Root)
				return true;
			else if (nextDepth + 1 == positions.Count) {
				nextDepth++;
				_pos++;
				_nav.MoveTo (startPosition);
				return true;
			}
			else if (nextDepth < positions.Count) {
				int thisTimePos = (int) positions [nextDepth];
				_nav.MoveToFirstChild ();
				for (int i = 0; i < thisTimePos; i++)
					_nav.MoveToNext ();
				nextDepth++;
				_pos++;
				return true;
			}
			finished = true;
			return false;
		}

		public override bool ReverseAxis {
			get { return true; }
		}

		public override bool RequireSorting { get { return true; } }
	}

	internal class DescendantIterator : SimpleIterator
	{
		protected int _depth;
		private bool _finished;

		public DescendantIterator (BaseIterator iter) : base (iter) {}

		protected DescendantIterator (DescendantIterator other) : base (other)
		{
			_depth = other._depth;
		}

		public override XPathNodeIterator Clone () { return new DescendantIterator (this); }

		[MonoTODO]
		public override bool MoveNext ()
		{
			if (_finished)
				return false;

			if (_nav.MoveToFirstChild ())
			{
				_depth ++;
				_pos ++;
				return true;
			}
			while (_depth != 0)
			{
				if (_nav.MoveToNext ())
				{
					_pos ++;
					return true;
				}
				if (!_nav.MoveToParent ())	// should NEVER fail!
					throw new XPathException ("unexpected depth");	// TODO: better message
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
		}

		public override XPathNodeIterator Clone () { return new DescendantOrSelfIterator (this); }

		[MonoTODO]
		public override bool MoveNext ()
		{
			if (_finished)
				return false;

			if (_pos == 0)
			{
				// self
				_pos ++;
				return true;
			}
			if (_nav.MoveToFirstChild ())
			{
				_depth ++;
				_pos ++;
				return true;
			}
			while (_depth != 0)
			{
				if (_nav.MoveToNext ())
				{
					_pos ++;
					return true;
				}
				if (!_nav.MoveToParent ())	// should NEVER fail!
					throw new XPathException ("unexpected depth");	// TODO: better message
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
					return true;
				} else {
					while (_nav.MoveToParent ()) {
						if (_nav.MoveToNext ()) {
							_pos ++;
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
					return true;
				}
				do
				{
					if (_nav.MoveToNext ())
					{
						_pos ++;
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
		}
		protected PrecedingIterator (PrecedingIterator other) : base (other) 
		{
			startPosition = other.startPosition;
			started = other.started;
			finished = other.finished;
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
					return true;
				}
			}
			else if (_nav.MoveToNextNamespace ())
			{
				_pos ++;
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
					return true;
				}
			}
			else if (_nav.MoveToNextAttribute ())
			{
				_pos ++;
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
			if (name != null)
				name = Current.NameTable.Add (name);

			if (ns != null)
				ns = Current.NameTable.Add (ns);
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

#if false
	internal class SlashIterator : BaseIterator
	{
		protected BaseIterator _iterLeft;
		protected BaseIterator _iterRight;
		protected NodeSet _expr;
		protected int _pos;

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
		}
		public override XPathNodeIterator Clone () { return new SlashIterator (this); }

		public override bool MoveNext ()
		{
			while (_iterRight == null || !_iterRight.MoveNext ())
			{
				if (!_iterLeft.MoveNext ())
					return false;
				_iterRight = _expr.EvaluateNodeSet (_iterLeft);
			}
			_pos ++;
			return true;
		}
		public override XPathNavigator Current { 
			get { 
				if (_iterRight == null) return null;
				
				return _iterRight.Current;
			}
		}
		public override int CurrentPosition { get { return _pos; }}
	}
#else
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
			if (_nextIterRight != null)
				_nextIterRight = (BaseIterator) other._nextIterRight.Clone ();
//			_nextIterRight = other._nextIterRight;
		}
		public override XPathNodeIterator Clone () { return new SlashIterator (this); }

		public override bool MoveNext ()
		{
			if (_finished)
				return false;
			if (RequireSorting) {
				if (_pos <= 0) {
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
#if false
				while (_iterRight == null || !_iterRight.MoveNext ())
				{
					if (!_iterLeft.MoveNext ())
						return false;
					_iterRight = _expr.EvaluateNodeSet (_iterLeft);
				}
				_pos ++;
				return true;
#else
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
							/*
							if (_iterList.Count > 0) {
								int last = _iterList.Count -1;
								BaseIterator tmpIter = _iterList.GetByIndex (last) as BaseIterator;
								_iterList.RemoveAt (last);
								switch (tmpIter.Current.ComparePosition (_iterRight.Current)) {
								case XmlNodeOrder.Same:
								case XmlNodeOrder.Before:
									_iterList.Add (_iterRight, _iterRight);
									_iterRight = tmpIter;
									break;
								default:
									_iterList.Add (_iterRight, _iterRight);
									break;
								}
							}
							*/
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
#endif
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
				XPathNavigator nav = _iterRight.Current.Clone ();
				_navStore.Add (nav);
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
	}

#endif

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
				bool fTrue = true;
				
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

	// WARNING: Before using be sure about IEnumerator argument, because
	internal class EnumeratorIterator : BaseIterator
	{
		protected IEnumerator _enum;
		protected int _pos;

		public EnumeratorIterator (BaseIterator iter, IEnumerator enumerator) : base (iter)
		{
			if (!(enumerator is ICloneable))
				throw new ArgumentException ("Target enumerator must be cloneable.");
			_enum = enumerator;
		}
		
		public EnumeratorIterator (IEnumerator enumerator, XmlNamespaceManager nsm) : base (nsm)
		{
			if (!(enumerator is ICloneable))
				throw new ArgumentException ("Target enumerator must be cloneable.");
			_enum = enumerator;
		}

		protected EnumeratorIterator (EnumeratorIterator other) : base (other)
		{
			ICloneable enumClone = other._enum as ICloneable;
			_enum = (IEnumerator) enumClone.Clone ();
			_pos = other._pos;
		}
		public override XPathNodeIterator Clone () { return new EnumeratorIterator (this); }

		public override bool MoveNext ()
		{
			if (!_enum.MoveNext ())
				return false;
			_pos++;
			return true;
		}
		public override XPathNavigator Current { get { return (XPathNavigator) _enum.Current; }}
		public override int CurrentPosition { get { return _pos; }}

		public override bool RequireSorting { get { return true; } }
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
