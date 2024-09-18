
using UnityEngine;
using UnityEngine.InputSystem;

public class SledRotator : MonoBehaviour
{
    [SerializeField] private float _LowerCOMAmount;
    [SerializeField] private float _SteerForce = .7f;

    private Rigidbody _rb;

    private Vector3 _steerInput = Vector2.zero;
    private float _lerpedSteerInput;
    private float _steerTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Lower the center of mass of the Rigidbody
        _rb.centerOfMass = new Vector3(_rb.centerOfMass.x, _rb.centerOfMass.y - _LowerCOMAmount, _rb.centerOfMass.z);
    }

    private void Update()
    {
        // Update the time spent steering
        if (_steerInput.x != 0)
        {
            _steerTime += Time.deltaTime;
        }
        else
        {
            _steerTime = 0;
        }

        // Use Mathf.Lerp to smooth out the steering input and icrease the speed the longer the player is steering
        _lerpedSteerInput = Mathf.Lerp(0, _steerInput.x, 20 * (_steerTime * 10f + 1f) * Time.fixedDeltaTime);
    }

    private void FixedUpdate()
    {
        // Return if the Rigidbody is kinematic (not affected by physics)
        if (_rb.isKinematic) return;

        // Rotate the sled based on the steering input
        transform.Rotate(Vector3.up, _lerpedSteerInput * _SteerForce * Time.fixedDeltaTime);

        // Apply a rotation offset to the velocity for a more realistic steering effect
        Quaternion offset = Quaternion.Euler(0f, _lerpedSteerInput * _SteerForce * Time.fixedDeltaTime, 0f);
        _rb.velocity = offset * _rb.velocity;
    }

    // Called when the "Steer" action is triggered in the Unity Input System
    private void OnSteer(InputValue value)
    {
        _steerInput = value.Get<Vector2>();
    }
}
