Imports System
Module IntegerLiteral
	Sub Main()
	Try				'Assigning boolean to integer
		Dim a As Integer
		a=True
		If a<>-1 Then
			Console.WriteLine("IntegerLiteralA:Failed")
		End If
	Catch e As Exception
		Console.WriteLine(e.Message)
	End Try
					' Assigning float to integer
					' if option strict is off this 
					' Test case should pass
	 Try
                Dim a As Integer	
                a=1.23
                If a<>1 Then
                        Console.WriteLine("IntegerLiteralA:Failed")
                End If
        Catch e As Exception
                Console.WriteLine(e.Message)
        End Try

	End Sub
End Module
