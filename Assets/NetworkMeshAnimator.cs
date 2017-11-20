using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class NetworkMeshAnimator {

	private UDPServer listner;
	private SkinnedMeshRenderer meshTarget;
	private UnityMainThreadDispatcher dispatcher;
	private bool isAcceptingMessages = false;

	private static NetworkMeshAnimator instance;

	public static NetworkMeshAnimator Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = new NetworkMeshAnimator();
			}
			return instance;
		}
	}

	private NetworkMeshAnimator() {
		
		this.listner  = new UDPServer ((String message) => { 
			if (isAcceptingMessages) {
				dispatcher.Enqueue (SetBlendShapesOnMainThread (message));
			}
		});


		EditorApplication.playModeStateChanged += PlayStateChanged;

		listner.Start ();

	}
		
	private static void PlayStateChanged(PlayModeStateChange state)
	{
		if (state.Equals (PlayModeStateChange.ExitingPlayMode)) {
		
			instance.StopAcceptingMessages ();
		}
	}

	public void StartAcceptingMessages() {
		Debug.Log("Started accepting messages");

		meshTarget = GameObject.Find ("BlendShapeTarget").GetComponent<SkinnedMeshRenderer> ();

		if (meshTarget == null) {
			Debug.LogError ("Cannot find BlendShapeTarget. Have you added it to your scene?");
			return;
		}

		if (UnityMainThreadDispatcher.Exists()) {
			dispatcher = UnityMainThreadDispatcher.Instance ();
		} else {
			Debug.LogError ("Cannot reach BlendShapeTarget. Have you added the UnityMainThreadDispatcher to your scene?");
		}

		isAcceptingMessages = true;

	}

	public void StopAcceptingMessages() {
		Debug.Log("Stoped accepting messages");
		isAcceptingMessages = false;
	}

	public bool IsAcceptingMessages() {
		return isAcceptingMessages;
	}

	public IEnumerator SetBlendShapesOnMainThread(string messageString) {

		foreach (string message in messageString.Split (new Char[] { '|' }))
		{
			var cleanString = message.Replace (" ", "").Replace ("msg:", "");
			var strArray  = cleanString.Split (new Char[] {'-'});

			if (strArray.Length == 2) {
				var weight = float.Parse (strArray.GetValue (1).ToString());

				var mappedShapeName = strArray.GetValue (0).ToString ().Replace ("_L", "Left").Replace ("_R", "Right");

				var index = meshTarget.sharedMesh.GetBlendShapeIndex (mappedShapeName);

				if (index > -1) {
					meshTarget.SetBlendShapeWeight (index, weight);
				}
			}
		}

		yield return null;
	}
}

