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
// Novell.Directory.Ldap.Events.Edir.EventData.DebugEventData.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections;
using System.Text;

using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Events.Edir.EventData
{
  /// <summary> 
  /// This class represents the data for Debug Events.
  /// </summary>
  public class DebugEventData : BaseEdirEventData
  {
    protected int ds_time;
    public int DSTime
    {
      get
      {
	return ds_time;
      }
    }

    protected int milli_seconds;
    public int MilliSeconds
    {
      get
      {
	return milli_seconds;
      }
    }

    protected string strPerpetratorDN;
    public string PerpetratorDN
    {
      get 
      {
	return strPerpetratorDN;
      }
    }

    protected string strFormatString;
    public string FormatString
    {
      get
      {
	return strFormatString;
      }
    }

    protected int nVerb;
    public int Verb
    {
      get
      {
	return nVerb;
      }
    }

    protected int parameter_count;
    public int ParameterCount
    {
      get
      {
	return parameter_count;
      }
    }

    protected ArrayList parameter_collection;
    public ArrayList Parameters
    {
      get
      {
	return parameter_collection;
      }
    }
    
    public DebugEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
        int[] length = new int[1];

        ds_time = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
        milli_seconds =
            ((Asn1Integer) decoder.decode(decodedData, length)).intValue();

        strPerpetratorDN =
            ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
        strFormatString =
            ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
        nVerb = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
        parameter_count =
            ((Asn1Integer) decoder.decode(decodedData, length)).intValue();

	parameter_collection = new ArrayList();

        if (parameter_count > 0) 
	{
            Asn1Sequence seq = (Asn1Sequence) decoder.decode(decodedData, length);
            for (int i = 0; i < parameter_count; i++) 
	    {
                parameter_collection.Add(
                    new DebugParameter((Asn1Tagged) seq.get_Renamed(i))
		    );
            }
        }

	DataInitDone();
    }    

    /// <summary> 
    /// Returns a string representation of the object.
    /// </summary>
    public override string ToString()
    {
      StringBuilder buf = new StringBuilder();
      buf.Append("[DebugEventData");
      buf.AppendFormat("(Millseconds={0})", milli_seconds);
      buf.AppendFormat("(DSTime={0})", ds_time);
      buf.AppendFormat("(PerpetratorDN={0})", strPerpetratorDN);
      buf.AppendFormat("(Verb={0})",nVerb);
      buf.AppendFormat("(ParameterCount={0})", parameter_count);
      for (int i = 0; i < parameter_count; i++)
      {
	buf.AppendFormat("(Parameter[{0}]={1})", i, parameter_collection[i]);
      }
      buf.Append("]");

      return buf.ToString();
    }
  }
}
