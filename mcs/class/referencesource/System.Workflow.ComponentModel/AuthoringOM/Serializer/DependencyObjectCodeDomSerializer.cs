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

    #region Class DependencyObjectCodeDomSerializer
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class DependencyObjectCodeDomSerializer : CodeDomSerializer
    {
        public DependencyObjectCodeDomSerializer()
        {
        }

        public override object Serialize(IDesignerSerializationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            if (manager.Context == null)
                throw new ArgumentException("manager", SR.GetString(SR.Error_MissingContextProperty));

            if (obj == null)
                throw new ArgumentNullException("obj");

            DependencyObject dependencyObject = obj as DependencyObject;
            if (dependencyObject == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(DependencyObject).FullName), "obj");

            Activity activity = obj as Activity;
            if (activity != null)
                manager.Context.Push(activity);

            CodeStatementCollection retVal = null;
            try
            {
                if (activity != null)
                {
                    CodeDomSerializer componentSerializer = manager.GetSerializer(typeof(Component), typeof(CodeDomSerializer)) as CodeDomSerializer;
                    if (componentSerializer == null)
                        throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(CodeDomSerializer).FullName));
                    retVal = componentSerializer.Serialize(manager, activity) as CodeStatementCollection;
                }
                else
                {
                    retVal = base.Serialize(manager, obj) as CodeStatementCollection;
                }

                if (retVal != null)
                {
                    CodeStatementCollection codeStatements = new CodeStatementCollection(retVal);
                    CodeExpression objectExpression = SerializeToExpression(manager, obj);
                    if (objectExpression != null)
                    {
                        ArrayList propertiesSerialized = new ArrayList();
                        List<DependencyProperty> dependencyProperties = new List<DependencyProperty>(dependencyObject.MetaDependencyProperties);
                        foreach (DependencyProperty dp in dependencyObject.DependencyPropertyValues.Keys)
                        {
                            if (dp.IsAttached)
                            {
                                if ((dp.IsEvent && dp.OwnerType.GetField(dp.Name + "Event", BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly) != null) ||
                                    (!dp.IsEvent && dp.OwnerType.GetField(dp.Name + "Property", BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly) != null))
                                    dependencyProperties.Add(dp);
                            }
                        }
                        foreach (DependencyProperty dependencyProperty in dependencyProperties)
                        {
                            object value = null;
                            if (dependencyObject.IsBindingSet(dependencyProperty))
                                value = dependencyObject.GetBinding(dependencyProperty);
                            else if (!dependencyProperty.IsEvent)
                                value = dependencyObject.GetValue(dependencyProperty);
                            else
                                value = dependencyObject.GetHandler(dependencyProperty);
                            // Attached property should always be set through SetValue, no matter if it's a meta property or if there is a data context.
                            // Other meta properties will be directly assigned.
                            // Other instance property will go through SetValue if there is a data context or if it's of type Bind.
                            if (value != null &&
                                (dependencyProperty.IsAttached || (!dependencyProperty.DefaultMetadata.IsMetaProperty && value is ActivityBind)))
                            {
                                object[] attributes = dependencyProperty.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                                if (attributes.Length > 0)
                                {
                                    DesignerSerializationVisibilityAttribute serializationVisibilityAttribute = attributes[0] as DesignerSerializationVisibilityAttribute;
                                    if (serializationVisibilityAttribute.Visibility == DesignerSerializationVisibility.Hidden)
                                        continue;
                                }

                                // Events of type Bind will go through here.  Regular events will go through IEventBindingService.
                                CodeExpression param1 = null;
                                string dependencyPropertyName = dependencyProperty.Name + ((dependencyProperty.IsEvent) ? "Event" : "Property");
                                FieldInfo fieldInfo = dependencyProperty.OwnerType.GetField(dependencyPropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                                if (fieldInfo != null && !fieldInfo.IsPublic)
                                    param1 = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(DependencyProperty)), "FromName", new CodePrimitiveExpression(dependencyProperty.Name), new CodeTypeOfExpression(dependencyProperty.OwnerType));
                                else
                                    param1 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(dependencyProperty.OwnerType), dependencyPropertyName);

                                CodeExpression param2 = SerializeToExpression(manager, value);

                                //Fields property fails to serialize to expression due to reference not being created,
                                //the actual code for fields are generated in datacontext code generator so we do nothing here
                                if (param1 != null && param2 != null)
                                {
                                    CodeMethodInvokeExpression codeMethodInvokeExpr = null;
                                    if (value is ActivityBind)
                                        codeMethodInvokeExpr = new CodeMethodInvokeExpression(objectExpression, "SetBinding", new CodeExpression[] { param1, new CodeCastExpression(new CodeTypeReference(typeof(ActivityBind)), param2) });
                                    else
                                        codeMethodInvokeExpr = new CodeMethodInvokeExpression(objectExpression, (dependencyProperty.IsEvent) ? "AddHandler" : "SetValue", new CodeExpression[] { param1, param2 });
                                    retVal.Add(codeMethodInvokeExpr);

                                    // Remove the property set statement for the event which is replaced by the SetValue() expression.
                                    foreach (CodeStatement statement in codeStatements)
                                    {
                                        if (statement is CodeAssignStatement && ((CodeAssignStatement)statement).Left is CodePropertyReferenceExpression)
                                        {
                                            CodePropertyReferenceExpression prop = ((CodeAssignStatement)statement).Left as CodePropertyReferenceExpression;
                                            if (prop.PropertyName == dependencyProperty.Name && prop.TargetObject.Equals(objectExpression))
                                                retVal.Remove(statement);
                                        }
                                    }
                                }

                                propertiesSerialized.Add(dependencyProperty);
                            }
                        }

                        IEventBindingService eventBindingService = manager.GetService(typeof(IEventBindingService)) as IEventBindingService;
                        if (eventBindingService == null)
                        {
                            // At compile time, we don't have an event binding service.  We need to mannually emit the code to add
                            // event handlers.
                            foreach (EventDescriptor eventDesc in TypeDescriptor.GetEvents(dependencyObject))
                            {
                                string handler = WorkflowMarkupSerializationHelpers.GetEventHandlerName(dependencyObject, eventDesc.Name);
                                if (!string.IsNullOrEmpty(handler))
                                {
                                    CodeEventReferenceExpression eventRef = new CodeEventReferenceExpression(objectExpression, eventDesc.Name);
                                    CodeDelegateCreateExpression listener = new CodeDelegateCreateExpression(new CodeTypeReference(eventDesc.EventType), new CodeThisReferenceExpression(), handler);
                                    retVal.Add(new CodeAttachEventStatement(eventRef, listener));
                                }
                            }
                        }

                        // We also need to handle properties of type System.Type.  If the value is a design time type, xomlserializer 
                        // is not going to be able to deserialize the type.  We then store the type name in the user data and
                        // output a "typeof(xxx)" expression using the type name w/o validating the type.
                        if (dependencyObject.UserData.Contains(UserDataKeys.DesignTimeTypeNames))
                        {
                            Hashtable typeNames = dependencyObject.UserData[UserDataKeys.DesignTimeTypeNames] as Hashtable;
                            foreach (object key in typeNames.Keys)
                            {
                                string propName = null;
                                string ownerTypeName = null;
                                string typeName = typeNames[key] as string;
                                DependencyProperty dependencyProperty = key as DependencyProperty;
                                if (dependencyProperty != null)
                                {
                                    if (propertiesSerialized.Contains(dependencyProperty))
                                        continue;

                                    object[] attributes = dependencyProperty.DefaultMetadata.GetAttributes(typeof(DesignerSerializationVisibilityAttribute));
                                    if (attributes.Length > 0)
                                    {
                                        DesignerSerializationVisibilityAttribute serializationVisibilityAttribute = attributes[0] as DesignerSerializationVisibilityAttribute;
                                        if (serializationVisibilityAttribute.Visibility == DesignerSerializationVisibility.Hidden)
                                            continue;
                                    }

                                    propName = dependencyProperty.Name;
                                    ownerTypeName = dependencyProperty.OwnerType.FullName;
                                }
                                else if (key is string)
                                {
                                    int indexOfDot = ((string)key).LastIndexOf('.');
                                    Debug.Assert(indexOfDot != -1, "Wrong property name in DesignTimeTypeNames hashtable.");
                                    if (indexOfDot != -1)
                                    {
                                        ownerTypeName = ((string)key).Substring(0, indexOfDot);
                                        propName = ((string)key).Substring(indexOfDot + 1);
                                    }
                                }

                                if (!string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(propName) && !string.IsNullOrEmpty(ownerTypeName))
                                {
                                    if (ownerTypeName == obj.GetType().FullName)
                                    {
                                        // Property is not an attached property.  Serialize using regular property set expression.
                                        CodePropertyReferenceExpression propertyRef = new CodePropertyReferenceExpression(objectExpression, propName);
                                        retVal.Add(new CodeAssignStatement(propertyRef, new CodeTypeOfExpression(typeName)));
                                    }
                                    else
                                    {
                                        // This is an attached property.  Serialize using SetValue() expression.
                                        CodeExpression param1 = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(ownerTypeName), propName + "Property");
                                        CodeExpression param2 = new CodeTypeOfExpression(typeName);
                                        retVal.Add(new CodeMethodInvokeExpression(objectExpression, "SetValue", new CodeExpression[] { param1, param2 }));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (activity != null)
                {
                    object pushedActivity = manager.Context.Pop();
                    System.Diagnostics.Debug.Assert(pushedActivity == activity);
                }
            }

            return retVal;
        }
    }
    #endregion

}
