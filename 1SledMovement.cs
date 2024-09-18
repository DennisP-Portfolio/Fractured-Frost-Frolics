using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SledMovement : MonoBehaviour
{
    [SerializeField] private float _Force; 
    [SerializeField] private TextMeshProUGUI _ReadyText;
    [SerializeField] private MetersTravelledCounter _MetersTravelledCounter;

    private Rigidbody _rb;
    private bool _touchingGround;
    private float _airTimer;
    private float _initialForce;

    private bool _isReady;
    private bool _canSlide;

    private Vector3 _spawnPos;

    private SledCollision _sledCol;
    private PauseManager _pauseManager;


    private void Awake()
    {
        _isReady = false;
        _rb = GetComponent<Rigidbody>();
        _initialForce = _Force;
        _sledCol = GetComponent<SledCollision>();
        _pauseManager = FindObjectOfType<PauseManager>();
        _spawnPos = transform.position;
    }

    private void Update()
    {
        if (!_canSlide) return;

        // Adjust force based on sled's tilt angle
        if (transform.localEulerAngles.x > 0 && transform.localEulerAngles.x < 30)
        {
            _Force += transform.localEulerAngles.x / 2 * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!_canSlide) return;

        // Apply force to sled when touching the ground and not tilted too much
        if (_touchingGround && transform.eulerAngles.x >= -1f)
        {
            _rb.AddForce(transform.forward * _Force * Time.fixedDeltaTime);
            _airTimer = 0f;
        }

        // Adjust rotation in the air to simulate sled movement
        if (!_touchingGround)
        {
            _airTimer += Time.fixedDeltaTime;
            Quaternion baseRotation = Quaternion.Euler(15f, transform.eulerAngles.y, 0f);

            // Rotate the sled towards the base rotation
            if (!_sledCol._died) transform.rotation = Quaternion.RotateTowards(transform.rotation, baseRotation, 10f * (_airTimer * 2f + 1f) * Time.fixedDeltaTime);
        }

        _touchingGround = false;
        _rb.drag = 0.2f;
    }

    private void OnPush(InputValue value)
    {
        // Toggle readiness on button press if not sliding, not paused, and not in restart screen
        if (value.isPressed && !_canSlide && !_pauseManager._paused && !_pauseManager._restartScreen)
            _isReady = !_isReady;

        // Update UI based on sled readiness
        if (_isReady)
        {
            _ReadyText.text = "Ready";
            _ReadyText.color = Color.green;
        }
        else
        {
            _ReadyText.text = "Not Ready";
            _ReadyText.color = Color.red;
        }
    }

    public void PushSled()
    {
        // Enable sliding, update spawn position, and enable meters counter
        _canSlide = true;
        _spawnPos = transform.position;
        _spawnPos.y += .1f;
        _ReadyText.enabled = false;
        _MetersTravelledCounter.EnableMeterCounter(true);
    }

    public void ResetSpawnPos()
    {
        _spawnPos = transform.position;
    }

    public void Respawn()
    {
        // Disable sliding, reset readiness, and reset other sled properties on respawn
        _canSlide = false;
        _isReady = false;
        _ReadyText.text = "Not Ready";
        _ReadyText.color = Color.red;
        _ReadyText.enabled = true;
        _rb.isKinematic = false;
        _rb.velocity = Vector3.zero;
        _rb.useGravity = true;
        transform.localEulerAngles = new Vector3(0, 180, 0);
        transform.position = _spawnPos;
        _Force = _initialForce;
        GetComponent<SledCollision>().Respawn();
        _MetersTravelledCounter.EnableMeterCounter(false);
    }

    private void OnCollisionStay(Collision collision)
    {
        // Check if the sled is touching the ground based on collision normal
        if (collision.GetContact(0).normal.y > 0.8f)
        {
            _touchingGround = true;
            _rb.drag = 1f;
        }
    }

    public bool CheckIfReady()
    {
        return _isReady;
    }
}
