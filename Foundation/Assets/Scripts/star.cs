using UnityEngine;
using System.Collections;

public class star : MonoBehaviour {
	public int winner;
	public int level;

	// Use this for initialization
	void Start () {
		checkLevel();
	}
	
	// Update is called once per frame
	void Update () {
		if (GameHandler.Instance.GAME_STATUS == Game.WON) {

		} else if (GameHandler.Instance.GAME_STATUS == Game.LOST) {

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
