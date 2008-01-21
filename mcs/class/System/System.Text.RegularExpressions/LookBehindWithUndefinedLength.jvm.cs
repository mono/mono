using System;
using System.Collections.Generic;
using System.Text;
using java.util.regex;
using java.lang;

namespace System.Text.RegularExpressions
{
	sealed class LookBehindWithUndefinedLength : IConstructType
	{
		//private const string DEFINITION = @"\(\?<[=!].*(?:[\*\+]|\{\d+,\}).*\)";
		private const string DEFINITION = @"\(\?<[=!][^\)]*\)";

		public bool HasConstruct (string pattern, RegexOptions options) {
			return JavaUtils.IsMatch (pattern, DEFINITION);
		}

		public string Reformat (RegexOptions options,
			string reformattedPattern,
			PatternGrouping patternGrouping) {
			throw new NotImplementedException ("Reformat for look ahead with undefined length construct is not implemented.");
		}
	}
}
