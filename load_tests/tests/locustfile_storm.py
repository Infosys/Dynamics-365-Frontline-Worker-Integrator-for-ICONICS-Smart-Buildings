import time,datetime, json, uuid, random, string, os
from locust import HttpUser, task, between,constant
from random import randint

class QuickstartUser(HttpUser):
    wait_time = constant(1)

    IoTHubName = os.getenv("IOT_HUB_NAME")
    host = "https://" + IoTHubName + ".azure-devices.net/"

    @task
    def view_item(self):
        deviceID = "BXConnector"
        IoTHubSASToken =  os.getenv("IOT_HUB_SAS_TOKEN")

        # RestAPI Version
        iotHubAPIVer = "2018-04-01"
        target = "/devices/" + deviceID + "/messages/events?api-version=" + iotHubAPIVer

        # Headers
        Headers = {}
        Headers['Authorization'] = IoTHubSASToken
        Headers['Content-Type'] = "application/json"

        # Message Payload
        current_datetime =  datetime.datetime.now()
        body = {}
        body['messageId'] = str(uuid.uuid4())
    
        # Generate a random string of the form 'A35' etc
        body['AssetName'] = random.choice(string.ascii_uppercase) + str(randint(10,99)) 
        body['AssetPath'] = "Campus\\Bldg\\Device\\Sensor\\AssetName"
        body['FaultName'] = "FSCFault"
        body['FaultActiveTime'] = str(current_datetime)
        body['MessageSource'] = "ICONICS FDD"
        body['FaultCostValue'] = "FaultCostNumeric"

        json_fault_msg = json.dumps(body)

        # Send Message
        resp = self.client.post(
            target,
            data=json_fault_msg,
            auth=None,
            headers=Headers,
            name="BXConnectorRequest",
        )
