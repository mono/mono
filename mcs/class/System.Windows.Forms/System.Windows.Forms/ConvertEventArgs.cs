//
// System.Windows.Forms.ConvertEventArgs.cs
//
// Author:
//  Stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Finished by Dennis Hayes (dennish@raytek.com)
//  Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms {

	/// <summary>
	/// Provides data for the Format and Parse events.
	/// </summary>

	public class ConvertEventArgs : EventArgs {

		#region Fields

		private Type desiredtype;
		private object objectvalue;

		#endregion
		
		//Constructor
		public ConvertEventArgs(object objectValue,Type desiredType) 
		{
			this.desiredtype = desiredType;
			this.objectvalue = objectValue;
		}
		
		#region Public Properties

		public Type DesiredType 
		{
			get { 
					return desiredtype; 
				}
		}
		
		public object Value {
			get { 
				return objectvalue; 
			}
			set {
				objectvalue = value; 
			}
		}
		#endregion
	}
}
