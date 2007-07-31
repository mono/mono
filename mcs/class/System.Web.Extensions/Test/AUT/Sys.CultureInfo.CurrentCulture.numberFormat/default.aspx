<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<script runat="server">
   
    protected void Button1_Click(object sender, EventArgs e)
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("he-IL");
    }
</script>
<head id="Head1" runat="server">
    <title>Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnableScriptGlobalization="true"/>
        <div>
            <h3>numberFormat.[FormatType] field of Sys.CultureInfo.CurrentCulture object</h3>
            <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
            <asp:Label ID="Label2" runat="server" Text="Label"></asp:Label>
            <asp:Button ID="Button1" runat="server" Text="Culture Changer" OnClick="Button1_Click" />
            <br /><input id="TextBox" />
        </div>
    </form>
</body>
</html>
<script type="text/javascript">
    // Create the CurrentCulture object
    var cultureObject = Sys.CultureInfo.CurrentCulture;
    // Get the name field of the CurrentCulture object
    var cultureName = cultureObject.name;
    // Get the numberFormat object from the CurrentCulture object
    var nfObject = cultureObject.numberFormat;
    // Create an array of format types
    var myArray = ['CurrencyDecimalDigits', 'CurrencyDecimalSeparator', 'IsReadOnly', 'CurrencyGroupSizes',
                   'NumberGroupSizes', 'PercentGroupSizes', 'CurrencyGroupSeparator', 
                   'CurrencySymbol', 'NaNSymbol', 'CurrencyNegativePattern', 'NumberNegativePattern', 
                   'PercentPositivePattern', 'PercentNegativePattern', 'NegativeInfinitySymbol', 
                   'NegativeSign', 'NumberDecimalDigits', 'NumberDecimalSeparator', 
                   'NumberGroupSeparator', 'CurrencyPositivePattern', 'PositiveInfinitySymbol', 
                   'PositiveSign', 'PercentDecimalDigits', 'PercentDecimalSeparator', 
                   'PercentGroupSeparator', 'PercentSymbol', 'PerMilleSymbol', 
                   'NativeDigits', 'DigitSubstitution'];

    var result = 'Culture Name: ' + cultureName;
    var result2 = 'Culture Name: ' + cultureName;
    for (var i = 0, l = myArray.length; i < l; i++) {
        var arrayVal = myArray[i];
        if (typeof(arrayVal) !== 'undefined') {
            result += "<tr><td>" + arrayVal + "</td><td>" + eval("nfObject." + arrayVal) + '</td></tr>';
        }
    }
    var resultHeader = "<tr><td><b>FormatType</b></td><td><b>FormatValue</b></td></tr>"
    $get('Label1').innerHTML = "<table border=1>" + resultHeader + result + "</table>";
    $get('TextBox').value=result2 + ' CurrencySymbol: ' + nfObject.CurrencySymbol;
 
    var n = 99.987;
    $get('Label2').innerHTML = "<p/><h3>numberFormat Example: </h3>" + 
    n.localeFormat("C");

 </script>
