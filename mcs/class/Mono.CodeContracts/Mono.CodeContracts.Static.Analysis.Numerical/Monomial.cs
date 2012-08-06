// 
// Monomial.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using System.Linq;
using System.Text;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    struct Monomial<Variable>
    {
        private static readonly IComparer<Variable> comparer = new ExpressionViaStringComparer<Variable>();

        private readonly Rational coefficient;

        private readonly Sequence<Variable> variables;

        public Rational Coeff { get { return coefficient; } }

        public IEnumerable<Variable> Variables { get { return variables.AsEnumerable (); } }

        public int Degree { get; private set; }

        public bool IsLinear { get { return this.Degree <= 1; } }

        public bool IsConstant { get { return Degree == 0; } }
        
        private Monomial (Rational coeff)
            : this ()
        {
            this.coefficient = coeff;
            this.variables = Sequence<Variable>.Empty;
            this.Degree = 0;
        }

        public Monomial (Variable x)
            : this ()
        {
            this.coefficient = Rational.One;
            this.variables = Sequence<Variable>.Singleton (x);
            this.Degree = 1;
        }

        public Monomial (Rational k, Variable x)
            : this ()
        {
            this.coefficient = k;
            this.variables = k == Rational.Zero ? Sequence<Variable>.Empty : Sequence<Variable>.Singleton (x);
            this.Degree = variables.Length ();
        }

        private Monomial (Rational k, Sequence<Variable> vars)
            : this ()
        {
            this.coefficient = k;
            this.variables = k == Rational.Zero ? Sequence<Variable>.Empty : vars;
            this.Degree = variables.Length ();
        }

        public bool Contains(Variable var)
        {
            return variables.Any ((v) => v.Equals (var));
        }

        public bool IsSingleVariable(out Variable var)
        {
            if (this.Degree == 1)
                return true.With (this.variables.Head, out var);
            
            return false.Without (out var);
        }

        public Monomial<Variable> Rename(Variable x, Variable rename)
        {
            if (!this.Contains(x))
                return this;

            Func<Variable, Variable> renamer = v => v.Equals(x) ? rename : v;

            return From(Coeff, this.variables.Select(renamer));
        }

        public Monomial<Variable> With(Rational coeff)
        {
            return new Monomial<Variable> (coeff, variables);
        }

        public Monomial<Variable> With(Func<Rational, Rational> func)
        {
            return new Monomial<Variable>(func(this.coefficient), variables);
        } 

        public static Monomial<Variable> operator -(Monomial<Variable> m)
        {
            return m.With (-m.coefficient);
        }

        public static Monomial<Variable> From(Rational coeff)
        {
            return new Monomial<Variable> (coeff);
        }

        public static Monomial<Variable> From(Rational coeff, Sequence<Variable> vars)
        {
            return From(coeff, vars, seq => seq.AsEnumerable());
        }

        public static Monomial<Variable> From(Rational coeff, IEnumerable<Variable> vars)
        {
            return From(coeff, vars, seq => seq);
        }

        private static Monomial<Variable> From<T>(Rational coeff, T vars, Func<T, IEnumerable<Variable>> toEnumerable )
        {
            if (coeff == Rational.Zero)
                return new Monomial<Variable>(coeff);

            var list = toEnumerable(vars).ToList();
            list.Sort(comparer);

            return new Monomial<Variable>(coeff, Sequence<Variable>.From(list));
        }

        public override string ToString()
        {
            return string.Format("{0} * {1}", coefficient, VarsToString(variables));
        }

        private static string VarsToString(Sequence<Variable> seq)
        {
            int len = seq.Length();
            var sb = new StringBuilder();
            if (len == 0)
                sb.Append("1");
            else
            {
                sb.Append(seq.Head);
                seq.Tail.ForEach(v => sb.AppendFormat(" * {0}", v));
            }

            return sb.ToString();
        }

        public bool IsEquivalentTo(Monomial<Variable> that)
        {
            if (this.coefficient != that.coefficient || this.Degree != that.Degree)
                return false;

            if (this.Degree == 1)
                return this.variables.Head.Equals(that.variables.Head);

            foreach (var var in this.variables.AsEnumerable())
            {
                if (!that.Contains(var))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;

            var that = obj is Monomial<Variable> ? (Monomial<Variable>) obj : default(Monomial<Variable>);
            return this.IsEquivalentTo(that);
        }

        public override int GetHashCode()
        {
            int res = 0;
            this.variables.ForEach(v => res += v.GetHashCode());
            return res + this.coefficient.GetHashCode();
        }

        public bool IsIntConstant (out long constant)
        {
            if (IsConstant && this.coefficient.IsInteger)
                return true.With ((long) coefficient.NextInt64, out constant);

            return false.Without (out constant);
        }
    }
}