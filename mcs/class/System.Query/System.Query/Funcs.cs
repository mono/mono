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
//
// Authors:
//        Alejandro Serrano "Serras" (trupill@yahoo.es)
//

using System;

namespace System.Query
{
        public delegate TR Func<TR> ();
        public delegate TR Func<T0, TR> (T0 a0);
        public delegate TR Func<T0, T1, TR> (T0 a0, T1 a1);
        public delegate TR Func<T0, T1, T2, TR> (T0 a0, T1 a1, T2 a2);
        public delegate TR Func<T0, T1, T2, T3, TR> (T0 a0, T1 a1, T2 a2, T3 a3);
}
