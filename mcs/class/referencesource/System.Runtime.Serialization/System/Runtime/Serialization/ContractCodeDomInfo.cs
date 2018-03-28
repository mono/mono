//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.CodeDom;
    using System.Collections.Generic;

    internal class ContractCodeDomInfo
    {
        internal bool IsProcessed;
        internal CodeTypeDeclaration TypeDeclaration;
        internal CodeTypeReference TypeReference;
        internal CodeNamespace CodeNamespace;
        internal bool ReferencedTypeExists;
        internal bool UsesWildcardNamespace;
        string clrNamespace;
        Dictionary<string, object> memberNames;

        internal string ClrNamespace
        {
            get { return (ReferencedTypeExists ? null : clrNamespace); }
            set
            {
                if (ReferencedTypeExists)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotSetNamespaceForReferencedType, TypeReference.BaseType)));
                else
                    clrNamespace = value;
            }
        }

        internal Dictionary<string, object> GetMemberNames()
        {
            if (ReferencedTypeExists)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotSetMembersForReferencedType, TypeReference.BaseType)));
            else
            {
                if (memberNames == null)
                {
                    memberNames = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                }
                return memberNames;
            }
        }
    }
}
