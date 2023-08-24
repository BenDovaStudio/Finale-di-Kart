using _Scripts.Controllers;
using DG.Tweening;
using UnityEngine;

public class RacePromptUI : MonoBehaviour {

    #region Variables

    [SerializeField] private RectTransform selfRectTransform;

    private Vector2 defaultRectAnchoredTransform;

    #endregion
    
    
    #region Builtin Methods

    private void Awake() {
        defaultRectAnchoredTransform = selfRectTransform.anchoredPosition;
    }

    private void OnEnable() {
        AudioController.Instance.PlayNotificationSfx();
        selfRectTransform.DOAnchorPosX(0, 0.8f).SetEase(Ease.OutSine);
    }

    private void OnDisable() {
        selfRectTransform.anchoredPosition = defaultRectAnchoredTransform;
    }

    #endregion

    #region NetworkEvnets

    // public override onNetwor

    #endregion
    
}