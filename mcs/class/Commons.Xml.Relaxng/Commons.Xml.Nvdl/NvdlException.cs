using System;
using System.Xml;

namespace Commons.Xml.Nvdl
{
	public class NvdlException : Exception
	{
		public NvdlException (string message)
			: base (message)
		{
		}

		public NvdlException (string message, Exception inner)
			: base (message ,inner)
		{
		}

		internal static string FormatMessage (string message,
			IXmlLineInfo lineInfo)
		{
			NvdlElementBase source = lineInfo as NvdlElementBase;
			XmlReader reader = lineInfo as XmlReader;
			if (source != null && source.HasLineInfo ())
				return String.Format ("{0}. {1} ({2},{3})",
					message, source.SourceUri,
					source.LineNumber, source.LinePosition);
			else if (lineInfo != null && lineInfo.HasLineInfo ())
				return String.Format ("{0}. {3}({1},{2})",
					message,
					lineInfo.LineNumber,
					lineInfo.LinePosition,
					reader != null ? reader.BaseURI + ' ' : String.Empty);
			else
				return message;
		}
	}

	public class NvdlCompileException : NvdlException
	{
		public NvdlCompileException (string message,
			IXmlLineInfo source)
			: this (message, null, source)
		{
		}

		public NvdlCompileException (string message, Exception inner,
			IXmlLineInfo source)
			: base (FormatMessage (message, source), inner)
		{
		}
	}

	public class NvdlValidationException : NvdlException
	{
		public NvdlValidationException (string message,
			IXmlLineInfo source)
			: this (message, null, source)
		{
		}

		public NvdlValidationException (string message, Exception inner,
			IXmlLineInfo source)
			: base (FormatMessage (message, source), inner)
		{
		}
	}

	public class NvdlInstanceValidationException : NvdlException
	{
		public NvdlInstanceValidationException (string message,
			NvdlValidatorGenerator generator,
			string nvdlLocation)
			: this (message, null, generator, nvdlLocation)
		{
		}

		public NvdlInstanceValidationException (string message, Exception inner,
			NvdlValidatorGenerator generator,
			string nvdlLocation)
			: base (FormatMessageWithDefinition (message, nvdlLocation), inner)
		{
		}

		// assuming that wrapped exception message usually 
		// contains the actual instance location info.
		static string FormatMessageWithDefinition (string message, string nvdlLocation)
		{
			return String.Format ("{0}. Related NVDL script: {1}", message, nvdlLocation);
		}
	}
}

