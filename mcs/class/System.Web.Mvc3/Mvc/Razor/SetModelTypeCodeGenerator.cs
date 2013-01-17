using System.Globalization;
using System.Web.Mvc.ExpressionUtil;
using System.Web.Razor.Generator;

namespace System.Web.Mvc.Razor
{
    internal class SetModelTypeCodeGenerator : SetBaseTypeCodeGenerator
    {
        private string _genericTypeFormat;

        public SetModelTypeCodeGenerator(string modelType, string genericTypeFormat)
            : base(modelType)
        {
            _genericTypeFormat = genericTypeFormat;
        }

        protected override string ResolveType(CodeGeneratorContext context, string baseType)
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                _genericTypeFormat,
                context.Host.DefaultBaseClass,
                baseType);
        }

        public override bool Equals(object obj)
        {
            SetModelTypeCodeGenerator other = obj as SetModelTypeCodeGenerator;
            return other != null &&
                   base.Equals(obj) &&
                   String.Equals(_genericTypeFormat, other._genericTypeFormat, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            var combiner = new HashCodeCombiner();
            combiner.AddInt32(base.GetHashCode());
            combiner.AddObject(_genericTypeFormat);
            return combiner.CombinedHash;
        }

        public override string ToString()
        {
            return "Model:" + BaseType;
        }
    }
}
