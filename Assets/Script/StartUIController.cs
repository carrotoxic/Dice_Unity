using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartUIController : MonoBehaviour
{
    // =========================
    // Full Energy Burst FX
    // =========================
    [Header("Full Energy Burst FX")]
    [SerializeField] private GameObject fullEnergyBurstPrefab;
    [SerializeField] private int fullEnergyBurstCount = 150;
    [SerializeField] private float fullEnergyBurstDestroyExtra = 0.5f;

    // =========================
    // Break Apart + Launch
    // =========================
    [Header("Break Apart + Launch (when burst)")]
    [SerializeField] private float breakupExplosionForce = 8.0f;
    [SerializeField] private float breakupExplosionRadius = 2.0f;
    [SerializeField] private float breakupUpwardModifier = 1.0f;
    [SerializeField] private float breakupForwardImpulse = 2.5f;
    [SerializeField] private float breakupRandomTorque = 12f;
    [SerializeField] private bool destroyGroupAfterBreak = true;

    // =========================
    // Post Launch Dice (spawn -> fly through camera)
    // =========================
    [Header("Post Launch Dice (Spawn at Camera+Forward*30 -> Fly Through Camera)")]
    [SerializeField] private bool spawnDiceAfterLaunch = true;
    [SerializeField] private float postDiceSpawnForwardDistance = 30f;     // camera 前 30
    [SerializeField] private float postDiceApproachDistance = 0.6f;        // 飞到镜头前多近（越小越贴脸）
    [SerializeField] private float postDiceOvershootBehindCameraDistance = 6f; // 穿过镜头后飞到镜头后多远
    [SerializeField] private float postDiceFlyDuration = 0.28f;            // 总时长（越小越快）
    [SerializeField] private float postDiceRollMinDegPerSec = 600f;
    [SerializeField] private float postDiceRollMaxDegPerSec = 1400f;
    [SerializeField] private float postDiceDestroyAfterArrive = 0f;        // 0=不销毁；>0 到达后几秒销毁

    // ✅ 更随机、更分散的生成参数
    [Header("Post Dice Spawn Randomness (Spread + Separation)")]
    [SerializeField] private float postSpawnRadius = 1.4f;          // 生成圆半径（越大越分散）
    [SerializeField] private float postMinSeparation = 0.55f;       // 最小间距（越大越不挤）
    [SerializeField] private float postDepthJitter = 0.8f;          // 沿 forward 前后抖动（更3D）
    [SerializeField] private int postSampleAttemptsPerDie = 24;     // 拒绝采样次数

    // =========================
    // Post Dice Fill Light (Point Light follow dice center)
    // =========================
    [Header("Post Dice Fill Light (Follow Dice)")]
    [SerializeField] private bool enablePostDiceFillLight = true;
    [SerializeField] private Color postDiceLightColor = new Color(1f, 0.95f, 0.85f, 1f);
    [SerializeField] private float postDiceLightRange = 6f;
    [SerializeField] private float postDiceLightIntensity = 0.7f;
    [SerializeField] private float postDiceLightForwardToCamera = 0.8f; // 从骰子中心往相机方向推
    [SerializeField] private float postDiceLightUpOffset = 0.2f;        // 稍微抬高一点
    [SerializeField] private bool postDiceLightShadows = false;

    private Light postDiceFillLight;

    // =========================
    // Slow-Mo Trigger + SlowEnergy
    // =========================
    [Header("Slow-Mo Trigger (Distance <= 20)")]
    [SerializeField] private float slowMoTriggerDistance = 20f;

    [Tooltip("慢镜头时骰子飞行速度倍率（越小越慢）")]
    [SerializeField] private float slowMoSpeedMultiplier = 0.25f;

    [Tooltip("是否使用全局 timeScale 慢镜头（开启后整个世界都会慢）")]
    [SerializeField] private bool useGlobalSlowMo = false;

    [Tooltip("全局慢镜头 timeScale（仅 useGlobalSlowMo=true 时生效）")]
    [SerializeField] private float globalSlowMoTimeScale = 0.2f;

    [Tooltip("slowMo 触发时 fixedDeltaTime 跟随缩放（仅全局慢镜头）")]
    [SerializeField] private bool scaleFixedDeltaTimeWithTimeScale = true;

    [Header("Slow Energy (100 -> 0)")]
    [SerializeField] private float slowEnergyMax = 100f;
    [SerializeField] private float slowEnergyDecayPerSecond = 25f;

    // =========================
    // Slow Energy Bar UI (Fade + Filled)
    // =========================
    [Header("Slow Energy Bar UI (Fade + Filled)")]
    [SerializeField] private GameObject slowEnergyBarRoot;   // CUD/SlowEnergyBar
    [SerializeField] private Image slowEnergyFillImage;      // CUD/SlowEnergyBar/bar(Image Filled)
    [SerializeField] private float slowEnergyFadeInDuration = 0.35f;
    [SerializeField] private float slowEnergyFadeOutDuration = 0.35f;

    // =========================
    // Energy Bar Color
    // =========================
    [Header("Energy Bar Color")]
    [SerializeField] private Color energyLowColor = new Color(0.2f, 1f, 0.2f, 1f);
    [SerializeField] private Color energyHighColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private bool keepAlphaDrivenByFade = true;

    // =========================
    // UI Objects
    // =========================
    [Header("UI Objects")]
    [SerializeField] private GameObject title;
    [SerializeField] private GameObject start;
    [SerializeField] private GameObject pressStart;

    // =========================
    // Energy Bar UI
    // =========================
    [Header("Energy Bar UI (Fade whole group + Filled bar)")]
    [SerializeField] private GameObject energyBarRoot;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private float energyFadeInDuration = 0.5f;
    [SerializeField] private float energyDecayPerSecond = 25f;
    [SerializeField] private float energyAddPerClick = 10f;

    [Header("Energy Auto Fade Out")]
    [SerializeField] private float energyFadeOutDelay = 1.0f;
    [SerializeField] private float energyFadeOutDuration = 0.5f;

    // =========================
    // Energy Drives Group + Camera
    // =========================
    [Header("Energy Drives Group + Camera")]
    [SerializeField] private float groupSwayAmplitudeX = 0.18f;
    [SerializeField] private float groupSwayMinHz = 0.15f;
    [SerializeField] private float groupSwayMaxHz = 2.0f;
    [SerializeField] private float groupSwayPositionLerp = 12f;
    [SerializeField] private float cameraDriveLerp = 8f;
    [SerializeField] private bool driveCameraRotationToo = true;

    // =========================
    // Camera Full Energy Snap + Shake
    // =========================
    [Header("Camera Full Energy Snap + Shake")]
    [SerializeField] private float fullEnergyThreshold = 100f;
    [SerializeField] private float fullEnergyDelay = 1.0f;

    [SerializeField] private float camSnapBackDuration = 0.12f;
    [SerializeField] private float camSnapBackSmoothness = 1.6f;

    [SerializeField] private float camShakeDuration = 0.25f;
    [SerializeField] private float camShakePosAmp = 0.08f;
    [SerializeField] private float camShakeRotAmpDeg = 3.5f;
    [SerializeField] private float camShakeFrequency = 28f;

    // =========================
    // Camera
    // =========================
    [Header("Camera")]
    [SerializeField] private Transform cam;
    [SerializeField] private float moveDuration = 1.2f;

    [Header("Camera After Group (Second move)")]
    [SerializeField] private Vector3 cameraAfterGroupPos = new Vector3(-0.513f, 2.338f, 0.637f);
    [SerializeField] private float cameraAfterGroupMoveDuration = 1.0f;

    // =========================
    // Spawn Bottom / Dice / Cup
    // =========================
    [Header("Spawn Bottom")]
    [SerializeField] private GameObject bottomPrefab;
    [SerializeField] private Vector3 bottomSpawnPos = new Vector3(0.233f, 1.911f, 3.053f);
    [SerializeField] private Vector3 bottomSpawnEuler = Vector3.zero;
    [SerializeField] private bool spawnOnce = true;

    [Header("Settle / Freeze Bottom")]
    [SerializeField] private float settleSpeed = 0.05f;
    [SerializeField] private float settleTime = 0.4f;

    [Header("Spawn Dice")]
    [SerializeField] private GameObject dicePrefab;
    [SerializeField] private int diceCount = 5;
    [SerializeField] private float diceSpawnHeight = 0.35f;

    [Header("Cup (spawn offscreen -> move in)")]
    [SerializeField] private GameObject cupPrefab;
    [SerializeField] private Vector3 cupOffscreenPos = new Vector3(0.268f, 1.554f, 4.28f);
    [SerializeField] private Vector3 cupOffscreenEuler = Vector3.zero;

    [SerializeField] private Vector3 cupTargetPos = new Vector3(0.92f, 1.35f, 2.38f);
    [SerializeField] private Vector3 cupTargetEuler = new Vector3(-38.36f, -28.83f, 19.13f);
    [SerializeField] private float cupMoveDuration = 1.0f;

    [Header("Dice Timing")]
    [SerializeField] private float firstDiceDelayAfterCupArrives = 0.0f;
    [SerializeField] private float diceIntervalSeconds = 1.0f;

    [Header("Bottom After Sequence Move (Smooth + Rotate)")]
    [SerializeField] private Vector3 bottomMidTargetPos = new Vector3(0.77f, 1.357f, 1.737f);
    [SerializeField] private Vector3 bottomFinalTargetPos = new Vector3(0.831f, 1.504f, 2.232f);
    [SerializeField] private Vector3 bottomFinalTargetEuler = new Vector3(139.09f, 12.968f, 8.91f);
    [SerializeField] private float bottomMidDuration = 0.9f;
    [SerializeField] private float bottomFinalDuration = 0.9f;

    [Header("Group Cup + Bottom After All Done")]
    [SerializeField] private string groupName = "CupBottom_Group";
    [SerializeField] private bool groupAtBottomPosition = true;

    [Header("Group Smooth Rotation + Move")]
    [SerializeField] private Vector3 groupTargetEuler = new Vector3(-137.032f, 0f, 0f);
    [SerializeField] private float groupRotateDuration = 1.0f;
    [SerializeField] private float groupTargetY = 0.95f;
    [SerializeField] private float groupMoveYDuration = 1.0f;

    private readonly Vector3 targetPos = new Vector3(0.01f, 1.88f, 1.23f);
    private readonly Vector3 targetEuler = new Vector3(30.061f, 41.397f, 1.179f);

    // =========================
    // Runtime State
    // =========================
    private Coroutine running;
    private bool waitingForSpace = false;
    private bool bottomSpawned = false;

    private Transform bottomTr;
    private Rigidbody bottomRb;
    private Transform cupTr;

    private readonly List<Rigidbody> spawnedDice = new List<Rigidbody>();

    // Post Dice runtime
    private class PostDie
    {
        public Transform tr;
        public int value;
        public float rollSpeed;
        public float rollAngle;
        public Vector3 startPos;
        public Vector2 planeOffset;
        public float depthOffset;
    }
    private readonly List<PostDie> postDice = new List<PostDie>();
    private Coroutine postDiceCo;

    // SlowEnergy runtime
    private float slowEnergy = 0f;
    private bool slowMoTriggered = false;
    private bool slowMoArmed = false;
    private float defaultFixedDeltaTime;

    // SlowEnergyBar fade runtime
    private readonly List<Graphic> slowEnergyGraphics = new List<Graphic>();
    private Coroutine slowEnergyFadeCo;
    private float slowEnergyAlpha = 0f;
    private bool slowEnergyBarVisible = false;

    // Energy runtime
    private bool energyLogicEnabled = false;
    private float energy = 0f;

    private float zeroIdleTimer = 0f;
    private bool energyBarVisible = false;
    private bool isFadingOut = false;
    private float currentEnergyAlpha = 0f;
    private Coroutine energyFadeCo;
    private readonly List<Graphic> energyGraphics = new List<Graphic>();

    // Group + Camera drive
    private Transform cupBottomGroupTr;
    private Vector3 groupBasePos;
    private float groupSwayPhase;

    private Vector3 cameraBasePos;
    private Quaternion cameraBaseRot;

    // Full energy
    private bool energyLocked = false;
    private bool fullEnergySequenceStarted = false;
    private bool cameraDriveEnabled = true;

    // =========================
    // Unity lifecycle
    // =========================
    private void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (pressStart != null) pressStart.SetActive(false);

        CacheEnergyGraphics();
        HideEnergyBarImmediate();
        SetEnergyUI(0f);

        CacheSlowEnergyGraphics();
        HideSlowEnergyBarImmediate();
        SetSlowEnergyUI(0f);

        DestroyPostDiceFillLight();
    }

    private void OnDisable()
    {
        EndSlowMo(forceRestoreTimeScale: true);
        DestroyPostDiceFillLight();
    }

    private void Update()
    {
        if (waitingForSpace)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                OnSpacePressed();
            return;
        }

        if (!energyLogicEnabled) return;

        if (!energyLocked)
        {
            bool clickedThisFrame = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (clickedThisFrame)
            {
                if (!energyBarVisible || isFadingOut || currentEnergyAlpha < 0.999f)
                    FadeInEnergyBarWhole();

                zeroIdleTimer = 0f;
                energy = Mathf.Clamp(energy + energyAddPerClick, 0f, 100f);
            }
            else
            {
                energy = Mathf.Max(0f, energy - energyDecayPerSecond * Time.deltaTime);

                if (Mathf.Approximately(energy, 0f))
                {
                    zeroIdleTimer += Time.deltaTime;
                    if (energyBarVisible && !isFadingOut && zeroIdleTimer >= energyFadeOutDelay)
                        FadeOutEnergyBarWhole();
                }
                else
                {
                    zeroIdleTimer = 0f;
                }
            }

            if (!fullEnergySequenceStarted && energy >= fullEnergyThreshold)
            {
                energy = fullEnergyThreshold;
                energyLocked = true;
                fullEnergySequenceStarted = true;

                FadeOutEnergyBarWhole();
                StartCoroutine(FullEnergySequence());
            }
        }

        SetEnergyUI(energy);
    }

    private void LateUpdate()
    {
        UpdatePostDiceFaceToCamera();
        UpdatePostDiceFillLight();     // ✅ light 跟随后置骰子中心
        UpdateSlowEnergyTickAndUI();

        if (!energyLogicEnabled) return;
        if (!cameraDriveEnabled) return;

        float norm = Mathf.Clamp01(energy / 100f);

        if (cupBottomGroupTr != null)
        {
            float hz = Mathf.Lerp(groupSwayMinHz, groupSwayMaxHz, norm);
            groupSwayPhase += Time.deltaTime * hz * Mathf.PI * 2f;

            float offsetX = Mathf.Sin(groupSwayPhase) * groupSwayAmplitudeX;
            Vector3 desired = groupBasePos + new Vector3(offsetX, 0f, 0f);

            cupBottomGroupTr.position = Vector3.Lerp(
                cupBottomGroupTr.position,
                desired,
                1f - Mathf.Exp(-groupSwayPositionLerp * Time.deltaTime)
            );
        }

        if (cam != null)
        {
            Vector3 desiredPos = Vector3.Lerp(cameraBasePos, targetPos, norm);
            cam.position = Vector3.Lerp(cam.position, desiredPos, 1f - Mathf.Exp(-cameraDriveLerp * Time.deltaTime));

            if (driveCameraRotationToo)
            {
                Quaternion targetRot = Quaternion.Euler(targetEuler);
                Quaternion desiredRot = Quaternion.Slerp(cameraBaseRot, targetRot, norm);
                cam.rotation = Quaternion.Slerp(cam.rotation, desiredRot, 1f - Mathf.Exp(-cameraDriveLerp * Time.deltaTime));
            }
        }
    }

    // =========================
    // Full Energy Sequence
    // =========================
    private IEnumerator FullEnergySequence()
    {
        if (fullEnergyDelay > 0f)
            yield return new WaitForSeconds(fullEnergyDelay);

        yield return StartCoroutine(CameraSnapBackShakeBurstAndBreak_KeepCameraStill());
    }

    private IEnumerator CameraSnapBackShakeBurstAndBreak_KeepCameraStill()
    {
        if (cam == null) yield break;

        Vector3 basePos = cameraBasePos != Vector3.zero ? cameraBasePos : cameraAfterGroupPos;
        Quaternion baseRot = cameraBaseRot != Quaternion.identity ? cameraBaseRot : cam.rotation;

        cameraDriveEnabled = false;

        SpawnFullEnergyBurstFX();
        BreakApartGroupAndLaunch();

        // 快速平滑回位（无回弹）
        Vector3 fromPos = cam.position;
        Quaternion fromRot = cam.rotation;

        float dur = Mathf.Max(0.0001f, camSnapBackDuration);
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);

            float s = u * u * (3f - 2f * u);
            if (camSnapBackSmoothness != 1f)
                s = Mathf.Pow(s, 1f / Mathf.Max(0.0001f, camSnapBackSmoothness));

            cam.position = Vector3.Lerp(fromPos, basePos, s);
            cam.rotation = Quaternion.Slerp(fromRot, baseRot, s);
            yield return null;
        }

        cam.position = basePos;
        cam.rotation = baseRot;

        // Shake
        float shakeT = 0f;
        Vector3 shakeBasePos = cam.position;
        Quaternion shakeBaseRot = cam.rotation;

        while (shakeT < camShakeDuration)
        {
            shakeT += Time.deltaTime;
            float u = Mathf.Clamp01(shakeT / Mathf.Max(0.0001f, camShakeDuration));
            float strength = 1f - u;

            float tt = Time.time * camShakeFrequency;

            float px = (Mathf.PerlinNoise(tt, 0.17f) - 0.5f) * 2f;
            float py = (Mathf.PerlinNoise(0.33f, tt) - 0.5f) * 2f;
            float pz = (Mathf.PerlinNoise(tt * 0.7f, 0.77f) - 0.5f) * 2f;

            Vector3 posOffset = new Vector3(px, py, pz) * camShakePosAmp * strength;

            float rx = (Mathf.PerlinNoise(tt, 1.11f) - 0.5f) * 2f;
            float ry = (Mathf.PerlinNoise(2.22f, tt) - 0.5f) * 2f;
            float rz = (Mathf.PerlinNoise(tt * 0.9f, 3.33f) - 0.5f) * 2f;

            Vector3 rotOffset = new Vector3(rx, ry, rz) * camShakeRotAmpDeg * strength;

            cam.position = shakeBasePos + posOffset;
            cam.rotation = shakeBaseRot * Quaternion.Euler(rotOffset);

            yield return null;
        }

        cam.position = shakeBasePos;
        cam.rotation = shakeBaseRot;
    }

    // =========================
    // Burst FX + Break Apart
    // =========================
    private void SpawnFullEnergyBurstFX()
    {
        if (fullEnergyBurstPrefab == null) return;

        Vector3 pos = (cupBottomGroupTr != null) ? cupBottomGroupTr.position : groupBasePos;
        GameObject fx = Instantiate(fullEnergyBurstPrefab, pos, Quaternion.identity);

        ParticleSystem ps = fx.GetComponentInChildren<ParticleSystem>();
        if (ps == null)
        {
            Destroy(fx, 2f);
            return;
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play(true);
        ps.Emit(fullEnergyBurstCount);

        var main = ps.main;
        float maxLife = main.startLifetime.constantMax;
        float destroyTime = main.duration + maxLife + fullEnergyBurstDestroyExtra;
        Destroy(fx, Mathf.Max(0.5f, destroyTime));
    }

    private void ClearSpawnedDice()
    {
        for (int i = spawnedDice.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = spawnedDice[i];
            if (rb == null) continue;
            if (rb.gameObject != null) Destroy(rb.gameObject);
        }
        spawnedDice.Clear();
    }

    private void BreakApartGroupAndLaunch()
    {
        ClearSpawnedDice();

        if (cupBottomGroupTr == null) return;

        Vector3 origin = cupBottomGroupTr.position;

        if (cupTr != null) cupTr.SetParent(null, true);
        if (bottomTr != null) bottomTr.SetParent(null, true);

        if (destroyGroupAfterBreak)
            Destroy(cupBottomGroupTr.gameObject);

        LaunchOne(cupTr, origin);
        LaunchOne(bottomTr, origin);

        SpawnAndFlyPostDice();
    }

    private void LaunchOne(Transform tr, Vector3 origin)
    {
        if (tr == null) return;

        Rigidbody rb = tr.GetComponent<Rigidbody>();
        if (rb == null) rb = tr.gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.AddExplosionForce(breakupExplosionForce, origin, breakupExplosionRadius, breakupUpwardModifier, ForceMode.Impulse);

        if (cam != null)
            rb.AddForce(cam.forward * breakupForwardImpulse, ForceMode.Impulse);

        rb.AddTorque(Random.onUnitSphere * breakupRandomTorque, ForceMode.Impulse);
    }

    // =========================
    // Random offsets with separation
    // =========================
    private List<Vector2> SampleOffsetsWithSeparation(int count, float radius, float minSep, int attemptsPerDie)
    {
        List<Vector2> pts = new List<Vector2>(count);

        float minSepSqr = minSep * minSep;
        int hardAttempts = Mathf.Max(4, attemptsPerDie);

        for (int i = 0; i < count; i++)
        {
            Vector2 best = Vector2.zero;
            float bestScore = -1f;
            bool found = false;

            for (int a = 0; a < hardAttempts; a++)
            {
                Vector2 cand = Random.insideUnitCircle * radius;

                bool ok = true;
                float nearestSqr = float.MaxValue;

                for (int j = 0; j < pts.Count; j++)
                {
                    float ds = (cand - pts[j]).sqrMagnitude;
                    nearestSqr = Mathf.Min(nearestSqr, ds);
                    if (ds < minSepSqr) { ok = false; break; }
                }

                if (ok)
                {
                    best = cand;
                    found = true;
                    break;
                }

                if (nearestSqr > bestScore)
                {
                    bestScore = nearestSqr;
                    best = cand;
                }
            }

            pts.Add(best);
        }

        return pts;
    }

    // =========================
    // Post Dice spawn (random & separated) + light spawn
    // =========================
    private void SpawnAndFlyPostDice()
    {
        if (!spawnDiceAfterLaunch) return;
        if (dicePrefab == null) return;
        if (cam == null) return;

        ClearPostDiceImmediate();

        slowMoArmed = true;
        slowMoTriggered = false;
        slowEnergy = 0f;
        HideSlowEnergyBarImmediate();

        Vector3 baseCenter = cam.position + cam.forward * postDiceSpawnForwardDistance;

        List<Vector2> offsets = SampleOffsetsWithSeparation(
            5,
            postSpawnRadius,
            postMinSeparation,
            postSampleAttemptsPerDie
        );

        for (int i = 0; i < 5; i++)
        {
            Vector2 off = offsets[i];
            float depthOff = Random.Range(-postDepthJitter, postDepthJitter);

            Vector3 spawnPos =
                baseCenter +
                cam.right * off.x +
                cam.up * off.y +
                cam.forward * depthOff;

            GameObject go = Instantiate(dicePrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            PostDie d = new PostDie
            {
                tr = go.transform,
                value = Random.Range(1, 7),
                rollSpeed = Random.Range(postDiceRollMinDegPerSec, postDiceRollMaxDegPerSec),
                rollAngle = Random.Range(0f, 360f),
                startPos = spawnPos,
                planeOffset = off,
                depthOffset = depthOff
            };

            postDice.Add(d);
        }

        // ✅ 生成弱光并立即定位一次
        EnsurePostDiceFillLight();
        UpdatePostDiceFillLight();

        postDiceCo = StartCoroutine(Co_FlyPostDiceThroughCamera_Dynamic_SlowEnergy());
    }

    private IEnumerator Co_FlyPostDiceThroughCamera_Dynamic_SlowEnergy()
    {
        float dur = Mathf.Max(0.0001f, postDiceFlyDuration);
        float half = dur * 0.5f;

        float p1 = 0f;
        float p2 = 0f;

        List<Vector3> seg2Start = new List<Vector3>(postDice.Count);

        // 第一段：到镜头前
        while (p1 < 1f)
        {
            TryTriggerSlowMoByDistance();

            float speedMul = slowMoTriggered ? Mathf.Clamp01(slowMoSpeedMultiplier) : 1f;
            float dt = Time.deltaTime * speedMul;

            p1 += dt / Mathf.Max(0.0001f, half);
            float u = Mathf.Clamp01(p1);
            float s = u * u * (3f - 2f * u);

            Vector3 centerA = cam.position + cam.forward * postDiceApproachDistance;

            for (int i = 0; i < postDice.Count; i++)
            {
                var d = postDice[i];
                if (d == null || d.tr == null) continue;

                Vector3 targetPos =
                    centerA +
                    cam.right * (d.planeOffset.x * 0.35f) +
                    cam.up * (d.planeOffset.y * 0.35f);

                d.tr.position = Vector3.Lerp(d.startPos, targetPos, s);
                d.rollAngle += d.rollSpeed * dt;
            }

            yield return null;
        }

        // 第二段起点
        seg2Start.Clear();
        for (int i = 0; i < postDice.Count; i++)
        {
            var d = postDice[i];
            seg2Start.Add(d != null && d.tr != null ? d.tr.position : Vector3.zero);
        }

        // 第二段：穿过镜头到镜头后
        while (p2 < 1f)
        {
            TryTriggerSlowMoByDistance();

            float speedMul = slowMoTriggered ? Mathf.Clamp01(slowMoSpeedMultiplier) : 1f;
            float dt = Time.deltaTime * speedMul;

            p2 += dt / Mathf.Max(0.0001f, half);
            float u = Mathf.Clamp01(p2);
            float s = u * u * (3f - 2f * u);

            Vector3 centerB = cam.position - cam.forward * postDiceOvershootBehindCameraDistance;

            for (int i = 0; i < postDice.Count; i++)
            {
                var d = postDice[i];
                if (d == null || d.tr == null) continue;

                Vector3 endPos =
                    centerB +
                    cam.right * (d.planeOffset.x * 0.35f) +
                    cam.up * (d.planeOffset.y * 0.35f);

                d.tr.position = Vector3.Lerp(seg2Start[i], endPos, s);
                d.rollAngle += d.rollSpeed * dt;
            }

            yield return null;
        }

        if (postDiceDestroyAfterArrive > 0.01f)
        {
            yield return new WaitForSeconds(postDiceDestroyAfterArrive);
            ClearPostDiceImmediate();
        }

        postDiceCo = null;
    }

    private void TryTriggerSlowMoByDistance()
    {
        if (!slowMoArmed || slowMoTriggered) return;
        if (cam == null) return;
        if (postDice.Count == 0) return;

        Vector3 center = GetPostDiceCenter(out int count);
        if (count == 0) return;

        float dist = Vector3.Distance(center, cam.position);
        if (dist <= slowMoTriggerDistance)
        {
            slowMoTriggered = true;
            slowEnergy = slowEnergyMax;

            FadeInSlowEnergyBarWhole();
            SetSlowEnergyUI(slowEnergy);

            if (useGlobalSlowMo)
            {
                Time.timeScale = Mathf.Clamp(globalSlowMoTimeScale, 0.01f, 1f);
                if (scaleFixedDeltaTimeWithTimeScale)
                    Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
            }
        }
    }

    private void UpdateSlowEnergyTickAndUI()
    {
        if (!slowMoTriggered) return;

        slowEnergy = Mathf.Max(0f, slowEnergy - slowEnergyDecayPerSecond * Time.unscaledDeltaTime);
        SetSlowEnergyUI(slowEnergy);

        if (slowEnergy <= 0.0001f)
        {
            EndSlowMo(forceRestoreTimeScale: false);
        }
    }

    private void EndSlowMo(bool forceRestoreTimeScale)
    {
        if (!slowMoTriggered && !forceRestoreTimeScale) return;

        slowMoTriggered = false;
        slowMoArmed = false;

        FadeOutSlowEnergyBarWhole();

        if (useGlobalSlowMo || forceRestoreTimeScale)
        {
            Time.timeScale = 1f;
            if (scaleFixedDeltaTimeWithTimeScale)
                Time.fixedDeltaTime = defaultFixedDeltaTime;
        }
    }

    // =========================
    // Post Dice Face-to-camera
    // =========================
    private void UpdatePostDiceFaceToCamera()
    {
        if (cam == null) return;
        if (postDice.Count == 0) return;

        Vector3 camUp = cam.up;

        for (int i = 0; i < postDice.Count; i++)
        {
            var d = postDice[i];
            if (d == null || d.tr == null) continue;

            Vector3 dirToCam = (cam.position - d.tr.position);
            if (dirToCam.sqrMagnitude < 0.000001f) dirToCam = cam.forward;
            dirToCam.Normalize();

            Quaternion look = Quaternion.LookRotation(dirToCam, camUp);
            Quaternion faceOffset = GetFaceOffsetToForward(d.value);
            Quaternion roll = Quaternion.AngleAxis(d.rollAngle, Vector3.forward);

            d.tr.rotation = look * roll * faceOffset;
        }
    }

    // =========================
    // Post Dice Fill Light helpers
    // =========================
    private void EnsurePostDiceFillLight()
    {
        if (!enablePostDiceFillLight) return;
        if (postDiceFillLight != null) return;

        GameObject go = new GameObject("PostDiceFillPointLight_Runtime");
        postDiceFillLight = go.AddComponent<Light>();
        postDiceFillLight.type = LightType.Point;
        postDiceFillLight.color = postDiceLightColor;
        postDiceFillLight.range = postDiceLightRange;
        postDiceFillLight.intensity = postDiceLightIntensity;
        postDiceFillLight.shadows = postDiceLightShadows ? LightShadows.Soft : LightShadows.None;
    }

    private Vector3 GetPostDiceCenter(out int count)
    {
        Vector3 sum = Vector3.zero;
        count = 0;
        for (int i = 0; i < postDice.Count; i++)
        {
            var d = postDice[i];
            if (d == null || d.tr == null) continue;
            sum += d.tr.position;
            count++;
        }
        if (count == 0) return Vector3.zero;
        return sum / count;
    }

    private void UpdatePostDiceFillLight()
    {
        if (postDiceFillLight == null) return;
        if (cam == null) return;
        if (postDice == null || postDice.Count == 0) return;

        Vector3 center = GetPostDiceCenter(out int count);
        if (count == 0) return;

        Vector3 toCam = (cam.position - center);
        if (toCam.sqrMagnitude < 0.000001f) toCam = -cam.forward;
        toCam.Normalize();

        Vector3 lightPos = center + toCam * postDiceLightForwardToCamera + cam.up * postDiceLightUpOffset;
        postDiceFillLight.transform.position = lightPos;
    }

    private void DestroyPostDiceFillLight()
    {
        if (postDiceFillLight == null) return;
        Destroy(postDiceFillLight.gameObject);
        postDiceFillLight = null;
    }

    private void ClearPostDiceImmediate()
    {
        if (postDiceCo != null)
        {
            StopCoroutine(postDiceCo);
            postDiceCo = null;
        }

        for (int i = postDice.Count - 1; i >= 0; i--)
        {
            var d = postDice[i];
            if (d != null && d.tr != null)
                Destroy(d.tr.gameObject);
        }
        postDice.Clear();

        EndSlowMo(forceRestoreTimeScale: true);
        DestroyPostDiceFillLight();
    }

    /// <summary>
    /// 将“某个点数的面”对齐到 local +Z（forward）
    /// 假设模型：+Z=1, +X=2, -Z=3, -X=4, +Y=5, -Y=6
    /// 如果你的模型不一致，就改这里的表
    /// </summary>
    private Quaternion GetFaceOffsetToForward(int value)
    {
        value = Mathf.Clamp(value, 1, 6);
        switch (value)
        {
            case 1: return Quaternion.Euler(0f, 0f, 0f);
            case 2: return Quaternion.Euler(0f, -90f, 0f);
            case 3: return Quaternion.Euler(0f, 180f, 0f);
            case 4: return Quaternion.Euler(0f, 90f, 0f);
            case 5: return Quaternion.Euler(90f, 0f, 0f);
            case 6: return Quaternion.Euler(-90f, 0f, 0f);
        }
        return Quaternion.identity;
    }

    // =========================
    // Flow (你的原流程)
    // =========================
    private void OnSpacePressed()
    {
        if (pressStart != null) pressStart.SetActive(false);
        waitingForSpace = false;
        SpawnBottom();
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
        if (cam == null) yield break;

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
        if (bottomPrefab == null) return;
        if (spawnOnce && bottomSpawned) return;

        Quaternion rot = Quaternion.Euler(bottomSpawnEuler);
        GameObject obj = Instantiate(bottomPrefab, bottomSpawnPos, rot);

        bottomTr = obj.transform;

        if (obj.GetComponent<Collider>() == null)
            obj.AddComponent<BoxCollider>();

        bottomRb = obj.GetComponent<Rigidbody>();
        if (bottomRb == null) bottomRb = obj.AddComponent<Rigidbody>();

        bottomRb.useGravity = true;
        bottomRb.isKinematic = false;
        bottomRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        bottomSpawned = true;

        StartCoroutine(FreezeWhenSettled(bottomRb));
        StartCoroutine(SpawnCupMoveThenSpawnDice(bottomSpawnPos));
    }

    private IEnumerator SpawnCupMoveThenSpawnDice(Vector3 diceSpawnCenter)
    {
        if (cupPrefab == null) yield break;

        Quaternion cupRot0 = Quaternion.Euler(cupOffscreenEuler);
        GameObject cup = Instantiate(cupPrefab, cupOffscreenPos, cupRot0);
        cupTr = cup.transform;

        yield return StartCoroutine(MoveTransformSmooth(
            cupTr,
            cupTargetPos,
            Quaternion.Euler(cupTargetEuler),
            cupMoveDuration
        ));

        if (firstDiceDelayAfterCupArrives > 0f)
            yield return new WaitForSeconds(firstDiceDelayAfterCupArrives);

        if (dicePrefab == null) yield break;

        for (int i = 0; i < diceCount; i++)
        {
            SpawnOneDie(diceSpawnCenter);
            yield return new WaitForSeconds(diceIntervalSeconds);
        }

        if (bottomTr != null)
        {
            if (bottomRb != null) bottomRb.isKinematic = true;

            yield return StartCoroutine(MoveTransformSmooth(
                bottomTr,
                bottomMidTargetPos,
                bottomTr.rotation,
                bottomMidDuration
            ));

            yield return StartCoroutine(MoveTransformSmooth(
                bottomTr,
                bottomFinalTargetPos,
                Quaternion.Euler(bottomFinalTargetEuler),
                bottomFinalDuration
            ));
        }

        yield return StartCoroutine(CreateGroupAndParentSmoothRotateAndMoveYThenMoveCamera_SecondMove());
    }

    private IEnumerator CreateGroupAndParentSmoothRotateAndMoveYThenMoveCamera_SecondMove()
    {
        if (cupTr == null || bottomTr == null) yield break;

        GameObject group = new GameObject(groupName);
        cupBottomGroupTr = group.transform;

        if (groupAtBottomPosition)
            group.transform.position = bottomTr.position;

        group.transform.rotation = Quaternion.identity;

        cupTr.SetParent(group.transform, true);
        bottomTr.SetParent(group.transform, true);

        StartCoroutine(RotateTransformSmooth(group.transform, Quaternion.Euler(groupTargetEuler), groupRotateDuration));
        StartCoroutine(MoveTransformToY(group.transform, groupTargetY, groupMoveYDuration));

        yield return new WaitForSeconds(Mathf.Max(groupRotateDuration, groupMoveYDuration));

        if (cam != null)
            yield return StartCoroutine(MoveTransformPositionOnly(cam, cameraAfterGroupPos, cameraAfterGroupMoveDuration));

        if (cupBottomGroupTr != null) groupBasePos = cupBottomGroupTr.position;
        groupSwayPhase = 0f;

        if (cam != null)
        {
            cameraBasePos = cam.position;
            cameraBaseRot = cam.rotation;
        }

        energyLogicEnabled = true;
        energy = 0f;
        SetEnergyUI(0f);
    }

    private void SpawnOneDie(Vector3 spawnCenter)
    {
        Vector3 pos = new Vector3(spawnCenter.x, spawnCenter.y + diceSpawnHeight, spawnCenter.z);
        GameObject d = Instantiate(dicePrefab, pos, Quaternion.identity);

        if (d.GetComponent<Collider>() == null)
            d.AddComponent<BoxCollider>();

        Rigidbody rb = d.GetComponent<Rigidbody>();
        if (rb == null) rb = d.AddComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        spawnedDice.Add(rb);
    }

    private IEnumerator MoveTransformSmooth(Transform tr, Vector3 toPos, Quaternion toRot, float duration)
    {
        if (tr == null) yield break;

        Vector3 fromPos = tr.position;
        Quaternion fromRot = tr.rotation;

        if (duration <= 0.0001f)
        {
            tr.SetPositionAndRotation(toPos, toRot);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float s = u * u * (3f - 2f * u);

            tr.position = Vector3.Lerp(fromPos, toPos, s);
            tr.rotation = Quaternion.Slerp(fromRot, toRot, s);
            yield return null;
        }

        tr.SetPositionAndRotation(toPos, toRot);
    }

    private IEnumerator RotateTransformSmooth(Transform tr, Quaternion toRot, float duration)
    {
        if (tr == null) yield break;

        Quaternion fromRot = tr.rotation;

        if (duration <= 0.0001f)
        {
            tr.rotation = toRot;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float s = u * u * (3f - 2f * u);

            tr.rotation = Quaternion.Slerp(fromRot, toRot, s);
            yield return null;
        }

        tr.rotation = toRot;
    }

    private IEnumerator MoveTransformToY(Transform tr, float targetY, float duration)
    {
        if (tr == null) yield break;

        Vector3 from = tr.position;
        Vector3 to = new Vector3(from.x, targetY, from.z);

        if (duration <= 0.0001f)
        {
            tr.position = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float s = u * u * (3f - 2f * u);

            tr.position = Vector3.Lerp(from, to, s);
            yield return null;
        }

        tr.position = to;
    }

    private IEnumerator MoveTransformPositionOnly(Transform tr, Vector3 toPos, float duration)
    {
        if (tr == null) yield break;

        Vector3 fromPos = tr.position;

        if (duration <= 0.0001f)
        {
            tr.position = toPos;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float s = u * u * (3f - 2f * u);

            tr.position = Vector3.Lerp(fromPos, toPos, s);
            yield return null;
        }

        tr.position = toPos;
    }

    private IEnumerator FreezeWhenSettled(Rigidbody rb)
    {
        float still = 0f;
        while (still < settleTime)
        {
            Vector3 v = rb.linearVelocity;
            Vector3 av = rb.angularVelocity;

            if (v.magnitude < settleSpeed && av.magnitude < settleSpeed)
                still += Time.deltaTime;
            else
                still = 0f;

            yield return null;
        }

        rb.isKinematic = true;
    }

    // =========================
    // Energy UI (原有)
    // =========================
    private void CacheEnergyGraphics()
    {
        energyGraphics.Clear();
        if (energyBarRoot == null) return;
        energyBarRoot.GetComponentsInChildren(true, energyGraphics);
    }

    private void HideEnergyBarImmediate()
    {
        if (energyBarRoot != null)
            energyBarRoot.SetActive(false);

        energyLogicEnabled = false;

        zeroIdleTimer = 0f;
        energyBarVisible = false;
        isFadingOut = false;
        currentEnergyAlpha = 0f;

        energyLocked = false;
        fullEnergySequenceStarted = false;
        cameraDriveEnabled = true;

        spawnedDice.Clear();
        ClearPostDiceImmediate();

        SetEnergyUI(0f);
    }

    private void FadeInEnergyBarWhole()
    {
        if (energyBarRoot == null) return;
        if (energyGraphics.Count == 0) CacheEnergyGraphics();

        if (!energyBarRoot.activeSelf)
            energyBarRoot.SetActive(true);

        if (energyFadeCo != null) StopCoroutine(energyFadeCo);

        SetAllEnergyGraphicsAlpha(currentEnergyAlpha);
        energyFadeCo = StartCoroutine(FadeAllEnergyGraphicsTo(1f, energyFadeInDuration));

        energyBarVisible = true;
        isFadingOut = false;
    }

    private void FadeOutEnergyBarWhole()
    {
        if (energyBarRoot == null) return;
        if (energyGraphics.Count == 0) CacheEnergyGraphics();

        if (!energyBarRoot.activeSelf)
            energyBarRoot.SetActive(true);

        if (energyFadeCo != null) StopCoroutine(energyFadeCo);

        SetAllEnergyGraphicsAlpha(currentEnergyAlpha);
        energyFadeCo = StartCoroutine(FadeAllEnergyGraphicsTo(0f, energyFadeOutDuration));

        isFadingOut = true;
    }

    private void SetAllEnergyGraphicsAlpha(float a)
    {
        for (int i = 0; i < energyGraphics.Count; i++)
        {
            var g = energyGraphics[i];
            if (g == null) continue;
            Color c = g.color; c.a = a; g.color = c;
        }
    }

    private IEnumerator FadeAllEnergyGraphicsTo(float targetAlpha, float duration)
    {
        float from = currentEnergyAlpha;

        if (duration <= 0.0001f)
        {
            currentEnergyAlpha = targetAlpha;
            SetAllEnergyGraphicsAlpha(targetAlpha);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float s = u * u * (3f - 2f * u);

            float a = Mathf.Lerp(from, targetAlpha, s);
            currentEnergyAlpha = a;
            SetAllEnergyGraphicsAlpha(a);
            yield return null;
        }

        currentEnergyAlpha = targetAlpha;
        SetAllEnergyGraphicsAlpha(targetAlpha);

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            energyBarVisible = false;
            isFadingOut = false;
        }
    }

    private void SetEnergyUI(float energyValue)
    {
        float norm = Mathf.Clamp01(energyValue / 100f);
        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = norm;

            Color c = Color.Lerp(energyLowColor, energyHighColor, norm);
            if (keepAlphaDrivenByFade) c.a = energyFillImage.color.a;
            energyFillImage.color = c;
        }
    }

    // =========================
    // SlowEnergy UI (渐显/渐隐 + fillAmount)
    // =========================
    private void CacheSlowEnergyGraphics()
    {
        slowEnergyGraphics.Clear();
        if (slowEnergyBarRoot == null) return;
        slowEnergyBarRoot.GetComponentsInChildren(true, slowEnergyGraphics);
    }

    private void HideSlowEnergyBarImmediate()
    {
        if (slowEnergyBarRoot != null) slowEnergyBarRoot.SetActive(false);
        slowEnergyAlpha = 0f;
        slowEnergyBarVisible = false;
        SetSlowEnergyGraphicsAlpha(0f);
    }

    private void FadeInSlowEnergyBarWhole()
    {
        if (slowEnergyBarRoot == null) return;
        if (slowEnergyGraphics.Count == 0) CacheSlowEnergyGraphics();

        if (!slowEnergyBarRoot.activeSelf)
            slowEnergyBarRoot.SetActive(true);

        if (slowEnergyFadeCo != null) StopCoroutine(slowEnergyFadeCo);

        SetSlowEnergyGraphicsAlpha(slowEnergyAlpha);
        slowEnergyFadeCo = StartCoroutine(FadeSlowEnergyGraphicsTo(1f, slowEnergyFadeInDuration));
        slowEnergyBarVisible = true;
    }

    private void FadeOutSlowEnergyBarWhole()
    {
        if (slowEnergyBarRoot == null) return;
        if (slowEnergyGraphics.Count == 0) CacheSlowEnergyGraphics();

        if (!slowEnergyBarRoot.activeSelf)
            slowEnergyBarRoot.SetActive(true);

        if (slowEnergyFadeCo != null) StopCoroutine(slowEnergyFadeCo);

        SetSlowEnergyGraphicsAlpha(slowEnergyAlpha);
        slowEnergyFadeCo = StartCoroutine(FadeSlowEnergyGraphicsTo(0f, slowEnergyFadeOutDuration));
    }

    private void SetSlowEnergyGraphicsAlpha(float a)
    {
        for (int i = 0; i < slowEnergyGraphics.Count; i++)
        {
            var g = slowEnergyGraphics[i];
            if (g == null) continue;
            Color c = g.color; c.a = a; g.color = c;
        }
        slowEnergyAlpha = a;
    }

    private IEnumerator FadeSlowEnergyGraphicsTo(float targetAlpha, float duration)
    {
        float from = slowEnergyAlpha;

        if (duration <= 0.0001f)
        {
            SetSlowEnergyGraphicsAlpha(targetAlpha);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            float s = u * u * (3f - 2f * u);
            float a = Mathf.Lerp(from, targetAlpha, s);
            SetSlowEnergyGraphicsAlpha(a);
            yield return null;
        }

        SetSlowEnergyGraphicsAlpha(targetAlpha);

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            slowEnergyBarVisible = false;
            if (slowEnergyBarRoot != null) slowEnergyBarRoot.SetActive(false);
        }
    }

    private void SetSlowEnergyUI(float slowEnergyValue)
    {
        if (slowEnergyFillImage == null) return;
        float norm = Mathf.Clamp01(slowEnergyValue / Mathf.Max(0.0001f, slowEnergyMax));
        slowEnergyFillImage.fillAmount = norm;
    }
}