
using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace testwebemailcontrols
{
	public partial class test : System.Web.UI.UserControl
	{
        private ctlItem _slam;
        StringBuilder _stringBuilderSlam;
        string _stringSlam;
        int _intSlam;
        DateTime _dateTimeSlam;
        
        [PersistenceModeAttribute(PersistenceMode.InnerProperty)]
        public ctlItem slam
        {
            get { return _slam; }
            set { _slam = value; }
        }

	public StringBuilder stringBuilderSlam {
	    get { return _stringBuilderSlam; }
	    set { _stringBuilderSlam = value; }
	}
    
	public string stringSlam {
	    get { return _stringSlam; }
	    set { _stringSlam = value; }
	}
	
	public int intSlam {
	    get { return _intSlam; }
	    set { _intSlam = value; }
	}

	public DateTime dateTimeSlam {
	    get { return _dateTimeSlam; }
	    set { _dateTimeSlam = value; }
	}
	
        public class ctlItem
        {
            string _Text = "123";
            string _state = "345";

            public string State
            {
                get { return _state; }
                set { _state = value; }
            }

            [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
            public string Text
            {
                get { return _Text; }
                set { _Text = value; }
            }
        }
	}
}

