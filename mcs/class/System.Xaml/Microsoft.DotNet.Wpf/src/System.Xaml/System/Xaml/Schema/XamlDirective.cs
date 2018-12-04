// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace System.Xaml
{
    public class XamlDirective : XamlMember
    {
        private AllowedMemberLocations _allowedLocation;
        private IList<string> _xamlNamespaces;

        internal XamlDirective(IEnumerable<string> xamlNamespaces, string name, AllowedMemberLocations allowedLocation, MemberReflector reflector)
            : base(name, reflector) 
        {
            _xamlNamespaces = GetReadOnly(xamlNamespaces);
            _allowedLocation = allowedLocation;
        }

        public XamlDirective(IEnumerable<string> xamlNamespaces, string name, XamlType xamlType,
            XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
            : base(name, new MemberReflector(xamlType, typeConverter))
        {
            if (xamlType == null)
            {
                throw new ArgumentNullException("xamlType");
            }

            _xamlNamespaces = GetReadOnly(xamlNamespaces);
            _allowedLocation = allowedLocation;
        }

        public XamlDirective(string xamlNamespace, string name)
            :base(name, null)
        {
            _xamlNamespaces = GetReadOnly(xamlNamespace);
            _allowedLocation = AllowedMemberLocations.Any;
        }

        public AllowedMemberLocations AllowedLocation { get { return _allowedLocation; } }

        public override int GetHashCode()
        {
            int result = (Name == null) ? 0 : Name.GetHashCode();
            foreach (string ns in _xamlNamespaces)
            {
                result ^= ns.GetHashCode();
            }
            return result;
        }

        public override string ToString()
        {
            if (_xamlNamespaces.Count > 0)
            {
                return "{" + _xamlNamespaces[0] + "}" + Name;
            }
            else
            {
                return Name;
            }
        }

        public override IList<string> GetXamlNamespaces()
        {
            return _xamlNamespaces;
        }

        // We use the private field _xamlNamespaces to avoid expensive lookups or mutable
        // return values from overriden implementations of GetXamlNamespaces. But this will produce
        // hard-to-understand behavior if the namespaces returned from GetXamlNamespaces are different
        // from the ones passed in the ctor. Ideally we would provide overridable equality here.
        internal static bool NamespacesAreEqual(XamlDirective directive1, XamlDirective directive2)
        {
            IList<string> ns1 = directive1._xamlNamespaces;
            IList<string> ns2 = directive2._xamlNamespaces;

            if (ns1.Count != ns2.Count)
            {
                return false;
            }
            for (int i = 0; i < ns1.Count; i++)
            {
                if (ns1[i] != ns2[i])
                {
                    return false;
                }
            }
            return true;
        }

        protected sealed override XamlMemberInvoker LookupInvoker()
        {
            return XamlMemberInvoker.DirectiveInvoker;
        }

        protected sealed override ICustomAttributeProvider LookupCustomAttributeProvider()
        {
            return null;
        }

        protected sealed override IList<XamlMember> LookupDependsOn()
        {
            return null;
        }

        protected sealed override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
        {
            return null;
        }

        protected sealed override bool LookupIsAmbient()
        {
            return false;
        }

        protected sealed override bool LookupIsEvent()
        {
            return false;
        }

        protected sealed override bool LookupIsReadOnly()
        {
            return false;
        }

        protected sealed override bool LookupIsReadPublic()
        {
            return true;
        }

        protected sealed override bool LookupIsUnknown()
        {
            return base.IsUnknown;
        }

        protected sealed override bool LookupIsWriteOnly()
        {
            return false;
        }

        protected sealed override bool LookupIsWritePublic()
        {
            return true;
        }

        protected sealed override XamlType LookupTargetType()
        {
            return null;
        }

        protected sealed override XamlValueConverter<TypeConverter> LookupTypeConverter()
        {
            // We set this value in ctor, so this call won't produce infinite loop
            return base.TypeConverter;
        }

        protected sealed override XamlType LookupType()
        {
            // We set this value in ctor, so this call won't produce infinite loop
            return base.Type;
        }

        protected sealed override MethodInfo LookupUnderlyingGetter()
        {
            return null;
        }

        protected sealed override MemberInfo LookupUnderlyingMember()
        {
            return null;
        }

        protected sealed override MethodInfo LookupUnderlyingSetter()
        {
            return null;
        }

        private static ReadOnlyCollection<string> GetReadOnly(string xamlNamespace)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }
            return new ReadOnlyCollection<string>(new string[] { xamlNamespace });
        }

        private static ReadOnlyCollection<string> GetReadOnly(IEnumerable<string> xamlNamespaces)
        {
            if (xamlNamespaces == null)
            {
                throw new ArgumentNullException("xamlNamespaces");
            }
            List<string> nsList = new List<string>(xamlNamespaces);
            foreach (string ns in nsList)
            {
                if (ns == null)
                {
                    throw new ArgumentException(SR.Get(SRID.CollectionCannotContainNulls, "xamlNamespaces"));
                }
            }
            return nsList.AsReadOnly();
        }
    }
}
