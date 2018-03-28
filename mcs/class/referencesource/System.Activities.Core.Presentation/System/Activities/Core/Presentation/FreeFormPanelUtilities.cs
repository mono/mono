//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.View;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Media;

    internal static class FreeFormPanelUtilities
    {
        internal static bool IsRightToLeft(Visual visual)
        {
            Fx.Assert(visual != null, "visual != null");
            Transform trf = ViewUtilities.GetTransformToRoot(visual) as Transform;
            Matrix matrix = trf != null ? trf.Value : Matrix.Identity;
            Vector dir = new Vector(1, 0); // Point to right.
            dir = matrix.Transform(dir);
            return dir.X < dir.Y;
        }
    }
}
