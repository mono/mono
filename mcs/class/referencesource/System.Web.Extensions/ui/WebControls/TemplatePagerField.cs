//------------------------------------------------------------------------------
// <copyright file="TemplatePagerField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web;
using System.Web.Resources;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    public class TemplatePagerField : DataPagerField {
        private int _startRowIndex;
        private int _maximumRows;
        private int _totalRowCount;
        private ITemplate _pagerTemplate;

        private static readonly object EventPagerCommand = new object();
        private EventHandlerList _events;

        public TemplatePagerField() {
        }
                
        /// <devdoc>
        /// Indicates the list of event handler delegates for the view. This property is read-only.
        /// </devdoc>
        private EventHandlerList Events {
            get {
                if (_events == null) {
                    _events = new EventHandlerList();
                }
                return _events;
            }
        }
                
        /// <devdoc>
        /// <para> Specifies the <see cref='System.Web.UI.ITemplate' qualify='true'/> that defines how items are rendered. </para>
        /// </devdoc>
        [
            Browsable(false),
            DefaultValue(null),
            ResourceDescription("TemplatePagerField_PagerTemplate"),
            PersistenceMode(PersistenceMode.InnerProperty),
            TemplateContainer(typeof(DataPagerFieldItem), BindingDirection.TwoWay)
        ]
        public virtual ITemplate PagerTemplate {
            get {
                return _pagerTemplate;
            }
            set {
                _pagerTemplate = value;
                OnFieldChanged();
            }
        }

        [
        Category("Action"),
        ResourceDescription("TemplatePagerField_OnPagerCommand")
        ]
        public event EventHandler<DataPagerCommandEventArgs> PagerCommand {
            add {
                Events.AddHandler(EventPagerCommand, value);
            }
            remove {
                Events.RemoveHandler(EventPagerCommand, value);
            }
        }
        
        protected override void CopyProperties(DataPagerField newField) {
            ((TemplatePagerField)newField).PagerTemplate = PagerTemplate;
            base.CopyProperties(newField);
        }

        protected override DataPagerField CreateField() {
            return new TemplatePagerField();
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        public override void HandleEvent(CommandEventArgs e) {
            DataPagerFieldItem item = null;
            DataPagerFieldCommandEventArgs cea = e as DataPagerFieldCommandEventArgs;
            if (cea != null) {
                item = cea.Item;
            }

            DataPagerCommandEventArgs pagerEventArgs = new DataPagerCommandEventArgs(this, _totalRowCount, e, item);
            OnPagerCommand(pagerEventArgs);

            if (pagerEventArgs.NewStartRowIndex != -1) {
                DataPager.SetPageProperties(pagerEventArgs.NewStartRowIndex, pagerEventArgs.NewMaximumRows, true);
            }

        }

        public override void CreateDataPagers(DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex) {
            _startRowIndex = startRowIndex;
            _maximumRows = maximumRows;
            _totalRowCount = totalRowCount;

            if (_pagerTemplate != null) {
                _pagerTemplate.InstantiateIn(container);
            }
        }

        /// <devdoc>
        /// <para>Raises the <see cref='System.Web.UI.WebControls.ListView.TotalRowCountAvailable'/>event of a <see cref='System.Web.UI.WebControls.ListView'/>.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected virtual void OnPagerCommand(DataPagerCommandEventArgs e) {
            EventHandler<DataPagerCommandEventArgs> handler = (EventHandler<DataPagerCommandEventArgs>)Events[EventPagerCommand];
            if (handler != null) {
                handler(this, e);
            }
            else {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.TemplatePagerField_UnhandledEvent, "PagerCommand"));
            }
        }
    }
}
