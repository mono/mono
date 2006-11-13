/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Events.Edir.EventData.BaseEdirEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
  /// <summary> 
  /// This is the base class for all types of data classes associated
  /// with an event.
  /// </summary>
  public class BaseEdirEventData
  {
    protected MemoryStream decodedData = null;
    protected LBERDecoder decoder = null;

    protected EdirEventDataType event_data_type;

    /// <summary> 
    /// The value for this attribute allows the caller to identify the
    /// type of the data object.
    /// </summary>
    public EdirEventDataType EventDataType
    {
      get
      {
	return event_data_type;
      }
    }

    public BaseEdirEventData(EdirEventDataType eventDataType, Asn1Object message)
    {
      event_data_type = eventDataType;

      byte[] byteData = SupportClass.ToByteArray(((Asn1OctetString) message).byteValue());
      decodedData = new MemoryStream(byteData);
      decoder = new LBERDecoder();
    }

    protected void DataInitDone()
    {
      // We dont want the unnecessary memory to remain occupied if
      // this object is retained by the caller
      decodedData = null;
      decoder = null;
    }
  }
}
