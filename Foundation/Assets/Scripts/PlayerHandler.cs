using UnityEngine;
using System.Collections;

public class PlayerHandler : MonoBehaviour {
	public int PLAYER_NUM;
	public bool isThisPlayer;
	public build BuildController;
	public LemmingController LemmingController;
	public GestureHandler GestureHandler;

	// Use this for initialization
	void Start () {
		BuildController = this.GetComponent<build>();
		LemmingController = this.GetComponent<LemmingController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void AddGestures() {
		GestureHandler = this.gameObject.AddComponent("GestureHandler") as GestureHandler;
		BuildController.GH = GestureHandler;
	}

	public void setAsPlayer() {
		isThisPlayer = true;
		AddGestures();
	}
}
