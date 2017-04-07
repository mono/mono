using System;
using System.Collections.Generic;
using System.Text;

//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor,deliant

#if WINFORMS_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{

    internal static class Constants
    {
        public const string AutoValue = "Auto";
        public const string NotSetValue = "NotSet";
        public const string MinValue = "Min";
        public const string MaxValue = "Max";
    }

}

