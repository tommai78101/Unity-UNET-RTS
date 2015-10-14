﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Spawner : NetworkBehaviour {
	public GameObject spawnPrefab;
	public GameObject selectionManagerPrefab;
	public GameObject splitManagerPrefab;
	public GameObject mergeManagerPrefab;
	[SerializeField]
	public NetworkConnection owner;

	public override void OnStartLocalPlayer() {
		//I kept this part in, because I don't know if this is the function that sets isLocalPlayer to true, 
		//or this function triggers after isLocalPlayer is set to true.
		base.OnStartLocalPlayer();

		NetworkIdentity identity = this.GetComponent<NetworkIdentity>();
		if (this.isServer) {
			this.owner = identity.connectionToClient;
		}
		else {
			this.owner = identity.connectionToServer;
		}

		//On initialization, make the client (local client and remote clients) tell the server to call on an [ClientRpc] method.
		CmdCall();
	}

	[Command]
	public void CmdCall() {
		//Calling [ClientRpc] on the server.
		RpcLog();
	}

	[ClientRpc]
	public void RpcLog() {
		//First, checks to see what type of recipient is receiving this message. If it's server, the output message should tell the user what the type is.
		Debug.Log("RPC: This is " + (this.isServer ? " Server" : " Client"));

		//Second, initialize everything that is common for both server and client. 
		//Camera stuff
		GameObject[] cameraObjects = GameObject.FindGameObjectsWithTag("MainCamera");
		foreach (GameObject cam in cameraObjects) {
			Material borderMaterial = Resources.Load<Material>("Border");
			if (borderMaterial != null) {
				if (cam.GetComponent<PostRenderer>() == null) {
					cam.AddComponent<PostRenderer>();
				}
				PostRenderer renderer = cam.GetComponent<PostRenderer>();
				if (renderer != null) {
					renderer.borderMaterial = borderMaterial;
				}
			}

			if (cam.GetComponent<CameraPanning>() == null) {
				Debug.Log("Camera Panning is added to camera. Please check.");
				CameraPanning panning = cam.AddComponent<CameraPanning>();
				panning.cameraPanning = true;
			}
			else {
				if (!cam.GetComponent<CameraPanning>().cameraPanning) {
					cam.GetComponent<CameraPanning>().cameraPanning = true;
				}
			}
		}

		//Finally, initialize server only stuff or client only stuff.
		//Also, finally found a use-case for [Server] / [ServerCallback]. Simplifies things a bit.

		GameObject cam = null;
		foreach (GameObject camObj in cameraObjects) {
			if (camObj.Equals(Camera.main)) {
				cam = camObj;
				break;
			}
		}
		ServerInitialize(cam);
	}

	[ServerCallback]
	public void ServerInitialize(GameObject cam) {
		//Server code
		//This is run for spawning new non-player objects. Since it is a server calling to all clients (local and remote), it needs to pass in a
		//NetworkConnection that connects from server to THAT PARTICULAR client, who is going to own client authority on the spawned object.

		//Player unit
		GameObject obj = MonoBehaviour.Instantiate(this.spawnPrefab) as GameObject;
		obj.transform.position = this.transform.position;
		NetworkIdentity objIdentity = obj.GetComponent<NetworkIdentity>();
		NetworkServer.SpawnWithClientAuthority(obj, this.connectionToClient);

		//Setting up the camera
		if (cam != null) {
			Vector3 pos = obj.transform.position;
			pos.y = cam.transform.position.y;
			cam.transform.position = pos;
		}

		//Player selection manager
		GameObject manager = MonoBehaviour.Instantiate(this.selectionManagerPrefab) as GameObject;
		SelectionManager selectionManager = manager.GetComponent<SelectionManager>();
		if (selectionManager != null) {
			selectionManager.allObjects.Add(obj);
			selectionManager.authorityOwner = objIdentity.clientAuthorityOwner;
		}
		NetworkServer.SpawnWithClientAuthority(manager, this.connectionToClient);

		//Player split manager
		manager = MonoBehaviour.Instantiate(this.splitManagerPrefab) as GameObject;
		SplitManager splitManager = manager.GetComponent<SplitManager>();
		if (splitManager != null) {
			splitManager.selectionManager = selectionManager;
		}
		NetworkServer.SpawnWithClientAuthority(manager, this.connectionToClient);

		//Player merge manager
		manager = MonoBehaviour.Instantiate(this.mergeManagerPrefab) as GameObject;
		MergeManager mergeManager = manager.GetComponent<MergeManager>();
		if (mergeManager != null) {
			mergeManager.selectionManager = selectionManager;
		}
		NetworkServer.SpawnWithClientAuthority(manager, this.connectionToClient);
	}

	public void OnDestroy() {
		//By default, NetworkManager destroys game objects that were spawned into the game via NetworkServer.Spawn() or NetworkServer.SpawnWithClientAuthority().
		//This is why NetworkBehaviours and MonoBehaviours could not fire OnPlayerDisconnected() and OnDisconnectedFromServer() event methods. The
		//game objects the NetworkBehaviours and MonoBehaviours had been attached to are destroyed before they have the chance to fire the event methods.

		//This hasAuthority flag checking is still required, just like any other event methods from NetworkBehaviours.
		if (!this.hasAuthority) {
			return;
		}

		//This is called to destroy the camera panning. When the game ends, the player shouldn't be moving around.
		GameObject[] cams = GameObject.FindGameObjectsWithTag("MainCamera");
		foreach (GameObject cam in cams) {
			CameraPanning panning = cam.GetComponent<CameraPanning>();
			if (panning != null) {
				Debug.Log("Destroying camera panning.");
				Destroy(panning);
			}
		}
	}
}