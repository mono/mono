Imports System
Namespace N1
	Namespace N2
		Class C1
			Public Shared a As Integer
			Public Shared b As Integer
			Public Shared Function F() As Integer
			 	Try
					N1.N2.C1.a=20
				Catch e As Exception
					Console.WriteLine("error resolving a fully qualified name")
				End Try
				b=30
				Return 47
			End Function
		End Class
	End Namespace
End Namespace
Module QualifiedNames
	Sub Main()
		If N1.N2.C1.F()<>47 Then
			Throw New Exception("#A1:QualifiedNames:Failed-Error accessing function using fully qualified names")
		End If
		If N1.N2.C1.a<>20 Then
			Throw New Exception("#A2:QualifiedNames:Failed-Error accessing variables using fully qualified names")
		End If
		If N1.N2.C1.b<>30 Then
			Throw New Exception("#A3:QualifiedNames:Failed-Error accessing variables using fully qualified names")
		End If
	End Sub
End Module
