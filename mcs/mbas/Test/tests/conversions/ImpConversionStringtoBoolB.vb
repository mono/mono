'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoBooleanA
	Sub Main()
			Dim a as Boolean
			Dim b as String= "False"
			a = b
			if a <> False
				Throw new System.Exception("Conversion of String to Boolean not working. Expected False but got " &a) 
			End if		
	End Sub
End Module
