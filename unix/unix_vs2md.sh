#!/bin/sh
# Visual Studio to MonoDevelop .csproj converter
# Usage: Run from TrickEmu directory with "sh unix/unix_vs2md.sh"

## GNU sed (default for XGL (X11+GNU+Linux))
find . -name '*.csproj' -exec sed -i 's/>v4.5.2</>v4.5</g' "{}" \;

## Non-GNU sed (used on macOS. Its official name, as of 2016, I checked.)
#find . -name '*.csproj' -exec sed -i '' 's/>v4.5.2</>v4.5</g' "{}" \;
