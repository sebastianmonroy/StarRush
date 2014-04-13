using UnityEngine;
using System.Collections;

public class visibility : MonoBehaviour {
	public bool visible;

	void Start () {

	}
	
	void Update () {
		if (!visible && this.gameObject.renderer.enabled) {
			this.gameObject.renderer.enabled = false;
		} else if (visible && !this.gameObject.renderer.enabled) {
			this.gameObject.renderer.enabled = true;
		}
	}
}
