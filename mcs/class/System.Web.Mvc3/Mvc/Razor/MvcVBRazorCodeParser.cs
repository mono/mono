using System.Globalization;
using System.Linq;
using System.Web.Mvc.Resources;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;
using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Mvc.Razor
{
    public class MvcVBRazorCodeParser : VBCodeParser
    {
        internal const string ModelTypeKeyword = "ModelType";
        private const string GenericTypeFormatString = "{0}(Of {1})";
        private SourceLocation? _endInheritsLocation;
        private bool _modelStatementFound;

        public MvcVBRazorCodeParser()
        {
            MapDirective(ModelTypeKeyword, ModelTypeDirective);
        }

        protected override bool InheritsStatement()
        {
            // Verify we're on the right keyword and accept
            Assert(VBKeyword.Inherits);
            VBSymbol inherits = CurrentSymbol;
            NextToken();
            _endInheritsLocation = CurrentLocation;
            PutCurrentBack();
            PutBack(inherits);
            EnsureCurrent();

            bool result = base.InheritsStatement();
            CheckForInheritsAndModelStatements();
            return result;
        }

        private void CheckForInheritsAndModelStatements()
        {
            if (_modelStatementFound && _endInheritsLocation.HasValue)
            {
                Context.OnError(_endInheritsLocation.Value, String.Format(CultureInfo.CurrentCulture, MvcResources.MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword, ModelTypeKeyword));
            }
        }

        protected virtual bool ModelTypeDirective()
        {
            AssertDirective(ModelTypeKeyword);

            Span.CodeGenerator = SpanCodeGenerator.Null;
            Context.CurrentBlock.Type = BlockType.Directive;

            AcceptAndMoveNext();
            SourceLocation endModelLocation = CurrentLocation;

            if (At(VBSymbolType.WhiteSpace))
            {
                Span.EditHandler.AcceptedCharacters = AcceptedCharacters.None;
            }

            AcceptWhile(VBSymbolType.WhiteSpace);
            Output(SpanKind.MetaCode);

            if (_modelStatementFound)
            {
                Context.OnError(endModelLocation, String.Format(CultureInfo.CurrentCulture, MvcResources.MvcRazorCodeParser_OnlyOneModelStatementIsAllowed, ModelTypeKeyword));
            }
            _modelStatementFound = true;

            if (EndOfFile || At(VBSymbolType.WhiteSpace) || At(VBSymbolType.NewLine))
            {
                Context.OnError(endModelLocation, MvcResources.MvcRazorCodeParser_ModelKeywordMustBeFollowedByTypeName, ModelTypeKeyword);
            }

            // Just accept to a newline
            AcceptUntil(VBSymbolType.NewLine);
            if (!Context.DesignTimeMode)
            {
                // We want the newline to be treated as code, but it causes issues at design-time.
                Optional(VBSymbolType.NewLine);
            }

            string baseType = String.Concat(Span.Symbols.Select(s => s.Content)).Trim();
            Span.CodeGenerator = new SetModelTypeCodeGenerator(baseType, GenericTypeFormatString);

            CheckForInheritsAndModelStatements();
            Output(SpanKind.Code);
            return false;
        }
    }
}
