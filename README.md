"# SEP4-3x-IoT" 

This file should include a short description of major important functionality along with a table of accessible endpoints for the other system components.


**`/ENDPOINT/PATH/HERE`**
| Action                                    | Endpoint                  |
| :-----------------------------------------|:--------------------------|
| Get all data sensor data.                 | `GET /endpoint`           |
| Get sensor data from the last 15 minutes. | `GET /endpoint/recent`    |
| Send a command to a device.               | `POST /endpoint/{action}` |

## Supported Device Actions

| Device   | Actions               |
|----------|-----------------------|
| Heater   | turn on, turn off     |
| Windows  | open, close           |
| Curtain  | open, close           |
