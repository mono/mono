//
// System.Web.UI.Design.SupportsPreviewControlAttribute
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2007 Novell, Inc (http://www.novell.com)
//

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

#if NET_2_0

namespace System.Web.UI.Design
{
	[AttributeUsage (AttributeTargets.Class)]
	public sealed class SupportsPreviewControlAttribute : Attribute
	{
		bool is_default, supports_preview;

		public SupportsPreviewControlAttribute (bool supportsPreviewControl)
			: this (supportsPreviewControl,false)
		{
		}

		SupportsPreviewControlAttribute (bool supportsPreviewControl, bool isDefault)
		{
			this.supports_preview = supportsPreviewControl;
			this.is_default = isDefault;
		}

		public static readonly SupportsPreviewControlAttribute Default =
			new SupportsPreviewControlAttribute (false, true);

		public bool SupportsPreviewControl {
			get { return supports_preview; }
		}

		public override bool Equals (object obj)
		{
			SupportsPreviewControlAttribute a = obj as SupportsPreviewControlAttribute;
			return a != null && a.supports_preview == supports_preview;
		}

		public override int GetHashCode ()
		{
			return supports_preview ? 1 : 0;
		}

		public override bool IsDefaultAttribute ()
		{
			return is_default;
		}
	}
}

#endif
