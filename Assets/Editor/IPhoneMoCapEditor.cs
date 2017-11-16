using UnityEngine;
using UnityEditor;
using System.Net.Sockets;
using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections;


public class IPhoneMoCapEditor  : EditorWindow
{
//	Thread thread = null;

	SimpleUDPSocketListner sUDPListner = null;


	// Add menu named "My Window" to the Window menu
	[MenuItem("Window/iPhoneMoCap")]
	static void Init()
	{
		IPhoneMoCapEditor window = (IPhoneMoCapEditor)EditorWindow.GetWindow(typeof(IPhoneMoCapEditor));
		window.Show();
	}

	void OnGUI()
	{
		if (GUILayout.Button (sUDPListner == null ? "Start UDP Listener" : "Stop UDP Listener")) {

			if (UnityMainThreadDispatcher.Exists()) {
				if (sUDPListner == null) {

					sUDPListner = new SimpleUDPSocketListner ((String message) => { 


						UnityMainThreadDispatcher.Instance ().Enqueue (SetBlendShapeOnMainThread (message));
				
					});

					sUDPListner.Start ();

				} else {
					sUDPListner.Stop();

					sUDPListner = null;
				}

			} else {
				Debug.LogError ("Cannot start Server. Have you added the UnityMainThreadDispatcher to your scene?");
			}

		}
	}

	public IEnumerator SetBlendShapeOnMainThread(string message) {
		var mesh = GameObject.Find ("BlendShapeTarget").GetComponent<SkinnedMeshRenderer> ();

		var cleanString = message.Replace (" ", "").Replace ("msg:", "");
		var strArray  = cleanString.Split (new Char[] {'-'});

			if (strArray.Length == 2) {
			var weight = float.Parse (strArray.GetValue (1).ToString());

			var mappedShapeName = strArray.GetValue (0).ToString ().Replace ("_L", "Left").Replace ("_R", "Right");

			var index = mesh.sharedMesh.GetBlendShapeIndex (mappedShapeName);

			if (index > -1) {
				mesh.SetBlendShapeWeight (index, weight);
			}

		}
		yield return null;
	}
}