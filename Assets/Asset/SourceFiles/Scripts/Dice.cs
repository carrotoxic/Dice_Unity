using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dice: MonoBehaviour
{
    public bool IsRolling => _rolling;
    public int CurrentValue { get; private set; } = -1;
    
    [SerializeField] float _torqueMinimum = 0.1f;
    [SerializeField] float _torqueMaximum = 2;
    [SerializeField] float _throwStrength = 10;
    [SerializeField] float _upRatio = 0.25f;          // how much of force goes upward
    [SerializeField] TextMeshProUGUI _textBox;

    private static readonly Vector3 BOWL_CENTER = new Vector3(1.79f, 7.8f, 0f);
    [SerializeField] private Vector3 _spawnPos;
    [SerializeField] private Vector3 _spawnEuler;
    
    
    private Rigidbody _rb;
    private bool _rolling;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // Hard-freeze physics until Roll is pressed
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _spawnPos = transform.position;
        _spawnEuler = transform.eulerAngles;

        if (_textBox != null) _textBox.text = "";
    }


    public void Roll()
    {
        print("Rolling dice");
        if (_rolling) return;
        _rolling = true;

        transform.position = _spawnPos;
        transform.rotation = Quaternion.Euler(_spawnEuler);

        Vector3 toBowl = BOWL_CENTER - transform.position;

        // Unfreeze physics
        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _textBox.text = "";

        toBowl.y = 0f;
        toBowl = toBowl.sqrMagnitude < 1e-6f ? transform.forward : toBowl.normalized;

        Vector3 impulseDir = (toBowl * (1f - _upRatio) + Vector3.up * _upRatio).normalized;

        _rb.AddForce(impulseDir * _throwStrength, ForceMode.Impulse);

        // Random spin (unbiased)
        _rb.AddTorque(
            transform.forward * Random.Range(-_torqueMaximum, _torqueMaximum) +
            transform.up      * Random.Range(-_torqueMaximum, _torqueMaximum) +
            transform.right   * Random.Range(-_torqueMaximum, _torqueMaximum),
            ForceMode.Impulse
        );

        StartCoroutine(waitForStop());
    }

    IEnumerator waitForStop()
    {
        yield return new WaitForFixedUpdate();

        float stillTime = 0f;
        const float settleSeconds = 0.4f;

        while (true)
        {
            bool slow =
                _rb.angularVelocity.sqrMagnitude < 0.01f &&
                _rb.linearVelocity.sqrMagnitude < 0.0025f;

            stillTime = slow ? stillTime + Time.fixedDeltaTime : 0f;

            if (stillTime >= settleSeconds) break;
            yield return new WaitForFixedUpdate();
        }

        CheckRoll();

        // Freeze again after result (so it doesn't drift)
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        _rolling = false;
    }

    void CheckRoll()
    {
        // World up direction
        Vector3 up = Vector3.up;

        // Candidate face directions (world space)
        float dotUp      = Vector3.Dot(transform.up, up);
        float dotDown    = Vector3.Dot(-transform.up, up);
        float dotRight   = Vector3.Dot(transform.right, up);
        float dotLeft    = Vector3.Dot(-transform.right, up);
        float dotForward = Vector3.Dot(transform.forward, up);
        float dotBack    = Vector3.Dot(-transform.forward, up);

        // Find which local axis is most aligned with world up
        float maxDot = dotUp;
        int faceId = 0; // 0=+Y,1=-Y,2=+X,3=-X,4=+Z,5=-Z

        if (dotDown > maxDot) { maxDot = dotDown; faceId = 1; }
        if (dotRight > maxDot){ maxDot = dotRight; faceId = 2; }
        if (dotLeft > maxDot) { maxDot = dotLeft; faceId = 3; }
        if (dotForward > maxDot){ maxDot = dotForward; faceId = 4; }
        if (dotBack > maxDot) { maxDot = dotBack; faceId = 5; }

        // If it's too tilted, treat as invalid (optional)
        if (maxDot < 0.9f)
        {
            _textBox.text = "?";
            return;
        }

        // Map faceId -> dice value (YOU MUST set this to match your model)
        int rollValue = faceId switch
        {
            0 => 2, // +Y on top
            1 => 5, // -Y on top
            2 => 4, // +X on top
            3 => 3, // -X on top
            4 => 1, // +Z on top
            5 => 6, // -Z on top
            _ => -1
        };

        CurrentValue = rollValue;
        _textBox.text = rollValue.ToString();
    }
    
}
