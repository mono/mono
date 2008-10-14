// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;

namespace Mono.Mozilla.DOM
{
	
	
	internal enum DocumentEncoderFlags: uint 
	{
		/** 
		* Output only the selection (as opposed to the whole document).
		*/
		OutputSelectionOnly = (1 << 0),
		
		/** Plaintext output: Convert html to plaintext that looks like the html.
		* Implies wrap (except inside <pre>), since html wraps.
		* HTML output: always do prettyprinting, ignoring existing formatting.
		* (Probably not well tested for HTML output.)
		*/
		OutputFormatted     = (1 << 1),
		
		/** Don't do prettyprinting of HTML.  Don't do any wrapping that's not in
		* the existing HTML source.  This option overrides OutputFormatted if both
		* are set.
		* @note This option does not affect entity conversion.
		*/
		OutputRaw           = (1 << 2),
		
		/** 
		* Do not print html head tags.
		*/
		OutputBodyOnly      = (1 << 3),
		
		/**
		* Wrap even if we're not doing formatted output (e.g. for text fields)
		* XXXbz this doesn't seem to be used by all serializers... document?  How
		* does this interact with
		* OutputFormatted/OutputRaw/OutputWrap/OutputFormatFlowed?
		*/
		OutputPreformatted  = (1 << 4),
		
		/**
		* Output as though the content is preformatted
		* (e.g. maybe it's wrapped in a MOZ_PRE or MOZ_PRE_WRAP style tag)
		* XXXbz this doesn't seem to be used by all serializers... document?  How
		* does this interact with
		* OutputFormatted/OutputRaw/OutputPreformatted/OutputFormatFlowed?
		*/
		OutputWrap          = (1 << 5),
		
		/**
		* Output for format flowed (RFC 2646). This is used when converting
		* to text for mail sending. This differs just slightly
		* but in an important way from normal formatted, and that is that
		* lines are space stuffed. This can't (correctly) be done later.
		* XXXbz this doesn't seem to be used by all serializers... document?  How
		* does this interact with
		* OutputFormatted/OutputRaw/OutputPreformatted/OutputWrap?
		*/
		OutputFormatFlowed  = (1 << 6),
		
		/**
		* Convert links, image src, and script src to absolute URLs when possible
		*/
		OutputAbsoluteLinks = (1 << 7),
		
		/**
		* Attempt to encode entities standardized at W3C (HTML, MathML, etc).
		* This is a catch-all flag for documents with mixed contents. Beware of
		* interoperability issues. See below for other flags which might likely
		* do what you want.
		*/
		OutputEncodeW3CEntities = (1 << 8),
		
		/** 
		* LineBreak processing: if this flag is set than CR line breaks will
		* be written. If neither this nor OutputLFLineBreak is set, then we
		* will use platform line breaks. The combination of the two flags will
		* cause CRLF line breaks to be written.
		*/
		OutputCRLineBreak = (1 << 9),
		
		/** 
		* LineBreak processing: if this flag is set than LF line breaks will
		* be written. If neither this nor OutputCRLineBreak is set, then we
		* will use platform line breaks. The combination of the two flags will
		* cause CRLF line breaks to be written.
		*/
		OutputLFLineBreak = (1 << 10),
		
		/**
		* Output the content of noscript elements (only for serializing
		* to plaintext).
		*/
		OutputNoScriptContent = (1 << 11),
		
		/**
		* Output the content of noframes elements (only for serializing
		* to plaintext).
		*/
		OutputNoFramesContent = (1 << 12),
		
		/**
		* Don't allow any formatting nodes (e.g. <br>, <b>) inside a <pre>.
		* This is used primarily by mail.
		*/
		OutputNoFormattingInPre = (1 << 13),
		
		/**
		* Encode entities when outputting to a string.
		* E.g. If set, we'll output &nbsp; if clear, we'll output 0xa0.
		* The basic set is just &nbsp; &amp; &lt; &gt; &quot; for interoperability
		* with older products that don't support &alpha; and friends.
		*/
		OutputEncodeBasicEntities = (1 << 14),
		
		/**
		* Encode entities when outputting to a string.
		* The Latin1 entity set additionally includes 8bit accented letters
		* between 128 and 255.
		*/
		OutputEncodeLatin1Entities = (1 << 15),
		
		/**
		* Encode entities when outputting to a string.
		* The HTML entity set additionally includes accented letters, greek
		* letters, and other special markup symbols as defined in HTML4.
		*/
		OutputEncodeHTMLEntities = (1 << 16),
		
		/**
		* Normally &nbsp; is replaced with a space character when
		* encoding data as plain text, set this flag if that's
		* not desired.
		*/
		OutputPersistNBSP = (1 << 17),
		
	}
}
