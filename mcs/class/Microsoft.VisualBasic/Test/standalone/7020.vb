Imports System
Imports System.IO
Imports Microsoft.VisualBasic

Public Class TestClass
    Public Function Test() As Integer
        Dim fput As Integer
        Dim item1 As Short
        Dim item2 As Integer
        Dim item3 As Single
        Dim item4 As Double
        Dim item5 As Decimal 
        Dim item6 As Byte
        Dim item7 As Boolean
        Dim item8 As Date
        Dim item9 As String

        Dim caughtException As Boolean

        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()


        strPathName = System.IO.Directory.GetCurrentDirectory() 
        strFileName = "/6748.txt"

        '// RecordNumber < 1 and not equal to -1.
        caughtException = False
        Try
            fput = FreeFile()

            FileOpen(fput, strPathName & strFileName, OpenMode.Random, , ,22) 

            FileGet(fput, item1, 1)
            FileGet(fput, item2, 2)
            FileGet(fput, item3, 3)
            FileGet(fput, item4, 4)
            FileGet(fput, item5, 5)
            FileGet(fput, item6, 6)
            FileGet(fput, item7, 7)
            FileGet(fput, item8, 8)
            'FileGet(fput, item9, 9)

	System.Console.WriteLine(item1)
	System.Console.WriteLine(item2)
	System.Console.WriteLine(item3)
	System.Console.WriteLine(item4)
	System.Console.WriteLine(item5)
	System.Console.WriteLine(item6)
	System.Console.WriteLine(item7)
	System.Console.WriteLine(item8)
	' System.Console.WriteLine(item9)
        Catch e As Exception
		
	 Return Err.Number
        End Try
	FileClose(fput)
        Return 0



    End Function

End Class

