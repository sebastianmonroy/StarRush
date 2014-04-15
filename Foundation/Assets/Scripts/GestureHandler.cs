using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Gesture { CLICK, SCROLL_LEFT, SCROLL_RIGHT, SCROLL_UP, SCROLL_DOWN, SPACE_BAR, NOTHING };

public class GestureHandler : MonoBehaviour {
	public Gesture CurrentGesture;			// The gesture that is currently being executed, can be referenced from other scripts
	public Ray CurrentRay;					// The current Ray from the camera (set only to update during "CLICK" gestures)
	private float delayCount;				// Could be used during "CLICK" gestures to determine whether user is performing a dragging motion or an accidental touch 
	private float scrollThreshold = 10;		// Used to define the minimum length of a two-finger scroll gesture in order to be recognized by the game
	private PlayerHandler PC;

	void Start () {
		PC = this.gameObject.GetComponent<PlayerHandler>();
		CurrentGesture = Gesture.NOTHING;
		delayCount = 0;
	}
	
	void Update () {
		// Update CurrentGesture and CurrentRay every frame
		if (PC.isThisPlayer) {
			getInput();
		}
	}

	private void getInput() {
		// Handles assigning the Current Gesture and Current Ray from Camera based on user input
		if (Input.GetMouseButtonDown(0)) {
			// PC: Left Mouse Click
			CurrentRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			CurrentGesture = Gesture.CLICK;
		} else if (Input.GetAxis("Mouse ScrollWheel") != 0) {
			// PC: Scrollwheel Scroll
			float deltaY = Input.GetAxis("Mouse ScrollWheel");
			if (deltaY > 0) {
				CurrentGesture = Gesture.SCROLL_UP;
			} else {
				CurrentGesture = Gesture.SCROLL_DOWN;
			}
		} else if (Input.GetAxis("Horizontal") != 0) {
			// PC: Left, Right Arrows
			float deltaX = Input.GetAxis("Horizontal");
			if (deltaX > 0) {
				CurrentGesture = Gesture.SCROLL_RIGHT;
			} else {
				CurrentGesture = Gesture.SCROLL_LEFT;
			}
		} else if (Input.GetAxis("Vertical") != 0) {
			// PC: Up, Down Arrows
			float deltaX = Input.GetAxis("Vertical");
			if (deltaX > 0) {
				CurrentGesture = Gesture.SCROLL_UP;
			} else {
				CurrentGesture = Gesture.SCROLL_DOWN;
			}
		} else if (Input.touchCount == 1) {
			// Mobile: Single Touch Click
			delayCount += Time.deltaTime;
			if (delayCount >= 0.1f) {
				CurrentRay = Camera.main.ScreenPointToRay(Input.touches[0].position);
				CurrentGesture = Gesture.CLICK;
			} else {
				delayCount += Time.deltaTime;
				CurrentGesture = Gesture.NOTHING;
			}			
		} else if (Input.touchCount == 2) {
			// Mobile : Dual Touch Scroll
			delayCount = 0;
			float totalMoveX = 0;
			float totalMoveY = 0;
			foreach (Touch t in Input.touches) {
				totalMoveX += t.deltaPosition.x * Time.deltaTime/t.deltaTime;
				totalMoveY += t.deltaPosition.y * Time.deltaTime/t.deltaTime;
			}

			int deltaX = (int) (totalMoveX / Input.touchCount);
			int deltaY = (int) (totalMoveY / Input.touchCount);

			if (deltaX >= deltaY) {
				if (deltaX >= scrollThreshold) {
					CurrentGesture = Gesture.SCROLL_RIGHT;
				} else if (deltaX <= -scrollThreshold) {
					CurrentGesture = Gesture.SCROLL_LEFT;
				} else {
					CurrentGesture = Gesture.NOTHING;
				}
			} else {
				if (deltaY >= scrollThreshold) {
					CurrentGesture = Gesture.SCROLL_UP;
				} else if (deltaY <= -scrollThreshold) {
					CurrentGesture = Gesture.SCROLL_DOWN;
				} else {
					CurrentGesture = Gesture.NOTHING;
				}
			}
		} else if (Input.touchCount == 3) {
			delayCount = 0;
			CurrentGesture = Gesture.SPACE_BAR;
		} else if (Input.GetKeyDown(KeyCode.Space)) {
			// PC: Spacebar
			delayCount = 0;
			CurrentGesture = Gesture.SPACE_BAR;
		} else {
			// No Input Detected
			delayCount = 0;
			CurrentGesture = Gesture.NOTHING;
		}
	}

	/*public Gesture GetGesture() {
		return CurrentGesture;
	}

	public Ray GetRay() {
		return CurrentRay;
	}*/
}
