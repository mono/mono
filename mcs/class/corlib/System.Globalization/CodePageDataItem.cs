//
// System.Globalization.CodePageDataItem.cs
//
// Author:
//   Kornél Pál <http://www.kornelpal.hu/>
//
// Copyright (C) 2006 Kornél Pál
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

//
// .NET Framework 1.x serializes encoding properties using this class and
// .NET Framework 2.0 can deserialize it but drops the properties.
// This class supports serialization compatibility.
//

using System;

namespace System.Globalization
{
	[Serializable]
	internal sealed class CodePageDataItem
	{
#pragma warning disable 169	
		private string m_bodyName;
		private int m_codePage;
		private int m_dataIndex;
		private string m_description;
		private uint m_flags;
		private string m_headerName;
		private int m_uiFamilyCodePage;
		private string m_webName;

		private CodePageDataItem ()
		{
		}
#pragma warning disable 169		
	}
}
