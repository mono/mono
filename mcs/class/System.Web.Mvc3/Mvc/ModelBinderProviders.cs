namespace System.Web.Mvc {

    public static class ModelBinderProviders {

        private readonly static ModelBinderProviderCollection _binderProviders = new ModelBinderProviderCollection {
        };

        public static ModelBinderProviderCollection BinderProviders {
            get {
                return _binderProviders;
            }
        }
    }
}