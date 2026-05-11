db = db.getSiblingDB("measurement_data");

db.createCollection("measurements");

db.runCommand({
  collMod: "measurements",
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: [
        "roomId",
        "timestamp",
        "temperature",
        "humidity",
        "lightLevel"
      ],
      properties: {
        _id: {
          bsonType: "objectId"
        },
        roomId: {
          bsonType: "string",
          description: "Room identifier must be a string"
        },
        timestamp: {
          bsonType: "date",
          description: "Timestamp must be a valid date"
        },
        temperature: {
          bsonType: ["double", "int", "decimal"],
          description: "Temperature value"
        },
        humidity: {
          bsonType: ["double", "int", "decimal"],
          minimum: 0,
          description: "Humidity percentage"
        },
        lightLevel: {
          bsonType: ["double", "int", "decimal"],
          minimum: 0,
          description: "Light level measurement"
        }
      }
    }
  },
  validationLevel: "strict"
});

db.measurements.createIndex({ roomId: 1 });
db.measurements.createIndex({ timestamp: -1 });
db.measurements.createIndex({ roomId: 1, timestamp: -1 });