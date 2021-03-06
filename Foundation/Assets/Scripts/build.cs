﻿using UnityEngine;
using System.Collections;

public class build : MonoBehaviour {
	public GameObject[] tetrisPrefabs;
	public GameObject lemmingPrefab;

	public float waitDuration;				// How long to wait between acknowledging gestures
	private float waitCount;
	
	// CLIENT INFO
	public GameObject selectedTetris;
	private GameObject selectedBlock;
	private GameObject selectedPreviewBlock;
	public GameObject nextTetris;
	private int nextTetrisType;

	// SERVER INFO
	public int selectedTetrisType;
	public Vector3 selectedTetrisLocation;
	public Quaternion selectedTetrisRotation; 

	public PlayerHandler PC;

	public bool debug;

	void Start () {
		PC = this.transform.GetComponent<PlayerHandler>();
		waitCount = waitDuration/5;	// Don't feel like waiting as long for the first tetris block to spawn
		//getNextTetris();
	}
	
	void Update () {
		if (PC.isThisPlayer) {
			// Only run Update if this is the current Player's build script.
			if (PC != null && selectedTetris == null && GameHandler.Instance.GAME_STATUS == Game.STARTED) {
				getNextTetris();
			}

			if (waitCount >= waitDuration && PC.GestureHandler != null) {
				// Prevents multiple gestures from being registered when only one was intended
				// Prevents any actions from being performed if there is no gesture handler
				switch(PC.GestureHandler.CurrentGesture) {
					// Handles performing certain "build" actions based on the user input detected in GestureHandler.cs
					case Gesture.CLICK:
						// CLICK gesture detected
						RaycastHit hit;
						if (Physics.Raycast(PC.GestureHandler.CurrentRay, out hit, Mathf.Infinity, ~(1 << 8))) {
							if (debug) 	print("clicked on " + hit.transform.gameObject.tag);
							TetriminoHandler selectedTetrimino = selectedTetris.GetComponent<TetriminoHandler>();
							if (hit.transform.gameObject.tag == "Block" && hit.transform.gameObject.GetComponent<block>().PC == this.PC) {
								// only able to click on blocks that belong to this player
								if (selectedBlock != null) {
									// remove previous preview if necessary
									selectedBlock.GetComponent<block>().destroyPreview();
								}
								// preview available sides for selected block
								selectedBlock = hit.transform.gameObject;
								selectedBlock.GetComponent<block>().previewSides();
								if (selectedTetris.active) {
									selectedTetris.active = false;
								}
								waitCount = waitDuration;
							} else if (hit.transform.gameObject.tag == "Preview") {
								// show tetris prediction when preview block is touched
								selectedPreviewBlock = hit.transform.gameObject;
								selectedTetris.active = true;
								CorrectPrediction();
								waitCount = 0;
							} else if (hit.transform.gameObject.tag == "Prediction") {
								// create tetrimino where the prediction is (if not colliding or out of bounds)
								if (!selectedTetrimino.isColliding && !selectedTetrimino.isOutOfBounds && selectedBlock != null) {
									selectedTetrimino.setPreview(false);
									PC.LemmingController.addTetrimino(selectedTetris);
									selectedBlock.GetComponent<block>().destroyPreview();
									
									PC.networkView.RPC("CreateTetris", RPCMode.Others);
									getNextTetris();
									waitCount = 0;
								}
							}
						}
						break;
					case Gesture.SCROLL_LEFT:
						// SCROLL_LEFT gesture detected
						if (debug) 	print("scroll left");
						if (selectedTetris.active && selectedTetris.GetComponent<TetriminoHandler>().isPreview) {
							// If there is currently a selected tetris block, rotate it accordingly
							selectedTetris.GetComponent<TetriminoHandler>().incrementYRotation(1);
							CorrectPrediction();
							waitCount = 0;
						}
						break;
					case Gesture.SCROLL_RIGHT:
						// SCROLL_RIGHT gesture detected
						if (debug) 	print("scroll right");
						if (selectedTetris.active && selectedTetris.GetComponent<TetriminoHandler>().isPreview) {
							// If there is currently a selected tetris block, rotate it accordingly
							selectedTetris.GetComponent<TetriminoHandler>().incrementYRotation(-1);
							CorrectPrediction();
							waitCount = 0;
						}
						break;
					case Gesture.SCROLL_DOWN:
						// SCROLL_DOWN gesture detected
						if (debug) 	print("scroll down");
						if (selectedTetris.active && selectedTetris.GetComponent<TetriminoHandler>().isPreview) {
							// If there is currently a selected tetris block, rotate it accordingly
							selectedTetris.GetComponent<TetriminoHandler>().incrementRotation(selectedTetris.transform.position - this.transform.position);
							CorrectPrediction();
							waitCount = 0;
						}
						break;
					case Gesture.SCROLL_UP:
						// SCROLL_UP gesture detected
						if (debug) 	print("scroll up");
						if (selectedTetris.active && selectedTetris.GetComponent<TetriminoHandler>().isPreview) {
							// If there is currently a selected tetris block, rotate it accordingly
							selectedTetris.GetComponent<TetriminoHandler>().incrementRotation(selectedTetris.transform.position - this.transform.position);
							CorrectPrediction();
							waitCount = 0;
						}
						break;
					case Gesture.SPACE_BAR:
						// SPACE_BAR gesture detected
						PC.networkView.RPC("CreateLemming", RPCMode.Others);
						PC.CreateLemming();
						break;
					default:
						break;
				}
			}
			waitCount += Time.deltaTime;
		} else {
			// Prevent other players' blocks from rendering like predictions.
			foreach (Transform t in this.transform) {
				if (t.gameObject.tag == "Tetris") {
					t.gameObject.GetComponent<TetriminoHandler>().setPreview(false);
				}
			}
		}
	}

	public void CorrectPrediction() {
		Vector3 dir = selectedPreviewBlock.GetComponent<block_preview>().direction;
		Vector3 offset = dir * selectedTetris.GetComponent<TetriminoHandler>().getOffset(dir);
		selectedTetris.transform.position = selectedPreviewBlock.transform.position + offset;

		selectedTetrisLocation = selectedTetris.transform.position;
		selectedTetrisRotation = selectedTetris.transform.rotation;
		PC.networkView.RPC("SetTetrisLocation", RPCMode.Others, selectedTetrisLocation);
		PC.networkView.RPC("SetTetrisRotation", RPCMode.Others, selectedTetrisRotation);
	}

	public GameObject instantiateTetris(int tetrisID) {
		GameObject tetrisObject = Instantiate(tetrisPrefabs[tetrisID]) as GameObject;
		tetrisObject.transform.parent = this.transform;
		return tetrisObject;
	}

	public GameObject SpawnTetris() {
		//Debug.Log("create tetris");
		selectedTetris = instantiateTetris(selectedTetrisType);
		selectedTetris.GetComponent<TetriminoHandler>().setPreview(false);
		selectedTetris.transform.position = selectedTetrisLocation;
		selectedTetris.transform.rotation = selectedTetrisRotation;
		selectedTetris.GetComponent<TetriminoHandler>().playerNum = PC.PLAYER_NUM;
		PC.LemmingController.addTetrimino(selectedTetris);
		return selectedTetris;
	}

	public void getNextTetris() {
		//Debug.Log("get next tetris");
		//print ("get next");
		int tetrisType;
		if (nextTetris == null) {
			tetrisType = Random.Range((int) 0, (int) 5);
			selectedTetris = instantiateTetris(tetrisType);
			selectedTetrisType = tetrisType;
		} else {
			selectedTetris = nextTetris;
			selectedTetrisType = nextTetrisType;
		}
		PC.networkView.RPC("SetTetrisType", RPCMode.Others, selectedTetrisType);
		selectedTetris.GetComponent<TetriminoHandler>().playerNum = PC.PLAYER_NUM;
		selectedTetris.active = false;

		nextTetrisType = Random.Range((int) 0, (int) 5);
		nextTetris = instantiateTetris(nextTetrisType);
		nextTetris.GetComponent<TetriminoHandler>().playerNum = PC.PLAYER_NUM;
		nextTetris.active = false;
	}

	public GameObject SpawnLemming() {
		GameObject lemming = Instantiate(lemmingPrefab, this.transform.position, this.transform.rotation) as GameObject;
		lemming.transform.parent = this.transform;
		return lemming;
	}
}