Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'arr should contain atleast one negative value and one positive value
			Try
				Dim arr() as double = {-70000,-60000,-50000}
				Return IRR(arr, 0.1)
				Throw New Exception ("#IRR1")
			Catch ex As Exception
				If (ex.GetType ().ToString () <> "System.ArgumentException")
					Throw New Exception ("#IRR2")
				End If
			End Try
		'End Code
	End Function
End Class