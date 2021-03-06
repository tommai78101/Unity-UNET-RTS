﻿using UnityEngine;
using System.Collections;

namespace MultiPlayer {
	public class CanvasSwitch : MonoBehaviour {
		public CanvasGroup canvasGroup;
		public bool showCanvas;
		public KeyCode keyCode;

		public void Start() {
			if (this.canvasGroup == null) {
				Debug.LogError("Canvas group has not been set. Please check.");
			}
			this.showCanvas = false;
			this.ToggleCanvas();
		}

		public void Update() {
			if (Input.GetKeyUp(this.keyCode)) {
				this.showCanvas = !this.showCanvas;
				this.ToggleCanvas();
			}
		}

		public void ToggleCanvas() {
			this.canvasGroup.alpha = this.showCanvas ? 1f : 0f;
			this.canvasGroup.interactable = this.showCanvas;
			this.canvasGroup.blocksRaycasts = this.showCanvas;
		}
	}
}
