// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Reflection;
using System.Workflow.Activities.Common;


namespace System.Workflow.Activities.Rules
{
    public enum RuleAttributeTarget
    {
        Parameter,
        This
    }

    public abstract class RuleAttribute : Attribute
    {
        internal abstract bool Validate(RuleValidation validation, MemberInfo member, Type contextType, ParameterInfo[] parameters);
        internal abstract void Analyze(RuleAnalysis analysis, MemberInfo member, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions);
    }

    public abstract class RuleReadWriteAttribute : RuleAttribute
    {
        private RuleAttributeTarget attributeTarget;
        private string attributePath;

        protected RuleReadWriteAttribute(string path, RuleAttributeTarget target)
        {
            this.attributeTarget = target;
            this.attributePath = path;
        }

        public string Path
        {
            get { return attributePath; }
        }

        public RuleAttributeTarget Target
        {
            get { return attributeTarget; }
        }

        internal override bool Validate(RuleValidation validation, MemberInfo member, Type contextType, ParameterInfo[] parameters)
        {
            ValidationError error = null;
            string message = null;

            if (string.IsNullOrEmpty(attributePath))
            {
                // It is allowed to pass null or the empty string to [RuleRead] or [RuleWrite].  This
                // is how you indicate that a method or property has no dependencies or side effects.
                return true;
            }

            bool valid = true;

            string[] parts = attributePath.Split('/');

            // Check the first part.

            string firstPart = parts[0];
            int startOfRelativePortion = 0;
            if (attributeTarget == RuleAttributeTarget.This)
            {
                // When target is "This", the path is allowed to start with the token "this".  It is
                // then skipped for the rest of the validation, and the contextType remains what it
                // was when passed in.
                if (firstPart == "this")
                    ++startOfRelativePortion;
            }
            else
            {
                // When target is "Parameter", the path must start with the name of a parameter.
                bool found = false;
                for (int p = 0; p < parameters.Length; ++p)
                {
                    ParameterInfo param = parameters[p];
                    if (param.Name == firstPart)
                    {
                        found = true;

                        // The context type is the parameter type.
                        contextType = param.ParameterType;
                        break;
                    }
                }

                if (!found)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.InvalidRuleAttributeParameter, firstPart, member.Name);
                    error = new ValidationError(message, ErrorNumbers.Error_InvalidRuleAttributeParameter);
                    error.UserData[RuleUserDataKeys.ErrorObject] = this;
                    validation.AddError(error);
                    return false;
                }

                ++startOfRelativePortion;
            }

            int numParts = parts.Length;

            // Check the last part.  The last part is allowed to be empty, or "*".

            string lastPart = parts[numParts - 1];
            if (string.IsNullOrEmpty(lastPart) || lastPart == "*")
                numParts -= 1;

            // Check the rest of the parts.

            Type currentType = contextType;
            for (int i = startOfRelativePortion; i < numParts; ++i)
            {
                // Can't have embedded "*" wildcards.
                if (parts[i] == "*")
                {
                    // The "*" occurred in the middle of the path, which is a no-no.
                    error = new ValidationError(Messages.InvalidWildCardInPathQualifier, ErrorNumbers.Error_InvalidWildCardInPathQualifier);
                    error.UserData[RuleUserDataKeys.ErrorObject] = this;
                    validation.AddError(error);
                    valid = false;
                    break;
                }

                // Skip array types.
                while (currentType.IsArray)
                    currentType = currentType.GetElementType();

                // Make sure the member exists in the current type.
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                if (validation.AllowInternalMembers(currentType))
                    bindingFlags |= BindingFlags.NonPublic;

                FieldInfo field = currentType.GetField(parts[i], bindingFlags);
                if (field != null)
                {
                    currentType = field.FieldType;
                }
                else
                {
                    PropertyInfo property = currentType.GetProperty(parts[i], bindingFlags);
                    if (property != null)
                    {
                        currentType = property.PropertyType;
                    }
                    else
                    {
                        message = string.Format(CultureInfo.CurrentCulture, Messages.UpdateUnknownFieldOrProperty, parts[i]);
                        error = new ValidationError(message, ErrorNumbers.Error_UnknownFieldOrProperty);
                        error.UserData[RuleUserDataKeys.ErrorObject] = this;
                        validation.AddError(error);
                        valid = false;
                        break;
                    }
                }
            }

            return valid;
        }

        internal void AnalyzeReadWrite(RuleAnalysis analysis, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            if (string.IsNullOrEmpty(attributePath))
            {
                // If the suffix is null or empty, this means the RuleAttributeTarget has no dependencies.
                if (attributeTarget == RuleAttributeTarget.This)
                {
                    // The target object has no dependencies.
                    attributedExpressions.Add(targetExpression);
                }
                else if (attributeTarget == RuleAttributeTarget.Parameter)
                {
                    // ALL arguments have no dependencies.
                    for (int i = 0; i < argumentExpressions.Count; ++i)
                        attributedExpressions.Add(argumentExpressions[i]);
                }
            }
            else
            {
                string suffix = attributePath;

                bool isRead = !analysis.ForWrites;
                bool isWrite = analysis.ForWrites;

                if (attributeTarget == RuleAttributeTarget.This)
                {
                    // Target is "This", so perform the analysis on the target expression.

                    // Remove the optional "this/" token if present.
                    string optionalPrefix = "this/";
                    if (suffix.StartsWith(optionalPrefix, StringComparison.Ordinal))
                        suffix = suffix.Substring(optionalPrefix.Length);

                    RuleExpressionWalker.AnalyzeUsage(analysis, targetExpression, isRead, isWrite, new RulePathQualifier(suffix, targetQualifier));
                    attributedExpressions.Add(targetExpression);
                }
                else if (attributeTarget == RuleAttributeTarget.Parameter)
                {
                    string paramName = null;

                    int firstSlash = suffix.IndexOf('/');
                    if (firstSlash >= 0)
                    {
                        paramName = suffix.Substring(0, firstSlash);
                        suffix = suffix.Substring(firstSlash + 1);
                    }
                    else
                    {
                        paramName = suffix;
                        suffix = null;
                    }

                    // Find the ParameterInfo that corresponds to this attribute path.
                    ParameterInfo param = Array.Find<ParameterInfo>(parameters,
                                                                    delegate(ParameterInfo p) { return p.Name == paramName; });
                    if (param != null)
                    {
                        RulePathQualifier qualifier = string.IsNullOrEmpty(suffix) ? null : new RulePathQualifier(suffix, null);

                        // 99.9% of the time, the parameter usage attribute only applies to one argument.  However,
                        // if this attribute corresponds to the last parameter, then just assume that all the trailing
                        // arguments correspond.  (In other words, if the caller passed more arguments then there
                        // are parameters, we assume it was a params array.)
                        //
                        // Usually this loop will only execute once.
                        int end = param.Position + 1;
                        if (param.Position == parameters.Length - 1)
                            end = argumentExpressions.Count;

                        for (int i = param.Position; i < end; ++i)
                        {
                            CodeExpression argExpr = argumentExpressions[i];
                            RuleExpressionWalker.AnalyzeUsage(analysis, argExpr, isRead, isWrite, qualifier);
                            attributedExpressions.Add(argExpr);
                        }
                    }
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RuleReadAttribute : RuleReadWriteAttribute
    {
        public RuleReadAttribute(string path, RuleAttributeTarget target)
            : base(path, target)
        {
        }

        public RuleReadAttribute(string path)
            : base(path, RuleAttributeTarget.This)
        {
        }

        internal override void Analyze(RuleAnalysis analysis, MemberInfo member, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            // A RuleRead attribute is only applicable if we're analyzing for reads.
            if (analysis.ForWrites)
                return;

            base.AnalyzeReadWrite(analysis, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RuleWriteAttribute : RuleReadWriteAttribute
    {
        public RuleWriteAttribute(string path, RuleAttributeTarget target)
            : base(path, target)
        {
        }

        public RuleWriteAttribute(string path)
            : base(path, RuleAttributeTarget.This)
        {
        }

        internal override void Analyze(RuleAnalysis analysis, MemberInfo member, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            // A RuleWrite attribute is only applicable if we're analyzing for writes.
            if (!analysis.ForWrites)
                return;

            base.AnalyzeReadWrite(analysis, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RuleInvokeAttribute : RuleAttribute
    {
        private string methodInvoked;

        public RuleInvokeAttribute(string methodInvoked)
        {
            this.methodInvoked = methodInvoked;
        }

        public string MethodInvoked
        {
            get { return methodInvoked; }
        }

        internal override bool Validate(RuleValidation validation, MemberInfo member, Type contextType, ParameterInfo[] parameters)
        {
            Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
            methodStack.Push(member);

            bool result = ValidateInvokeAttribute(validation, member, contextType, methodStack);

            methodStack.Pop();

            return result;
        }

        private bool ValidateInvokeAttribute(RuleValidation validation, MemberInfo member, Type contextType, Stack<MemberInfo> methodStack)
        {
            string message;
            ValidationError error;

            if (string.IsNullOrEmpty(methodInvoked))
            {
                // Invoked method or property name was null or empty.
                message = string.Format(CultureInfo.CurrentCulture, Messages.AttributeMethodNotFound, member.Name, this.GetType().Name, Messages.NullValue);
                error = new ValidationError(message, ErrorNumbers.Warning_RuleAttributeNoMatch, true);
                error.UserData[RuleUserDataKeys.ErrorObject] = this;
                validation.AddError(error);
                return false;
            }

            bool valid = true;

            // Go through all the methods and properties on the target context,
            // looking for all the ones that match the name on the attribute.
            MemberInfo[] members = contextType.GetMember(methodInvoked, MemberTypes.Method | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            if (members == null || members.Length == 0)
            {
                // Invoked method or property didn't exist.
                message = string.Format(CultureInfo.CurrentCulture, Messages.AttributeMethodNotFound, member.Name, this.GetType().Name, methodInvoked);
                error = new ValidationError(message, ErrorNumbers.Warning_RuleAttributeNoMatch, true);
                error.UserData[RuleUserDataKeys.ErrorObject] = this;
                validation.AddError(error);
                valid = false;
            }
            else
            {
                for (int i = 0; i < members.Length; ++i)
                {
                    MemberInfo mi = members[i];
                    if (!methodStack.Contains(mi)) // Prevent recursion
                    {
                        methodStack.Push(mi);

                        object[] attrs = mi.GetCustomAttributes(typeof(RuleAttribute), true);
                        if (attrs != null && attrs.Length != 0)
                        {
                            foreach (RuleAttribute invokedRuleAttr in attrs)
                            {
                                RuleReadWriteAttribute readWriteAttr = invokedRuleAttr as RuleReadWriteAttribute;
                                if (readWriteAttr != null)
                                {
                                    // This read/write attribute may not specify a target of "Parameter", since
                                    // we can't map from the invoker's parameters to the invokee's parameters.
                                    if (readWriteAttr.Target == RuleAttributeTarget.Parameter)
                                    {
                                        message = string.Format(CultureInfo.CurrentCulture, Messages.InvokeAttrRefersToParameterAttribute, mi.Name);
                                        error = new ValidationError(message, ErrorNumbers.Error_InvokeAttrRefersToParameterAttribute, true);
                                        error.UserData[RuleUserDataKeys.ErrorObject] = this;
                                        validation.AddError(error);
                                        valid = false;
                                    }
                                    else
                                    {
                                        // Validate the read/write attribute normally.
                                        readWriteAttr.Validate(validation, mi, contextType, null);
                                    }
                                }
                                else
                                {
                                    RuleInvokeAttribute invokeAttr = (RuleInvokeAttribute)invokedRuleAttr;
                                    invokeAttr.ValidateInvokeAttribute(validation, mi, contextType, methodStack);
                                }
                            }
                        }

                        methodStack.Pop();
                    }
                }
            }

            return valid;
        }

        internal override void Analyze(RuleAnalysis analysis, MemberInfo member, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
            methodStack.Push(member);

            AnalyzeInvokeAttribute(analysis, member.DeclaringType, methodStack, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);

            methodStack.Pop();
        }

        private void AnalyzeInvokeAttribute(RuleAnalysis analysis, Type contextType, Stack<MemberInfo> methodStack, CodeExpression targetExpression, RulePathQualifier targetQualifier, CodeExpressionCollection argumentExpressions, ParameterInfo[] parameters, List<CodeExpression> attributedExpressions)
        {
            // Go through all the methods and properties on the target context,
            // looking for all the ones that match the name on the attribute.
            MemberInfo[] members = contextType.GetMember(methodInvoked, MemberTypes.Method | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            for (int m = 0; m < members.Length; ++m)
            {
                MemberInfo mi = members[m];
                if (!methodStack.Contains(mi)) // Prevent recursion
                {
                    methodStack.Push(mi);

                    object[] attrs = mi.GetCustomAttributes(typeof(RuleAttribute), true);
                    if (attrs != null && attrs.Length != 0)
                    {
                        RuleAttribute[] ruleAttrs = (RuleAttribute[])attrs;
                        for (int i = 0; i < ruleAttrs.Length; ++i)
                        {
                            RuleAttribute ruleAttr = ruleAttrs[i];

                            RuleReadWriteAttribute readWriteAttr = ruleAttr as RuleReadWriteAttribute;
                            if (readWriteAttr != null)
                            {
                                // Just analyze the read/write attribute normally.
                                readWriteAttr.Analyze(analysis, mi, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
                            }
                            else
                            {
                                RuleInvokeAttribute invokeAttr = (RuleInvokeAttribute)ruleAttr;
                                invokeAttr.AnalyzeInvokeAttribute(analysis, contextType, methodStack, targetExpression, targetQualifier, argumentExpressions, parameters, attributedExpressions);
                            }
                        }
                    }

                    methodStack.Pop();
                }
            }
        }
    }
}
