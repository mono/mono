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
// Novell.Directory.Ldap.Events.Edir.EventData.ModuleStateEventData.cs
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
  /// This class represents the data for Module State Events.
  /// </summary>
  public class ModuleStateEventData : BaseEdirEventData
  {
    protected string strConnectionDN;
    public string ConnectionDN
    {
      get
      {
	return strConnectionDN;
      }
    }

    protected int nFlags;
    public int Flags
    {
      get
      {
	return nFlags;
      }
    }

    protected string strName;
    public string Name
    {
      get
      {
	return strName;
      }
    }

    protected string strDescription;
    public string Description
    {
      get
      {
	return strDescription;
      }
    }

    protected string strSource;
    public string Source
    {
      get
      {
	return strSource;
      }
    }

    public ModuleStateEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
      int[] length = new int[1];

      strConnectionDN = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
      nFlags = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      strName = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
      strDescription = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
      strSource = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();

      DataInitDone();
    }

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString() 
    {
      StringBuilder buf = new StringBuilder();
      buf.Append("[ModuleStateEvent");
      buf.AppendFormat("(connectionDN={0})", strConnectionDN);
      buf.AppendFormat("(flags={0})", nFlags);
      buf.AppendFormat("(Name={0})", strName);
      buf.AppendFormat("(Description={0})", strDescription);
      buf.AppendFormat("(Source={0})", strSource);
      buf.Append("]");

      return buf.ToString();
    }
  }
}
