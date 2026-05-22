CREATE TABLE app_user (
                          id VARCHAR(16) UNIQUE PRIMARY KEY,
                          name VARCHAR(16) UNIQUE NOT NULL,
                          email VARCHAR(255) UNIQUE NOT NULL,
                          password_hash VARCHAR(255) NOT NULL
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

CREATE TABLE device_action_log (
                                   id SERIAL PRIMARY KEY,
                                   room_id VARCHAR(16) NOT NULL,
                                   device_type actuator_type NOT NULL,
                                   previous_state actuator_state,
                                   new_state actuator_state NOT NULL,
                                   timestamp_utc TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                   FOREIGN KEY(room_id) REFERENCES room(id)
);

INSERT INTO app_user (id, name, email, password_hash)
VALUES ('shared-user', 'SharedUser', 'shared@example.com', 'temporary');

INSERT INTO room (id, user_id, name)
VALUES ('shared', 'shared-user', 'Shared');

INSERT INTO actuator (id, room_id, state, type)
VALUES
    ('heater-shared', 'shared', 'Off/Closed', 'Heater'),
    ('window-shared', 'shared', 'Off/Closed', 'Window Servo'),
    ('curtain-shared', 'shared', 'On/Open', 'Curtain Servo'),
    ('humidifier-shared', 'shared', 'On/Open', 'Humidifier Servo');

INSERT INTO device_action_log (room_id, device_type, previous_state, new_state)
VALUES
    ('shared', 'Curtain Servo', 'Off/Closed', 'On/Open');

\dt