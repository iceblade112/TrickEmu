How to build TrickEmu using xbuild
==================================

##Building on Unix-like systems
1. Install Mono with your method of choice, and make sure xbuild is available in your PATH.
2. Open a terminal window.
3. Change directory to the TrickEmu folder
4. Enter the command below:
```
xbuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.5 TrickEmu.sln
```

