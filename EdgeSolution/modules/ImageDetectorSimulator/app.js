'use strict';

var Transport = require('azure-iot-device-mqtt').Mqtt;
var Client = require('azure-iot-device').ModuleClient;
var Message = require('azure-iot-device').Message;
var looper;

const appleFinder = () => {
  let payload = {
    output_pin: 72, 
    output_value: false

  }
  var outputMsg = new Message(JSON.stringify(payload));
  client.sendOutputEvent('imageDetected', outputMsg, printResultFor('Sending Image Found message'));
}


Client.fromEnvironment(Transport, function (err, client) {
  if (err) {
    throw err;
  } else {
    client.on('error', function (err) {
      throw err;
    });

    // connect to the Edge instance
    client.open(function (err) {
      if (err) {
        throw err;
      } else {
        console.log('IoT Hub module client initialized');

        // Act on input messages to the module.
        client.on('stop', function () {
          clearInterval(looper)
        });

        client.onM('start', function () {
          looper = setInterval(appleFinder, 120000)
        });
      }
    });
  }
});


// Helper function to print results in the console
function printResultFor(op) {
  return function printResult(err, res) {
    if (err) {
      console.log(op + ' error: ' + err.toString());
    }
    if (res) {
      console.log(op + ' status: ' + res.constructor.name);
    }
  };
}
