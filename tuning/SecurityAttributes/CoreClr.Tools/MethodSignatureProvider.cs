using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace CoreClr.Tools
{
	public static class MethodSignatureProvider
	{
		private static Dictionary<MethodReference, string> cache = new Dictionary<MethodReference,string>();

		public static string SignatureFor(MethodReference method)
		{
			if (cache.ContainsKey(method)) return cache[method];

			var sb = new StringBuilder();
			sb.Append(method.ReturnType.ReturnType.FullName);
			sb.Append(" ");
			sb.Append(method.DeclaringType.FullName);
			sb.Append("::");
			sb.Append(method.Name);
			if (method.HasGenericParameters)
			{
				sb.Append("<");
				for (int i = 0; i < method.GenericParameters.Count; i++)
				{
					if (i > 0)
						sb.Append(",");
					sb.Append(method.GenericParameters[i].Name);
				}
				sb.Append(">");
			}
			sb.Append("(");
			if (method.HasParameters)
			{
				int sentinel = method.GetSentinel();
				for (int i = 0; i < method.Parameters.Count; i++)
				{
					if (i > 0)
						sb.Append(",");

					if (i == sentinel)
						sb.Append("...,");

					sb.Append(method.Parameters[i].ParameterType.FullName);
				}
			}
			sb.Append(")");
			var result= sb.ToString();
			cache[method] = result;
			return result;
		}
	}
}

