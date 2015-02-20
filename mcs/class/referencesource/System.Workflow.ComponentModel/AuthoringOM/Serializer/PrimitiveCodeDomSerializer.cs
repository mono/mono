namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Resources;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using Microsoft.CSharp;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics;

    #region Class PrimitiveCodeDomSerializer
    // work around : PD7's PrimitiveCodeDomSerializer does not handle well strings bigger than 200 characters, 
    //       we push our own version to fix it.
    internal class PrimitiveCodeDomSerializer : CodeDomSerializer
    {
        private static readonly string JSharpFileExtension = ".jsl";
        private static PrimitiveCodeDomSerializer defaultSerializer;

        internal static PrimitiveCodeDomSerializer Default
        {
            get
            {
                if (defaultSerializer == null)
                {
                    defaultSerializer = new PrimitiveCodeDomSerializer();
                }
                return defaultSerializer;
            }
        }

        public override object Serialize(IDesignerSerializationManager manager, object value)
        {

            CodeExpression expression = new CodePrimitiveExpression(value);

            if (value == null
                || value is bool
                || value is char
                || value is int
                || value is float
                || value is double)
            {

                // work aroundf for J#, since they don't support auto-boxing of value types yet.
                CodeDomProvider codeProvider = manager.GetService(typeof(CodeDomProvider)) as CodeDomProvider;
                if (codeProvider != null && String.Equals(codeProvider.FileExtension, JSharpFileExtension))
                {
                    // See if we are boxing - if so, insert a cast.
                    ExpressionContext cxt = manager.Context[typeof(ExpressionContext)] as ExpressionContext;
                    //Debug.Assert(cxt != null, "No expression context on stack - J# boxing cast will not be inserted");
                    if (cxt != null)
                    {
                        if (cxt.ExpressionType == typeof(object))
                        {
                            expression = new CodeCastExpression(value.GetType(), expression);
                            expression.UserData.Add("CastIsBoxing", true);
                        }
                    }
                }
                return expression;
            }

            String stringValue = value as string;
            if (stringValue != null)
            {
                // WinWS: The commented code breaks us when we have long strings
                //if (stringValue.Length > 200)
                //{
                // return SerializeToResourceExpression(manager, stringValue);
                //}
                //else 
                return expression;
            }

            // generate a cast for non-int types because we won't parse them properly otherwise because we won't know to convert
            // them to the narrow form.
            //
            return new CodeCastExpression(new CodeTypeReference(value.GetType()), expression);
        }
    }
    #endregion

}
