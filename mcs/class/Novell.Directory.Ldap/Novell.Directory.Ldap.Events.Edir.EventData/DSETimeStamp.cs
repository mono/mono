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
// Novell.Directory.Ldap.Events.Edir.EventData.DSETimeStamp.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Text;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir
{
  /// <summary> 
  /// The class represents the Timestamp datastructure for Edir events
  /// Notification.
  /// </summary>
  public class DSETimeStamp
  {
    protected int nSeconds;
    public int Seconds
    {
      get
      {
	return nSeconds;
      }
    }

    protected int replica_number;
    public int ReplicaNumber
    {
      get
      {
	return replica_number;
      }
    }

    protected int nEvent;
    public int Event
    {
      get
      {
	return nEvent;
      }
    }

    public DSETimeStamp(Asn1Sequence dseObject)
    {
      nSeconds = ((Asn1Integer)dseObject.get_Renamed(0)).intValue();
      replica_number = ((Asn1Integer) dseObject.get_Renamed(1)).intValue();
      nEvent = ((Asn1Integer) dseObject.get_Renamed(2)).intValue();
    }

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString()
    {
      StringBuilder buf = new StringBuilder();

      buf.AppendFormat("[TimeStamp (seconds={0})", nSeconds);
      buf.AppendFormat("(replicaNumber={0})", replica_number);
      buf.AppendFormat("(event={0})", nEvent);
      buf.Append("]");
      
      return buf.ToString();
    }
  }
}

