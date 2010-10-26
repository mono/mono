using Mono.Cecil;

namespace CoreClr.Tools
{
	static class MethodDefinitionComparator
	{
		public static bool Compare(MethodDefinition m1, MethodDefinition m2)
		{
			if (m1.Name != m2.Name)
				return false;

			if (m1.Parameters.Count != m2.Parameters.Count)
				return false;

			for (int i = 0; i < m1.Parameters.Count; i++)
			{
				ParameterDefinition p1 = m1.Parameters[i];
				ParameterDefinition p2 = m2.Parameters[i];
				if (p1.ParameterType.FullName != p2.ParameterType.FullName)
					return false;
			}
			return (m1.ReturnType.ReturnType.FullName == m2.ReturnType.ReturnType.FullName);
		}
	}
}
