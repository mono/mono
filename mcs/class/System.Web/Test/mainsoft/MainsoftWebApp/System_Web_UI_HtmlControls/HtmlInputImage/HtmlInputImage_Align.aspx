<%@ Register TagPrefix="cc1" Namespace="GHTWebControls" Assembly="MainsoftWebApp" %>
<%@ Page Language="c#" AutoEventWireup="false" Codebehind="HtmlInputImage_Align.aspx.cs" Inherits="GHTTests.System_Web_dll.System_Web_UI_HtmlControls.HtmlInputImage_Align" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>HtmlInputImage_Align</title>
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
				<cc1:GHTSubTest id="GHTSubTest1" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Align_1">
					<INPUT id="Image1" type="image" src="/test.img" align="left" value="value of ImageInput1"
						name="InputImage1" runat="server"></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest2" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Align_2">
					<INPUT id="Image2" type="image" src="/test.img" align="notExist" value="value of ImageInput2"
						name="InputImage2" runat="server"></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest3" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Align_3">
					<INPUT id="Image3" type="image" src="/test.img" value="value of ImageInput3" name="InputImage3"
						runat="server"></cc1:GHTSubTest></P>
			<P>
				<cc1:GHTSubTest id="GHTSubTest4" runat="server" Height="56px" Width="553px" Description="HtmlInputImage_Align_4">
					<INPUT id="Image4" type="image" src="/test.img" align="right" value="value of ImageInput4"
						name="InputImage4" runat="server"></cc1:GHTSubTest></P>
		</form>
	</body>
</HTML>
