Imports System
Module DoubleLiteral
	Sub Main()
		Try
			Dim a As Double=True
			If a<>-1 Then
                                Console.WriteLine("DoubleLiteralB:Failed")
                        End If
                                
			
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
