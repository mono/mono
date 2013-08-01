// Authors:
// Francis Fisher (frankie@terrorise.me.uk)
//

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


		protected string GetContentString(
			SerializationContents content,
			bool serializable
			){
			throw new NotImplementedException ();
		}

		public void Load(Stream stream){
			throw new NotImplementedException ();
		}

		public void Load(string fileName){
			throw new NotImplementedException ();
		}

		public void Load(TextReader reader){
			throw new NotImplementedException ();
		}

		public void Load(XmlReader reader){
			throw new NotImplementedException ();
		}

		public void Reset(){
			throw new NotImplementedException ();
		}

		public void Save(
			Stream stream
			){
			throw new NotImplementedException ();
		}
		public void Save(
			string fileName
			){
			throw new NotImplementedException ();
		}

		public void Save(
			TextWriter writer
			){
			throw new NotImplementedException ();
		}

		public void Save(
			XmlWriter writer
			){
			throw new NotImplementedException ();
		}
	}
}