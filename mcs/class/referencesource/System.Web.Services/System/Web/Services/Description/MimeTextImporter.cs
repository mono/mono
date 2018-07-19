namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Xml.Serialization;

    internal class MimeTextReturn : MimeReturn {
        MimeTextBinding textBinding;

        internal MimeTextBinding TextBinding {
            get { return textBinding; }
            set { textBinding = value; }
        }
    }

    internal class MimeTextImporter : MimeImporter {
        string methodName;

        internal override MimeParameterCollection ImportParameters() {
            return null;
        }

        internal override MimeReturn ImportReturn() {
            MimeTextBinding mimeTextBinding = (MimeTextBinding)ImportContext.OperationBinding.Output.Extensions.Find(typeof(MimeTextBinding));
            if (mimeTextBinding == null) return null;
            if (mimeTextBinding.Matches.Count == 0) {
                ImportContext.UnsupportedOperationBindingWarning(Res.GetString(Res.MissingMatchElement0));
                return null;
            }
            methodName = CodeIdentifier.MakeValid(ImportContext.OperationBinding.Name);

            MimeTextReturn importedReturn = new MimeTextReturn();
            importedReturn.TypeName = ImportContext.ClassNames.AddUnique(methodName + "Matches", mimeTextBinding);
            importedReturn.TextBinding = mimeTextBinding;
            importedReturn.ReaderType = typeof(TextReturnReader);
            return importedReturn;
        }

        internal override void GenerateCode(MimeReturn[] importedReturns, MimeParameterCollection[] importedParameters) { 
            for (int i = 0; i < importedReturns.Length; i++) {
                if (importedReturns[i] is MimeTextReturn) {  
                    GenerateCode((MimeTextReturn)importedReturns[i], ImportContext.ServiceImporter.CodeGenerationOptions);
                }
            }
        }

        void GenerateCode(MimeTextReturn importedReturn, CodeGenerationOptions options) {
            GenerateCode(importedReturn.TypeName, importedReturn.TextBinding.Matches, options);
        }

        void GenerateCode(string typeName, MimeTextMatchCollection matches, CodeGenerationOptions options) {
            CodeIdentifiers members = new CodeIdentifiers();
            CodeTypeDeclaration codeClass = WebCodeGenerator.AddClass(ImportContext.CodeNamespace, typeName, string.Empty, new string[0], null, CodeFlags.IsPublic, 
                ImportContext.ServiceImporter.CodeGenerator.Supports(GeneratorSupport.PartialTypes));

            string[] fieldTypeNames = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++) {
                MimeTextMatch match = matches[i];
                string name = members.AddUnique(CodeIdentifier.MakeValid(match.Name.Length == 0 ? methodName + "Match" : match.Name), match);
                CodeAttributeDeclarationCollection metadata = new CodeAttributeDeclarationCollection();
                if (match.Pattern.Length == 0) throw new ArgumentException(Res.GetString(Res.WebTextMatchMissingPattern));

                CodeExpression pattern = new CodePrimitiveExpression(match.Pattern);
                int numPropValues = 0;
                if (match.Group != 1) 
                    numPropValues++;
                if (match.Capture != 0)
                    numPropValues++;
                if (match.IgnoreCase)
                    numPropValues++;
                if (match.Repeats != 1 && match.Repeats != int.MaxValue)
                    numPropValues++;
                CodeExpression[] propertyValues = new CodeExpression[numPropValues];
                string[] propertyNames = new string[propertyValues.Length];
                numPropValues = 0;
                if (match.Group != 1) {
                    propertyValues[numPropValues] = new CodePrimitiveExpression(match.Group);
                    propertyNames[numPropValues] = "Group";
                    numPropValues++;
                }
                if (match.Capture != 0) {
                    propertyValues[numPropValues] = new CodePrimitiveExpression(match.Capture);
                    propertyNames[numPropValues] = "Capture";
                    numPropValues++;
                }
                if (match.IgnoreCase) {
                    propertyValues[numPropValues] = new CodePrimitiveExpression(match.IgnoreCase);
                    propertyNames[numPropValues] = "IgnoreCase";
                    numPropValues++;
                }
                if (match.Repeats != 1 && match.Repeats != int.MaxValue) {
                    propertyValues[numPropValues] = new CodePrimitiveExpression(match.Repeats);
                    propertyNames[numPropValues] = "MaxRepeats";
                    numPropValues++;
                }
                WebCodeGenerator.AddCustomAttribute(metadata, typeof(MatchAttribute), new CodeExpression[] { pattern }, propertyNames, propertyValues);

                string fieldTypeName;
                if (match.Matches.Count > 0) {
                    fieldTypeName = ImportContext.ClassNames.AddUnique(CodeIdentifier.MakeValid(match.Type.Length == 0 ? name : match.Type), match);
                    fieldTypeNames[i] = fieldTypeName;
                }
                else {
                    fieldTypeName = typeof(string).FullName;
                }
                if (match.Repeats != 1)
                    fieldTypeName += "[]";
                
                CodeTypeMember member = WebCodeGenerator.AddMember(codeClass, fieldTypeName, name, null, metadata, CodeFlags.IsPublic, options);
                
                if (match.Matches.Count == 0 && match.Type.Length > 0) {
                    ImportContext.Warnings |= ServiceDescriptionImportWarnings.OptionalExtensionsIgnored;
                    ProtocolImporter.AddWarningComment(member.Comments, Res.GetString(Res.WebTextMatchIgnoredTypeWarning));
                }
            }

            for (int i = 0; i < fieldTypeNames.Length; i++) {
                string fieldTypeName = fieldTypeNames[i];
                if (fieldTypeName != null) {
                    GenerateCode(fieldTypeName, matches[i].Matches, options);
                }
            }
        }

    }
}

