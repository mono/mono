REM LineNo: 27
REM ExpectedError: BC30002
REM ErrorMessage: Type 'Exception' is not defined.

'Imports System
Namespace NS1
	Module M
		Public a As Integer=20
	End Module
End Namespace
Namespace NS2
	Module M
		Dim a As Integer=30
		Sub Main()
			Dim a As Integer=40
			System.Console.WriteLine(a)
		 	Try	
				If a<>40 Then
					Throw New System.Exception("#A1:Namespace:Failed")
				End If		
				If NS1.M.a<>20 Then
                                       Throw New System.Exception("#A2:Namespace:Failed")
                                End If
				If NS2.M.a<>30 Then
                                        Throw New System.Exception("#A3:Namespace:Failed")
                                End If
			Catch e As Exception
				System.Console.WriteLine(e.Message)
			End Try
		End Sub
	End Module
End Namespace

