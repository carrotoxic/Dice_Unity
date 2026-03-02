using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class StartUIController : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject start;
    [SerializeField] private GameObject pressStart;

    [Header("Camera")]
    [SerializeField] private Transform cam;
    [SerializeField] private float moveDuration = 1.2f;

    [Header("Spawn Bottom")]
    [SerializeField] private GameObject bottomPrefab;
    [SerializeField] private Vector3 bottomSpawnPos = new Vector3(0.233f, 1.911f, 3.053f);
    [SerializeField] private Vector3 bottomSpawnEuler = Vector3.zero;
    [SerializeField] private bool spawnOnce = true;

    [Header("Spawn Dice")]
    [SerializeField] private GameObject dicePrefab; // ✅ 拖你的骰子 prefab
    [SerializeField] private int diceCount = 5;
    [SerializeField] private float diceSpawnDelay = 1.0f;
    [SerializeField] private float diceSpawnHeight = 0.35f;   // 在碗上方生成（避免卡在碗里）
    [SerializeField] private float diceSpawnRadius = 0.08f;   // x/z 轻微散开
    [SerializeField] private float diceRandomYaw = 180f;      // 随机旋转幅度

    private readonly Vector3 targetPos = new Vector3(0.01f, 1.88f, 1.23f);
    private readonly Vector3 targetEuler = new Vector3(30.061f, 41.397f, 1.179f);

    private Coroutine running;
    private bool waitingForSpace = false;
    private bool bottomSpawned = false;

    private void Awake()
    {
        if (pressStart != null) pressStart.SetActive(false);
    }

    private void Update()
    {
        if (!waitingForSpace) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnSpacePressed();
        }
    }

    private void OnSpacePressed()
    {
        if (pressStart != null) pressStart.SetActive(false);
        waitingForSpace = false;

        SpawnBottom();

        Debug.Log("[StartUI] Space pressed -> Press Start hidden + Bottom spawned.");
    }

    public void OnStartButtonClicked()
    {
        if (running != null) return;

        if (title != null) title.SetActive(false);
        if (start != null) start.SetActive(false);

        waitingForSpace = false;
        if (pressStart != null) pressStart.SetActive(false);

        running = StartCoroutine(MoveCameraThenShowPressStart());
    }

    private IEnumerator MoveCameraThenShowPressStart()
    {
        if (cam == null)
        {
            Debug.LogError("[StartUI] Camera Transform is not assigned!");
            yield break;
        }

        Vector3 startPos = cam.position;
        Quaternion startRot = cam.rotation;
        Quaternion targetRot = Quaternion.Euler(targetEuler);

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / moveDuration);
            float s = u * u * (3f - 2f * u);

            cam.position = Vector3.Lerp(startPos, targetPos, s);
            cam.rotation = Quaternion.Slerp(startRot, targetRot, s);
            yield return null;
        }

        cam.SetPositionAndRotation(targetPos, targetRot);

        if (pressStart != null) pressStart.SetActive(true);
        waitingForSpace = true;

        running = null;
    }

    private void SpawnBottom()
    {
        if (bottomPrefab == null)
        {
            Debug.LogError("[StartUI] bottomPrefab is not assigned!");
            return;
        }

        if (spawnOnce && bottomSpawned) return;

        Quaternion rot = Quaternion.Euler(bottomSpawnEuler);
        GameObject obj = Instantiate(bottomPrefab, bottomSpawnPos, rot);

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) rb = obj.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        bottomSpawned = true;
        StartCoroutine(FreezeWhenSettled(rb));

        // ✅ 生成 bottom 后，延迟 1 秒再生成骰子
        StartCoroutine(SpawnDiceAfterDelay(bottomSpawnPos));
    }

    private IEnumerator SpawnDiceAfterDelay(Vector3 spawnCenter)
    {
        yield return new WaitForSeconds(diceSpawnDelay);

        if (dicePrefab == null)
        {
            Debug.LogError("[StartUI] dicePrefab is not assigned!");
            yield break;
        }

        for (int i = 0; i < diceCount; i++)
        {
            // 在同一位置附近轻微散开，并在上方一点生成，确保会落入碗里
            Vector2 offset2D = Random.insideUnitCircle * diceSpawnRadius;
            Vector3 pos = new Vector3(
                spawnCenter.x + offset2D.x,
                spawnCenter.y + diceSpawnHeight + Random.Range(0f, 0.12f),
                spawnCenter.z + offset2D.y
            );

            Quaternion rot = Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(-diceRandomYaw, diceRandomYaw),
                Random.Range(0f, 360f)
            );

            GameObject d = Instantiate(dicePrefab, pos, rot);

            // 确保骰子有 Collider 和 Rigidbody（才能掉落并碰撞）
            if (d.GetComponent<Collider>() == null)
                d.AddComponent<BoxCollider>();

            Rigidbody rb = d.GetComponent<Rigidbody>();
            if (rb == null) rb = d.AddComponent<Rigidbody>();

            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        Debug.Log("[StartUI] Dice spawned.");
    }

    [SerializeField] private float settleSpeed = 0.05f;     // 认为“静止”的速度阈值
    [SerializeField] private float settleTime = 0.4f;      // 连续静止多久算落稳

    private IEnumerator FreezeWhenSettled(Rigidbody rb)
    {
        float still = 0f;
        while (still < settleTime)
        {
            // 同时看线速度和角速度，避免它还在慢慢滚
            if (rb.linearVelocity.magnitude < settleSpeed && rb.angularVelocity.magnitude < settleSpeed)
                still += Time.deltaTime;
            else
                still = 0f;

            yield return null;
        }

        rb.isKinematic = true; // ✅ 锁死不再动（仍然能被骰子“碰到”，但自己不动）
                               // 或者：rb.constraints = RigidbodyConstraints.FreezeAll;

        Debug.Log("[StartUI] Bottom settled -> frozen.");
    }
}