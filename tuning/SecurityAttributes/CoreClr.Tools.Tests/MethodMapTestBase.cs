using Mono.Cecil;
using NUnit.Framework;

namespace CoreClr.Tools.Tests
{
    public class MethodMapTestBase
    {
        protected CecilDefinitionFinder _finder;
        protected MethodMap _subject;

        [SetUp]
        public void SetUp()
        {
            var executingAssemblyDefinition = CecilUtilsForTests.GetExecutingAssemblyDefinition();
            _finder = new CecilDefinitionFinder(executingAssemblyDefinition);
            _subject = new MethodMap(_finder.FindType(GetType().FullName));
        }

        protected MethodDefinition MethodDefinitionOf<T>(string methodName)
        {
            return _finder.FindMethod(string.Format("System.Void {0}::{1}()", typeof(T).CecilTypeName(), methodName));
        }
    }
}