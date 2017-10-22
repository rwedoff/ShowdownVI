using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Threading;
using System.Collections;

public class PhoneServer : MonoBehaviour
{
    private Thread mThread;
    private TcpListener server;
    private static TcpClient client;
    private static NetworkStream stream;
    private IPAddress ipAddress = null;
    private static bool isConnected;
    private System.Object thisLock = new System.Object();
    private bool wasConnected;
    private AudioSource beginAudio;
    private bool init;

#region Unity Code
    private void Start()
    {
        isConnected = false;
        wasConnected = false;
        beginAudio = GetComponent<AudioSource>();
        init = true;
        //Time.timeScale = 0;
    }

    private void Update()
    {
        if (init && !beginAudio.isPlaying)
        {
            init = false;
            SetIPAddress();
            ThreadStart ts = new ThreadStart(ServerUpdate);
            mThread = new Thread(ts);
            mThread.Start();
        }

        if (!wasConnected && isConnected)
        {
            NumberSpeech.PlayAudio(13);
            wasConnected = true;
            Time.timeScale = 1;
        }
        if(wasConnected && !isConnected)
        {
            NumberSpeech.PlayAudio(14);
            wasConnected = false;
            Time.timeScale = 0;
        }
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
        if (server != null)
        {
            server.Stop();
            if(client != null)
                client.Close();
            mThread.Abort();
        }
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
            }
        }
        if (ipAddress == null)
        {
            throw new Exception("No Available Ip Address");
        }

        SpeakIPAddress();
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
                StartCoroutine(PlayGlobalAudio(10, counter++));
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
                }
                if (!client.Connected)
                {
                    Debug.Log("Disconnected");
                    isConnected = false;
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
        }
    }
#endregion
}