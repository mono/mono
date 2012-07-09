#region Copyright Header
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
#endregion

using System.Collections.Generic;
using System.Linq;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    internal struct Monomial<Variable>
    {
        private static readonly IComparer<Variable> comparer; // todo:

        private readonly Rational coefficient;

        private readonly Sequence<Variable> variables;

        public Rational Coeff { get { return coefficient; } }

        public IEnumerable<Variable> Variables { get { return variables.AsEnumerable (); } }

        public int Degree { get; private set; }

        public bool IsLinear { get { return this.Degree <= 1; } }

        public bool IsConstant { get { return Degree == 0; } }

        public Monomial (int k)
            : this (Rational.For (k))
        {
        }

        public Monomial (Rational coeff)
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

        public Monomial (Rational k, IEnumerable<Variable> vars)
            : this ()
        {
            this.coefficient = k;
            if (k == Rational.Zero)
                this.variables = Sequence<Variable>.Empty;
            else
            {
                var list = vars.ToList ();
                list.Sort (comparer);

                this.variables = Sequence<Variable>.From (list);
            }
            this.Degree = variables.Length ();
        }

        public Monomial(Rational k, Sequence<Variable> vars)
            : this ()
        {
            this.coefficient = k;
            this.variables = vars;
            this.Degree = vars.Length ();
        }

        public bool Contains(Variable var)
        {
            return variables.Any ((v) => v.Equals (var));
        }

        public bool IsVariable(out Variable var)
        {
            if (this.Degree == 1)
                return true.With (this.variables.Head, out var);
            
            return false.Without (out var);
        }

        public Monomial<Variable> Rename(Variable x, Variable rename)
        {
            if (!this.Contains ())
        } 

        public static Monomial<Variable> operator -(Monomial<Variable> m)
        {
            return new Monomial<Variable> (-m.Coeff, m.variables);
        }
    }
}