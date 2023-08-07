using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Prompts {
	public class JoinCodePrompt : MonoBehaviour {


		// public delegate TextCompleted
		public delegate void TextCompleted(string text);

		public TextCompleted OnTextCompleted;

		[SerializeField]
		private TMP_InputField tmpInputField;

		[SerializeField]
		private Button yesButton;

		[SerializeField]
		private Button noButton;

		public static JoinCodePrompt Singleton { get; private set; }

		private void Awake() {
			if (Singleton is null) Singleton = this;
			else Destroy(gameObject);

			gameObject.SetActive(false);

			if (tmpInputField)
				tmpInputField.onEndEdit.AddListener((string text) => {
					if (text != "") {
						gameObject.SetActive(false);
						OnTextCompleted?.Invoke(text);
					}
					else { }
				});


			if (yesButton)
				yesButton.onClick.AddListener(() => {
					if (tmpInputField.text != "") {
						gameObject.SetActive(false);
						OnTextCompleted?.Invoke(tmpInputField.text);
					}
					else { }
				});

			if (noButton) noButton.onClick.AddListener(() => { gameObject.SetActive(false); });

		}

		public void GetInput() {
			Singleton.gameObject.SetActive(true);
		}
	}
}