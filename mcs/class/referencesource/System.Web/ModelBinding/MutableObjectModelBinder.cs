namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class MutableObjectModelBinder : IModelBinder {

        private ModelMetadataProvider _metadataProvider;

        internal ModelMetadataProvider MetadataProvider {
            get {
                if (_metadataProvider == null) {
                    _metadataProvider = ModelMetadataProviders.Current;
                }
                return _metadataProvider;
            }
            set {
                _metadataProvider = value;
            }
        }

        public virtual bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            // Recursive method - prevent stack overflow.
            RuntimeHelpers.EnsureSufficientExecutionStack();

            ModelBinderUtil.ValidateBindingContext(bindingContext);

            EnsureModel(modelBindingExecutionContext, bindingContext);
            IEnumerable<ModelMetadata> propertyMetadatas = GetMetadataForProperties(modelBindingExecutionContext, bindingContext);
            ComplexModel complexModel = CreateAndPopulateComplexModel(modelBindingExecutionContext, bindingContext, propertyMetadatas);

            // post-processing, e.g. property setters and hooking up validation
            ProcessComplexModel(modelBindingExecutionContext, bindingContext, complexModel);
            bindingContext.ValidationNode.ValidateAllProperties = true; // complex models require full validation
            return true;
        }

        protected virtual bool CanUpdateProperty(ModelMetadata propertyMetadata) {
            return CanUpdatePropertyInternal(propertyMetadata);
        }

        internal static bool CanUpdatePropertyInternal(ModelMetadata propertyMetadata) {
            return (!propertyMetadata.IsReadOnly || CanUpdateReadOnlyProperty(propertyMetadata.ModelType));
        }

        private static bool CanUpdateReadOnlyProperty(Type propertyType) {
            // Value types have copy-by-value semantics, which prevents us from updating
            // properties that are marked readonly.
            if (propertyType.IsValueType) {
                return false;
            }

            // Arrays are strange beasts since their contents are mutable but their sizes aren't.
            // Therefore we shouldn't even try to update these. Further reading:
            // http://blogs.msdn.com/ericlippert/archive/2008/09/22/arrays-considered-somewhat-harmful.aspx
            if (propertyType.IsArray) {
                return false;
            }

            // Special-case known immutable reference types
            if (propertyType == typeof(string)) {
                return false;
            }

            return true;
        }

        private ComplexModel CreateAndPopulateComplexModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext, IEnumerable<ModelMetadata> propertyMetadatas) {
            // create a Complex Model and call into the Complex Model binder
            ComplexModel originalComplexModel = new ComplexModel(bindingContext.ModelMetadata, propertyMetadatas);
            ModelBindingContext complexModelBindingContext = new ModelBindingContext(bindingContext) {
                ModelMetadata = MetadataProvider.GetMetadataForType(() => originalComplexModel, typeof(ComplexModel)),
                ModelName = bindingContext.ModelName
            };

            IModelBinder complexModelBinder = bindingContext.ModelBinderProviders.GetRequiredBinder(modelBindingExecutionContext, complexModelBindingContext);
            complexModelBinder.BindModel(modelBindingExecutionContext, complexModelBindingContext);
            return (ComplexModel)complexModelBindingContext.Model;
        }

        protected virtual object CreateModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            // If the Activator throws an exception, we want to propagate it back up the call stack, since the application
            // developer should know that this was an invalid type to try to bind to.
            return SecurityUtils.SecureCreateInstance(bindingContext.ModelType);
        }

        // Called when the property setter null check failed, allows us to add our own error message to ModelState.
        internal static EventHandler<ModelValidatedEventArgs> CreateNullCheckFailedHandler(ModelBindingExecutionContext modelBindingExecutionContext, ModelMetadata modelMetadata, object incomingValue) {
            return (sender, e) =>
            {
                ModelValidationNode validationNode = (ModelValidationNode)sender;
                ModelStateDictionary modelState = e.ModelBindingExecutionContext.ModelState;

                if (modelState.IsValidField(validationNode.ModelStateKey)) {
                    string errorMessage = ModelBinderErrorMessageProviders.ValueRequiredErrorMessageProvider(modelBindingExecutionContext, modelMetadata, incomingValue);
                    if (errorMessage != null) {
                        modelState.AddModelError(validationNode.ModelStateKey, errorMessage);
                    }
                }
            };
        }

        protected virtual void EnsureModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            if (bindingContext.Model == null) {
                bindingContext.ModelMetadata.Model = CreateModel(modelBindingExecutionContext, bindingContext);
            }
        }

        protected virtual IEnumerable<ModelMetadata> GetMetadataForProperties(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            // keep a set of the required properties so that we can cross-reference bound properties later
            HashSet<string> requiredProperties;
            HashSet<string> skipProperties;
            GetRequiredPropertiesCollection(bindingContext.ModelType, out requiredProperties, out skipProperties);

            return from propertyMetadata in bindingContext.ModelMetadata.Properties
                   let propertyName = propertyMetadata.PropertyName
                   let shouldUpdateProperty = requiredProperties.Contains(propertyName) || !skipProperties.Contains(propertyName)
                   where shouldUpdateProperty && CanUpdateProperty(propertyMetadata)
                   select propertyMetadata;
        }

        private static object GetPropertyDefaultValue(PropertyDescriptor propertyDescriptor) {
            DefaultValueAttribute attr = propertyDescriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
            return (attr != null) ? attr.Value : null;
        }

        internal static void GetRequiredPropertiesCollection(Type modelType, out HashSet<string> requiredProperties, out HashSet<string> skipProperties) {
            requiredProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            skipProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Use attributes on the property before attributes on the type.
            ICustomTypeDescriptor modelDescriptor = TypeDescriptorHelper.Get(modelType);
            PropertyDescriptorCollection propertyDescriptors = modelDescriptor.GetProperties();
            BindingBehaviorAttribute typeAttr = modelDescriptor.GetAttributes().OfType<BindingBehaviorAttribute>().SingleOrDefault();

            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors) {
                BindingBehaviorAttribute propAttr = propertyDescriptor.Attributes.OfType<BindingBehaviorAttribute>().SingleOrDefault();
                BindingBehaviorAttribute workingAttr = propAttr ?? typeAttr;
                if (workingAttr != null) {
                    switch (workingAttr.Behavior) {
                        case BindingBehavior.Required:
                            requiredProperties.Add(propertyDescriptor.Name);
                            break;

                        case BindingBehavior.Never:
                            skipProperties.Add(propertyDescriptor.Name);
                            break;
                    }
                }
            }
        }

        internal void ProcessComplexModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext, ComplexModel complexModel) {
            HashSet<string> requiredProperties;
            HashSet<string> skipProperties;
            GetRequiredPropertiesCollection(bindingContext.ModelType, out requiredProperties, out skipProperties);

            // Are all of the required fields accounted for?
            HashSet<string> missingRequiredProperties = new HashSet<string>(requiredProperties);
            missingRequiredProperties.ExceptWith(complexModel.Results.Select(r => r.Key.PropertyName));
            string missingPropertyName = missingRequiredProperties.FirstOrDefault();
            if (missingPropertyName != null) {
                string fullPropertyKey = ModelBinderUtil.CreatePropertyModelName(bindingContext.ModelName, missingPropertyName);
                throw Error.BindingBehavior_ValueNotFound(fullPropertyKey);
            }

            // for each property that was bound, call the setter, recording exceptions as necessary
            foreach (var entry in complexModel.Results) {
                ModelMetadata propertyMetadata = entry.Key;

                ComplexModelResult complexModelResult = entry.Value;
                if (complexModelResult != null) {
                    SetProperty(modelBindingExecutionContext, bindingContext, propertyMetadata, complexModelResult);
                    bindingContext.ValidationNode.ChildNodes.Add(complexModelResult.ValidationNode);
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We're recording this exception so that we can act on it later.")]
        protected virtual void SetProperty(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext, ModelMetadata propertyMetadata, ComplexModelResult complexModelResult) {
            PropertyDescriptor propertyDescriptor = TypeDescriptorHelper.Get(bindingContext.ModelType).GetProperties().Find(propertyMetadata.PropertyName, true /* ignoreCase */);
            if (propertyDescriptor == null || propertyDescriptor.IsReadOnly) {
                return; // nothing to do
            }

            object value = complexModelResult.Model ?? GetPropertyDefaultValue(propertyDescriptor);
            propertyMetadata.Model = value;

            // 'Required' validators need to run first so that we can provide useful error messages if
            // the property setters throw, e.g. if we're setting entity keys to null. See comments in
            // DefaultModelBinder.SetProperty() for more information.
            if (value == null) {
                string modelStateKey = complexModelResult.ValidationNode.ModelStateKey;
                if (bindingContext.ModelState.IsValidField(modelStateKey)) {
                    ModelValidator requiredValidator = ModelValidatorProviders.Providers.GetValidators(propertyMetadata, modelBindingExecutionContext).Where(v => v.IsRequired).FirstOrDefault();
                    if (requiredValidator != null) {
                        foreach (ModelValidationResult validationResult in requiredValidator.Validate(bindingContext.Model)) {
                            bindingContext.ModelState.AddModelError(modelStateKey, validationResult.Message);
                        }
                    }
                }
            }

            if (value != null || TypeHelpers.TypeAllowsNullValue(propertyDescriptor.PropertyType)) {
                try {
                    propertyDescriptor.SetValue(bindingContext.Model, value);
                }
                catch (Exception ex) {
                    // don't display a duplicate error message if a binding error has already occurred for this field
                    string modelStateKey = complexModelResult.ValidationNode.ModelStateKey;
                    if (bindingContext.ModelState.IsValidField(modelStateKey)) {
                        bindingContext.ModelState.AddModelError(modelStateKey, ex);
                    }
                }
            }
            else {
                // trying to set a non-nullable value type to null, need to make sure there's a message
                string modelStateKey = complexModelResult.ValidationNode.ModelStateKey;
                if (bindingContext.ModelState.IsValidField(modelStateKey)) {
                    complexModelResult.ValidationNode.Validated += CreateNullCheckFailedHandler(modelBindingExecutionContext, propertyMetadata, value);
                }
            }
        }

    }
}
