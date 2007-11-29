using System;
using System.Collections.Generic;
using System.Text;
using java.util.regex;
using java.lang;

namespace System.Text.RegularExpressions
{
	sealed class NotAllowedConstruct : IConstructType
	{
		private const string DEFINITION = @"(\A|((\A|[^\\])([\\]{2})*\((\?([:>=!]|<([=!]|(\w+>))))?))\{\d+(,(\d+)?)?\}";

		public bool HasConstruct (string pattern, RegexOptions options) {
			return JavaUtils.IsMatch (pattern, DEFINITION);
		}

		public string Reformat (RegexOptions options,
			string reformattedPattern,
			PatternGrouping patternGrouping) {
			throw new NotImplementedException ("Reformat for not allowed constructs is not implemented.");
		}
	}
}
