'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBooleantoStringA
	Sub Main()
			Dim a as Boolean = True
			Dim b as String= a
			if b <> "True"
				Throw new System.Exception("Conversion of Boolean to String not working. Expected True but got " &b) 
			End if		
	End Sub
End Module
