//
// System.Web.Configuration.IRemoteWebConfigurationHostServer.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System.Resources;
using System.Runtime.InteropServices;

#if NET_2_0
namespace System.Web.Configuration
{
	[ComVisibleAttribute (true)]
	[GuidAttribute ("A99B591A-23C6-4238-8452-C7B0E895063D")]
	public interface IRemoteWebConfigurationHostServer
	{
		string DoEncryptOrDecrypt (
		    bool do_encrypt, string xml_string, string protection_provider_name,
		    string protection_provider_type, string [] params_keys, string [] param_values);

		byte [] GetData (string filename, bool getReadTimeOnly, out long readTime);

		void GetFileDetails (string name, out bool exists, out long size, out long create_data, out long last_write_date);

		string GetFilePaths (int webLevel, string path, string site, string locationSubPath);

		void WriteData (string fileName, string templateFileName, byte [] data, ref long readTime);
	}
}
#endif
