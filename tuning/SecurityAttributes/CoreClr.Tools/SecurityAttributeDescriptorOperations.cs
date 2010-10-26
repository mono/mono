using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreClr.Tools
{
	public static class SecurityAttributeDescriptorOperations
	{
		/// <summary>
		/// SSC U SC = SSC
		/// +SC U anything = SC
		/// +SSC U anything = SSC
		/// </summary>
		/// <param name="descriptors"></param>
		/// <returns></returns>
		public static IEnumerable<SecurityAttributeDescriptor> Normalize(this IEnumerable<SecurityAttributeDescriptor> descriptors)
		{
			return descriptors.GroupBy(d => d.Signature).SelectMany(g => Reduce(g));
		}

		public static IEnumerable<SecurityAttributeDescriptor> Merge(this IEnumerable<SecurityAttributeDescriptor> self, IEnumerable<SecurityAttributeDescriptor> other)
		{
			return self.Concat(other).Normalize();
		}

		private static IEnumerable<SecurityAttributeDescriptor> Reduce(IGrouping<string, SecurityAttributeDescriptor> g)
		{
			var overrides = g.Where(d => d.Override != SecurityAttributeOverride.None).ToList();
			switch (overrides.Count)
			{
				case 0:
					yield return g.OrderByDescending(d => d.AttributeType).First();
					break;

				case 1:
					var @override = overrides[0];
					if (@override.Override == SecurityAttributeOverride.Add)
						yield return new SecurityAttributeDescriptor(@override.AttributeType, @override.Target, @override.Signature);
					break;

				default:
					throw new ArgumentException(string.Format("Conflicting overrides: {0}", overrides));
			}

		}
	}
}

