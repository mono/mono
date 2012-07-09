using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    abstract class FunctionalAbstractDomain<TThis, TDomain, TCodomain> : IAbstractDomain<TThis>
        where TThis : FunctionalAbstractDomain<TThis, TDomain, TCodomain>
        where TCodomain : IAbstractDomain<TCodomain>
    {
        private Dictionary<TDomain, TCodomain> elements;

        public IEnumerable<KeyValuePair<TDomain, TCodomain>> Elements { get { return this.elements; } }
        public IEnumerable<TDomain> Keys { get { return this.elements.Keys; } }

        protected DomainKind State { get; set; }

        public bool IsBottom { get { return this.State == DomainKind.Bottom; } }

        public bool IsTop
        {
            get
            {
                if (this.elements.Any(pair => !pair.Value.IsTop))
                    return false;

                this.State = DomainKind.Top;
                return true;
            }
        }

        public virtual TCodomain this[TDomain key]
        {
            get { return this.elements[key]; }
            set
            {
                if (value.IsBottom)
                    this.State = DomainKind.Bottom;
                this.elements[key] = value;
            }
        }

        public TThis Join (TThis that)
        {
            if (ReferenceEquals(this, that) || this.IsBottom)
                return that;

            if (that.IsBottom)
                return (TThis)this;

            return FindIntersection (that, (l, r) => l.Join (r), (c) => !c.IsTop, (c) => c.IsBottom, (c, t) => t.Bottom);
        }

        public TThis Join(TThis that, bool widen, out bool weaker)
        {
            weaker = false;
            return Join(that);
        }

        public TThis Widen (TThis that)
        {
            if (this.IsBottom)
                return that;
            if (that.IsBottom)
                return (TThis)this;

            return FindIntersection (that, (l, r) => l.Widen (r), (c) => !c.IsTop, (_) => false, (_, __) => __);
        }

        public TThis Meet(TThis that)
        {
            if (this.IsBottom)
                return (TThis)this;
            if (that.IsBottom)
                return that;

            return FindIntersection (that, (l, r) => l.Meet (r), (c) => true, _ => false, (_, __) => __);
        }

        public bool LessEqual(TThis that)
        {
            bool result;
            if ((this as TThis).TryTrivialLessEqual (that, out result))
                return result;

            foreach (var pair in that.elements)
            {
                var codomainThat = pair.Value;
                if (codomainThat.IsTop)
                    continue;

                TCodomain codomainThis;
                if (!this.elements.TryGetValue(pair.Key, out codomainThis) || !codomainThis.LessEqual(codomainThat))
                    return false;
            }

            return true;
        }

        private TThis FindIntersection (TThis that, Func<TCodomain, TCodomain, TCodomain> operation, 
                                        Predicate<TCodomain> putToResult, Predicate<TCodomain> fastBreak, Func<TCodomain,TThis, TThis> fastBreakResult)
        {
            Dictionary<TDomain, TCodomain> min;
            Dictionary<TDomain, TCodomain> max;
            if (this.elements.Count <= that.elements.Count)
            {
                min = this.elements;
                max = that.elements;
            }
            else
            {
                min = that.elements;
                max = this.elements;
            }

            var res = NewInstance ();
            foreach (var pair in min)
            {
                TCodomain codomain;
                if (max.TryGetValue(pair.Key, out codomain))
                {
                    var elem = operation (pair.Value, codomain);
                    if (fastBreak (elem))
                        return fastBreakResult (elem, res);
                    if (putToResult (elem))
                        res[pair.Key] = elem;
                }
            }

            return res;
        }

        public TThis ImmutableVersion ()
        {
            return (TThis)this;
        }

        public TThis Clone ()
        {
            throw new System.NotImplementedException ();
        }

        public void Dump (TextWriter tw)
        {
            throw new System.NotImplementedException ();
        }

        public int Count
        {
            get { return this.elements.Count; }
        }

        protected FunctionalAbstractDomain ()
        {
            this.elements = new Dictionary<TDomain, TCodomain> ();
            this.State = DomainKind.Normal;
        }

        protected FunctionalAbstractDomain (FunctionalAbstractDomain<TThis,TDomain,TCodomain> that)
        {
            this.elements = new Dictionary<TDomain, TCodomain> (that.elements);
            this.State = that.State;
        }

        public TThis Top
        {
            get
            {
                var @this = this.NewInstance ();
                @this.State = DomainKind.Top;
                return @this;
            }
        }

        public TThis Bottom
        {
            get
            {
                var @this = this.NewInstance ();
                @this.State = DomainKind.Bottom;
                return @this;
            }
        }

        public void ClearElements()
        {
            this.elements.Clear();
        }

        public bool TryGetValue(TDomain key, out TCodomain value)
        {
            return elements.TryGetValue (key, out value);
        }

        protected abstract TThis NewInstance();

        public override string ToString()
        {
            if (this.Count == 0)
                return "empty";

            var sb = new StringBuilder();

            foreach (var key in this.elements.Keys)
                sb.AppendFormat ("{0} -> {1}, ", key, this[key]);

            return sb.ToString();
        }
    }
}