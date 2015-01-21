
using System.Reflection;
using System.Security;
using System.Runtime.Versioning;

namespace System {

	public partial class AppDomain
	{
		internal String GetTargetFrameworkName()
		{
			return ".NETFramework,Version=v4.5";
		}
	}
}
