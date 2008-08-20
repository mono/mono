//
// FaultReason.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

using TextList = System.Collections.Generic.SynchronizedReadOnlyCollection<System.ServiceModel.FaultReasonText>;

namespace System.ServiceModel
{
	public class FaultReason
	{
		List<FaultReasonText> trans = new List<FaultReasonText> ();
		TextList public_trans;

		public FaultReason (FaultReasonText translation)
		{
			if (translation == null)
				throw new ArgumentNullException ("translation");
			trans.Add (translation);
		}

		public FaultReason (IEnumerable<FaultReasonText> translations)
		{
			if (translations == null)
				throw new ArgumentNullException ("translations");
			foreach (FaultReasonText t in translations)
				trans.Add (t);
			if (trans.Count == 0)
				throw new ArgumentException ("The argument list should contain at least one fault reason text.");
		}

		public FaultReason (string text)
			: this (new FaultReasonText (text))
		{
		}

		public TextList Translations {
			get {
				if (public_trans == null)
					public_trans = new TextList (new object (), trans);
				return public_trans;
			}
		}

		public FaultReasonText GetMatchingTranslation ()
		{
			return GetMatchingTranslation (CultureInfo.CurrentCulture);
		}

		public FaultReasonText GetMatchingTranslation (
			CultureInfo cultureInfo)
		{
			foreach (FaultReasonText t in trans)
				if (t.Matches (cultureInfo))
					return t;
			return trans [0];
		}

		public override string ToString ()
		{
			return GetMatchingTranslation ().Text;
		}
	}
}
