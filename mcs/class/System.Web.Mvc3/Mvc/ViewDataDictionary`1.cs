namespace System.Web.Mvc {
    using System;

    public class ViewDataDictionary<TModel> : ViewDataDictionary {
        public ViewDataDictionary() :
            base(default(TModel)) {
        }

        public ViewDataDictionary(TModel model) :
            base(model) {
        }

        public ViewDataDictionary(ViewDataDictionary viewDataDictionary) :
            base(viewDataDictionary) {
        }

        public new TModel Model {
            get {
                return (TModel)base.Model;
            }
            set {
                SetModel(value);
            }
        }

        public override ModelMetadata ModelMetadata {
            get {
                ModelMetadata result = base.ModelMetadata;
                if (result == null) {
                    result = base.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(TModel));
                }
                return result;
            }
            set {
                base.ModelMetadata = value;
            }
        }

        protected override void SetModel(object value) {
            bool castWillSucceed = TypeHelpers.IsCompatibleObject<TModel>(value);

            if (castWillSucceed) {
                base.SetModel((TModel)value);
            }
            else {
                InvalidOperationException exception = (value != null)
                    ? Error.ViewDataDictionary_WrongTModelType(value.GetType(), typeof(TModel))
                    : Error.ViewDataDictionary_ModelCannotBeNull(typeof(TModel));
                throw exception;
            }
        }

    }
}
