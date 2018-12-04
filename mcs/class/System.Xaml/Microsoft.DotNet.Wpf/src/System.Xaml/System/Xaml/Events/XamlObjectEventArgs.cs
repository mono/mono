// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace System.Xaml
{
    public class XamlObjectEventArgs : EventArgs
    {
        public XamlObjectEventArgs(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            Instance = instance;
        }

        internal XamlObjectEventArgs(object instance, Uri sourceBamlUri, int elementLineNumber, int elementLinePosition) :
            this(instance)
        {
            SourceBamlUri = sourceBamlUri;
            ElementLineNumber = elementLineNumber;
            ElementLinePosition = elementLinePosition;
        }

        public object Instance { get; private set; }

        public Uri SourceBamlUri { get; private set; }

        public int ElementLineNumber { get; private set; }

        public int ElementLinePosition { get; private set; }
    }
}
