#!/bin/bash
mcs -r:System.Drawing -r:Mono.Cairo -pkg:gtk-sharp circles.cs sysdraw.cs

