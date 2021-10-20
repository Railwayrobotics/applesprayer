# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for
# full license information.

import RPi.GPIO as GPIO
import json

import time
import os
import sys
import asyncio
from six.moves import input
import threading
from azure.iot.device.aio import IoTHubModuleClient

async def main():
    try:
        if not sys.version >= "3.5.3":
            raise Exception( "The sample requires python 3.5.3+. Current version of Python: %s" % sys.version )
        print ( "IoT Hub Client for Python" )

        # The client object is used to interact with your Azure IoT hub.
        module_client = IoTHubModuleClient.create_from_edge_environment()

        # connect the client.
        await module_client.connect()

        GPIO.setmode(GPIO.BCM)
        GPIO.setup(18, GPIO.OUT)

        # interface the gpio
        async def write_to_gpio(data):
            print("writing value " + data["value"] + " to pin " + data["output_pin"])

            GPIO.output(data["output_pin"], data["value"])

        # define behavior for receiving an input message on input1
        async def input1_listener(module_client):
            while True:
                try: 
                    print("Listening..")

                    input_message = await module_client.receive_message_on_input("imageDetected")  # blocking call

                    print("the data in the message received on imageDetected was ")
                    print(input_message.data)
                    print("custom properties are")
                    print(input_message.custom_properties)

                    if not input_message is None and not input_message.data is None:
                       print("data is accepted")
                       message_string = input_message.data.decode('utf-8')
                       print(message_string)

                       data = json.loads(message_string)

                       print("Writing to gpio")
                       await write_to_gpio(data)

                    else:
                        print("Unrecognized message")

                except Exception as e:
                    print("******************* Exception")
                    print(e)

        # define behavior for halting the application
        def stdin_listener():
            while True:
                try:
                    selection = input("Press Q to quit\n")
                    if selection == "Q" or selection == "q":
                        print("Quitting...")
                        break
                except:
                    time.sleep(10)

        # Schedule task for C2D Listener
        listeners = asyncio.gather(input1_listener(module_client))

        print ( "The sample is now waiting for messages. ")

        # Run the stdin listener in the event loop
        loop = asyncio.get_event_loop()
        user_finished = loop.run_in_executor(None, stdin_listener)

        # Wait for user to indicate they are done listening for messages
        await user_finished

        # Cancel listening
        listeners.cancel()

        # Finally, disconnect
        await module_client.disconnect()
        GPIO.cleanup()

    except Exception as e:
        print ( "Unexpected error %s " % e )
        raise

if __name__ == "__main__":
    loop = asyncio.get_event_loop()
    loop.run_until_complete(main())
    loop.close()

    # If using Python 3.7 or above, you can use following code instead:
    # asyncio.run(main())