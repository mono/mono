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
// Novell.Directory.Ldap.Events.Edir.EventData.ConnectionStateEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Text;

using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
  /// <summary> 
  /// This class represents the data for Connection State Events.
  /// </summary>
  public class ConnectionStateEventData : BaseEdirEventData
  {
    protected string strConnectionDN;
    public string ConnectionDN
    {
      get
      {
	return strConnectionDN;
      }
    }

    protected int old_flags;
    public int OldFlags
    {
      get
      {
	return old_flags;
      }
    }

    protected int new_flags;
    public int NewFlags
    {
      get
      {
	return new_flags;
      }
    }

    protected string source_module;
    public string SourceModule
    {
      get
      {
	return source_module;
      }
    }

    public ConnectionStateEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
      int[] length = new int[1];

      strConnectionDN = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
      old_flags = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      new_flags = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      source_module = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();

      DataInitDone();
    }

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString() 
    {
      StringBuilder buf = new StringBuilder();
      buf.Append("[ConnectionStateEvent");
      buf.AppendFormat("(ConnectionDN={0})", strConnectionDN);
      buf.AppendFormat("(oldFlags={0})", old_flags);
      buf.AppendFormat("(newFlags={0})", new_flags);
      buf.AppendFormat("(SourceModule={0})", source_module);
      buf.Append("]");

      return buf.ToString();
    }
  }
}
