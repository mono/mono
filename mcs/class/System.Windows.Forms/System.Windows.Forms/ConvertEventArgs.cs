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
