using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using MonoTests.SystemWeb.Framework;

public partial class Sections_ECCN_test : System.Web.UI.Page
{
	//protected override void OnPreInit (EventArgs e)
	public Sections_ECCN_test ()
	{
		WebTest t = WebTest.CurrentTest;
		if (t != null)
			t.Invoke (this);
	}
		
	public override void VerifyRenderingInServerForm (Control c)
	{

	}

    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void btnSearch_Click(object sender, System.EventArgs e)
    {

        gvECCN.Visible = true;
    }

}

namespace App.Test
{

    public class ECCNYearList
    {

        private Int32 _year = 0;
        public Int32 year
        {
            get { return _year; }
            set { _year = value; }
        }

        private String _yearText = "";
        public String yearText
        {
            get { return _yearText; }
            set { _yearText = value; }
        }

        public ECCNYearList() { }

        public ECCNYearList(Int32 p_year) { year = p_year; yearText = p_year.ToString(); }
    }

    /// <summary>
    /// Class to get a list of US States
    /// </summary>
    [Serializable]
    public class ECCN
    {
        #region Public/Private properties

        private Int32 _year = 0;
        public Int32 year
        {
            get { return _year; }
            set { _year = value; }
        }

        private String _eccn = "";
        public String eccn
        {
            get { return _eccn; }
            set { _eccn = value; }
        }

        private String _sched_b = "";
        public String sched_b
        {
            get { return _sched_b; }
            set { _sched_b = value; }
        }

        private Int32 _count = 0;
        public Int32 count
        {
            get { return _count; }
            set { _count = value; }
        }

        private Double _percent = 0.0;
        public Double percent
        {
            get { return _percent; }
            set { _percent = value; }
        }

        private Int32 _rownum = 0;
        public Int32 rownum
        {
            get { return _rownum; }
            set { _rownum = value; }
        }

        private Int32 _total = 0;
        public Int32 total
        {
            get { return _total; }
            set { _total = value; }
        }

        #endregion

        #region Constructors

        public ECCN() { }

        public ECCN(Int32 p_year, String p_eccn, String p_sched_b, Int32 p_count, Double p_percent, Int32 p_rownum, Int32 p_total)
        {
        
            year = p_year;
            eccn = p_eccn.Trim().ToUpper();
            sched_b = p_sched_b.Trim();
            count = p_count;
            percent = p_percent;
            rownum = p_rownum;
            total = p_total;
        }

        #endregion

        public static List<ECCN> GetECCNSummaryWithFilter(Int32 startRowIndex, Int32 maximumRows, Int32 year,  String eccn)
        {

            List<ECCN> summary = new List<ECCN>();

            summary.Add(new ECCN(2009, "test", "test", 1, 2.5, 1, 100));

            return summary;

        }

        public static Int32 GetECCNSummaryCountWithFilter(Int32 year, String eccn)
        { 
            return 1;
        }


        public static List<ECCNYearList> GetECCNYearList()
        {

            List<ECCNYearList> result = new List<ECCNYearList>();

            result.Add(new ECCNYearList(2009));
            result.Add(new ECCNYearList(2008));
            return result;

        }

    }

}

