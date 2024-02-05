import sqlite3

# SQLite database file paths
source_db_path = 'D:/Powerwall sqlite/aviation.db' 
target_db_path = 'D:/Powerwall sqlite/aviation111.db'  

# Connect to the source database
conn_source = sqlite3.connect(source_db_path)
cursor_source = conn_source.cursor()

# Perform the aggregation query
query_aggregate = """
SELECT ADES, ades_name, "ADES Latitude", "ADES Longitude", AVG(`delay (minutes)`) AS avg_delay FROM flights_data GROUP BY ADES;
"""
cursor_source.execute(query_aggregate)
aggregated_data = cursor_source.fetchall()

# Close the source database connection
cursor_source.close()
conn_source.close()

# Connect to the target database
conn_target = sqlite3.connect(target_db_path)
cursor_target = conn_target.cursor()

# Create the new table in the target database
query_create_table = """
CREATE TABLE IF NOT EXISTS aggregated_delays (
    airport_code TEXT,
    airport_name TEXT,
    latitude REAL,
    longitude REAL,
    avg_delay REAL
);
"""
cursor_target.execute(query_create_table)
print("Created aggregated_delays table in target database")

# Insert the aggregated data into the new table
query_insert_data = """
INSERT INTO aggregated_delays (airport_code, airport_name, latitude, longitude, avg_delay) VALUES (?, ?, ?, ?, ?);
"""
cursor_target.executemany(query_insert_data, aggregated_data)
print("Aggregated data inserted into target database")

# Commit the changes and close the target database connection
conn_target.commit()
cursor_target.close()
conn_target.close()
print("Finished execution")
