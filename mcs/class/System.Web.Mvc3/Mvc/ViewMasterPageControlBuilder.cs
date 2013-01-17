namespace System.Web.Mvc {
    using System.CodeDom;
    using System.Web.UI;

    internal sealed class ViewMasterPageControlBuilder : FileLevelMasterPageControlBuilder, IMvcControlBuilder {
        public string Inherits {
            get;
            set;
        }

        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod) {
            if (!String.IsNullOrWhiteSpace(Inherits)) {
                derivedType.BaseTypes[0] = new CodeTypeReference(Inherits);
            }
        }
    }
}
