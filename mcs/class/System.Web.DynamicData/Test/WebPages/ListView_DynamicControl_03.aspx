<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ListView_DynamicControl_03.aspx.cs" Inherits="ListView_DynamicControl_03" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html >
<head id="Head1" runat="server">
  <title>DynamicControl Sample</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <asp:DynamicDataManager ID="DynamicDataManager3" runat="server" AutoLoadForeignKeys="true" />
      <asp:ListView ID="ListView3" runat="server" DataSourceID="DynamicDataSource3">
        <LayoutTemplate>
          <div runat="server" id="itemPlaceholder" />
        </LayoutTemplate>
        <ItemTemplate>
	<div>
		<test:PokerDynamicControl runat="server" DataField="Char_Column" />
		<test:PokerDynamicControl runat="server" DataField="Byte_Column" />
		<test:PokerDynamicControl runat="server" DataField="Int_Column" />
		<test:PokerDynamicControl runat="server" DataField="Long_Column" />
		<test:PokerDynamicControl runat="server" DataField="Bool_Column" />
		<test:PokerDynamicControl runat="server" DataField="String_Column" />
		<test:PokerDynamicControl runat="server" DataField="Float_Column" />
		<test:PokerDynamicControl runat="server" DataField="Single_Column" />
		<test:PokerDynamicControl runat="server" DataField="Double_Column" />
		<test:PokerDynamicControl runat="server" DataField="Decimal_Column" />
		<test:PokerDynamicControl runat="server" DataField="SByte_Column" />
		<test:PokerDynamicControl runat="server" DataField="UInt_Column" />
		<test:PokerDynamicControl runat="server" DataField="ULong_Column" />
		<test:PokerDynamicControl runat="server" DataField="Short_Column" />
		<test:PokerDynamicControl runat="server" DataField="UShort_Column" />
		<test:PokerDynamicControl runat="server" DataField="DateTime_Column" />
		<test:PokerDynamicControl runat="server" DataField="FooEmpty_Column" />
		<test:PokerDynamicControl runat="server" DataField="Object_Column" />
		<test:PokerDynamicControl runat="server" DataField="ByteArray_Column" />
		<test:PokerDynamicControl runat="server" DataField="IntArray_Column" />
		<test:PokerDynamicControl runat="server" DataField="StringArray_Column" />
		<test:PokerDynamicControl runat="server" DataField="ObjectArray_Column" />
		<test:PokerDynamicControl runat="server" DataField="StringList_Column" />
		<test:PokerDynamicControl runat="server" DataField="Dictionary_Column" />
		<test:PokerDynamicControl runat="server" DataField="ICollection_Column" />
		<test:PokerDynamicControl runat="server" DataField="IEnumerable_Column" />
		<test:PokerDynamicControl runat="server" DataField="ICollectionByte_Column" />
		<test:PokerDynamicControl runat="server" DataField="IEnumerableByte_Column" />
		<test:PokerDynamicControl runat="server" DataField="ByteMultiArray_Column" />
		<test:PokerDynamicControl runat="server" DataField="BoolArray_Column" />
		<test:PokerDynamicControl runat="server" DataField="MaximumLength_Column1" />
		<test:PokerDynamicControl runat="server" DataField="MaximumLength_Column2" />
		<test:PokerDynamicControl runat="server" DataField="MaximumLength_Column3" />
		<test:PokerDynamicControl runat="server" DataField="MaximumLength_Column4" />
		<test:PokerDynamicControl runat="server" DataField="MaximumLength_Column5" />
        </div>
        </ItemTemplate>
      </asp:ListView>

	<test:DynamicDataSource runat="server" id="DynamicDataSource3" />
    </div>
    </form>
</body>
</html>
