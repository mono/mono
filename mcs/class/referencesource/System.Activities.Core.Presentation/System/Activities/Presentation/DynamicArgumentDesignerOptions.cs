//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class DynamicArgumentDesignerOptions
    {
        string argumentPrefix = DynamicArgumentDesigner.DefaultArgumentPrefix;
        string hintText;

        public string Title
        {
            get;
            set;
        }

        public string ArgumentPrefix
        {
            get { return this.argumentPrefix; }
            set { this.argumentPrefix = value; }
        }

        internal bool HideDirection
        {
            get;
            set;
        }

        internal string HintText
        {
            get { return this.hintText; }
            set { this.hintText = value; }
        }
    }
}
