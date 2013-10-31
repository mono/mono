//
// MethodSignatureGenerator.cs
//
// Author:
//      Atsushi Enomoto (atsushi@ximian.com)
//      Aakash Apoorv (aakash.apoorv@outlook.com)
//
// Copyright (C) 2007 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime;

namespace System.Data.Design
{
        public class MethodSignatureGenerator
        {
                private static readonly char endOfStatement = ';';
                private CodeDomProvider codeProvider;
                private DbSource methodSource;
                private Type containerParameterType = typeof (DataSet);
                private bool pagingMethod;
                private bool getMethod;
                private ParameterGenerationOption parameterOption;
                private string tableClassName;
                private string datasetClassName;
                private DesignTable designTable;

                public CodeDomProvider CodeProvider {
                        get { return this.codeProvider; }
                        set { this.codeProvider = value; }
                }

                public Type ContainerParameterType {
                        get { return this.containerParameterType; }			
                        set {
                                if (value != typeof (DataSet) && value != typeof (DataTable))
                                        throw new InternalException ("Unsupported container parameter type.");
                                
                                this.containerParameterType = value;
                        }
                }

                public string DataSetClassName {
                        get { return this.datasetClassName; }
                        set { this.datasetClassName = value; }
                }

                public bool IsGetMethod {
                        get { return this.getMethod; }
                        set { this.getMethod = value; }
                }

                public bool PagingMethod {
                        get { return this.pagingMethod; }
                        set { this.pagingMethod = value; }
                }

                public ParameterGenerationOption ParameterOption {
                        get { return this.parameterOption; }
                        set { this.parameterOption = value; }
                }

                public string TableClassName {
                        get { return this.tableClassName; }
                        set { this.tableClassName = value; }
                }

                public CodeMemberMethod GenerateMethod () {

                        if (this.codeProvider == null)
                                throw new ArgumentException ("codeProvider");
                                
                        if (this.methodSource == null)
                                throw new ArgumentException ("MethodSource");
                        
                        QueryGeneratorBase queryGeneratorBase = null;
           	
                        if (this.methodSource.QueryType == QueryType.Rowset && this.methodSource.CommandOperation == CommandOperation.Select) {
                                queryGeneratorBase = new QueryGenerator (null);
                                queryGeneratorBase.ContainerParameterTypeName = this.GetParameterTypeName ();
                                queryGeneratorBase.ContainerParameterName = this.GetParameterName ();
                                queryGeneratorBase.ContainerParameterType = this.containerParameterType;
                        } else {
                                queryGeneratorBase = new FunctionGenerator (null);
                        }
            
                        queryGeneratorBase.DeclarationOnly = true;
                        queryGeneratorBase.CodeProvider = this.codeProvider;
                        queryGeneratorBase.MethodSource = this.methodSource;
                        queryGeneratorBase.MethodName = this.GetMethodName ();
                        queryGeneratorBase.ParameterOption = this.parameterOption;
                        queryGeneratorBase.GeneratePagingMethod = this.pagingMethod;
                        queryGeneratorBase.GenerateGetMethod = this.getMethod;

                        return queryGeneratorBase.Generate ();
                }

                public string GenerateMethodSignature () {

                        if (this.codeProvider == null)
                                throw new ArgumentException ("codeProvider");
        
                        if (this.methodSource == null)
                                throw new ArgumentException ("MethodSource");
                
                        string value = null;
                        CodeTypeDeclaration codeType = this.GenerateMethodWrapper (out value);
                        StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture);
                        this.codeProvider.GenerateCodeFromType (codeType, stringWriter, null);
                        string text = stringWriter.GetStringBuilder().ToString();
                        string[] array = text.Split(Environment.NewLine.ToCharArray());
                        string[] array2 = array;
            
                        for (int i = 0; i < array2.Length; i++) {
                                string text2 = array2[i];
                
                                if (text2.Contains(value))
                                        return text2.Trim().TrimEnd( new char[] { MethodSignatureGenerator.endOfStatement });
                        
                                return null;
                        }
                }

                public CodeTypeDeclaration GenerateUpdatingMethods () {

                        if (this.designTable == null)
                                throw new InternalException ("DesignTable should not be null.");
                        
                        if (StringUtil.Empty (this.datasetClassName))
                                throw new InternalException ("DatasetClassName should not be empty.");
                        
                        CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration ("wrapper");
                        codeTypeDeclaration.IsInterface = true;
                        new QueryHandler (this.codeProvider, this.designTable) {
                                DeclarationsOnly = true
                                }.AddUpdateQueriesToDataComponent (codeTypeDeclaration, this.datasetClassName, this.codeProvider);
                                                
                        return codeTypeDeclaration;
                }

                public void SetDesignTableContent (string designTableContent) {

                        DesignDataSource designDataSource = new DesignDataSource();
                        StringReader textReader = new StringReader (designTableContent);
                        designDataSource.ReadXmlSchema (textReader, null);

                        if (designDataSource.DesignTables == null || designDataSource.DesignTables.Count != 1)
                                throw new InternalException ("Unexpected number of sources in deserialized DataSource.");
                        
                        IEnumerator enumerator = designDataSource.DesignTables.GetEnumerator ();
                        enumerator.MoveNext ();
                        this.designTable = (DesignTable) enumerator.Current;
                }

                public void SetMethodSourceContent (string methodSourceContent) {

                        DesignDataSource designDataSource = new DesignDataSource ();
                        StringReader textReader = new StringReader (methodSourceContent);
                        designDataSource.ReadXmlSchema (textReader, null);

                        if (designDataSource.Sources == null || designDataSource.Sources.Count != 1)
                                throw new InternalException ("Unexpected number of sources in deserialized DataSource.");

                        IEnumerator enumerator = designDataSource.Sources.GetEnumerator ();
                        enumerator.MoveNext ();
                        this.methodSource = (DbSource) enumerator.Current;
                }

                private CodeTypeDeclaration GenerateMethodWrapper (out string methodName) {

                        CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration ("Wrapper");
                        codeTypeDeclaration.IsInterface = true;
                        CodeMemberMethod codeMemberMethod = this.GenerateMethod();
                        codeTypeDeclaration.Members.Add (codeMemberMethod);
                        methodName = codeMemberMethod.Name;

                        return codeTypeDeclaration;
                }

                private string GetParameterName () {

                        if (this.containerParameterType == typeof (DataTable))
                                return "dataTable";

                        return "dataSet";
                }

                private string GetParameterTypeName () {

                        if (StringUtil.Empty (this.datasetClassName))
                                throw new InternalException ("DatasetClassName should not be empty.");

                        if (!(this.containerParameterType == typeof (DataTable)))
                                return this.datasetClassName;

                        if (StringUtil.Empty (this.tableClassName))
                                throw new InternalException ("TableClassName should not be empty.");

                        return CodeGenHelper.GetTypeName (this.codeProvider, this.datasetClassName, this.tableClassName);
                }

                private string GetMethodName () {

                        if (this.methodSource.QueryType == QueryType.Rowset) {
                                if (this.getMethod) {
                                        if (this.pagingMethod) {
                                                if (this.methodSource.GeneratorGetMethodNameForPaging != null) 
                                                        return this.methodSource.GeneratorGetMethodNameForPaging;

                                                return this.methodSource.GetMethodName + DataComponentNameHandler.PagingMethodSuffix;
                                        } else {
                                                if (this.methodSource.GeneratorGetMethodName != null) 
                                                        return this.methodSource.GeneratorGetMethodName;

                                                return this.methodSource.GetMethodName;
                                        }
                                } else {
                                        if (this.pagingMethod) {
                                                if (this.methodSource.GeneratorSourceNameForPaging != null)
                                                        return this.methodSource.GeneratorSourceNameForPaging;

                                                return this.methodSource.Name + DataComponentNameHandler.PagingMethodSuffix;
                                        } else {
                                                if (this.methodSource.GeneratorSourceName != null)
                                                        return this.methodSource.GeneratorSourceName;

                                                return this.methodSource.Name;
                                        }
                                }
                        } else {
                                if (this.methodSource.GeneratorSourceName != null)
                                        return this.methodSource.GeneratorSourceName;
                                
                                return this.methodSource.Name;
                        }
                }
        }
}
