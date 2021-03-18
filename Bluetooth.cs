using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class Bluetooth
{
    public string DeviceName = "DSD TECH";
    public string ServiceUUID = "FFE0";
    public string Characteristic = "FFE1";

    private string bleState = "Disconnected";

    enum States
    {
        None,
        Scan,
        Connect,
        Subscribe,
        Unsubscribe,
        Disconnect,
        Communication,
    }

    private bool _workingFoundDevice = true;
    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private bool _foundID = false;

    // this is our hm10 device
    private string _hm10;

    void Reset()
    {
        _workingFoundDevice = false;    // used to guard against trying to connect to a second device while still connecting to the first
        _connected = false;
        _timeout = 0f;
        _state = States.None;
        _foundID = false;
        _hm10 = null;
    }

    void SetState(States newState, float timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    public string BleState()
    {
        return bleState;
    }

    void StartProcess()
    {
        bleState = "Initializing...";

        Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(States.Scan, 0.1f);
            bleState = "Initialized";

        }, (error) => {

            BluetoothLEHardwareInterface.Log("Error: " + error);
        });
    }

    public void Start()
    {
        StartProcess();
    }

    public void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        break;

                    case States.Scan:
                        bleState = "Scanning for HM10 devices...";

                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {

                            // we only want to look at devices that have the name we are looking for
                            // this is the best way to filter out devices
                            if (name.Contains(DeviceName))
                            {
                                _workingFoundDevice = true;

                                // it is always a good idea to stop scanning while you connect to a device
                                // and get things set up
                                BluetoothLEHardwareInterface.StopScan();
                                bleState = "Connected";

                                // add it to the list and set to connect to it
                                _hm10 = address;

                                bleState = "Found HM10";

                                SetState(States.Connect, 0.5f);

                                _workingFoundDevice = false;
                            }

                        }, null, false, false);
                        break;

                    case States.Connect:
                        // set these flags
                        _foundID = false;

                        bleState = "Connecting to HM10";

                        // note that the first parameter is the address, not the name. I have not fixed this because
                        // of backwards compatiblity.
                        // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                        // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                        // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                        BluetoothLEHardwareInterface.ConnectToPeripheral(_hm10, null, null, (address, serviceUUID, characteristicUUID) => {

                            if (IsEqual(serviceUUID, ServiceUUID))
                            {
                                // if we have found the characteristic that we are waiting for
                                // set the state. make sure there is enough timeout that if the
                                // device is still enumerating other characteristics it finishes
                                // before we try to subscribe
                                if (IsEqual(characteristicUUID, Characteristic))
                                {
                                    _connected = true;
                                    SetState(States.Subscribe, 2f);

                                    bleState = "Connected to HM10";
                                }
                            }
                        }, (disconnectedAddress) => {
                            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                            bleState = "Disconnected";
                        });
                        break;

                    case States.Subscribe:
                        bleState = "Subscribing to HM10";

                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_hm10, ServiceUUID, Characteristic, null, (address, characteristicUUID, bytes) => {
                            bleState = "Received Serial: " + Encoding.UTF8.GetString(bytes);
                            //int.TryParse(Encoding.UTF8.GetString(bytes), out thrust);
                            //thrust += 50;

                            //if (thrust > 60)
                            //{
                                //rocketController.Thrust(thrust / 300.0f);
                            //}
                            //else { rocketController.DisableBooster(); }


                        });
                        // set to the none state and the user can start sending and receiving data
                        _state = States.None;
                        bleState = "Waiting...";

                        break;

                    case States.Unsubscribe:
                        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_hm10, ServiceUUID, Characteristic, null);
                        SetState(States.Disconnect, 4f);
                        break;

                    case States.Disconnect:
                        if (_connected)
                        {
                            BluetoothLEHardwareInterface.DisconnectPeripheral(_hm10, (address) => {
                                BluetoothLEHardwareInterface.DeInitialize(() => {

                                    _connected = false;
                                    _state = States.None;
                                });
                            });
                        }
                        else
                        {
                            BluetoothLEHardwareInterface.DeInitialize(() => {

                                _state = States.None;
                            });
                        }
                        break;
                }
            }
        }
    }

    string FullUUID(string uuid)
    {
        return "0000" + uuid + "-0000-1000-8000-00805F9B34FB";
    }

    bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);
        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }
}
