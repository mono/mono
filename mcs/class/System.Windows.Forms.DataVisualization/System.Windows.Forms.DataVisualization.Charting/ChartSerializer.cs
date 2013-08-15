// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//
// (C) Francis Fisher 2013
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

using System.IO;
using System.Xml;

namespace System.Windows.Forms.DataVisualization.Charting
{
	public class ChartSerializer
	{
		public SerializationContents Content { get; set; }
		public SerializationFormat Format { get; set; }
		public bool IsResetWhenLoading { get; set; }
		public bool IsTemplateMode { get; set; }
		public bool IsUnknownAttributeIgnored { get; set; }
		public string NonSerializableContent { get; set; }
		public string SerializableContent { get; set; }


		[MonoTODO]
		protected string GetContentString (SerializationContents content, bool serializable)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (TextReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (XmlReader reader)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Reset ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Save (Stream stream)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Save (string fileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Save (TextWriter writer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Save (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}
	}
}
