//
// System.Windows.Forms.ConvertEventArgs.cs
//
// Author:
//  Stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Finished by Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the Format and Parse events.
	/// </summary>

	public class ConvertEventArgs : EventArgs {

		Type desiredType;
		object objectValue;
		
		//Constructor
		public ConvertEventArgs(object objectValue,Type desiredType) 
		{
			this.desiredType = desiredType;
			this.objectValue = objectValue;
		}
		
		public Type DesiredType {
			get { return desiredType; }
		}
		
		public object Value {
			get { return objectvalue; }
			set { objectvalue = value; }
		}
	}
}
