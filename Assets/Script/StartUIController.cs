using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartUIController : MonoBehaviour
{
    // =========================
    // Bullet Time Fill Light (AUTO SPAWN POINT, near camera)
    // =========================
    [Header("Bullet Time Fill Light (Auto Spawn Point, near camera)")]
    [SerializeField] private bool autoSpawnDiceFillLight = true;
    [SerializeField] private Color fillLightColor = new Color(1f, 0.92f, 0.82f, 1f);
    [SerializeField] private float fillLightRange = 2.2f;
    [SerializeField] private float fillLightIntensity = 1.6f;
    [SerializeField] private float fillLightForwardToCamera = 0.25f; // 往镜头方向推多远
    [SerializeField] private float fillLightUp = 0.25f;              // 稍微抬高
    [SerializeField] private bool fillLightShadows = false;

    private Light runtimeDiceFillLight;

    // =========================
    // Full Energy Burst FX
    // =========================
    [Header("Full Energy Burst FX")]
    [SerializeField] private GameObject fullEnergyBurstPrefab;   // FX_Smoke_13.prefab
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
    // Dice Fly To Camera + Bullet Time + Click Randomize
    // =========================
    [Header("Dice Fly To Camera + Bullet Time")]
    [SerializeField] private float diceFlyDuration = 0.55f;
    [SerializeField] private float diceHoldDistance = 0.55f;
    [SerializeField] private float diceSpreadX = 0.22f;
    [SerializeField] private float diceSpreadY = 0.10f;
    [SerializeField] private float diceApproachSpin = 720f;
    [SerializeField] private float diceCollisionOffSeconds = 0.20f;

    [Header("Bullet Time")]
    [SerializeField] private bool enableBulletTime = true;
    [SerializeField] private float bulletTimeScale = 0.08f;
    [SerializeField] private float bulletTimeFixedDelta = 0.002f;

    [Header("Bullet Time Dice Motion")]
    [SerializeField] private float bulletDiceDriftSpeed = 0.1f;   // 子弹时间里漂移速度
    [SerializeField] private float bulletDiceMaxDrift = 0.35f;     // 最大漂移距离（防穿镜头）
    [SerializeField] private bool bulletDiceDriftTowardCamera = true;

    [Header("Dice Click Randomize (Smooth)")]
    [SerializeField] private LayerMask diceRaycastMask = ~0;
    [SerializeField] private float diceRotateDuration = 0.18f; // ✅ 点击后平滑换面的时长

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

    // 第一次镜头移动的目的地
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

    // Energy
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

    // Bullet time
    private bool inBulletTime = false;
    private float defaultFixedDelta;
    private Vector3 bulletDiceStartCenter;

    private Camera camComp;

    private void Awake()
    {
        defaultFixedDelta = Time.fixedDeltaTime;
        camComp = (cam != null) ? cam.GetComponent<Camera>() : null;

        if (pressStart != null) pressStart.SetActive(false);

        CacheEnergyGraphics();
        HideEnergyBarImmediate();
        SetEnergyUI(0f);

        DestroyDiceFillLight();
    }

    private void OnDisable()
    {
        ExitBulletTime();
        DestroyDiceFillLight();
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

        // ✅ 子弹时间：骰子继续漂移 + 点击平滑换面
        if (inBulletTime)
        {
            UpdateBulletTimeDiceDrift();
            HandleDiceClickSmooth();
            return;
        }

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
        UpdateDiceFillLightPosition();

        if (!energyLogicEnabled) return;
        if (!cameraDriveEnabled) return;
        if (inBulletTime) return;

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
            cam.position = Vector3.Lerp(
                cam.position,
                desiredPos,
                1f - Mathf.Exp(-cameraDriveLerp * Time.deltaTime)
            );

            if (driveCameraRotationToo)
            {
                Quaternion targetRot = Quaternion.Euler(targetEuler);
                Quaternion desiredRot = Quaternion.Slerp(cameraBaseRot, targetRot, norm);
                cam.rotation = Quaternion.Slerp(
                    cam.rotation,
                    desiredRot,
                    1f - Mathf.Exp(-cameraDriveLerp * Time.deltaTime)
                );
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
        yield return StartCoroutine(FlyDiceToCameraThenEnterBulletTime());
    }

    private IEnumerator CameraSnapBackShakeBurstAndBreak_KeepCameraStill()
    {
        if (cam == null) yield break;

        Vector3 basePos = cameraBasePos != Vector3.zero ? cameraBasePos : cameraAfterGroupPos;
        Quaternion baseRot = cameraBaseRot != Quaternion.identity ? cameraBaseRot : cam.rotation;

        cameraDriveEnabled = false;

        SpawnFullEnergyBurstFX();
        BreakApartGroupAndLaunch();

        Vector3 fromPos = cam.position;
        Quaternion fromRot = cam.rotation;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, camSnapBackDuration);
        while (t < dur)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / dur);
            float s = EaseOutBack(u);

            cam.position = Vector3.LerpUnclamped(fromPos, basePos, s);
            cam.rotation = Quaternion.SlerpUnclamped(fromRot, baseRot, s);
            yield return null;
        }

        cam.position = basePos;
        cam.rotation = baseRot;

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

    private IEnumerator FlyDiceToCameraThenEnterBulletTime()
    {
        if (cam == null) yield break;

        List<Rigidbody> dice = new List<Rigidbody>();
        for (int i = 0; i < spawnedDice.Count && dice.Count < 5; i++)
            if (spawnedDice[i] != null) dice.Add(spawnedDice[i]);
        if (dice.Count == 0) yield break;

        Vector3 targetCenter = cam.position + cam.forward * diceHoldDistance;

        for (int i = 0; i < dice.Count; i++)
        {
            Rigidbody rb = dice[i];
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (diceCollisionOffSeconds > 0.001f)
                StartCoroutine(DisableCollisionTemporarily(rb, diceCollisionOffSeconds));
        }

        Vector3[] startPos = new Vector3[dice.Count];
        for (int i = 0; i < dice.Count; i++) startPos[i] = dice[i].position;

        float t = 0f;
        while (t < diceFlyDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / diceFlyDuration);
            float s = u * u * (3f - 2f * u);

            targetCenter = cam.position + cam.forward * diceHoldDistance;
            Vector3 right = cam.right;
            Vector3 up = cam.up;

            for (int i = 0; i < dice.Count; i++)
            {
                float sx = (i - (dice.Count - 1) * 0.5f) * diceSpreadX;
                float sy = ((i % 2 == 0) ? diceSpreadY : -diceSpreadY);
                Vector3 targetPos = targetCenter + right * sx + up * sy;

                Rigidbody rb = dice[i];
                rb.position = Vector3.Lerp(startPos[i], targetPos, s);

                Quaternion spin = Quaternion.Euler(
                    (diceApproachSpin * u) * (i + 1) * Time.deltaTime,
                    (diceApproachSpin * 0.7f * u) * Time.deltaTime,
                    (diceApproachSpin * 0.5f * u) * Time.deltaTime
                );
                rb.rotation = spin * rb.rotation;
            }

            yield return null;
        }

        if (enableBulletTime)
            EnterBulletTime();

        inBulletTime = true;

        // ✅ 记录进入子弹时间的中心（用于限制漂移距离）
        bulletDiceStartCenter = GetDiceCenterWorld();

        // ✅ 进入子弹时间瞬间：所有骰子顶面朝向镜头（保证你看得到顶面点数）
        AlignAllDiceTopToCamera();

        // ✅ 点光生成（骰子中心靠近 camera 一点）
        EnsureDiceFillLight();
        UpdateDiceFillLightPosition();
    }

    private void AlignAllDiceTopToCamera()
    {
        if (cam == null) return;

        // 让顶面“朝向屏幕/相机”：up -> -cam.forward
        Vector3 targetNormal = -cam.forward;

        for (int i = 0; i < spawnedDice.Count; i++)
        {
            Rigidbody rb = spawnedDice[i];
            if (rb == null) continue;

            Quaternion delta = Quaternion.FromToRotation(rb.transform.up, targetNormal);
            rb.rotation = delta * rb.rotation;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void UpdateBulletTimeDiceDrift()
    {
        if (cam == null) return;

        Vector3 dir = bulletDiceDriftTowardCamera ? (-cam.forward) : (cam.forward);
        float dt = Time.unscaledDeltaTime;

        Vector3 centerNow = GetDiceCenterWorld();
        float moved = Vector3.Dot(centerNow - bulletDiceStartCenter, dir);

        if (moved >= bulletDiceMaxDrift) return;

        Vector3 step = dir * bulletDiceDriftSpeed * dt;

        for (int i = 0; i < spawnedDice.Count; i++)
        {
            Rigidbody rb = spawnedDice[i];
            if (rb == null) continue;
            rb.position += step;
        }
    }

    private void EnterBulletTime()
    {
        if (inBulletTime) return;
        Time.timeScale = Mathf.Clamp(bulletTimeScale, 0.01f, 1f);
        Time.fixedDeltaTime = bulletTimeFixedDelta;
    }

    private void ExitBulletTime()
    {
        if (!inBulletTime) return;
        inBulletTime = false;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDelta;

        DestroyDiceFillLight();
    }

    private void HandleDiceClickSmooth()
    {
        if (cam == null) return;
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = (camComp != null)
            ? camComp.ScreenPointToRay(Mouse.current.position.ReadValue())
            : new Ray(cam.position, cam.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, diceRaycastMask, QueryTriggerInteraction.Ignore))
        {
            DieController dc = hit.collider.GetComponentInParent<DieController>();
            if (dc != null)
            {
                dc.RandomizeFaceSmooth(diceRotateDuration); // ✅ 平滑换点数
            }
        }
    }

    private IEnumerator DisableCollisionTemporarily(Rigidbody rb, float seconds)
    {
        if (rb == null) yield break;

        Collider[] cols = rb.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
            if (cols[i] != null) cols[i].enabled = false;

        yield return new WaitForSeconds(seconds);

        for (int i = 0; i < cols.Length; i++)
            if (cols[i] != null) cols[i].enabled = true;
    }

    // =========================
    // Auto Point Light Helpers (near camera)
    // =========================
    private Vector3 GetFillLightPosNearCamera()
    {
        Vector3 center = GetDiceCenterWorld();
        if (cam == null) return center + Vector3.up * fillLightUp;

        Vector3 toCam = cam.position - center;
        if (toCam.sqrMagnitude < 0.000001f) toCam = -cam.forward;
        toCam.Normalize();

        return center + toCam * fillLightForwardToCamera + Vector3.up * fillLightUp;
    }

    private void EnsureDiceFillLight()
    {
        if (!autoSpawnDiceFillLight) return;
        if (runtimeDiceFillLight != null) return;

        GameObject go = new GameObject("DiceFillPointLight_Runtime");
        runtimeDiceFillLight = go.AddComponent<Light>();

        runtimeDiceFillLight.type = LightType.Point;
        runtimeDiceFillLight.color = fillLightColor;
        runtimeDiceFillLight.range = fillLightRange;
        runtimeDiceFillLight.intensity = fillLightIntensity;
        runtimeDiceFillLight.shadows = fillLightShadows ? LightShadows.Soft : LightShadows.None;

        go.transform.position = GetFillLightPosNearCamera();
    }

    private void UpdateDiceFillLightPosition()
    {
        if (runtimeDiceFillLight == null) return;
        runtimeDiceFillLight.transform.position = GetFillLightPosNearCamera();
    }

    private Vector3 GetDiceCenterWorld()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        for (int i = 0; i < spawnedDice.Count; i++)
        {
            Rigidbody rb = spawnedDice[i];
            if (rb == null) continue;
            sum += rb.position;
            count++;
        }

        if (count == 0)
        {
            if (cam != null) return cam.position + cam.forward * diceHoldDistance;
            return Vector3.zero;
        }

        return sum / count;
    }

    private void DestroyDiceFillLight()
    {
        if (runtimeDiceFillLight == null) return;
        Destroy(runtimeDiceFillLight.gameObject);
        runtimeDiceFillLight = null;
    }

    // =========================
    // Burst FX + Break Apart
    // =========================
    private void SpawnFullEnergyBurstFX()
    {
        if (fullEnergyBurstPrefab == null) return;

        Vector3 pos = (cupBottomGroupTr != null) ? cupBottomGroupTr.position : groupBasePos;
        Quaternion rot = Quaternion.identity;

        GameObject fx = Instantiate(fullEnergyBurstPrefab, pos, rot);

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

    private void BreakApartGroupAndLaunch()
    {
        if (cupBottomGroupTr == null) return;

        Vector3 origin = cupBottomGroupTr.position;

        if (cupTr != null) cupTr.SetParent(null, true);
        if (bottomTr != null) bottomTr.SetParent(null, true);

        if (destroyGroupAfterBreak)
            Destroy(cupBottomGroupTr.gameObject);

        LaunchOne(cupTr, origin);
        LaunchOne(bottomTr, origin);
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

    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
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
    // Energy UI (no CanvasGroup)
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

        ExitBulletTime();
        DestroyDiceFillLight();

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

            Color c = g.color;
            c.a = a;
            g.color = c;
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
}