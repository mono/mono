/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc.Async {
    using System;
    using System.Threading;

    public sealed class OperationCounter {

        private int _count;

        public int Count {
            get {
                return Thread.VolatileRead(ref _count);
            }
        }

        public event EventHandler Completed;

        private int AddAndExecuteCallbackIfCompleted(int value) {
            int newCount = Interlocked.Add(ref _count, value);
            if (newCount == 0) {
                OnCompleted();
            }

            return newCount;
        }

        public int Decrement() {
            return AddAndExecuteCallbackIfCompleted(-1);
        }

        public int Decrement(int value) {
            return AddAndExecuteCallbackIfCompleted(-value);
        }

        public int Increment() {
            return AddAndExecuteCallbackIfCompleted(1);
        }

        public int Increment(int value) {
            return AddAndExecuteCallbackIfCompleted(value);
        }

        private void OnCompleted() {
            EventHandler handler = Completed;
            if (handler != null) {
                handler(this, EventArgs.Empty);
            }
        }

    }
}
