// 
// DisInterval.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        public class DisInterval : IntervalBase<DisInterval, Rational> {
                public static readonly DisInterval NotZero =
                        For (Sequence<Interval>.From (Interval.For (Rational.MinusInfinity, -1L),
                                                      Interval.For (1L, Rational.PlusInfinity)));

                static DisInterval cached_bottom;
                static DisInterval cached_top;

                readonly Sequence<Interval> intervals;
                readonly Interval join_interval;
                readonly State state;

                DisInterval (Interval interval)
                        : base (interval.LowerBound, interval.UpperBound)
                {
                        var list = Sequence<Interval>.Empty;
                        if (interval.IsTop)
                                state = State.Top;
                        else if (interval.IsBottom)
                                state = State.Bottom;
                        else {
                                state = State.Normal;
                                list = list.Cons (interval);
                        }

                        intervals = list;
                        join_interval = interval;
                }

                DisInterval (Sequence<Interval> intervals)
                        : base (Rational.MinusInfinity, Rational.PlusInfinity)
                {
                        bool isBottom;
                        this.intervals = Normalize (intervals, out isBottom);

                        if (isBottom) {
                                join_interval = Interval.BottomValue;
                                state = State.Bottom;

                                return;
                        }

                        join_interval = JoinAll (intervals);
                        if (join_interval.IsBottom)
                                state = State.Bottom;
                        else if (join_interval.IsTop)
                                state = intervals.Length () <= 1 ? State.Top : State.Normal;
                        else {
                                LowerBound = join_interval.LowerBound;
                                UpperBound = join_interval.UpperBound;
                                state = State.Normal;
                        }
                }

                DisInterval (State state)
                        : base (Rational.MinusInfinity, Rational.PlusInfinity)
                {
                        this.state = state;
                        join_interval = state == State.Bottom ? Interval.BottomValue : Interval.TopValue;
                        intervals = Sequence<Interval>.Empty;
                }

                public static DisInterval BottomValue { get { return cached_bottom ?? (cached_bottom = new DisInterval (State.Bottom)); } }

                public static DisInterval TopValue { get { return cached_top ?? (cached_top = new DisInterval (State.Top)); } }

                public Interval AsInterval { get { return join_interval; } }

                public override DisInterval Top { get { return TopValue; } }

                public override DisInterval Bottom { get { return BottomValue; } }

                public override bool IsTop { get { return state == State.Top; } }

                public override bool IsBottom { get { return state == State.Bottom; } }

                public bool IsNotZero
                {
                        get
                        {
                                if (!this.IsNormal ())
                                        return false;

                                return intervals.All (intv => !intv.Includes (0));
                        }
                }

                public bool IsPositiveOrZero { get { return this.IsNormal () && LowerBound >= 0L; } }

                public override bool Equals (object other)
                {
                        if (ReferenceEquals (this, other))
                                return true;
                        var that = other as DisInterval;

                        if (that == null) {
                                var intv = other as Interval;
                                if (intv == null)
                                        return false;

                                return Equals (For (intv));
                        }

                        if (state == that.state && join_interval.Equals (that.join_interval))
                                return HaveSameIntervals (intervals, that.intervals);

                        return false;
                }

                static bool HaveSameIntervals (Sequence<Interval> left, Sequence<Interval> right)
                {
                        if (left.Length () != right.Length ())
                                return false;

                        var curLeft = left;
                        var curRight = right;

                        while (!curLeft.IsEmpty ()) {
                                if (!curLeft.Head.Equals (curRight.Head))
                                        return false;
                                curLeft = curLeft.Tail;
                                curRight = curRight.Tail;
                        }

                        return true;
                }

                public override int GetHashCode ()
                {
                        unchecked {
                                return (state.GetHashCode () * 397) ^
                                       (join_interval != null ? join_interval.GetHashCode () : 0);
                        }
                }

                public static Sequence<Interval> Normalize (Sequence<Interval> intervals, out bool isBottom)
                {
                        if (intervals.Length () == 0) {
                                isBottom = false;
                                return intervals;
                        }

                        Comparison<Interval> comparison =
                                (a, b) => a.Equals (b) ? 0 : a.UpperBound <= b.UpperBound ? -1 : 1;

                        var intervalList = new List<Interval> (intervals.AsEnumerable ());
                        intervalList.Sort (comparison);

                        var list = Sequence<Interval>.Empty;

                        var bottomCnt = 0;
                        Interval last = null;

                        foreach (var t in intervalList) {
                                var cur = t;

                                if (cur.IsBottom)
                                        bottomCnt++;
                                else if (cur.IsTop) {
                                        isBottom = false;
                                        return Sequence<Interval>.Empty;
                                }
                                else if (!cur.Equals (last)) {
                                        if (last != null) {
                                                while (list != null) {
                                                        last = list.Head;
                                                        if (Interval.AreConsecutiveIntegers (last, cur)) {
                                                                list = list.Tail;
                                                                cur = last.Join (cur);
                                                        }
                                                        else if (last.LessEqual (cur))
                                                                list = list.Tail;
                                                        else if (last.OverlapsWith (cur)) {
                                                                list = list.Tail;
                                                                cur = cur.Join (last);
                                                        }
                                                        else
                                                                break;
                                                }
                                        }

                                        last = cur;
                                        list = list.Cons (cur);
                                }
                        }

                        isBottom = bottomCnt == intervals.Length ();
                        return list.Reverse ();
                }

                public static Interval JoinAll (Sequence<Interval> list)
                {
                        if (list == null)
                                return Interval.TopValue;

                        var res = list.Head;

                        var cur = list.Tail;
                        while (cur != null) {
                                res = res.Join (cur.Head);
                                cur = cur.Tail;
                        }

                        return res;
                }

                protected override bool IsFiniteBound (Rational n)
                {
                        return !n.IsInfinity;
                }

                public static DisInterval operator + (DisInterval left, DisInterval right)
                {
                        return OperatorLifting (left, right, (a, b) => a + b, true);
                }

                public static DisInterval operator * (DisInterval left, DisInterval right)
                {
                        return OperatorLifting (left, right, (a, b) => a * b, true);
                }

                public static DisInterval operator / (DisInterval left, DisInterval right)
                {
                        return OperatorLifting (left, right, (a, b) => a / b, true);
                }

                public static DisInterval operator - (DisInterval left, DisInterval right)
                {
                        return OperatorLifting (left, right, (a, b) => a - b, true);
                }

                public static DisInterval operator - (DisInterval left)
                {
                        if (left.IsBottom || left.IsTop)
                                return left;

                        return For (left.intervals.Select (i => -i).Reverse ());
                }

                static DisInterval OperatorLifting (DisInterval left, DisInterval right,
                                                    Func<Interval, Interval, Interval> binop, bool propagateTop)
                {
                        if (left.IsBottom || right.IsBottom)
                                return BottomValue;

                        if ((propagateTop && (left.IsTop || right.IsTop)) || (left.IsTop && right.IsTop))
                                return TopValue;

                        var intervals = Sequence<Interval>.Empty;

                        var hasNoNormals = true;

                        if (propagateTop || (left.IsNormal () && right.IsNormal ()))
                                foreach (var leftIntv in left.intervals.AsEnumerable ())
                                        foreach (var rightIntv in right.intervals.AsEnumerable ()) {
                                                var res = binop (leftIntv, rightIntv);
                                                if (res.IsTop)
                                                        return TopValue;
                                                if (res.IsBottom)
                                                        continue;

                                                hasNoNormals = false;
                                                intervals = intervals.Cons (res);
                                        }
                        else {
                                var notTop = left.IsTop ? right : left;
                                var rightIsTop = !left.IsTop;

                                foreach (var intv in notTop.intervals.AsEnumerable ()) {
                                        var res = rightIsTop
                                                          ? binop (intv, Interval.TopValue)
                                                          : binop (Interval.TopValue, intv);

                                        if (res.IsTop)
                                                return TopValue;
                                        if (res.IsBottom)
                                                continue;

                                        hasNoNormals = false;
                                        intervals = intervals.Cons (res);
                                }
                        }

                        return hasNoNormals ? BottomValue : For (intervals.Reverse ());
                }

                public override DisInterval Widen (DisInterval that)
                {
                        if (IsTop || that.IsTop)
                                return TopValue;

                        return new DisInterval (Widen (intervals, that.intervals));
                }

                static Sequence<Interval> Widen (Sequence<Interval> left, Sequence<Interval> right)
                {
                        if (left.IsEmpty () || right.IsEmpty ())
                                return right;
                        if (left.Length () == 1 && right.Length () == 1)
                                return Sequence<Interval>.Singleton (left.Head.Widen (right.Head));

                        if (left.Length () == 1) {
                                if (left.Head.LessEqual (right.Head))
                                        return right;
                                return Sequence<Interval>.Singleton (left.Head.Widen (right.Head.Join (right.Last ())));
                        }

                        if (right.Length () == 1)
                                return Sequence<Interval>.Singleton (left.Head.Join (left.Last ()).Widen (right.Head));

                        var l = left.Head.Widen (right.Head);
                        var r = left.Last ().Widen (right.Last ());

                        var list = Sequence<Interval>.Singleton (l);

                        var curRight = right.Tail;
                        while (curRight != null && curRight.Tail != null) {
                                var curLeft = left.Tail;
                                while (curLeft != null && curRight.Tail != null)
                                        if (curLeft.Head.LessEqual (curRight.Head)) {
                                                list = list.Cons (curRight.Head);
                                                break;
                                        }
                        }
                        list = list.Cons (r);

                        return list.Reverse ();
                }

                public override DisInterval Meet (DisInterval that)
                {
                        DisInterval result;
                        if (this.TryTrivialMeet (that, out result))
                                return result;

                        bool isBottom;
                        var meetIntervals = Meet (intervals, that.intervals, out isBottom);

                        if (isBottom)
                                return BottomValue;
                        if (meetIntervals.Length () == 0)
                                return TopValue;

                        return For (meetIntervals);
                }

                static Sequence<Interval> Meet (Sequence<Interval> left, Sequence<Interval> right, out bool isBottom)
                {
                        isBottom = true;
                        var list = Sequence<Interval>.Empty;
                        foreach (var leftIntv in left.AsEnumerable ()) {
                                foreach (var rightIntv in right.AsEnumerable ()) {
                                        var res = leftIntv.Meet (rightIntv);
                                        if (res.IsNormal ()) {
                                                isBottom = false;
                                                list = list.Cons (res);
                                        }
                                }
                        }

                        return list.Reverse ();
                }

                public override DisInterval ImmutableVersion ()
                {
                        return this;
                }

                public override DisInterval Clone ()
                {
                        return this;
                }

                public override void Dump (TextWriter tw)
                {
                }

                public override bool LessEqual (DisInterval that)
                {
                        bool result;
                        if (this.TryTrivialLessEqual (that, out result))
                                return result;

                        if (!join_interval.LessEqual (that.join_interval))
                                return false;

                        return intervals.AsEnumerable ().All (inv => that.intervals.Any (inv.LessEqual));
                }

                public override DisInterval Join (DisInterval that, bool widening, out bool weaker)
                {
                        weaker = false;
                        return Join (that);
                }

                public override DisInterval Join (DisInterval that)
                {
                        DisInterval result;
                        if (this.TryTrivialJoin (that, out result))
                                return result;

                        var join = Join (intervals, that.intervals);
                        if (join.IsEmpty ())
                                return TopValue;

                        return For (join);
                }

                static Sequence<Interval> Join (Sequence<Interval> left, Sequence<Interval> right)
                {
                        var list = Sequence<Interval>.Empty;

                        var curLeft = left;
                        var curRight = right;

                        while (!curLeft.IsEmpty () && !curRight.IsEmpty ()) {
                                var l = curLeft.Head;
                                var r = curRight.Head;

                                if (l.IsTop || r.IsTop)
                                        return Sequence<Interval>.Empty;

                                if (l.IsBottom)
                                        curLeft = curLeft.Tail;
                                else if (r.IsBottom)
                                        curRight = curRight.Tail;
                                else if (l.LessEqual (r)) {
                                        list = list.Cons (r);
                                        curLeft = curLeft.Tail;
                                        curRight = curRight.Tail;
                                }
                                else if (r.LessEqual (l)) {
                                        list = list.Cons (l);
                                        curLeft = curLeft.Tail;
                                        curRight = curRight.Tail;
                                }
                                else if (r.OverlapsWith (l)) {
                                        list = list.Cons (l.Join (r));
                                        curLeft = curLeft.Tail;
                                        curRight = curRight.Tail;
                                }
                                else if (l.OnTheLeftOf (r)) {
                                        list = list.Cons (l);
                                        curLeft = curLeft.Tail;
                                }
                                else if (r.OnTheLeftOf (l)) {
                                        list = list.Cons (r);
                                        curRight = curRight.Tail;
                                }
                        }

                        while (!curLeft.IsEmpty ()) {
                                list = list.Cons (curLeft.Head);
                                curLeft = curLeft.Tail;
                        }

                        while (!curRight.IsEmpty ()) {
                                list = list.Cons (curRight.Head);
                                curRight = curRight.Tail;
                        }

                        return list.Reverse ();
                }

                public static DisInterval For (Interval interval)
                {
                        return new DisInterval (interval);
                }

                public static DisInterval For (Sequence<Interval> intervals)
                {
                        return new DisInterval (intervals);
                }

                public override string ToString ()
                {
                        if (IsTop)
                                return "Top";
                        if (IsBottom)
                                return this.BottomSymbolIfAny ();
                        if (intervals != null && intervals.Length () == 1)
                                return intervals.Head.ToString ();

                        return string.Format ("({0})", ToString (intervals));
                }

                string ToString (Sequence<Interval> list)
                {
                        if (list == null)
                                return "null";

                        var sb = new StringBuilder ();
                        var first = true;

                        foreach (var intv in list.AsEnumerable ()) {
                                if (first)
                                        first = false;
                                else
                                        sb.Append (" ");

                                sb.Append (intv);
                        }

                        return sb.ToString ();
                }

                public DisInterval Select (Func<Interval, Interval> selector)
                {
                        if (IsBottom)
                                return this;
                        if (IsTop)
                                return new DisInterval (selector (Interval.TopValue));

                        var list = Sequence<Interval>.Empty;

                        for (var cur = intervals; cur != null; cur = cur.Tail) {
                                var intv = selector (cur.Head);
                                if (intv.IsBottom)
                                        return Bottom;
                                if (intv.IsTop)
                                        return Top;

                                list = list.Cons (intv);
                        }

                        return new DisInterval (list.Reverse ());
                }

                public static DisInterval EverythingExcept (DisInterval interval)
                {
                        var left = Interval.For (Rational.MinusInfinity, interval.LowerBound - 1L);
                        var right = Interval.For (interval.UpperBound + 1L, Rational.PlusInfinity);

                        if (left.IsNormal () && right.IsNormal ())
                                return new DisInterval (Sequence<Interval>.From (left, right));

                        if (left.IsNormal ())
                                return new DisInterval (left);

                        if (right.IsNormal ())
                                return new DisInterval (right);

                        return TopValue;
                }

                #region Nested type: State

                enum State {
                        Normal = 0,
                        Top,
                        Bottom
                }

                #endregion
        }
}