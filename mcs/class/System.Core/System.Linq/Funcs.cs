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
// MERCHANTABILITY, FITNESS FOR TArg PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Alejandro Serrano "Serras" (trupill@yahoo.es)
//

namespace System.Linq
{
        public delegate TResult Func<TResult> ();
        public delegate TResult Func<TArg0, TResult> (TArg0 arg0);
        public delegate TResult Func<TArg0, TArg1, TResult> (TArg0 arg0, TArg1 arg1);
        public delegate TResult Func<TArg0, TArg1, TArg2, TResult> (TArg0 arg0, TArg1 arg1, TArg2 arg2);
        public delegate TResult Func<TArg0, TArg1, TArg2, TArg3, TResult> (TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3);
}
