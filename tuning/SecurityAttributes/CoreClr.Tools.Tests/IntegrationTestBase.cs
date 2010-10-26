using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace CoreClr.Tools.Tests
{
    public class IntegrationTestBase
    {
        protected static MethodPrivilegePropagationStringReport PropagateRequiredPrivileges(ICollection<TypeDefinition> criticalTypes, string[] canBeSscManual, string[] methodsRequiringPrivilegesThemselves, params AssemblyDefinition[] assemblies)
        {
            return new MethodPrivilegePropagation(assemblies, ResolveAllMethodDefinitions(assemblies, methodsRequiringPrivilegesThemselves), ResolveAllMethodDefinitions(assemblies, canBeSscManual), criticalTypes, new List<MethodToMethodCall>())
                .CreateReportBuilder()
                .BuildStringReport();
        }

        private static IEnumerable<MethodDefinition> ResolveAllMethodDefinitions(IEnumerable<AssemblyDefinition> assemblies, IEnumerable<string> methodsInAssemblies)
        {
            return assemblies
                .Zip(methodsInAssemblies, (assembly, methods) => new { assembly, methods })
                .SelectMany(row => new CecilDefinitionFinder(row.assembly).FindMethods(ParseSignatureList(row.methods)));
        }

        private static IEnumerable<string> ParseSignatureList(string signatures)
        {
            return signatures.NonEmptyLines().Where(s => !s.StartsWith("#"));
        }

        protected static List<string> DetectMethodsRequiringPrivileges(IEnumerable<TypeDefinition> criticalTypes, params AssemblyDefinition[] assemblies)
        {
            MethodPrivilegeDetector.CriticalTypes = criticalTypes.Cast<TypeReference>().ToList();
            return assemblies.Select(assembly => MethodPrivilegeDetector.ReportOfMethodsRequiringPrivilegesThemselves(assembly)).ToList();
        }
    }
}