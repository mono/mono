
'
' Author:
'   Sathya Sudha (ksathyasudha@novell.com)
' Copyright (C) 2004 Novell, Inc (http://www.novell.com)
'
' Permission is hereby granted, free of charge, to any person obtaining
' a copy of this software and associated documentation files (the
' "Software"), to deal in the Software without restriction, including
' without limitation the rights to use, copy, modify, merge, publish,
' distribute, sublicense, and/or sell copies of the Software, and to
' permit persons to whom the Software is furnished to do so, subject to
' the following conditions:
' 
' The above copyright notice and this permission notice shall be
' included in all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
' EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
' MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
' NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
' OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
' WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As String
        Dim str1 As String
        Dim strFileName As String
        Dim strPathName As String
        Dim curDir As String =  System.IO.Directory.GetCurrentDirectory()
        Dim i As Integer
        strPathName = "./data/"
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
