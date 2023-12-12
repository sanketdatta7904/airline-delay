import sqlite3
import time

conn = sqlite3.connect(r'C:\Users\Sanket PC\Desktop\group-2\sqlite\aviation111.db')

query = "SELECT airport_code, airport_name, latitude, longitude, avg_delay FROM aggregated_delays ORDER BY `avg_delay` DESC;"  

start_time = time.time()

cursor = conn.execute(query)

results = cursor.fetchall()

execution_time = time.time() - start_time

print(f"Query executed in {execution_time:.6f} seconds")

conn.close()
