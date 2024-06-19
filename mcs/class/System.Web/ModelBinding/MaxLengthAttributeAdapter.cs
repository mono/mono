using System.ComponentModel.DataAnnotations;

namespace System.Web.ModelBinding
{
  public sealed class MaxLengthAttributeAdapter : DataAnnotationsModelValidator<MaxLengthAttribute>
  {
    public MaxLengthAttributeAdapter(ModelMetadata metadata, ModelBindingExecutionContext context, MaxLengthAttribute attribute)
      : base(metadata, context, attribute)
    {
    }

    protected override string GetLocalizedErrorMessage(string errorMessage)
    {
      return this.GetLocalizedString(errorMessage, (object) this.Metadata.GetDisplayName(), (object) this.Attribute.Length);
    }
  }
}