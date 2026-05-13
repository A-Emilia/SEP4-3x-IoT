CREATE TABLE app_user (
    id VARCHAR(16) UNIQUE PRIMARY KEY,
    name VARCHAR(16) UNIQUE NOT NULL
);

CREATE TABLE room (
    id VARCHAR(16) UNIQUE PRIMARY KEY,
    user_id VARCHAR(16) NOT NULL,
    name VARCHAR(16) UNIQUE NOT NULL,
    FOREIGN KEY(user_id) REFERENCES app_user(id)
);

CREATE TYPE actuator_state AS ENUM (
    'On/Open',
    'Off/Closed'
);

CREATE TYPE actuator_type AS ENUM (
    'Heater',
    'Humidifier',
    'Window Servo',
    'Curtain Servo'
);

CREATE TABLE actuator (
    id VARCHAR(16) UNIQUE PRIMARY KEY,
    room_id VARCHAR(16) NOT NULL,
    state actuator_state NOT NULL,
    type actuator_type NOT NULL,
    FOREIGN KEY(room_id) REFERENCES room(id)
);

\dt