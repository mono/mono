//
// System.Web.Mail.RelatedBodyPart.cs
//
// Authors:
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Web;
namespace System.Web.Mail 
{
	public class RelatedBodyPart
	{
		string id;
		string fileName;
		
		public RelatedBodyPart (string id, string fileName)
		{
			this.id = id;
			if (FileExists (fileName))
				this.fileName = fileName;
			else
				throw new HttpException(500, "Invalid related body part");
		}
		
		public string Name {
			get { return id; }
			set { id = value; }
		}

		public string Path {
			get { return fileName; }
			set { fileName = value; }
		}
		
		bool FileExists (string fileName)
		{
			//I am handling local files only . Not sure how URL's
			//need to be handled.
			try {
				System.IO.File.OpenRead (fileName).Close ();
				return true;
			} catch (Exception) {
			    return false;
			}
		}
	}
}
#endif
