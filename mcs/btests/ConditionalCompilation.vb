Imports System
Module ConditionalCompilation
	Sub Main()
		Try
			'Using syntatically wrong statements inside a #If block that does not satisfy condition
			#If False
				Console.WriteLine("Hello)
			#End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module



