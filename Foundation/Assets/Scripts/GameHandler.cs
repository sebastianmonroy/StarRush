using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameHandler : MonoBehaviour {
	public static GameHandler Instance;
	public GameObject blockPrefab;
	public GameObject playerPrefab;
	public static float BLOCK_SIZE = 15;
	private GameObject FloorObject;
	public static Vector3 FLOOR_MIN;
	public static Vector3 FLOOR_MAX;
	public static float PRIORITY_DECAY_PERIOD = 2.0f;
	public static float PRIORITY_DECAY_AMOUNT = 0.01f;
	public int NUM_PLAYERS = 0;
	public int PLAYER_NUM = 0;
	public bool testBool = false;

	public Dictionary<NetworkPlayer, GameObject> playerList = new Dictionary<NetworkPlayer, GameObject>();

	void Start () {
		Instance = this;
        Network.isMessageQueueRunning = true;
		FloorObject = GameObject.FindWithTag("Floor");

		setFloorTiling();

		FLOOR_MIN = FloorObject.renderer.bounds.min;
		FLOOR_MAX = FloorObject.renderer.bounds.max;
	}

	void Update () {
		if (Network.isServer) {
			GameObject[] tetriminos = GameObject.FindGameObjectsWithTag("Tetris");
			foreach (GameObject go in tetriminos) {
				go.GetComponent<TetriminoHandler>().setPreview(false);
			}

			if (testBool) {
				networkView.RPC("CreateLemming", RPCMode.All, 1);
				testBool = false;
			}
		} else {
			if (testBool) {
				foreach (KeyValuePair<NetworkPlayer, GameObject> entry in playerList) {
					Debug.Log("Key = " + entry.Key + ", Value = " + entry.Value);
				}
				testBool = false;
			}
		}
	}

	[RPC]
	public void AddPlayer(NetworkPlayer networkPlayer) {
		NUM_PLAYERS++;
		spawnOriginBlock(NUM_PLAYERS);
		GameObject newPlayer = GameObject.Find("Player " + networkPlayer);
		newPlayer.active = true;
		playerList.Add(networkPlayer, newPlayer);
		Debug.Log("AddPlayer: Player " + networkPlayer + " added.");
	}

	/*[RPC]
	void RemovePlayer(NetworkPlayer player) {
		GameObject playerObject;
		playerList.TryGetValue(player, out playerObject);
		foreach (Transform t in playerObject.transform) {
			Destroy(t.gameObject);
		}
	}

	[RPC]
	void SetTetrisType(int tetrisID, NetworkMessageInfo info) {
		if (info.sender != Network.player || Network.isServer) {
			//int playerNum = int.Parse("" + info.sender);
			Debug.Log("playerNum = " + info.sender);
			GameObject playerObject;// = GameObject.Find("Player " + playerNum);
			playerList.TryGetValue(info.sender, out playerObject);
			build playerBuild = playerObject.GetComponent<build>();
			playerBuild.selectedTetrisID = tetrisID;
			Debug.Log("SetTetrisType for Player " + info.sender);
		}
	}

	[RPC]
	void SetTetrisLocation(Vector3 loc, NetworkMessageInfo info) {
		if (info.sender != Network.player || Network.isServer) {
			int playerNum = int.Parse("" + info.sender);
			GameObject playerObject = GameObject.Find("Player " + playerNum);
			build playerBuild = playerObject.GetComponent<build>();
			playerBuild.selectedTetrisLocation = loc;
			Debug.Log("SetTetrisLocation for Player " + playerNum);
		}
	}

	[RPC]
	void SetTetrisRotation(Quaternion rot, NetworkMessageInfo info) {
		if (info.sender != Network.player || Network.isServer) {
			int playerNum = int.Parse("" + info.sender);
			GameObject playerObject = GameObject.Find("Player " + playerNum);
			build playerBuild = playerObject.GetComponent<build>();
			playerBuild.selectedTetrisRotation = rot;
			Debug.Log("SetTetrisRotation for Player " + playerNum);
		}
	}

	[RPC]
	void CreateTetris(NetworkMessageInfo info) {
		if (info.sender != Network.player || Network.isServer) {
			int playerNum = int.Parse("" + info.sender);
			GameObject playerObject = GameObject.Find("Player " + playerNum);
			build playerBuild = playerObject.GetComponent<build>();
			GameObject newTetris = playerBuild.createTetris();
			newTetris.GetComponent<TetriminoHandler>().setPreview(false);
			Debug.Log("CreateTetris for Player " + playerNum);
		}
	}

	[RPC]
	public void AddPlayerToServer(NetworkPlayer networkPlayer) {
		if (Network.isServer) {
			if (playerList.ContainsKey(networkPlayer))
	        {
	            Debug.LogError("AddPlayerToServer: Player " + networkPlayer + " already exists!");
	        } else {
	        	NUM_PLAYERS++;
				spawnOriginBlock(NUM_PLAYERS);
				GameObject newPlayer = GameObject.Find("Player " + networkPlayer);
				newPlayer.active = true;
				playerList.Add(networkPlayer, newPlayer);
				Debug.Log("AddPlayerToServer: Player " + networkPlayer + " added.");
	        }
	    }
	}

	public void AddPlayersToClients() {
		if (Network.isServer) {
			foreach (KeyValuePair<NetworkPlayer, GameObject> entry in playerList) {
				//Debug.Log("Key = " + entry.Key + ", Value = " + entry.Value);
				networkView.RPC("AddPlayerToClient", RPCMode.Others, entry.Key);
			}
		}
	}

	[RPC]
	public void AddPlayerToClient(NetworkPlayer networkPlayer) {
		if (Network.isClient) {
			int playerNum = int.Parse("" + networkPlayer);
			GameObject newPlayer = GameObject.Find("Player " + playerNum);
			newPlayer.active = true;
			spawnOriginBlock(playerNum);
			playerList.Add(networkPlayer, newPlayer);

			if (Network.player == networkPlayer) {
				//PLAYER_NUM = int.Parse(networkPlayer);
				newPlayer.GetComponent<PlayerHandler>().setAsPlayer();
				PLAYER_NUM = playerNum;
				Debug.Log("AddPlayerToClient: Player " + networkPlayer + " assigned.");
			} else {
				Debug.Log("AddPlayerToClient: Player " + networkPlayer + " added to Player " + Network.player + "'s playerList.");
			}
		}
	}

	[RPC]
	void CreateLemming(int playerNum, NetworkMessageInfo info) {
		//Debug.Log("sender = " + info.sender);
		GameObject playerObject = GameObject.Find("Player " + playerNum);
		//playerList.TryGetValue(info.sender, out playerObject);
		build playerBuild = playerObject.GetComponent<build>();
		playerBuild.spawnLemming();
		Debug.Log("CreateLemming for Player " + playerNum);
	}*/

	public GameObject spawnOriginBlock(int playerNum) {
		Vector3 corner = Vector3.zero;
		switch (playerNum) {
			case 1:
				corner = new Vector3(FloorObject.renderer.bounds.min.x + BLOCK_SIZE/2, FloorObject.transform.position.y + FloorObject.renderer.bounds.max.y + BLOCK_SIZE/2, FloorObject.renderer.bounds.min.z + BLOCK_SIZE/2);
				break;
			case 2:
				corner = new Vector3(FloorObject.renderer.bounds.max.x - BLOCK_SIZE/2, FloorObject.transform.position.y + FloorObject.renderer.bounds.max.y + BLOCK_SIZE/2, FloorObject.renderer.bounds.max.z - BLOCK_SIZE/2);
				break;
			case 3:
				corner = new Vector3(FloorObject.renderer.bounds.min.x + BLOCK_SIZE/2, FloorObject.transform.position.y + FloorObject.renderer.bounds.max.y + BLOCK_SIZE/2, FloorObject.renderer.bounds.max.z - BLOCK_SIZE/2);
				break;
			case 4:
				corner = new Vector3(FloorObject.renderer.bounds.max.x - BLOCK_SIZE/2, FloorObject.transform.position.y + FloorObject.renderer.bounds.max.y + BLOCK_SIZE/2, FloorObject.renderer.bounds.min.z + BLOCK_SIZE/2);
				break;
			default:
				break;
		}

		GameObject originBlock = Instantiate(blockPrefab, corner, this.transform.rotation) as GameObject;
		originBlock.transform.localScale = Vector3.one * BLOCK_SIZE;
		originBlock.GetComponent<block>().playerNum = playerNum;

		GameObject playerObject = GameObject.Find("Player " + playerNum);
		originBlock.transform.parent = playerObject.transform;
		playerObject.GetComponent<LemmingController>().addBlock(originBlock);
		return originBlock;
	}

	private void setFloorTiling() {
		float tilingScale = (FloorObject.transform.parent.localScale.x * FloorObject.transform.localScale.x) / BLOCK_SIZE;
		
		FloorObject.renderer.material.mainTextureScale = new Vector2(tilingScale, tilingScale);
	}

	public static bool isInBounds(Vector3 test) {
		return (test.x > FLOOR_MIN.x && test.x < FLOOR_MAX.x && test.z > FLOOR_MIN.z && test.z < FLOOR_MAX.z && test.y > FLOOR_MIN.y);
	}

	public static bool isInBounds(GameObject test) {
		return (test.transform.position.x > FLOOR_MIN.x && test.transform.position.x < FLOOR_MAX.x && test.transform.position.z > FLOOR_MIN.z && test.transform.position.z < FLOOR_MAX.z && test.transform.position.y > FLOOR_MIN.y);
	}
}
