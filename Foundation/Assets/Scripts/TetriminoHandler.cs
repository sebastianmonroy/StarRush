using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TetriminoHandler : MonoBehaviour {
	public int playerNum;
	public bool isPreview = true;
	public bool isColliding;
	public bool isOutOfBounds;
	private Ray debugRay = new Ray();
	private float debugRayDistance = 0.0f;
	//private bool joined;
	public Material predictionGoodMaterial;
	public Material predictionBadMaterial;
	public Material originalMaterial;

	void Start () {
		setPreview(true);
		//joined = false;
	}
	
	void Update () {
		/*if (this.transform.position.y < 0) {
			Destroy(this.gameObject);
		}*/
		if (isPreview) {
			checkPrediction();
		}
	}

	public void setPlayerNum(int playerNum) {
		this.playerNum = playerNum;
		/*foreach (Transform t in this.transform) {
			block block = t.gameObject.GetComponent<block>();
			block.setPlayerNum(playerNum);
		}*/
	}

	public void setPreview(bool preview) {
		isPreview = preview;
		checkPrediction();
	}

	private void enableColliders(bool input) {
		foreach (Transform t in this.transform) {
			t.gameObject.collider.enabled = input;
		}
	}

	public void setX(float posX) {
		this.transform.position = new Vector3(Mathf.Clamp(posX, -90, 90), this.transform.position.y, this.transform.position.z);
		
	}

	public void setZ(float posZ) {
		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, Mathf.Clamp(posZ, -90, 90));
	
	}

	public void setXZ(float posX, float posZ) {
		setX(posX);
		setZ(posZ);
	}

	public void setRandomRotation() {
		int rotateCount;
		
		rotateCount = Random.Range((int) 0, (int) 4);
		incrementXRotation(rotateCount);

		rotateCount = Random.Range((int) 0, (int) 4);
		incrementYRotation(rotateCount);

		rotateCount = Random.Range((int) 0, (int) 4);
		incrementZRotation(rotateCount);
	}

	public void incrementYRotation(int amount) {
		//this.transform.rotation = new Vector3(this.transform.rotation.x, this.transform.rotation.y + 90*amount, this.transform.rotation.z);s
		this.transform.Rotate(0, 90 * amount, 0, Space.World);
	}

	public void incrementXRotation(int amount) {
		//this.transform.rotation = new Vector3(this.transform.rotation.x + 90*amount, this.transform.rotation.y, this.transform.rotation.z);
		this.transform.Rotate(90 * amount, 0, 0, Space.World);
	}

	public void incrementZRotation(int amount) {
		//this.transform.rotation = new Vector3(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z + 90*amount);
		this.transform.Rotate(0, 0, 90 * amount, Space.World);
	}

	public void incrementRotation(Vector3 aimDirection) {
		if (Mathf.Abs(aimDirection.x) >= Mathf.Abs(aimDirection.z)) {
			incrementZRotation((int) (aimDirection.x/Mathf.Abs(aimDirection.x)));
		} else {
			incrementXRotation((int) (aimDirection.z/Mathf.Abs(aimDirection.z)));
		}
	}

	public float getOffset(Vector3 dir) {
		Vector3 centerPosition = Vector3.zero;
		foreach (Transform t in this.transform) {
			if (t.localPosition == Vector3.zero) {
				centerPosition = t.gameObject.transform.position;
				break;
			}
		}

		//print("center = " + centerPosition);

		float offset = 0;
		if (dir == Vector3.left) {
			foreach (Transform t in this.transform) {
				if (isClose(t.position.y, centerPosition.y) && isClose(t.position.z, centerPosition.z) && t.position.x > centerPosition.x) {
					float temp = Mathf.Abs(centerPosition.x - t.position.x);
					if (temp > offset)	offset = temp;
				}
			}
		} else if (dir == Vector3.right) {
			foreach (Transform t in this.transform) {
				//print(t.position + " " + (t.position.y == centerPosition.y) + " " + (t.position.z == centerPosition.z) + " " + (t.position.x < centerPosition.x));
				if (isClose(t.position.y, centerPosition.y) && isClose(t.position.z, centerPosition.z) && t.position.x < centerPosition.x) {
					float temp = Mathf.Abs(centerPosition.x - t.position.x);
					if (temp > offset)	offset = temp;
				}
			}
		} else if (dir == Vector3.up) {
			foreach (Transform t in this.transform) {
				if (isClose(t.position.x, centerPosition.x) && isClose(t.position.z, centerPosition.z) && t.position.y < centerPosition.y) {
					float temp = Mathf.Abs(centerPosition.y - t.position.y);
					if (temp > offset)	offset = temp;
				}
			}
		} else if (dir == -Vector3.up) {
			foreach (Transform t in this.transform) {
				if (isClose(t.position.x, centerPosition.x) && isClose(t.position.z, centerPosition.z) && t.position.y > centerPosition.y) {
					float temp = Mathf.Abs(centerPosition.y - t.position.y);
					if (temp > offset)	offset = temp;
				}
			}
		} else if (dir == Vector3.forward) {
			foreach (Transform t in this.transform) {
				if (isClose(t.position.x, centerPosition.x) && isClose(t.position.y, centerPosition.y) && t.position.z < centerPosition.z) {
					float temp = Mathf.Abs(centerPosition.z - t.position.z);
					if (temp > offset)	offset = temp;
				}
			}
		} else if (dir == -Vector3.forward) {
			foreach (Transform t in this.transform) {
				if (isClose(t.position.x, centerPosition.x) && isClose(t.position.y, centerPosition.y) && t.position.z > centerPosition.z) {
					float temp = Mathf.Abs(centerPosition.z - t.position.z);
					if (temp > offset)	offset = temp;
				}
			}
		}

		checkPrediction();

		return offset;
	}

	private bool isClose(float a, float b) {
		return (Mathf.Abs(a-b) < 5);
	}

	private void checkPrediction() {
		isColliding = checkCollisions();
		isOutOfBounds = checkBounds();
		if (isPreview && (isColliding || isOutOfBounds)) {
			setMaterial(predictionBadMaterial);
			setTag("Prediction");
		} else if (isPreview && !(isColliding || isOutOfBounds)) {
			setMaterial(predictionGoodMaterial);
			setTag("Prediction");
		} else {
			setMaterial(originalMaterial);
			setTag("Block");
		}
	}

	public bool checkCollisions() {
		foreach (Transform t in this.transform) {
			if (t.GetComponent<block>().colliding) {
				return true;
			}
		}
		return false;
	}

	public bool checkBounds() {
		foreach (Transform t in this.transform) {
			if (t.GetComponent<block>().outOfBounds) {
				return true;
			}
		}
		return false;
	}

	public void setMaterial(Material mat) {
		foreach (Transform t in this.transform) {
			t.gameObject.renderer.material = mat;
		}
	}

	public void setTag(string str) {
		foreach (Transform t in this.transform) {
			t.gameObject.tag = str;
		}
	}
}
