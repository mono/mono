namespace System.Web.ModelBinding {
    public static class ModelMetadataProviders {
        private static ModelMetadataProvider _current = new DataAnnotationsModelMetadataProvider();

        public static ModelMetadataProvider Current {
            get {
                return _current;
            }
            set {
                _current = value ?? new EmptyModelMetadataProvider();
            }
        }
    }
}
