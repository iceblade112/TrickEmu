#!/bin/sh
# MonoDevelop to Visual Studio .csproj converter
# Usage: Run from TrickEmu directory with "sh unix/unix_md2vs.sh"

## GNU sed (default for XGL (X11+GNU+Linux))
find . -name '*.csproj' -exec sed -i 's/>v4.5</>v4.5.2</g' "{}" \;
find . -name '*.csproj' -exec sed -i 's/ToolsVersion="4.0"/ToolsVersion="14.0"/g' "{}" \;

## Non-GNU sed (used on macOS. Its official name, as of 2016, I checked.)
#find . -name '*.csproj' -exec sed -i '' 's/>v4.5</>v4.5.2</g' "{}" \;
#find . -name '*.csproj' -exec sed -i '' 's/ToolsVersion="4.0"/ToolsVersion="14.0"/g' "{}" \;
