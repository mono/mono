// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    [DataContract]
    internal class ArgumentInfo
    {
        // the following two fields are never to be serialized.
        private Type type;
        private bool HasGetTypeBeenAttempted;
                
        string versionlessAssemblyQualifiedTypeName;
        string name;
        string fullAssemblyQualifiedTypeName;
        ArgumentDirection direction;
                
        public ArgumentInfo(RuntimeArgument argument)
        {
            this.Name = argument.Name;

            Fx.Assert(argument.Type != null, "argument Type must not be null.");
            this.Type = argument.Type;
            this.HasGetTypeBeenAttempted = true;

            this.FullAssemblyQualifiedTypeName = this.Type.AssemblyQualifiedName;
            
            // this versionless assembly-qualified type name causes types of different versions 
            //  to be considered equal for the sake of argument matching.
            // Serializing the argument type info in a string format allows
            //  the map to be loaded into environment in which the types may not be available.
            this.versionlessAssemblyQualifiedTypeName = GenerateVersionlessAssemblyQualifiedTypeName(argument.Type);

            this.Direction = argument.Direction;
        }

        private Type Type
        {
            get
            {
                if (this.type == null && !this.HasGetTypeBeenAttempted)
                {
                    // For every deserialized ArgumentInfo, we are here only the very first call to the property getter.
                    this.HasGetTypeBeenAttempted = true;
                    try
                    {
                        this.type = Type.GetType(this.FullAssemblyQualifiedTypeName, false);                    
                    }
                    catch (Exception e)
                    {
                        if (e is TypeLoadException || e is FileNotFoundException || e is FileLoadException || e is ArgumentException)
                        {
                            this.type = null;
                            FxTrace.Exception.AsWarning(e);
                        }
                        else
                        {
                            throw;
                        }
                    }                    
                }
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
               
        public string Name 
        {
            get
            {
                return this.name;
            }
            private set
            {
                this.name = value;
            }
        }               
        
        public string FullAssemblyQualifiedTypeName
        {
            get
            {
                return this.fullAssemblyQualifiedTypeName;
            }
            private set
            {
                this.fullAssemblyQualifiedTypeName = value;
            }
        }                

        public ArgumentDirection Direction 
        {
            get
            {
                return this.direction;
            }
            private set
            {
                this.direction = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "VersionlessAssemblyQualifiedTypeName")]
        internal string SerializedVersionlessAssemblyQualifiedTypeName
        {
            get { return this.versionlessAssemblyQualifiedTypeName; }
            set { this.versionlessAssemblyQualifiedTypeName = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "Name")]
        internal string SerializedName
        {
            get { return this.Name; }
            set { this.Name = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "FullAssemblyQualifiedTypeName")]
        internal string SerializedFullAssemblyQualifiedTypeName
        {
            get { return this.FullAssemblyQualifiedTypeName; }
            set { this.FullAssemblyQualifiedTypeName = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "Direction")]
        internal ArgumentDirection SerializedDirection
        {
            get { return this.Direction; }
            set { this.Direction = value; }
        }

        private static bool TypeEquals(ArgumentInfo left, ArgumentInfo right)
        {
            Fx.Assert(left != null && right != null, "both left and right must not be null.");

            if (left.versionlessAssemblyQualifiedTypeName == right.versionlessAssemblyQualifiedTypeName)
            {
                return true;
            }

            //
            // Try to determine if the two argument types are in fact the same due to one being a TypeForwardedTo type to the other.
            // When forwarded types are used, it is expected that all the assemblies involved in type forwarding to be always available,
            //  whether during map calcuation, during implementation map rollup, or  during merging of multiple maps.
            // 
            if (left.Type != null && right.Type != null && left.Type == right.Type)
            {
                return true;
            }

            return false;
        }

        public static bool Equals(ArgumentInfo left, ArgumentInfo right)
        {
            if (left == null)
            {
                return right == null;
            }

            return right != null &&
                left.Name == right.Name && TypeEquals(left, right) && left.Direction == right.Direction;
        }

        public static IList<ArgumentInfo> List(Activity activity)
        {
            if (activity.RuntimeArguments == null)
            {
                return new List<ArgumentInfo>();
            }

            return (from r in activity.RuntimeArguments select new ArgumentInfo(r)).ToList();
        }

        public override bool Equals(object obj)
        {
            ArgumentInfo operand = obj as ArgumentInfo;
            return Equals(this, operand);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // generate assembly-qualified type name without assembly's version, culture and public token info
        static string GenerateVersionlessAssemblyQualifiedTypeName(Type type)
        {
            StringBuilder sb = new StringBuilder();

            BuildTypeSpec(sb, type);

            return sb.ToString();
        }

        static void BuildTypeSpec(StringBuilder sb, Type type)
        {
            if (type.IsByRef)
            {
                BuildReferenceTypeSpec(sb, type);
            }
            else
            {
                BuildSimpleTypeSpec(sb, type);
            }
        }

        static void BuildReferenceTypeSpec(StringBuilder sb, Type type)
        {
            Fx.Assert(type.HasElementType, "This type must have an element type.");

            BuildSimpleTypeSpec(sb, type.GetElementType());
            sb.Append('&');
        }

        static void BuildSimpleTypeSpec(StringBuilder sb, Type type)
        {
            if (type.IsPointer)
            {
                BuildPointerTypeSpec(sb, type);
            }
            else if (type.IsArray)
            {
                BuildArrayTypeSpec(sb, type);
            }
            else
            {
                BuildTypeName(sb, type);
            }
        }

        static void BuildPointerTypeSpec(StringBuilder sb, Type type)
        {
            Fx.Assert(type.HasElementType, "This type must have an element type.");

            BuildSimpleTypeSpec(sb, type.GetElementType());
            sb.Append('*');
        }

        static void BuildArrayTypeSpec(StringBuilder sb, Type type)
        {
            Fx.Assert(type.IsArray, "This type must be an array type.");
            Fx.Assert(type.HasElementType, "This type must have an element type.");

            BuildSimpleTypeSpec(sb, type.GetElementType());

            int arrayRank = type.GetArrayRank();
            Fx.Assert(arrayRank > 0, "number of dimentions of this array must be greater than 0.");

            sb.Append('[');
            for (int i = 1; i < arrayRank; i++)
            {
                sb.Append(',');
            }
            sb.Append(']');
        }

        static void BuildTypeName(StringBuilder sb, Type type)
        {
            BuildNamespaceTypeName(sb, type);
            sb.Append(", ");
            BuildAssemblyNameSpec(sb, type);
        }

        static void BuildNamespaceTypeName(StringBuilder sb, Type type)
        {
            if (!String.IsNullOrEmpty(type.Namespace))
            {
                sb.Append(type.Namespace);
                sb.Append('.');
            }
            BuildNestedTypeName(sb, type);
        }

        static void BuildNestedTypeName(StringBuilder sb, Type type)
        {
            if (type.IsNested)
            {
                BuildNestedTypeName(sb, type.DeclaringType);
                sb.Append('+');
            }

            BuildSimpleName(sb, type);
        }

        static void BuildSimpleName(StringBuilder sb, Type type)
        {
            sb.Append(type.Name);

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                sb.Append('[');
                Type[] genericArguments = type.GetGenericArguments();

                sb.Append('[');
                BuildTypeSpec(sb, genericArguments[0]);
                sb.Append(']');

                for (int i = 1; i < genericArguments.Length; i++)
                {
                    sb.Append(',');
                    sb.Append('[');
                    BuildTypeSpec(sb, genericArguments[i]);
                    sb.Append(']');
                }

                sb.Append(']');
            }
        }

        static void BuildAssemblyNameSpec(StringBuilder sb, Type type)
        {
            // only write assembly simple name,
            // omit version, culture and public token
            AssemblyName tempAssemName = new AssemblyName(type.Assembly.FullName);
            sb.Append(tempAssemName.Name);
        }
    }
}
