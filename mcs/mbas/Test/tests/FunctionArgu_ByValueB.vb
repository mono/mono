'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Value:
'APV-1.4.0: By Default VB pass arguments by values, i.e if procedure is pass without mentioning
'		type then it take as ByVal type
'=============================================================================================
Imports System
Module APV1_4_0
	Function F(p As Integer) as Integer
      p += 1
	return p
   End Function
   
   Sub Main()
      Dim a As Integer = 1
	Dim b as Integer = 0
      b = F(a)
	if b=a
		Throw new System.Exception("#A1, uncexcepted behaviour of Default VB pass arguments")
	end if
   End Sub 
End Module
'============================================================================================