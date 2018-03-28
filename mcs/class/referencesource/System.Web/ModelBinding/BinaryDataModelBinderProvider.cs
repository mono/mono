namespace System.Web.ModelBinding {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    // This is a single provider that can work with both byte[] and Binary models.
    public sealed class BinaryDataModelBinderProvider : ModelBinderProvider {

        private static readonly ModelBinderProvider[] _providers = new ModelBinderProvider[] {
            new SimpleModelBinderProvider(typeof(byte[]), new ByteArrayExtensibleModelBinder()),
#if UNDEF
            new SimpleModelBinderProvider(typeof(Binary), new LinqBinaryExtensibleModelBinder())
#endif
        };

        public override IModelBinder GetBinder(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
            return (from provider in _providers
                    let binder = provider.GetBinder(modelBindingExecutionContext, bindingContext)
                    where binder != null
                    select binder).FirstOrDefault();
        }

        // This is essentially a clone of the ByteArrayModelBinder from core
        private class ByteArrayExtensibleModelBinder : IModelBinder {
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to ignore when the data is corrupted")]
            [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.Web.ModelBinding.ValueProviderResult.ConvertTo(System.Type)", Justification = @"The default CultureInfo used by ValueProvider is fine.")]
            public bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext, ModelBindingContext bindingContext) {
                ModelBinderUtil.ValidateBindingContext(bindingContext);
                ValueProviderResult vpResult = bindingContext.UnvalidatedValueProvider.GetValue(bindingContext.ModelName);

                // case 1: there was no <input ... /> element containing this data
                if (vpResult == null) {
                    return false;
                }

                string base64string = (string)vpResult.ConvertTo(typeof(string));

                // case 2: there was an <input ... /> element but it was left blank
                if (String.IsNullOrEmpty(base64string)) {
                    return false;
                }

                // Future proofing. If the byte array is actually an instance of System.Data.Linq.Binary
                // then we need to remove these quotes put in place by the ToString() method.
                string realValue = base64string.Replace("\"", String.Empty);
                try {
                    bindingContext.Model = ConvertByteArray(Convert.FromBase64String(realValue));
                    return true;
                }
                catch {
                    // corrupt data - just ignore
                    return false;
                }
            }

            protected virtual object ConvertByteArray(byte[] originalModel) {
                return originalModel;
            }
        }
#if UNDEF
        // This is essentially a clone of the LinqBinaryModelBinder from core
        private class LinqBinaryExtensibleModelBinder : ByteArrayExtensibleModelBinder {
            protected override object ConvertByteArray(byte[] originalModel) {
                return new Binary(originalModel);
            }
        }
#endif
    }
}
