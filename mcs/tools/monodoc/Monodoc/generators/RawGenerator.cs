using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using Monodoc;

namespace Monodoc.Generators
{
	/// <summary>
	/// This generators returns the raw content of the HelpSource without any transformation
	/// </summary>
	public class RawGenerator : IDocGenerator<string>
	{
		public string Generate (HelpSource hs, string id)
		{
			if (hs == null || string.IsNullOrEmpty (id))
				return null;

			IEnumerable<string> parts;
			if (hs.IsMultiPart (id, out parts))
				return GenerateMultiPart (hs, parts, id);

			if (hs.IsRawContent (id))
				return hs.GetText (id) ?? string.Empty;

			var result = hs.IsGeneratedContent (id) ? hs.GetCachedText (id) : new StreamReader (hs.GetCachedHelpStream (id)).ReadToEnd ();

			return result;
		}

		string GenerateMultiPart (HelpSource hs, IEnumerable<string> ids, string originalId)
		{
			var sb = new StringBuilder ();
			foreach (var id in ids)
				sb.AppendLine (Generate (hs, id));
			return sb.ToString ();
		}
	}
}
