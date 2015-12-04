//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.ComponentModel;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;
    using System.Windows;
    using System.Windows.Data;

    interface IFlowSwitchLink
    {
        [BrowsableAttribute(false)]
        ModelItem ModelItem
        { get; set; }

        [BrowsableAttribute(false)]
        FlowNode ParentFlowSwitch
        {
            get;
            set;
        }

        bool IsDefaultCase
        {
            get;
            set;
        }

        string CaseName
        {
            get;
        }

        object CaseObject
        {
            get;
        }

        MultiBinding CreateConnectorLabelTextBinding();
    }

    interface IFlowSwitchDefaultLink : IFlowSwitchLink
    {
        string DefaultCaseDisplayName
        {
            get;
            set;
        }
    }
}
