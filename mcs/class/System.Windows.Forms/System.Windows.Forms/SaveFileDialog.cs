//
// System.Windows.Forms.SaveFileDialog.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@raytek.com)
//  Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
//
// (C) 2002 Ximian, Inc
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

using System.IO;

namespace System.Windows.Forms
{
	public sealed class SaveFileDialog : FileDialog
	{
		private bool createPrompt = false;
		private bool overwritePrompt = true;

		//
		//  --- Constructor
		//		
		public SaveFileDialog ()
		{
			Reset();
		}

		//
		//  --- Public Properties
		//		
		public bool CreatePrompt {
			get {return createPrompt;}
			set {createPrompt = value;}
		}
		
		public bool OverwritePrompt {
			get {return overwritePrompt;}
			set {overwritePrompt = value;}
		}

		//
		//  --- Public Methods
		//		
		public Stream OpenFile ()
		{
			return new FileStream (FileName, FileMode.OpenOrCreate);
		}
		
		internal override void initOpenFileName (ref OPENFILENAME opf)
		{
			base.initOpenFileName (ref opf);

			if (createPrompt)
				opf.Flags |= (int) (OpenFileDlgFlags.OFN_CREATEPROMPT);
			
			if (overwritePrompt)
				opf.Flags |= (int) (OpenFileDlgFlags.OFN_OVERWRITEPROMPT);
		}
		
		public override void Reset ()
		{
			isSave = true;
			createPrompt = false;
			overwritePrompt = true;
		}
	}
}
