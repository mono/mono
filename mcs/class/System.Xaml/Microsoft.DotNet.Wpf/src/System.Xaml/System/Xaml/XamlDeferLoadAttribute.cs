// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace System.Windows.Markup
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class XamlDeferLoadAttribute : Attribute
    {
        string _contentTypeName;
        string _loaderTypeName;

        public XamlDeferLoadAttribute(Type loaderType, Type contentType)
        {
            if (loaderType == null)
            {
                throw new ArgumentNullException("loaderType");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            _loaderTypeName = loaderType.AssemblyQualifiedName;
            _contentTypeName = contentType.AssemblyQualifiedName;
            LoaderType = loaderType;
            ContentType = contentType;
        }

        public XamlDeferLoadAttribute(string loaderType, string contentType)
        {
            if (loaderType == null)
            {
                throw new ArgumentNullException("loaderType");
            }
            if (contentType == null)
            {
                throw new ArgumentNullException("contentType");
            }
            _loaderTypeName = loaderType;
            _contentTypeName = contentType;
        }

        public string LoaderTypeName
        {
            get { return _loaderTypeName; }
        }

        public string ContentTypeName
        {
            get { return _contentTypeName; }
        }

        public Type LoaderType { get; private set; }
        public Type ContentType { get; private set; }
    }
}
