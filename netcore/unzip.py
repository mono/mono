#!/usr/bin/env python
import zipfile
import sys

print(sys.argv[1])
zip_ref = zipfile.ZipFile(sys.argv[1], 'r')
zip_ref.extractall()
zip_ref.close()