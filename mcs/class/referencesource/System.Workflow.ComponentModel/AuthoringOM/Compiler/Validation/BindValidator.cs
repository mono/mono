namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;

    #region Class BindValidator
    internal static class BindValidatorHelper
    {
        internal static Type GetActivityType(IServiceProvider serviceProvider, Activity refActivity)
        {
            Type type = null;
            string typeName = refActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string;
            if (refActivity.Site != null && !string.IsNullOrEmpty(typeName))
            {
                ITypeProvider typeProvider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider != null && !string.IsNullOrEmpty(typeName))
                    type = typeProvider.GetType(typeName, false);
            }
            else
            {
                type = refActivity.GetType();
            }

            return type;
        }
    }

    #endregion

    #region Class FieldBindValidator
    internal sealed class FieldBindValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            FieldBind bind = obj as FieldBind;
            if (bind == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(FieldBind).FullName), "obj");

            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(BindValidationContext).Name));

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).Name));

            ValidationError error = null;
            if (string.IsNullOrEmpty(bind.Name))
            {
                error = new ValidationError(SR.GetString(SR.Error_PropertyNotSet, "Name"), ErrorNumbers.Error_PropertyNotSet);
                error.PropertyName = GetFullPropertyName(manager) + ".Name";
            }
            else
            {
                BindValidationContext validationBindContext = manager.Context[typeof(BindValidationContext)] as BindValidationContext;
                if (validationBindContext == null)
                {
                    Type baseType = BindHelpers.GetBaseType(manager, validationContext);
                    if (baseType != null)
                    {
                        AccessTypes accessType = BindHelpers.GetAccessType(manager, validationContext);
                        validationBindContext = new BindValidationContext(baseType, accessType);
                    }
                    //else
                    //{
                    //    error = new ValidationError(SR.GetString(SR.Error_BindBaseTypeNotSpecified, validationContext.PropertyName), ErrorNumbers.Error_BindBaseTypeNotSpecified);
                    //    error.PropertyName = GetFullPropertyName(manager) + ".Name";
                    //}
                }
                if (validationBindContext != null)
                {
                    Type targetType = validationBindContext.TargetType;
                    if (error == null)
                        validationErrors.AddRange(this.ValidateField(manager, activity, bind, new BindValidationContext(targetType, validationBindContext.Access)));
                }
            }
            if (error != null)
                validationErrors.Add(error);

            return validationErrors;
        }

        private ValidationErrorCollection ValidateField(ValidationManager manager, Activity activity, FieldBind bind, BindValidationContext validationContext)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            string dsName = bind.Name;
            Activity activityContext = Helpers.GetEnclosingActivity(activity);
            Activity dataSourceActivity = activityContext;
            if (dsName.IndexOf('.') != -1 && dataSourceActivity != null)
                dataSourceActivity = Helpers.GetDataSourceActivity(activity, bind.Name, out dsName);

            if (dataSourceActivity == null)
            {
                ValidationError error = new ValidationError(SR.GetString(SR.Error_NoEnclosingContext, activity.Name), ErrorNumbers.Error_NoEnclosingContext);
                error.PropertyName = GetFullPropertyName(manager) + ".Name";
                validationErrors.Add(error);
            }
            else
            {
                ValidationError error = null;
                // 
                System.Type resolvedType = Helpers.GetDataSourceClass(dataSourceActivity, manager);
                if (resolvedType == null)
                {
                    error = new ValidationError(SR.GetString(SR.Error_TypeNotResolvedInFieldName, "Name"), ErrorNumbers.Error_TypeNotResolvedInFieldName);
                    error.PropertyName = GetFullPropertyName(manager);
                }
                else
                {
                    FieldInfo fieldInfo = resolvedType.GetField(dsName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    if (fieldInfo == null)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_FieldNotExists, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_FieldNotExists);
                        error.PropertyName = GetFullPropertyName(manager);
                    }
                    //else if (dataSourceActivity != activityContext && !fieldInfo.IsAssembly && !fieldInfo.IsPublic)
                    //{
                    //    error = new ValidationError(SR.GetString(SR.Error_FieldNotAccessible, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_FieldNotAccessible);
                    //    error.PropertyName = GetFullPropertyName(manager);
                    //}
                    else if (fieldInfo.FieldType == null)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_FieldTypeNotResolved, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_FieldTypeNotResolved);
                        error.PropertyName = GetFullPropertyName(manager);
                    }
                    else
                    {
                        MemberInfo memberInfo = fieldInfo;
                        if ((bind.Path == null || bind.Path.Length == 0) && (validationContext.TargetType != null && !ActivityBindValidator.DoesTargetTypeMatch(validationContext.TargetType, fieldInfo.FieldType, validationContext.Access)))
                        {
                            error = new ValidationError(SR.GetString(SR.Error_FieldTypeMismatch, GetFullPropertyName(manager), fieldInfo.FieldType.FullName, validationContext.TargetType.FullName), ErrorNumbers.Error_FieldTypeMismatch);
                            error.PropertyName = GetFullPropertyName(manager);
                        }
                        else if (!string.IsNullOrEmpty(bind.Path))
                        {
                            memberInfo = MemberBind.GetMemberInfo(fieldInfo.FieldType, bind.Path);
                            if (memberInfo == null)
                            {
                                error = new ValidationError(SR.GetString(SR.Error_InvalidMemberPath, dsName, bind.Path), ErrorNumbers.Error_InvalidMemberPath);
                                error.PropertyName = GetFullPropertyName(manager) + ".Path";
                            }
                            else
                            {
                                IDisposable localContextScope = (WorkflowCompilationContext.Current == null ? WorkflowCompilationContext.CreateScope(manager) : null);
                                try
                                {
                                    if (WorkflowCompilationContext.Current.CheckTypes)
                                    {
                                        error = MemberBind.ValidateTypesInPath(fieldInfo.FieldType, bind.Path);
                                        if (error != null)
                                            error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                    }
                                }
                                finally
                                {
                                    if (localContextScope != null)
                                    {
                                        localContextScope.Dispose();
                                    }
                                }

                                if (error == null)
                                {
                                    Type memberType = (memberInfo is FieldInfo ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType);
                                    if (!ActivityBindValidator.DoesTargetTypeMatch(validationContext.TargetType, memberType, validationContext.Access))
                                    {
                                        error = new ValidationError(SR.GetString(SR.Error_TargetTypeDataSourcePathMismatch, validationContext.TargetType.FullName), ErrorNumbers.Error_TargetTypeDataSourcePathMismatch);
                                        error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                    }
                                }
                            }
                        }
                        if (error == null)
                        {
                            if (memberInfo is PropertyInfo)
                            {
                                PropertyInfo pathPropertyInfo = memberInfo as PropertyInfo;
                                if (!pathPropertyInfo.CanRead && ((validationContext.Access & AccessTypes.Read) != 0))
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_PropertyNoGetter, pathPropertyInfo.Name, bind.Path), ErrorNumbers.Error_PropertyNoGetter);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                                else if (!pathPropertyInfo.CanWrite && ((validationContext.Access & AccessTypes.Write) != 0))
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_PropertyNoSetter, pathPropertyInfo.Name, bind.Path), ErrorNumbers.Error_PropertyNoSetter);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                            }
                            else if (memberInfo is FieldInfo)
                            {
                                FieldInfo pathFieldInfo = memberInfo as FieldInfo;
                                if (((pathFieldInfo.Attributes & (FieldAttributes.InitOnly | FieldAttributes.Literal)) != 0) &&
                                    ((validationContext.Access & AccessTypes.Write) != 0))
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_ReadOnlyField, pathFieldInfo.Name), ErrorNumbers.Error_ReadOnlyField);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                            }
                        }
                    }
                }
                if (error != null)
                    validationErrors.Add(error);
            }
            return validationErrors;
        }
    }

    #endregion

    #region Class PropertyBindValidator

    internal sealed class PropertyBindValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            PropertyBind bind = obj as PropertyBind;
            if (bind == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(PropertyBind).FullName), "obj");

            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(BindValidationContext).Name));

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).Name));

            ValidationError error = null;
            if (string.IsNullOrEmpty(bind.Name))
            {
                error = new ValidationError(SR.GetString(SR.Error_PropertyNotSet, "Name"), ErrorNumbers.Error_PropertyNotSet);
                error.PropertyName = GetFullPropertyName(manager) + ".Name";
            }
            else
            {
                BindValidationContext validationBindContext = manager.Context[typeof(BindValidationContext)] as BindValidationContext;
                if (validationBindContext == null)
                {
                    Type baseType = BindHelpers.GetBaseType(manager, validationContext);
                    if (baseType != null)
                    {
                        AccessTypes accessType = BindHelpers.GetAccessType(manager, validationContext);
                        validationBindContext = new BindValidationContext(baseType, accessType);
                    }
                }
                if (validationBindContext != null)
                {
                    Type targetType = validationBindContext.TargetType;
                    if (error == null)
                        validationErrors.AddRange(this.ValidateBindProperty(manager, activity, bind, new BindValidationContext(targetType, validationBindContext.Access)));
                }
            }
            if (error != null)
                validationErrors.Add(error);

            return validationErrors;
        }

        private ValidationErrorCollection ValidateBindProperty(ValidationManager manager, Activity activity, PropertyBind bind, BindValidationContext validationContext)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            string dsName = bind.Name;
            Activity activityContext = Helpers.GetEnclosingActivity(activity);
            Activity dataSourceActivity = activityContext;
            if (dsName.IndexOf('.') != -1 && dataSourceActivity != null)
                dataSourceActivity = Helpers.GetDataSourceActivity(activity, bind.Name, out dsName);

            if (dataSourceActivity == null)
            {
                ValidationError error = new ValidationError(SR.GetString(SR.Error_NoEnclosingContext, activity.Name), ErrorNumbers.Error_NoEnclosingContext);
                error.PropertyName = GetFullPropertyName(manager) + ".Name";
                validationErrors.Add(error);
            }
            else
            {
                ValidationError error = null;

                PropertyInfo propertyInfo = null;
                System.Type resolvedType = null;
                if (propertyInfo == null)
                {
                    resolvedType = BindValidatorHelper.GetActivityType(manager, dataSourceActivity);
                    if (resolvedType != null)
                        propertyInfo = resolvedType.GetProperty(dsName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }

                if (resolvedType == null)
                {
                    error = new ValidationError(SR.GetString(SR.Error_TypeNotResolvedInPropertyName, "Name"), ErrorNumbers.Error_TypeNotResolvedInPropertyName);
                    error.PropertyName = GetFullPropertyName(manager);
                }
                else
                {
                    if (propertyInfo == null)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_PropertyNotExists, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_PropertyNotExists);
                        error.PropertyName = GetFullPropertyName(manager);
                    }
                    else if (!propertyInfo.CanRead)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_PropertyReferenceNoGetter, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_PropertyReferenceNoGetter);
                        error.PropertyName = GetFullPropertyName(manager);
                    }
                    else if (propertyInfo.GetGetMethod() == null)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_PropertyReferenceGetterNoAccess, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_PropertyReferenceGetterNoAccess);
                        error.PropertyName = GetFullPropertyName(manager);
                    }
                    else if (dataSourceActivity != activityContext && !propertyInfo.GetGetMethod().IsAssembly && !propertyInfo.GetGetMethod().IsPublic)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_PropertyNotAccessible, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_PropertyNotAccessible);
                        error.PropertyName = GetFullPropertyName(manager);
                    }
                    else if (propertyInfo.PropertyType == null)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_PropertyTypeNotResolved, GetFullPropertyName(manager), dsName), ErrorNumbers.Error_PropertyTypeNotResolved);
                        error.PropertyName = GetFullPropertyName(manager);
                    }
                    else
                    {
                        MemberInfo memberInfo = propertyInfo;
                        if ((bind.Path == null || bind.Path.Length == 0) && (validationContext.TargetType != null && !ActivityBindValidator.DoesTargetTypeMatch(validationContext.TargetType, propertyInfo.PropertyType, validationContext.Access)))
                        {
                            error = new ValidationError(SR.GetString(SR.Error_PropertyTypeMismatch, GetFullPropertyName(manager), propertyInfo.PropertyType.FullName, validationContext.TargetType.FullName), ErrorNumbers.Error_PropertyTypeMismatch);
                            error.PropertyName = GetFullPropertyName(manager);
                        }
                        else if (!string.IsNullOrEmpty(bind.Path))
                        {
                            memberInfo = MemberBind.GetMemberInfo(propertyInfo.PropertyType, bind.Path);
                            if (memberInfo == null)
                            {
                                error = new ValidationError(SR.GetString(SR.Error_InvalidMemberPath, dsName, bind.Path), ErrorNumbers.Error_InvalidMemberPath);
                                error.PropertyName = GetFullPropertyName(manager) + ".Path";
                            }
                            else
                            {
                                IDisposable localContextScope = (WorkflowCompilationContext.Current == null ? WorkflowCompilationContext.CreateScope(manager) : null);
                                try
                                {
                                    if (WorkflowCompilationContext.Current.CheckTypes)
                                    {
                                        error = MemberBind.ValidateTypesInPath(propertyInfo.PropertyType, bind.Path);
                                        if (error != null)
                                            error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                    }
                                }
                                finally
                                {
                                    if (localContextScope != null)
                                    {
                                        localContextScope.Dispose();
                                    }
                                }

                                if (error == null)
                                {
                                    Type memberType = (memberInfo is FieldInfo ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType);
                                    if (!ActivityBindValidator.DoesTargetTypeMatch(validationContext.TargetType, memberType, validationContext.Access))
                                    {
                                        error = new ValidationError(SR.GetString(SR.Error_TargetTypeDataSourcePathMismatch, validationContext.TargetType.FullName), ErrorNumbers.Error_TargetTypeDataSourcePathMismatch);
                                        error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                    }
                                }
                            }
                        }
                        if (error == null)
                        {
                            if (memberInfo is PropertyInfo)
                            {
                                PropertyInfo pathPropertyInfo = memberInfo as PropertyInfo;
                                if (!pathPropertyInfo.CanRead && ((validationContext.Access & AccessTypes.Read) != 0))
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_PropertyNoGetter, pathPropertyInfo.Name, bind.Path), ErrorNumbers.Error_PropertyNoGetter);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                                else if (!pathPropertyInfo.CanWrite && ((validationContext.Access & AccessTypes.Write) != 0))
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_PropertyNoSetter, pathPropertyInfo.Name, bind.Path), ErrorNumbers.Error_PropertyNoSetter);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                            }
                            else if (memberInfo is FieldInfo)
                            {
                                FieldInfo pathFieldInfo = memberInfo as FieldInfo;
                                if (((pathFieldInfo.Attributes & (FieldAttributes.InitOnly | FieldAttributes.Literal)) != 0) &&
                                    ((validationContext.Access & AccessTypes.Write) != 0))
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_ReadOnlyField, pathFieldInfo.Name), ErrorNumbers.Error_ReadOnlyField);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                            }
                        }
                    }
                }
                if (error != null)
                    validationErrors.Add(error);
            }
            return validationErrors;
        }
    }

    #endregion

    #region Class MethodBindValidator

    internal sealed class MethodBindValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            MethodBind bind = obj as MethodBind;
            if (bind == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(MethodBind).FullName), "obj");

            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(BindValidationContext).Name));

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).Name));

            ValidationError error = null;
            if (string.IsNullOrEmpty(bind.Name))
            {
                error = new ValidationError(SR.GetString(SR.Error_PropertyNotSet, "Name"), ErrorNumbers.Error_PropertyNotSet);
                error.PropertyName = GetFullPropertyName(manager) + ".Name";
            }
            else
            {
                BindValidationContext validationBindContext = manager.Context[typeof(BindValidationContext)] as BindValidationContext;
                if (validationBindContext == null)
                {
                    Type baseType = BindHelpers.GetBaseType(manager, validationContext);
                    if (baseType != null)
                    {
                        AccessTypes accessType = BindHelpers.GetAccessType(manager, validationContext);
                        validationBindContext = new BindValidationContext(baseType, accessType);
                    }
                    //else
                    //{
                    //    error = new ValidationError(SR.GetString(SR.Error_BindBaseTypeNotSpecified, validationContext.PropertyName), ErrorNumbers.Error_BindBaseTypeNotSpecified);
                    //    error.PropertyName = GetFullPropertyName(manager) + ".Name";
                    //}
                }
                if (validationBindContext != null)
                {
                    Type targetType = validationBindContext.TargetType;
                    if (error == null)
                        validationErrors.AddRange(this.ValidateMethod(manager, activity, bind, new BindValidationContext(targetType, validationBindContext.Access)));
                }
            }
            if (error != null)
                validationErrors.Add(error);

            return validationErrors;
        }

        private ValidationErrorCollection ValidateMethod(ValidationManager manager, Activity activity, MethodBind bind, BindValidationContext validationBindContext)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            if ((validationBindContext.Access & AccessTypes.Write) != 0)
            {
                ValidationError error = new ValidationError(SR.GetString(SR.Error_HandlerReadOnly), ErrorNumbers.Error_HandlerReadOnly);
                error.PropertyName = GetFullPropertyName(manager);
                validationErrors.Add(error);
            }
            else
            {
                if (!TypeProvider.IsAssignable(typeof(Delegate), validationBindContext.TargetType))
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_TypeNotDelegate, validationBindContext.TargetType.FullName), ErrorNumbers.Error_TypeNotDelegate);
                    error.PropertyName = GetFullPropertyName(manager);
                    validationErrors.Add(error);
                }
                else
                {
                    string dsName = bind.Name;
                    Activity activityContext = Helpers.GetEnclosingActivity(activity);
                    Activity dataSourceActivity = activityContext;
                    if (dsName.IndexOf('.') != -1 && dataSourceActivity != null)
                        dataSourceActivity = Helpers.GetDataSourceActivity(activity, bind.Name, out dsName);

                    if (dataSourceActivity == null)
                    {
                        ValidationError error = new ValidationError(SR.GetString(SR.Error_NoEnclosingContext, activity.Name), ErrorNumbers.Error_NoEnclosingContext);
                        error.PropertyName = GetFullPropertyName(manager) + ".Name";
                        validationErrors.Add(error);
                    }
                    else
                    {
                        string message = string.Empty;
                        int errorNumber = -1;
                        System.Type resolvedType = Helpers.GetDataSourceClass(dataSourceActivity, manager);
                        if (resolvedType == null)
                        {
                            message = SR.GetString(SR.Error_TypeNotResolvedInMethodName, GetFullPropertyName(manager) + ".Name");
                            errorNumber = ErrorNumbers.Error_TypeNotResolvedInMethodName;
                        }
                        else
                        {
                            try
                            {
                                ValidationHelpers.ValidateIdentifier(manager, dsName);
                            }
                            catch (Exception e)
                            {
                                validationErrors.Add(new ValidationError(e.Message, ErrorNumbers.Error_InvalidIdentifier));
                            }

                            // get the invoke method
                            MethodInfo invokeMethod = validationBindContext.TargetType.GetMethod("Invoke");
                            if (invokeMethod == null)
                                throw new Exception(SR.GetString(SR.Error_DelegateNoInvoke, validationBindContext.TargetType.FullName));

                            // resolve the method
                            List<Type> paramTypes = new List<Type>();
                            foreach (ParameterInfo paramInfo in invokeMethod.GetParameters())
                                paramTypes.Add(paramInfo.ParameterType);

                            MethodInfo methodInfo = Helpers.GetMethodExactMatch(resolvedType, dsName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, paramTypes.ToArray(), null);
                            if (methodInfo == null)
                            {
                                if (resolvedType.GetMethod(dsName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy) != null)
                                {
                                    message = SR.GetString(SR.Error_MethodSignatureMismatch, GetFullPropertyName(manager) + ".Name");
                                    errorNumber = ErrorNumbers.Error_MethodSignatureMismatch;
                                }
                                else
                                {
                                    message = SR.GetString(SR.Error_MethodNotExists, GetFullPropertyName(manager) + ".Name", bind.Name);
                                    errorNumber = ErrorNumbers.Error_MethodNotExists;
                                }
                            }
                            // 





                            else if (!invokeMethod.ReturnType.Equals(methodInfo.ReturnType))
                            {
                                message = SR.GetString(SR.Error_MethodReturnTypeMismatch, GetFullPropertyName(manager), invokeMethod.ReturnType.FullName);
                                errorNumber = ErrorNumbers.Error_MethodReturnTypeMismatch;
                            }
                        }
                        if (message.Length > 0)
                        {
                            ValidationError error = new ValidationError(message, errorNumber);
                            error.PropertyName = GetFullPropertyName(manager) + ".Path";
                            validationErrors.Add(error);
                        }
                    }
                }
            }
            return validationErrors;
        }

    }

    #endregion

    #region Class ActivityBindValidator
    internal sealed class ActivityBindValidator : Validator
    {
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "There is no security issue since this is a design time class")]
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            ActivityBind bind = obj as ActivityBind;
            if (bind == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(ActivityBind).FullName), "obj");

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).Name));

            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(BindValidationContext).Name));

            ValidationError error = null;
            if (string.IsNullOrEmpty(bind.Name))
            {
                error = new ValidationError(SR.GetString(SR.Error_IDNotSetForActivitySource), ErrorNumbers.Error_IDNotSetForActivitySource);
                error.PropertyName = GetFullPropertyName(manager) + ".Name";
                validationErrors.Add(error);
            }
            else
            {
                Activity refActivity = Helpers.ParseActivityForBind(activity, bind.Name);
                if (refActivity == null)
                {
                    if (bind.Name.StartsWith("/"))
                        error = new ValidationError(SR.GetString(SR.Error_CannotResolveRelativeActivity, bind.Name), ErrorNumbers.Error_CannotResolveRelativeActivity);
                    else
                        error = new ValidationError(SR.GetString(SR.Error_CannotResolveActivity, bind.Name), ErrorNumbers.Error_CannotResolveActivity);
                    error.PropertyName = GetFullPropertyName(manager) + ".Name";
                    validationErrors.Add(error);
                }

                if (String.IsNullOrEmpty(bind.Path))
                {
                    error = new ValidationError(SR.GetString(SR.Error_PathNotSetForActivitySource), ErrorNumbers.Error_PathNotSetForActivitySource);
                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                    validationErrors.Add(error);
                }

                if (refActivity != null && validationErrors.Count == 0)
                {
                    string memberName = bind.Path;
                    string path = String.Empty;
                    int indexOfSeparator = memberName.IndexOfAny(new char[] { '.', '/', '[' });
                    if (indexOfSeparator != -1)
                    {
                        path = memberName.Substring(indexOfSeparator);
                        path = path.StartsWith(".") ? path.Substring(1) : path;
                        memberName = memberName.Substring(0, indexOfSeparator);
                    }

                    Type baseType = BindHelpers.GetBaseType(manager, validationContext);

                    //We need to bifurcate to field, method, property or ActivityBind, we need to distinguish based on first
                    //part of the path
                    MemberInfo memberInfo = null;
                    Type declaringType = null;

                    if (!String.IsNullOrEmpty(memberName))
                    {
                        declaringType = BindValidatorHelper.GetActivityType(manager, refActivity);
                        if (declaringType != null)
                        {
                            memberInfo = MemberBind.GetMemberInfo(declaringType, memberName);

                            //it could be an indexer property that requires [..] part to get correctly resolved
                            if (memberInfo == null && path.StartsWith("[", StringComparison.Ordinal))
                            {
                                string indexerPart = bind.Path.Substring(indexOfSeparator);
                                int closingBracketIndex = indexerPart.IndexOf(']');
                                if (closingBracketIndex != -1)
                                {
                                    string firstIndexerPart = indexerPart.Substring(0, closingBracketIndex + 1); //strip potential long path like Item[0].Foo
                                    path = (closingBracketIndex + 1 < indexerPart.Length) ? indexerPart.Substring(closingBracketIndex + 1) : string.Empty;
                                    path = path.StartsWith(".") ? path.Substring(1) : path;
                                    indexerPart = firstIndexerPart;
                                }
                                memberName = memberName + indexerPart;
                                memberInfo = MemberBind.GetMemberInfo(declaringType, memberName);
                            }
                        }
                    }

                    Validator validator = null;
                    object actualBind = null; //now there are two different class hierarchies - ActivityBind is not related to the BindBase/MemberBind
                    if (memberInfo != null)
                    {
                        string qualifier = (!String.IsNullOrEmpty(refActivity.QualifiedName)) ? refActivity.QualifiedName : bind.Name;

                        if (memberInfo is FieldInfo)
                        {
                            actualBind = new FieldBind(qualifier + "." + memberName, path);
                            validator = new FieldBindValidator();
                        }
                        else if (memberInfo is MethodInfo)
                        {
                            if (typeof(Delegate).IsAssignableFrom(baseType))
                            {
                                actualBind = new MethodBind(qualifier + "." + memberName);
                                validator = new MethodBindValidator();
                            }
                            else
                            {
                                error = new ValidationError(SR.GetString(SR.Error_InvalidMemberType, memberName, GetFullPropertyName(manager)), ErrorNumbers.Error_InvalidMemberType);
                                error.PropertyName = GetFullPropertyName(manager);
                                validationErrors.Add(error);
                            }
                        }
                        else if (memberInfo is PropertyInfo)
                        {
                            //Only if the referenced activity is the same it is a PropertyBind otherwise it is a ActivityBind
                            if (refActivity == activity)
                            {
                                actualBind = new PropertyBind(qualifier + "." + memberName, path);
                                validator = new PropertyBindValidator();
                            }
                            else
                            {
                                actualBind = bind;
                                validator = this;
                            }
                        }
                        else if (memberInfo is EventInfo)
                        {
                            actualBind = bind;
                            validator = this;
                        }
                    }
                    else if (memberInfo == null && baseType != null && typeof(Delegate).IsAssignableFrom(baseType))
                    {
                        actualBind = bind;
                        validator = this;
                    }

                    if (validator != null && actualBind != null)
                    {
                        if (validator == this && actualBind is ActivityBind)
                            validationErrors.AddRange(ValidateActivityBind(manager, actualBind));
                        else
                            validationErrors.AddRange(validator.Validate(manager, actualBind));
                    }
                    else if (error == null)
                    {
                        error = new ValidationError(SR.GetString(SR.Error_PathCouldNotBeResolvedToMember, bind.Path, (!string.IsNullOrEmpty(refActivity.QualifiedName)) ? refActivity.QualifiedName : refActivity.GetType().Name), ErrorNumbers.Error_PathCouldNotBeResolvedToMember);
                        error.PropertyName = GetFullPropertyName(manager);
                        validationErrors.Add(error);
                    }
                }
            }

            return validationErrors;
        }

        internal static bool DoesTargetTypeMatch(Type baseType, Type memberType, AccessTypes access)
        {
            if ((access & AccessTypes.ReadWrite) == AccessTypes.ReadWrite)
                return TypeProvider.IsRepresentingTheSameType(memberType, baseType);
            else if ((access & AccessTypes.Read) == AccessTypes.Read)
                return TypeProvider.IsAssignable(baseType, memberType, true);
            else if ((access & AccessTypes.Write) == AccessTypes.Write)
                return TypeProvider.IsAssignable(memberType, baseType, true);
            else
                return false;
        }

        private ValidationErrorCollection ValidateActivityBind(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            ActivityBind bind = obj as ActivityBind;

            PropertyValidationContext validationContext = manager.Context[typeof(PropertyValidationContext)] as PropertyValidationContext;
            if (validationContext == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(BindValidationContext).Name));

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).Name));

            ValidationError error = null;

            //Redirect from here to FieldBind/MethodBind by creating their instances
            BindValidationContext validationBindContext = manager.Context[typeof(BindValidationContext)] as BindValidationContext;
            if (validationBindContext == null)
            {
                Type baseType = BindHelpers.GetBaseType(manager, validationContext);
                if (baseType != null)
                {
                    AccessTypes accessType = BindHelpers.GetAccessType(manager, validationContext);
                    validationBindContext = new BindValidationContext(baseType, accessType);
                }
                //else
                //{
                //    error = new ValidationError(SR.GetString(SR.Error_BindBaseTypeNotSpecified, validationContext.PropertyName), ErrorNumbers.Error_BindBaseTypeNotSpecified);
                //    error.PropertyName = GetFullPropertyName(manager) + ".Name";
                //}
            }
            if (validationBindContext != null)
            {
                Type targetType = validationBindContext.TargetType;
                if (error == null)
                    validationErrors.AddRange(this.ValidateActivity(manager, bind, new BindValidationContext(targetType, validationBindContext.Access)));
            }

            if (error != null)
                validationErrors.Add(error);

            return validationErrors;
        }

        private ValidationErrorCollection ValidateActivity(ValidationManager manager, ActivityBind bind, BindValidationContext validationContext)
        {
            ValidationError error = null;
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();

            Activity activity = manager.Context[typeof(Activity)] as Activity;
            if (activity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).Name));

            Activity refActivity = Helpers.ParseActivityForBind(activity, bind.Name);
            if (refActivity == null)
            {
                error = (bind.Name.StartsWith("/", StringComparison.Ordinal)) ? new ValidationError(SR.GetString(SR.Error_CannotResolveRelativeActivity, bind.Name), ErrorNumbers.Error_CannotResolveRelativeActivity) : new ValidationError(SR.GetString(SR.Error_CannotResolveActivity, bind.Name), ErrorNumbers.Error_CannotResolveActivity);
                error.PropertyName = GetFullPropertyName(manager) + ".Name";
            }
            else if (bind.Path == null || bind.Path.Length == 0)
            {
                error = new ValidationError(SR.GetString(SR.Error_PathNotSetForActivitySource), ErrorNumbers.Error_PathNotSetForActivitySource);
                error.PropertyName = GetFullPropertyName(manager) + ".Path";
            }
            else
            {
                // 

                if (!bind.Name.StartsWith("/", StringComparison.Ordinal) && !ValidationHelpers.IsActivitySourceInOrder(refActivity, activity))
                {
                    error = new ValidationError(SR.GetString(SR.Error_BindActivityReference, refActivity.QualifiedName, activity.QualifiedName), ErrorNumbers.Error_BindActivityReference, true);
                    error.PropertyName = GetFullPropertyName(manager) + ".Name";
                }

                IDesignerHost designerHost = manager.GetService(typeof(IDesignerHost)) as IDesignerHost;
                WorkflowDesignerLoader loader = manager.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                if (designerHost != null && loader != null)
                {
                    Type refActivityType = null;
                    if (designerHost.RootComponent == refActivity)
                    {
                        ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
                        if (typeProvider == null)
                            throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                        refActivityType = typeProvider.GetType(designerHost.RootComponentClassName);
                    }
                    else
                    {
                        refActivity.GetType();
                    }

                    if (refActivityType != null)
                    {
                        MemberInfo memberInfo = MemberBind.GetMemberInfo(refActivityType, bind.Path);
                        if (memberInfo == null || (memberInfo is PropertyInfo && !(memberInfo as PropertyInfo).CanRead))
                        {
                            error = new ValidationError(SR.GetString(SR.Error_InvalidMemberPath, refActivity.QualifiedName, bind.Path), ErrorNumbers.Error_InvalidMemberPath);
                            error.PropertyName = GetFullPropertyName(manager) + ".Path";
                        }
                        else
                        {
                            Type memberType = null;
                            if (memberInfo is FieldInfo)
                                memberType = ((FieldInfo)(memberInfo)).FieldType;
                            else if (memberInfo is PropertyInfo)
                                memberType = ((PropertyInfo)(memberInfo)).PropertyType;
                            else if (memberInfo is EventInfo)
                                memberType = ((EventInfo)(memberInfo)).EventHandlerType;

                            if (!DoesTargetTypeMatch(validationContext.TargetType, memberType, validationContext.Access))
                            {
                                if (typeof(WorkflowParameterBinding).IsAssignableFrom(memberInfo.DeclaringType))
                                {
                                    error = new ValidationError(SR.GetString(SR.Warning_ParameterBinding, bind.Path, refActivity.QualifiedName, validationContext.TargetType.FullName), ErrorNumbers.Warning_ParameterBinding, true);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                                else
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_TargetTypeMismatch, memberInfo.Name, memberType.FullName, validationContext.TargetType.FullName), ErrorNumbers.Error_TargetTypeMismatch);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                            }
                        }
                    }
                }
                else
                {
                    MemberInfo memberInfo = MemberBind.GetMemberInfo(refActivity.GetType(), bind.Path);
                    if (memberInfo == null || (memberInfo is PropertyInfo && !(memberInfo as PropertyInfo).CanRead))
                    {
                        error = new ValidationError(SR.GetString(SR.Error_InvalidMemberPath, refActivity.QualifiedName, bind.Path), ErrorNumbers.Error_InvalidMemberPath);
                        error.PropertyName = GetFullPropertyName(manager) + ".Path";
                    }
                    else
                    {
                        DependencyProperty dependencyProperty = DependencyProperty.FromName(memberInfo.Name, memberInfo.DeclaringType);
                        object value = BindHelpers.ResolveActivityPath(refActivity, bind.Path);
                        if (value == null)
                        {
                            Type memberType = null;
                            if (memberInfo is FieldInfo)
                                memberType = ((FieldInfo)(memberInfo)).FieldType;
                            else if (memberInfo is PropertyInfo)
                                memberType = ((PropertyInfo)(memberInfo)).PropertyType;
                            else if (memberInfo is EventInfo)
                                memberType = ((EventInfo)(memberInfo)).EventHandlerType;

                            if (!TypeProvider.IsAssignable(typeof(ActivityBind), memberType) && !DoesTargetTypeMatch(validationContext.TargetType, memberType, validationContext.Access))
                            {
                                if (typeof(WorkflowParameterBinding).IsAssignableFrom(memberInfo.DeclaringType))
                                {
                                    error = new ValidationError(SR.GetString(SR.Warning_ParameterBinding, bind.Path, refActivity.QualifiedName, validationContext.TargetType.FullName), ErrorNumbers.Warning_ParameterBinding, true);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                                else
                                {
                                    error = new ValidationError(SR.GetString(SR.Error_TargetTypeMismatch, memberInfo.Name, memberType.FullName, validationContext.TargetType.FullName), ErrorNumbers.Error_TargetTypeMismatch);
                                    error.PropertyName = GetFullPropertyName(manager) + ".Path";
                                }
                            }
                        }
                        // If this is the top level activity, we should not valid that the bind can be resolved because
                        // the value of bind can be set when this activity is used in another activity.
                        else if (value is ActivityBind && refActivity.Parent != null)
                        {
                            ActivityBind referencedBind = value as ActivityBind;
                            bool bindRecursionContextAdded = false;

                            // Check for recursion
                            BindRecursionContext recursionContext = manager.Context[typeof(BindRecursionContext)] as BindRecursionContext;
                            if (recursionContext == null)
                            {
                                recursionContext = new BindRecursionContext();
                                manager.Context.Push(recursionContext);
                                bindRecursionContextAdded = true;
                            }
                            if (recursionContext.Contains(activity, bind))
                            {
                                error = new ValidationError(SR.GetString(SR.Bind_ActivityDataSourceRecursionDetected), ErrorNumbers.Bind_ActivityDataSourceRecursionDetected);
                                error.PropertyName = GetFullPropertyName(manager) + ".Path";
                            }
                            else
                            {
                                recursionContext.Add(activity, bind);

                                PropertyValidationContext propertyValidationContext = null;
                                if (dependencyProperty != null)
                                    propertyValidationContext = new PropertyValidationContext(refActivity, dependencyProperty);
                                else
                                    propertyValidationContext = new PropertyValidationContext(refActivity, memberInfo as PropertyInfo, memberInfo.Name);

                                validationErrors.AddRange(ValidationHelpers.ValidateProperty(manager, refActivity, referencedBind, propertyValidationContext, validationContext));
                            }

                            if (bindRecursionContextAdded)
                                manager.Context.Pop();
                        }
                        else if (validationContext.TargetType != null && !DoesTargetTypeMatch(validationContext.TargetType, value.GetType(), validationContext.Access))
                        {
                            error = new ValidationError(SR.GetString(SR.Error_TargetTypeMismatch, memberInfo.Name, value.GetType().FullName, validationContext.TargetType.FullName), ErrorNumbers.Error_TargetTypeMismatch);
                            error.PropertyName = GetFullPropertyName(manager) + ".Path";
                        }
                    }
                }
            }

            if (error != null)
                validationErrors.Add(error);

            return validationErrors;
        }
    }

    #endregion
}
