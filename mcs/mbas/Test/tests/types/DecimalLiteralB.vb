Imports System
Module DecimalLiteral
	Sub Main()
		Try
			Dim a As Decimal=True
			If a<>-1 Then
                                Console.WriteLine("DecimalLiteralB:Failed")
                        End If
                                
			
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
