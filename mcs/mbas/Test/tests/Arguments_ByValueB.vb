'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Value:
'APV-1.4.0: By Default VB pass arguments by values, i.e if procedure is pass without mentioning
'		type then it take as ByVal type
'=============================================================================================
Imports System
Module APV1_4_0
	Sub F(p As Integer)
      p += 1
   End Sub 
   
   Sub Main()
      Dim a As Integer = 1
      F(a)
	if a<>1
		Throw new System.Exception("#A1, uncexcepted behaviour of Default VB pass arguments")
	end if
   End Sub 
End Module
'=============================================================================================