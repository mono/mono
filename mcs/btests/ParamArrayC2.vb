REM LineNo: 13
REM ExpectedError: BC30667
REM ErrorMessage: ParamArray parameters must be declared 'ByVal'.

'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Param Array:
'APR-1.1.1:If ParamArray modifier is precied by ByRef modifier the it produces compiler error 
'=============================================================================================
Imports System
Module PA_1_1_1
   Sub F(ParamArray ByRef args() As Integer)
	Dim a as Integer
	a = args.Length
	if a=0
		Throw New System.Exception("#A1, Unexcepted behavoiur of PARAM ARRAY")
	end if

   End Sub
   Sub Main()
      Dim a As Integer() = { 1, 2, 3 }
      F(a)
      F(10, 20, 30, 40)
   End Sub
End Module
'================================================================================