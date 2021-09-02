'use strict';

var Transport = require('azure-iot-device-mqtt').Mqtt;
var Client = require('azure-iot-device').ModuleClient;
var hubClient = {};
var Message = require('azure-iot-device').Message;
var looper;

const appleFinder = () => {
  let payload = {
    output_pin: 72, 
    output_value: 'GPIO.LOW'

  }
  var outputMsg = new Message(JSON.stringify(payload));
  hubClient.sendOutputEvent('imageDetected', outputMsg, printResultFor('Sending Image Found message'));
}


Client.fromEnvironment(Transport, function (err, client) {
  if (err) {
    throw err;
  } else {
    hubClient = client;
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

        client.onMethod('start', function(request, response) {
          console.log('start timer');
          looper = setInterval(appleFinder, 120000)
         var responseBody = {
            message: 'started'
          };
          response.send(200, responseBody, function(err) {
            if (err) {
              console.log('failed sending method response: ' + err);
            } else {
              console.log('successfully sent method response');
            }
          });
        });

        client.onMethod('stop', function(request, response) {
          console.log('stop timer');
          clearInterval(looper);
         var responseBody = {
            message: 'stopped'
          };
          response.send(200, responseBody, function(err) {
            if (err) {
              console.log('failed sending method response: ' + err);
            } else {
              console.log('successfully sent method response');
            }
          });
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
