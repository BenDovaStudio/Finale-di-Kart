using UnityEngine;

namespace _Scripts.Controllers {
    public class AudioController : MonoBehaviour
    {
        #region Variables

        public static AudioController Instance;

        [SerializeField] private AudioClip buttonClickSfx;
        [SerializeField] private AudioClip notificationSfx;


        [SerializeField] private AudioClip toggleOnSfx;
        [SerializeField] private AudioClip toggleOffSfx;
    


        [SerializeField] private AudioSource sourceUISfx;
        [SerializeField] private AudioSource sourceSfx;

        #endregion


        #region Builtin Methods

        private void Awake() {
            if (Instance is null) Instance = this;
            else Destroy(gameObject);
        }

        #endregion


        #region Custom Methods

        public void PlayNotificationSfx() {
            if (sourceUISfx) sourceUISfx.PlayOneShot(notificationSfx);
        }

        public void PlayButtonClickSfx() {
            if (sourceUISfx) sourceUISfx.PlayOneShot(buttonClickSfx);
        }

        public void PlayToggleSfx(bool on) {
            if (sourceUISfx) sourceUISfx.PlayOneShot(on ? toggleOnSfx : toggleOffSfx);
        }

        #endregion
    }
}
