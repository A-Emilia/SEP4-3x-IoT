"# SEP4-3x-IoT" 

This file should include a short description of major important functionality along with a table of accessible endpoints for the other system components.


**`/ENDPOINT/PATH/HERE`**
| Action                                    | Endpoint                                    |
| :-----------------------------------------|:--------------------------------------------|
| Get all current sensor data.              | `GET /sensor-data/current`                  |
| Get sensor data based on a time period.   | `GET /sensor-data/history?from=...&to=...`  |
| Send a command to a device.               | `POST /devices/action`                      |

## Supported Device Actions

| Device   | Actions               |
|----------|-----------------------|
| Heater   | turn on, turn off     |
| Windows  | open, close           |
| Curtain  | open, close           |
