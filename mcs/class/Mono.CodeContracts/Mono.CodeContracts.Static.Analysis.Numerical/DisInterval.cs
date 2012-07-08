using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mono.CodeContracts.Static.Lattices;
using Mono.Collections.Generic;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    class DisInterval : IntervalBase<DisInterval, Rational>
    {
        public override bool Equals (object other)
        {
            if (ReferenceEquals(this, other))
                return true;
            var that = other as DisInterval;

            if (that == null)
            {
                var intv = other as Interval;
                if (intv == null)
                    return false;

                return Equals (For (intv));
            }

            if (state == that.state && joinInterval.Equals(that.joinInterval))
                return HaveSameIntervals (intervals, that.intervals);

            return false;
        }

        private static bool HaveSameIntervals (IList<Interval> left, IList<Interval> right)
        {
            if (left.Count != right.Count)
                return false;
            
            for (int i = 0; i < left.Count; i++)
            {
                if (!left[i].Equals(right[i]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                return (state.GetHashCode () * 397) ^ (joinInterval != null ? joinInterval.GetHashCode () : 0);
            }
        }

        private enum State
        {
            Normal = 0,
            Top, 
            Bottom
        }
        
        private readonly State state; 
        private readonly IList<Interval> intervals;
        private readonly Interval joinInterval;

        private static DisInterval cachedBottom;
        private static DisInterval cachedTop;

        public static DisInterval BottomValue
        {
            get
            {
                if (cachedBottom == null)
                    cachedBottom = new DisInterval (State.Bottom);
                return cachedBottom;
            }
        }

        public static DisInterval TopValue
        {
            get
            {
                if (cachedTop == null)
                    cachedTop = new DisInterval(State.Top);
                return cachedTop;
            }
        }


        private static readonly IList<Interval> EmptyReadOnlyList = new ReadOnlyCollection<Interval> (new Interval[0]);

        private DisInterval (Interval interval)
            : base(interval.LowerBound, interval.UpperBound)
        {
            var list = new List<Interval> ();
            if (interval.IsTop)
                state = State.Top;
            else if (interval.IsBottom)
                state = State.Bottom;
            else
            {
                state = State.Normal;
                list.Add (interval);
            }

            this.intervals = list;
            this.joinInterval = interval;
        }

        private DisInterval (IList<Interval> intervals)
            : base (Rational.MinusInfinity, Rational.PlusInfinity)
        {
            bool isBottom;
            this.intervals = Normalize(intervals, out isBottom);

            if (isBottom)
            {
                this.joinInterval = Interval.BottomValue;
                this.state = State.Bottom;
                
                return;
            }

            this.joinInterval = JoinAll (intervals);
            if (joinInterval.IsBottom)
                this.state = State.Bottom;
            else if (joinInterval.IsTop)
                this.state = intervals.Count <= 1 ? State.Top : State.Normal;
            else
            {
                this.LowerBound = joinInterval.LowerBound;
                this.UpperBound = joinInterval.UpperBound;
                this.state = State.Normal;
            }
        }

        private DisInterval (State state)
            : base (Rational.MinusInfinity, Rational.PlusInfinity)
        {
            this.state = state;
            this.joinInterval = state == State.Bottom ? Interval.BottomValue : Interval.TopValue;
            this.intervals = EmptyReadOnlyList;
        }

        public static IList<Interval> Normalize (IList<Interval> intervals, out bool isBottom)
        {
            if (intervals.Count == 0)
            {
                isBottom = false;
                return intervals;
            }

            Comparison<Interval> comparison = (a, b) => a.Equals (b) ? 0 : a.UpperBound <= b.UpperBound ? -1 : 1;
            var intervalList = new List<Interval> (intervals);
            intervalList.Sort(comparison);

            var list = new List<Interval>(intervalList.Count); //currently disjoint intervals in order by comparison

            int bottomCnt = 0;
            Interval last = null;

            for (int i = 0; i < intervalList.Count; i++)
            {
                Interval cur = intervalList[i];

                if (cur.IsBottom)
                    bottomCnt++;
                else if (cur.IsTop)
                {
                    isBottom = false;
                    return EmptyReadOnlyList;
                } 
                else if (!cur.Equals (last))
                {
                    if (last != null)
                    {
                        while (list.Count > 0)
                        {
                            last = list[list.Count - 1];
                            if (Interval.AreConsecutiveIntegers (last, cur))
                            {
                                list.RemoveAt (list.Count - 1);
                                cur = last.Join (cur);
                            } 
                            else if (last.LessEqual (cur))
                                list.RemoveAt (list.Count - 1);
                            else if (last.OverlapsWith (cur))
                            {
                                list.RemoveAt (list.Count - 1);
                                cur = cur.Join (last);
                            }
                            else
                                break;
                        }
                    }

                    last = cur;
                    list.Add (cur);
                }
            }

            isBottom = bottomCnt == intervals.Count;
            return list;
        }

        public static Interval JoinAll (IList<Interval> list)
        {
            if (list.Count == 0)
                return Interval.TopValue;

            Interval res = list[0];
            for (int i = 1; i < list.Count; i++)
                res = res.Join (list[i]);

            return res;
        }

        public Interval AsInterval { get { return joinInterval; } }

        public override DisInterval Top { get { return TopValue; } }

        public override DisInterval Bottom { get { return BottomValue; } }

        public override bool IsTop { get { return state == State.Top; } }

        public override bool IsBottom { get { return state == State.Bottom; } }

        protected override bool IsFiniteBound (Rational n)
        {
            return !n.IsInfinity;
        }

        public static DisInterval operator+ (DisInterval left, DisInterval right)
        {
            return OperatorLifting (left, right, (a, b) => a + b, true);
        }

        public static DisInterval operator *(DisInterval left, DisInterval right)
        {
            return OperatorLifting(left, right, (a, b) => a * b, true);
        }

        public static DisInterval operator /(DisInterval left, DisInterval right)
        {
            return OperatorLifting(left, right, (a, b) => a / b, true);
        }

        public static DisInterval operator -(DisInterval left, DisInterval right)
        {
            return OperatorLifting(left, right, (a, b) => a - b, true);
        }

        public static DisInterval operator -(DisInterval left)
        {
            if (left.IsBottom || left.IsTop)
                return left;

            var list = new List<Interval> (left.intervals.Count);
            
            for (int i = left.intervals.Count - 1; i >= 0; i--)
                list.Add (-left.intervals[i]);

            return For(list);
        }

        private static DisInterval OperatorLifting (DisInterval left, DisInterval right, Func<Interval, Interval, Interval> binop, bool propagateTop)
        {
            if (left.IsBottom || right.IsBottom)
                return BottomValue;

            if ((propagateTop && (left.IsTop || right.IsTop)) || (left.IsTop && right.IsTop))
                return TopValue;

            var intervals = new List<Interval> (left.intervals.Count + right.intervals.Count);

            bool hasNoNormals = true;

            if (propagateTop || (left.IsNormal() && right.IsNormal()))
                foreach (var leftIntv in left.intervals)
                    foreach (var rightIntv in right.intervals)
                    {
                        Interval res = binop (leftIntv, rightIntv);
                        if (res.IsTop)
                            return TopValue;
                        if (res.IsBottom)
                            continue;

                        hasNoNormals = false;
                        intervals.Add (res);
                    }
            else
            {
                var notTop = left.IsTop ? right : left;
                bool rightIsTop = !left.IsTop;

                foreach (var intv in notTop.intervals)
                {
                    var res = rightIsTop ? binop (intv, Interval.TopValue) : binop (Interval.TopValue, intv);

                    if (res.IsTop)
                        return TopValue;
                    if (res.IsBottom)
                        continue;

                    hasNoNormals = false;
                    intervals.Add (res);
                }
            }

            return hasNoNormals ? BottomValue : For (intervals);
        }

        public override DisInterval Meet (DisInterval that)
        {
            DisInterval result;
            if (this.TryTrivialMeet(that, out result))
                return result;

            bool isBottom;
            var intervals = Meet (this.intervals, that.intervals, out isBottom);
            if (isBottom)
                return BottomValue;
            if (intervals.Count == 0)
                return TopValue;

            return For (intervals);
        }

        private static List<Interval> Meet (IList<Interval> left, IList<Interval> right, out bool isBottom)
        {
            isBottom = true;
            var list = new List<Interval> ();
            foreach (var leftIntv in left)
            {
                foreach (var rightIntv in right)
                {
                    var res = leftIntv.Meet (rightIntv);
                    if (res.IsNormal ())
                    {
                        isBottom = false;
                        list.Add (res);
                    }
                }
            }

            return list;
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
            throw new System.NotImplementedException ();
        }

        public override bool LessEqual (DisInterval that)
        {
            bool result;
            if (this.TryTrivialLessEqual(that, out result))
                return result;

            if (!joinInterval.LessEqual(that.joinInterval))
                return false;

            foreach (var leftInv in intervals)
            {
                if (!that.intervals.Any (rightInv => leftInv.LessEqual (rightInv)))
                    return false;
            }

            return true;
        }

        public override DisInterval Join (DisInterval that, bool widening, out bool weaker)
        {
            weaker = false;
            return Join (that);
        }

        public DisInterval Join (DisInterval that)
        {
            DisInterval result;
            if (this.TryTrivialJoin(that, out result))
                return result;

            var intervals = Join (this.intervals, that.intervals);
            if (intervals.Count == 0)
                return TopValue;

            return For (intervals);
        }

        private static IList<Interval> Join (IList<Interval> left, IList<Interval> right)
        {
            var list = new List<Interval> (left.Count + right.Count);
            int indexL = 0;
            int indexR = 0;

            while (indexL < left.Count && indexR < right.Count)
            {
                var l = left[indexL];
                var r = right[indexR];
                if (l.IsTop || r.IsTop)
                    return EmptyReadOnlyList;

                if (l.IsBottom)
                    indexL++;
                else if (r.IsBottom)
                    indexR++;
                else if (l.LessEqual (r))
                {
                    list.Add (r);
                    ++indexL;
                    ++indexR;
                }
                else if (r.LessEqual (l))
                {
                    list.Add (l);
                    ++indexL;
                    ++indexR;
                } else if (r.OverlapsWith (l))
                {
                    list.Add (l.Join (r));
                    ++indexL;
                    ++indexR;
                } else if (l.OnTheLeftOf(r))
                {
                    list.Add (l);
                    ++indexL;
                } else if (r.OnTheLeftOf(r))
                {
                    list.Add (r);
                    ++indexR;
                }
            }

            while (indexL < left.Count) 
                list.Add (left[indexL++]);

            while (indexR < right.Count)
                list.Add(right[indexR++]);

            return list;
        }

        public static DisInterval For (Interval interval)
        {
            return new DisInterval (interval);
        }

        public static DisInterval For(IList<Interval> intervals)
        {
            return new DisInterval (intervals);
        }

        public override string ToString()
        {
            if (this.IsTop)
                return "Top";
            if (this.IsBottom)
                return this.BottomSymbolIfAny ();
            if (this.intervals != null && this.intervals.Count == 1)
                return this.intervals[0].ToString ();

            return string.Format ("({0})", this.ToString (this.intervals));
        }

        private string ToString (IList<Interval> list)
        {
            if (list == null)
                return "null";

            var sb = new StringBuilder ();
            bool first = true;

            foreach (var intv in list)
            {
                if (first)
                    first = false;
                else
                    sb.Append (" ");

                sb.Append (intv);
            }

            return sb.ToString ();
        }
    }
}