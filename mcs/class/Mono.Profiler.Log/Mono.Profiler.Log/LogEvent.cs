// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mono.Profiler.Log {

	public abstract class LogEvent {

		const BindingFlags PropertyFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

		const string Indent = "  ";

		internal LogEvent ()
		{
		}

		public LogBufferHeader Buffer { get; internal set; }

		public ulong Timestamp { get; internal set; }

		public override string ToString ()
		{
			var sb = new StringBuilder ();

			ToString (this, sb, string.Empty, GetType ().Name, 0);

			return sb.ToString ();
		}

		static void ToString (object source, StringBuilder result, string indent, string header, int level)
		{
			result.AppendLine ($"{indent}{header} {{");

			foreach (var prop in source.GetType ().GetProperties (PropertyFlags).OrderBy (p => p.MetadataToken)) {
				var name = prop.Name;
				var propIndent = indent + Indent;
				var value = prop.GetValue (source);

				if (value is IList list) {
					result.AppendLine ($"{propIndent}{name} = [{list.Count}] {{");

					for (var i = 0; i < list.Count; i++) {
						var elem = list [i];
						var type = elem.GetType ();
						var elemIndent = propIndent + Indent;
						var elemHeader = $"[{i}] = ";

						if (type.IsClass && type != typeof (string))
							ToString (elem, result, elemIndent, $"{elemHeader}{type.Name}", level + 1);
						else
							result.AppendLine ($"{elemIndent}{elemHeader}{elem}");
					}

					result.AppendLine ($"{propIndent}}}");
				} else
					result.AppendLine ($"{propIndent}{name} = {value}");
			}

			var end = $"{indent}}}";

			if (level == 0)
				result.Append (end);
			else
				result.AppendLine (end);
		}

		internal abstract void Accept (LogEventVisitor visitor);
	}
}
