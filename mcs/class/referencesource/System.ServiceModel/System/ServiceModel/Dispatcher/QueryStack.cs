//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;

    internal struct StackFrame
    {
        internal int basePtr;
        internal int endPtr;

#if NO
        internal StackFrame(int basePtr)
        {
            Fx.Assert(basePtr >= 0, "");
            this.basePtr = basePtr;
            this.endPtr = this.basePtr - 1;
        }
        
        internal StackFrame(int basePtr, int count)
        {
            Fx.Assert(basePtr >= 0, "");
            this.basePtr = basePtr;
            this.endPtr = basePtr + count - 1;
        }
#endif
        internal int Count
        {
            get
            {
                return this.endPtr - this.basePtr + 1;
            }
        }

        internal int EndPtr
        {
#if NO
            get
            {
                return this.endPtr;
            }
#endif
            set
            {
                Fx.Assert(value >= this.basePtr, "");
                this.endPtr = value;
            }
        }
#if NO 
        internal void Clear()
        {
            this.endPtr = this.basePtr - 1;
        }
#endif
        internal int this[int offset]
        {
            get
            {
                Fx.Assert(this.IsValidPtr(this.basePtr + offset), "");
                return this.basePtr + offset;
            }
        }
#if NO
        internal void Set(int basePtr)
        {
            Fx.Assert(basePtr >= 0, "");
            this.basePtr = basePtr;
            this.endPtr = this.basePtr - 1;
        }
#endif
        internal bool IsValidPtr(int ptr)
        {
            return (ptr >= this.basePtr && ptr <= this.endPtr);
        }
    }

    internal struct StackRegion
    {
        internal QueryRange bounds;
        internal int stackPtr;

        internal StackRegion(QueryRange bounds)
        {
            this.bounds = bounds;
            this.stackPtr = bounds.start - 1;
        }
#if NO
        internal StackRegion(QueryRange bounds, int stackPtr)
        {
            Fx.Assert(bounds.IsInRange(stackPtr), "");
            this.bounds = bounds;
            this.stackPtr = stackPtr;
        }
#endif
        internal int Count
        {
            get
            {
                return this.stackPtr - this.bounds.start + 1;
            }
        }
#if NO
        internal bool IsReady
        {
            get
            {
                return this.bounds.IsNotEmpty;
            }
        }
#endif
        internal bool NeedsGrowth
        {
            get
            {
                return (this.stackPtr > this.bounds.end);
            }
        }

        internal void Clear()
        {
            this.stackPtr = this.bounds.start - 1;
        }

        internal void Grow(int growBy)
        {
            this.bounds.end += growBy;
        }

        internal bool IsValidStackPtr()
        {
            return this.bounds.IsInRange(this.stackPtr);
        }

        internal bool IsValidStackPtr(int stackPtr)
        {
            return this.bounds.IsInRange(stackPtr);
        }
#if NO
        internal void Set(int start, int end)
        {
            this.bounds.Set(start, end);
            this.stackPtr += start;
        }
#endif
        internal void Shift(int shiftBy)
        {
            this.bounds.Shift(shiftBy);
            this.stackPtr += shiftBy;
        }
    }

    // The eval stack as well as all its contained data structures are STRUCTs
    // fast to allocate
    internal struct EvalStack
    {
        internal QueryBuffer<Value> buffer;
        internal StackRegion frames;
        internal StackRegion stack;
        internal const int DefaultSize = 2;
        internal bool contextOnTopOfStack;

        internal EvalStack(int frameCapacity, int stackCapacity)
        {
            Fx.Assert(frameCapacity >= 0 && stackCapacity >= 0, "");

            // All structs! Cost of allocation is relatively mild...
            this.buffer = new QueryBuffer<Value>(frameCapacity + stackCapacity);
            this.stack = new StackRegion(new QueryRange(0, stackCapacity - 1));
            this.buffer.Reserve(stackCapacity);
            this.frames = new StackRegion(new QueryRange(stackCapacity, stackCapacity + frameCapacity - 1));
            this.buffer.Reserve(frameCapacity);
            this.contextOnTopOfStack = false;
        }
#if NO 
        internal EvalStack(ref EvalStack stack)
        {
            this.buffer = new QueryBuffer<Value>(stack.buffer);
            this.stackCapacity = stack.stackCapacity;
            this.frameCapacity = stack.frameCapacity;
            this.stack = stack.stack;
            this.frames = stack.frames;
        }
#endif
        internal Value[] Buffer
        {
            get
            {
                return this.buffer.buffer;
            }
        }
#if NO        
        internal int FrameCount
        {
            get
            {
                return this.frames.Count;
            }
        }

        internal int FramePtr
        {
            get
            {
                return this.frames.stackPtr;
            }
        }
#endif
        internal StackFrame this[int frameIndex]
        {
            get
            {
                return this.buffer.buffer[this.frames.stackPtr - frameIndex].Frame;
            }
        }
#if NO        
        internal bool IsReady
        {
            get
            {
                return (this.buffer.count > 0);
            }
        }
#endif
        internal StackFrame SecondArg
        {
            get
            {
                return this[1];
            }
        }
#if NO        
        internal int StackPtr
        {
            get
            {
                return this.stack.stackPtr;
            }
        }
#endif
        internal StackFrame TopArg
        {
            get
            {
                return this[0];
            }
        }

        internal void Clear()
        {
            this.stack.Clear();
            this.frames.Clear();
            this.contextOnTopOfStack = false;
        }

        internal void CopyFrom(ref EvalStack stack)
        {
            this.buffer.CopyFrom(ref stack.buffer);
            this.frames = stack.frames;
            this.stack = stack.stack;
            this.contextOnTopOfStack = stack.contextOnTopOfStack;
        }

        internal int CalculateNodecount()
        {
            if (this.stack.stackPtr < 0)
            {
                return 0;
            }

            StackFrame topFrame = this.TopArg;
            int count = 0;

            for (int i = topFrame.basePtr; i <= topFrame.endPtr; ++i)
            {
                Fx.Assert(this.buffer[i].IsType(ValueDataType.Sequence), "");
                count += this.buffer[i].NodeCount;
            }

            return count;
        }
#if NO        
        internal void Erase()
        {
            this.buffer.Erase();
        }
#endif
        void GrowFrames()
        {
            int growBy = this.frames.Count;
            this.buffer.ReserveAt(this.frames.bounds.end + 1, growBy);
            this.frames.Grow(growBy);
        }

        void GrowStack(int growthNeeded)
        {
            int growBy = this.stack.bounds.Count;
            if (growthNeeded > growBy)
            {
                growBy = growthNeeded;
            }

            this.buffer.ReserveAt(this.stack.bounds.end + 1, growBy);
            this.stack.Grow(growBy);
            this.frames.Shift(growBy);
        }
#if NO        
        internal void Init()
        {
            this.buffer.Reserve(this.stackCapacity);
            this.stack.Set(0, stackCapacity - 1);
            this.buffer.Reserve(this.frameCapacity);
            this.frames.Set(stackCapacity, stackCapacity + frameCapacity - 1);
        }
                
        internal void Init(Value[] buffer, int stackCapacity, int frameCapacity)
        {
            Fx.Assert(null != buffer, "");
            this.stackCapacity = stackCapacity;
            this.frameCapacity = frameCapacity;
            
            this.buffer = new QueryBuffer<Value>(buffer);
            this.stack = new StackRegion(new QueryRange(0, stackCapacity - 1));
            this.buffer.Reserve(stackCapacity);
            this.frames = new StackRegion(new QueryRange(stackCapacity, stackCapacity + frameCapacity - 1));
            this.buffer.Reserve(frameCapacity);
        }
#endif
        internal bool InUse
        {
            get
            {
                if (contextOnTopOfStack)
                    return (this.frames.Count > 1);
                else
                    return (this.frames.Count > 0);
            }
        }

        internal bool PeekBoolean(int index)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            return this.buffer.buffer[index].GetBoolean();
        }

        internal double PeekDouble(int index)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            return this.buffer.buffer[index].GetDouble();
        }
#if NO
        internal int PeekInteger(int index)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            return (int)this.buffer.buffer[index].GetDouble();
        }
#endif
        internal NodeSequence PeekSequence(int index)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            return this.buffer.buffer[index].GetSequence();
        }

        internal string PeekString(int index)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            return this.buffer.buffer[index].GetString();
        }
#if NO
        internal void Pop()
        {
            this.stack.stackPtr--;
            Fx.Assert(this.stack.IsValidStackPtr(), "");
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void PopFrame()
        {
            Fx.Assert(this.frames.IsValidStackPtr(), "");
            this.stack.stackPtr = this.buffer.buffer[this.frames.stackPtr].StackPtr;
            this.frames.stackPtr--;
        }
#endif
        internal void PopFrame(ProcessingContext context)
        {
            Fx.Assert(this.frames.IsValidStackPtr(), "");

            StackFrame topArg = this.TopArg;
            for (int i = topArg.basePtr; i <= topArg.endPtr; ++i)
            {
                this.buffer.buffer[i].Clear(context);
            }

            this.stack.stackPtr = topArg.basePtr - 1;
            this.frames.stackPtr--;
        }

        internal void PushFrame()
        {
            this.frames.stackPtr++;
            if (this.frames.NeedsGrowth)
            {
                this.GrowFrames();
            }

            //
            // The first element in the new frame will be the NEXT item pushed onto the stack
            // We save offsets because stacks may get moved and repositioned
            //
            this.buffer.buffer[this.frames.stackPtr].StartFrame(this.stack.stackPtr);
        }

        internal void PopSequenceFrameTo(ref EvalStack dest)
        {
            StackFrame topFrame = this.TopArg;

            dest.PushFrame();
            int argCount = topFrame.Count;
            switch (argCount)
            {
                default:
                    dest.Push(this.buffer.buffer, topFrame.basePtr, argCount);
                    break;

                case 0:
                    break;

                case 1:
                    dest.Push(this.buffer.buffer[topFrame.basePtr].Sequence);
                    break;
            }

            // Pop original fame 
            this.stack.stackPtr = topFrame.basePtr - 1;
            this.frames.stackPtr--;
        }
#if NO
        internal void Push()
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(int count)
        {
            this.stack.stackPtr += count;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(count);
            }
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }
#endif

        internal void Push(string val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }

            this.buffer.buffer[this.stack.stackPtr].String = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(string val, int addCount)
        {
            int stackPtr = this.stack.stackPtr;
            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }

            int stackMax = stackPtr + addCount;
            while (stackPtr < stackMax)
            {
                this.buffer.buffer[++stackPtr].String = val;
            }

            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(bool val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }

            this.buffer.buffer[this.stack.stackPtr].Boolean = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(bool val, int addCount)
        {
            int stackPtr = this.stack.stackPtr;

            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }

            int stackMax = stackPtr + addCount;

            while (stackPtr < stackMax)
            {
                this.buffer.buffer[++stackPtr].Boolean = val;
            }

            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(double val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }

            this.buffer.buffer[this.stack.stackPtr].Double = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(double val, int addCount)
        {
            int stackPtr = this.stack.stackPtr;

            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }

            int stackMax = stackPtr + addCount;

            while (stackPtr < stackMax)
            {
                this.buffer.buffer[++stackPtr].Double = val;
            }

            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(NodeSequence val)
        {
            this.stack.stackPtr++;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(1);
            }

            this.buffer.buffer[this.stack.stackPtr].Sequence = val;
            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(NodeSequence val, int addCount)
        {
            // One of the addCount refs was added by the call to CreateSequence
            val.refCount += addCount - 1;

            int stackPtr = this.stack.stackPtr;

            this.stack.stackPtr += addCount;
            if (this.stack.NeedsGrowth)
            {
                this.GrowStack(addCount);
            }

            int stackMax = stackPtr + addCount;

            while (stackPtr < stackMax)
            {
                this.buffer.buffer[++stackPtr].Sequence = val;
            }

            this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
        }

        internal void Push(Value[] buffer, int startAt, int addCount)
        {
            if (addCount > 0)
            {
                int stackPtr = this.stack.stackPtr + 1;
                this.stack.stackPtr += addCount;
                if (this.stack.NeedsGrowth)
                {
                    this.GrowStack(addCount);
                }

                if (1 == addCount)
                {
                    this.buffer.buffer[stackPtr] = buffer[startAt];
                }
                else
                {
                    Array.Copy(buffer, startAt, this.buffer.buffer, stackPtr, addCount);
                }
                this.buffer.buffer[this.frames.stackPtr].FrameEndPtr = this.stack.stackPtr;
            }
        }
#if NO
        internal void Push(ref EvalStack source)
        {
            this.Push(source.buffer.buffer, source.stack.bounds.start, source.frames.bounds.end + 1);
        }
#endif
        internal void ReplaceAt(int index, NodeSequence seq)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index) && this.buffer.buffer[index].IsType(ValueDataType.Sequence), "");
            this.buffer.buffer[index].Sequence = seq;
        }

        internal void SetValue(ProcessingContext context, int index, bool val)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            this.buffer.buffer[index].Update(context, val);
        }

        internal void SetValue(ProcessingContext context, int index, double val)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            this.buffer.buffer[index].Update(context, val);
        }

        internal void SetValue(ProcessingContext context, int index, string val)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            this.buffer.buffer[index].Update(context, val);
        }

        internal void SetValue(ProcessingContext context, int index, NodeSequence val)
        {
            Fx.Assert(this.stack.IsValidStackPtr(index), "");
            this.buffer.buffer[index].Update(context, val);
        }

        internal void TransferPositionsTo(ref EvalStack stack)
        {
            StackFrame arg = this.TopArg;

            stack.PushFrame();
            for (int i = arg.basePtr; i <= arg.endPtr; ++i)
            {
                NodeSequence seq = this.buffer.buffer[i].Sequence;
                int nodeCount = seq.Count;

                if ((this.stack.stackPtr + nodeCount) > this.stack.bounds.end)
                {
                    this.GrowStack(nodeCount);
                }

                for (int n = 0; n < nodeCount; ++n)
                {
                    stack.Push((double)seq.Items[n].Position);
                }
            }
        }

        internal void TransferSequenceSizeTo(ref EvalStack stack)
        {
            StackFrame arg = this.TopArg;

            stack.PushFrame();
            for (int i = arg.basePtr; i <= arg.endPtr; ++i)
            {
                NodeSequence seq = this.buffer.buffer[i].Sequence;
                int nodeCount = seq.Count;

                if ((this.stack.stackPtr + nodeCount) > this.stack.bounds.end)
                {
                    this.GrowStack(nodeCount);
                }

                for (int n = 0; n < nodeCount; ++n)
                {
                    stack.Push((double)NodeSequence.GetContextSize(seq, n));
                }
            }
        }
#if NO        
        internal void Trim()
        {
            this.buffer.TrimToCount();
        }
#endif
    }
#if NO 
    internal struct BoundedStack<T>
    {
        QueryBuffer<T> buffer;
        int maxSize;
        
        internal BoundedStack(int capacity)
        {
            this.buffer = new QueryBuffer<T>(0);
            this.maxSize = capacity;
        }
        
        internal bool HasItems
        {
            get
            {
                return (this.buffer.count > 0);
            }
        }
        
        internal bool HasSpace
        {
            get
            {
                return (this.buffer.count < this.maxSize);
            }
        }
        
        internal int MaxSize
        {
            get
            {
                return this.maxSize;
            }
            set
            {
                Fx.Assert(value >= 0, "");
                this.maxSize = value;
                if (value < this.buffer.count)
                {
                    this.buffer.count = value;
                    this.buffer.TrimToCount();
                }
            }
        }
                
        internal T Pop()
        {
            return this.buffer.Pop();
        }
        
        internal void Push(T t)
        {
            if (this.buffer.count == this.maxSize)
            {
                return;
            }
            
            this.buffer.Push(t);
        }
        
        internal void Trim()
        {
            this.buffer.TrimToCount();
        }
    }
#endif
}
