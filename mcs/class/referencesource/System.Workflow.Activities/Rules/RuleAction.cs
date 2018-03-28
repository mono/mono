#pragma warning disable 1634, 1691
using System;
using System.Text;
using System.CodeDom;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    [Serializable]
    public abstract class RuleAction
    {
        public abstract bool Validate(RuleValidation validator);
        public abstract void Execute(RuleExecution context);
        public abstract ICollection<string> GetSideEffects(RuleValidation validation);
        public abstract RuleAction Clone();
    }

    [Serializable]
    public class RuleHaltAction : RuleAction
    {
        public override bool Validate(RuleValidation validator)
        {
            // Trivial... nothing to validate.
            return true;
        }

        public override void Execute(RuleExecution context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            context.Halted = true;
        }

        public override ICollection<string> GetSideEffects(RuleValidation validation)
        {
            return null;
        }

        public override RuleAction Clone()
        {
            return (RuleAction)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return "Halt";
        }

        public override bool Equals(object obj)
        {
            return (obj is RuleHaltAction);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }


    [Serializable]
    public class RuleUpdateAction : RuleAction
    {
        private string path;

        public RuleUpdateAction(string path)
        {
            this.path = path;
        }

        public RuleUpdateAction()
        {
        }

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public override bool Validate(RuleValidation validator)
        {
            if (validator == null)
                throw new ArgumentNullException("validator");

            bool success = true;

            if (path == null)
            {
                ValidationError error = new ValidationError(Messages.NullUpdate, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = this;
                validator.AddError(error);
                success = false;
            }

            // now make sure that the path is valid
            string[] parts = path.Split('/');
            if (parts[0] == "this")
            {
                Type currentType = validator.ThisType;
                for (int i = 1; i < parts.Length; ++i)
                {
                    if (parts[i] == "*")
                    {
                        if (i < parts.Length - 1)
                        {
                            // The "*" occurred in the middle of the path, which is a no-no.
                            ValidationError error = new ValidationError(Messages.InvalidWildCardInPathQualifier, ErrorNumbers.Error_InvalidWildCardInPathQualifier);
                            error.UserData[RuleUserDataKeys.ErrorObject] = this;
                            validator.AddError(error);
                            success = false;
                            break;
                        }
                        else
                        {
                            // It occurred at the end, which is okay.
                            break;
                        }
                    }
                    else if (string.IsNullOrEmpty(parts[i]) && i == parts.Length - 1)
                    {
                        // It's okay to end with a "/".
                        break;
                    }

                    while (currentType.IsArray)
                        currentType = currentType.GetElementType();

                    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                    if (validator.AllowInternalMembers(currentType))
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
                            string message = string.Format(CultureInfo.CurrentCulture, Messages.UpdateUnknownFieldOrProperty, parts[i]);
                            ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidUpdate);
                            error.UserData[RuleUserDataKeys.ErrorObject] = this;
                            validator.AddError(error);
                            success = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                ValidationError error = new ValidationError(Messages.UpdateNotThis, ErrorNumbers.Error_InvalidUpdate);
                error.UserData[RuleUserDataKeys.ErrorObject] = this;
                validator.AddError(error);
                success = false;
            }

            return success;
        }

        public override void Execute(RuleExecution context)
        {
            // This action has no execution behaviour.
        }

        public override ICollection<string> GetSideEffects(RuleValidation validation)
        {
            return new string[] { this.path };
        }

        public override RuleAction Clone()
        {
            return (RuleAction)this.MemberwiseClone();
        }

        public override string ToString()
        {
            return "Update(\"" + this.path + "\")";
        }

        public override bool Equals(object obj)
        {
#pragma warning disable 56506
            RuleUpdateAction other = obj as RuleUpdateAction;
            return ((other != null) && (string.Equals(this.Path, other.Path, StringComparison.Ordinal)));
#pragma warning restore 56506
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class RuleStatementAction : RuleAction
    {
        private CodeStatement codeDomStatement;

        public RuleStatementAction(CodeStatement codeDomStatement)
        {
            this.codeDomStatement = codeDomStatement;
        }

        public RuleStatementAction(CodeExpression codeDomExpression)
        {
            this.codeDomStatement = new CodeExpressionStatement(codeDomExpression);
        }

        public RuleStatementAction()
        {
        }

        public CodeStatement CodeDomStatement
        {
            get { return codeDomStatement; }
            set { codeDomStatement = value; }
        }

        public override bool Validate(RuleValidation validator)
        {
            if (validator == null)
                throw new ArgumentNullException("validator");

            if (codeDomStatement == null)
            {
                ValidationError error = new ValidationError(Messages.NullStatement, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = this;
                validator.AddError(error);
                return false;
            }
            else
            {
                return CodeDomStatementWalker.Validate(validator, codeDomStatement);
            }
        }

        public override void Execute(RuleExecution context)
        {
            if (codeDomStatement == null)
                throw new InvalidOperationException(Messages.NullStatement);
            CodeDomStatementWalker.Execute(context, codeDomStatement);
        }

        public override ICollection<string> GetSideEffects(RuleValidation validation)
        {
            RuleAnalysis analysis = new RuleAnalysis(validation, true);
            if (codeDomStatement != null)
                CodeDomStatementWalker.AnalyzeUsage(analysis, codeDomStatement);
            return analysis.GetSymbols();
        }

        public override RuleAction Clone()
        {
            RuleStatementAction newAction = (RuleStatementAction)this.MemberwiseClone();
            newAction.codeDomStatement = CodeDomStatementWalker.Clone(codeDomStatement);
            return newAction;
        }

        public override string ToString()
        {
            if (codeDomStatement == null)
                return "";

            StringBuilder decompilation = new StringBuilder();
            CodeDomStatementWalker.Decompile(decompilation, codeDomStatement);
            return decompilation.ToString();
        }

        public override bool Equals(object obj)
        {
#pragma warning disable 56506
            RuleStatementAction other = obj as RuleStatementAction;
            return ((other != null) && (CodeDomStatementWalker.Match(CodeDomStatement, other.CodeDomStatement)));
#pragma warning restore 56506
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
