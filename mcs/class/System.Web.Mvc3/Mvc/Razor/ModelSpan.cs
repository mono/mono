namespace System.Web.Mvc.Razor {
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;

    public class ModelSpan : CodeSpan {
        public ModelSpan(SourceLocation start, string content, string modelTypeName)
            : base(start, content) {
            this.ModelTypeName = modelTypeName;
        }

        internal ModelSpan(ParserContext context, string modelTypeName)
            : this(context.CurrentSpanStart, context.ContentBuffer.ToString(), modelTypeName) {
        }

        public string ModelTypeName {
            get;
            private set;
        }

        public override int GetHashCode() {
            return base.GetHashCode() ^ (ModelTypeName ?? String.Empty).GetHashCode();
        }

        public override bool Equals(object obj) {
            ModelSpan span = obj as ModelSpan;
            return span != null && Equals(span);
        }

        private bool Equals(ModelSpan span) {
            return base.Equals(span) && String.Equals(ModelTypeName, span.ModelTypeName, StringComparison.Ordinal);
        }
    }
}
