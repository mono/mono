// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
// ***********************************************************************

#if (CLR_2_0 || CLR_4_0) && !NETCF
using System;

namespace NUnit.Framework.Internal
{
    [Category("Generics")]
    [TestFixture(100.0, 42)]
    [TestFixture(42, 100.0)]
    public class DeduceTypeArgsFromArgs<T1, T2>
    {
        T1 t1;
        T2 t2;

        public DeduceTypeArgsFromArgs(T1 t1, T2 t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }

        [TestCase(5, 7)]
        public void TestMyArgTypes(T1 t1, T2 t2)
        {
            Assert.That(t1, Is.TypeOf<T1>());
            Assert.That(t2, Is.TypeOf<T2>());
        }
    }
}
#endif
