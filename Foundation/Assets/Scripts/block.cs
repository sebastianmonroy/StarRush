using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class block : MonoBehaviour {
	public GameObject BlockPreviewPrefab;
	public bool colliding = false;
	public bool outOfBounds = false;
	public float priority;
	private float decayCount;
	private float decayPeriod;
	public int level;
	public bool debug;
	private PlayerHandler PC;
	public int playerNum;

	// Use this for initialization
	void Start () {
		priority = 0;
		decayCount = 0;
		decayPeriod = GameHandler.PRIORITY_DECAY_PERIOD;
	}

	// Update is called once per frame
	void Update () {
		if (PC == null) {
			if (playerNum == 0 && this.transform.parent.GetComponent<TetriminoHandler>() != null) {
				playerNum = this.transform.parent.GetComponent<TetriminoHandler>().playerNum;
			} else if (playerNum != 0) {
				PC = this.transform.parent.parent.GetComponent<PlayerHandler>();
			}
		} else {
			if (this.transform.parent.gameObject.tag == "Tetris") {
				if (this.transform.parent.GetComponent<TetriminoHandler>().isPreview) {
					checkCollisions();
					checkBounds();
				} else {
					updatePriority();
					checkLevel();
				}
			} else {
				updatePriority();
				checkLevel();
			}
		}
	}

	/*public void setPlayerNum(int playerNum) {
		this.playerNum = playerNum;
		PC = GameObject.Find("Player " + playerNum).GetComponent<PlayerHandler>();
	}*/

	public void updatePriority() {
		if (decayCount >= decayPeriod) {
			decreasePriority(GameHandler.PRIORITY_DECAY_AMOUNT);
			decayCount = 0;
		} else {
			decayCount += Time.deltaTime;
		}
	}

	public void increasePriority(float amount) {
		priority += amount;
		if (priority > 1.0f) {
			priority = 1.0f;
		}
		updateColor();
		PC.LemmingController.addBlock(this.gameObject);

		if (debug)	print("Block: priority = " + priority);
	}

	public void decreasePriority(float amount) {
		priority -= amount;
		if (priority < 0.02f) {
			priority = 0.02f;
		}
		updateColor();

		if (debug)	print("Block: priority = " + priority);
	}

	public void updateColor() {
		this.renderer.material.color = new Color(1.0f - priority, 1.0f - priority, 1.0f, 1.0f);
	}
	
	public bool hasBlockAbove(){
		RaycastHit hit;
		Ray r = new Ray(transform.position, Vector3.up);
		
		if (Physics.Raycast(r, out hit, GameHandler.BLOCK_SIZE)) {
			if (hit.transform.gameObject.tag == "Block") {
				return true;
			}
		}
		return false;
	}

	public void previewSides() {
		destroyPreview();

		Vector3[] directions = {Vector3.left, Vector3.right, Vector3.forward, -Vector3.forward, -Vector3.up, Vector3.up};
		List<Vector3> potentials = new List<Vector3>();

		RaycastHit hit;
		foreach (Vector3 v in directions) {
			if (!Physics.Raycast(this.transform.position, v, out hit, this.renderer.bounds.size.x, 1 << 9)) {
				potentials.Add(v);
			}
		}

		foreach (Vector3 p in potentials) {
			Vector3 potentialPosition = this.transform.position + p * this.renderer.bounds.size.x;
			if (GameHandler.isInBounds(potentialPosition)) {
				GameObject previewBlock = Instantiate(BlockPreviewPrefab, potentialPosition, this.transform.rotation) as GameObject;
				previewBlock.transform.localScale = Vector3.one * GameHandler.BLOCK_SIZE/2;
				//previewBlock.transform.parent = this.transform;
				previewBlock.GetComponent<block_preview>().direction = p;
				previewBlock.transform.parent = this.transform;
			}
		}
	}

	public void destroyPreview() {
		foreach (Transform t in this.transform) {
			if (t.gameObject.tag == "Preview") {
				Destroy(t.gameObject);
			}
		}
	}

	public void checkBounds() {
		if (!outOfBounds && !GameHandler.isInBounds(this.transform.position)) {
			outOfBounds = true;
		} else if (outOfBounds && GameHandler.isInBounds(this.transform.position)) {
			outOfBounds = false;
		}
	}

	public void checkCollisions() {
		if (!colliding && isColliding()) {
			colliding = true;
		} else if (colliding && !isColliding()) {
			colliding = false;
		}
	}

	public void checkLevel() {
		level = (int) Mathf.Floor(this.transform.position.y / GameHandler.BLOCK_SIZE);
	}

	public bool isColliding() {
		GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");

		foreach (GameObject b in blocks) {
			if (b != this.gameObject) {
				if (this.collider.bounds.Intersects(b.collider.bounds)) {
					//print("collision with " + b.gameObject.name);
					return true;
				}
			}
		}

		return false;
	}
}