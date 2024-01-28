
import sqlite3



# SQLite database file path
sqlite_db_path = 'D:/sqlite/aviation_new.db' 



conn = sqlite3.connect(sqlite_db_path)

query_create_table = """
CREATE TABLE IF NOT EXISTS aggregated_delays (
    airport_code TEXT,
    airport_name TEXT,
    ades_type TEXT,
    latitude REAL,
    longitude REAL,
    avg_delay REAL,
    country TEXT
);
"""

query_insert_data = """
INSERT INTO aggregated_delays (airport_code, airport_name,ades_type,latitude,longitude, avg_delay, country)
SELECT ADES, ades_name,ades_type, "ADES Latitude", "ADES Longitude", AVG(`delay (minutes)`) AS avg_delay, ades_country FROM flights_data GROUP BY ADES;
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
