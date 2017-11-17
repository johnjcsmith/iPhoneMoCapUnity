using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;

public class SimpleUDPSocketListner: IDisposable
{
	private Action<String> messageCallback;
	private bool isRunning = false;

	public static IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, 8080);

	public SimpleUDPSocketListner(Action<String> messageCallback) {
		this.messageCallback = messageCallback;
	}

	public void Start()
	{

		Debug.Log("Started listening to connection");
		isRunning = true;
		StartListening();
	}
	public void Stop()
	{

		isRunning = false;

		try
		{
			udp.Close();
			Debug.Log("Stopped listening to connection");
		}
		catch (Exception ex) { 
			Debug.LogError ("Error closing connection: " + ex.Message);
		}
	}

	public bool IsRunning() {
		return isRunning;
	}

	private readonly UdpClient udp = new UdpClient(remoteIP);


	private void StartListening()
	{
		
		udp.BeginReceive(Receive, new object());
	}


	private void Receive(IAsyncResult ar)
	{
		byte[] bytes = udp.EndReceive(ar, ref remoteIP);
		string message = Encoding.ASCII.GetString(bytes);

		messageCallback (message);

		StartListening();
	}

	public void Dispose() {

		this.Stop ();
	}
}




