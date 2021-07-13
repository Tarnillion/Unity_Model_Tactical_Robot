using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Net;
public class RobotController : MonoBehaviour
{
    public TurretController TurretControllerObject;

    public string IP_4th_Octet = "160";

    public float delaySeconds = 0.1f;

    [Range(0.0f, 255.0f)]
    public uint DriveValue = 127;

    [Range(0.0f, 255.0f)]
    public uint TurnValue = 127;

    [Range(0.0f, 255.0f)]
    public uint BaseValue = 127;

    [Range(0.0f, 255.0f)]
    public uint ShoulderValue = 127;

    [Range(0.0f, 255.0f)]
    public uint ElbowValue = 127;

    [Range(0.0f, 255.0f)]
    public uint WristValue = 127;

    public bool ResetValues = false;

    public uint Gripper = 0;

    public uint PanValue = 127;

    public uint TiltValue = 127;

    public uint ZoomValue = 0;

    public uint FocusValue = 0;

    public uint CameraSelect = 0;

    public bool CameraLight = false;

    public bool HalfSpeed = false;



    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private bool notClosing = true;

    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    private void ListenForData()
    {
        try
        {
            byte[] ipAddr = { Convert.ToByte("192"), Convert.ToByte("168"), Convert.ToByte("10"), Convert.ToByte("160") };
            IPAddress ipAddress = new IPAddress(ipAddr);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 3380);
            socketConnection = new TcpClient();
            socketConnection.Connect(remoteEP);
            ////socketConnection.ReceiveTimeout = 10;
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                // Get a stream object for reading
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    if (notClosing)
                    {
                        int length;
                        // Read incomming stream into byte arrary.
                        while (socketConnection != null && (length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message.
                            string serverMessage = Encoding.ASCII.GetString(incommingData);
                            uint cBatt = incommingData[3];
                            uint dBatt = incommingData[4];
                            uint basePos = incommingData[5];
                            uint shldrPos = incommingData[6];

                            TurretControllerObject.BaseAngle = basePos;
                            TurretControllerObject.ShoulderAngle = shldrPos;

                            Debug.Log("server message received as: " + serverMessage);
                            Debug.Log("cBatt: " + cBatt.ToString() + "  dBatt: " + dBatt.ToString() + "  Base Position: " + basePos.ToString() + "  Shoulder Position: " + shldrPos.ToString());
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }


    private void SendMessage()
    {
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing.
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {

                byte[] robotPacket =
                {
                    0x53, 0x44, 0x52,
                    Convert.ToByte(DriveValue),
                    Convert.ToByte(TurnValue),
                    Convert.ToByte(PanValue),
                    Convert.ToByte(TiltValue),
                    Convert.ToByte(ZoomValue),
                    Convert.ToByte(FocusValue),
                    Convert.ToByte(CameraSelect),
                    Convert.ToByte(CameraLight),
                    Convert.ToByte(BaseValue),
                    Convert.ToByte(ShoulderValue),
                    Convert.ToByte(ElbowValue),
                    Convert.ToByte(WristValue),
                    Convert.ToByte(Gripper),
                    Convert.ToByte(HalfSpeed),
                    Convert.ToByte(0),
                    Convert.ToByte(0),
                    Convert.ToByte(0)
                };

                //string SDRPayLoad = "TTTTTTTTTTTTTTTTTTTT";
                //string clientMessage = SDRHeader + SDRPayLoad;
                // Convert string message to byte array.                
                //byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                
                stream.Write(robotPacket, 0, robotPacket.Length);
                //Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    // Use this for initialization
    void Start()
    {
        ConnectToTcpServer();
        SendMessage();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SendCommands());
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            socketConnection.Close();
            clientReceiveThread.Abort();
            notClosing = false;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    IEnumerator SendCommands()
    {
        yield return new WaitForSeconds(delaySeconds);
        SendMessage();
    }
}
