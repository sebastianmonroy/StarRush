using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHandler : MonoBehaviour {
	public int PLAYER_NUM;
	public bool isThisPlayer;
	
	public GestureHandler GestureHandler;
	public build BuildController;
	public LemmingController LemmingController;

	public List<GameObject> lemmings = new List<GameObject>();

	// Use this for initialization
	void Start () {
		GestureHandler = this.GetComponent<GestureHandler>();
		BuildController = this.GetComponent<build>();
		LemmingController = this.GetComponent<LemmingController>();
		this.networkView.group = this.PLAYER_NUM;	// Player-specific messages only sent to this player's group
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setAsPlayer() {
		isThisPlayer = true;
	}

	// BUILD NETWORKING FUNCTIONS
	[RPC]
	void SetTetrisType(int tetrisType) {
		BuildController.selectedTetrisType = tetrisType;
		Debug.Log("Build: SetTetrisType for Player " + PLAYER_NUM);
	}

	[RPC]
	void SetTetrisLocation(Vector3 loc) {
		BuildController.selectedTetrisLocation = loc;
		Debug.Log("Build: SetTetrisLocation for Player " + PLAYER_NUM);
	}

	[RPC]
	void SetTetrisRotation(Quaternion rot) {
		BuildController.selectedTetrisRotation = rot;
		Debug.Log("Build: SetTetrisRotation for Player " + PLAYER_NUM);
	}

	[RPC]
	void CreateTetris() {
		GameObject newTetris = BuildController.SpawnTetris();
		newTetris.GetComponent<TetriminoHandler>().setPreview(false);
		Debug.Log("Build: CreateTetris for Player " + PLAYER_NUM);
	}

	[RPC]
	void CreateLemming() {
		GameObject newLemming = BuildController.SpawnLemming();
		this.lemmings.Add(newLemming);
		Debug.Log("Build: CreateLemming for Player " + PLAYER_NUM);
	}

	public void RemoveLemming(GameObject lemming) {
		this.lemmings.Remove(lemming);
	}
}
