import sqlite3
import time

conn = sqlite3.connect('D:/sqlite/aviation.db')


query_for_filghts_data = "SELECT adep_name, ADEP Latitude, ADEP Longitude, ades_name, ADES Latitude, ADES Longitude, `delay (minutes)` FROM flights_data ORDER BY `delay (minutes)` DESC;" 

query_for_aggregated_delays = "SELECT ades_type, AVG(`avg_delay`) AS avg_delay FROM aggregated_delays GROUP BY ades_type;" 

query_for_route_delays = """SELECT RouteID, Year, Quarter, TotalDelay
FROM route_delay_quarterly
WHERE (Year > 2014 OR (Year = 2015 AND Quarter >= 4)) AND (Year < 2016 OR (Year = 2017 AND Quarter <= 2));
"""
start_time = time.time()

cursor = conn.execute(query_for_route_delays)

results = cursor.fetchall()
print("DATA SIZE = ", len(results))
# print(results)
execution_time = time.time() - start_time

print(f"Query executed in {execution_time:.6f} seconds")

conn.close()
