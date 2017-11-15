using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text;

public class SimpleSocketHandler : IDisposable
{
	private const string LocalHost = "0.0.0.0";

	private readonly IPAddress serverAddress;
	private readonly int serverPortNumber;
	private readonly TcpListener serverTcpConnectionListener;
	private Action<String> messageCallback;

	private bool acceptingConnections = true;
	private readonly ManualResetEvent stopHandle = new ManualResetEvent(false);

	private Thread listenerThread;

	public SimpleSocketHandler(Action<String> messageCallback) : this(IPAddress.Parse(LocalHost), 8080) { 
		this.messageCallback = messageCallback;
	}

	public SimpleSocketHandler(IPAddress hostAddress, int portNumber)
	{
		serverAddress = hostAddress;
		serverPortNumber = portNumber;
		serverTcpConnectionListener = new TcpListener(serverAddress, serverPortNumber);
	}

	public void Start()
	{
		if (!acceptingConnections)
			return;

		listenerThread = new Thread(ListenForConnections);
		listenerThread.Start();
	}

	private void ListenForConnections()
	{
		serverTcpConnectionListener.Start();
		Debug.Log("Listening for new connections.");

		while (acceptingConnections)
		{
			var asyncResult = serverTcpConnectionListener.BeginAcceptTcpClient(HandleNewConnection, serverTcpConnectionListener);

			WaitHandle.WaitAny(new[] { stopHandle, asyncResult.AsyncWaitHandle });
		}

		serverTcpConnectionListener.Stop();
	}

	private void HandleNewConnection(IAsyncResult iar)
	{
		Debug.Log("New Connection Received.");

		TcpListener connectionListener = (TcpListener) iar.AsyncState;
		TcpClient client;
		try
		{
			client = connectionListener.EndAcceptTcpClient(iar);

			ServiceClient(client);

			// Continue accepting new connections
			connectionListener.BeginAcceptTcpClient(new AsyncCallback(HandleNewConnection), connectionListener);

		}
		catch (SocketException ex)
		{
			Debug.LogError("Unexpected error accepting connection in HandleNewConnection " + ex.Message );
			return;
		}
		catch (ObjectDisposedException)
		{
			
			Debug.Log("Connection closed.");
			return;
		} catch (Exception ex) 
		{
			Debug.LogError("Unexpected error accepting connection in HandleNewConnection " + ex.Message );
			return;
		}
	}


	private void ServiceClient(TcpClient client)
	{
		try
		{
			while (true)
			{


				var socket = client.Client;

				// Read the length prefix

				byte[] sizeinfo = new byte[4];

				int totalRead = 0, currentRead = 0;

				currentRead = totalRead = socket.Receive(sizeinfo);

				// If we havent got all 4 bytes of size info, keep reading until we do.
				while (totalRead < sizeinfo.Length && currentRead > 0)
				{
					currentRead = socket.Receive(sizeinfo, 
						totalRead,
						sizeinfo.Length - totalRead,
						SocketFlags.None);


					totalRead += currentRead;
				}



				// Read 4 Byte int representation
				int messageSize = BitConverter.ToInt32(sizeinfo, 0);


				// Read the data


				byte[] data = new byte[messageSize];


				totalRead = 0;


				// Read the first chunk of data and continue doing so until we have the amount specified in the length message
				do {
					currentRead = socket.Receive(data,
						totalRead,
						data.Length - totalRead,
						SocketFlags.None);
					totalRead += currentRead;
				} while (totalRead < messageSize && currentRead > 0);


				messageCallback(Encoding.ASCII.GetString(data, 0, totalRead));   

			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Unexpected error reading message." + ex.Message);
		}
	}

	public void Close()
	{
		acceptingConnections = false;
		stopHandle.Set();
		listenerThread.Join();
	}

	public void Dispose()
	{
		Close ();
	}
}