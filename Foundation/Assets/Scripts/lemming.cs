using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class lemming : MonoBehaviour {
	public float speed;
	public float walkPeriod;
	public Vector3 CurrentDirection;
	public Action CurrentAction;
	public Action PreviousAction;
	private float walkCount;
	private GameObject climbTarget;
	private GameObject avoidTarget;
	private Vector3 positionMarker;
	private Vector3 directionMarker;
	private List<GameObject> blocksClimbed;
	public bool debug;
	public int level;
	public int bestLevel;
	PlayerHandler PC;

	public enum Action {
		WALKING,
		AVOIDING,
		CLIMBING_UP,
		CLIMBING_ON,
		FALLING
	}

	// Use this for initialization
	void Start () {
		PC = this.transform.parent.GetComponent<PlayerHandler>();
		setAction(Action.WALKING);
		CurrentDirection = Vector3.forward;
		walkCount = 0;
		bestLevel = 0;
		blocksClimbed = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		/*if (this.rigidbody.constraints != (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ)) {
			this.rigidbody.constraints = (RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);
		}*/

		if (this.rigidbody.constraints != RigidbodyConstraints.FreezeRotation) {
			this.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		}


		switch (CurrentAction) {
			case Action.WALKING:
				this.rigidbody.useGravity = false;
				checkLevel();

				walkCount += Time.deltaTime;
				if (walkCount >= walkPeriod) {
					walkCount = 0;
					setTargetRotation();
				}

				RaycastHit hit;
				if (!Physics.Raycast(this.transform.position + CurrentDirection * this.transform.localScale.y, -Vector3.up, out hit)) {
					walkCount = walkPeriod;
				} 
				
				if (!Physics.Raycast(this.transform.position, -Vector3.up, out hit, GameHandler.BLOCK_SIZE)) {
					setAction(Action.FALLING);
				}

				walk();
				break;
			case Action.AVOIDING:
				this.rigidbody.useGravity = false;

				walkCount += Time.deltaTime;

				if (walkCount >= 2 * GameHandler.BLOCK_SIZE / speed) { // t = d / v
					walkCount = 0;
					setAction(Action.WALKING);
				}

				if (!Physics.Raycast(this.transform.position, -Vector3.up, GameHandler.BLOCK_SIZE)) {
					setAction(Action.FALLING);
				}

				walk();
				break;
			case Action.CLIMBING_UP:
				this.rigidbody.useGravity = false;
				this.rigidbody.velocity = Vector3.zero;
				CurrentDirection = Vector3.up;

				if (climbTarget != null) {
					if (this.collider.bounds.min.y > climbTarget.collider.bounds.max.y) {
						// done climbing up
						positionMarker = this.transform.position;
						setAction(Action.CLIMBING_ON);
					}
				}

				climb();
				break;
			case Action.CLIMBING_ON:
				this.rigidbody.useGravity = false;
				this.rigidbody.velocity = Vector3.zero;

				if (climbTarget != null) {
					CurrentDirection = directionMarker;
					if (Vector3.Distance(this.transform.position, positionMarker) > 1.5 * this.collider.bounds.size.x) {
						recordClimbedBlock(climbTarget);
						//climbTarget = null;
						walkCount = 0;
						setAction(Action.WALKING);
					}
				}

				walk();
				break;
			case Action.FALLING:
				this.rigidbody.useGravity = true;

				if (this.transform.position.y < 0) {
					Destroy(this.gameObject);
				}

				if (climbTarget != null) {
					recordFallOffBlock(climbTarget);
					climbTarget = null;
				}

				walkCount = walkPeriod;
				break;
		}
	}

	private void setAction(Action NextAction) {
		PreviousAction = CurrentAction;
		CurrentAction = NextAction;
		if (PreviousAction != CurrentAction && debug) {
			print("Lemming: " + CurrentAction);
		}
	}

	void OnCollisionEnter(Collision collision) {
		switch (CurrentAction) {
			case Action.WALKING:
				if (collision.gameObject.tag == "Block") {
					// Bumped into block
					GameObject blockObject = collision.gameObject;
					block block = blockObject.GetComponent<block>();
					if (block.level == this.level) {
						// Block is on my level
						if (!block.hasBlockAbove()) {
							//if (debug)	print("bar");
							// And no block above block I bumped into
							RaycastHit hit;
							bool somethingAboveMe = Physics.Raycast(this.transform.position, Vector3.up, out hit, GameHandler.BLOCK_SIZE);

							if (!somethingAboveMe || (somethingAboveMe && hit.transform.gameObject.tag != "Block")) {
									//if (debug)	print("yo");
									// No block above me, so climb
									climbTarget = blockObject;
									directionMarker = CurrentDirection;
									setAction(Action.CLIMBING_UP);
							} else {
								// Can't climb this block on my level, avoid it.
								if (debug)	print("avoiding block");
								//this.level = block.level;
								avoidThisCollision(collision);
							}
						} else {
							// Can't climb this block on my level, avoid it.
							if (debug)	print("avoiding block");
							//this.level = block.level;
							avoidThisCollision(collision);
						}
					} else {
						// Block not on my level, continue walking
						//if (debug)	print("foo " + blockObject.transform.position.y + " < " + this.transform.position.y);
					}
				}
				break;
			case Action.AVOIDING:
				if (collision.gameObject.tag == "Block") {
					// Bumped into block
					if (collision.gameObject != avoidTarget) {
						// Block isn't the block I'm currently avoiding
						GameObject blockObject = collision.gameObject;
						block block = blockObject.GetComponent<block>();
						if (block.level == this.level) {
							// Block is on my level
							if (!block.hasBlockAbove()) {
								//if (debug)	print("bar");
								// No block above block I bumped into
								RaycastHit hit;
								bool somethingAboveMe = Physics.Raycast(this.transform.position, Vector3.up, out hit, GameHandler.BLOCK_SIZE);

								if (!somethingAboveMe || (somethingAboveMe && hit.transform.gameObject.tag != "Block")) {
										//if (debug)	print("yo");
										// No block above me, so climb
										climbTarget = blockObject;
										directionMarker = CurrentDirection;
										setAction(Action.CLIMBING_UP);
								}
							} else {
								// Can't climb this block on my level, avoid it.
								if (debug)	print("avoiding block continued");
								avoidThisCollision(collision);
							}
						} /*else {
							// Block not on my level, continue walking
							if (debug)	print("foo " + blockObject.transform.position.y + " < " + this.transform.position.y);
						}*/
					}
				} else {
					if (collision.gameObject.tag == "Wall") {
						avoidThisCollision(collision);
					}
				}
				break;
			case Action.CLIMBING_UP:
				if (collision.gameObject.tag == "Block") {
					GameObject blockObject = collision.gameObject;
					if (blockObject != climbTarget) {
						setAction(Action.FALLING);
					}
				}
				break;
			case Action.FALLING:
				if (collision.gameObject.tag == "Block") {
					setAction(Action.WALKING);
					//level = collision.gameObject.GetComponent<block>().level + 1;
				}
				break;
		}

		if (collision.gameObject.tag == "Floor") {
			setAction(Action.WALKING);
			blocksClimbed.Clear();
			level = 0;
			bestLevel = 0;
		}
	}

	void OnCollisionStay(Collision collision) {
		if (CurrentAction == Action.FALLING) {
			if (collision.gameObject.tag == "Block" || collision.gameObject.tag == "Floor") {
				setAction(Action.WALKING);
			}
		}
	}

	private void avoidThisCollision(Collision collision) {
		foreach (ContactPoint contact in collision.contacts) {
			if (contact.otherCollider.tag == "Block" || contact.otherCollider.tag == "Wall") {
				avoidTarget = contact.otherCollider.gameObject;
				setRotation(contact.point - this.transform.position);
				turnRight();
				setAction(Action.AVOIDING);
				break;
			}
		}
	}

	private void recordClimbedBlock(GameObject blockObject) {
		block block = blockObject.GetComponent<block>();
		level = block.level + 1;

		//if (debug) 	print("index = " + block.level);
		blocksClimbed.Remove(blockObject);
		blocksClimbed.Add(blockObject);

		//PC.LemmingController.removeBlock(blockObject);
		PC.LemmingController.addBlock(blockObject);

		/*block.increasePriority(0.02f);

		if (block.level >= PC.LemmingController.HIGHEST_LEVEL) {
			highlightClimbedBlocks();
		}*/

		if (level >= bestLevel) {
			bestLevel = level;
			highlightClimbedBlocks();
		}
	}

	private void recordFallOffBlock(GameObject blockObject) {
		block block = blockObject.GetComponent<block>();
		block.decreasePriority(0.01f);
	}

	private void highlightClimbedBlocks() {
		foreach (GameObject b in blocksClimbed) {
			block block = b.GetComponent<block>();
			block.increasePriority(0.25f * (block.level+1)/(bestLevel+1));
		}
		climbTarget = null;
	}

	private void walk() {
		Vector3 newPosition = this.transform.position + this.transform.forward * speed * Time.deltaTime;
		if (GameHandler.isInBounds(newPosition)) {
			this.transform.position = newPosition;
		} else {
			CurrentDirection = -1 * this.transform.forward;
			walkCount = 0;
		}
	}

	private void climb() {
		Vector3 newPosition = this.transform.position + Vector3.up * speed * Time.deltaTime;
		this.transform.position = newPosition;
	}

	private void setRandomRotation() {
		//Vector3 newRotation = new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
		Quaternion rot = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), Vector3.up);
		this.transform.rotation = rot;
		CurrentDirection = this.transform.forward;
	}

	private void setRotation(Vector3 newRotation)  {
		this.transform.LookAt(this.transform.position + new Vector3(newRotation.x, 0, newRotation.z).normalized);
		CurrentDirection = this.transform.forward;
	}

	private void setRotation(float degrees) {
		this.transform.Rotate(Vector3.up, degrees);
		CurrentDirection = this.transform.forward;
	}

	private void addRotation(float degrees) {
		//this.transform.rotation *= Quaternion.Euler(0, degrees, 0);
		this.transform.RotateAround(this.transform.position, Vector3.up, degrees);
		CurrentDirection = this.transform.forward;
	}

	private void turnRight() {
		if (debug) print("turned right");
		//setRotation(this.transform.position + this.transform.right);
		addRotation(90);
	}

	private void setTargetRotation() {
		Vector3 targetRotation = PC.LemmingController.getTargetDirection(this.gameObject);
		if (targetRotation == Vector3.zero) {
			if (debug) print("rando");
			setRandomRotation();
		} else {
			if (debug) print("target = " + targetRotation);
			setRotation(PC.LemmingController.getTargetDirection(this.gameObject));
		}
	}

	private void checkLevel() {
		this.level = (int) Mathf.Floor(this.transform.position.y / GameHandler.BLOCK_SIZE);
	}

	//change to an action, default is a random one
	string changeDirection(string a = "random") {
		switch(a){
			case "random"://pick a random action
				int r = Random.Range(0,4);
				switch(r){
					case 0:
						return changeDirection("left");
					case 1:
						return changeDirection("forward");
					case 2:
						return changeDirection("right");
					case 3:
						return changeDirection("back");
				}
				break;
			case "left":
				CurrentDirection = Vector3.left;
				return "left";
			case "forward":
				CurrentDirection = Vector3.forward;
				return "forward";
			case "right":
				CurrentDirection = Vector3.right;
				return "right";
			case "back":
				CurrentDirection = Vector3.back;
				return "back";
			case "up":
				CurrentDirection = Vector3.up;
				return "up";
			case "down":
				CurrentDirection = Vector3.down;
				return "down";
		}
		return "failed";
	}

	public void OnDestroy() {
		PC.RemoveLemming(this.gameObject);
	}
}
