import sqlite3
import time

conn = sqlite3.connect('D:/sqlite/aviation.db')

# just the flights data
query_for_filghts_data = "SELECT adep_name, ADEP Latitude, ADEP Longitude, ades_name, ADES Latitude, ADES Longitude, `delay (minutes)` FROM flights_data ORDER BY `delay (minutes)` DESC;" 

# db to get the aggregated delays, additionally containing the type (small_airport, medium_airport, large_airport, heliport, closed, seaplane_base)
query_for_aggregated_delays = "SELECT ades_type, AVG(`avg_delay`) AS avg_delay FROM aggregated_delays GROUP BY ades_type;" 
# delay of a specific route
query_for_route_delays = """SELECT RouteID, SUM(delay) as TotalDelay
FROM route_delay
WHERE Date BETWEEN '2015-03-01' AND '2018-09-01'
GROUP BY RouteID;"""
start_time = time.time()

cursor = conn.execute(query_for_route_delays)

results = cursor.fetchall()
# print(results)
execution_time = time.time() - start_time

print(f"Query executed in {execution_time:.6f} seconds")

conn.close()
