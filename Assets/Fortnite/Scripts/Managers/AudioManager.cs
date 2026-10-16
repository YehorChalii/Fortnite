
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _SFXSource;

    [Header("Place")]
    [SerializeField] private AudioClip _woodBuildingPlaceClip;
    [SerializeField] private AudioClip _brickBuildingPlaceClip;
    [SerializeField] private AudioClip _metalBuildingPlaceClip;

    [Header("Destroy")]
    [SerializeField] private AudioClip _woodBuildingDestroyClip;
    [SerializeField] private AudioClip _brickBuildingDestroyClip;
    [SerializeField] private AudioClip _metalBuildingDestroyClip;

    [SerializeField] private Vector2 _randomPitchRange;
    [SerializeField] private Vector2 _randomVolumeRange;

    private void OnEnable()
    {
        BuildingManager.OnBuildingPlaced += HandleBuildingPlace;
        BuildingManager.OnBuildingDestroyed += HandleBuildingDestroy;
    }
    private void OnDisable()
    {
        BuildingManager.OnBuildingPlaced -= HandleBuildingPlace;
        BuildingManager.OnBuildingDestroyed -= HandleBuildingDestroy;
    }

    private void HandleBuildingPlace(BuildingMaterial material)
    {
        switch (material)
        {
            case BuildingMaterial.Wood:
                PlayClip(_woodBuildingPlaceClip);
                break;
            case BuildingMaterial.Brick:
                PlayClip(_brickBuildingPlaceClip);
                break;
            case BuildingMaterial.Metal:
                PlayClip(_metalBuildingPlaceClip);
                break;
        }
    }

    private void HandleBuildingDestroy(BuildingMaterial material)
    {
        switch (material)
        {
            case BuildingMaterial.Wood:
                PlayClip(_woodBuildingDestroyClip);
                break;
            case BuildingMaterial.Brick:
                PlayClip(_brickBuildingDestroyClip);
                break;
            case BuildingMaterial.Metal:
                PlayClip(_metalBuildingDestroyClip);
                break;
        }
    }

    private void PlayClip(AudioClip clip)
    {
        _SFXSource.pitch = Random.Range(_randomPitchRange.x, _randomPitchRange.y);
        _SFXSource.volume = Random.Range(_randomVolumeRange.x, _randomVolumeRange.y);

        _SFXSource.PlayOneShot(clip);
    }
}
