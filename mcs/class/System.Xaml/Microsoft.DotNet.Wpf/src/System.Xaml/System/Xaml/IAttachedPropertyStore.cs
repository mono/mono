// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xaml
{
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;
    using System.Xaml;

    public interface IAttachedPropertyStore
    {
        // The number of properties currently attached to this instance
        int PropertyCount
        { get; }

        // Retrieve the set of attached properties for this instance. This is
        //  a copy of the current set of properties.
        void CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index);
        // Remove the property 'name' from this instance. If the property doesn't
        //  currently exist this returns false.
        bool RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier);
        // Set the property 'name' to 'value' for this instance. If the property
        //  doesn't currently exist on this instance it will be created.
        void SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value);
        // Retrieve the value of the attached property 'name' for this instance.
        //  If there is not an attached property defined for this instance with
        //  this 'name' then returns false. If the value of the attached property
        //  for this instance with this 'name' cannot be cast to T then returns
        //  false.
        [SuppressMessage("Microsoft.Design", "CA1007")]
        bool TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value);
    }
}
