namespace System.Web.Mvc {
    using System.ComponentModel.DataAnnotations;

    public class DataAnnotationsModelValidator<TAttribute> : DataAnnotationsModelValidator where TAttribute : ValidationAttribute {
        public DataAnnotationsModelValidator(ModelMetadata metadata, ControllerContext context, TAttribute attribute)
            : base(metadata, context, attribute) { }

        protected new TAttribute Attribute {
            get {
                return (TAttribute)base.Attribute;
            }
        }
    }
}
