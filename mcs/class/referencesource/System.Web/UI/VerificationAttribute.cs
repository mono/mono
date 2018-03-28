//------------------------------------------------------------------------------
// <copyright file="VerificationAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple=true)]
    public sealed class VerificationAttribute : Attribute {

        private string _guideline;       //  "WCAG 1.1", "ADA508"
        private string _checkpoint;      //10.1: in rule and used by checker
        private VerificationReportLevel _reportLevel;//VerificationReportLevel.Error| Warning| Guideline
        private int _priority;           //1,2,3,4
        private string _message;         //"something is in error"
        private VerificationRule _rule;  //VerificationRule.Required | Prohibited
        private string _conditionalProperty; //"foo"
        private VerificationConditionalOperator _conditionalOperator; //VerificationConditionalOperator.Equals | NotEquals
        private string _conditionalValue; //eg. String.Empty 
        private string _guidelineUrl;

        /* minimal constructor
         * implies  VerificationRule.Equals
         *          ConditionalProperty = String.Empty
         *          VerificationConditionalOperator.Equals
         *          ConditionalValue = String.Empty
         *          GuidelineUrl = String.Empty
         */
        public VerificationAttribute (
                    string guideline,
                    string checkpoint,
                    VerificationReportLevel reportLevel,
                    int priority,
                    string message) : 
                        this(guideline, 
                             checkpoint, 
                             reportLevel, 
                             priority, 
                             message, 
                             VerificationRule.Required, /*VerificationRule*/
                             String.Empty, /*ConditionalProperty*/
                             VerificationConditionalOperator.Equals, 
                             String.Empty, /*ConditionalValue*/
                             String.Empty /*GuidelineUrl*/) {
        }

        /* constructor that implies
         *      ConditionalProperty = String.Empty
         *      VerificationConditionalOperator.Equals
         *      ConditionalValue = String.Empty
         *      GuidelineUrl = String.Empty
         */
        /*
        public VerificationAttribute (
                    string guideline,
                    string checkpoint,
                    VerificationReportLevel reportLevel,
                    int priority,
                    string message,
                    VerificationRule rule) : 
                        this(guideline,
                             checkpoint,
                             reportLevel,
                             priority,
                             message,
                             rule,
                             String.Empty, //ConditionalProperty
                             VerificationConditionalOperator.Equals,
                             String.Empty, //ConditionalValue
                             String.Empty) { //GuidelineUrl
        }

        */

        /*specifying just a ConditionalProperty implies:
         *      VerificationConditionalOperator.NotEquals 
         *      ConditionalValue = String.Empty 
         *      GuidelineUrl = String.Empty
         */
        public VerificationAttribute (
                    string guideline,
                    string checkpoint,
                    VerificationReportLevel reportLevel,
                    int priority,
                    string message,
                    VerificationRule rule,
                    string conditionalProperty) : 
                        this(guideline,
                             checkpoint,
                             reportLevel,
                             priority,
                             message,
                             rule, 
                             conditionalProperty,
                             VerificationConditionalOperator.NotEquals,
                             String.Empty, /*ConditionalValue*/
                             String.Empty /*GuidelineUrl*/) {
        }

        /*implies GuidelineUrl = String.Empty */
        internal VerificationAttribute (
                    string guideline,
                    string checkpoint,
                    VerificationReportLevel reportLevel,
                    int priority,
                    string message,
                    VerificationRule rule,
                    string conditionalProperty,
                    VerificationConditionalOperator conditionalOperator,
                    string conditionalValue) : 
                        this(guideline,
                             checkpoint,
                             reportLevel,
                             priority,
                             message,
                             rule,
                             conditionalProperty,
                             conditionalOperator,
                             conditionalValue,
                             String.Empty /*GuidelineUrl*/) {
        }

        public VerificationAttribute(
                    string guideline,
                    string checkpoint,
                    VerificationReportLevel reportLevel,
                    int priority,
                    string message,
                    VerificationRule rule,
                    string conditionalProperty,
                    VerificationConditionalOperator conditionalOperator,
                    string conditionalValue,
                    string guidelineUrl) {

                _guideline = guideline;
                _checkpoint = checkpoint;
                _reportLevel = reportLevel;
                _priority = priority;
                _message = message;
                _rule = rule;
                _conditionalProperty = conditionalProperty;
                _conditionalOperator = conditionalOperator;
                _conditionalValue = conditionalValue;
                _guidelineUrl = guidelineUrl;
        }

        private VerificationAttribute() {
        }

        //WCAG 1.1, ADA508, etc.
        public string Guideline {
            get {
                return _guideline;
            }
        }

        //10.1,  12.4, etc.
        public string Checkpoint {
            get {
                return _checkpoint;
            }
        }

        //VerificationReportLevel.Error | Warning | Guideline
        public VerificationReportLevel VerificationReportLevel {
            get {
                return _reportLevel;
            }
        }

        //1, 2, 3, 4, etc.
        public int Priority {
            get {
                return _priority;
            }
        }

        //message to use if verification rule is true
        public string Message {
            get {
                return _message;
            }
        }

        //VerificationRule.Required | Prohibited
        public VerificationRule VerificationRule {
            get {
                return _rule;
            }
        }

        //name of other control property to condition the assertion
        //used as lhs of conditional expression
        public string ConditionalProperty {
            get {
                return _conditionalProperty;
            }
        }

        //VerificationConditionalOperator.Equals | NotEquals
        //operator to apply to condition statement
        public VerificationConditionalOperator VerificationConditionalOperator {
            get {
                return _conditionalOperator;
            }
        }

        //value to use as rhs in conditional expression
        public string ConditionalValue {
            get {
                return _conditionalValue;
            }
        }

        public string GuidelineUrl {
            get {
                return _guidelineUrl;
            }
        }
    }

    public enum VerificationRule {
        Required,
        Prohibited,
        NotEmptyString
    }

    public enum VerificationReportLevel {
        Error,
        Warning,
        Guideline
    }

    public enum VerificationConditionalOperator {
        Equals,
        NotEquals
    }
}

