//---------------------------------------------------------------------
// <copyright file="NamespaceEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Data;
using Som = System.Data.EntityModel.SchemaObjectModel;
using System.Collections.Generic;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Data.Entity.Design.SsdlGenerator;
using System.Data.Entity.Design.Common;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// This class is responsible for Emitting the code to create the CLR namespace container and assembly level attributes
    /// </summary>
    internal sealed class NamespaceEmitter : Emitter
    {
        #region Static Fields
        private static Pair<Type, CreateEmitter>[] EmitterCreators = new Pair<Type, CreateEmitter>[]
        {
            new Pair<Type,CreateEmitter>(typeof(EntityType), delegate (ClientApiGenerator generator, GlobalItem element) { return new EntityTypeEmitter(generator,(EntityType)element); }),
            new Pair<Type,CreateEmitter>(typeof(ComplexType), delegate (ClientApiGenerator generator, GlobalItem element) { return new ComplexTypeEmitter(generator,(ComplexType)element); }),
            new Pair<Type,CreateEmitter>(typeof(EntityContainer), delegate (ClientApiGenerator generator, GlobalItem element) { return new EntityContainerEmitter(generator,(EntityContainer)element); }),            
            new Pair<Type,CreateEmitter>(typeof(AssociationType), delegate (ClientApiGenerator generator, GlobalItem element) { return new AssociationTypeEmitter(generator,(AssociationType)element); }),            
        };        
        #endregion

        #region Private Fields

        private string _codeNamespace = null;
        private string _targetFilePath = null;
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator"></param>
        public NamespaceEmitter(ClientApiGenerator generator, string codeNamespace, string targetFilePath)
            : base(generator)
        {
            _codeNamespace = codeNamespace;
            _targetFilePath = targetFilePath != null ? targetFilePath : string.Empty;
        }

        /// <summary>
        /// Creates the CodeTypeDeclarations necessary to generate the code
        /// </summary>
        public void Emit()
        {
            // it is a valid scenario for namespaceName to be empty
            string namespaceName = Generator.SourceObjectNamespaceName;

            // emit the namespace definition
            CodeNamespace codeNamespace = new CodeNamespace( namespaceName );

            // output some boiler plate comments
            string comments = Strings.NamespaceComments(
                System.IO.Path.GetFileName( _targetFilePath ),
                DateTime.Now.ToString( System.Globalization.CultureInfo.CurrentCulture ));
            CommentEmitter.EmitComments( CommentEmitter.GetFormattedLines( comments, false ), codeNamespace.Comments, false );
            CompileUnit.Namespaces.Add( codeNamespace );

            // Add the assembly attribute.
            CodeAttributeDeclaration assemblyAttribute;
            // SQLBUDT 505339: VB compiler fails if multiple assembly attributes exist in the same project.
            // This adds a GUID to the assembly attribute so that each generated file will have a unique EdmSchemaAttribute in VB.
            if (this.Generator.Language == System.Data.Entity.Design.LanguageOption.GenerateVBCode) //The GUID is only added in VB
            {
                assemblyAttribute = AttributeEmitter.EmitSimpleAttribute("System.Data.Objects.DataClasses.EdmSchemaAttribute", System.Guid.NewGuid().ToString());
            }
            else
            {
                assemblyAttribute = AttributeEmitter.EmitSimpleAttribute("System.Data.Objects.DataClasses.EdmSchemaAttribute");
            }
            CompileUnit.AssemblyCustomAttributes.Add(assemblyAttribute);

            Dictionary<string, string> usedClassName = new Dictionary<string, string>(StringComparer.Ordinal);
            // Emit the classes in the schema
            foreach (GlobalItem element in Generator.GetSourceTypes())
            {
                Debug.Assert(!(element is EdmFunction), "Are you trying to emit functions now? If so add an emitter for it.");

                if (AddElementNameToCache(element, usedClassName))
                {
                    SchemaTypeEmitter emitter = CreateElementEmitter(element);
                    CodeTypeDeclarationCollection typeDeclaration = emitter.EmitApiClass();
                    if (typeDeclaration.Count > 0)
                    {
                        codeNamespace.Types.AddRange(typeDeclaration);
                    }
                }
            }
        }

        #endregion


        #region Private Properties
        /// <summary>
        /// Gets the compile unit (top level codedom object)
        /// </summary>
        /// <value></value>
        private CodeCompileUnit CompileUnit
        {
            get
            {
                return Generator.CompileUnit;
            }
        }        
        #endregion

        private bool AddElementNameToCache(GlobalItem element, Dictionary<string, string> cache)
        {
            if (element.BuiltInTypeKind == BuiltInTypeKind.EntityContainer)
            {
                return TryAddNameToCache((element as EntityContainer).Name, element.BuiltInTypeKind.ToString(), cache);
            }
            else if (element.BuiltInTypeKind == BuiltInTypeKind.EntityType ||
                element.BuiltInTypeKind == BuiltInTypeKind.ComplexType ||
                element.BuiltInTypeKind == BuiltInTypeKind.AssociationType)
            {
                return TryAddNameToCache((element as StructuralType).Name, element.BuiltInTypeKind.ToString(), cache);
            }
            return true;
        }

        private bool TryAddNameToCache(string name, string type, Dictionary<string, string> cache)
        {
            if (!cache.ContainsKey(name))
            {
                cache.Add(name, type);
            }
            else
            {
                this.Generator.AddError(Strings.DuplicateClassName(type, name, cache[name]), ModelBuilderErrorCode.DuplicateClassName, 
                    EdmSchemaErrorSeverity.Error, name);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create an Emitter for a schema type element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private SchemaTypeEmitter CreateElementEmitter( GlobalItem element )
        {
            Type typeOfElement = element.GetType();
            foreach ( Pair<Type, CreateEmitter> pair in EmitterCreators )
            {
                if ( pair.First.IsAssignableFrom( typeOfElement ) )
                    return pair.Second( Generator, element );
            }
            return null;
        }

        private delegate SchemaTypeEmitter CreateEmitter( ClientApiGenerator generator, GlobalItem item );

        /// <summary>
        /// Reponsible for relating two objects together into a pair
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        private class Pair<T1, T2>
        {
            public T1 First;
            public T2 Second;
            internal Pair( T1 first, T2 second )
            {
                First = first;
                Second = second;
            }
        }
    }
}
