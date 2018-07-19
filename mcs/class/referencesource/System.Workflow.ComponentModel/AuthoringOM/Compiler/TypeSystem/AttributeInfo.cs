namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Summary description for AttributeInfo.
    /// </summary>
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class AttributeInfoAttribute : Attribute
    {
        private AttributeInfo attributeInfo;

        internal AttributeInfoAttribute(AttributeInfo attributeInfo)
        {
            if (attributeInfo == null)
                throw new ArgumentNullException("attributeInfo");

            this.attributeInfo = attributeInfo;
        }

        internal static AttributeInfoAttribute CreateAttributeInfoAttribute(Type attributeType, string[] argumentNames, object[] argumentValues)
        {
            return new AttributeInfoAttribute(new AttributeInfo(attributeType, argumentNames, argumentValues));
        }

        public AttributeInfo AttributeInfo
        {
            get
            {
                return this.attributeInfo;
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class AttributeInfo
    {
        #region Members and Constructors

        private Type attributeType;
        private string[] argumentNames;
        private object[] argumentValues;

        internal AttributeInfo(Type attributeType, string[] argumentNames, object[] argumentValues)
        {
            this.attributeType = attributeType;
            this.argumentNames = (string[])argumentNames.Clone();
            this.argumentValues = (object[])argumentValues.Clone();
        }

        #endregion

        #region Properties

        public Type AttributeType
        {
            get
            {
                return attributeType;
            }
        }

        public ReadOnlyCollection<object> ArgumentValues
        {
            get
            {
                List<object> arguments = new List<object>(this.argumentValues);
                return arguments.AsReadOnly();
            }
        }

        public bool Creatable
        {
            get
            {
                if (attributeType.Assembly == null)
                    return false;

                foreach (object argument in argumentValues)
                {
                    if (argument is Exception)
                        return false;
                }
                return true;
            }
        }

        #endregion

        #region Public methods

        public Attribute CreateAttribute()
        {
            if (!Creatable)
                throw new InvalidOperationException(SR.GetString(SR.CannotCreateAttribute));

            List<string> propertyNames = new List<string>();
            ArrayList propertyValues = new ArrayList();
            ArrayList constructorArguments = new ArrayList();

            // go over the arguments, seperate named vs. non-named arguments
            for (int loop = 0; loop < argumentNames.Length; loop++)
            {
                if ((argumentNames[loop] == null) || (argumentNames[loop].Length == 0))
                    constructorArguments.Add(argumentValues[loop]);
                else
                {
                    propertyNames.Add(argumentNames[loop]);
                    propertyValues.Add(argumentValues[loop]);
                }
            }

            // creating the Attribute
            Attribute attribute = (Attribute)Activator.CreateInstance(attributeType, constructorArguments.ToArray());

            // setting named properties
            for (int loop = 0; loop < propertyNames.Count; loop++)
            {
                // 
                attributeType.GetProperty(propertyNames[loop]).SetValue(attribute, propertyValues[loop], null);
            }

            return attribute;
        }

        public object GetArgumentValueAs(IServiceProvider serviceProvider, int argumentIndex, Type requestedType)
        {
            if (argumentIndex >= this.ArgumentValues.Count || argumentIndex < 0)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentIndex), "argumentIndex");

            if (requestedType == null)
                throw new ArgumentNullException("requestedType");

            SupportedLanguages language = CompilerHelpers.GetSupportedLanguage(serviceProvider);

            if (requestedType == typeof(string))
            {
                string returnValue = this.ArgumentValues[argumentIndex] as string;

                // string values read by the code-dom parser are double escaped, so
                // remove the 2nd escaping (we need to leave the escaping in at parse time)
                // in case the attribute argument is never processed and emitted as 
                // the code snippet
                if (returnValue != null)
                {
                    try
                    {
                        returnValue = Regex.Unescape(returnValue);
                    }
                    catch
                    {
                    }
                }

                if (returnValue != null)
                {
                    if (returnValue.EndsWith("\"", StringComparison.Ordinal))
                        returnValue = returnValue.Substring(0, returnValue.Length - 1);

                    if (language == SupportedLanguages.CSharp && returnValue.StartsWith("@\"", StringComparison.Ordinal))
                        returnValue = returnValue.Substring(2, returnValue.Length - 2);
                    else if (returnValue.StartsWith("\"", StringComparison.Ordinal))
                        returnValue = returnValue.Substring(1, returnValue.Length - 1);
                }

                return returnValue;
            }
            else if (requestedType.IsEnum)
            {
                string parseableValue = "";
                bool firstValue = true;
                foreach (string enumValue in (this.ArgumentValues[argumentIndex] as string).Split(new string[] { language == SupportedLanguages.CSharp ? "|" : "Or" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!firstValue)
                        parseableValue += ",";

                    int valueSep = enumValue.LastIndexOf('.');
                    if (valueSep != -1)
                        parseableValue += enumValue.Substring(valueSep + 1);
                    else
                        parseableValue += enumValue;

                    firstValue = false;
                }

                return Enum.Parse(requestedType, parseableValue);
            }
            else if (requestedType == typeof(bool))
            {
                return System.Convert.ToBoolean(this.ArgumentValues[argumentIndex], CultureInfo.InvariantCulture);
            }
            else if (requestedType == typeof(Type))
            {
                string typeName = "";
                if (this.ArgumentValues[argumentIndex] is CodeTypeOfExpression)
                    typeName = DesignTimeType.GetTypeNameFromCodeTypeReference((this.ArgumentValues[argumentIndex] as CodeTypeOfExpression).Type, null);

                ITypeProvider typeProvider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
                if (typeProvider == null)
                    throw new Exception(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).ToString()));

                Type returnType = ParseHelpers.ParseTypeName(typeProvider, language, typeName);
                if (returnType == null)
                {
                    // Try to parse the attribute value manually
                    string[] genericParamTypeNames = null;
                    string baseTypeName = string.Empty;
                    string elementDecorators = string.Empty;
                    if (ParseHelpers.ParseTypeName(typeName, language == SupportedLanguages.CSharp ? ParseHelpers.ParseTypeNameLanguage.CSharp : ParseHelpers.ParseTypeNameLanguage.VB, out baseTypeName, out genericParamTypeNames, out elementDecorators))
                    {
                        if (baseTypeName != null && genericParamTypeNames != null)
                        {
                            string parsedTypeName = baseTypeName + "`" + genericParamTypeNames.Length.ToString(CultureInfo.InvariantCulture) + "[";
                            foreach (string genericArg in genericParamTypeNames)
                            {
                                if (genericArg != genericParamTypeNames[0])
                                    parsedTypeName += ",";

                                Type genericArgType = ParseHelpers.ParseTypeName(typeProvider, language, genericArg);
                                if (genericArgType != null)
                                    parsedTypeName += "[" + genericArgType.FullName + "]";
                                else
                                    parsedTypeName += "[" + genericArg + "]";
                            }
                            parsedTypeName += "]";

                            returnType = ParseHelpers.ParseTypeName(typeProvider, language, parsedTypeName);
                        }
                    }
                }

                return returnType;
            }

            return null;
        }

        #endregion
    }
}
