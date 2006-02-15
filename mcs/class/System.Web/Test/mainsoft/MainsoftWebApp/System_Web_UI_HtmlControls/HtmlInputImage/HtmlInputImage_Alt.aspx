<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HtmlInputImage_Alt.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_HtmlControls.HtmlInputImage_Alt" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HtmlInputImage_Alt</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="Visual Basic .NET 7.1" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script LANGUAGE="JavaScript">
        function ScriptTest()
        {
            var theform;
		    if (window.navigator.appName.toLowerCase().indexOf("netscape") > -1) {
			    theform = document.forms["Form1"];
		    }
		    else {
			    theform = document.Form1;
		    }
        }
		</script>
	</HEAD>
	<body>
		<form id="Form1" method="post" runat="server">
			<P>
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Alt_1">
					<INPUT id="Image1" type="image" alt="alt of ImageInput1" src="/test.img" value="value of ImageInput1"
						name="InputImage1" runat="server"></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="Ghtsubtest2" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Alt_2">
					<INPUT id="Image2" type="image" alt="" src="/test.img" value="value of ImageInput1" name="InputImage1"
						runat="server"></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="Ghtsubtest3" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Alt_3">
					<INPUT id="Image3" type="image" alt="alt of ImageInput3" src="/test.img" value="value of ImageInput1"
						name="InputImage1" runat="server"></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="Ghtsubtest4" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Alt_4">
					<INPUT id="Image4" type="image" alt="alt of ImageInput4" src="/test.img" value="value of ImageInput1"
						name="InputImage1" runat="server"></cc1:GHTSubTest></P>
		</form>
		<br>
		<br>
	</body>
</HTML>
