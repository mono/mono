REM LineNo: 21
REM ExpectedError: BC30192
REM ErrorMessage: ParamArray parameter must be last in parameter list.

REM LineNo: 24
REM ExpectedError: BC30451
REM ErrorMessage: Name 'b' is not declared.

REM LineNo: 35
REM ExpectedError: BC30311
REM ErrorMessage: Value of type '1-dimensional array of Integer' cannot be converted to 'Integer'.

'============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: ParamArray:
'APR-1.0.0: ParamArray can be used only on the last argument of argument list. it allows us to 'pass an arbitrary list. It allows us to pass an arbitrary number of argument to the procedure 
'=============================================================================================
Imports System
Module PA_1_0_0
   Function F(ParamArray args() As Integer, ByVal b as Integer)as Boolean
      Dim a as integer
	a = args.Length
	if a=b
	return true
	else 
	return false
	end if
   End Function
   Sub Main()
      Dim a As Integer() = { 1, 2, 3 }
	Dim c as Integer
	c = a.Length
      Dim b as Boolean
	b= F(a,c)
	if b<>true
		Throw New System.Exception("#A1, Unexcepted Behaviour in F(a,c)")
	end if
   End Sub
End Module
'=============================================================================================