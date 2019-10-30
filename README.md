# UnityLocker
## About
UnityLocker is a tool that allows teams to lock files inside unity, which makes them unsavable for other users. This is specially useful when working with scenes and prefabs that are usually the cause of hard-to-fix conflicts and headaches.

## How it works
UnityLocker needs a connection to some kind of server instance, where lock data is stored. As an example, a Flask app has been created which listenes to requests about locking, unlocking and getting the status of locked assets. 
Inside Unity, right clicking an asset presents a context menu which contains the buttons "Lock", "Unlock (Globally)" and "Unlock (From current commit)".

### Lock
Locking an assets marks the assets, and other users cannot save that particular asset. To lock a file, either right click an asset and select "Lock", or select an asset and via the Asset menu, select "Lock".

### Unlock (Globally)
Unlocking a file globally is used when you either locked a file by accident, or locked a file but never changed it. To unlock a file, either right click an asset and select "Unlock (Globally)", or select an asset and via the Asset menu, select "Unlock (Globally)".

### Unlock (From current commit)
Unlocking a file from the current commit marks the asset as unlocked for users above your current commit. This means that they need to have your changes merged into their branch before being able to save the asset. To unlock a file from the current commit, either right click an asset and select "Unlock (From current commit)", or select an asset and via the Asset menu, select "Unlock (From current commit)".

## Installation
