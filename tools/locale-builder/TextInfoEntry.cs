//
// TextInfoEntry.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
//
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

using System.Text;

namespace Mono.Tools.LocaleBuilder
{
	public class TextInfoEntry : Entry
	{
		public string ANSICodePage;
		public string EBCDICCodePage;
		public string MacCodePage;
		public string OEMCodePage;
		public string ListSeparator;
		public bool IsRightToLeft;

		public override string ToString ()
		{
			StringBuilder b = new StringBuilder ();
			b.Append ("{ ");
			b.Append (ANSICodePage).Append (", ");
			b.Append (EBCDICCodePage).Append (", ");
			b.Append (MacCodePage).Append (", ");
			b.Append (OEMCodePage).Append (", ");
			b.Append (IsRightToLeft ? "1" : "0").Append (", '");

			// TODO: It's more than 1 char for some cultures
			if (ListSeparator.Length <= 1)
				b.Append (ListSeparator);
			else
				b.Append (";");

			b.Append ("' }");

			return b.ToString ();
		}
	}
}