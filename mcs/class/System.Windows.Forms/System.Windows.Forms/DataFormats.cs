//
// System.Windows.Forms.DataFormats
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//


namespace System.Windows.Forms {

	/// <summary>
	/// Provides static (Shared in Visual Basic), predefined Clipboard format names.
	/// Use them to identify the format of data that you store in an IDataObject.
	/// </summary>

	[MonoTODO]
	public class DataFormats {

		#region Fields
		public static readonly string Bitmap="Bitmap";
		public static readonly string CommaSeparatedValue="Csv";
		public static readonly string Dib="DeviceIndependentBitmap";
		public static readonly string Dif="DataInterchangeFormat";
		public static readonly string EnhancedMetafile="EnhancedMetafile";
		public static readonly string FileDrop="FileDrop";
		public static readonly string Html="HTML Format";
		public static readonly string Locale="Locale";
		public static readonly string MetafilePict="MetaFilePict";
		public static readonly string OemText="OEMText";
		public static readonly string Palette="Palette";
		public static readonly string PenData="PenData";
		public static readonly string Riff="RiffAudio";
		public static readonly string Rtf="Rich Text Format";
		public static readonly string Serializable="WindowsForms10PersistentObject";
		public static readonly string StringFormat="System.String";
		public static readonly string Text="Text";
		public static readonly string Tiff="TaggedImageFileFormat";
		public static readonly string UnicodeText="UnicodeText";
		public static readonly string WaveAudio="WaveAudio";
		public static readonly string SymbolicLink="SymbolLink";
		#endregion
		
		#region Methods
		/// <todo>
		/// these methods will have to use Windows Registry to fetch the existing Format (id,name)
		/// or create a new format with the given string/int
		/// </todo>
		[MonoTODO]
		public static Format GetFormat(int id) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static Format GetFormat(string format) 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		/// sub-class: DataFormats.Format
		/// <summary>
		/// Represents a clipboard format type.
		/// </summary>
		public class Format {
			int id;
			string name;
			
			#region Constructors
			/// <note>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </note>
			public Format (string name,int id) 
			{
				this.id=id;
				this.name=name;
			}
			#endregion

			#region Methods
			public int Id {
				get { return id; }
			}

			public string Name {
				get { return name; }
			}
			#endregion
		}
	}
}
