

Imports Microsoft.VisualBasic

Public Class TestClass
    Public Function Test() As String
        Dim str1 As String
        Dim strFileName As String
        Dim strPathName As String
        Dim curDir As String =  System.IO.Directory.GetCurrentDirectory()
        Dim i As Integer

        strPathName = "/home/banirban/sudha/tests/data/"
        strFileName = "hidden.txt"

        'check if directory has ReadOnly files
        str1 = Dir(strPathName, vbNormal)
        'If (str1 <> Dir(strPathName & str1, vbNormal)) Then Return "failed to locate a ReadOnly file"
        'If str1 = "" Then Return "failed to find a readOnly file"
	System.Console.WriteLine(str1)
	System.Console.WriteLine(Dir())
	System.Console.WriteLine(Dir())
	System.Console.WriteLine(Dir())
	'do while str1 <> ""
		'System.Console.WriteLine(str1)
		'str1 = Dir()
	'loop

        Return "success"

    End Function
End Class
