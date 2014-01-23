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

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        struct Monomial<TVar> {
                static readonly IComparer<TVar> Comparer = new ExpressionViaStringComparer<TVar> ();

                readonly Rational coefficient;

                readonly Sequence<TVar> variables;

                public Rational Coeff { get { return coefficient; } }

                public IEnumerable<TVar> Variables { get { return variables.AsEnumerable (); } }

                public int Degree { get; private set; }

                public bool IsLinear { get { return Degree <= 1; } }

                public bool IsConstant { get { return Degree == 0; } }

                Monomial (Rational coeff)
                        : this ()
                {
                        coefficient = coeff;
                        variables = Sequence<TVar>.Empty;
                        Degree = 0;
                }

                private Monomial (TVar x)
                        : this ()
                {
                        coefficient = Rational.One;
                        variables = Sequence<TVar>.Singleton (x);
                        Degree = 1;
                }

                private Monomial (Rational k, TVar x)
                        : this ()
                {
                        coefficient = k;
                        variables = k == Rational.Zero ? Sequence<TVar>.Empty : Sequence<TVar>.Singleton (x);
                        Degree = variables.Length ();
                }

                Monomial (Rational k, Sequence<TVar> vars)
                        : this ()
                {
                        coefficient = k;
                        variables = k == Rational.Zero ? Sequence<TVar>.Empty : vars;
                        Degree = variables.Length ();
                }

                public bool Contains (TVar var)
                {
                        return variables.Any (v => v.Equals (var));
                }

                public bool IsSingleVariable (out TVar var)
                {
                        if (Degree == 1)
                                return true.With (variables.Head, out var);

                        return false.Without (out var);
                }

                public Monomial<TVar> Rename (TVar x, TVar rename)
                {
                        if (!Contains (x))
                                return this;

                        Func<TVar, TVar> renamer = v => v.Equals (x) ? rename : v;

                        return From (Coeff, variables.Select (renamer));
                }

                public Monomial<TVar> With (Rational coeff)
                {
                        return new Monomial<TVar> (coeff, variables);
                }

                public Monomial<TVar> With (Func<Rational, Rational> func)
                {
                        return new Monomial<TVar> (func (coefficient), variables);
                }

                public static Monomial<TVar> operator - (Monomial<TVar> m)
                {
                        return m.With (-m.coefficient);
                }

                public static Monomial<TVar> From (Rational coeff)
                {
                        return new Monomial<TVar> (coeff);
                }

                public static Monomial<TVar> From (Rational coeff, Sequence<TVar> vars)
                {
                        return From (coeff, vars, seq => seq.AsEnumerable ());
                }

                public static Monomial<TVar> From (Rational coeff, IEnumerable<TVar> vars)
                {
                        return From (coeff, vars, seq => seq);
                }

                static Monomial<TVar> From<T> (Rational coeff, T vars, Func<T, IEnumerable<TVar>> toEnumerable)
                {
                        if (coeff == Rational.Zero)
                                return new Monomial<TVar> (coeff);

                        var list = toEnumerable (vars).ToList ();
                        list.Sort (Comparer);

                        return new Monomial<TVar> (coeff, Sequence<TVar>.From (list));
                }

                public override string ToString ()
                {
                        return string.Format ("{0} * {1}", coefficient, VarsToString (variables));
                }

                static string VarsToString (Sequence<TVar> seq)
                {
                        var len = seq.Length ();
                        var sb = new StringBuilder ();
                        if (len == 0)
                                sb.Append ("1");
                        else {
                                sb.Append (seq.Head);
                                seq.Tail.ForEach (v => sb.AppendFormat (" * {0}", v));
                        }

                        return sb.ToString ();
                }

                public bool IsEquivalentTo (Monomial<TVar> that)
                {
                        if (coefficient != that.coefficient || Degree != that.Degree)
                                return false;

                        if (Degree == 1)
                                return variables.Head.Equals (that.variables.Head);

                        foreach (var var in variables.AsEnumerable ()) {
                                if (!that.Contains (var))
                                        return false;
                        }

                        return true;
                }

                public override bool Equals (object obj)
                {
                        if (ReferenceEquals (obj, null))
                                return false;

                        if (obj is Monomial<TVar>)
                                return IsEquivalentTo ((Monomial<TVar>) obj);
                        return false;
                }

                public override int GetHashCode ()
                {
                        var res = 0;
                        variables.ForEach (v => res += v.GetHashCode ());
                        return res + coefficient.GetHashCode ();
                }

                public bool IsIntConstant (out long constant)
                {
                        if (IsConstant && coefficient.IsInteger)
                                return true.With ((long) coefficient.NextInt64, out constant);

                        return false.Without (out constant);
                }
        }
}