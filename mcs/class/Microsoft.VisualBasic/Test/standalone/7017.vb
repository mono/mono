Imports Microsoft.VisualBasic
Imports System

Public Class TestClass
    Public Function Test() As Integer
        Dim str1 As Integer
        Dim fn As Integer
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()

        '// create a file for the test
        Dim SourceFile As String
        SourceFile = System.IO.Directory.GetCurrentDirectory() + "/invalid.file"
	Try 
        str1= FileLen(SourceFile)

	Catch e As System.IO.FileNotFoundException 
		Console.WriteLine("Exception raised")
		return Err.Number
	End Try
	return str1
    End Function
End Class

