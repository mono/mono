namespace System.Web.DynamicData {
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    internal class RepeaterDataBoundAdapter : System.Web.UI.WebControls.IDataBoundControl {
        private Repeater _repeater;
        public RepeaterDataBoundAdapter(Repeater repeater) {
            _repeater = repeater;
        }

        public string[] DataKeyNames {
            get {
                return null;
            }
            set {
                throw new NotImplementedException();
            }
        }

        public string DataMember {
            get {
                return _repeater.DataMember;
            }
            set {
                _repeater.DataMember = value;
            }
        }

        public object DataSource {
            get {
                return _repeater.DataSource;
            }
            set {
                _repeater.DataSource = value;
            }
        }

        public string DataSourceID {
            get {
                return _repeater.DataSourceID;
            }
            set {
                _repeater.DataSourceID = value;
            }
        }

        public IDataSource DataSourceObject {
            get {
                return Misc.FindControl(_repeater, DataSourceID) as IDataSource;
            }
        }
    }
}
