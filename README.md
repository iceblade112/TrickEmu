# TrickEmu
[![Build Status](https://travis-ci.org/iceblade112/TrickEmu.svg?branch=master)](https://travis-ci.org/iceblade112/TrickEmu)

TrickEmu is a WIP emulator for Trickster Online 0.50 coded in C# and licensed under AGPLv3. TrickEmu compiles with Mono. Any complaints about the license go to [@PyroSamurai](https://github.com/PyroSamurai).

### Requirements
* MySQL, MariaDB, or some other drop-in MySQL replacement
* MySQL Connector for .NET
* Xamarin Studio/MonoDevelop (with modifications) or Microsoft Visual Studio 2015

### TO-DO
Pretty much everything except for the finished stuff. The list includes:
* Better networking
* Some more character selection data
* Character deletion
* Character movement
* Character equipment
* Character inventory
* Character etc.

### Finished but not working
* Chat box above the player's head
* Sitting (semi-finished)
* "Personal notice" (that's what the Chinese translates to! I forgot what it is in English.)

### Finished
* Capturing basic login packets
* Selecting a server (127.0.0.1)
* Connecting to a game server (127.0.0.1)
* Chat, minus the message box on the top of the player
* A basic GM chat command (!gmc)
