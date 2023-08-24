using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ServerListUI : MonoBehaviour{

    #region Variables

    [SerializeField] private RectTransform selfRectTransform;

    // private Vector2 defaultRectAnchoredTransform;
    private Vector3 defaultLocalScale;

    #endregion
    
    
    #region Builtin Methods

    private void Awake() {
        // defaultRectAnchoredTransform = selfRectTransform.anchoredPosition;
        defaultLocalScale = transform.localScale;
    }

    private void OnEnable() {
        transform.DOScale(1, 0.4f).SetEase(Ease.InSine);
    }

    private void OnDisable() {
        // selfRectTransform.anchoredPosition = defaultRectAnchoredTransform;
        transform.localScale = defaultLocalScale;
    }

    #endregion

    #region NetworkEvnets

    // public override onNetwor

    #endregion
    
}