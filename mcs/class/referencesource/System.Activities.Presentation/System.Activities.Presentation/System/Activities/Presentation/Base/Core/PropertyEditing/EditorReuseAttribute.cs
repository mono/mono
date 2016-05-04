//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.PropertyEditing
{
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class EditorReuseAttribute : Attribute
    {
        bool reuseEditor;

        public EditorReuseAttribute(bool reuseEditor)
        {
            this.reuseEditor = reuseEditor;
        }
        public bool ReuseEditor
        { get { return this.reuseEditor; } }
    }
}
