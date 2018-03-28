//------------------------------------------------------------------------------
// <copyright file="TemplateBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
/*
 * Classes related to templated control support
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Reflection;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    /*
     * This class defines the TemplateAttribute attribute that can be placed on
     * properties of type ITemplate.  It allows the parser to strongly type the
     * container, which makes it easier to write render code in a template
     */

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TemplateContainerAttribute : Attribute {
        private Type _containerType;

        private BindingDirection _bindingDirection;


        /// <devdoc>
        /// <para>Whether the template supports two-way binding.</para>
        /// </devdoc>
        public BindingDirection BindingDirection {
            get {
                return _bindingDirection;
            }
        }


        /// <devdoc>
        ///    <para></para>
        /// </devdoc>
        public Type ContainerType {
            get {
                return _containerType;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public TemplateContainerAttribute(Type containerType) : this(containerType, BindingDirection.OneWay) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public TemplateContainerAttribute(Type containerType, BindingDirection bindingDirection) {
            _containerType = containerType;
            _bindingDirection = bindingDirection;
        }
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class TemplateBuilder : ControlBuilder, ITemplate {
        internal string _tagInnerText;

        private bool _allowMultipleInstances;

        private IDesignerHost _designerHost;


        public TemplateBuilder() {
            _allowMultipleInstances = true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        //  ID parameter name changed from ControlBuilder, so leave this to avoid a "breaking change"
        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string ID, IDictionary attribs) {
            base.Init(parser, parentBuilder, type, tagName, ID, attribs);

            if (InPageTheme && ParentBuilder != null && ParentBuilder.IsControlSkin) {
                ((PageThemeParser)Parser).CurrentSkinBuilder = parentBuilder;
            }
        }

        public override void CloseControl() {
            base.CloseControl();

            if (InPageTheme && ParentBuilder != null && ParentBuilder.IsControlSkin) {
                ((PageThemeParser)Parser).CurrentSkinBuilder = null;
            }
        }

        internal bool AllowMultipleInstances {
            get { return _allowMultipleInstances; }
            set { _allowMultipleInstances = value; }
        }

        // This code is only executed when used from the designer

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object BuildObject() {
            return this;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool NeedsTagInnerText() {
            // We want SetTagInnerText() to be called if we're running in the
            // designer.
            return InDesigner;
        }

        internal void SetDesignerHost(IDesignerHost designerHost) {
            _designerHost = designerHost;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void SetTagInnerText(string text) {
            // Save the inner text of the template tag
            _tagInnerText = text;
        }

        /*
         * ITemplate implementation
         * This implementation of ITemplate is only used in the designer
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void InstantiateIn(Control container) {

            IServiceProvider builderServiceProvider = null;

            // Use the designer host to get one at designtime as the service provider
            if (_designerHost != null) {
                builderServiceProvider = _designerHost;
            }
            else if (!IsNoCompile) {
                // Otherwise, create a ServiceContainer and try using the container as the service provider
                ServiceContainer serviceContainer = new ServiceContainer();
                if (container is IThemeResolutionService) {
                    serviceContainer.AddService(typeof(IThemeResolutionService), (IThemeResolutionService)container);
                }

                if (container is IFilterResolutionService) {
                    serviceContainer.AddService(typeof(IFilterResolutionService), (IFilterResolutionService)container);
                }
                builderServiceProvider = serviceContainer;
            }

            HttpContext context = null;
            TemplateControl savedTemplateControl = null;

            TemplateControl templateControl = container as TemplateControl;

            if (templateControl != null) {
                context = HttpContext.Current;
                if (context != null)
                    savedTemplateControl = context.TemplateControl;
            }

            try {
                if (!IsNoCompile)
                    SetServiceProvider(builderServiceProvider);

                if (context != null) {
                    context.TemplateControl = templateControl;
                }

                BuildChildren(container);
            }
            finally {
                if (!IsNoCompile)
                    SetServiceProvider(null);

                // Restore the previous template control
                if (context != null)
                    context.TemplateControl = savedTemplateControl;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual string Text {
            get { return _tagInnerText; }
            set { _tagInnerText = value; }
        }
    }

    // Delegates used for the compiled template

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public delegate void BuildTemplateMethod(Control control);

    /*
     * This class is the ITemplate implementation that is called from the
     * generated page class code.  It just passes the Initialize call on to a
     * delegate.
     */

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class CompiledTemplateBuilder : ITemplate {
        private BuildTemplateMethod _buildTemplateMethod;


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CompiledTemplateBuilder(BuildTemplateMethod buildTemplateMethod) {
            _buildTemplateMethod = buildTemplateMethod;
        }

        // ITemplate::InstantiateIn

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void InstantiateIn(Control container) {
            _buildTemplateMethod(container);
        }
    }
}
