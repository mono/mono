using System;
using System.Collections.Generic;
using System.IO;

namespace CoreClr.Tools
{
	public class SecurityAttributeDescriptorParser
	{
		public static IEnumerable<SecurityAttributeDescriptor> ParseString(string contents)
		{
			return new SecurityAttributeDescriptorParser(new StringReader(contents)).Parse();
		}

		readonly TextReader _input;

		public SecurityAttributeDescriptorParser(TextReader input)
		{
			_input = input;
		}

		public IEnumerable<SecurityAttributeDescriptor> Parse()
		{
			string line;
			while (null != (line = _input.ReadLine()))
			{
				var descriptor = ParseLine(line);
				if (descriptor == null)
					continue;
				yield return descriptor;
			}
		}

		public static SecurityAttributeDescriptor ParseLine(string line)
		{
			line = line.Trim();
			if (line.Length == 0 || line.StartsWith("#"))
				return null;

			var index = line.IndexOf(':');
			var marker = line.Substring(0, index);
			var @override = ParseOptionalOverrideMarker(marker[0]);
			var signature = line.Substring(index + 1).Trim();
			var attributeTypeMarker = @override == SecurityAttributeOverride.None ? marker : marker.Substring(1);
			switch (attributeTypeMarker)
			{
				case "SC-M":
					return new SecurityAttributeDescriptor(@override, SecurityAttributeType.Critical, TargetKind.Method, signature);
				case "SSC-M":
					return new SecurityAttributeDescriptor(@override, SecurityAttributeType.SafeCritical, TargetKind.Method, signature);
				case "SC-T":
					return new SecurityAttributeDescriptor(@override, SecurityAttributeType.Critical, TargetKind.Type, signature);
				default:
					throw new ArgumentException(string.Format("Unrecognized line: '{0}'", line));
			}
		}

		private static SecurityAttributeOverride ParseOptionalOverrideMarker(char c)
		{
			switch (c)
			{
				case '+':
					return SecurityAttributeOverride.Add;
				case '-':
					return SecurityAttributeOverride.Remove;
				case '!':
					return SecurityAttributeOverride.Force;
				default:
					return SecurityAttributeOverride.None;
			}
		}
	}
}

