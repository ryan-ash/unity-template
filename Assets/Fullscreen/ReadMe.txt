Thanks for using Fullscreen Editor :)

DESCRIPTION:
An extension that does what its name says, it puts editor windows in fullscreen mode, simple and easy, useful for recording tutorials, testing in a real gaming environment and tweaking your scene.
Everything is still fully functional in fullscreen.

Source code included

Any suggestion, bug report or question feel free to contact me:
samuelschultze@gmail.com

HOW TO USE:
- Just press the shortcut to fullscreen a window or close it

KEY BINDINGS:
- Ctrl+F9 = The window currently under the mouse cursor
- Ctrl+F10 = Game view
- Ctrl+F11 = Scene view
- Ctrl+F12 = Main view (this will fullscreen the entire program, not just a window)

FAQ:
Q: How to change the keybindings? 
A: Open the code and find a line called "Menu items paths", it has an explanation of how to do it.

Q: What is this "Show toolbar" thing?
A: It's a setting for hiding or showing the Scene View or GameView toolbar while on fullscreen, the toolbar that contains the Maximize on play, Stats, Mute Audio and so on.

Q: How does the "Fullscreen on play" works?
A: The same way as the default "Maximize on play" does, but it will put the Game View in fullscreen instead of just maximizing it.

Q: My Unity flashes when switching Fullscreen, is that normal?
A: It's just the extension saving and loading your Layout, so you don't lose it when going into fullscreen.

Q: I see this "OptionalModule.zip" thing in the extension folder, what is it?
A: It's a compiled assembly of the plugin, if you want the extension to be available in all projects without the need to import the package, there is an InstallMe.txt with further explanation and installation guide, you can delete it if you're not going to install.

Q: My game fail to compile if I use the extension, how to fix it?
A: The extension must be placed inside a folder called Editor because it uses the UnityEditor API.

KNOWN ISSUES:
- May throw some errors when using custom windows, these errors are harmless.
- Will always open on the same screen if using OSX or Linux.
- Won't work properly if using a windows scale different than 100%.

CHANGELOG:
Version 1.1.2:
- Fixed a crash when trying to close a fullscreen window after closing Unity and opening it again.

Version 1.1.1:
- Experimental Mac OSX support.
- Fixed a bug where using Input while on playmode would give wrong values.
- Fixed a NullReferenceException using fullscreen on play.

Version 1.1.0:
- Added an option to fullscreen the window currently under the mouse.
- Added a MenuItem for hiding the toolbar of GameView and SceneView.
- Added a MenuItem for fullscreen on play, so it will fullscreen GameView every time you run your game.
- Fixed a bug of GameView losing focus when it's over other GameView.