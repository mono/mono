//------------------------------------------------------------------------------
// <copyright file="Function.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace MS.Internal.Xml.XPath {
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Diagnostics;
    using System.Collections;

    internal class Function : AstNode {
        public enum FunctionType {
            FuncLast,
            FuncPosition,
            FuncCount,
            FuncID,
            FuncLocalName,
            FuncNameSpaceUri,
            FuncName,
            FuncString,
            FuncBoolean,
            FuncNumber,
            FuncTrue,
            FuncFalse,
            FuncNot,
            FuncConcat,
            FuncStartsWith,
            FuncContains,
            FuncSubstringBefore,
            FuncSubstringAfter,
            FuncSubstring,
            FuncStringLength,
            FuncNormalize,
            FuncTranslate,
            FuncLang,
            FuncSum,
            FuncFloor,
            FuncCeiling,
            FuncRound,
            FuncUserDefined,
        };

        private FunctionType functionType;
        private ArrayList argumentList;

        private string name = null;
        private string prefix = null;

        public Function(FunctionType ftype, ArrayList argumentList) {
            this.functionType = ftype;
            this.argumentList = new ArrayList(argumentList);
        }

        public Function(string prefix, string name, ArrayList argumentList) {
            this.functionType = FunctionType.FuncUserDefined;
            this.prefix = prefix;
            this.name = name;
            this.argumentList = new ArrayList(argumentList);
        }

        public Function(FunctionType ftype) {
            this.functionType = ftype;
        }

        public Function(FunctionType ftype, AstNode arg) {
            functionType = ftype;
            argumentList = new ArrayList();
            argumentList.Add(arg);
        }

        public override AstType Type { get {return  AstType.Function;} }

        public override XPathResultType ReturnType {
            get {
                return ReturnTypes[(int) functionType];
            }
        }

        public FunctionType TypeOfFunction { get { return functionType; } }
        public ArrayList    ArgumentList   { get { return argumentList; } }
        public string       Prefix         { get { return prefix;       } }
        public string       Name           { get { return name;         } }

        internal static XPathResultType[] ReturnTypes = {
            /* FunctionType.FuncLast            */ XPathResultType.Number ,
            /* FunctionType.FuncPosition        */ XPathResultType.Number ,
            /* FunctionType.FuncCount           */ XPathResultType.Number ,
            /* FunctionType.FuncID              */ XPathResultType.NodeSet,
            /* FunctionType.FuncLocalName       */ XPathResultType.String ,
            /* FunctionType.FuncNameSpaceUri    */ XPathResultType.String ,
            /* FunctionType.FuncName            */ XPathResultType.String ,
            /* FunctionType.FuncString          */ XPathResultType.String ,
            /* FunctionType.FuncBoolean         */ XPathResultType.Boolean,
            /* FunctionType.FuncNumber          */ XPathResultType.Number ,
            /* FunctionType.FuncTrue            */ XPathResultType.Boolean,
            /* FunctionType.FuncFalse           */ XPathResultType.Boolean,
            /* FunctionType.FuncNot             */ XPathResultType.Boolean,
            /* FunctionType.FuncConcat          */ XPathResultType.String ,
            /* FunctionType.FuncStartsWith      */ XPathResultType.Boolean,
            /* FunctionType.FuncContains        */ XPathResultType.Boolean,
            /* FunctionType.FuncSubstringBefore */ XPathResultType.String ,
            /* FunctionType.FuncSubstringAfter  */ XPathResultType.String ,
            /* FunctionType.FuncSubstring       */ XPathResultType.String ,
            /* FunctionType.FuncStringLength    */ XPathResultType.Number ,
            /* FunctionType.FuncNormalize       */ XPathResultType.String ,
            /* FunctionType.FuncTranslate       */ XPathResultType.String ,
            /* FunctionType.FuncLang            */ XPathResultType.Boolean,
            /* FunctionType.FuncSum             */ XPathResultType.Number ,
            /* FunctionType.FuncFloor           */ XPathResultType.Number ,
            /* FunctionType.FuncCeiling         */ XPathResultType.Number ,
            /* FunctionType.FuncRound           */ XPathResultType.Number ,
            /* FunctionType.FuncUserDefined     */ XPathResultType.Any  
        };
    }
}
