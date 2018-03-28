//------------------------------------------------------------------------------
// <copyright file="DeviceSpecificChoice.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.Mobile;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * DeviceSpecificChoice object.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice"]/*' />
    [
        ControlBuilderAttribute(typeof(DeviceSpecificChoiceControlBuilder)),
        PersistName("Choice"),
        PersistChildren(false),
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DeviceSpecificChoice : IParserAccessor, IAttributeAccessor
    {
        private String _deviceFilter = String.Empty;
        private String _argument;
        private String _xmlns;
        private IDictionary _contents;
        private IDictionary _templates;
        private DeviceSpecific _owner;

        private static IComparer _caseInsensitiveComparer =
            new CaseInsensitiveComparer(CultureInfo.InvariantCulture);

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.Filter"]/*' />
        [
            DefaultValue("")
        ]
        public String Filter  
        {
            get
            {
                Debug.Assert(_deviceFilter != null);
                return _deviceFilter; 
            }
             
            set
            {
                if (value == null)
                {
                    value = String.Empty;
                }
                _deviceFilter = value; 
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.Argument"]/*' />
        public String Argument  
        {
            get
            {
                return _argument; 
            }
             
            set
            {
                _argument = value; 
            }
        }

        // This property is used by the Designer, and has no runtime effect
        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.Xmlns"]/*' />
        [
            DefaultValue("")
        ]
        public String Xmlns
        {
            get
            {
                return _xmlns;
            }

            set
            {
                _xmlns = value;
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.Contents"]/*' />
        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ]
        public IDictionary Contents
        {
            get
            {
                if (_contents == null)
                {
                    _contents = new ListDictionary(_caseInsensitiveComparer);
                }
                return _contents;
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.Templates"]/*' />
        [
            PersistenceMode(PersistenceMode.InnerProperty),
        ]
        public IDictionary Templates
        {
            get
            {
                if (_templates == null)
                {
                    _templates = new ListDictionary(_caseInsensitiveComparer);
                }
                return _templates;
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.HasTemplates"]/*' />
        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool HasTemplates
        {
            get
            {
                return _templates != null && _templates.Count > 0;
            }
        }

        internal void ApplyProperties()
        {
            IDictionaryEnumerator enumerator = Contents.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Object parentObject = Owner.Owner;
                
                String propertyName = (String)enumerator.Key;
                String propertyValue = enumerator.Value as String;

                // The ID property may not be overridden, according to spec
                // (since it will override the parent's ID, not very useful). 
                if (String.Equals(propertyName, "id", StringComparison.OrdinalIgnoreCase)) {
                    throw new ArgumentException(
                        SR.GetString(SR.DeviceSpecificChoice_InvalidPropertyOverride,
                                     propertyName));
                }
                
                if (propertyValue != null)
                {
                    // Parse through any "-" syntax items.

                    int dash;
                    while ((dash = propertyName.IndexOf("-", StringComparison.Ordinal)) != -1)
                    {   
                        String containingObjectName = propertyName.Substring(0, dash);
                        PropertyDescriptor pd = TypeDescriptor.GetProperties(parentObject).Find(
                                    containingObjectName, true);
                        if (pd == null)
                        {
                            throw new ArgumentException(
                                SR.GetString(SR.DeviceSpecificChoice_OverridingPropertyNotFound,
                                             propertyName));
                        }

                        parentObject = pd.GetValue(parentObject);
                        propertyName = propertyName.Substring(dash + 1);
                    }

                    if (!FindAndApplyProperty(parentObject, propertyName, propertyValue) &&
                        !FindAndApplyEvent(parentObject, propertyName, propertyValue))
                    {
                        // If control supports IAttributeAccessor (which it should)
                        // use it to set a custom attribute.

                        IAttributeAccessor a = parentObject as IAttributeAccessor;
                        if (a != null)
                        {
                            a.SetAttribute(propertyName, propertyValue);
                        }
                        else
                        {
                            throw new ArgumentException(
                                SR.GetString(SR.DeviceSpecificChoice_OverridingPropertyNotFound,
                                         propertyName));
                        }
                    }
                }
            }
        }

        private bool FindAndApplyProperty(Object parentObject, String name, String value)
        {
            PropertyDescriptor pd = TypeDescriptor.GetProperties(parentObject).Find(name, true);
            if (pd == null)
            {
                return false;
            }

            // Make sure the property is declarable.

            if (pd.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden)) 
            {
                throw new ArgumentException(
                    SR.GetString(SR.DeviceSpecificChoice_OverridingPropertyNotDeclarable, name));
            }

            Object o;
            Type type = pd.PropertyType;

            if (type.IsAssignableFrom(typeof(String)))
            {
                o = value;
            }
            else if (type.IsAssignableFrom(typeof(int)))
            {
                o = Int32.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (type.IsEnum)
            {
                o = Enum.Parse(type, value, true);
            }
            else if (value.Length == 0)
            {
                o = null;
            }
            else
            {
                TypeConverter converter = pd.Converter;
                if (converter != null)
                {
                    o = converter.ConvertFromInvariantString(value);
                }
                else
                {
                    throw new InvalidCastException(
                        SR.GetString(SR.DeviceSpecificChoice_OverridingPropertyTypeCast, name));
                }
            }
            pd.SetValue(parentObject, o);
            return true;
        }

        private bool FindAndApplyEvent(Object parentObject, String name, String value)
        {
            if (name.Length > 2 &&
                    Char.ToLower(name[0], CultureInfo.InvariantCulture) == 'o' &&
                    Char.ToLower(name[1], CultureInfo.InvariantCulture) == 'n')
            {
                String eventName = name.Substring(2);
                EventDescriptor ed = TypeDescriptor.GetEvents(parentObject).Find(eventName, true);
                if (ed != null)
                {
                    Delegate d = Delegate.CreateDelegate(ed.EventType, Owner.MobilePage, value);
                    ed.AddEventHandler(parentObject, d);
                    return true;
                }
            }
            return false;
        }

        internal DeviceSpecific Owner
        {
            get
            {
                return _owner;
            }

            set
            {
                _owner = value;
            }
        }

        internal bool Evaluate(MobileCapabilities capabilities)
        {
            // Evaluate the <Choice> by first looking to see if it's null, then
            // checking against evaluators defined in code on the page, then by
            // consulting the MobileCapabilities object.
            bool result;
            if (_deviceFilter != null && _deviceFilter.Length == 0) {
                // indicates device-independent <choice> clause
                result = true;
            }
            else if (CheckOnPageEvaluator(capabilities, out result))
            {
                // result already been set through the out-bound parameter
                // above. 
            }
            else
            {
                // The exception message generated by HasCapability() failing is 
                // inappropriate, so we substitute a more specific one.
                try
                {
                    result = capabilities.HasCapability(_deviceFilter, _argument);
                }
                catch
                {
                    throw new ArgumentException(SR.GetString(
                                    SR.DeviceSpecificChoice_CantFindFilter,
                                    _deviceFilter));
                }
                
            }

            return result;
        }

        // Return true if specified evaluator exists on the page with the
        // correct signature.  If it does, return result of invoking it in
        // evaluatorResult. 
        private bool CheckOnPageEvaluator(MobileCapabilities capabilities,
                                          out bool evaluatorResult)
        {
            evaluatorResult = false;
            TemplateControl containingTemplateControl = Owner.ClosestTemplateControl;

            MethodInfo methodInfo =
                containingTemplateControl.GetType().GetMethod(_deviceFilter,
                                                              new Type[]
                                                              {
                                                                  typeof(MobileCapabilities), 
                                                                  typeof(String)
                                                              }
                    );

            if (methodInfo == null || methodInfo.ReturnType != typeof(bool))
            {
                return false;
            }
            else
            {
                evaluatorResult = (bool)
                    methodInfo.Invoke(containingTemplateControl,
                                      new Object[]
                                      {
                                          capabilities,
                                          _argument
                                      }
                                     );

                return true;
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.IAttributeAccessor.GetAttribute"]/*' />
        /// <internalonly/>
        protected String GetAttribute(String key)
        {
            Object o = Contents[key];
            if (o != null & !(o is String))
            {
                throw new ArgumentException(SR.GetString(
                            SR.DeviceSpecificChoice_PropertyNotAnAttribute));
            }
            return (String)o;
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.IAttributeAccessor.SetAttribute"]/*' />
        /// <internalonly/>
        protected void SetAttribute(String key, String value)
        {
            Contents[key] = value;
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoice.IParserAccessor.AddParsedSubObject"]/*' />
        /// <internalonly/>
        protected void AddParsedSubObject(Object obj)
        {
            DeviceSpecificChoiceTemplateContainer c = obj as DeviceSpecificChoiceTemplateContainer;
            if (c != null)
            {
                Templates[c.Name] = c.Template;
            }
        }

        #region IAttributeAccessor implementation
        String IAttributeAccessor.GetAttribute(String name) {
            return GetAttribute(name);
        }

        void IAttributeAccessor.SetAttribute(String name, String value) {
            SetAttribute(name, value);
        }
        #endregion

        #region IParserAccessor implementation
        void IParserAccessor.AddParsedSubObject(Object obj) {
            AddParsedSubObject(obj);
        }
        #endregion
    }

    // TEMPLATE BAG
    //
    // The following classes are public by necessity (since they are exposed to
    // the framework), but all internal to the DeviceSpecificChoice. They have to do with
    // persistence of arbitrary templates in a choice. Here's a description of what is done:
    //
    // ASP.NET provides no way for an object or control to allow an arbitrary bag of 
    // templates. It only allows one way to define templates - the parent object must have
    // a property, of type ITemplate, with the same name as the template name. For example,
    // the code
    //
    //      <ParentCtl>
    //          <FirstTemplate>....</FirstTemplate>
    //          <SecondTemplate>....</SecondTemplate>
    //          <ThirdTemplate>....</ThirdTemplate>
    //      </ParentCtl>
    //
    // only works if the ParentCtl class exposes ITemplate properties with names FirstTemplate,
    // SecondTemplate, and ThirdTemplate.
    //
    // Because Choices apply to any control, that could potentially require any named template,
    // what we really need is something like a "template bag" that takes arbitrary templates.
    //
    // To work around this, here's what is done. First, at compile time:
    //
    // 1) DeviceSpecificChoice has its own control builder at compile time. When it is given a
    //    sub-object (in GetChildControlType), it returns DeviceSpecificChoiceTemplateType, which
    //    is a marker type similar to that used in ASP.NET. However, it is our own class, and
    //    has DeviceSpecificChoiceTemplateBuilder as its builder.
    // 2) DeviceSpecificChoiceTemplateBuilder inherits from TemplateBuilder, and thus has the same
    //    behavior as TemplateBuilder for parsing and compiling a template. However, it has
    //    an overriden Init method, which changes the tag name (and thus, the template name) 
    //    to a constant, "Template". It also saves the real template name in a property.
    // 3) When parsed, the framework calls the AppendSubBuilder method of the 
    //    DeviceSpecificChoiceBuilder, to add the template builder into it. But this builder
    //    first creates an intermediate builder, for the class DeviceSpecificChoiceTemplateContainer,
    //    adding the template name as a property in the builder's attribute dictionary. It then
    //    adds the intermediate builder into itself, and the template builder into it.
    //
    // All this has the net effect of automatically transforming something like
    //
    //      <Choice>
    //          <ItemTemplate>...</ItemTemplate>
    //          <HeaderTemplate>...</HeaderTemplate>
    //      </Choice>
    // 
    // into
    //
    //      <Choice>
    //          <DeviceSpecificChoiceTemplateContainer Name="ItemTemplate">
    //              <Template>...</Template>
    //          </DeviceSpecificChoiceTemplateContainer>
    //          <DeviceSpecificChoiceTemplateContainer Name="HeaderTemplate">
    //              <Template>...</Template>
    //          </DeviceSpecificChoiceTemplateContainer>
    //      </Choice>
    //
    // Now, at runtime the compiled code creates a DeviceSpecificChoiceTemplateContainer object,
    // and calls the AddParsedSubObject method of the DeviceSpecificChoice with it. This code (above)
    // then extracts the template referred to by the Template property of the object, and 
    // uses the Name property to add it to the template bag. Presto, we have a general template bag.

    /*
     * DeviceSpecificChoice control builder. For more information, see note on "Template Bag" above.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DeviceSpecificChoiceControlBuilder : ControlBuilder
    {
        private bool _isDeviceIndependent = false;
        internal bool IsDeviceIndependent()
        {
            return _isDeviceIndependent;
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceControlBuilder.Init"]/*' />
        public override void Init(TemplateParser parser, 
                                  ControlBuilder parentBuilder,
                                  Type type, 
                                  String tagName, 
                                  String id, 
                                  IDictionary attributes) 
        {
            if (!(parentBuilder is DeviceSpecificControlBuilder))
            {
                throw new ArgumentException(
                    SR.GetString(SR.DeviceSpecificChoice_ChoiceOnlyExistInDeviceSpecific));
            }

            _isDeviceIndependent = attributes == null || attributes["Filter"] == null;

            base.Init (parser, parentBuilder, type, tagName, id, attributes);
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceControlBuilder.AppendLiteralString"]/*' />
        public override void AppendLiteralString(String text)
        {
            // Ignore literal strings.
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceControlBuilder.GetChildControlType"]/*' />
        public override Type GetChildControlType(String tagName, IDictionary attributes) 
        {
            // Assume children are templates.

            return typeof(DeviceSpecificChoiceTemplateType);
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceControlBuilder.AppendSubBuilder"]/*' />
        public override void AppendSubBuilder(ControlBuilder subBuilder) 
        {
            DeviceSpecificChoiceTemplateBuilder tplBuilder = 
                subBuilder as DeviceSpecificChoiceTemplateBuilder;
            if (tplBuilder != null)
            {
                // Called to add a template. Insert an intermediate control, 
                // by creating and adding its builder.

                ListDictionary dict = new ListDictionary();

                // Add the template's name as a Name attribute for the control.
                dict["Name"] = tplBuilder.TemplateName;

                // 1 and "xxxx" are bogus filename/line number values.
                ControlBuilder container = ControlBuilder.CreateBuilderFromType(
                                                Parser, this, 
                                                typeof(DeviceSpecificChoiceTemplateContainer),
                                                "Templates",
                                                null, dict, 1, null);
                base.AppendSubBuilder(container);

                // Now, append the template builder into the new intermediate builder.

                container.AppendSubBuilder(subBuilder);
            }
            else
            {
                base.AppendSubBuilder(subBuilder);
            }
        }
    }

    /*
     * DeviceSpecificChoiceTemplateType - marker type for a template that goes inside
     *      a Choice. Used only at compile time, and never instantiated. See note
     *      on "Template Bag" above.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    [
        ControlBuilderAttribute(typeof(DeviceSpecificChoiceTemplateBuilder))
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class DeviceSpecificChoiceTemplateType : Control, IParserAccessor

    {
        private DeviceSpecificChoiceTemplateType()
        {
        }

        void IParserAccessor.AddParsedSubObject(Object o)
        {
        }
    }

    /*
     * DeviceSpecificChoiceTemplateBuilder - builder for a template that goes inside
     *      a Choice. See note on "Template Bag" above.
     *      When a Choice is device-independent, it also parses literal text content.
     *      The code for this is copied from LiteralTextContainerControlBuilder.cs
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceTemplateBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DeviceSpecificChoiceTemplateBuilder : TemplateBuilder
    {
        private String _templateName;
        private bool _doLiteralText = false;
        private bool _controlsInserted = false;

        internal String TemplateName
        {
            get
            {
                return _templateName;
            }
        }

        CompileLiteralTextParser _textParser = null;
        internal CompileLiteralTextParser TextParser
        {
            get
            {
                if (_textParser == null)
                {
                    _textParser = 
                        new CompileLiteralTextParser(Parser, this, "xxxx", 1);
                    if (_controlsInserted)
                    {
                        _textParser.ResetBreaking();
                        _textParser.ResetNewParagraph();
                    }
                }
                return _textParser;
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceTemplateBuilder.Init"]/*' />
        public override void Init(TemplateParser parser, 
                                  ControlBuilder parentBuilder,
                                  Type type, 
                                  String tagName,
                                  String id, 
                                  IDictionary attributes) 
        {
            // Save off template name, and always pass the name "Template" to the base
            // class, because the intermediate object has this property as the name.

            _templateName = tagName;
            base.Init(parser, parentBuilder, type, "Template", id, attributes);

            // Are we a device-independent template?

            if (!InDesigner)
            {
                DeviceSpecificChoiceControlBuilder choiceBuilder = 
                    parentBuilder as DeviceSpecificChoiceControlBuilder;
                _doLiteralText = choiceBuilder != null && choiceBuilder.IsDeviceIndependent();
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceTemplateBuilder.AppendLiteralString"]/*' />
        public override void AppendLiteralString(String text)
        {
            if (_doLiteralText)
            {
                if (LiteralTextParser.IsValidText(text))
                {
                    TextParser.Parse(text);
                }
            }
            else
            {
                base.AppendLiteralString(text);
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceTemplateBuilder.AppendSubBuilder"]/*' />
        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            if (_doLiteralText)
            {
                // The first one is used if ASP.NET is compiled with FAST_DATABINDING off. The second
                // is used if it is compiled with FAST_DATABINDING on. Note: We can't do a type 
                // comparison because CodeBlockBuilder is internal.
                // if (typeof(DataBoundLiteralControl).IsAssignableFrom(subBuilder.ControlType))
                if (subBuilder.GetType().FullName == "System.Web.UI.CodeBlockBuilder")
                {
                    TextParser.AddDataBinding(subBuilder);
                }
                else
                {
                    base.AppendSubBuilder(subBuilder);
                    if (subBuilder.ControlType != typeof(LiteralText))
                    {
                        if (_textParser != null)
                        {
                            _textParser.ResetBreaking();
                        }
                        else
                        {
                            _controlsInserted = true;
                        }
                    }
                }
            }
            else
            {
                base.AppendSubBuilder(subBuilder);
            }
        }
    }

    /*
     * DeviceSpecificChoiceTemplateContainer - "dummy" container object for 
     *      a template that goes inside a Choice. Once the Choice receives and
     *      extracts the information out of it, this object is simply discarded.
     *      See note on "Template Bag" above.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceTemplateContainer"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class DeviceSpecificChoiceTemplateContainer
    {
        private ITemplate _template;
        private String _name;

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceTemplateContainer.Template"]/*' />
        [
            Filterable(false),
            TemplateContainer(typeof(TemplateContainer)),
        ]
        public ITemplate Template
        {
            get
            {
                return _template;
            }
            set
            {
                _template = value;
            }
        }

        /// <include file='doc\DeviceSpecificChoice.uex' path='docs/doc[@for="DeviceSpecificChoiceTemplateContainer.Name"]/*' />
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
    }
}
