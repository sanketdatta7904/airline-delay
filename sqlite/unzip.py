import gzip
import os
import shutil

main_directory = r'D:\aviation dataset'
dest_directory = r'D:\Powerwall sqlite\files' 


def unzip_specific_files(main_dir, dest_dir):
    for root, dirs, files in os.walk(main_dir):
        for dir_name in dirs:
            folder_path = os.path.join(root, dir_name)
            for filename in os.listdir(folder_path):
                if (filename.startswith('Flights_20'))and(filename.endswith('csv.gz')):
                    file_path = os.path.join(folder_path, filename)
                    if filename:
                        extract_to = dest_directory 
                        extract_path = os.path.join(extract_to, filename[:-3])  # Remove .gz extension
                        print(f"Extracting {filename}...")
                        with gzip.open(file_path, 'rb') as f_in:
                            with open(extract_path, 'wb') as f_out:
                                shutil.copyfileobj(f_in, f_out)
                        print(f"{filename} extracted successfully to {extract_to}")

unzip_specific_files(main_directory, dest_directory)
