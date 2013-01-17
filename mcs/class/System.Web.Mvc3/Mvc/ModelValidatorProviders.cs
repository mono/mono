namespace System.Web.Mvc {
    public static class ModelValidatorProviders {

        private static readonly ModelValidatorProviderCollection _providers = new ModelValidatorProviderCollection() {
            new DataAnnotationsModelValidatorProvider(),
            new DataErrorInfoModelValidatorProvider(),
            new ClientDataTypeModelValidatorProvider()
        };

        public static ModelValidatorProviderCollection Providers {
            get {
                return _providers;
            }
        }

    }
}
