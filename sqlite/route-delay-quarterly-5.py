
import sqlite3
import time


# SQLite database file path
sqlite_db_path = 'D:/sqlite/aviation_new.db' 



conn = sqlite3.connect(sqlite_db_path)

query_create_table = """CREATE TABLE route_delay_quarterly (
    RouteID TEXT,
    Year INTEGER,
    Quarter INTEGER,
    Average_Delay REAL,
    Traffic_Count INTEGER,
    Source_latitude REAL,
    Source_longitude REAL,
    Dest_latitude REAL,
    Dest_longitude REAL,
    Source_country TEXT,
    Dest_country TEXT
);
"""

query_insert_quarterly_data = """
INSERT INTO route_delay_quarterly (RouteID, Source_latitude, Source_longitude, Dest_latitude, Dest_longitude, Year, Quarter, Average_Delay, Traffic_Count, Source_country, Dest_country)
SELECT 
    RouteID, 
    Source_latitude,
    Source_longitude,
    Dest_latitude,
    Dest_longitude,
    strftime('%Y', Date) as Year,
    CASE 
        WHEN strftime('%m', Date) BETWEEN '01' AND '03' THEN 1
        WHEN strftime('%m', Date) BETWEEN '04' AND '06' THEN 2
        WHEN strftime('%m', Date) BETWEEN '07' AND '09' THEN 3
        WHEN strftime('%m', Date) BETWEEN '10' AND '12' THEN 4
    END as Quarter,
    AVG(Delay) as Average_Delay,
    COUNT(*) as Traffic_Count,
    Source_country AS Source_country,
    Dest_country AS Dest_country
FROM route_delay
GROUP BY RouteID, Year, Quarter;
"""

start_time = time.time()

cursor = conn.cursor()
cursor.execute(query_create_table)
print("Created route_quarterly_delay table")
cursor.execute(query_insert_quarterly_data)
print("Data insertion completed")

cursor.close()
conn.commit()
conn.close()
print("Finished execution")
execution_time = time.time() - start_time

print(f"Query executed in {execution_time:.6f} seconds")
