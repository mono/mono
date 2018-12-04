// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace System.Xaml.Permissions
{
    [Serializable]
    public sealed class XamlLoadPermission : CodeAccessPermission, IUnrestrictedPermission
    {
        private static IList<XamlAccessLevel> s_emptyAccessLevel;
        private bool _isUnrestricted;

        public XamlLoadPermission(PermissionState state)
        {
            Init(state == PermissionState.Unrestricted, null);
        }

        public XamlLoadPermission(XamlAccessLevel allowedAccess)
        {
            if (allowedAccess == null)
            {
                throw new ArgumentNullException("allowedAccess");
            }
            Init(false, new XamlAccessLevel[] { allowedAccess });
        }

        public XamlLoadPermission(IEnumerable<XamlAccessLevel> allowedAccess)
        {
            if (allowedAccess == null)
            {
                throw new ArgumentNullException("allowedAccess");
            }
            List<XamlAccessLevel> accessList = new List<XamlAccessLevel>(allowedAccess);
            foreach (XamlAccessLevel accessLevel in allowedAccess)
            {
                if (accessLevel == null)
                {
                    throw new ArgumentException(SR.Get(SRID.CollectionCannotContainNulls, "allowedAccess"));
                }
                accessList.Add(accessLevel);
            }
            Init(false, accessList);
        }

#if NETCOREAPP3_0

        [Runtime.InteropServices.ComVisible(false)]
        public override bool Equals(object obj)
        {
            IPermission perm = obj as IPermission;
            if (obj != null && perm == null)
            {
                return false;
            }

            try
            {
                if (!IsSubsetOf(perm))
                {
                    return false;
                }

                if (perm != null && !perm.IsSubsetOf(this))
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                // Any argument exception implies inequality
                // Note that we require a try/catch here because we have to deal with
                // custom permissions that may throw exceptions indiscriminately
                return false;
            }

            return true;
        }

        [Runtime.InteropServices.ComVisible(false)]
        public override int GetHashCode()
        {
            // This implementation is only to silence a compiler warning
            return base.GetHashCode();
        }

#endif 

        // copy ctor. We can reuse the list of the existing instance, because it is a
        // ReadOnlyCollection over a privately created array, hence is never mutated,
        // even if the other instance is mutated via FromXml().
        private XamlLoadPermission(XamlLoadPermission other)
        {
            _isUnrestricted = other._isUnrestricted;
            this.AllowedAccess = other.AllowedAccess;
        }

        private void Init(bool isUnrestricted, IList<XamlAccessLevel> allowedAccess)
        {
            _isUnrestricted = isUnrestricted;
            if (allowedAccess == null)
            {
                if (s_emptyAccessLevel == null)
                {
                    s_emptyAccessLevel = new ReadOnlyCollection<XamlAccessLevel>(new XamlAccessLevel[0]);
                }
                AllowedAccess = s_emptyAccessLevel;
            }
            else
            {
                Debug.Assert(!isUnrestricted);
                AllowedAccess = new ReadOnlyCollection<XamlAccessLevel>(allowedAccess);
            }
        }

        public IList<XamlAccessLevel> AllowedAccess { get; private set; } // ReadOnlyCollection

        public override IPermission Copy()
        {
            return new XamlLoadPermission(this);
        }

        public override void FromXml(SecurityElement elem)
        {
            if (elem == null)
            {
                throw new ArgumentNullException("elem");
            }
            if (elem.Tag != XmlConstants.IPermission)
            {
                throw new ArgumentException(SR.Get(SRID.SecurityXmlUnexpectedTag, elem.Tag, XmlConstants.IPermission), "elem");
            }

            string className = elem.Attribute(XmlConstants.Class);
            if (!className.StartsWith(GetType().FullName, false, TypeConverterHelper.InvariantEnglishUS))
            {
                throw new ArgumentException(SR.Get(SRID.SecurityXmlUnexpectedValue, className, XmlConstants.Class, GetType().FullName), "elem");
            }

            string version = elem.Attribute(XmlConstants.Version);
            if (version != null && version != XmlConstants.VersionNumber)
            {
                throw new ArgumentException(SR.Get(SRID.SecurityXmlUnexpectedValue, className, XmlConstants.Version, XmlConstants.VersionNumber), "elem");
            }

            string unrestricted = elem.Attribute(XmlConstants.Unrestricted);
            if (unrestricted != null && bool.Parse(unrestricted))
            {
                Init(true, null);
            }
            else
            {
                List<XamlAccessLevel> allowedAccess = null;
                if (elem.Children != null)
                {
                    allowedAccess = new List<XamlAccessLevel>(elem.Children.Count);
                    foreach (SecurityElement child in elem.Children)
                    {
                        allowedAccess.Add(XamlAccessLevel.FromXml(child));
                    }
                }
                Init(false, allowedAccess);
            }
        }

        public bool Includes(XamlAccessLevel requestedAccess)
        {
            if (requestedAccess == null)
            {
                throw new ArgumentNullException("requestedAccess");
            }
            if (_isUnrestricted)
            {
                return true;
            }
            foreach (XamlAccessLevel allowedAccess in AllowedAccess)
            {
                if (allowedAccess.Includes(requestedAccess))
                {
                    return true;
                }
            }
            return false;
        }

        public override IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            XamlLoadPermission other = CastPermission(target, "target");
            if (other.IsUnrestricted())
            {
                return this.Copy();
            }
            if (this.IsUnrestricted())
            {
                return other.Copy();
            }

            List<XamlAccessLevel> result = new List<XamlAccessLevel>();
            // We could optimize this with a hash, but we don't expect people to be creating
            // large unions of access levels.
            foreach (XamlAccessLevel accessLevel in this.AllowedAccess)
            {
                // First try the full access level
                if (other.Includes(accessLevel))
                {
                    result.Add(accessLevel);
                }
                // Then try the assembly subset
                else if (accessLevel.PrivateAccessToTypeName != null)
                {
                    XamlAccessLevel assemblyAccess = accessLevel.AssemblyOnly();
                    if (other.Includes(assemblyAccess))
                    {
                        result.Add(assemblyAccess);
                    }
                }
            }
            return new XamlLoadPermission(result);
        }

        public override bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                bool isEmpty = !IsUnrestricted() && AllowedAccess.Count == 0;
                return isEmpty;
            }
            XamlLoadPermission other = CastPermission(target, "target");
            if (other.IsUnrestricted())
            {
                return true;
            }
            if (this.IsUnrestricted())
            {
                return false;
            }

            foreach (XamlAccessLevel accessLevel in this.AllowedAccess)
            {
                if (!other.Includes(accessLevel))
                {
                    return false;
                }
            }
            return true;
        }

        public override SecurityElement ToXml()
        {
            SecurityElement securityElement = new SecurityElement(XmlConstants.IPermission);
            securityElement.AddAttribute(XmlConstants.Class, this.GetType().AssemblyQualifiedName);
            securityElement.AddAttribute(XmlConstants.Version, XmlConstants.VersionNumber);

            if (IsUnrestricted())
            {
                securityElement.AddAttribute(XmlConstants.Unrestricted, Boolean.TrueString);
            }
            else
            {
                foreach (XamlAccessLevel accessLevel in AllowedAccess)
                {
                    securityElement.AddChild(accessLevel.ToXml());
                }
            }

            return securityElement;
        }

        public override IPermission Union(IPermission other)
        {
            if (other == null)
            {
                return this.Copy();
            }
            XamlLoadPermission xamlOther = CastPermission(other, "other");
            if (IsUnrestricted() || xamlOther.IsUnrestricted())
            {
                return new XamlLoadPermission(PermissionState.Unrestricted);
            }

            List<XamlAccessLevel> mergedAccess = new List<XamlAccessLevel>(this.AllowedAccess);
            foreach (XamlAccessLevel accessLevel in xamlOther.AllowedAccess)
            {
                if (!this.Includes(accessLevel))
                {
                    mergedAccess.Add(accessLevel);
                    if (accessLevel.PrivateAccessToTypeName != null)
                    {
                        // If we have an entry for access to just the assembly of this type, it is now redundant
                        for (int i = 0; i < mergedAccess.Count; i++)
                        {
                            if (mergedAccess[i].PrivateAccessToTypeName == null &&
                                mergedAccess[i].AssemblyNameString == accessLevel.AssemblyNameString)
                            {
                                mergedAccess.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
            return new XamlLoadPermission(mergedAccess);
        }
        
        public bool IsUnrestricted()
        {
            return _isUnrestricted;
        }

        private static XamlLoadPermission CastPermission(IPermission other, string argName)
        {
            XamlLoadPermission result = other as XamlLoadPermission;
            if (result == null)
            {
                throw new ArgumentException(SR.Get(SRID.ExpectedLoadPermission), argName);
            }
            return result;
        }

        private static class XmlConstants
        {
            public const string IPermission = "IPermission";
            public const string Class = "class";
            public const string Version = "version";
            public const string VersionNumber = "1";
            public const string Unrestricted = "Unrestricted";
        }
    }

}
