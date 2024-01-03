
-- Get flights based on delays in descending order
SELECT adep_name, ADEP Latitude, ADEP Longitude, ades_name, ADES Latitude, ADES Longitude, `delay (minutes)`
FROM flights_data
ORDER BY `delay (minutes)` DESC
LIMIT 400


-- Create table for airports and delays aggregated
CREATE TABLE IF NOT EXISTS aggregated_delays (
    airport_name TEXT,
    airport_code TEXT,
    total_delays INTEGER
);

-- Insert aggregated data into the new table
INSERT INTO aggregated_delays (airport_name, total_delays)
SELECT a.name AS airport_name, SUM(f.Delay) AS total_delays
FROM flights_data AS f
JOIN airport_codes AS a ON f.ADES = a.code  -- Assuming airport_codes table contains airport codes and names
GROUP BY f.ADES;

-- Indexing on the new table for faster retrieval if needed
CREATE INDEX idx_airport_name_delay ON aggregated_delays (airport_name);