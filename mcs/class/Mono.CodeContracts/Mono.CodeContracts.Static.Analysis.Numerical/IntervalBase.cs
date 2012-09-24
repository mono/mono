// 
// IntervalBase.cs
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

using System.IO;

using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        /// <summary>
        /// Represents a generic class for intervals on numeric values.
        /// </summary>
        public abstract class IntervalBase<TInterval, TNumeric> : IAbstractDomain<TInterval>
                where TInterval : IntervalBase<TInterval, TNumeric> {
                protected IntervalBase (TNumeric lowerBound, TNumeric upperBound)
                {
                        LowerBound = lowerBound;
                        UpperBound = upperBound;
                }

                public TNumeric UpperBound { get; protected set; }
                public TNumeric LowerBound { get; protected set; }

                public abstract TInterval Top { get; }
                public abstract TInterval Bottom { get; }

                public abstract bool IsTop { get; }
                public abstract bool IsBottom { get; }

                public bool IsSinglePoint { get { return this.IsNormal () && LowerBound.Equals (UpperBound); } }
                public bool IsFinite { get { return this.IsNormal () && IsFiniteBound (LowerBound) && IsFiniteBound (UpperBound); } }

                public abstract bool LessEqual (TInterval that);

                public abstract TInterval Join (TInterval that, bool widening, out bool weaker);
                public abstract TInterval Join (TInterval that);

                public abstract TInterval Widen (TInterval that);
                public abstract TInterval Meet (TInterval that);

                public abstract TInterval ImmutableVersion ();
                public abstract TInterval Clone ();

                public abstract void Dump (TextWriter tw);

                public override string ToString ()
                {
                        return string.Format ("[{0}, {1}]{2}", LowerBound, UpperBound, this.BottomSymbolIfAny ());
                }

                protected abstract bool IsFiniteBound (TNumeric n);
       }
}