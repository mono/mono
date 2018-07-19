//------------------------------------------------------------------------------
// <copyright file="FlowSwitchLabelFeature.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;

    internal class FlowSwitchLabelFeature : ViewStateAttachedPropertyFeature
    {
        public const string DefaultCaseDisplayNamePropertyName = "DefaultCaseDisplayName";
        public const string DefaultCaseDisplayNameDefaultValue = "Default";

        protected override IEnumerable<AttachedPropertyInfo> AttachedProperties
        {
            get
            {
                yield return new AttachedPropertyInfo<string> { PropertyName = DefaultCaseDisplayNamePropertyName, IsBrowsable = false, IsVisibleToModelItem = true, DefaultValue = DefaultCaseDisplayNameDefaultValue };
            }
        }
    }
}
