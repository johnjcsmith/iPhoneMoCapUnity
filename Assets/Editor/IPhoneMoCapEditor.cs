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



	[MenuItem("iPhoneMoCap/MeshPreview")]
	static void Init()
	{
		IPhoneMoCapEditor window = (IPhoneMoCapEditor)EditorWindow.GetWindow(typeof(IPhoneMoCapEditor));
		window.Show();
	}

	void OnGUI()
	{

		if (EditorApplication.isPlaying) {

			if (GUILayout.Button (NetworkMeshAnimator.Instance.isRunning () ? "Dissable Mesh Preview" : "Enable Mesh Preview")) {
				if (NetworkMeshAnimator.Instance.isRunning ()) {
					NetworkMeshAnimator.Instance.Stop ();
				} else {
					NetworkMeshAnimator.Instance.Start ();
				}
			}

		} else {
			GUILayout.Label ("Please run your scene to enable MeshPreview.");
		}


	}


}