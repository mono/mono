//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;

    class ExpressionSelection : ContextItem
    {

        ModelItem modelItem;

        public ExpressionSelection()
        {
        }

        public ExpressionSelection(ModelItem modelItem)        
        {
            this.modelItem = modelItem;
        }

        public ModelItem ModelItem
        {
            get { return this.modelItem; }
        }

        public override Type ItemType
        {
            get
            {
                return typeof(ExpressionSelection);
            }
        }
    }
}
