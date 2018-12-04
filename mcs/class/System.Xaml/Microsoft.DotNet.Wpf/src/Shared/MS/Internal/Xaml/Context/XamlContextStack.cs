// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace MS.Internal.Xaml.Context
{
    //This stack has the following features:
    //  1) it recycles frames
    //  2) it is <T>, and avoids activator.createinstance with the creationDelegate
    class XamlContextStack<T> where T : XamlFrame
    {
        private int _depth = -1;
        T _currentFrame = null;
        T _recycledFrame = null;
        Func<T> _creationDelegate;

        public XamlContextStack(Func<T> creationDelegate)
        {
            _creationDelegate = creationDelegate;
            Grow();
            _depth = 0;
            Debug.Assert(CurrentFrame != null);
            Debug.Assert(CurrentFrame.Depth == Depth);
        }

        public XamlContextStack(XamlContextStack<T> source, bool copy)
        {
            _creationDelegate = source._creationDelegate;
            _depth = source.Depth;
            if (!copy)
            {
                _currentFrame = source.CurrentFrame;
            }
            else
            {
                T iteratorFrame = source.CurrentFrame;
                T lastFrameInNewStack = null;
                while (iteratorFrame != null)
                {
                    T newFrame = (T)iteratorFrame.Clone();
                    if (_currentFrame == null)
                    {
                        _currentFrame = newFrame;
                    }
                    if (lastFrameInNewStack != null)
                    {
                        lastFrameInNewStack.Previous = newFrame;
                    }
                    lastFrameInNewStack = newFrame;
                    iteratorFrame = (T)iteratorFrame.Previous;
                }
            }
        }

        //allocate a new frame as the new currentFrame;
        private void Grow()
        {
            T lastFrame = _currentFrame;
            _currentFrame = _creationDelegate();
            _currentFrame.Previous = lastFrame;
        }

        public T CurrentFrame
        {
            get { return _currentFrame; }
        }

        public T PreviousFrame
        {
            get { return (T)_currentFrame.Previous; }
        }

        public T PreviousPreviousFrame
        {
            get { return (T)_currentFrame.Previous.Previous; }
        }

        public T GetFrame(int depth)
        {
            T iteratorFrame = _currentFrame;
            Debug.Assert(iteratorFrame != null);
            while (iteratorFrame.Depth > depth)
            {
                iteratorFrame = (T)iteratorFrame.Previous;
            }
            return iteratorFrame;
        }

        //Consumers of this stack call PushScope, and we'll either allocate a new frame
        // or we'll grab one from our recycled linked list.
        public void PushScope()
        {
            if (_recycledFrame == null)
            {
                Grow();
            }
            else //use recycled frame
            {
                T lastFrame = _currentFrame;
                _currentFrame = _recycledFrame;
                _recycledFrame = (T)_recycledFrame.Previous;
                _currentFrame.Previous = lastFrame;
            }
            _depth++;
            Debug.Assert(CurrentFrame.Depth == Depth);
        }

        //Consumers of this stack call PopScope, and we'll move the currentFrame from the main 
        // linked list to the recylced linked list and call .Reset
        public void PopScope()
        {
            _depth--;
            T frameToRecycle = _currentFrame;
            _currentFrame = (T)_currentFrame.Previous;
            frameToRecycle.Previous = _recycledFrame;
            _recycledFrame = frameToRecycle;
            frameToRecycle.Reset();
            Debug.Assert(CurrentFrame.Depth == Depth);
        }

        public int Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        //In case the stack needs to survive and you don't want to keep the recylced frames around.
        public void Trim()
        {
            _recycledFrame = null;
        }

        public string Frames
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                T iteratorFrame = _currentFrame;
                sb.AppendLine("Stack: " + (_currentFrame == null ? -1 : _currentFrame.Depth + 1).ToString(CultureInfo.InvariantCulture) + " frames");
                ShowFrame(sb, _currentFrame);
                return sb.ToString();
            }
        }

        private void ShowFrame(StringBuilder sb, T iteratorFrame)
        {
            if (iteratorFrame == null)
                return;
            if (iteratorFrame.Previous != null)
                ShowFrame(sb, (T)iteratorFrame.Previous);
            sb.AppendLine("  " + iteratorFrame.Depth + " " + iteratorFrame.ToString());
        }
    }
}
