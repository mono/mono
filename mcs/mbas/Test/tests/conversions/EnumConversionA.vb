'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module EnumConversion
	Enum e as Integer
		A = 9.90
		B 		
	End Enum
	Sub Main()
		Dim i as Double = e.B
		if i <> 11 then
			Throw new System.Exception("Enum Conversion is not working properly. Expected 11 but got "&i)
		End if 		
	End Sub
End Module
