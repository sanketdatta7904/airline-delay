
import sqlite3



# SQLite database file path
sqlite_db_path = 'D:/sqlite/aviation_new.db' 



conn = sqlite3.connect(sqlite_db_path)

query_clean_aggregated_delays_no_latlong = """
DELETE FROM aggregated_delays
WHERE latitude IS NULL OR longitude IS NULL;
"""

query_clean_flights_data = """DELETE FROM flights_data
WHERE 
    "ADEP Latitude" IS NULL OR "ADEP Longitude" IS NULL OR
    "ADES Latitude" IS NULL OR "ADES Longitude" IS NULL;"""
cursor = conn.cursor()
cursor.execute(query_clean_aggregated_delays_no_latlong)
print("Cleaned aggregated_delays table")
cursor.execute(query_clean_flights_data)
print("Cleaned aggregated_delays table")
cursor.close()
conn.commit()
conn.close()
print("Finished execution")
