
import sqlite3



# SQLite database file path
sqlite_db_path = 'D:/sqlite/aviation.db' 



conn = sqlite3.connect(sqlite_db_path)

query_create_table = """
CREATE TABLE IF NOT EXISTS route_delay (
    RouteID TEXT,
    delay INTEGER,
    Date TEXT
);
"""

query_alter_flights_data_table = """
INSERT INTO route_delay (RouteID, Date, delay)
SELECT 
    ADEP || '-' || ADES AS RouteID,
    SUBSTR("ACTUAL ARRIVAL TIME", 7, 4) || '-' || -- Year
    SUBSTR("ACTUAL ARRIVAL TIME", 4, 2) || '-' || -- Month
    SUBSTR("ACTUAL ARRIVAL TIME", 1, 2) AS Date,   -- Day
    "delay (minutes)" AS delay
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
