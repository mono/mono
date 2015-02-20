//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    internal sealed class ListItemViewAttribute : Attribute
    {
        private Type viewType;

        public ListItemViewAttribute(Type viewType)
        {
            this.ViewType = viewType;
        }

        public Type ViewType
        {
            get { return viewType; }
            set { viewType = value; }
        }

    }

}
