using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Threading;

public class UDPServer: IDisposable
{

	public static IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, 49452);

	private IPEndPoint broadCastIP = new IPEndPoint(IPAddress.Broadcast, 49452);

	private Action<String> messageCallback;
	private UdpClient udp = new UdpClient(remoteIP);

	public UDPServer(Action<String> messageCallback) {
		this.messageCallback = messageCallback;
	}

	public void Start()
	{

		StartListening();
		Debug.Log("Started listening to UDP connection");
	}

	public void Stop()
	{
		try
		{
			udp.Close();
			Debug.Log("Stopped listening to UDP connection");
		}
		catch (Exception ex) { 
			Debug.LogError ("Error closing connection: " + ex.Message);
		}
	}


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

	public void SendBroadCast() {
		
		UdpClient broadCastClient = new UdpClient();


		byte[] bytes = Encoding.ASCII.GetBytes("iPhoneMoCapBroadCast");
		broadCastClient.Send(bytes, bytes.Length, broadCastIP);
		broadCastClient.Close();
	}
}




