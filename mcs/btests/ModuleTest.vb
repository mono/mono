Imports System
Module ModuleTest
	Dim a As Integer=30
	Sub Main()
		Dim a As Integer
		If a<>0 Then	
			Throw New Exception("#A1: Module:Failed")
		End If	
		If ModuleTest.a<>30 Then
			Throw New Exception("#A2: Module: Failed")
		End If
	End Sub
End Module
