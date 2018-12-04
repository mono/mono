// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MS.Internal.Xaml.Context
{
    abstract class XamlFrame
    {
        private int _depth;
        private XamlFrame _previous;

        protected XamlFrame()
        {
            _depth = -1;
        }

        // Copy constructor
        protected XamlFrame(XamlFrame source)
        {
            _depth = source._depth;
        }

        public virtual XamlFrame Clone()
        {
            // Clone should only be overridden for the classes that really need it
            // ObjectWriterFrame overrides this so we can reuse the context for 
            // Templates.  
            throw new NotImplementedException();
        }

        // Reset the contents of the Frame so it can be reused in a stack without reallocating.
        // Depth and previous do not change when we reuse the Frame.
        public abstract void Reset();

        public int Depth
        {
            get
            {
                Debug.Assert(_depth != -1, "Context Frame is uninitialized");
                return _depth;
            }
        }

        public XamlFrame Previous
        {
            get { return _previous; }
            set
            {
                _previous = value;
                _depth = (_previous == null) ? 0 : _previous._depth + 1;
            }
        }
    }
}
