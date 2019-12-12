# UnityLocker
## About
UnityLocker is a tool that allows teams to lock files inside unity, which makes them unsavable for other users. This is specially useful when working with scenes and prefabs that are usually the cause of hard-to-fix conflicts and headaches.

## Why this was created
My team had issues with people working in the same scene simultaneously, and decided that was no longer a possibility. The solution they came up with was to write in Slack which scenes they were working on, and when they no longer were working with them. As you might expect, it's hard to see what scenes are taken and when they're not, which is why I created this.

## How it works
UnityLocker checks with a server which files are locked, and displays padlock icons on those files. In the hierarchy window, locked scenes and prefabs are displayed by a colored box overlay.

![Scene locked by you](/Readme/hierarchy_scene_locked.png)

![Scene unlocked at a later commit](/Readme/hierarchy_scene_locked_unlocked_later.png)

![Scene locked by someone else](/Readme/hierarchy_scene_locked_someone_else.png)

The green one means that you have locked that file, no one can save that asset except you.

The yellow one means that someone had locked it, but then finished their work and unlocked it. To make it saveable for you again, you need to be ahead of their commit.

The red one means that the file is currently locked by someone else.

### Lock
Locking an assets marks the selected assets, and other users cannot save those particular assets.

### Revert Lock
Reverting a file is used when you either locked a file by accident, or locked a file but never changed it. 

### Finish Lock
Finishing a file marks the asset as unlocked for users above your current commit. This means that they need to have your changes merged into their branch before being able to save the asset and/or lock the file. 

## Locked Files Window
This window displays all files currently locked by you and everyone else.
To open, click Window/Locked Files.

## Asset History Window
This window displays when and by who the file was locked an unlocked.
To open, click Window/Asset History.

## Installation
1. Download Server.zip, extract and run Starter.bat. This will start a flask app on your device that listenes to port 5000.
2. Add the needed files to your Unity project (More info below!), then open the settings file (Tools/Open Locker Settings File).
3. Fill in the missing information:

`Base URL`: The url to where your server is running. Port must be included. E.g. `localhost:5000`

`Parent Folder Count`: How deep down inside the git folder your Unity project is. Below this field, you can see the current path. Change the number until this is correct.

`Version Control`: Change this to the version control software you're using.

`Valid Asset Types`: Change this to determine what types of assets are valid.

4. Push all assets and make sure your team gets them as well.
5. Start locking files!

## Required Files
`lockermain.unitypackage` This is the main package and is required to be able to use this tool.

`lockerjsondotnet.unitypackage` This is the only package that UnityLocker is dependent on. If your project already has a JSON.NET plugin, this is not needed. (https://github.com/JamesNK/Newtonsoft.Json, MIT licence)

`lockerversioncontrolgit.unitypackage` This adds the ability to use Git as your version control handler. (.NET 4.6 needed)

`lockerlibgit2sharp.unitypackage` This is needed for the Git version control handler to work. If your project already has a LibGit2Sharp plugin, this is not needed. (https://github.com/libgit2/libgit2sharp, MIT Licence) (.NET 4.6 needed, does not work for Unity 2017)
