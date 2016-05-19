// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Security.Permissions;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules.Design
{

    internal abstract class RuleDefinitionDynamicPropertyDescriptor : DynamicPropertyDescriptor
    {
        public RuleDefinitionDynamicPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor)
            : base(serviceProvider, descriptor)
        {
        }

        protected RuleDefinitions GetRuleDefinitions(object component)
        {
            IReferenceService referenceService = ((IReferenceService)this.ServiceProvider.GetService(typeof(IReferenceService)));
            if (referenceService == null)
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(IReferenceService).FullName));

            Activity activity = referenceService.GetComponent(component) as Activity;
            if (activity == null)
                return null;

            Activity root = Helpers.GetRootActivity(activity);
            if (root == null)
                return null;

            Activity declaring = Helpers.GetDeclaringActivity(activity);
            if (declaring == root || declaring == null)
                return ConditionHelper.Load_Rules_DT(this.ServiceProvider, root);
            else
                return ConditionHelper.GetRuleDefinitionsFromManifest(declaring.GetType());
        }
    }

    #region Class RuleConditionReferenceTypeConverter
    internal class RuleConditionReferenceTypeConverter : TypeConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection newProps = new PropertyDescriptorCollection(null);
            newProps.Add(new RuleConditionReferenceNamePropertyDescriptor(context, TypeDescriptor.CreateProperty(typeof(RuleConditionReference), "ConditionName", typeof(string), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content), DesignOnlyAttribute.Yes)));
            newProps.Add(new RuleConditionReferencePropertyDescriptor(context, TypeDescriptor.CreateProperty(typeof(RuleConditionReference), "Expression", typeof(CodeExpression), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content), DesignOnlyAttribute.Yes)));

            return newProps.Sort(new string[] { "ConditionName", "Expression" });
        }
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    #endregion

    #region Class CodeDomRuleExpressionTypeConverter
    internal class RuleConditionReferenceExpressionTypeConverter : TypeConverter
    {
        internal RuleConditionReferenceExpressionTypeConverter()
        {
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            CodeExpression expression = value as CodeExpression;
            if (expression == null)
                return Messages.ConditionExpression;

            return new RuleExpressionCondition(expression).ToString();
        }
    }

    #endregion

    #region Class RuleSetReferenceTypeConverter
    internal class RuleSetReferenceTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object valueToConvert)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            string ruleSetName = valueToConvert as string;
            if ((ruleSetName == null) || (ruleSetName.TrimEnd().Length == 0))
                ruleSetName = string.Empty;

            ISite site = PropertyDescriptorUtils.GetSite(context, context.Instance);
            if (site == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(ISite).FullName);
                throw new InvalidOperationException(message);
            }

            RuleSetCollection ruleSetCollection = null;
            RuleDefinitions rules = ConditionHelper.Load_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
            if (rules != null)
                ruleSetCollection = rules.RuleSets;

            if (ruleSetCollection != null && ruleSetName.Length != 0 && !ruleSetCollection.Contains(ruleSetName))
            {
                //in this case, RuleExpressionCondition is the only type allowed in the ruleConditionCollection 
                RuleSet newRuleSet = new RuleSet();
                newRuleSet.Name = ruleSetName;
                ruleSetCollection.Add(newRuleSet);
                ConditionHelper.Flush_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
            }

            RuleSetReference ruleSetReference = new RuleSetReference();
            ruleSetReference.RuleSetName = ruleSetName;

            return ruleSetReference;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if (destinationType != typeof(string))
                return base.ConvertTo(context, culture, value, destinationType);

            RuleSetReference convertedValue = value as RuleSetReference;

            if (convertedValue != null)
                return convertedValue.RuleSetName;

            return null;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            ISite site = null;
            IComponent component = PropertyDescriptorUtils.GetComponent(context);
            if (component != null)
                site = component.Site;

            PropertyDescriptorCollection newProps = new PropertyDescriptorCollection(null);
            newProps.Add(new RuleSetPropertyDescriptor(site, TypeDescriptor.CreateProperty(typeof(RuleSet), "RuleSet Definition", typeof(RuleSet), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content), DesignOnlyAttribute.Yes)));

            return newProps;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
    #endregion

    #region Class RuleSetDefinitionTypeConverter
    internal class RuleSetDefinitionTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            RuleSet rs = value as RuleSet;
            if (destinationType == typeof(string) && rs != null)
                return DesignerHelpers.GetRuleSetPreview(rs);
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion

    #region Class CodeDomRuleNamePropertyDescriptor
    internal class RuleConditionReferenceNamePropertyDescriptor : DynamicPropertyDescriptor
    {
        public RuleConditionReferenceNamePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor)
            : base(serviceProvider, descriptor)
        {
        }

        public override object GetEditor(Type editorBaseType)
        {
            SecurityPermission MyPermission = new SecurityPermission(PermissionState.Unrestricted);
            MyPermission.Demand();

            return new ConditionNameEditor();
        }

        public override string Description
        {
            get
            {
                return Messages.NamePropertyDescription;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override object GetValue(object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            RuleConditionReference conditionDecl = component as RuleConditionReference;
            if (conditionDecl == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.NotARuleConditionReference, "component"), "component");

            if (conditionDecl.ConditionName != null)
                return conditionDecl.ConditionName;

            return null;
        }

        public override void SetValue(object component, object value)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            RuleConditionReference conditionDecl = component as RuleConditionReference;
            if (conditionDecl == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.NotARuleConditionReference, "component"), "component");

            string conditionName = value as string;
            if ((conditionName == null) || (conditionName.TrimEnd().Length == 0))
                conditionName = string.Empty;

            ISite site = PropertyDescriptorUtils.GetSite(this.ServiceProvider, component);
            if (site == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(ISite).FullName);
                throw new InvalidOperationException(message);
            }

            RuleConditionCollection conditionDefinitions = null;
            RuleDefinitions rules = ConditionHelper.Load_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
            if (rules != null)
                conditionDefinitions = rules.Conditions;

            if (conditionDefinitions != null && conditionName.Length != 0 && !conditionDefinitions.Contains(conditionName))
            {
                //in this case, RuleExpressionCondition is the only type allowed in the ruleConditionCollection 
                RuleExpressionCondition newCondition = new RuleExpressionCondition();
                newCondition.Name = conditionName;
                conditionDefinitions.Add(newCondition);
                ConditionHelper.Flush_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
            }

            // Cause component change events to be fired.
            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(component)["ConditionName"];
            if (propertyDescriptor != null)
                PropertyDescriptorUtils.SetPropertyValue(site, propertyDescriptor, component, conditionName);
        }
    }
    #endregion

    #region Class CodeDomRuleExpressionPropertyDescriptor
    internal class RuleConditionReferencePropertyDescriptor : RuleDefinitionDynamicPropertyDescriptor
    {
        public RuleConditionReferencePropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor)
            : base(serviceProvider, descriptor)
        {
        }

        public override TypeConverter Converter
        {
            get
            {
                return new RuleConditionReferenceExpressionTypeConverter();
            }
        }

        public override string Description
        {
            get
            {
                return Messages.ExpressionPropertyDescription;
            }
        }

        public override object GetEditor(Type editorBaseType)
        {
            SecurityPermission MyPermission = new SecurityPermission(PermissionState.Unrestricted);
            MyPermission.Demand();

            return new LogicalExpressionEditor();
        }

        public override object GetValue(object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            RuleConditionReference conditionDecl = component as RuleConditionReference;
            if (conditionDecl == null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Messages.NotARuleConditionReference, "component"), "component");

            if (conditionDecl.ConditionName != null)
            {
                RuleDefinitions rules = GetRuleDefinitions(component);
                if (rules != null)
                {
                    RuleConditionCollection conditionDefs = rules.Conditions;
                    if (conditionDefs != null && conditionDefs.Contains(conditionDecl.ConditionName))
                    {
                        //in this case, RuleExpressionCondition is the only type allowed in the ruleConditionCollection 
                        RuleExpressionCondition conditionDefinition = (RuleExpressionCondition)conditionDefs[conditionDecl.ConditionName];
                        return conditionDefinition.Expression;
                    }
                }
            }
            return null;
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }


        public override void SetValue(object component, object value)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            RuleConditionReference conditionDecl = component as RuleConditionReference;
            if (conditionDecl == null)
                throw new ArgumentNullException("component");

            CodeExpression expression = value as CodeExpression;
            if (conditionDecl.ConditionName != null)
            {
                ISite site = PropertyDescriptorUtils.GetSite(this.ServiceProvider, component);
                if (site == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(ISite).FullName);
                    throw new InvalidOperationException(message);
                }

                RuleConditionCollection conditionDefs = null;
                RuleDefinitions rules = ConditionHelper.Load_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
                if (rules != null)
                    conditionDefs = rules.Conditions;

                if (conditionDefs != null && conditionDefs.Contains(conditionDecl.ConditionName))
                {
                    //in this case, RuleExpressionCondition is the only type allowed in the ruleConditionCollection 
                    RuleExpressionCondition conditionDefinition = (RuleExpressionCondition)conditionDefs[conditionDecl.ConditionName];
                    conditionDefinition.Expression = expression;
                    ConditionHelper.Flush_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
                }
            }
        }
    }
    #endregion

    #region Class RuleSetPropertyDescriptor
    internal class RuleSetPropertyDescriptor : RuleDefinitionDynamicPropertyDescriptor
    {
        public RuleSetPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor descriptor)
            : base(serviceProvider, descriptor)
        {
        }

        public override TypeConverter Converter
        {
            get
            {
                return new RuleSetDefinitionTypeConverter();
            }
        }

        public override string Description
        {
            get
            {
                return SR.GetString(SR.RuleSetDefinitionDescription);
            }
        }

        public override object GetEditor(Type editorBaseType)
        {
            SecurityPermission MyPermission = new SecurityPermission(PermissionState.Unrestricted);
            MyPermission.Demand();

            return new RuleSetDefinitionEditor();
        }

        public override object GetValue(object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            RuleSetReference ruleSetReference = component as RuleSetReference;
            if (!string.IsNullOrEmpty(ruleSetReference.RuleSetName))
            {
                RuleDefinitions rules = GetRuleDefinitions(component);
                if (rules != null)
                {
                    RuleSetCollection ruleSetCollection = rules.RuleSets;
                    if (ruleSetCollection != null && ruleSetCollection.Contains(ruleSetReference.RuleSetName))
                    {
                        //in this case, RuleExpressionCondition is the only type allowed in the ruleConditionCollection 
                        RuleSet ruleSet = ruleSetCollection[ruleSetReference.RuleSetName];
                        return ruleSet;
                    }
                }
            }
            return null;
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }


        public override void SetValue(object component, object value)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            RuleSetReference ruleSetReference = component as RuleSetReference;
            if (ruleSetReference == null)
                throw new ArgumentNullException("component");

            RuleSet ruleSet = value as RuleSet;
            if (!string.IsNullOrEmpty(ruleSetReference.RuleSetName))
            {
                ISite site = PropertyDescriptorUtils.GetSite(this.ServiceProvider, component);
                if (site == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.MissingService, typeof(ISite).FullName);
                    throw new InvalidOperationException(message);
                }

                RuleSetCollection ruleSetCollection = null;
                RuleDefinitions rules = ConditionHelper.Load_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
                if (rules != null)
                    ruleSetCollection = rules.RuleSets;

                if (ruleSetCollection != null && ruleSetCollection.Contains(ruleSetReference.RuleSetName))
                {
                    ruleSetCollection.Remove(ruleSetReference.RuleSetName);
                    ruleSetCollection.Add(ruleSet);
                    ConditionHelper.Flush_Rules_DT(site, Helpers.GetRootActivity(site.Component as Activity));
                }
            }
        }

    }

    #endregion
}
