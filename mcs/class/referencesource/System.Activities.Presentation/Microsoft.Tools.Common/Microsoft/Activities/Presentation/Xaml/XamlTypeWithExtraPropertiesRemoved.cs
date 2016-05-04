//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xaml;

    internal class XamlTypeWithExtraPropertiesRemoved : XamlType
    {
        private ICollection<string> propertiesToBeRemoved;

        public XamlTypeWithExtraPropertiesRemoved(Type underlyingType, XamlSchemaContext schemaContext, ICollection<string> propertiesToBeRemoved)
            : base(underlyingType, schemaContext)
        {
            this.propertiesToBeRemoved = propertiesToBeRemoved;
        }

        protected override XamlMember LookupMember(string name, bool skipReadOnlyCheck)
        {
            if (this.propertiesToBeRemoved.Contains(name))
            {
                return null;
            }

            return base.LookupMember(name, skipReadOnlyCheck);
        }

        protected override IEnumerable<XamlMember> LookupAllMembers()
        {
            List<XamlMember> members = new List<XamlMember>();

            foreach (XamlMember member in base.LookupAllMembers())
            {
                if (!this.propertiesToBeRemoved.Contains(member.Name))
                {
                    members.Add(member);
                }
            }

            return members;
        }
    }
}
