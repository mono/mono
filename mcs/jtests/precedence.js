var expected, result;

print ("Test precedence of * over + (#1):");
result = 3 + 2 * 2;
expected = 7;
if (expected != result)
    print ("FAILED.");
else
    print ("SUCCEED.");


print ("Test precedence of * over + (#2):");
result = 3 * 2 + 2;
expected = 8;
if (expected != result)
    print ("FAILED.");
else
    print ("SUCCEED.");


print ("Test precedence of / over + (#3):");
result = 3 + 2 / 2;
expected = 4;
if (expected != result)
    print ("FAILED.");
else
    print ("SUCCEED.");


print ("Test precedence of / over + (#4):");
result = 3 / 2 + 2;
expected = 3.5;
if (expected != result)
    print ("FAILED.");
else
    print ("SUCCEED.");


print ("Test precedence of * over - (#5)");
result = 3 - 2 * 2;
expected = -1;
if (expected != result)
    print ("FAILED.");
else
    print ("SUCCEED.");
