// This is a revised source code from CodeDom.

//------------------------------------------------------------------------------
// <copyright file="CodeTypeReference.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Runtime.Serialization {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Globalization;
    
    enum CodeTypeReferenceOptions {
        GlobalReference =  0x00000001,
        GenericTypeParameter = 0x00000002
    }

    class CodeTypeReference {
        private string baseType;
        [OptionalField] 
        private bool isInterface;
        private int arrayRank;
        private CodeTypeReference arrayElementType;
        [OptionalField] 
        private List<CodeTypeReference> typeArguments;
        [OptionalField]
        private CodeTypeReferenceOptions referenceOptions;
        [OptionalField]
        private bool needsFixup = false;
        
        public CodeTypeReference() {
            baseType = string.Empty;
            this.arrayRank = 0;
            this.arrayElementType = null;            
        }

        public CodeTypeReference(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");
            
            if (type.IsArray) {
                this.arrayRank = type.GetArrayRank();
                this.arrayElementType = new CodeTypeReference(type.GetElementType());
                this.baseType = null;
            } else {
                InitializeFromType(type);
                this.arrayRank = 0;
                this.arrayElementType = null;
            }

            this.isInterface = type.IsInterface;
        }

        public CodeTypeReference (Type type, CodeTypeReferenceOptions codeTypeReferenceOption) : this(type) {
            referenceOptions = codeTypeReferenceOption;
        }
        
        public CodeTypeReference (String typeName, CodeTypeReferenceOptions codeTypeReferenceOption) {
            Initialize(typeName, codeTypeReferenceOption);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        // We support the reflection format for generice type name.
        // The format is like:
        //
        public CodeTypeReference(string typeName) {
            Initialize(typeName);
        }

        private void InitializeFromType(Type type) {
            baseType = type.Name;
            if (!type.IsGenericParameter) {
                Type currentType = type;
                while (currentType.IsNested) {
                    currentType = currentType.DeclaringType;
                    baseType = currentType.Name + "+" + baseType;
                }
                if (!String.IsNullOrEmpty(type.Namespace))
                    baseType = type.Namespace + "." + baseType;
            }

            // pick up the type arguments from an instantiated generic type but not an open one    
            if (type.IsGenericType && !type.ContainsGenericParameters) {
                Type[] genericArgs = type.GetGenericArguments();
                for (int i = 0; i < genericArgs.Length; i++) {
                    TypeArguments.Add(new CodeTypeReference(genericArgs[i]));
                }
            }
            else if (!type.IsGenericTypeDefinition) 
            {
                // if the user handed us a non-generic type, but later
                // appends generic type arguments, we'll pretend
                // it's a generic type for their sake - this is good for
                // them if they pass in System.Nullable class when they
                // meant the System.Nullable<T> value type.
                needsFixup = true;
            }
        }

        private void Initialize(string typeName) {
            Initialize(typeName, this.referenceOptions);
        }

        private void Initialize(string typeName, CodeTypeReferenceOptions options)
        {
            Options = options;
            if (typeName == null || typeName.Length == 0) {
                typeName = typeof(void).FullName;            
                this.baseType = typeName;
                this.arrayRank = 0;
                this.arrayElementType = null;
                return;                
            }

            typeName = RipOffAssemblyInformationFromTypeName(typeName);
            
            int end = typeName.Length -1;
            int current = end;
            needsFixup = true;      // default to true, and if we find arity or generic type args, we'll clear the flag.
            
            // Scan the entire string for valid array tails and store ranks for array tails
            // we found in a queue.
            Queue q = new Queue();
            while(current >= 0) {
                int rank = 1;
                if( typeName[current--] == ']') {
                    while(current >=0 && typeName[current] == ',') {
                        rank++;
                        current--;
                    }

                    if( current>=0 && typeName[current] == '[') { // found a valid array tail
                        q.Enqueue(rank); 
                        current--;   
                        end = current; 
                        continue;
                    }
                }
                break;
            }
            
            // Try find generic type arguments
            current = end;
            ArrayList typeArgumentList = new ArrayList();
            Stack subTypeNames = new Stack();
            if( current > 0 && typeName[current--] == ']') {
                needsFixup = false;
                int unmatchedRightBrackets = 1;
                int subTypeNameEndIndex = end;
                
                // Try find the matching '[', if we can't find it, we will not try to parse the string
                while(current >= 0) {
                    if( typeName[current] == '[' ) {      
                        // break if we found matched brackets
                        if( --unmatchedRightBrackets == 0) break;
                    }
                    else if( typeName[current] == ']' ) {                   
                        ++unmatchedRightBrackets;
                    }
                    else if( typeName[current] == ',' && unmatchedRightBrackets == 1) {
                        //
                        // Type name can contain nested generic types. Following is an example:
                        // System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089], 
                        //          [System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], 
                        //           mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
                        // 
                        // Spliltting by ',' won't work. We need to do first-level split by ','. 
                        //
                        if( current + 1 < subTypeNameEndIndex) {
                            subTypeNames.Push(typeName.Substring(current+1 , subTypeNameEndIndex - current - 1));                            
                        }

                        subTypeNameEndIndex = current;
                        
                    }    
                    --current;
                }
            
                if( current > 0 && (end - current - 1) > 0) { 
                    // push the last generic type argument name if there is any
                    if( current + 1 < subTypeNameEndIndex) {
                        subTypeNames.Push(typeName.Substring(current+1 , subTypeNameEndIndex - current - 1));                                                    
                    }
                        
                    // we found matched brackets and the brackets contains some characters.                    
                    while( subTypeNames.Count > 0) {
                        String name = RipOffAssemblyInformationFromTypeName((string)subTypeNames.Pop());                         
                        typeArgumentList.Add(new CodeTypeReference(name));
                    }
                    end = current - 1;
                }
            }

            if( end < 0) {  // this can happen if we have some string like "[...]"
                this.baseType = typeName;
                return;
            }

            if (q.Count > 0 ) {             
                
                CodeTypeReference type = new CodeTypeReference(typeName.Substring(0, end + 1), Options);

                for(int i = 0; i < typeArgumentList.Count; i++) {
                    type.TypeArguments.Add((CodeTypeReference)typeArgumentList[i]);
                }

                while( q.Count > 1) {
                    type = new CodeTypeReference( type, (int)q.Dequeue());  
                } 
                
                // we don't need to create a new CodeTypeReference for the last one.
                Debug.Assert(q.Count == 1 , "We should have one and only one in the rank queue.");                                
                this.baseType = null;
                this.arrayRank = (int)q.Dequeue();
                this.arrayElementType = type;
            }
            else if( typeArgumentList.Count > 0 ) {
                for( int i = 0; i < typeArgumentList.Count; i++) {
                    TypeArguments.Add((CodeTypeReference)typeArgumentList[i]);
                }

                this.baseType = typeName.Substring(0, end + 1);
            }
            else{
                this.baseType = typeName;
            }

            // Now see if we have some arity.  baseType could be null if this is an array type. 
            if (baseType != null && baseType.IndexOf('`') != -1)
                needsFixup = false;
            
        }

        public CodeTypeReference(string typeName, params CodeTypeReference[] typeArguments) : this(typeName){
            if( typeArguments != null && typeArguments.Length > 0) {
                TypeArguments.AddRange(typeArguments);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeTypeReference(string baseType, int rank) {
            this.baseType = null;
            this.arrayRank = rank;
            this.arrayElementType = new CodeTypeReference(baseType);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeTypeReference(CodeTypeReference arrayType, int rank) {
            this.baseType = null;
            this.arrayRank = rank;
            this.arrayElementType = arrayType;
        }
 
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public CodeTypeReference ArrayElementType {
            get {
                return arrayElementType;
            }
            set {
                arrayElementType = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int ArrayRank {
            get {
                return arrayRank;
            }
            set {
                arrayRank = value;
            }
        }

        internal int NestedArrayDepth {
            get {
                
                if (arrayElementType == null)
                    return 0;

                return 1 + arrayElementType.NestedArrayDepth;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string BaseType {
            get {
                if (arrayRank > 0 && arrayElementType != null) {
                    return arrayElementType.BaseType;
                }
                if (String.IsNullOrEmpty(baseType)) 
                    return string.Empty;
                
                string returnType = baseType;
                if (needsFixup && TypeArguments.Count > 0)
                    returnType = returnType + '`' + TypeArguments.Count.ToString(CultureInfo.InvariantCulture);

                return returnType;
                
            }
            set {
                baseType = value;
                Initialize(baseType);
            }
        }

        [System.Runtime.InteropServices.ComVisible(false)]
        public CodeTypeReferenceOptions Options {
            get { return referenceOptions;}
            set { referenceOptions = value;}            
        }

        [System.Runtime.InteropServices.ComVisible(false)]        
        public List<CodeTypeReference> TypeArguments{ 
            get {
                if (arrayRank > 0 && arrayElementType != null) {
                    return arrayElementType.TypeArguments;
                }

                if( typeArguments == null) {
                    typeArguments = new List<CodeTypeReference>();
                }
                return typeArguments;
            }
        }

        internal bool IsInterface {
            get {
                // Note that this only works correctly if the Type ctor was used. Otherwise, it's always false.
                return this.isInterface;
            }
        }

        //
        // The string for generic type argument might contain assembly information and square bracket pair.
        // There might be leading spaces in front the type name.
        // Following function will rip off assembly information and brackets 
        // Following is an example:
        // " [System.Collections.Generic.List[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral,
        //   PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"
        //
        private string RipOffAssemblyInformationFromTypeName(string typeName) {
            int start = 0;
            int end = typeName.Length - 1;
            string result = typeName;
            
            // skip white space in the beginning
            while( start < typeName.Length && Char.IsWhiteSpace(typeName[start])) start++;
            while( end >= 0 && Char.IsWhiteSpace(typeName[end])) end--;
                    
            if(start < end) {
                if (typeName[start] =='[' && typeName[end] == ']') {  
                    start++;
                    end--;
                }

                // if we still have a ] at the end, there's no assembly info. 
                if (typeName[end] != ']') {
                    int commaCount = 0;                            
                    for(int index = end; index >= start; index--) {
                        if( typeName[index] == ',') {
                            commaCount++;
                            if( commaCount == 4) {
                                result = typeName.Substring( start, index - start); 
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
    

