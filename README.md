# UnityLocker
## About
UnityLocker is a tool that allows teams to lock files inside unity, which makes them unsavable for other users. This is specially useful when working with scenes and prefabs that are usually the cause of hard-to-fix conflicts and headaches.

## How it works
UnityLocker needs a connection to some kind of server instance, where lock data is stored. As an example, a Flask app has been provided which listenes to requests about locking, unlocking and getting the status of locked assets. 
Inside Unity, right clicking an asset presents a context menu which contains the buttons "Lock", "Unlock (Globally)" and "Unlock (From current commit)".

### Lock
Locking an assets marks the assets, and other users cannot save that particular asset. To lock a file, either right click an asset and select "Lock", or select an asset and via the Asset menu, select "Lock".

### Revert
Reverting a file is used when you either locked a file by accident, or locked a file but never changed it. To unlock a file, either right click an asset and select "Revert", or select an asset and via the Asset menu, select "Revert".

### Unlock
Unlocking a file marks the asset as unlocked for users above your current commit. This means that they need to have your changes merged into their branch before being able to save the asset and/or lock the file. To unlock a file, either right click an asset and select "Unlock", or select an asset and via the Asset menu, select "Unlock".

## Installation
1. Download the sample server and run Starter.bat. This will start a flask app on your device that listenes to port 5000.
2. Add the needed files to your Unity project, then open the settings file (Tools/Open Locker Settings File).
3. Fill in the missing information:
`Base URL`: The url to where your server is running. Port must be included. E.g. `localhost:5000`
`Parent Folder Count`: How deep down inside the git folder your Unity project is. Below this field, you can see the current path. Change the number until this is correct.
`Version Control`: Change this to the version control software you're using.
`Valid Asset Types`: Change this to determine what types of assets are valid.
4. Push all assets and make sure your team gets them as well.
5. Start locking files!
