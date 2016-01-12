// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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

#if !CLR_4_0 || SILVERLIGHT
using System.Threading;

namespace NUnit.Framework.Internal.WorkItems
{
    /// <summary>
    /// A simplified implementation of .NET 4 CountdownEvent
    /// for use in earlier versions of .NET. Only the methods
    /// used by NUnit are implemented.
    /// </summary>
    public class CountdownEvent
    {
        int _initialCount;
        int _remainingCount;
        object _lock = new object();
        ManualResetEvent _event = new ManualResetEvent(false);

        /// <summary>
        /// Construct a CountdownEvent
        /// </summary>
        /// <param name="initialCount">The initial count</param>
        public CountdownEvent(int initialCount)
        {
            _initialCount = _remainingCount = initialCount;
        }

        /// <summary>
        /// Gets the initial count established for the CountdownEvent
        /// </summary>
        public int InitialCount
        {
            get { return _initialCount; }
        }

        /// <summary>
        /// Gets the current count remaining for the CountdownEvent
        /// </summary>
        public int CurrentCount
        {
            get { return _remainingCount; }
        }

        /// <summary>
        /// Decrement the count by one
        /// </summary>
        public void Signal()
        {
            lock (_lock)
            {
                if (--_remainingCount == 0)
                    _event.Set();
            }
        }

        /// <summary>
        /// Block the thread until the count reaches zero
        /// </summary>
        public void Wait()
        {
            _event.WaitOne();
        }
    }
}
#endif
