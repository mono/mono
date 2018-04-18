using System;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Mono
{
	class StackFrameData
	{
		static Regex regex = new Regex (@"\w*at (?<Method>.+) *(\[0x(?<IL>.+)\]|<0x.+ \+ 0x(?<NativeOffset>.+)>( (?<MethodIndex>\d+)|)) in <(?<MVID>[^>#]+)(#(?<AOTID>[^>]+)|)>:0");

		public readonly string TypeFullName;
		public readonly string MethodSignature;
		public readonly int Offset;
		public readonly bool IsILOffset;
		public readonly uint MethodIndex;
		public readonly string Line;
		public readonly string Mvid;
		public readonly string Aotid;

		private StackFrameData (string line, string typeFullName, string methodSig, int offset, bool isILOffset, uint methodIndex, string mvid, string aotid)
		{
			Line = line;
			TypeFullName = typeFullName;
			MethodSignature = methodSig;
			Offset = offset;
			IsILOffset = isILOffset;
			MethodIndex = methodIndex;
			Mvid = mvid;
			Aotid = aotid;
		}

		public StackFrameData Relocate (string typeName, string methodName)
		{
			return new StackFrameData (Line, typeName, methodName, Offset, IsILOffset, MethodIndex, Mvid, Aotid);
		}

		public static bool TryParse (string line, out StackFrameData stackFrame)
		{
			stackFrame = null;

			var match = regex.Match (line);
			if (!match.Success)
				return false;

			string typeFullName, methodSignature;
			var methodStr = match.Groups ["Method"].Value.Trim ();
			if (!ExtractSignatures (methodStr, out typeFullName, out methodSignature))
				return false;

			var isILOffset = !string.IsNullOrEmpty (match.Groups ["IL"].Value);
			var offsetVarName = (isILOffset)? "IL" : "NativeOffset";
			var offset = int.Parse (match.Groups [offsetVarName].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

			uint methodIndex = 0xffffff;
			if (!string.IsNullOrEmpty (match.Groups ["MethodIndex"].Value))
				methodIndex = uint.Parse (match.Groups ["MethodIndex"].Value, CultureInfo.InvariantCulture);

			var mvid = match.Groups ["MVID"].Value;
			var aotid = match.Groups ["AOTID"].Value;

			stackFrame = new StackFrameData (line, typeFullName, methodSignature, offset, isILOffset, methodIndex, mvid, aotid);

			return true;
		}

		static bool ExtractSignatures (string str, out string typeFullName, out string methodSignature)
		{
			var methodNameEnd = str.IndexOf ('(');
			if (methodNameEnd == -1) {
				typeFullName = methodSignature = null;
				return false;
			}

			var typeNameEnd = str.LastIndexOf ('.', methodNameEnd);
			if (typeNameEnd == -1) {
				typeFullName = methodSignature = null;
				return false;
			}

			// Adjustment for Type..ctor ()
			if (typeNameEnd > 0 && str [typeNameEnd - 1] == '.') {
				--typeNameEnd;
			}

			typeFullName = str.Substring (0, typeNameEnd);
			// Remove generic parameters
			typeFullName = Regex.Replace (typeFullName, @"\[[^\[\]]*\]$", "");
			typeFullName = Regex.Replace (typeFullName, @"\<[^\[\]]*\>$", "");

			methodSignature = str.Substring (typeNameEnd + 1);

			return true;
		}
	}
}
