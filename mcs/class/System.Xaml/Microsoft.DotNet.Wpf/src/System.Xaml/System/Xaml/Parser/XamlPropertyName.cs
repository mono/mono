// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Xaml.MS.Impl;
using System.Collections.Generic;

namespace MS.Internal.Xaml.Parser
{
    internal class XamlPropertyName : XamlName
    {
        private XamlPropertyName(XamlName owner, string prefix, string name)
            : base(name)
        {
            if (owner != null)
            {
                this.Owner = owner;
                this._prefix = owner.Prefix ?? String.Empty;
            }
            else
                this._prefix = prefix ?? String.Empty;
        }

        public readonly XamlName Owner;

        public static XamlPropertyName Parse(string longName)
        {
            if(String.IsNullOrEmpty(longName))
            {
                return null;
            }

            string prefix;
            string dottedName;

            if (!XamlQualifiedName.Parse(longName, out prefix, out dottedName))
            {
                return null;
            }

            int start = 0;
            string owner = String.Empty;

            int dotIdx = dottedName.IndexOf('.');
            if (dotIdx != -1)
            {
                owner = dottedName.Substring(start, dotIdx);

                if (String.IsNullOrEmpty(owner))
                {
                    return null;
                }

                start = dotIdx + 1;
            }

            string name = (start == 0) ? dottedName : dottedName.Substring(start);

            XamlQualifiedName ownerName = null;
            if (!String.IsNullOrEmpty(owner))
            {
                ownerName = new XamlQualifiedName(prefix, owner);
            }

            XamlPropertyName propName = new XamlPropertyName(ownerName, prefix, name);
            return propName;
        }

        public static XamlPropertyName Parse(string longName, string namespaceURI)
        {
            XamlPropertyName propName = Parse(longName);
            propName._namespace = namespaceURI;
            return propName;
        }

        public override string ScopedName
        {
            get
            {
                return IsDotted ?
                    Owner.ScopedName + "." + Name :
                    Name;
            }
        }

        public string OwnerName
        {
            get
            {
                return IsDotted ?
                    Owner.Name :
                    String.Empty;
            }
        }

        public bool IsDotted
        {
            get { return Owner != null; }
        }
    }
}
