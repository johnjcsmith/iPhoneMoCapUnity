using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;

public class SimpleUDPSocketListner: IDisposable
{
	private Action<String> messageCallback;


	public static IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, 8080);


	public SimpleUDPSocketListner(Action<String> messageCallback) {
		this.messageCallback = messageCallback;
	}

	public void Start()
	{

		Debug.Log("Started listening");
		StartListening();
	}
	public void Stop()
	{
		try
		{
			udp.Close();
			Debug.Log("Stopped listening");
		}
		catch (Exception ex) { 
			Debug.LogError ("Error closing connection: " + ex.Message);
		}
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




