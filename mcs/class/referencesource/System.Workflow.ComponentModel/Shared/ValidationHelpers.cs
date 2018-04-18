// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in sync.  Any change made here must also
 * be made to WF\Activities\Common\ValidationHelpers.cs
*********************************************************************/
namespace System.Workflow.ComponentModel.Compiler
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.CodeDom.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    #endregion

    internal static class ValidationHelpers
    {
        #region Validation & helpers for ID and Type

        internal static void ValidateIdentifier(IServiceProvider serviceProvider, string identifier)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(serviceProvider);
            CodeDomProvider provider = CompilerHelpers.GetCodeDomProvider(language);
            if (language == SupportedLanguages.CSharp && identifier.StartsWith("@", StringComparison.Ordinal) ||
                language == SupportedLanguages.VB && identifier.StartsWith("[", StringComparison.Ordinal) && identifier.EndsWith("]", StringComparison.Ordinal) ||
                !provider.IsValidIdentifier(identifier))
            {
                throw new Exception(SR.GetString(SR.Error_InvalidLanguageIdentifier, identifier));
            }
        }

        internal static ValidationError ValidateIdentifier(string propName, IServiceProvider context, string identifier)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            ValidationError error = null;
            if (identifier == null || identifier.Length == 0)
                error = new ValidationError(SR.GetString(SR.Error_PropertyNotSet, propName), ErrorNumbers.Error_PropertyNotSet);
            else
            {
                try
                {
                    ValidationHelpers.ValidateIdentifier(context, identifier);
                }
                catch (Exception e)
                {
                    error = new ValidationError(SR.GetString(SR.Error_InvalidIdentifier, propName, e.Message), ErrorNumbers.Error_InvalidIdentifier);
                }
            }
            if (error != null)
                error.PropertyName = propName;
            return error;
        }

        internal static ValidationError ValidateNameProperty(string propName, IServiceProvider context, string identifier)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            ValidationError error = null;
            if (identifier == null || identifier.Length == 0)
                error = new ValidationError(SR.GetString(SR.Error_PropertyNotSet, propName), ErrorNumbers.Error_PropertyNotSet);
            else
            {
                SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(context);
                CodeDomProvider provider = CompilerHelpers.GetCodeDomProvider(language);

                if (language == SupportedLanguages.CSharp && identifier.StartsWith("@", StringComparison.Ordinal) ||
                    language == SupportedLanguages.VB && identifier.StartsWith("[", StringComparison.Ordinal) && identifier.EndsWith("]", StringComparison.Ordinal) ||
                    !provider.IsValidIdentifier(provider.CreateEscapedIdentifier(identifier)))
                {
                    error = new ValidationError(SR.GetString(SR.Error_InvalidIdentifier, propName, SR.GetString(SR.Error_InvalidLanguageIdentifier, identifier)), ErrorNumbers.Error_InvalidIdentifier);
                }
            }
            if (error != null)
                error.PropertyName = propName;
            return error;
        }

        internal static ValidationErrorCollection ValidateUniqueIdentifiers(Activity rootActivity)
        {
            if (rootActivity == null)
                throw new ArgumentNullException("rootActivity");

            Hashtable identifiers = new Hashtable();
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            Queue activities = new Queue();
            activities.Enqueue(rootActivity);
            while (activities.Count > 0)
            {
                Activity activity = (Activity)activities.Dequeue();
                if (activity.Enabled)
                {
                    if (identifiers.ContainsKey(activity.QualifiedName))
                    {
                        ValidationError duplicateError = new ValidationError(SR.GetString(SR.Error_DuplicatedActivityID, activity.QualifiedName), ErrorNumbers.Error_DuplicatedActivityID);
                        duplicateError.PropertyName = "Name";
                        duplicateError.UserData[typeof(Activity)] = activity;
                        validationErrors.Add(duplicateError);
                    }
                    else
                        identifiers.Add(activity.QualifiedName, activity);

                    if (activity is CompositeActivity && ((activity.Parent == null) || !Helpers.IsCustomActivity(activity as CompositeActivity)))
                    {
                        foreach (Activity child in Helpers.GetAllEnabledActivities((CompositeActivity)activity))
                            activities.Enqueue(child);
                    }
                }
            }

            return validationErrors;

        }
        #endregion

        #region Validation for Activity Ref order

        internal static bool IsActivitySourceInOrder(Activity request, Activity response)
        {
            if (request.Parent == null)
                return true;
            List<Activity> responsePath = new List<Activity>();
            responsePath.Add(response);
            Activity responseParent = response is CompositeActivity ? (CompositeActivity)response : response.Parent;
            while (responseParent != null)
            {
                responsePath.Add(responseParent);
                responseParent = responseParent.Parent;
            }

            Activity requestChild = request;
            CompositeActivity requestParent = request is CompositeActivity ? (CompositeActivity)request : request.Parent;
            while (requestParent != null && !responsePath.Contains(requestParent))
            {
                requestChild = requestParent;
                requestParent = requestParent.Parent;
            }

            if (requestParent == requestChild)
                return true;

            bool incorrectOrder = false;
            int index = (responsePath.IndexOf(requestParent) - 1);
            index = (index < 0) ? 0 : index; //sometimes parent gets added to the collection twice which causes index to be -1
            Activity responseChild = responsePath[index];

            if (requestParent == null || Helpers.IsAlternateFlowActivity(requestChild) || Helpers.IsAlternateFlowActivity(responseChild))
                incorrectOrder = true;
            else
            {
                for (int i = 0; i < requestParent.EnabledActivities.Count; i++)
                {
                    if (requestParent.EnabledActivities[i] == requestChild)
                        break;
                    else if (requestParent.EnabledActivities[i] == responseChild)
                    {
                        incorrectOrder = true;
                        break;
                    }
                }
            }
            return !incorrectOrder;
        }

        #endregion

        internal static ValidationErrorCollection ValidateObject(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            if (obj == null)
                return errors;

            Type objType = obj.GetType();
            if (!objType.IsPrimitive && (objType != typeof(string)))
            {
                bool removeValidatedObjectCollection = false;
                Dictionary<int, object> validatedObjects = manager.Context[typeof(Dictionary<int, object>)] as Dictionary<int, object>;
                if (validatedObjects == null)
                {
                    validatedObjects = new Dictionary<int, object>();
                    manager.Context.Push(validatedObjects);
                    removeValidatedObjectCollection = true;
                }

                try
                {
                    if (!validatedObjects.ContainsKey(obj.GetHashCode()))
                    {
                        validatedObjects.Add(obj.GetHashCode(), obj);
                        try
                        {
                            Validator[] validators = manager.GetValidators(objType);
                            foreach (Validator validator in validators)
                                errors.AddRange(validator.Validate(manager, obj));
                        }
                        finally
                        {
                            validatedObjects.Remove(obj.GetHashCode());
                        }
                    }
                }
                finally
                {
                    if (removeValidatedObjectCollection)
                        manager.Context.Pop();
                }
            }

            return errors;
        }

        internal static ValidationErrorCollection ValidateActivity(ValidationManager manager, Activity activity)
        {
            ValidationErrorCollection errors = ValidationHelpers.ValidateObject(manager, activity);

            foreach (ValidationError error in errors)
            {
                if (!error.UserData.Contains(typeof(Activity)))
                    error.UserData[typeof(Activity)] = activity;
            }
            return errors;
        }

        internal static ValidationErrorCollection ValidateProperty(ValidationManager manager, Activity activity, object obj, PropertyValidationContext propertyValidationContext)
        {
            return ValidateProperty(manager, activity, obj, propertyValidationContext, null);
        }

        internal static ValidationErrorCollection ValidateProperty(ValidationManager manager, Activity activity, object obj, PropertyValidationContext propertyValidationContext, object extendedPropertyContext)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (propertyValidationContext == null)
                throw new ArgumentNullException("propertyValidationContext");

            ValidationErrorCollection errors = new ValidationErrorCollection();

            manager.Context.Push(activity);
            manager.Context.Push(propertyValidationContext);
            if (extendedPropertyContext != null)
                manager.Context.Push(extendedPropertyContext);
            try
            {
                errors.AddRange(ValidationHelpers.ValidateObject(manager, obj));
            }
            finally
            {
                manager.Context.Pop();
                manager.Context.Pop();
                if (extendedPropertyContext != null)
                    manager.Context.Pop();
            }

            return errors;
        }
    }
}
