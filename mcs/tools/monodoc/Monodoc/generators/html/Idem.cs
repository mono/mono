using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Monodoc;
using Monodoc.Generators;

namespace Monodoc.Generators.Html
{
	// Input is expected to be already HTML so just return it
	public class Idem : IHtmlExporter
	{
		public string CssCode {
			get {
				return string.Empty;
			}
		}

		public string Export (Stream input, Dictionary<string, string> extraArgs)
		{
			if (input == null)
				return null;
			return new StreamReader (input).ReadToEnd ();
		}

		public string Export (string input, Dictionary<string, string> extraArgs)
		{
			if (string.IsNullOrEmpty (input))
				return null;
			return input;
		}
	}
}
