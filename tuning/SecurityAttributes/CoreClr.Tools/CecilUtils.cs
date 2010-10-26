using System.Reflection;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public static class CecilUtils
	{
	    public static string AssemblyPath(this AssemblyDefinition assembly)
	    {
	        return assembly.MainModule.Image.FileInformation.FullName;
	    }
	}
}
