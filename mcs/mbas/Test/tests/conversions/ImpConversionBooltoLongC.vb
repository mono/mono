'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBooleantoLongC
	Sub Main()
			Dim a as Boolean = True
			Dim b as Long = 111 + a
			if b <> 110
				Throw new System.Exception("Addition of Boolean & Long not working. Expected 110 but got " &b) 
			End if		
	End Sub
End Module
