
import sqlite3



# SQLite database file path
sqlite_db_path = 'D:/sqlite/aviation.db' 



conn = sqlite3.connect(sqlite_db_path)

query_create_table = """
CREATE TABLE IF NOT EXISTS route_delay (
    RouteID TEXT,
    delay REAL,
    Date TEXT,
    Source_latitude REAL,
    Source_longitude REAL,
    Dest_latitude REAL,
    Dest_longitude REAL
);
"""

query_alter_flights_data_table = """
INSERT INTO route_delay (RouteID, Date, delay, Source_latitude, Source_longitude, Dest_latitude, Dest_longitude)
SELECT 
    ADEP || '-' || ADES AS RouteID,
    SUBSTR("ACTUAL ARRIVAL TIME", 7, 4) || '-' || -- Year
    SUBSTR("ACTUAL ARRIVAL TIME", 4, 2) || '-' || -- Month
    SUBSTR("ACTUAL ARRIVAL TIME", 1, 2) AS Date,   -- Day
    "delay (minutes)" AS delay,
    "ADEP Latitude" AS Source_latitude,
    "ADEP Longitude" AS Source_longitude,
    "ADES Latitude" AS Dest_latitude,
    "ADES Longitude" AS Dest_longitude
FROM flights_data;

"""



cursor = conn.cursor()
cursor.execute(query_create_table)
print("Created route_delay table")
cursor.execute(query_alter_flights_data_table)
print("Operation completed")

cursor.close()
conn.commit()
conn.close()
print("Finished execution")
