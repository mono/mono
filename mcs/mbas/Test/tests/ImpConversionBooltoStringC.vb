'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBooleantoStringC
	Sub Main()
			Dim a as Boolean = True
			Dim b as String = "111" + a
			if b <> "110"
				Throw new System.Exception("Concat of Boolean & String not working. Expected 110 but got " &b) 
			End if		
	End Sub
End Module
