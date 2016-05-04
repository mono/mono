//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System.Activities.Presentation.Model;
namespace System.Activities.Presentation
{
    //Marker interface to support expanded children inside ActivityDesigner/ServiceDesigner.
    internal interface IExpandChild
    {
        ModelItem ExpandedChild
        {
            get;
        }
    }
}
