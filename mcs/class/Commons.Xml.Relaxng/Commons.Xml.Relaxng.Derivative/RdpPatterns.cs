//
// Commons.Xml.Relaxng.Derivative.RdpPatterns.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

using System;
using System.Collections;
using System.Xml;

namespace Commons.Xml.Relaxng.Derivative
{
	public delegate RdpPattern RdpApplyAfterHandler (RdpPattern p);

	// abstract Pattern
	public abstract class RdpPattern : ICloneable
	{
		internal protected bool NullableComputed;
		internal protected bool IsNullable;
		Hashtable patternPool;

		public abstract RngPatternType PatternType { get; }

		private Hashtable setupTable (RngPatternType type, RdpPattern p)
		{
			// Why?
			if (patternPool == null) {
				patternPool = new Hashtable ();
			}

			Hashtable typePool = (Hashtable) patternPool [type];
			if (typePool == null) {
				typePool = new Hashtable ();
				patternPool [type] = typePool;
			}
			Hashtable pTable = (Hashtable) typePool [p];
			if (pTable == null) {
				pTable = new Hashtable ();
				typePool [p] = pTable;
			}
			return pTable;
		}

		public RdpChoice MakeChoice (RdpPattern p1, RdpPattern p2)
		{
			Hashtable p1Table = setupTable (RngPatternType.Choice, p1);
			if (p1Table [p2] == null) {
				RdpChoice c = new RdpChoice (p1, p2);
				c.setInternTable (this.patternPool);
				p1Table [p2] = c;
			}
			return (RdpChoice) p1Table [p2];
		}

		public RdpPattern MakeGroup (RdpPattern p1, RdpPattern p2)
		{
			Hashtable p1Table = setupTable (RngPatternType.Group, p1);
			if (p1Table [p2] == null) {
				RdpGroup g = new RdpGroup (p1, p2);
				g.setInternTable (this.patternPool);
				p1Table [p2] = g;
			}
			return (RdpGroup) p1Table [p2];
		}

		public RdpInterleave MakeInterleave (RdpPattern p1, RdpPattern p2)
		{
			Hashtable p1Table = setupTable (RngPatternType.Interleave, p1);
			if (p1Table [p2] == null) {
				RdpInterleave i = new RdpInterleave (p1, p2);
				i.setInternTable (this.patternPool);
				p1Table [p2] = i;
			}
			return (RdpInterleave) p1Table [p2];
		}

		public RdpAfter MakeAfter (RdpPattern p1, RdpPattern p2)
		{
			Hashtable p1Table = setupTable (RngPatternType.After, p1);
			if (p1Table [p2] == null) {
				RdpAfter a = new RdpAfter (p1, p2);
				a.setInternTable (this.patternPool);
				p1Table [p2] = a;
			}
			return (RdpAfter) p1Table [p2];
		}

		public RdpOneOrMore MakeOneOrMore (RdpPattern p)
		{
			Hashtable pTable = (Hashtable) patternPool [RngPatternType.OneOrMore];
			if (pTable == null) {
				pTable = new Hashtable ();
				patternPool [RngPatternType.OneOrMore] = pTable;
			}
			if (pTable [p] == null)
				pTable [p] = new RdpOneOrMore (p);
			return (RdpOneOrMore) pTable [p];
		}

		internal void setInternTable (Hashtable ht)
		{
			this.patternPool = ht;

			Hashtable pt = ht [PatternType] as Hashtable;
			if (pt == null) {
				pt = new Hashtable ();
				ht [PatternType] = pt;
			}

			RdpAbstractSingleContent single =
				this as RdpAbstractSingleContent;
			if (single != null) {
				if (pt [single.Child] == null) {
					pt [single.Child] = this;
					single.Child.setInternTable (ht);
				}
				return;
			}

			RdpAbstractBinary binary =
				this as RdpAbstractBinary;
			if (binary != null) {
				Hashtable lTable = setupTable (PatternType, binary.LValue);
				if (lTable [binary.RValue] == null) {
					lTable [binary.RValue] = this;
					binary.LValue.setInternTable (ht);
					binary.RValue.setInternTable (ht);
				}
				return;
			}

			// For rest patterns, only check recursively, without pooling.
			RdpAttribute attr = this as RdpAttribute;
			if (attr != null) {
				attr.Children.setInternTable (ht);
				return;
			}
			RdpElement el = this as RdpElement;
			if (el != null) {
				el.Children.setInternTable (ht);
				return;
			}
			RdpDataExcept dex= this as RdpDataExcept;
			if (dex != null) {
				dex.Except.setInternTable (ht);
				return;
			}

			switch (PatternType) {
			case RngPatternType.Empty:
			case RngPatternType.NotAllowed:
			case RngPatternType.Text:
			case RngPatternType.Data:
			case RngPatternType.Value:
				return;
			}

			throw new InvalidOperationException ();
		}

		internal virtual RdpPattern expandRef (Hashtable defs)
		{
			return this;
		}

		internal virtual RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			return this;
		}

		public abstract bool Nullable { get; }

		public abstract object Clone ();

		public virtual RdpPattern TextDeriv (string s)
		{
			return RdpNotAllowed.Instance;
		}

		public RdpPattern ChildDeriv (RdpChildNode child)
		{
			RdpTextChild tc = child as RdpTextChild;
			RdpElementChild ec = child as RdpElementChild;
			if (tc != null) {
				return TextDeriv (tc.Text);
			} else {
				// complex stuff;-(
				return StartTagOpenDeriv (ec.LocalName, ec.NamespaceURI)
						.AttsDeriv (ec.Attributes)
						.StartTagCloseDeriv ()
					.ChildrenDeriv (ec.ChildNodes)
					.EndTagDeriv ();
			}
		}

		public RdpPattern ListDeriv (string [] list)
		{
			return listDerivInternal (list, 0);
		}

		private RdpPattern listDerivInternal (string [] list, int start)
		{
			if (list.Length <= start)
				return this;
			else
				return this.TextDeriv (list [start]).listDerivInternal (list, start + 1);
		}

		// Choice(this, p)
		public virtual RdpPattern Choice (RdpPattern p)
		{
			if (p is RdpNotAllowed)
				return this;
			else if (this is RdpNotAllowed)
				return p;
			// Atsushi Eno added
//			else if (this == RdpEmpty.Instance && p == RdpEmpty.Instance)
//				return this;
			else
				return MakeChoice (this, p);
		}

		// Group(this, p)
		public virtual RdpPattern Group (RdpPattern p)
		{
			if (p is RdpNotAllowed || this is RdpNotAllowed)
				return RdpNotAllowed.Instance;
			else if (p is RdpEmpty)
				return this;
			else if (this is RdpEmpty)
				return p;
			else
				return MakeGroup (this, p);
		}

		// Interleave(this, p)
		public virtual RdpPattern Interleave (RdpPattern p)
		{
			if (p is RdpNotAllowed || this is RdpNotAllowed)
				return RdpNotAllowed.Instance;
			else if (p is RdpEmpty)
				return this;
			else if (this is RdpEmpty)
				return p;
			else
				return MakeInterleave (this, p);
		}

		// After(this, p)
		public virtual RdpPattern After (RdpPattern p)
		{
			if (this is RdpNotAllowed || p is RdpNotAllowed)
				return RdpNotAllowed.Instance;
			else
				return MakeAfter (this, p);
		}


		// applyAfter((f, p1=this), p2)
		public virtual RdpPattern ApplyAfter (RdpApplyAfterHandler h)
		{
			return RdpNotAllowed.Instance;
		}

		// startTagOpenDeriv (this, qname)
		// startTagOpenDeriv _ qn = NotAllowed (default)
		public virtual RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			return RdpNotAllowed.Instance;
		}

		// attsDeriv (ctx, this, [att])
		// It is equal to "foreach (attr att in attributes) AttDeriv(att)"
		public RdpPattern AttsDeriv (RdpAttributes attributes)
		{
			return attsDerivInternal (attributes, 0);
		}

		RdpPattern attsDerivInternal (RdpAttributes attributes, int start)
		{
			if (attributes.Count <= start)
				return this;
			else {
				RdpAttributeNode attr = 
					(RdpAttributeNode) attributes [start];
				return AttDeriv (attr.LocalName, attr.NamespaceURI, attr.Value).attsDerivInternal (attributes, start + 1);
			}
		}

		// attDeriv(ctx, this, att)
		// attDeriv _ _ _ = NotAllowed
		public virtual RdpPattern AttDeriv (string name, string ns, string value)
		{
			return RdpNotAllowed.Instance;
		}

		public bool ValueMatch (string s)
		{
			return (Nullable && RdpUtil.Whitespace (s)) ||
				TextDeriv (s).Nullable;
		}

		public virtual RdpPattern StartTagCloseDeriv ()
		{
			return this;
		}

		public RdpPattern ChildrenDeriv (RdpChildNodes children)
		{
			return childrenDerivInternal (children, 0);
		}

		RdpPattern childrenDerivInternal (RdpChildNodes children, int start)
		{
			if (children.Count == 0) {
				// childrenDeriv cx p []
				RdpChildNodes c = new RdpChildNodes ();
				c.Add (new RdpTextChild (String.Empty));
				return ChildrenDeriv (c);
			} else if (children.Count == 1 && children [0] is RdpTextChild) {
				// childrenDeriv cx p [(TextNode s)]
				RdpTextChild tc = children [0] as RdpTextChild;
				RdpPattern p1 = ChildDeriv (tc);
				return RdpUtil.Whitespace (tc.Text) ?
					RdpUtil.Choice (this, p1) : p1;
			} else
				// childrenDeriv cx p children
				return stripChildrenDerivInternal (children, start);
		}

		RdpPattern stripChildrenDerivInternal (RdpChildNodes children, int start)
		{
			if (children.Count == start)
				return this;
			else {
				RdpChildNode firstChild =
					children [start] as RdpChildNode;
				RdpPattern p =
					(firstChild.IsNonWhitespaceText) ?
					this : ChildDeriv (firstChild);
				return p.childrenDerivInternal (children, start + 1);
			}
		}

		public RdpPattern OneOrMore ()
		{
			if (PatternType == RngPatternType.NotAllowed)
				return RdpNotAllowed.Instance;
			else
				return MakeOneOrMore (this);
		}

		public virtual RdpPattern EndTagDeriv ()
		{
			return RdpNotAllowed.Instance;
		}

	}

	// Empty
	public class RdpEmpty : RdpPattern
	{
		public RdpEmpty () {}
		static RdpEmpty ()
		{
			instance = new RdpEmpty ();
		}

		public override bool Nullable {
			get { return true; }
		}

		static RdpEmpty instance;
		public static RdpEmpty Instance {
			get { return instance; }
		}

		public override object Clone ()
		{
			return instance;
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Empty; }
		}
	}

	// NotAllowed
	public class RdpNotAllowed : RdpPattern
	{
		public RdpNotAllowed () {}
		static RdpNotAllowed ()
		{
			instance = new RdpNotAllowed ();
		}

		static RdpNotAllowed instance;
		public static RdpNotAllowed Instance {
			get { return instance; }
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RdpPattern ApplyAfter (RdpApplyAfterHandler h)
		{
			return RdpNotAllowed.Instance;
		}

		public override object Clone ()
		{
			return instance;
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.NotAllowed; }
		}
	}

	// Text
	public class RdpText : RdpPattern
	{
		public RdpText () {}
		static RdpText ()
		{
			instance = new RdpText ();
		}

		public override bool Nullable {
			get { return true; }
		}

		static RdpText instance;
		public static RdpText Instance {
			get { return instance; }
		}

		public override RdpPattern TextDeriv (string s)
		{
			return this;
		}

		public override object Clone ()
		{
			return instance;
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Text; }
		}
	}

	// AbstractBinary
	public abstract class RdpAbstractBinary : RdpPattern
	{
		public RdpAbstractBinary (RdpPattern l, RdpPattern r)
		{
			this.l = l;
			this.r = r;
		}

		RdpPattern l;
		public RdpPattern LValue {
			get { return l; }
			set { l = value; }
		}

		RdpPattern r;
		public RdpPattern RValue {
			get { return r; }
			set { r = value; }
		}

		bool expanded;
		internal override RdpPattern expandRef (Hashtable defs)
		{
			if (!expanded) {
				l = l.expandRef (defs);
				r = r.expandRef (defs);
			}
			return this;
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (LValue.PatternType == RngPatternType.NotAllowed ||
				RValue.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else if (LValue.PatternType == RngPatternType.Empty) {
				result = true;
				return RValue.reduceEmptyAndNotAllowed (ref result, visited);
			} else if (RValue.PatternType == RngPatternType.Empty) {
				result = true;
				return LValue.reduceEmptyAndNotAllowed (ref result, visited);
			} else {
				LValue = LValue.reduceEmptyAndNotAllowed (ref result, visited);
				RValue = RValue.reduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}
	}

	// Choice
	public class RdpChoice : RdpAbstractBinary
	{
		public RdpChoice (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get {
				if (!NullableComputed) {
					IsNullable =
						LValue.Nullable || RValue.Nullable;
					NullableComputed = true;
				}
				return IsNullable;
			}
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (LValue.PatternType == RngPatternType.NotAllowed &&
				RValue.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else if (LValue.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return RValue.reduceEmptyAndNotAllowed (ref result, visited);
			} else if (RValue.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return LValue.reduceEmptyAndNotAllowed (ref result, visited);
			} else if (LValue.PatternType == RngPatternType.Empty &&
				RValue.PatternType == RngPatternType.Empty) {
				result = true;
				return RdpEmpty.Instance;
			} else if (RValue.PatternType == RngPatternType.Empty) {
				result = true;
				RValue = LValue.reduceEmptyAndNotAllowed (ref result, visited);
				LValue = RdpEmpty.Instance;
				return this;
			} else {
				LValue = LValue.reduceEmptyAndNotAllowed (ref result, visited);
				RValue = RValue.reduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s)
		{
			return LValue.TextDeriv (s).Choice (RValue.TextDeriv (s));
		}

		public override RdpPattern ApplyAfter (RdpApplyAfterHandler handler)
		{
//			return handler (LValue).Choice (handler (RValue));
			return LValue.ApplyAfter (handler).Choice (RValue.ApplyAfter (handler));
		}

		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
#if UseStatic
			return RdpUtil.Choice (
				RdpUtil.StartTagOpenDeriv (LValue, qname),
				RdpUtil.StartTagOpenDeriv (RValue, qname));
#else
			RdpPattern lDeriv = LValue.StartTagOpenDeriv (name, ns);
			return lDeriv.Choice (RValue.StartTagOpenDeriv (name, ns));
#endif
		}

		// attDeriv cx (Choice p1 p2) att =
		//  choice (attDeriv cx p1 att) (attDeriv cx p2 att)
		public override RdpPattern AttDeriv (string name, string ns, string value)
		{
			return LValue.AttDeriv (name, ns, value)
				.Choice (RValue.AttDeriv (name, ns, value));
		}

		// startTagCloseDeriv (Choice p1 p2) =
		//  choice (startTagCloseDeriv p1) (startTagCloseDeriv p2)
		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ()
				.Choice (RValue.StartTagCloseDeriv ());
		}

		public override RdpPattern EndTagDeriv ()
		{
			return LValue.EndTagDeriv ().Choice (RValue.EndTagDeriv ());
		}

		public override object Clone ()
		{
			return new RdpChoice (this.LValue.Clone () as RdpPattern,
				this.RValue.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Choice; }
		}
	}

	// Interleave
	public class RdpInterleave : RdpAbstractBinary
	{
		public RdpInterleave (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get {
				if (!NullableComputed) {
					IsNullable =
						LValue.Nullable && RValue.Nullable;
					NullableComputed = true;
				}
				return IsNullable;
			}
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (LValue.PatternType == RngPatternType.NotAllowed ||
				RValue.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else {
				LValue = LValue.reduceEmptyAndNotAllowed (ref result, visited);
				RValue = RValue.reduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s)
		{
			return LValue.TextDeriv (s).Interleave (RValue)
				.Choice (LValue.Interleave (RValue.TextDeriv (s)));
		}

		// => choice (applyAfter (flip interleave p2) (startTagOpenDeriv p1 qn)) (applyAfter (interleave p1) (startTagOpenDeriv p2 qn)
		// => p1.startTagOpenDeriv(qn).applyAfter (flip interleave p2).choice (p2.startTagOpenDeriv(qn).applyAfter (interleave p1) )
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern handledL = LValue.StartTagOpenDeriv (name, ns);
			RdpPattern handledR = RValue.StartTagOpenDeriv (name, ns);
			RdpFlip flipL = new RdpFlip (new RdpBinaryFunction (RdpUtil.Interleave), RValue);
			// FIXME: It is not flip in fact, but there are no proper class ;-(
			RdpFlip flipR = new RdpFlip (new RdpBinaryFunction (RdpUtil.Interleave), handledR);
			RdpPattern choiceL = handledL.ApplyAfter (new RdpApplyAfterHandler (flipL.Apply));
			RdpPattern choiceR = LValue.ApplyAfter (new RdpApplyAfterHandler (flipR.Apply));

			return choiceL.Choice (choiceR);
		}

		// attDeriv cx (Interleave p1 p2) att =
		//  choice (interleave (attDeriv cx p1 att) p2)
		//         (interleave p1 (attDeriv cx p2 att))
		public override RdpPattern AttDeriv (string name, string ns, string value)
		{
			return LValue.AttDeriv (name, ns, value)
				.Interleave (RValue)
				.Choice (LValue.Interleave (
					RValue.AttDeriv (name, ns, value)));
		}

		// startTagCloseDeriv (Interleave p1 p2) =
		//  interleave (startTagCloseDeriv p1) (startTagCloseDeriv p2)
		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ()
				.Interleave (RValue.StartTagCloseDeriv ());
		}

		/*
		// FIXME: This is not specified in James Clark's algorithm, so
		// this may raise unstable behaviour!!
		// I think this is right but not confident.
		
		// ... then I reminded to include it.
		public override RdpPattern EndTagDeriv ()
		{
			return LValue.Nullable ? RValue : RdpNotAllowed.Instance;
		}
		*/

		public override object Clone ()
		{
			return new RdpInterleave (this.LValue.Clone () as RdpPattern,
				this.RValue.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Interleave; }
		}
	}

	// Group
	public class RdpGroup : RdpAbstractBinary
	{
		public RdpGroup (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get {
				if (!NullableComputed) {
					IsNullable =
						LValue.Nullable && RValue.Nullable;
					NullableComputed = true;
				}
				return IsNullable;
			}
		}

		public override RdpPattern TextDeriv (string s)
		{
			RdpPattern p = LValue.TextDeriv (s).Group (RValue);
			return p.Nullable ?
				p.Choice (RValue.TextDeriv(s)) : p;
		}

		// startTagOpenDeriv (Group p1 p2) qn =
		//  let x = applyAfter (flip group p2) (startTagOpenDeriv p1 qn)
		//  in if nullable p1 then
		//       choice x (startTagOpenDeriv p2 qn)
		//     else
		//       x
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern handled = LValue.StartTagOpenDeriv (name, ns);
			RdpFlip f = new RdpFlip (new RdpBinaryFunction (RdpUtil.Group), RValue);
			RdpPattern x = handled.ApplyAfter (new RdpApplyAfterHandler (f.Apply));
			if (LValue.Nullable)
				return x.Choice (RValue.StartTagOpenDeriv (name, ns));
			else
				return x;
		}

		// attDeriv cx (Group p1 p2) att =
		//  choice (group (attDeriv cx p1 att) p2)
		//         (group p1 (attDeriv cx p2 att))
		public override RdpPattern AttDeriv (string name, string ns, string value)
		{
			return LValue.AttDeriv (name, ns, value).Group (RValue)
				.Choice (LValue.Group (
					RValue.AttDeriv (name, ns, value)));
		}

		// startTagCloseDeriv (Group p1 p2) =
		//  group (startTagCloseDeriv p1) (startTagCloseDeriv p2)
		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ()
				.Group (RValue.StartTagCloseDeriv ());
		}

		public override object Clone ()
		{
			return new RdpGroup (this.LValue.Clone () as RdpPattern,
				this.RValue.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Group; }
		}
	}

	public abstract class RdpAbstractSingleContent : RdpPattern
	{
		RdpPattern child;
		bool isExpanded;

		internal override RdpPattern expandRef (Hashtable defs)
		{
			if (!isExpanded)
				child = child.expandRef (defs);
			return this;
		}

		public RdpAbstractSingleContent (RdpPattern p)
		{
			this.child = p;
		}

		public RdpPattern Child {
			get { return child; }
			set { child = value; }
		}

	}

	// OneOrMore
	public class RdpOneOrMore : RdpAbstractSingleContent
	{
		public RdpOneOrMore (RdpPattern p) : base (p)
		{
		}

		public override bool Nullable {
			get { return Child.Nullable; }
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (Child.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else if (Child.PatternType == RngPatternType.Empty)
				return RdpEmpty.Instance;
			else {
				Child = Child.reduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s)
		{
			return Child.TextDeriv (s).Group (Choice (RdpEmpty.Instance));
		}

		// attDeriv cx (OneOrMore p) att =
		//  group (attDeriv cx p att) (choice (OneOrMore p) Empty)
		public override RdpPattern AttDeriv (string name, string ns, string value)
		{
#if UseStatic
			return RdpUtil.Group (
				RdpUtil.AttDeriv (ctx, children, att),
				RdpUtil.Choice (RdpUtil.OneOrMore (children), RdpEmpty.Instance));
#else
			return Child.AttDeriv (name, ns, value)
				.Group (Choice (RdpEmpty.Instance));
#endif
		}

		// startTagOpenDeriv (OneOrMore p) qn =
		//  applyAfter (flip group (choice (OneOrMore p) Empty))
		//             (startTagOpenDeriv p qn)
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern rest = RdpEmpty.Instance.Choice (Child.OneOrMore ());
			RdpPattern handled = Child.StartTagOpenDeriv (name, ns);
			RdpFlip f = new RdpFlip (new RdpBinaryFunction (RdpUtil.Group), rest);
			return handled.ApplyAfter (new RdpApplyAfterHandler (f.Apply));
		}

		// startTagCloseDeriv (OneOrMore p) =
		//  oneOrMore (startTagCloseDeriv p)
		public override RdpPattern StartTagCloseDeriv ()
		{
#if UseStatic
			return RdpUtil.OneOrMore (
				RdpUtil.StartTagCloseDeriv (children));
#else
			return Child.StartTagCloseDeriv ().OneOrMore ();
#endif
		}

		public override object Clone ()
		{
			return new RdpOneOrMore (Child.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.OneOrMore; }
		}
	}

	// List
	public class RdpList : RdpAbstractSingleContent
	{
		public RdpList (RdpPattern p) : base (p)
		{
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (Child.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else {
				Child = Child.reduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RdpPattern TextDeriv (string s)
		{
			return Child.TextDeriv (s).Group (Choice (RdpEmpty.Instance));
		}

		public override object Clone ()
		{
			return new RdpList (Child.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.List; }
		}
	}

	// Data
	public class RdpData : RdpPattern
	{
		public RdpData (RdpDatatype dt, RdpParamList pl)
		{
			this.dt = dt;
			this.pl = pl;
		}

		RdpDatatype dt;
		public RdpDatatype Datatype {
			get { return dt; }
		}

		RdpParamList pl;
		public RdpParamList ParamList {
			get { return pl; }
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RdpPattern TextDeriv (string s)
		{
			if (dt.IsAllowed (pl, s))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
		}

		public override object Clone ()
		{
			return new RdpData (dt.Clone () as RdpDatatype,
				pl.Clone () as RdpParamList);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Data; }
		}
	}

	// DataExcept
	public class RdpDataExcept : RdpData
	{
		public RdpDataExcept (RdpDatatype dt, RdpParamList pl, RdpPattern except)
			: base (dt, pl)
		{
//			this.dt = dt;
//			this.pl = pl;
			this.except = except;
		}

//		RdpDatatype dt;
//		public RdpDatatype Datatype {
//			get { return dt; }
//		}

//		RdpParamList pl;
//		public RdpParamList ParamList {
//			get { return pl; }
//		}

		RdpPattern except;
		public RdpPattern Except {
			get { return except; }
			set { except = value; }
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (except.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return new RdpData (this.Datatype, this.ParamList);
			} else {
				except = except.reduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override RdpPattern TextDeriv (string s)
		{
			if (Datatype.IsAllowed (ParamList, s) && !except.TextDeriv (s).Nullable)
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
		}

		public override object Clone ()
		{
			return new RdpDataExcept (Datatype.Clone () as RdpDatatype,
				ParamList.Clone () as RdpParamList,
				this.except.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.DataExcept; }
		}
	}

	// Value
	public class RdpValue : RdpPattern
	{
		public RdpValue (RdpDatatype dt, string value, RdpContext vctx)
		{
			this.dt = dt;
			this.value = value;
			this.vctx = vctx;
		}

		RdpDatatype dt;
		public RdpDatatype Datatype {
			get { return dt; }
		}

		string value;
		public string Value {
			get { return value; }
		}

		RdpContext vctx;
		public RdpContext ValueContext {
			get { return vctx; }
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RdpPattern TextDeriv (string s)
		{
			if (dt.IsTypeEqual (value, vctx, s))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
		}

		public override object Clone ()
		{
			return new RdpValue (dt.Clone () as RdpDatatype,
				this.value, this.vctx.Clone () as RdpContext);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Value; }
		}
	}

	// Attribute
	public class RdpAttribute : RdpPattern
	{
		public RdpAttribute (RdpNameClass nameClass, RdpPattern p)
		{
			this.nameClass = nameClass;
			this.children = p;
		}

		RdpNameClass nameClass;
		public RdpNameClass NameClass {
			get { return nameClass; }
		}

		RdpPattern children;
		public RdpPattern Children {
			get { return children; }
			set { children = value; }
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			if (children.PatternType == RngPatternType.NotAllowed) {
				result = true;
				return RdpNotAllowed.Instance;
			} else {
				children = children.reduceEmptyAndNotAllowed (ref result, visited);
				return this;
			}
		}

		public override bool Nullable {
			get { return false; }
		}

		// attDeriv cx (Attribute nc p) (AttributeNode qn s) =
		//  if contains nc qn && valueMatch cx p s then Empty else NotAllowed
		public override RdpPattern AttDeriv (string name, string ns, string value)
		{
#if UseStatic
			if (RdpUtil.Contains (this.nameClass, att.QName)
				&& RdpUtil.ValueMatch (ctx, this.children, att.Value))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
#else
			if (nameClass.Contains (name, ns) &&
				children.ValueMatch (value))
				return RdpEmpty.Instance;
			else
				return RdpNotAllowed.Instance;
#endif
		}

		// startTagCloseDeriv (Attribute _ _) = NotAllowed
		public override RdpPattern StartTagCloseDeriv ()
		{
			return RdpNotAllowed.Instance;
		}

		public override object Clone ()
		{
			return new RdpAttribute (this.nameClass.Clone () as RdpNameClass,
				children.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Attribute; }
		}
	}

	// Element
	public class RdpElement : RdpPattern
	{
		public RdpElement (RdpNameClass nameClass, RdpPattern p)
		{
			this.nameClass = nameClass;
			this.children = p;
		}

		RdpNameClass nameClass;
		public RdpNameClass NameClass {
			get { return nameClass; }
		}

		RdpPattern children;
		public RdpPattern Children {
			get { return children; }
			set { children = value; }
		}

		bool isExpanded;
		internal override RdpPattern expandRef (Hashtable defs)
		{
			if (!isExpanded) {
				isExpanded = true;
				children = children.expandRef (defs);
			}
			return this;
		}

		internal override RdpPattern reduceEmptyAndNotAllowed (ref bool result, Hashtable visited)
		{
			if (visited.Contains (this))
				return this;
			visited.Add (this, this);

			children = children.reduceEmptyAndNotAllowed (ref result, visited);
			return this;
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
#if UseStatic
			if (RdpUtil.Contains (this.nameClass, qname))
				return RdpUtil.After (this.Children, RdpEmpty.Instance);
			else
				return RdpNotAllowed.Instance;
#else
			return nameClass.Contains (name, ns) ?
				children.After (RdpEmpty.Instance) :
				RdpNotAllowed.Instance;
#endif
		}

		public override object Clone ()
		{
			return new RdpElement (this.nameClass.Clone () as RdpNameClass,
				children.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.Element; }
		}
	}

	// After
	public class RdpAfter : RdpAbstractBinary
	{
		public RdpAfter (RdpPattern l, RdpPattern r) : base (l, r)
		{
		}

		public override bool Nullable {
			get { return false; }
		}

		public override RdpPattern TextDeriv (string s)
		{
			return LValue.TextDeriv (s).After (RValue);
		}

		// startTagOpenDeriv (After p1 p2) qn =
		//   applyAfter (flip after p2) (startTagOpenDeriv p1 qn)
		public override RdpPattern StartTagOpenDeriv (string name, string ns)
		{
			RdpPattern handled = LValue.StartTagOpenDeriv (name, ns);
			RdpFlip f = new RdpFlip (new RdpBinaryFunction (RdpUtil.After), RValue);
			return handled.ApplyAfter (new RdpApplyAfterHandler (
				f.Apply));
		}

		public override RdpPattern ApplyAfter (RdpApplyAfterHandler handler)
		{
			return LValue.After (handler (RValue));
		}

		// attDeriv cx (After p1 p2) att =
		//  after (attDeriv cx p1 att) p2
		public override RdpPattern AttDeriv (string name, string ns, string value)
		{
			return LValue.AttDeriv (name, ns, value).After (RValue);
		}

		public override RdpPattern StartTagCloseDeriv ()
		{
			return LValue.StartTagCloseDeriv ().After (RValue);
		}

		public override RdpPattern EndTagDeriv ()
		{
			return LValue.Nullable ? RValue : RdpNotAllowed.Instance;
		}

		public override object Clone ()
		{
			return new RdpAfter (LValue.Clone () as RdpPattern,
				RValue.Clone () as RdpPattern);
		}

		public override RngPatternType PatternType {
			get { return RngPatternType.After; }
		}
	}
}

