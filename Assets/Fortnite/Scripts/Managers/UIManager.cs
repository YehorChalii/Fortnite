using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private Image _crosshair;
    [SerializeField] private Sprite _regularCrosshairSprite;
    [SerializeField] private Sprite _buildingCrosshairSprite;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _nullColor;

    [Space]
    [Header("Building Layout")]
    [SerializeField] private GameObject _buildingBindingsLayout;
    [SerializeField] private float _buildingLayoutLerpDuration = 0.2f;
    [SerializeField] private Vector3 _hiddenScale = new Vector3(0.5f, 0.5f, 0.5f);

    private RectTransform _buildingBindingsRect;
    private Coroutine _scaleCoroutine;


    private void Awake()
    {
        _buildingBindingsRect = _buildingBindingsLayout.GetComponent<RectTransform>();
        _buildingBindingsRect.localScale = _hiddenScale;
    }

    private void OnEnable()
    {
        BuildingManager.OnBuildingModeToggle += HandleBuildingModeToggle;
        BuildingManager.OnUpdateCrosshair += HandleUpdateCrosshair;
    }

    private void OnDisable()
    {
        BuildingManager.OnBuildingModeToggle -= HandleBuildingModeToggle;
        BuildingManager.OnUpdateCrosshair -= HandleUpdateCrosshair;
    }

    private void HandleBuildingModeToggle(bool buildingMode)
    {
        _crosshair.sprite = buildingMode ? _buildingCrosshairSprite : _regularCrosshairSprite;

        if (!buildingMode)
            _crosshair.color = _defaultColor;

        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);

        _scaleCoroutine = StartCoroutine(LerpScale(buildingMode ? Vector3.one : _hiddenScale));
    }

    private void HandleUpdateCrosshair(bool snapPointAvailable)
    {
        _crosshair.color = snapPointAvailable ? _defaultColor : _nullColor;
    }

    private IEnumerator LerpScale(Vector3 targetScale)
    {
        Vector3 startScale = _buildingBindingsRect.localScale;
        float timer = 0f;

        while (timer < _buildingLayoutLerpDuration)
        {
            timer += Time.deltaTime;
            _buildingBindingsRect.localScale = Vector3.Lerp(startScale, targetScale, timer / _buildingLayoutLerpDuration);
            yield return null;
        }

        _buildingBindingsRect.localScale = targetScale;
    }
}