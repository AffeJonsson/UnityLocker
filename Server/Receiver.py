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


def actual_lock_asset(asset, locker):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    js["RawLockData"].append({"Guid": asset, "LockerName": locker})
    file = open("locked-assets.json", "w")
    file.writelines(json.dumps(js))
    file.close()
    lock.release()
    pass


def actual_unlock_asset(asset):
    lock.acquire(True)
    file = open("locked-assets.json", "r")
    js = json.loads('\n'.join(file.readlines()))
    file.close()
    asset = [x for x in js["RawLockData"] if x["Guid"] == asset][0]
    js["RawLockData"].remove(asset)
    file = open("locked-assets.json", "w")
    file.writelines(json.dumps(js))
    file.close()
    lock.release()
    pass


if __name__ == '__main__':
    app.run(host='127.0.0.1')

