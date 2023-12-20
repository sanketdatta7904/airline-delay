import sqlite3

conn = sqlite3.connect('D:/sqlite/aviation.db')


create_index_queries = [
    "CREATE INDEX idx_date ON route_delay (Date);",
    "CREATE INDEX idx_routeid ON route_delay (RouteID);",
    "CREATE INDEX idx_routeid_date ON route_delay (RouteID, Date);"
]

cursor = conn.cursor()


for query in create_index_queries:
    try:
        cursor.execute(query)
        print(f"Successfully executed: {query}")
    except sqlite3.Error as e:
        print(f"An error occurred: {e}")

conn.commit()

conn.close()
