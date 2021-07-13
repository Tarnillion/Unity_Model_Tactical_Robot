using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;

public class RobotComm : MonoBehaviour
{

    public static string IPAdd_1stOCT = "192";
    public static string IPAdd_2ndOCT = "168";
    public static string IPAdd_3rdOCT = "1";
    public static string IPAdd_4thOCT = "109";
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    public class TXPacket
    {
        public byte sync_1;
        public byte sync_2;
        public byte sync_3;
        public byte drive; //deadband, overflow protection
        public byte turn; //deadband, overflow protection
        public byte pan; //deadband
        public byte tilt; //deadband
        public byte zoom;
        public byte focus;
        public byte camSel;
        public byte camLi;
        public byte baseT; //deadband, overflow protection
        public byte shoulderT; //deadband, overflow protection
        public byte elbowT; //deadband, overflow protection, set bounds
        public byte wristT; //deadband, overflow protection
        public byte gripperT;
        public byte speed;
        public byte reset;
        public byte checksumHigh; //calculate check sum check >> 8 & 0xFF
        public byte checksumLow; //calculate check sum check & 0xFF

        public byte[] dataArray()
        {
            byte[] tmp = { sync_1, sync_2, sync_3, drive, turn, pan, tilt, zoom, focus, camSel, camLi, baseT, shoulderT, elbowT, wristT, gripperT, speed, reset, checksumHigh, checksumLow };
            return tmp;
        }
    }

    public class RobotController
    {

        public int txSize { get; set; }

        const int rxLength = 9;

        byte[] rx = new byte[rxLength];

        byte[] temp = new byte[rxLength * 3];

        int rxCalc = 0, rxCheck = 0;

        private int rxTimeoutCounter;

        private int rxTimeoutTime;

        public RobotController()
        {
            // initialization ?
            // test connection?
            //
        }

        //may not be needed due to this waiting on async thread?
        public void CheckRxTimeout()
        {
            if (rxTimeoutCounter >= rxTimeoutTime)
            {
                //stop
                //delay
                //reconnect
                //if reconnect then continue;
            }
        }

        public TXPacket BuildPacket()
        {
            TXPacket packet = new TXPacket()
            {
                sync_1 = 0x68,
                sync_2 = 0x65,
                sync_3 = 0x6C,
                drive = 0x6c,
                turn = 0x6f,
                pan = 0x74,
                tilt = 0x68,
                zoom = 0x65,
                focus = 0x72,
                camSel = 0x65,
                camLi = 0x62,
                baseT = 0x69,
                shoulderT = 0x67,
                elbowT = 0x72,
                wristT = 0x6f,
                gripperT = 0x62,
                speed = 0x6f,
                reset = 0x74,
                checksumHigh = 0x21,
                checksumLow = 0x21
            };

            return packet;
        }
    }

    public class AsynchronousClient
    {
        // The port number for the remote device.  
        private const int port = 80;

        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;



        public static void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the
                // remote device is "host.contoso.com".  
                byte[] ipAddr = { Convert.ToByte(IPAdd_1stOCT), Convert.ToByte(IPAdd_2ndOCT), Convert.ToByte(IPAdd_3rdOCT), Convert.ToByte(IPAdd_4thOCT) };
                IPAddress ipAddress = new IPAddress(ipAddr);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.  
                Socket client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                //build contoller and packet

                var controller = new RobotController();
                var packet = controller.BuildPacket();



                // Send test data to the remote device.  
                //Send(client, "GET / HTTP/1.1" + Environment.NewLine + Environment.NewLine);
                Send(client, packet.dataArray());
                Send(client, "GET / HTTP/1.1" + Environment.NewLine + Environment.NewLine);
                sendDone.WaitOne();

                // Receive the response from the remote device.  
                Receive(client);
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", response);

                // Release the socket.  //do we need to do this. or keep it open....setting a timeout control.
                client.Shutdown(SocketShutdown.Both);
                client.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.  
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // All the data has arrived; put it in response.  
                    if (state.sb.Length > 1)
                    {
                        response = state.sb.ToString();
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void Send(Socket client, byte[] data)
        {
            client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("client starting");
        AsynchronousClient.StartClient();
        Debug.Log("client finished");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
