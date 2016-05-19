//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    class ConnectorMovedEventArgs : EventArgs
    {
        List<Point> newConnectorLocation;

        public ConnectorMovedEventArgs(List<Point> newConnectorLocation)
        {
            this.newConnectorLocation = newConnectorLocation;
        }

        public List<Point> NewConnectorLocation
        {
            get
            {
                return this.newConnectorLocation;
            }
        }
    }

}
