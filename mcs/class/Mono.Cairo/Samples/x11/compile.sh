#!/bin/bash
mcs -r:System.Drawing -r:Mono.Cairo arc.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo arcneg.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo clip.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo clip_img.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo curve_rect.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo curve_to.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo fillstroke.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo gradient.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo image.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo image_pattern.cs x11.cs
mcs -r:System.Drawing -r:Mono.Cairo text.cs x11.cs
