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

