Imports System

Module AssignmentStatementsA

    Sub main()
        Dim i As Int

        i = 2
	Console.WriteLine("Value of i is {0}", i)
	
        If i <> 2 Then
            Throw New Exception("#ASA1 - Assignment Statement failed")
        End If

    End Sub

End Module
