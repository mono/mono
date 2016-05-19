//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;
    using System.Activities.Presentation;

    // <summary>
    // EventArgs we use to fire CategoryList.ContainerGenerated event
    // </summary>
    internal class ContainerGeneratedEventArgs : EventArgs 
    {

        private CiderCategoryContainer _container;

        public ContainerGeneratedEventArgs(CiderCategoryContainer container) 
        {
            if (container == null) 
            {
                throw FxTrace.Exception.ArgumentNull("container");
            }
            _container = container;
        }

        public CiderCategoryContainer Container 
        {
            get {
                return _container;
            }
        }
    }

    // <summary>
    // Used in conjunction with ContainerGeneratedEventArgs
    // </summary>
    // <param name="sender"></param>
    // <param name="e"></param>
    internal delegate void ContainerGeneratedHandler(object sender, ContainerGeneratedEventArgs e);
}
