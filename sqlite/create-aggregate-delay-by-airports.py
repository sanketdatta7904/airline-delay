
import sqlite3



# SQLite database file path
sqlite_db_path = 'D:/Powerwall sqlite/aviation.db' 

sqlite_db_path_1 = 'D:/Powerwall sqlite/aviation111.db'  


conn = sqlite3.connect(sqlite_db_path)

query_create_table = """
CREATE TABLE IF NOT EXISTS aggregated_delays (
    airport_code TEXT,
    airport_name TEXT,
    latitude REAL,
    longitude REAL,
    avg_delay REAL
);
"""

query_insert_data = """
INSERT INTO aggregated_delays (airport_code, airport_name,latitude,longitude, avg_delay)
SELECT ADES, ades_name, "ADES Latitude", "ADES Longitude", AVG(`delay (minutes)`) AS avg_delay FROM flights_data GROUP BY ADES;
"""

cursor = conn.cursor()
cursor.execute(query_create_table)
print("Created aggregated_delays table")
cursor.execute(query_insert_data)
print("Operation completed")

cursor.close()
conn.commit()
conn.close()
print("Finished execution")
