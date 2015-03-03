#!/bin/bash
mcs create-keyboards.cs ../System.Windows.Forms/KeyboardLayouts.cs /r:System.Windows.Forms.dll /out:create-keyboards.exe
mono create-keyboards.exe
