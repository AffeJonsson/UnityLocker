import json
import Database

from flask import Flask, request
from threading import Thread

app = Flask(__name__)
db = Database.Database('locked-assets')


@app.route('/get-locked-assets', methods=['GET'])
def get_locked_assets():
    locked_assets = db.get_locked_assets()
    ret = []
    for asset in locked_assets:
        obj = {"Guid": asset[1], "LockerName": asset[2], "Locked": asset[3], "UnlockSha": asset[4], "Date": asset[5]}
        ret.append(obj)
    return json.dumps({"RawLockData": ret})


@app.route('/lock-assets', methods=['POST'])
def lock_assets():
    Thread(target=actual_lock_assets, args=[request.form['Guid'], request.form['LockerName']]).start()
    return ""


@app.route('/unlock-assets', methods=['POST'])
def unlock_assets():
    Thread(target=actual_unlock_assets, args=[request.form['Guid']]).start()
    return ""


@app.route('/unlock-assets-at-commit', methods=['POST'])
def unlock_assets_at_commit():
    Thread(target=actual_unlock_assets_at_commit, args=[request.form['Guid'], request.form['Sha']]).start()
    return ""


@app.route('/clear-locks', methods=['POST'])
def clear_locks():
    Thread(target=actual_clear_locks).start()
    return ""


def actual_lock_assets(assets, locker):
    assets = json.loads(assets)
    for asset in assets:
        db.lock_asset(asset, locker)


# Unlock file globally, not requiring a specific commit
def actual_unlock_assets(assets):
    assets = json.loads(assets)
    for asset in assets:
        db.revert_asset_lock(asset)


# Set UnlockSha to require users to be above that commit to be able to modify file.
def actual_unlock_assets_at_commit(assets, sha):
    assets = json.loads(assets)
    for asset in assets:
        db.unlock_asset(asset, sha)


# Clear all locked files and history
def actual_clear_locks():
    db.clear_locks()


if __name__ == '__main__':
    app.run(host='127.0.0.1')

