Imports Microsoft.VisualBasic 
Imports System
Public Class TestClass
	Private Declare Function SQLCancel Lib "ODBC32.dll" _
	(ByVal hstmt As Integer) As Long
	Public Function Test() As String
		'Begin Code
			On Error Resume Next
			Dim myhandle As Integer
	 	  	' Call with invalid argument.
	 	  	Dim d as Integer = SQLCancel(myhandle)
			Return Err.LastDllError
		'End Code
	End Function
End Class
