namespace System.Web.Mvc {
    public class ModelMetadataProviders {
        private ModelMetadataProvider _currentProvider;
        private static ModelMetadataProviders _instance = new ModelMetadataProviders();
        private IResolver<ModelMetadataProvider> _resolver;

        internal ModelMetadataProviders(IResolver<ModelMetadataProvider> resolver = null) {
            _resolver = resolver ?? new SingleServiceResolver<ModelMetadataProvider>(
                () => _currentProvider,
                new DataAnnotationsModelMetadataProvider(),
                "ModelMetadataProviders.Current"
            );
        }

        public static ModelMetadataProvider Current {
            get {
                return _instance.CurrentInternal;
            }
            set {
                _instance.CurrentInternal = value;
            }
        }

        internal ModelMetadataProvider CurrentInternal {
            get {
                return _resolver.Current;
            }
            set {
                _currentProvider = value ?? new EmptyModelMetadataProvider();
            }
        }
    }
}