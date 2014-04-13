using UnityEngine;
using System.Collections;

public class PlayerHandler : MonoBehaviour {
	public int PLAYER_NUM;
	public bool isThisPlayer;
	
	public GestureHandler GestureHandler;
	public build BuildController;
	public LemmingController LemmingController;

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
}
