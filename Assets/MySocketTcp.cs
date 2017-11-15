using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text;

public class MySocketTcp : IDisposable
{
	private const string LocalHost = "0.0.0.0";

	private readonly IPAddress _myAddress;
	private readonly int _myPortNumber;
	private readonly TcpListener _myTcpListener;
	private Action<String> messageCallback;

	private bool _stopping;
	private readonly ManualResetEvent _stopHandle = new ManualResetEvent(false);

	private Thread _mainThread;

	public MySocketTcp(Action<String> messageCallback) : this(IPAddress.Parse(LocalHost), 8080) { 
		this.messageCallback = messageCallback;
	}

	public MySocketTcp(IPAddress hostAddress, int portNumber)
	{
		_myAddress = hostAddress;
		_myPortNumber = portNumber;
		_myTcpListener = new TcpListener(_myAddress, _myPortNumber);
	}

	public void Start()
	{
		if (_stopping)
			return;

		_mainThread = new Thread(Listen);
		_mainThread.Start();
	}

	private void Listen()
	{
		_myTcpListener.Start();

		while (!_stopping)
		{
			var asyncResult = _myTcpListener.BeginAcceptTcpClient(OnAccept, _myTcpListener);
			//blocks until a client has connected to the server or stopping has been signalled
			WaitHandle.WaitAny(new[] { _stopHandle, asyncResult.AsyncWaitHandle });
		}

		_myTcpListener.Stop();
	}

	private void OnAccept(IAsyncResult iar)
	{
		TcpListener l = (TcpListener) iar.AsyncState;
		TcpClient c;
		try
		{
			Debug.Log("New Connection.");

			c = l.EndAcceptTcpClient(iar);
			ServiceClient(c);
			// keep listening for new connections
			l.BeginAcceptTcpClient(new AsyncCallback(OnAccept), l);

		}
		catch (SocketException ex)
		{
			Debug.Log("Error accepting TCP connection: " + ex.Message );

			// unrecoverable
//			_doneEvent.Set();
			return;
		}
		catch (ObjectDisposedException)
		{
			
			Debug.Log("Listen canceled.");
			return;
		} catch (Exception ex) 
		{
			Debug.Log("Something Else " + ex.Message);
		}
	}

	private void ServiceClient(TcpClient client)
	{
		try
		{
			while (true)
			{
				var socket = client.Client;
//				byte[] buffer = new byte[1024];
//				socket.Receive(buffer, 1024, buffer.Length, 0);
//
//				string message = Encoding.UTF8.GetString(buffer);
//
//				messageCallback(message);


				byte[] sizeinfo = new byte[4];


				//read the size of the message
				int totalread = 0, currentread = 0;


				currentread = totalread = socket.Receive(sizeinfo);


				while (totalread < sizeinfo.Length && currentread > 0)
				{
					currentread = socket.Receive(sizeinfo, 
						totalread, //offset into the buffer
						sizeinfo.Length - totalread, //max amount to read
						SocketFlags.None);


					totalread += currentread;
				}


				int messagesize = 0;


				//could optionally call BitConverter.ToInt32(sizeinfo, 0);
				messagesize |= sizeinfo[0];
				messagesize |= (((int)sizeinfo[1]) << 8);
				messagesize |= (((int)sizeinfo[2]) << 16);
				messagesize |= (((int)sizeinfo[3]) << 24);

				//create a byte array of the correct size
				//note:  there really should be a size restriction on
				//              messagesize because a user could send
				//              Int32.MaxValue and cause an OutOfMemoryException
				//              on the receiving side.  maybe consider using a short instead
				//              or just limit the size to some reasonable value
				byte[] data = new byte[messagesize];


				//read the first chunk of data
				totalread = 0;
				currentread = totalread = socket.Receive(data,
					totalread, //offset into the buffer
					data.Length - totalread, //max amount to read
					SocketFlags.None);


				//if we didn't get the entire message, read some more until we do
				while (totalread < messagesize && currentread > 0)
				{
					currentread = socket.Receive(data,
						totalread, //offset into the buffer
						data.Length - totalread, //max amount to read
						SocketFlags.None);
					totalread += currentread;
				}


				messageCallback(Encoding.ASCII.GetString(data, 0, totalread));   

			}
		}
		catch (Exception ex)
		{
			Debug.Log("Disconected." + ex.Message);
		}
	}

	public void Close()
	{
		_stopping = true;
		_stopHandle.Set();
		_mainThread.Join();
	}

	public void Dispose()
	{
		//TODO: dispose all IDisposable properties
	}
}