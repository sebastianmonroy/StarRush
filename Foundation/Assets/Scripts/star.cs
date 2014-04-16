using UnityEngine;
using System.Collections;

public class star : MonoBehaviour {
	private int winner;
	public int level;
	public bool resetButton = false;

	// Use this for initialization
	void Start () {
		checkLevel();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI() {
		if (GameHandler.Instance.GAME_STATUS == Game.WON) {
			resetButton = GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-25, 100, 50), "You won! :)");
		} else if (GameHandler.Instance.GAME_STATUS == Game.LOST) {
			resetButton = GUI.Button(new Rect(Screen.width/2-50, Screen.height/2-25, 100, 50), "You lost :(");
		}

		if (resetButton) {
			Network.Disconnect();
			Application.LoadLevel("game");
		}
	}

	public void checkLevel() {
		level = (int) Mathf.Floor(this.transform.position.y / GameHandler.BLOCK_SIZE);
	}


	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.tag == "Lemming") {
			lemming lem = collision.gameObject.GetComponent<lemming>();
			winner = lem.PC.PLAYER_NUM;
			if (lem.PC.isThisPlayer) {
				GameHandler.Instance.GAME_STATUS = Game.WON;
			} else {
				GameHandler.Instance.GAME_STATUS = Game.LOST;
			}
		}
	}
}
