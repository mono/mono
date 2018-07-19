namespace System.Web.ModelBinding {
    public static class ModelValidatorProviders {

        private static readonly ModelValidatorProviderCollection _providers = new ModelValidatorProviderCollection() {
            new DataAnnotationsModelValidatorProvider(),
#if UNDEF
            new DataErrorInfoModelValidatorProvider(),
            new ClientDataTypeModelValidatorProvider()
#endif
        };

        public static ModelValidatorProviderCollection Providers {
            get {
                return _providers;
            }
        }

    }
}
