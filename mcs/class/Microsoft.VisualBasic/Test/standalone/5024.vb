Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test()
		'Begin Code
			On Error Resume Next
			Err.Raise (514, Nothing, "This is just a sample error", nothing, nothing)
			If Err.Number <> 514 then
				Throw New Exception ("#Raise01")
			End If
			If Err.Description <> "This is just a sample error" then
				Throw New Exception ("#Raise02")
			End If
		'End Code
	End Function
End Class
