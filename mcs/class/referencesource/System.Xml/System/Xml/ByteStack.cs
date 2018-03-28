//------------------------------------------------------------------------------
// <copyright file="ByteStack.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System;

namespace System.Xml {

// This stack is designed to minimize object creation for the
// objects being stored in the stack by allowing them to be
// re-used over time.  It basically pushes the objects creating
// a high water mark then as Pop() is called they are not removed
// so that next time Push() is called it simply returns the last
// object that was already on the stack.

    internal class ByteStack {
        private byte[] stack;
        private int growthRate;
        private int top;
        private int size;

        public ByteStack(int growthRate) {
            this.growthRate = growthRate;
            top = 0;
            stack = new byte[growthRate];
            size = growthRate; 
        }

        public void Push(byte data) {
            if (size == top) {
                byte[] newstack = new byte[size + growthRate];
                if (top > 0) {
                    Buffer.BlockCopy(stack, 0, newstack, 0, top);
                }
                stack = newstack;
                size += growthRate;
            }
            stack[top++] = data;
        }

        public byte Pop() {
            if (top > 0) {
                return stack[--top];
            } else {
                return 0;
            }
        }

        public byte Peek() {
            if (top > 0) {
                return stack[top - 1];
            } else {
                return 0;
            }
        }

        public int Length {
            get { 
                return top;
            }
        }
    }
}
