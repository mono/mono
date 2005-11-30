echo on
@echo "processing Parser.jay..."
"%1\..\..\jay\jay.exe" -ct < "%1..\..\jay\skeleton.cs" "%1Mono.Data.SqlExpressions\Parser.jay" > "%1Mono.Data.SqlExpressions\Parser.cs"
