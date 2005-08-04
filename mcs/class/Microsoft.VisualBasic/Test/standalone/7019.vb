
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
Imports System
Imports System.IO
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As Integer
        Dim fput As Integer
        Dim item As String = "Hello world"
        Dim caughtException As Boolean
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        Try
        strPathName = System.IO.Directory.GetCurrentDirectory() 
        strFileName = "/6748.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        '// RecordNumber < 1 and not equal to -1.
        caughtException = False
            fput = FreeFile()
            FileOpen(fput, strPathName & strFileName, OpenMode.Random, , ,22) 
            FilePut(fput, item, 1)
            FilePut(fput, item, 2)
	    FileClose(fput)
            fput = FreeFile()
            FileOpen(fput, strPathName & strFileName, OpenMode.Random, , ,22) 
            FileGet(fput, item, 1)
	    System.Console.WriteLine(item)
            FileGet(fput, item, 2)
	    System.Console.WriteLine(item)
        Catch e As Exception
		
	 Return Err.Number
        End Try
	FileClose(fput)
        Return 0
    End Function
End Class
