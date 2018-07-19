using System;
using System.CodeDom;
using System.Text;
using System.Workflow.ComponentModel.Compiler;
using System.Globalization;

namespace System.Workflow.ComponentModel.Serialization
{
    /// <summary>
    /// This class serializes and deserializes CodeTypeReference objects used by rules.
    /// It saves the AssemblyQualifiedName, so that the type can be loaded if the assembly is not
    /// previously loaded at run-time.
    /// </summary>
    internal sealed class CodeTypeReferenceSerializer : WorkflowMarkupSerializer
    {
        // this must match the name used by rules (RuleUserDataKeys.QualifiedName)
        internal const string QualifiedName = "QualifiedName";

        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return (value is CodeTypeReference);
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (serializationManager == null)
                throw new ArgumentNullException("serializationManager");
            if (value == null)
                throw new ArgumentNullException("value");

            CodeTypeReference reference = value as CodeTypeReference;
            if (reference == null)
                return string.Empty;

            // make the typename as best we can, and try to get the fully qualified name
            // if a type is used in an assembly not referenced, GetType will complain
            string typeName = ConvertTypeReferenceToString(reference);
            Type type = serializationManager.GetType(typeName);
            if (type == null)
            {
                // TypeProvider can't find it, see if it's a common type in mscorlib
                type = Type.GetType(typeName, false);
                if (type == null)
                {
                    // still no luck finding it, so simply save the name without assembly information
                    // this is equivalent to what happened before
                    return typeName;
                }
            }
            //
            // If we get a real type make sure that we get the correct fully qualified name for the target framework version
            string assemblyFullName = null;
            TypeProvider typeProvider = serializationManager.GetService(typeof(ITypeProvider)) as TypeProvider;
            if (typeProvider != null)
            {
                assemblyFullName = typeProvider.GetAssemblyName(type);
            }
            //
            // If we didn't find an assembly value it is either a local type or something is wrong
            // However per the general guidance on multi-targeting it is up to the caller
            // to make sure that writers (such as Xoml) are given types that exist in the target framework
            // This makes it the job of the rules designer or rules validator to not call the Xoml stack
            // with types that do not exist in the target framework
            if (string.IsNullOrEmpty(assemblyFullName))
            {
                typeName = type.AssemblyQualifiedName;
            }
            else
            {
                typeName = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", type.FullName, assemblyFullName);
            }
            return typeName;
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (!propertyType.IsAssignableFrom(typeof(CodeTypeReference)))
                return null;

            // if the string is empty or markup extension,
            // then the object is null
            if (string.IsNullOrEmpty(value) || IsValidCompactAttributeFormat(value))
                return null;

            // value is the fully qualified name of the type
            // however, it may refer to non-existant assemblies, so we may get an error
            CodeTypeReference result;
            try
            {
                Type type = serializationManager.GetType(value);
                if (type != null)
                {
                    result = new CodeTypeReference(type);
                    result.UserData[QualifiedName] = type.AssemblyQualifiedName;
                    return result;
                }
            }
            catch (Exception)
            {
                // something went wrong getting the type, so simply pass in the string and
                // let CodeTypeReference figure it out. Note that CodeTypeReference has a method
                // RipOffAssemblyInformationFromTypeName, so assembly names are ignored.
            }
            result = new CodeTypeReference(value);
            result.UserData[QualifiedName] = value;
            return result;
        }

        private static string ConvertTypeReferenceToString(CodeTypeReference reference)
        {
            // CodeTypeReferences are nested structures that represent a type.
            // This code converts one into a string that GetType() should like.

            StringBuilder result;
            if (reference.ArrayElementType != null)
            {
                // type represents an array
                result = new StringBuilder(ConvertTypeReferenceToString(reference.ArrayElementType));
                if (reference.ArrayRank > 0)
                {
                    result.Append("[");
                    result.Append(',', reference.ArrayRank - 1);
                    result.Append("]");
                }
            }
            else
            {
                // type is not an array, but may have type arguments
                result = new StringBuilder(reference.BaseType);
                if ((reference.TypeArguments != null) && (reference.TypeArguments.Count > 0))
                {
                    string prefix = "[";
                    foreach (CodeTypeReference nested in reference.TypeArguments)
                    {
                        result.Append(prefix);
                        result.Append(ConvertTypeReferenceToString(nested));
                        prefix = ", ";
                    }
                    result.Append("]");
                }
            }
            return result.ToString();
        }
    }
}
