namespace System.Web.Mvc.Razor {
    using System.CodeDom;
    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using System.Web.Razor.Parser.SyntaxTree;

    public class MvcVBRazorCodeGenerator : VBRazorCodeGenerator {
        public MvcVBRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
            : base(className, rootNamespaceName, sourceFileName, host) {
        }

        protected override bool TryVisitSpecialSpan(Span span) {
            return TryVisit<ModelSpan>(span, VisitModelSpan);
        }

        private void VisitModelSpan(ModelSpan span) {
            string modelName = span.ModelTypeName;
            var baseType = new CodeTypeReference(Host.DefaultBaseClass + "(Of " + modelName + ")");

            GeneratedClass.BaseTypes.Clear();
            GeneratedClass.BaseTypes.Add(baseType);

            if (DesignTimeMode) {
                WriteHelperVariable(span.Content, "__modelHelper");
            }
        }
    }
}
