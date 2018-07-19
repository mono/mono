//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Activation
{
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel.Activation;
    using System.Web;
    using System.Web.Compilation;

    sealed class XamlBuildProviderExtension : System.Xaml.Hosting.IXamlBuildProviderExtension
    {
        const string GeneratedNamespace = "GeneratedNamespace";
        const string ExpressionRootFactorySuffix = "_ExpressionRootFactory";
        const string CreateExpressionRootMethodName = "CreateExpressionRoot";
        const string ActivityParameterName = "activity";

        string generatedPrimaryTypeName;        

        internal XamlBuildProviderExtension()
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
        }

        public void GenerateCode(AssemblyBuilder assemblyBuilder, Stream xamlStream, BuildProvider buildProvider)
        {
            object serviceObject = WorkflowServiceHostFactory.LoadXaml(xamlStream);

            WorkflowService workflowService = serviceObject as WorkflowService;
            if (workflowService != null && workflowService.Body != null)
            {
                string activityName;
                if (this.TryGenerateSource(assemblyBuilder, buildProvider, workflowService, false, null, out activityName))
                {
                    this.generatedPrimaryTypeName = GeneratedNamespace + "." + activityName + ExpressionRootFactorySuffix;
                }

                // find all supported versions xamlx files, load and compile them
                IList<Tuple<string, Stream>> streams = null;

                string xamlVirtualFile = GetXamlVirtualPath(buildProvider);
                string xamlFileName = Path.GetFileNameWithoutExtension(VirtualPathUtility.GetFileName(xamlVirtualFile));
                WorkflowServiceHostFactory.GetSupportedVersionStreams(xamlFileName, out streams);
                if (streams != null)
                {
                    try
                    {
                        foreach (Tuple<string, Stream> stream in streams)
                        {
                            try
                            {
                                WorkflowService service = WorkflowServiceHostFactory.CreatetWorkflowService(stream.Item2, workflowService.Name);
                                if (service != null && service.Body != null)
                                {
                                    this.TryGenerateSource(assemblyBuilder, buildProvider, service, true, stream.Item1, out activityName);
                                }
                            }
                            catch (Exception e)
                            {
                                Exception newException;
                                if (Fx.IsFatal(e) || !WorkflowServiceHostFactory.TryWrapSupportedVersionException(stream.Item1, e, out newException))
                                {
                                    throw;
                                }

                                throw FxTrace.Exception.AsError(newException);
                            }
                        }
                    }
                    finally
                    {
                        foreach (Tuple<string, Stream> stream in streams)
                        {
                            stream.Item2.Dispose();
                        }
                    }
                }
            }            
        }        

        public Type GetGeneratedType(CompilerResults results)
        {
            if (this.generatedPrimaryTypeName != null)
            {
                return results.CompiledAssembly.GetType(this.generatedPrimaryTypeName);
            }

            return null;
        }

        // if the 1st parameter "supportedVersionXamlxfilePath" is null, we fall back to the primary generated type
        internal static ICompiledExpressionRoot GetExpressionRoot(string supportedVersionXamlxfilePath, WorkflowService service, string virtualPath)
        {
            Assembly compiledAssembly = BuildManager.GetCompiledAssembly(virtualPath);
            if (compiledAssembly == null)
            {
                return null;
            }

            Type generatedType;
            if (supportedVersionXamlxfilePath != null)
            {
                string fullTypeNameToSearch = GeneratedNamespace + "." + WorkflowServiceHostFactory.GetSupportedVersionGeneratedTypeName(supportedVersionXamlxfilePath) + ExpressionRootFactorySuffix;
                generatedType = compiledAssembly.GetType(fullTypeNameToSearch);
            }
            else
            {
                generatedType = BuildManager.GetCompiledType(virtualPath);
            }
             
            Type workflowServiceType = typeof(WorkflowService);
            if (generatedType != workflowServiceType && workflowServiceType.IsAssignableFrom(generatedType))
            {
                MethodInfo createExpressionRootMethod = generatedType.GetMethod(CreateExpressionRootMethodName, BindingFlags.Public | BindingFlags.Static);
                if (createExpressionRootMethod != null)
                {
                    return (ICompiledExpressionRoot)createExpressionRootMethod.Invoke(null, new object[] { service.Body });
                }
            }
            
            return null;
        }

        bool TryGenerateSource(AssemblyBuilder assemblyBuilder, BuildProvider buildProvider, WorkflowService workflowService, bool isSupportedVersion, string filePath, out string activityName)
        {
            bool generatedSource;
            string codeFileName;
            this.GenerateSource(isSupportedVersion, filePath, assemblyBuilder, workflowService, out codeFileName, out generatedSource, out activityName);

            if (generatedSource)
            {
                this.WriteCodeFile(assemblyBuilder, buildProvider, codeFileName);
                this.GenerateExpressionRootFactory(assemblyBuilder, buildProvider, GeneratedNamespace, activityName);
            }

            return generatedSource;
        }
        
        void GenerateExpressionRootFactory(AssemblyBuilder assemblyBuilder, BuildProvider buildProvider, string activityNamespace, string activityName)
        {
            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();

            // namespace <%= activityNamespace %>
            // {
            //     public class <%= activityName %>_ExpressionRootFactory : System.ServiceModel.Activities.WorkflowService
            //     {
            //         public static System.Activities.XamlIntegration.ICompiledExpressionRoot CreateExpressionRoot(System.Activities.Activity activity)
            //         {
            //             return new <%= activityNamespace %>.<%= activityName %>(activity);
            //         }
            //     }
            // }
            CodeNamespace codeNamespace = new CodeNamespace(activityNamespace);
            CodeTypeDeclaration codeTypeDeclaration = new CodeTypeDeclaration(activityName + ExpressionRootFactorySuffix);
            codeTypeDeclaration.BaseTypes.Add(new CodeTypeReference(typeof(WorkflowService)));
                        
            CodeMemberMethod codeMemberMethod = new CodeMemberMethod();
            codeMemberMethod.Name = CreateExpressionRootMethodName;
            codeMemberMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            codeMemberMethod.ReturnType = new CodeTypeReference(typeof(ICompiledExpressionRoot));
            codeMemberMethod.Parameters.Add(new CodeParameterDeclarationExpression(
                    new CodeTypeReference(typeof(Activity)),
                    ActivityParameterName));

            CodeTypeReference typeRef = new CodeTypeReference(activityNamespace + "." + activityName);
            CodeObjectCreateExpression expr = new CodeObjectCreateExpression(typeRef, new CodeArgumentReferenceExpression(ActivityParameterName));
            codeMemberMethod.Statements.Add(new CodeMethodReturnStatement(expr));

            codeTypeDeclaration.Members.Add(codeMemberMethod);
            codeNamespace.Types.Add(codeTypeDeclaration);
            codeCompileUnit.Namespaces.Add(codeNamespace);

            assemblyBuilder.AddCodeCompileUnit(buildProvider, codeCompileUnit);
        }
        
        void GenerateSource(bool isSupportedVersion, string filePath, AssemblyBuilder assemblyBuilder, WorkflowService workflowService, out string codeFileName, out bool generatedSource, out string activityName)
        {
            // Get unique file and type name for the workflowservice
            codeFileName = assemblyBuilder.GetTempFilePhysicalPath(assemblyBuilder.CodeDomProvider.FileExtension);

            if (isSupportedVersion)
            {
                activityName = WorkflowServiceHostFactory.GetSupportedVersionGeneratedTypeName(filePath);
            }
            else
            {
                activityName = workflowService.Name.LocalName + "_" + Guid.NewGuid().ToString().Replace("-", "_");
            }            

            TextExpressionCompilerSettings settings = new TextExpressionCompilerSettings
            {
                Activity = workflowService.Body,
                ActivityName = activityName,
                ActivityNamespace = GeneratedNamespace,
                Language = CodeDomProvider.GetLanguageFromExtension(assemblyBuilder.CodeDomProvider.FileExtension),
                GenerateAsPartialClass = false,
                AlwaysGenerateSource = false,
                ForImplementation = false
            };

            TextExpressionCompiler compiler = new TextExpressionCompiler(settings);            

            generatedSource = false;
            using (StreamWriter fileStream = new StreamWriter(codeFileName))
            {
                try
                {
                    generatedSource = compiler.GenerateSource(fileStream);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    throw FxTrace.Exception.AsError(new HttpCompileException(SR.XamlBuildProviderExtensionException(ex.Message)));
                }
            }
        }             

        void WriteCodeFile(AssemblyBuilder assemblyBuilder, BuildProvider buildProvider, string name)
        {
            using (TextWriter codeFile = assemblyBuilder.CreateCodeFile(buildProvider))
            {
                foreach (string line in File.ReadLines(name))
                {
                    codeFile.WriteLine(line);
                }
            }
        }

        // XamlBuildProvider is defined in a non-APTCA assembly and this aptca method calls it. This allows partially trusted callers to indirectly
        // call the XamlBuildProvider.VirtualPath property (that requires full trust to call directly). This is safe because this private method 
        // does not directly or indirectly allows users to access sensitive information, operations, or resources that can be used in a destructive manner.
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "The XamlBuildProvider.VirtualPath non-aptca property cannot be used in a destructive manner.")]
        private string GetXamlVirtualPath(BuildProvider buildProvider)
        {
            return ((System.Xaml.Hosting.XamlBuildProvider)buildProvider).VirtualPath;
        }
    }
}
