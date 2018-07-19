//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;

    internal class ListItemViewControl : UserControl
    {
        private DrawItemState drawItemState;

        private object item;
        private IServiceProvider serviceProvider;

        public virtual event EventHandler ItemChanged;

        public virtual DrawItemState DrawItemState
        {
            get { return drawItemState; }
            set { drawItemState = value; }
        }

        public virtual object Item
        {
            get { return item; }
            set
            {
                item = value;
                if (ItemChanged != null)
                {
                    ItemChanged.Invoke(this, null);
                }
            }
        }

        public IServiceProvider ServiceProvider
        {
            get { return serviceProvider; }
            set { serviceProvider = value; }
        }

        public virtual void UpdateView()
        {
        }

    }

}
