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
// Novell.Directory.Ldap.Events.Edir.EventData.SecurityEquivalenceEventData.cs
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
  /// This class represents the data for Security Equivalence Events.
  /// </summary>
  public class SecurityEquivalenceEventData : BaseEdirEventData
  {
    protected string strEntryDN;
    public string EntryDN
    {
      get
      {
	return strEntryDN;
      }
    }

    protected int retry_count;
    public int RetryCount
    {
      get
      {
	return retry_count;
      }
    }

    protected string strValueDN;
    public string ValueDN
    {
      get
      {
	return strValueDN;
      }
    }

    protected int referral_count;
    public int ReferralCount
    {
      get
      {
	return referral_count;
      }
    }

    protected ArrayList referral_list;
    public ArrayList ReferralList
    {
      get
      {
	return referral_list;
      }
    }

    public SecurityEquivalenceEventData(EdirEventDataType eventDataType, Asn1Object message)
      : base(eventDataType, message)
    {
      int[] length = new int[1];

      strEntryDN = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();
      retry_count = ((Asn1Integer) decoder.decode(decodedData, length)).intValue();
      strValueDN = ((Asn1OctetString) decoder.decode(decodedData, length)).stringValue();

      Asn1Sequence referalseq = ((Asn1Sequence) decoder.decode(decodedData, length));

      referral_count = ((Asn1Integer) referalseq.get_Renamed(0)).intValue();
      referral_list = new ArrayList();
      if (referral_count > 0) 
      {
	Asn1Sequence referalseqof = ((Asn1Sequence) referalseq.get_Renamed(1));

	for (int i = 0; i < referral_count; i++) 
	{
	  referral_list.Add( new ReferralAddress( (Asn1Sequence) referalseqof.get_Renamed(i) ) );
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
      buf.Append("[SecurityEquivalenceEventData");
      buf.AppendFormat("(EntryDN={0})", strEntryDN);
      buf.AppendFormat("(RetryCount={0})", retry_count);
      buf.AppendFormat("(valueDN={0})", strValueDN);
      buf.AppendFormat("(referralCount={0})", referral_count);
      buf.AppendFormat("(Referral Lists={0})", referral_list);
      buf.Append("]");

      return buf.ToString();
    }

  }
}
