//------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /// <include file='doc\Constants.uex' path='docs/doc[@for="ObjectListViewMode"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum ObjectListViewMode
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ObjectListViewMode.List"]/*' />
        List,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ObjectListViewMode.Commands"]/*' />
        Commands,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ObjectListViewMode.Details"]/*' />
        Details
    };

    /// <include file='doc\Constants.uex' path='docs/doc[@for="BooleanOption"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum BooleanOption
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="BooleanOption.NotSet"]/*' />
        NotSet = -1,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="BooleanOption.False"]/*' />
        False,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="BooleanOption.True"]/*' />
        True,
    };

    /// <include file='doc\Constants.uex' path='docs/doc[@for="FontSize"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum FontSize
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="FontSize.NotSet"]/*' />
        NotSet,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="FontSize.Normal"]/*' />
        Normal,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="FontSize.Small"]/*' />
        Small,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="FontSize.Large"]/*' />
        Large
    };

    /// <include file='doc\Constants.uex' path='docs/doc[@for="Alignment"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum Alignment
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Alignment.NotSet"]/*' />
        NotSet,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Alignment.Left"]/*' />
        Left,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Alignment.Center"]/*' />
        Center,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Alignment.Right"]/*' />
        Right
    }

    /// <include file='doc\Constants.uex' path='docs/doc[@for="Wrapping"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum Wrapping
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Wrapping.NotSet"]/*' />
        NotSet,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Wrapping.Wrap"]/*' />
        Wrap,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Wrapping.NoWrap"]/*' />
        NoWrap
    }

    /// <include file='doc\Constants.uex' path='docs/doc[@for="ListDecoration"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum ListDecoration
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListDecoration.None"]/*' />
        None,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListDecoration.Bulleted"]/*' />
        Bulleted,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListDecoration.Numbered"]/*' />
        Numbered
    }

    /// <include file='doc\Constants.uex' path='docs/doc[@for="ListSelectType"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum ListSelectType
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListSelectType.DropDown"]/*' />
        DropDown,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListSelectType.ListBox"]/*' />
        ListBox,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListSelectType.Radio"]/*' />
        Radio,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListSelectType.MultiSelectListBox"]/*' />
        MultiSelectListBox,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="ListSelectType.CheckBox"]/*' />
        CheckBox
    }

    /// <include file='doc\Constants.uex' path='docs/doc[@for="FormMethod"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum FormMethod
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="FormMethod.Get"]/*' />
        Get,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="FormMethod.Post"]/*' />
        Post,
    }

    /// <include file='doc\Constants.uex' path='docs/doc[@for="CommandFormat"]/*' />
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public enum CommandFormat
    {
        /// <include file='doc\Constants.uex' path='docs/doc[@for="CommandFormat.Button"]/*' />
        Button,
        /// <include file='doc\Constants.uex' path='docs/doc[@for="CommandFormat.Link"]/*' />
        Link,
    }

    /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class Constants
    {
        internal const String ErrorStyle = "error";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.FormIDPrefix"]/*' />
        public static readonly String FormIDPrefix = "#";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.UniqueFilePathSuffixVariableWithoutEqual"]/*' />
        public static readonly String UniqueFilePathSuffixVariableWithoutEqual = "__ufps";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.UniqueFilePathSuffixVariable"]/*' />
        public static readonly String UniqueFilePathSuffixVariable = UniqueFilePathSuffixVariableWithoutEqual + '=';
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.PagePrefix"]/*' />
        public static readonly String PagePrefix = "__PG_";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.EventSourceID"]/*' />
        public static readonly String EventSourceID = "__ET";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.EventArgumentID"]/*' />
        public static readonly String EventArgumentID = "__EA";

        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.HeaderTemplateTag"]/*' />
        public static readonly String HeaderTemplateTag = "HeaderTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.FooterTemplateTag"]/*' />
        public static readonly String FooterTemplateTag = "FooterTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.ItemTemplateTag"]/*' />
        public static readonly String ItemTemplateTag = "ItemTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.AlternatingItemTemplateTag"]/*' />
        public static readonly String AlternatingItemTemplateTag = "AlternatingItemTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.SeparatorTemplateTag"]/*' />
        public static readonly String SeparatorTemplateTag = "SeparatorTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.ContentTemplateTag"]/*' />
        public static readonly String ContentTemplateTag = "ContentTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.LabelTemplateTag"]/*' />
        public static readonly String LabelTemplateTag = "LabelTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.ItemDetailsTemplateTag"]/*' />
        public static readonly String ItemDetailsTemplateTag = "ItemDetailsTemplate";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.ScriptTemplateTag"]/*' />
        public static readonly String ScriptTemplateTag = "ScriptTemplate";

        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.SymbolProtocol"]/*' />
        public static readonly String SymbolProtocol = "symbol:";

        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.SelectionListSpecialCharacter"]/*' />
        public static readonly char SelectionListSpecialCharacter = '*';

        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.DefaultSessionsStateHistorySize"]/*' />
        public static readonly int DefaultSessionsStateHistorySize = 6;

        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.OptimumPageWeightParameter"]/*' />
        public static readonly String OptimumPageWeightParameter = "optimumPageWeight";
        /// <include file='doc\Constants.uex' path='docs/doc[@for="Constants.ScreenCharactersHeightParameter"]/*' />
        public static readonly String ScreenCharactersHeightParameter = "screenCharactersHeight";
    }
}
