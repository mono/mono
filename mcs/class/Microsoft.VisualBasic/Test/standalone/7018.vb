Imports System
Imports Microsoft.VisualBasic

Public Class TestClass
    Public Function Test() As String
        'BeginCod
        Dim fn As Integer    
    
        '// create a file for the test
        Dim SourceFile As String
        Dim DestFile As String
        DestFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
        SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/6499.txt"
        FileCopy(SourceFile, DestFile)

        fn = freefile
        FileOpen(fn, "data/textfile.txt", OpenMode.Input)
        FileClose(fn)
        Return "success"
    End Function
End Class
