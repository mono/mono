//------------------------------------------------------------------------------
// <copyright file="WmlObjectListAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlObjectListAdapter provides WML rendering of Object List control.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlObjectListAdapter : WmlControlAdapter
    {
        private const String _backToList = "__back";
        private const String _showMenu = "__menu";
        private const String _showDetails = "__details";
        private const String _showMoreFormat = "__more$({0})";
        private const String _showMoreFormatAnchor = "__more{0}";
        private const String _showMore = "__more";
        private const int _modeItemMenu = 1;
        private const int _modeItemDetails = 2;

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.Control"]/*' />
        protected new ObjectList Control
        {
            get
            {
                return (ObjectList)base.Control;
            }
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.OnPreRender"]/*' />
        public override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            switch (Control.ViewMode)
            {
                case ObjectListViewMode.List:
                    SecondaryUIMode = NotSecondaryUI;
                    break;

                case ObjectListViewMode.Commands:
                    SecondaryUIMode = _modeItemMenu;
                    break;

                case ObjectListViewMode.Details:
                    SecondaryUIMode = _modeItemDetails;
                    break;
            }

            if(Control.MobilePage.ActiveForm == Control.Form && 
                Control.Visible && 
                Control.ViewMode == ObjectListViewMode.Commands &&
                Control.Items.Count > 0)
            {
                Control.PreShowItemCommands (Control.SelectedIndex);
            }
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            switch (Control.ViewMode)
            {
                case ObjectListViewMode.List:
                    if (Control.HasControls())
                    {
                        writer.BeginCustomMarkup();
                        RenderChildren(writer);
                        writer.EndCustomMarkup();
                        return;
                    }
                    RenderItemsList(writer);
                    break;

                case ObjectListViewMode.Commands:
                    RenderItemMenu(writer, Control.Selection);
                    break;

                case ObjectListViewMode.Details:
                    if (Control.Selection.HasControls())
                    {
                        writer.BeginCustomMarkup();
                        Control.Selection.RenderChildren(writer);
                        writer.EndCustomMarkup();
                        return;
                    }
                    RenderItemDetails(writer, Control.Selection);
                    break;
            }
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.CreateTemplatedUI"]/*' />
        public override void CreateTemplatedUI(bool doDataBind)
        {
            if (Control.ViewMode == ObjectListViewMode.List)
            {
                Control.CreateTemplatedItemsList(doDataBind);
            }
            else
            {
                Control.CreateTemplatedItemDetails(doDataBind);
            }
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.RenderItemsList"]/*' />
        protected virtual void RenderItemsList(WmlMobileTextWriter writer)
        {
            bool rendersAcceptsInline = Device.RendersWmlDoAcceptsInline;
            bool rendersSelectsAsMenuCards = Device.RendersWmlSelectsAsMenuCards;
            bool rendersBreaksAfterAnchor = Device.RendersBreaksAfterWmlAnchor;
            int pageStart = Control.FirstVisibleItemIndex;
            int pageSize = Control.VisibleItemCount;
            ObjectListItemCollection items = Control.Items;

            if (pageSize == 0 || items.Count == 0)
            {
                return;
            }

            bool hasDefaultCommand = HasDefaultCommand();
            bool onlyHasDefaultCommand = OnlyHasDefaultCommand();
            bool requiresSecondScreen = HasItemDetails() || (!onlyHasDefaultCommand && HasCommands());
            bool itemRequiresHyperlink = requiresSecondScreen || hasDefaultCommand;

            writer.EnterLayout(Style);

            int[] tableFieldIndices = null;
            if (ShouldRenderAsTable() && (tableFieldIndices = Control.TableFieldIndices).Length != 0)
            {
                writer.BeginCustomMarkup();
                int fieldCount = tableFieldIndices.Length;
                writer.Write("<table columns=\"");
                writer.Write(fieldCount.ToString(CultureInfo.InvariantCulture));
                writer.WriteLine("\">");

                if (ShouldRenderTableHeaders())
                {
                    writer.Write("<tr>");
                    foreach (int fieldIndex in tableFieldIndices)
                    {
                        writer.Write("<td>");
                        writer.RenderText(Control.AllFields[fieldIndex].Title);
                        writer.Write("</td>");
                    }
                    writer.WriteLine("</tr>");
                }

                for (int i = 0; i < pageSize; i++)
                {
                    ObjectListItem item = items[pageStart + i];
                    writer.Write("<tr>");
                    for (int field = 0; field < fieldCount; field++)
                    {
                        writer.Write("<td>");
                        if (field == 0 && itemRequiresHyperlink)
                        {
                            RenderPostBackEvent(writer, 
                                requiresSecondScreen ?
                                    String.Format(CultureInfo.InvariantCulture, _showMoreFormatAnchor, item.Index) :
                                    item.Index.ToString(CultureInfo.InvariantCulture),
                                GetDefaultLabel(GoLabel),
                                false,
                                item[tableFieldIndices[0]],
                                false,
                                WmlPostFieldType.Raw);
                        }
                        else
                        {
                            writer.RenderText(item[tableFieldIndices[field]]);
                        }
                        writer.Write("</td>");
                    }
                    writer.WriteLine("</tr>");
                }
                writer.WriteLine("</table>");
                writer.EndCustomMarkup();
            }
            else
            {
                int labelFieldIndex = Control.LabelFieldIndex;
                ObjectListField labelField = Control.AllFields[labelFieldIndex];

                writer.EnterFormat(Style);
                for (int i = 0; i < pageSize; i++)
                {
                    ObjectListItem item = items[pageStart + i];
                    if (itemRequiresHyperlink)
                    {
                        RenderPostBackEvent(writer, 
                            requiresSecondScreen ?
                                String.Format(CultureInfo.InvariantCulture, _showMoreFormatAnchor, item.Index) :
                                item.Index.ToString(CultureInfo.InvariantCulture),
                            GetDefaultLabel(GoLabel),
                            false,
                            item[labelFieldIndex],
                            true,
                            WmlPostFieldType.Raw);
                    }
                    else
                    {
                        writer.RenderText(item[labelFieldIndex], true);
                    }
                }
                writer.ExitFormat(Style);
            }

            writer.ExitLayout(Style);
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.RenderItemMenu"]/*' />
        protected virtual void RenderItemMenu(WmlMobileTextWriter writer, ObjectListItem item)
        {
            bool requiresDetails = HasItemDetails();

            String detailsCommandText = Control.DetailsCommandText.Length == 0 ?
                SR.GetString(SR.WmlObjectListAdapterDetails) :
                Control.DetailsCommandText;
            String softkeyLabel = detailsCommandText.Length <= Device.MaximumSoftkeyLabelLength ?
                detailsCommandText :
                null;
            Style commandStyle = Control.CommandStyle;
            if (commandStyle.Alignment == Alignment.NotSet)
            {
                commandStyle.Alignment = Alignment.Left;
            }
            writer.EnterStyle(commandStyle);
            if (requiresDetails)
            {
                RenderPostBackEvent(writer, _showDetails,
                        softkeyLabel, true, detailsCommandText, true, WmlPostFieldType.Raw);
            }

            ObjectListCommandCollection commands = Control.Commands;
            foreach (ObjectListCommand command in commands)
            {
                RenderPostBackEvent(writer, command.Name, 
                    GetDefaultLabel(GoLabel), false, command.Text, true, WmlPostFieldType.Raw);
            }
            writer.ExitStyle(commandStyle);
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.RenderItemDetails"]/*' />
        protected virtual void RenderItemDetails(WmlMobileTextWriter writer, ObjectListItem item)
        {
            String backCommandText = Control.BackCommandText.Length == 0 ?
                GetDefaultLabel(BackLabel) :
                Control.BackCommandText;
            String softkeyLabel = backCommandText.Length <= Device.MaximumSoftkeyLabelLength ?
                backCommandText :
                null;

            Style labelStyle = Control.LabelStyle;
            writer.EnterStyle(labelStyle);
            writer.RenderText(item[Control.LabelFieldIndex], true);
            writer.ExitStyle(labelStyle);

            writer.EnterStyle(Style);
            IObjectListFieldCollection fields = Control.AllFields;
            int fieldIndex = 0;
            foreach (ObjectListField field in fields)
            {
                if (field.Visible)
                {
                    String displayText = String.Format(CultureInfo.InvariantCulture, "{0}: {1}", field.Title, item[fieldIndex]);
                    writer.RenderText(displayText, true);
                }
                fieldIndex++;
            }
            RenderPostBackEvent(writer, _backToList, softkeyLabel, true, backCommandText, true);
            writer.ExitStyle(Style);
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.HandlePostBackEvent"]/*' />
        public override bool HandlePostBackEvent(String eventArgument)
        {
            switch (Control.ViewMode)
            {
                case ObjectListViewMode.List:

                    // DCR 2493 - raise a selection event, and only continue
                    // handling if asked to.

                    if (eventArgument.StartsWith(_showMore, StringComparison.Ordinal))
                    {
                        int itemIndex = ParseItemArg(eventArgument);

                        if (Control.SelectListItem(itemIndex, true))
                        {
                            if (HasCommands())
                            {
                                Control.ViewMode = ObjectListViewMode.Commands;
                            }
                            else
                            {
                                Control.ViewMode = ObjectListViewMode.Details;
                            }
                        }
                    }
                    else
                    {
                        int itemIndex = -1;
                        try
                        {
                            itemIndex = Int32.Parse(eventArgument, CultureInfo.InvariantCulture);
                        }
                        catch (System.FormatException)
                        {
                            throw new Exception (SR.GetString(SR.ObjectListAdapter_InvalidPostedData));
                        }
                        if (Control.SelectListItem(itemIndex, false))
                        {
                            Control.RaiseDefaultItemEvent(itemIndex);
                        }
                    }
                    return true;

                case ObjectListViewMode.Commands:

                    if (eventArgument == _backToList)
                    {
                        Control.ViewMode = ObjectListViewMode.List;
                        return true;
                    }
                    else if (eventArgument == _showDetails)
                    {
                        Control.ViewMode = ObjectListViewMode.Details;
                        return true;
                    }
                    break;

                case ObjectListViewMode.Details:

                    if (eventArgument == _backToList)
                    {
                        Control.ViewMode = ObjectListViewMode.List;
                        return true;
                    }
                    else if (eventArgument == _showMenu)
                    {
                        Control.ViewMode = ObjectListViewMode.Commands;
                        return true;
                    }
                    break;
            }

            return false;
        }

        private static int ParseItemArg(String arg)
        {
            return Int32.Parse(arg.Substring(_showMore.Length), CultureInfo.InvariantCulture);
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.ShouldRenderAsTable"]/*' />
        protected virtual bool ShouldRenderAsTable()
        {
            int avgFieldWidth = 10; // an arbitrary estimate.
            return Device.ScreenCharactersHeight > 4 && VisibleTableFieldsCount * avgFieldWidth < Device.ScreenCharactersWidth;
        }

        private bool ShouldRenderTableHeaders()
        {
            return true;
        }

        private BooleanOption _hasItemDetails = BooleanOption.NotSet;
        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.HasItemDetails"]/*' />
        protected bool HasItemDetails()
        {
            if(Control.Items.Count == 0)
            {
                return false;
            }
            if (_hasItemDetails == BooleanOption.NotSet)
            {
                // Calculate how many visible fields are shown in list view.

                int visibleFieldsInListView;
                int[] tableFieldIndices;
                if (ShouldRenderAsTable() && (tableFieldIndices = Control.TableFieldIndices).Length != 0)
                {
                    visibleFieldsInListView = 0;
                    for (int i = 0; i < tableFieldIndices.Length; i++)
                    {
                        if (Control.AllFields[tableFieldIndices[i]].Visible)
                        {
                            visibleFieldsInListView++;
                        }
                    }
                }
                else
                {
                    visibleFieldsInListView = Control.AllFields[Control.LabelFieldIndex].Visible ?
                                                    1 : 0;
                }


                // Calculate the number of visible fields.

                _hasItemDetails = BooleanOption.False;
                int visibleFieldCount = 0;
                foreach (ObjectListField field in Control.AllFields)
                {
                    if (field.Visible)
                    {
                        visibleFieldCount++;
                        if (visibleFieldCount > visibleFieldsInListView)
                        {
                            _hasItemDetails = BooleanOption.True;
                            break;
                        }
                    }
                }
            }

            return _hasItemDetails == BooleanOption.True;
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.HasCommands"]/*' />
        protected bool HasCommands()
        {
            return Control.Commands.Count > 0;
        }

        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.HasDefaultCommand"]/*' />
        protected bool HasDefaultCommand()
        {
            return Control.DefaultCommand.Length > 0;
        }

        private BooleanOption _onlyHasDefaultCommand = BooleanOption.NotSet;
        /// <include file='doc\WmlObjectListAdapter.uex' path='docs/doc[@for="WmlObjectListAdapter.OnlyHasDefaultCommand"]/*' />
        protected bool OnlyHasDefaultCommand()
        {
            if (_onlyHasDefaultCommand == BooleanOption.NotSet)
            {
                String defaultCommand = Control.DefaultCommand;
                if (defaultCommand.Length > 0)
                {
                    int commandCount = Control.Commands.Count;
                    if (commandCount == 0 || 
                        (commandCount == 1 && 
                            String.Compare(defaultCommand, Control.Commands[0].Name, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        _onlyHasDefaultCommand = BooleanOption.True;
                    }
                    else
                    {
                        _onlyHasDefaultCommand = BooleanOption.False;
                    }
                }
                else
                {
                    _onlyHasDefaultCommand = BooleanOption.False;
                }
            }

            return _onlyHasDefaultCommand == BooleanOption.True;
        }
        
        // This appears in both Html and Wml adapters, is used in
        // ShouldRenderAsTable().  In adapters rather than control
        // because specialized rendering method.
        private int _visibleTableFieldsCount = -1;
        private int VisibleTableFieldsCount
        {
            get
            {
                if (_visibleTableFieldsCount == -1)
                {
                    int[] tableFieldIndices = Control.TableFieldIndices;
                    _visibleTableFieldsCount = 0;
                    for (int i = 0; i < tableFieldIndices.Length; i++)
                    {
                        if (Control.AllFields[tableFieldIndices[i]].Visible)
                        {
                            _visibleTableFieldsCount++;
                        }
                    }
                }
                return _visibleTableFieldsCount;
            }
        }


    }
}








