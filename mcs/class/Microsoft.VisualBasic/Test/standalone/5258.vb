Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass 
	Public Function Test() As String
		'Begin Code
			dim result as String
			FileOpen (1, "5258.txt", OpenMode.Input)
			result = LineInput (1)
			result = LineInput (1)
			Try
				result = LineInput (1)
			Catch e As Exception
				Return e.GetType ().ToString ()
			End Try
			Throw New Exception ("Expected EndOfStreamException but got no exception")
		'End Code
	End Function
End Class
