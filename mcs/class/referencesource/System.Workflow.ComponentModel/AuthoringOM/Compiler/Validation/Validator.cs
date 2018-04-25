namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;

    #region Class Validator
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class Validator
    {
        public virtual ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            return new ValidationErrorCollection();
        }

        public virtual ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (action == null)
                throw new ArgumentNullException("action");

            return null;
        }

        public virtual ValidationErrorCollection ValidateProperties(ValidationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (obj == null)
                throw new ArgumentNullException("obj");

            ValidationErrorCollection errors = new ValidationErrorCollection();

            Activity activity = manager.Context[typeof(Activity)] as Activity;

            // Validate all members that support validations.
            Walker walker = new Walker(true);
            walker.FoundProperty += delegate(Walker w, WalkerEventArgs args)
            {
                //If we find dynamic property of the same name then we do not invoke the validator associated with the property
                //Attached dependency properties will not be found by FromName().

                // args.CurrentProperty can be null if the property is of type IList.  The walker would go into each item in the
                // list, but we don't need to validate these items.
                if (args.CurrentProperty != null)
                {
                    DependencyProperty dependencyProperty = DependencyProperty.FromName(args.CurrentProperty.Name, args.CurrentProperty.DeclaringType);
                    if (dependencyProperty == null)
                    {
                        object[] validationVisibilityAtrributes = args.CurrentProperty.GetCustomAttributes(typeof(ValidationOptionAttribute), true);
                        ValidationOption validationVisibility = (validationVisibilityAtrributes.Length > 0) ? ((ValidationOptionAttribute)validationVisibilityAtrributes[0]).ValidationOption : ValidationOption.Optional;
                        if (validationVisibility != ValidationOption.None)
                        {
                            errors.AddRange(ValidateProperty(args.CurrentProperty, args.CurrentPropertyOwner, args.CurrentValue, manager));
                            // don't probe into subproperties as validate object inside the ValidateProperties call does it for us
                            args.Action = WalkerAction.Skip;
                        }
                    }
                }
            };

            walker.WalkProperties(activity, obj);

            return errors;
        }

        protected string GetFullPropertyName(ValidationManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            string fullName = string.Empty;

            // iterate the properties in the stack starting with the last one
            int iterator = 0;
            while (manager.Context[iterator] != null)
            {
                if (manager.Context[iterator] is PropertyValidationContext)
                {
                    PropertyValidationContext propertyValidationContext = manager.Context[iterator] as PropertyValidationContext;
                    if (propertyValidationContext.PropertyName == string.Empty)
                        fullName = string.Empty;  // property chain broke... dicard properties after break
                    else if (fullName == string.Empty)
                        fullName = propertyValidationContext.PropertyName;
                    else
                        fullName = propertyValidationContext.PropertyName + "." + fullName;
                }
                iterator++;
            }

            return fullName;
        }

        internal protected ValidationErrorCollection ValidateProperty(PropertyInfo propertyInfo, object propertyOwner, object propertyValue, ValidationManager manager)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();

            object[] validationVisibilityAtrributes = propertyInfo.GetCustomAttributes(typeof(ValidationOptionAttribute), true);
            ValidationOption validationVisibility = (validationVisibilityAtrributes.Length > 0) ? ((ValidationOptionAttribute)validationVisibilityAtrributes[0]).ValidationOption : ValidationOption.Optional;
            PropertyValidationContext propertyValidationContext = new PropertyValidationContext(propertyOwner, propertyInfo, propertyInfo.Name);
            manager.Context.Push(propertyValidationContext);

            try
            {
                if (propertyValue != null)
                {
                    errors.AddRange(ValidationHelpers.ValidateObject(manager, propertyValue));
                    if (propertyValue is IList)
                    {
                        PropertyValidationContext childContext = new PropertyValidationContext(propertyValue, null, "");
                        manager.Context.Push(childContext);

                        try
                        {
                            foreach (object child in (IList)propertyValue)
                                errors.AddRange(ValidationHelpers.ValidateObject(manager, child));
                        }
                        finally
                        {
                            System.Diagnostics.Debug.Assert(manager.Context.Current == childContext, "Unwinding contextStack: the item that is about to be popped is not the one we pushed.");
                            manager.Context.Pop();
                        }
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
    }
    #endregion
}
