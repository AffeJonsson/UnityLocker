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


@app.route('/lock-asset', methods=['POST'])
def lock_asset():
    thread = Thread(target=actual_lock_asset, args=[request.form['Guid'], request.form['LockerName']])
    thread.start()
    return ""


@app.route('/unlock-asset', methods=['POST'])
def unlock_asset():
    thread = Thread(target=actual_unlock_asset, args=[request.form['Guid']])
    thread.start()
    return ""


@app.route('/unlock-asset-at-commit', methods=['POST'])
def unlock_asset_at_commit():
    thread = Thread(target=actual_unlock_asset_at_commit, args=[request.form['Guid'], request.form['Sha']])
    thread.start()
    return ""


# Lock file, removing the unlock sha if it exists.
def actual_lock_asset(asset, locker):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    lock_datas = [x for x in js["RawLockData"] if x["Guid"] == asset]
    if len(lock_datas) > 0:
        lock_datas[0]["Locked"] = True
    else:
        js["RawLockData"].append({"Guid": asset, "LockerName": locker, "Locked": True, "UnlockSha": ""})
    file = open("locked-assets.json", "w")
    file.writelines(json.dumps(js))
    file.close()
    lock.release()


# Unlock file globally, not requiring a specific commit
def actual_unlock_asset(asset):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    lock_datas = [x for x in js["RawLockData"] if x["Guid"] == asset]
    if len(lock_datas) > 0:
        lock_datas[0]["Locked"] = False
        file = open("locked-assets.json", "w")
        file.writelines(json.dumps(js))
        file.close()
    lock.release()


# Set UnlockSha to require users to be above that commit to be able to modify file.
def actual_unlock_asset_at_commit(asset, sha):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    lock_datas = [x for x in js["RawLockData"] if x["Guid"] == asset]
    if len(lock_datas) > 0:
        lock_datas[0]["Locked"] = False
        lock_datas[0]["UnlockSha"] = sha
        file = open("locked-assets.json", "w")
        file.writelines(json.dumps(js))
        file.close()
    lock.release()


if __name__ == '__main__':
    app.run(host='127.0.0.1')

