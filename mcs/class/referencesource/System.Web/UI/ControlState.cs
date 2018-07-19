//------------------------------------------------------------------------------
// <copyright file="ControlState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * The possible states a container control can be in when children are added to it.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI {

    internal enum ControlState {
        Constructed = 0,
        FrameworkInitialized = 1,
        ChildrenInitialized = 2,
        Initialized = 3,
        ViewStateLoaded = 4,
        Loaded = 5,
        PreRendered = 6,
    }
}
