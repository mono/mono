//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Globalization;

    internal class PropertyReferenceChange : ModelChange
    {
        public ModelItem Owner { get; set; }

        public string TargetProperty { get; set; }

        public string OldSourceProperty { get; set; }

        public string NewSourceProperty { get; set; }

        public override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", SR.PropertyReferenceChangeEditingScopeDescription, this.TargetProperty);
            }
        }

        public override bool Apply()
        {
            PropertyReferenceUtilities.SetPropertyReference(this.Owner.GetCurrentValue(), this.TargetProperty, this.NewSourceProperty);
            this.Owner.OnPropertyReferenceChanged(this.TargetProperty);

            return true;
        }

        public override Change GetInverse()
        {
            return new PropertyReferenceChange()
            {
                Owner = this.Owner,
                TargetProperty = this.TargetProperty,
                OldSourceProperty = this.NewSourceProperty,
                NewSourceProperty = this.OldSourceProperty
            };
        }
    }
}
