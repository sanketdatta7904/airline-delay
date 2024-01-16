import pandas as pd
import sqlite3
import os
from datetime import datetime

# Directory path containing CSV files
csv_directory = 'D:/sqlite/files'

airport_data = pd.read_csv('D:/sqlite/airport code and names/airport-codes_csv.csv')

# airport_mapping = airport_data.set_index('Code')[['Name', 'Type']].to_dict(orient='index')


# SQLite database file path
sqlite_db_path = 'D:/sqlite/aviation.db'  

# connection to the SQLite database
conn = sqlite3.connect(sqlite_db_path)

# dictionary to store aggregated delays for each ADES
airport_delays = {}


# Function to check if the flights_data table exists in the database
def table_exists():
    query = "SELECT name FROM sqlite_master WHERE type='table' AND name='flights_data'"
    cursor = conn.cursor()
    cursor.execute(query)
    return cursor.fetchone() is not None


# Create an object to map airport codes to names and types
airport_mapping = {}
for index, row in airport_data.iterrows():
    airport_mapping[row['gps_code']] = {'name': row['name'], 'type': row['type']}

# print(airport_mapping)

append_flag = False
if not table_exists():
    append_flag = False
else:  
    append_flag = True

files_list = os.listdir(csv_directory)
end_of_files = False
while(end_of_files != True):
    if append_flag == False:
        # Iterate through CSV files in the directory
        file = files_list[0]
        if file.endswith('.csv'):
            file_path = os.path.join(csv_directory, file)
            
            # Read the CSV file with the header and selected columns
            data = pd.read_csv(file_path)
                
            # Add ADEP and ADES names using the airport_mapping
            data['adep_name'] = data['ADEP'].map(lambda x: airport_mapping[x]['name'] if x in airport_mapping else '')
            data['ades_name'] = data['ADES'].map(lambda x: airport_mapping[x]['name'] if x in airport_mapping else '')
                
            data['adep_type'] = data['ADEP'].map(lambda x: airport_mapping[x]['type'] if x in airport_mapping else '')
            data['ades_type'] = data['ADES'].map(lambda x: airport_mapping[x]['type'] if x in airport_mapping else '')
            time_format = '%d-%m-%Y %H:%M:%S'

            data['delay (minutes)'] = (datetime.strptime(data['ACTUAL ARRIVAL TIME'][0], time_format) - datetime.strptime(data['FILED ARRIVAL TIME'][0], time_format)).total_seconds() / 60
            # Write the data to the SQLite database with header included
            data.to_sql('flights_data', conn, if_exists='replace', index=False)
            append_flag = True
            print("Uploaded file", file)
            print("flights_data table created successfully.")
                
    else:
        for i in range(1, len(files_list)):
            file = files_list[i]
            if file.endswith('.csv'):
                file_path = os.path.join(csv_directory, file)
                time_format = '%d-%m-%Y %H:%M:%S'

                # Read the CSV file without header and selected columns
                data = pd.read_csv(file_path, header=0)
                # Add ADEP and ADES names using the airport_mapping
                data['adep_name'] = data['ADEP'].map(lambda x: airport_mapping[x]['name'] if x in airport_mapping else '')
                data['ades_name'] = data['ADES'].map(lambda x: airport_mapping[x]['name'] if x in airport_mapping else '')
                
                data['adep_type'] = data['ADEP'].map(lambda x: airport_mapping[x]['type'] if x in airport_mapping else '')
                data['ades_type'] = data['ADES'].map(lambda x: airport_mapping[x]['type'] if x in airport_mapping else '')
                data['delay (minutes)'] = (datetime.strptime(data['ACTUAL ARRIVAL TIME'][0], time_format) - datetime.strptime(data['FILED ARRIVAL TIME'][0], time_format)).total_seconds() / 60
                # Append the data to the existing table in the SQLite database
                data.to_sql('flights_data', conn, if_exists='append', index=False)
                print("Uploaded file and appended", file)
            
            if(i == len(files_list)-1):
                end_of_files = True


        print("Data appended to flights_data table successfully.")

# Commit changes and close the connection
conn.commit()
conn.close()


print(f"All CSV files in {csv_directory} uploaded to {sqlite_db_path} successfully.")


