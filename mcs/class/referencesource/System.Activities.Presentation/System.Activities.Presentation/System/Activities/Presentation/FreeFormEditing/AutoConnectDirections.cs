//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    /// <summary>
    /// Defines to which direction should the auto connection be allowed
    /// </summary>
    [Flags]
    internal enum AutoConnectDirections
    {
        /// <summary>
        /// Invalid direction
        /// </summary>
        None = 0,

        /// <summary>
        /// Left direction
        /// </summary>
        Left = 1,

        /// <summary>
        /// Right direction
        /// </summary>
        Right = 2,

        /// <summary>
        /// Top direction
        /// </summary>
        Top = 4,

        /// <summary>
        /// Bottom direction
        /// </summary>
        Bottom = 8
    }
}
