import sqlite3
import threading
import datetime


class Database:
    _connection = None
    _cursor = None
    _lock = threading.Lock()

    def __init__(self, database_name):
        self._connection = sqlite3.connect(database_name, check_same_thread=False)
        self._cursor = self._connection.cursor()
        self._cursor.execute('CREATE TABLE IF NOT EXISTS LockedAssets ('
                             'id integer, '
                             'guid text, '
                             'locker text, '
                             'locked integer, '
                             'sha text, '
                             'date text, '
                             'UNIQUE (id, guid))')

    def lock_asset(self, asset_guid, locker):
        self._lock.acquire(True)
        last_id = self.get_last_id(asset_guid)
        variables = (last_id + 1, asset_guid, locker, True, "", datetime.datetime.utcnow().isoformat())
        self._cursor.execute("INSERT INTO LockedAssets VALUES (?, ?, ?, ?, ?, ?)", variables)
        self._connection.commit()
        self._lock.release()

    def unlock_asset(self, asset_guid, unlock_sha):
        self._lock.acquire(True)
        last_entry = self.get_last_entry(asset_guid)
        last_id = last_entry[0]
        last_locker = last_entry[2]
        variables = (last_id + 1, asset_guid, last_locker, False, unlock_sha, datetime.datetime.utcnow().isoformat())
        self._cursor.execute("INSERT INTO LockedAssets VALUES (?, ?, ?, ?, ?, ?)", variables)
        self._connection.commit()
        self._lock.release()

    def revert_asset_lock(self, asset_guid):
        self._lock.acquire(True)
        last_entry = self.get_last_entry(asset_guid)
        last_id = last_entry[0]
        last_locker = last_entry[2]
        variables = (last_id + 1, asset_guid, last_locker, False, "", datetime.datetime.utcnow().isoformat())
        self._cursor.execute("INSERT INTO LockedAssets VALUES (?, ?, ?, ?, ?, ?)", variables)
        self._connection.commit()
        self._lock.release()

    def clear_locks(self):
        self._lock.acquire(True)
        self._cursor.execute("DELETE FROM LockedAssets")
        self._connection.commit()
        self._lock.release()

    def get_last_id(self, asset_guid):
        last_entry = self.get_last_entry(asset_guid)
        if last_entry[0] is None:
            return -1
        return last_entry[0]

    def get_last_entry(self, asset_guid):
        result = self._cursor.execute("SELECT MAX(id), guid, locker, locked, sha, date FROM LockedAssets WHERE guid = ?", (asset_guid,))
        return list(iter(result))[0]

    def get_asset_history(self, asset_guid):
        result = self._cursor.execute("SELECT id, guid, locker, locked, sha, date FROM LockedAssets WHERE guid = ?", (asset_guid,))
        return list(iter(result))

    def get_locked_assets(self):
        result = self._cursor.execute("SELECT a.* "
                                      "FROM LockedAssets a "
                                      "INNER JOIN ( "
                                      "  SELECT MAX(id) id, guid, locker, locked, sha, date "
                                      "  FROM LockedAssets "
                                      "  GROUP BY guid "
                                      ") b ON a.id = b.id AND a.guid = b.guid AND a.locked = 1")
        return list(iter(result))


if __name__ == '__main__':
    db = Database("Test")
    db.lock_asset("Hej", "Alf")
    db.unlock_asset("Hej", "1234567890")
    db.lock_asset("Hej", "Alf")
    db.revert_asset_lock("Hej")
    db.lock_asset("Hej2", "Alf")
    db.lock_asset("Hej3", "Alf")
    db.lock_asset("Hej", "Alf")
    for entry in db.get_locked_assets():
        print(entry)
    print('---')
    for entry in db.get_asset_history("Hej"):
        print(entry)
