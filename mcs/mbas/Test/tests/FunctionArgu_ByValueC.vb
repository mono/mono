'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Value:
'APV-1.1.0: If variable elements is of value type, i.e. it contains only a value then procedure '		cannot change the variable or any of its members
'=============================================================================================

Imports System
Module APV1_1_0
	Function F(p As String) As String
	p = "Sinha"
	return p
   End Function
   
   Sub Main()
      Dim a As String = "Manish"
	Dim b as String = ""
      b = F(a)
	if a=b
		Throw New System.Exception("#A1, Unexcepted behaviour in string of APV1_1_0")
	end if
   End Sub 
End Module
'===========================================================================================