Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Public Function Test() As String
		'Begin Code
			'arr should contain atleast one negative value and one positive value
			Try
				Dim arr() as double = {-70000,-34000,-20000}
				Dim d As double = IRR(arr, 0.1)
				Throw New Exception ("#IRR1")
			Catch ex As Exception
				If ex.GetType ().ToString () <> "System.ArgumentException" then
					Throw New Exception ("#IRR2")
				End If
			End Try
		'End Code
	End Function
End Class