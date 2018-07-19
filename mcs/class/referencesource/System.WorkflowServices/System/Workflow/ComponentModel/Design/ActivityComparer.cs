//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.ComponentModel.Design
{
    using System.Workflow.ComponentModel;

    internal delegate bool ActivityComparer<TActivity>(TActivity source, TActivity target) where TActivity : Activity;

}
