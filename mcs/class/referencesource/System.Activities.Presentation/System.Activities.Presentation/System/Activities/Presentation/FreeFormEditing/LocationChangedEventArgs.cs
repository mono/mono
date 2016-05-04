//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Windows;

    class LocationChangedEventArgs : EventArgs
    {
        Point newLocation;
               
        public LocationChangedEventArgs(Point newLocation)
        {
            this.newLocation = newLocation;
        }

        public Point NewLocation
        {
            get
            {
                return this.newLocation;
            }
        }
    }

}
