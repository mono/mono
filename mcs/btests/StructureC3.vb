REM LineNo: 19
REM ExpectedError: BC30435
REM ErrorMessage: Members in a Structure cannot be declared 'Protected'.

REM LineNo: 21
REM ExpectedError: BC31047
REM ErrorMessage: Protected types can only be declared inside of a class.

REM LineNo: 24
REM ExpectedError: BC31067
REM ErrorMessage: Method in a structure cannot be declared 'Protected' or 'Protected Friend'.

REM LineNo: 27
REM ExpectedError: BC30435
REM ErrorMessage: Members in a Structure cannot be declared 'Protected'.

Structure S
	Dim a as String
	Protected Const b as integer = 10

	protected Class c
	end class

	protected sub f(l as long)
	end sub

	Protected Structure S1
		dim g as string
	End Structure
End Structure

Module M
	Sub Main()
		dim x as S
	End Sub
End Module
