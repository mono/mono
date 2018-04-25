namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class DependencyObjectValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            ValidationErrorCollection validationErrors = base.Validate(manager, obj);
            DependencyObject dependencyObject = obj as DependencyObject;
            if (dependencyObject == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(DependencyObject).FullName), "obj");

            ArrayList allProperties = new ArrayList();

            // Validate all the settable dependency properties
            // attached property can not be found through the call to DependencyProperty.FromType()
            foreach (DependencyProperty prop in DependencyProperty.FromType(dependencyObject.GetType()))
            {
                // This property is attached to some other object.  We should not validate it here
                // because the context is wrong.
                if (!prop.IsAttached)
                    allProperties.Add(prop);
            }
            // 


            foreach (DependencyProperty prop in dependencyObject.MetaDependencyProperties)
            {
                if (prop.IsAttached)
                {
                    if (obj.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance) == null)
                        allProperties.Add(prop);
                }
            }

            foreach (DependencyProperty prop in allProperties)
            {
                object[] validationVisibilityAtrributes = prop.DefaultMetadata.GetAttributes(typeof(ValidationOptionAttribute));
                ValidationOption validationVisibility = (validationVisibilityAtrributes.Length > 0) ? ((ValidationOptionAttribute)validationVisibilityAtrributes[0]).ValidationOption : ValidationOption.Optional;
                if (validationVisibility != ValidationOption.None)
                    validationErrors.AddRange(ValidateDependencyProperty(dependencyObject, prop, manager));
            }

            return validationErrors;
        }

        private ValidationErrorCollection ValidateDependencyProperty(DependencyObject dependencyObject, DependencyProperty dependencyProperty, ValidationManager manager)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();

            Attribute[] validationVisibilityAtrributes = dependencyProperty.DefaultMetadata.GetAttributes(typeof(ValidationOptionAttribute));
            ValidationOption validationVisibility = (validationVisibilityAtrributes.Length > 0) ? ((ValidationOptionAttribute)validationVisibilityAtrributes[0]).ValidationOption : ValidationOption.Optional;

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).FullName));

            PropertyValidationContext propertyValidationContext = new PropertyValidationContext(activity, dependencyProperty);
            manager.Context.Push(propertyValidationContext);

            try
            {
                if (dependencyProperty.DefaultMetadata.DefaultValue != null)
                {
                    if (!dependencyProperty.PropertyType.IsValueType &&
                        dependencyProperty.PropertyType != typeof(string))
                    {
                        errors.Add(new ValidationError(SR.GetString(SR.Error_PropertyDefaultIsReference, dependencyProperty.Name), ErrorNumbers.Error_PropertyDefaultIsReference));
                    }
                    else if (!dependencyProperty.PropertyType.IsAssignableFrom(dependencyProperty.DefaultMetadata.DefaultValue.GetType()))
                    {
                        errors.Add(new ValidationError(SR.GetString(SR.Error_PropertyDefaultTypeMismatch, dependencyProperty.Name, dependencyProperty.PropertyType.FullName, dependencyProperty.DefaultMetadata.DefaultValue.GetType().FullName), ErrorNumbers.Error_PropertyDefaultTypeMismatch));
                    }
                }

                // If an event is of type Bind, GetBinding will return the Bind object.
                object propValue = null;
                if (dependencyObject.IsBindingSet(dependencyProperty))
                    propValue = dependencyObject.GetBinding(dependencyProperty);
                else if (!dependencyProperty.IsEvent)
                    propValue = dependencyObject.GetValue(dependencyProperty);

                if (propValue == null || propValue == dependencyProperty.DefaultMetadata.DefaultValue)
                {
                    if (dependencyProperty.IsEvent)
                    {
                        // Is this added through "+=" in InitializeComponent?  If so, the value should be in the instance properties hashtable
                        // If none of these, its value is stored in UserData at design time.
                        propValue = dependencyObject.GetHandler(dependencyProperty);
                        if (propValue == null)
                            propValue = WorkflowMarkupSerializationHelpers.GetEventHandlerName(dependencyObject, dependencyProperty.Name);

                        if (propValue is string && !string.IsNullOrEmpty((string)propValue))
                            errors.AddRange(ValidateEvent(activity, dependencyProperty, propValue, manager));
                    }
                    else
                    {
                        // Maybe this is an instance property.
                        propValue = dependencyObject.GetValue(dependencyProperty);
                    }
                }

                // Be careful before changing this. This is the (P || C) validation validation
                // i.e. validate properties being set only if Parent is set or 
                // a child exists.
                bool checkNotSet = (activity.Parent != null) || ((activity is CompositeActivity) && (((CompositeActivity)activity).EnabledActivities.Count != 0));

                if (validationVisibility == ValidationOption.Required &&
                    (propValue == null || (propValue is string && string.IsNullOrEmpty((string)propValue))) &&
                    (dependencyProperty.DefaultMetadata.IsMetaProperty) &&
                    checkNotSet)
                {
                    errors.Add(ValidationError.GetNotSetValidationError(GetFullPropertyName(manager)));
                }
                else if (propValue != null)
                {
                    if (propValue is IList)
                    {
                        PropertyValidationContext childContext = new PropertyValidationContext(propValue, null, String.Empty);
                        manager.Context.Push(childContext);

                        try
                        {
                            foreach (object child in (IList)propValue)
                                errors.AddRange(ValidationHelpers.ValidateObject(manager, child));
                        }
                        finally
                        {
                            System.Diagnostics.Debug.Assert(manager.Context.Current == childContext, "Unwinding contextStack: the item that is about to be popped is not the one we pushed.");
                            manager.Context.Pop();
                        }
                    }
                    else if (dependencyProperty.ValidatorType != null)
                    {
                        Validator validator = null;
                        try
                        {
                            validator = Activator.CreateInstance(dependencyProperty.ValidatorType) as Validator;
                            if (validator == null)
                                errors.Add(new ValidationError(SR.GetString(SR.Error_CreateValidator, dependencyProperty.ValidatorType.FullName), ErrorNumbers.Error_CreateValidator));
                            else
                                errors.AddRange(validator.Validate(manager, propValue));
                        }
                        catch
                        {
                            errors.Add(new ValidationError(SR.GetString(SR.Error_CreateValidator, dependencyProperty.ValidatorType.FullName), ErrorNumbers.Error_CreateValidator));
                        }
                    }
                    else
                    {
                        errors.AddRange(ValidationHelpers.ValidateObject(manager, propValue));
                    }
                }
            }
            finally
            {
                System.Diagnostics.Debug.Assert(manager.Context.Current == propertyValidationContext, "Unwinding contextStack: the item that is about to be popped is not the one we pushed.");
                manager.Context.Pop();
            }

            return errors;
        }

        private ValidationErrorCollection ValidateEvent(Activity activity, DependencyProperty dependencyProperty, object propValue, ValidationManager manager)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            if (propValue is string && !string.IsNullOrEmpty((string)propValue))
            {
                bool handlerExists = false;
                Type objType = null;
                Activity rootActivity = Helpers.GetRootActivity(activity);
                Activity enclosingActivity = Helpers.GetEnclosingActivity(activity);
                string typeName = rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
                if (rootActivity == enclosingActivity && !string.IsNullOrEmpty(typeName))
                {
                    ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                    if (typeProvider == null)
                        throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                    objType = typeProvider.GetType(typeName);
                }
                else
                    objType = enclosingActivity.GetType();

                if (objType != null)
                {
                    MethodInfo invokeMethod = dependencyProperty.PropertyType.GetMethod("Invoke");
                    if (invokeMethod != null)
                    {
                        // resolve the method
                        List<Type> paramTypes = new List<Type>();
                        foreach (ParameterInfo paramInfo in invokeMethod.GetParameters())
                            paramTypes.Add(paramInfo.ParameterType);

                        MethodInfo methodInfo = Helpers.GetMethodExactMatch(objType, propValue as string, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, paramTypes.ToArray(), null);
                        if (methodInfo != null && TypeProvider.IsAssignable(invokeMethod.ReturnType, methodInfo.ReturnType))
                            handlerExists = true;
                    }
                }

                if (!handlerExists)
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_CantResolveEventHandler, dependencyProperty.Name, propValue as string), ErrorNumbers.Error_CantResolveEventHandler);
                    error.PropertyName = GetFullPropertyName(manager);
                    validationErrors.Add(error);
                }
            }

            return validationErrors;
        }
    }
}
