using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.ModelBinding {
    public sealed class MinLengthAttributeAdapter: DataAnnotationsModelValidator<MinLengthAttribute> {
        public MinLengthAttributeAdapter(ModelMetadata metadata, ModelBindingExecutionContext context, MinLengthAttribute attribute)
            : base(metadata, context, attribute) {
        }

        protected override string GetLocalizedErrorMessage(string errorMessage) {
            return GetLocalizedString(errorMessage, Metadata.GetDisplayName(), Attribute.Length);

        }

    }
}
