import threading
import json

from flask import Flask, request
from threading import Thread

app = Flask(__name__)
lock = threading.Lock()


@app.route('/get-locked-assets', methods=['GET'])
def get_locked_assets():
    file = open("locked-assets.json", "r")
    contents = file.readlines()
    file.close()
    return '\n'.join(contents)


@app.route('/lock-assets', methods=['POST'])
def lock_assets():
    thread = Thread(target=actual_lock_assets, args=[request.form['Guid'], request.form['LockerName']])
    thread.start()
    return ""


@app.route('/unlock-assets', methods=['POST'])
def unlock_assets():
    thread = Thread(target=actual_unlock_assets, args=[request.form['Guid']])
    thread.start()
    return ""


@app.route('/unlock-assets-at-commit', methods=['POST'])
def unlock_assets_at_commit():
    thread = Thread(target=actual_unlock_assets_at_commit, args=[request.form['Guid'], request.form['Sha']])
    thread.start()
    return ""


# Lock file, removing the unlock sha if it exists.
def actual_lock_assets(assets, locker):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    assets = json.loads(assets)

    for asset in assets:
        found = False
        for lock_data in js["RawLockData"]:
            if lock_data["Guid"] == asset:
                lock_data["Locked"] = True
                lock_data["LockerName"] = locker
                found = True
                break
        if not found:
            js["RawLockData"].append({"Guid": asset, "LockerName": locker, "Locked": True, "UnlockSha": ""})
    file = open("locked-assets.json", "w")
    file.writelines(json.dumps(js))
    file.close()
    lock.release()


# Unlock file globally, not requiring a specific commit
def actual_unlock_assets(assets):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    assets = json.loads(assets)

    for asset in assets:
        for lock_data in js["RawLockData"]:
            if lock_data["Guid"] == asset:
                lock_data["Locked"] = False
                break

    file = open("locked-assets.json", "w")
    file.writelines(json.dumps(js))
    file.close()
    lock.release()


# Set UnlockSha to require users to be above that commit to be able to modify file.
def actual_unlock_assets_at_commit(assets, sha):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    assets = json.loads(assets)

    for asset in assets:
        for lock_data in js["RawLockData"]:
            if lock_data["Guid"] == asset:
                lock_data["Locked"] = False
                lock_data["UnlockSha"] = sha
                break

    file = open("locked-assets.json", "w")
    file.writelines(json.dumps(js))
    file.close()
    lock.release()


if __name__ == '__main__':
    app.run(host='127.0.0.1')

