using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LemmingController : MonoBehaviour {
	public GameObject lemmingPrefab;
	public static int HIGHEST_LEVEL;
	public static List<List<GameObject>> allBlocks = new List<List<GameObject>>();

	// Use this for initialization
	void Start () {
		HIGHEST_LEVEL = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void addBlock(GameObject blockObject) {
		removeBlock(blockObject);
		block block = blockObject.GetComponent<block>();
		if (block.level >= allBlocks.Count) {
			for (int i = 0; i < block.level - allBlocks.Count; i++) {
				allBlocks.Add(new List<GameObject>());
			}
			allBlocks.Insert(block.level, new List<GameObject> {blockObject});
		} else {
			allBlocks[block.level].Add(blockObject);
		}

		if (block.level > HIGHEST_LEVEL) {
			HIGHEST_LEVEL = block.level;
		}
		//print("highest level = " + HIGHEST_LEVEL);
		//print("list length = " + allBlocks.Count);
		//printList();
	}

	public void removeBlock(GameObject blockObject) {
		block block = blockObject.GetComponent<block>();
		//print (allBlocks.Count + " >= " + block.level);
		if (allBlocks.Count > block.level) {
			allBlocks[block.level].Remove(blockObject);
			//printList();
		}
	}

	public void addTetrimino(GameObject tetriminoObject) {
		foreach (Transform t in tetriminoObject.transform) {
			if (t.gameObject.tag == "Block") {
				block block = t.gameObject.GetComponent<block>();
				block.checkLevel();
				if (!block.hasBlockAbove()) {
					//print("added block @ " + t.gameObject.GetComponent<block>().level);
					addBlock(t.gameObject);
				}
			}
		}
	}

	public Vector3 getTargetDirection(GameObject lemmingObject) {
		lemming lemming = lemmingObject.GetComponent<lemming>();
		Vector3 vectorSum = Vector3.zero;
		//float prioritySum = 0;
		if (allBlocks.Count > lemming.level) {
			foreach (GameObject blockObject in allBlocks[lemming.level]) {
				block block = blockObject.GetComponent<block>();
				Vector3 toBlock = (blockObject.transform.position - lemmingObject.transform.position);
				vectorSum += block.priority * new Vector3(toBlock.x, 0, toBlock.z) / (1 + toBlock.magnitude);
				//prioritySum += block.priority;
			}
		}
		Vector3 targetDirection = new Vector3(vectorSum.x, 0, vectorSum.z).normalized;
		return targetDirection;
	}

	private void printList() {
		string str = "List = {";
		foreach (List<GameObject> list in allBlocks) {
			str += list.Count + " ";
		}
		str += "}";
		print(str);
	}
}