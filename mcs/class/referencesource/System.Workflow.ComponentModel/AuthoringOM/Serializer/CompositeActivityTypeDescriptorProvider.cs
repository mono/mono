namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Resources;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using Microsoft.CSharp;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics;

    #region CompositeActivityTypeDescriptorProvider

    internal class CompositeActivityTypeDescriptorProvider : TypeDescriptionProvider
    {
        public CompositeActivityTypeDescriptorProvider()
            : base(TypeDescriptor.GetProvider(typeof(CompositeActivity)))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            ICustomTypeDescriptor realTypeDescriptor = base.GetTypeDescriptor(objectType, instance);
            return new CompositeActivityTypeDescriptor(realTypeDescriptor);
        }
    }


    #endregion

}
