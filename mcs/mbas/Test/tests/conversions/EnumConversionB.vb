'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module EnumConversion
	Enum e as Integer
		A = -9.90
		B 		
		C = B
		D
	End Enum
	Sub Main()
		Dim i as Double = e.D
		if i <> -8 then
			Throw new System.Exception("Enum Conversion is not working properly. Expected -8 but got "&i)
		End if 		
	End Sub
End Module
