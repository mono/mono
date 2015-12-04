//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    internal delegate void SelectedPropertyNameChangedEventHandler(object sender, SelectedPropertyNameChangedEventArgs e);

    internal sealed class SelectedPropertyNameChangedEventArgs : EventArgs
    {
        public SelectedPropertyNameChangedEventArgs(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public string PropertyName
        {
            get;
            private set;
        }
    }   
}
