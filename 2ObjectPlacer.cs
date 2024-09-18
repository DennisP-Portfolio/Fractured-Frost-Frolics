using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] private int _SpawnAmountPerFrame = 5;
    [SerializeField] public bool _FinishedSpawning;
    [SerializeField] private bool _ShowToLoadingBar;
    [SerializeField] private BarController _BarController;

    [Header("Objects")]
    [SerializeField] private GameObject[] _Objects;
    [SerializeField] private int _Amount = 50;
    private List<GameObject> _SpawnedObjects = new();

    [Header("Bounds")]
    [SerializeField] private BoxCollider[] _Bounds;
    private int _whichBounds;
    [SerializeField] private GameObject _Parent;
    private Vector3 _spawnPoint;

    [Header("RND Rotation")]
    [SerializeField] private bool _rotateOnX;
    [SerializeField] private bool _rotateOnY;
    [SerializeField] private bool _rotateOnZ;
    [SerializeField] private bool _resetZRot;

    [Header("RND offset")]
    [SerializeField] private bool _RandomizeXOffset;
    [SerializeField] private bool _RandomizeYOffset;
    [SerializeField] private bool _RandomizeZOffset;
    [SerializeField] private float _RandomizeOffsetIntensity = .5f;

    [Header("RND scale")]
    [SerializeField] private bool _RandomizeXScale;
    [SerializeField] private bool _RandomizeYScale;
    [SerializeField] private bool _RandomizeZScale;
    [SerializeField] private float _RandomizeScaleIntensity = .2f;

    [Header("Layer")]
    [SerializeField] private bool _SpawnOnSpecificLayer;
    [SerializeField] private LayerMask _Layer;

    private void Start()
    {
        if (_Parent == null) _Parent = gameObject;
        if (!_SpawnOnSpecificLayer) StartCoroutine(SpawnObjects());
        else if (_SpawnOnSpecificLayer) StartCoroutine(SpawnObjectsOnSpecificLayer());
        if (_ShowToLoadingBar) _BarController.SetMaxValue(_Amount); 
    }

    private IEnumerator SpawnObjectsOnSpecificLayer()
    {
        // Set origin point for raycast inside bounds of a box collider
        Vector3 rayOrigin = RandomPointInBounds(_Bounds[0].bounds);
        int spawnedAmount = 0;
        int spawnedInFrameCount = 0;

        while (spawnedAmount < _Amount)
        {
            Vector3 possibleSpawnPoint;

            // Raycast to find a valid spawn point on the specified layer
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100, _Layer))
            {
                possibleSpawnPoint = hit.point;
                int whichObject = Random.Range(0, _Objects.Length);

                // Check if the hit object's layer matches the specified layer
                if (((1 << hit.transform.gameObject.layer) & _Layer) != 0)
                {
                    spawnedAmount++;
                    // Instantiate the object and apply random rotation, offset, and scale
                    var obj = Instantiate(_Objects[whichObject], possibleSpawnPoint, Quaternion.Euler(0, Random.Range(0, 359), 0), transform);
                    _SpawnedObjects.Add(obj);

                    // Randomize rotation
                    Vector3 randomRot = new Vector3();
                    if (_rotateOnX) randomRot.x = Random.Range(0, 359);
                    if (_rotateOnY) randomRot.y = Random.Range(0, 359);
                    if (_rotateOnZ) randomRot.z = Random.Range(0, 359);
                    obj.transform.rotation = Quaternion.Euler(randomRot);

                    // Reset Z rotation if specified
                    if (_resetZRot) obj.transform.localEulerAngles = new Vector3(randomRot.x, randomRot.y, 0);

                    // Randomize offset position
                    Vector3 rndOffsetPos = new Vector3();
                    if (_RandomizeXOffset) rndOffsetPos.x = Random.Range(-_RandomizeOffsetIntensity, _RandomizeOffsetIntensity);
                    if (_RandomizeYOffset) rndOffsetPos.y = Random.Range(-_RandomizeOffsetIntensity, _RandomizeOffsetIntensity);
                    if (_RandomizeZOffset) rndOffsetPos.z = Random.Range(-_RandomizeOffsetIntensity, _RandomizeOffsetIntensity);
                    obj.transform.position = obj.transform.position + rndOffsetPos;

                    // Randomize scale
                    Vector3 rndScale = new Vector3();
                    if (_RandomizeXScale) rndScale.x = Random.Range(-_RandomizeScaleIntensity, _RandomizeScaleIntensity);
                    if (_RandomizeYScale) rndScale.y = Random.Range(-_RandomizeScaleIntensity, _RandomizeScaleIntensity);
                    if (_RandomizeZScale) rndScale.z = Random.Range(-_RandomizeScaleIntensity, _RandomizeScaleIntensity);
                    obj.transform.localScale = obj.transform.localScale + rndScale;

                    spawnedInFrameCount++;
                }
            }

            rayOrigin = RandomPointInBounds(_Bounds[0].bounds);

            if (spawnedInFrameCount > _SpawnAmountPerFrame)
            {
                spawnedInFrameCount = 0;
                // Update loading bar progress
                if (_ShowToLoadingBar) _BarController.SetFloatValue(spawnedAmount);
                yield return new WaitForEndOfFrame();
            }
        }
        // Finalize loading bar progress
        if (_ShowToLoadingBar) _BarController.SetFloatValue(spawnedAmount);
        if (_ShowToLoadingBar) Invoke("DisableLoadingScreen", 1f);
        _FinishedSpawning = true;
    }

    private void DisableLoadingScreen()
    {
        _BarController.gameObject.SetActive(false);
    }

    public IEnumerator RespawnObjects()
    {
        if (_ShowToLoadingBar) _BarController.SetFloatValue(0);
        if (_ShowToLoadingBar) _BarController.gameObject.SetActive(true);
        _FinishedSpawning = false;
        foreach(GameObject obj in _SpawnedObjects)
        {
            Destroy(obj.gameObject);
        }
        _SpawnedObjects.Clear();
        StartCoroutine(SpawnObjectsOnSpecificLayer());
        yield return null;
    }

    /// <summary>
    /// Creates a random Vector3 position within set bounds
    /// </summary>
    private static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
