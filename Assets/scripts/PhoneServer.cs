using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Threading;
using System.Collections;
using UnityEngine.UI;

public class PhoneServer : MonoBehaviour
{
    private Thread mThread;
    private Thread updThread;
    private TcpListener server;
    private static TcpClient client;
    private static NetworkStream stream;
    private IPAddress ipAddress = null;
    private static bool isConnected;
    private System.Object thisLock = new System.Object();
    private bool wasConnected;
    private AudioSource beginAudio;
    private Socket updSocket;
    public Text ipText;
    private bool tutorialRan;
    private AudioSource tutorialAudio;

    public static bool Init;
    public bool tutorialMode;
    private bool resetAvail;

    #region Unity Code
    private void Start()
    {
        isConnected = false;
        wasConnected = false;
        beginAudio = GetComponent<AudioSource>();
        Init = true;
        GameUtils.tutorialMode = tutorialMode;
        tutorialRan = false;
        if (tutorialMode)
        {
            tutorialAudio = GetComponents<AudioSource>()[1];
        }
    }

    private void Update()
    {
        if (Init && !beginAudio.isPlaying)
        {
            //Debug only
            //Time.timeScale = 0;
            //Init = false;
            //End debug
            Init = true;
            Time.timeScale = 1;

            SetIPAddress();
            ThreadStart ts = new ThreadStart(ServerUpdate);
            mThread = new Thread(ts);
            mThread.Start();
            ThreadStart updThreadStart = new ThreadStart(SendAutoConnect);
            updThread = new Thread(updThreadStart);
            updThread.Start();
            Init = false;
        }

        if (!wasConnected && isConnected && !tutorialMode)
        {
            NumberSpeech.PlayAudio(14);
            wasConnected = true;
            Time.timeScale = 1;
        }
        if(wasConnected && !isConnected)
        {
            NumberSpeech.PlayAudio(15);
            wasConnected = false;
            Time.timeScale = 0;
        }

        if(tutorialMode && !wasConnected && isConnected)
        {
                tutorialRan = true;
                BeginTutorial();
        }
        if (tutorialRan)
        {
            if (tutorialAudio.isPlaying)
            {
                resetAvail = true;
                GameUtils.playState = GameUtils.GamePlayState.SettingBall;
            }
            else
            {
                GameUtils.PlayerServe = true;
                Time.timeScale = 1;
                StartCoroutine(TutorialReset());
            }
        }
    }

    private IEnumerator TutorialReset()
    {
        if (resetAvail)
        {
            resetAvail = false;
            yield return new WaitForSeconds(45);
            NumberSpeech.PlayAudio(17);
            GameUtils.playState = GameUtils.GamePlayState.SettingBall;
            GameUtils.PlayerServe = true;
            resetAvail = true;
        }

    }

    private void BeginTutorial()
    {

        wasConnected = true;
        tutorialAudio.Play();
    }

    private void OnDisable()
    {
        try
        {
            server.Stop();
            client.Close();
            mThread.Abort();
        }
        catch { };
    }

    void OnApplicationQuit()
    {
        try { 
            server.Stop();
            if(client != null)
                client.Close();
            mThread.Abort();
            updThread.Abort();
            updSocket.Close();
        }
        catch { }
    }
    #endregion

    #region Udp Server, alt Thread
    private void SendAutoConnect()
    {
        updSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPAddress broadcast = IPAddress.Parse("192.168.1.255");
        string sendString = ipAddress.ToString() + ";" + Environment.MachineName;
        byte[] sendbuf = Encoding.ASCII.GetBytes(sendString);
        IPEndPoint ep = new IPEndPoint(broadcast, 11000);
        while (!isConnected)
        {
            updSocket.SendTo(sendbuf, ep);
            //Debug.Log("Message sent to the broadcast address");
            Thread.Sleep(2000);
        }
        updSocket.Close();
    }
    #endregion

    #region Server Code, alt Thread
    private void SetIPAddress()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress[] ipAddresses = ipHostInfo.AddressList;

        foreach (IPAddress ip in ipAddresses)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip;
                ipText.text = "Computer Code: " + ip;
            }
        }
        if (ipAddress == null)
        {
            throw new Exception("No Available Ip Address");
        }

        //SpeakIPAddress();
    }

    private void SpeakIPAddress()
    {
        string ipString = ipAddress.ToString();

        int counter = 0;
        foreach (char ch in ipString)
        {
            int res;
            if (ch == '.')
            {
                StartCoroutine(PlayGlobalAudio(12, counter++));
            }
            else if (int.TryParse(ch.ToString(), out res))
            {
                StartCoroutine(PlayGlobalAudio(res, counter++));
            }
            else
            {
                Debug.LogError("Error in IP to text conversion");
            }
        }
    }

    private IEnumerator PlayGlobalAudio(int num, int counter)
    {
        yield return new WaitForSeconds(1 + counter);
        NumberSpeech.PlayAudio(num);
    }

    private void ServerUpdate()
    {
        server = null;
        try
        {
            // Set the TcpListener on port 3333.
            Int32 port = 3333;
            server = new TcpListener(ipAddress, port);
            Debug.Log(ipAddress);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String dataString = null;

            // Enter the listening loop.
            while (true)
            {
                Thread.Sleep(10);

                Debug.Log("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                client = server.AcceptTcpClient();

                if (client != null)
                {
                    Debug.Log("Connected!");
                    lock (thisLock)
                    {
                        isConnected = true;
                        //Time.timeScale = 1;
                    }
                }

                dataString = null;
                stream = client.GetStream();
                int i;
                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    dataString = Encoding.ASCII.GetString(bytes, 0, i);
                    Debug.Log("Received:"+ dataString +"data");
                    if (dataString.Equals("up;"))
                    {
                        PaddleScript.ScreenPressDown = true;
                    }
                    else if (dataString.Equals("down;"))
                    {
                        PaddleScript.ScreenPressDown = false;
                    }
                }
                if (!client.Connected)
                {
                    Debug.Log("Disconnected");
                    isConnected = false;
                    //Time.timeScale = 0;
                }
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException:" + e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }
    }

    /// <summary>
    /// Public method for sending a message to phone
    /// </summary>
    public static void SendMessageToPhone(string message)
    {
        try { 
            // Get a stream object for reading and writing
            stream = client.GetStream();
            byte[] msg1 = Encoding.ASCII.GetBytes(message);
            stream.Write(msg1, 0, msg1.Length);
        }
        catch 
        {
            Debug.Log("Not connected");
            isConnected = false;
            //Time.timeScale = 0;
        }
    }
#endregion
}