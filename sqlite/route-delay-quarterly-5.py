
import sqlite3
import time


# SQLite database file path
sqlite_db_path = 'D:/sqlite/aviation.db' 



conn = sqlite3.connect(sqlite_db_path)

query_create_table = """CREATE TABLE route_delay_quarterly (
    RouteID INTEGER,
    Year INTEGER,
    Quarter INTEGER,
    TotalDelay INTEGER,
    TrafficCount INTEGER
);
"""

query_insert_quarterly_data = """
INSERT INTO route_delay_quarterly (RouteID, Year, Quarter, TotalDelay, TrafficCount)
SELECT 
    RouteID, 
    strftime('%Y', Date) as Year,
    CASE 
        WHEN strftime('%m', Date) BETWEEN '01' AND '03' THEN 1
        WHEN strftime('%m', Date) BETWEEN '04' AND '06' THEN 2
        WHEN strftime('%m', Date) BETWEEN '07' AND '09' THEN 3
        WHEN strftime('%m', Date) BETWEEN '10' AND '12' THEN 4
    END as Quarter,
    SUM(Delay) as TotalDelay,
    COUNT(*) as TrafficCount
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
