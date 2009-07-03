<%@ Control Language="C#" CodeFile="Integer_Edit.ascx.cs" Inherits="Integer_EditField" %>

<asp:TextBox ID="TextBox1" runat="server" Text="<%# FieldValueEditString %>" Columns="10" CssClass="droplist"></asp:TextBox>

<asp:RequiredFieldValidator runat="server" ID="RequiredFieldValidator1" CssClass="droplist" ControlToValidate="TextBox1" Display="Dynamic" Enabled="false" />
<asp:CompareValidator runat="server" ID="CompareValidator1" CssClass="droplist" ControlToValidate="TextBox1" Display="Dynamic"
    Operator="DataTypeCheck" Type="Integer"/>
<asp:RegularExpressionValidator runat="server" ID="RegularExpressionValidator1" CssClass="droplist" ControlToValidate="TextBox1" Display="Dynamic" Enabled="false" />
<asp:RangeValidator runat="server" ID="RangeValidator1" CssClass="droplist" ControlToValidate="TextBox1" Type="Integer"
    Enabled="false" EnableClientScript="true" MinimumValue="0" MaximumValue="100" Display="Dynamic" />
<asp:DynamicValidator runat="server" ID="DynamicValidator1" CssClass="droplist" ControlToValidate="TextBox1" Display="Dynamic" />