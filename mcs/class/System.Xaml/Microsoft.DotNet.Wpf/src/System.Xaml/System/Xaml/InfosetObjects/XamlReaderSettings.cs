// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace System.Xaml
{
    /// <SecurityNote>
    /// The LocalAssembly and AllowProtectedMembersOnRoot properties are not SecurityCritical.
    /// XamlXmlReader uses these properties to surface non-public members in the nodestream, but it
    /// does not actually enable invocation of those members.
    /// XamlObjectReader invokes the non-public members, but does so transparently.
    /// Any component which allows invocation of non-public members in partial trust should demand
    /// an appropriate permission (such as ReflectionPerission or XamlLoadPermisison).
    /// </SecurityNote>
    public class XamlReaderSettings
    {
        public bool AllowProtectedMembersOnRoot { get; set; }
        public bool ProvideLineInfo { get; set; }
        public Uri BaseUri { get; set; }
        public Assembly LocalAssembly { get; set; }
        public bool IgnoreUidsOnPropertyElements { get; set; }
        public bool ValuesMustBeString { get; set; }

        public XamlReaderSettings()
        {
            InitializeProvideLineInfo();
        }

        public XamlReaderSettings(XamlReaderSettings settings) : this()
        {
            if (settings != null)
            {
                AllowProtectedMembersOnRoot = settings.AllowProtectedMembersOnRoot;
                ProvideLineInfo = settings.ProvideLineInfo;
                BaseUri = settings.BaseUri;
                LocalAssembly = settings.LocalAssembly;
                IgnoreUidsOnPropertyElements = settings.IgnoreUidsOnPropertyElements;
                ValuesMustBeString = settings.ValuesMustBeString;
            }
        }

        private void InitializeProvideLineInfo()
        {
            //By default, _provideLineInfo is true if the currently executing process is running in debug mode
            if (Debugger.IsAttached)
            {
                ProvideLineInfo = true;
            }
            else
            {
                ProvideLineInfo = false;
            }
        }
    }
}
