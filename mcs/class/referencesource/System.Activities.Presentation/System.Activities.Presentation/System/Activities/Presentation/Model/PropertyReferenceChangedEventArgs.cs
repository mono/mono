//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    internal delegate void PropertyReferenceChangedEventHandler(object sender, PropertyReferenceChangedEventArgs e);

    internal class PropertyReferenceChangedEventArgs : EventArgs
    {
        public PropertyReferenceChangedEventArgs(string targetProperty)
        {
            this.TargetProperty = targetProperty;
        }

        public string TargetProperty
        {
            get;
            private set;
        }
    }
}
