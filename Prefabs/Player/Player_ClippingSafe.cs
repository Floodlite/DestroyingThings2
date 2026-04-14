using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerClippingSafe : MonoBehaviour
{
    private Destroyer destroyer;
    private InputAction move;

    public Rigidbody rb;
    private Collider cachedCollider;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpHeight = 30f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float fallForce = 8f;
    [SerializeField] private float storedSpeed = 1f;
    [SerializeField] private float accelSpeed = 1f;
    [SerializeField] private float airborneMovement = 1f;
    [SerializeField] private float yourSpeed = 1f;
    [SerializeField] private float hoofSpeed = 24f;
    [SerializeField] private float maxDisplacementPerStep = 1.5f;
    [SerializeField] private float collisionSkin = 0.03f;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float groundCheckHeight = 0.05f;
    [SerializeField] private float wallProjectionThreshold = 0.01f;
    [SerializeField] private bool sneaking = false;
    [SerializeField] private bool drifting = false;
    [SerializeField] private bool accelerating = false;
    [SerializeField] private bool accelerationEnabled = true;

    private Vector3 moveDirection = Vector3.zero;
    public Vector3 movingDirection;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private bool breakdanceMode = false;
    [SerializeField] private bool hoofRecharged = true;

    [SerializeField] private bool boxHit = false;
    [SerializeField] private float boxDistance = 3f;
    [SerializeField] private RaycastHit objectHit;

    [SerializeField] private Vector3 gizmoCubeVector = new Vector3(0f, 1.5f, 0f);

    [SerializeField] private bool targetGet = false;
    [SerializeField] private GameObject targetAura;
    [SerializeField] private GameObject chargeAura;

    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] public int attackDamage = 1;
    private float basePunchDuration = 0.05f;
    private float basePunchSize = 3f;
    private float punchDuration;
    private float punchSize;
    private float startTime;
    private float currentTime;
    private bool punchInProgress;
    private float holdTime;
    [SerializeField] private float throttle = 1f;

    private bool jumpQueued;
    private bool highJumpQueued;
    private bool hoofQueued;
    private Vector3 queuedBurstImpulse = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();
        destroyer = new Destroyer();

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        Debug.Log("It's destruction time");
    }

    private void OnEnable()
    {
        move = destroyer.Player.Move;
        destroyer.Player.Move.Enable();

        destroyer.Player.Breakdance.started += BreakOut;
        destroyer.Player.Breakdance.Enable();

        destroyer.Player.Jump.started += HowToJump;
        destroyer.Player.Jump.Enable();

        destroyer.Player.Charge.started += ChargeCommand;
        destroyer.Player.Charge.Enable();

        destroyer.Player.Sneak.started += Sneak;
        destroyer.Player.Sneak.Enable();

        destroyer.Player.Drift.started += Drift;
        destroyer.Player.Drift.Enable();

        destroyer.Player.Accelerate.started += Accelerator;
        destroyer.Player.Accelerate.Enable();

        destroyer.Player.Punch.started += BeginPunch;
        destroyer.Player.Punch.Enable();
        destroyer.Player.Punch.canceled += ReleasePunch;

        destroyer.Player.High_Jump.started += HighJump;
        destroyer.Player.High_Jump.Enable();

        destroyer.Player.Hoof_It.started += HoofIt;
        destroyer.Player.Hoof_It.Enable();
        destroyer.Player.Hoof_It.AddCompositeBinding("OneModifier")
            .With("Binding", "<Keyboard>/W")
            .With("Binding", "<Keyboard>/A")
            .With("Binding", "<Keyboard>/S")
            .With("Binding", "<Keyboard>/D")
            .With("Modifier", "<Keyboard>/H");

        destroyer.Player.Escape.performed += GetOut;
        destroyer.Player.Escape.Enable();

        destroyer.Player.Enable();

        if (targetAura != null)
        {
            targetAura.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        destroyer.Player.Jump.started -= HowToJump;
        destroyer.Player.Breakdance.started -= BreakOut;
        destroyer.Player.High_Jump.started -= HighJump;
        destroyer.Player.Hoof_It.started -= HoofIt;
        destroyer.Player.Charge.started -= ChargeCommand;
        destroyer.Player.Sneak.started -= Sneak;
        destroyer.Player.Drift.started -= Drift;
        destroyer.Player.Accelerate.started -= Accelerator;
        destroyer.Player.Punch.started -= BeginPunch;
        destroyer.Player.Punch.canceled -= ReleasePunch;
        destroyer.Player.Escape.performed -= GetOut;
        destroyer.Player.Disable();
    }

    private void FixedUpdate()
    {
        yourSpeed = moveSpeed * storedSpeed * airborneMovement * accelSpeed;
        Vector2 moveInput = move.ReadValue<Vector2>();
        Vector3 desiredMoveForce = Vector3.zero;
        desiredMoveForce += moveInput.x * GetCameraR(playerCamera) * yourSpeed;
        desiredMoveForce += moveInput.y * GetCameraF(playerCamera) * yourSpeed;
        moveDirection = desiredMoveForce;
        movingDirection = desiredMoveForce;

        if (drifting)
        {
            transform.rotation *= Quaternion.AngleAxis(15f, Vector3.up);
        }

        rb.AddForce(desiredMoveForce, ForceMode.Acceleration);
        ProcessQueuedMovementActions(moveInput);

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * fallForce * Time.fixedDeltaTime;
        }

        ClampHorizontalVelocity();
        ClampStepDisplacement();

        if (!breakdanceMode)
        {
            LookDirection();
        }
    }

    private void ProcessQueuedMovementActions(Vector2 moveInput)
    {
        if (jumpQueued)
        {
            if (Grounded())
            {
                queuedBurstImpulse += Vector3.up * jumpHeight;
                StartCoroutine(GroundCheck());
            }
            else
            {
                queuedBurstImpulse += Vector3.up * jumpHeight * 0.055f;
            }
            jumpQueued = false;
        }

        if (highJumpQueued)
        {
            if (Grounded() && !breakdanceMode)
            {
                queuedBurstImpulse += Vector3.up * jumpHeight * 1.75f;
                queuedBurstImpulse += transform.forward * jumpHeight * 2.5f;
                StartCoroutine(GroundCheck());
                Debug.Log("Go long");
            }
            highJumpQueued = false;
        }

        if (hoofQueued)
        {
            if (!Grounded() && hoofRecharged)
            {
                hoofRecharged = false;
                queuedBurstImpulse += moveInput.x * GetCameraR(playerCamera) * moveSpeed * hoofSpeed;
                queuedBurstImpulse += moveInput.y * GetCameraF(playerCamera) * moveSpeed * hoofSpeed;
                queuedBurstImpulse += Vector3.down;
                Debug.Log("Hoofing it: Strong");
                StartCoroutine(RechargeHoof());
            }
            hoofQueued = false;
        }

        if (queuedBurstImpulse.sqrMagnitude > 0f)
        {
            ApplyBurstImpulseSafely(queuedBurstImpulse);
            queuedBurstImpulse = Vector3.zero;
        }
    }

    private void ApplyBurstImpulseSafely(Vector3 impulse)
    {
        float safeMass = Mathf.Max(0.0001f, rb.mass);
        Vector3 predictedVelocity = rb.linearVelocity + (impulse / safeMass);
        Vector3 predictedDisplacement = predictedVelocity * Time.fixedDeltaTime;

        if (TrySweep(predictedDisplacement, out RaycastHit sweepHit))
        {
            float safeDistance = Mathf.Max(0f, sweepHit.distance - collisionSkin);
            if (safeDistance > 0f && predictedDisplacement.sqrMagnitude > 0f)
            {
                rb.position += predictedDisplacement.normalized * safeDistance;
            }

            rb.linearVelocity = Vector3.ProjectOnPlane(predictedVelocity, sweepHit.normal);
            return;
        }

        rb.AddForce(impulse, ForceMode.Impulse);
    }

    private bool TrySweep(Vector3 displacement, out RaycastHit hit)
    {
        if (displacement.sqrMagnitude <= 0f)
        {
            hit = default;
            return false;
        }

        Vector3 direction = displacement.normalized;
        float distance = displacement.magnitude + collisionSkin;
        return rb.SweepTest(direction, out hit, distance, QueryTriggerInteraction.Ignore);
    }

    private void ClampHorizontalVelocity()
    {
        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }
    }

    private void ClampStepDisplacement()
    {
        if (Time.fixedDeltaTime <= 0f)
        {
            return;
        }

        float maxAllowedStepSpeed = maxDisplacementPerStep / Time.fixedDeltaTime;
        if (rb.linearVelocity.magnitude > maxAllowedStepSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxAllowedStepSpeed;
        }
    }

    public float getPlayerSpeed()
    {
        return yourSpeed;
    }

    public Vector3 getPlayerMoveDirection()
    {
        return movingDirection;
    }

    private void AccelerationII()
    {
        if (accelerating)
        {
            accelSpeed += 0.01f;
        }
        else
        {
            if (accelSpeed > 1f && accelSpeed < 10f)
            {
                accelSpeed -= 0.01f;
            }
            else if (accelSpeed > 10f)
            {
                accelSpeed -= 0.02f;
            }
        }

        if (accelSpeed > maxSpeed / 4f + (storedSpeed * 0.1f))
        {
            accelSpeed -= 0.1f;
        }
    }

    private void GetOut(InputAction.CallbackContext obj)
    {
        Debug.Log("Hmph.");
        Application.Quit();
    }

    private void BreakOut(InputAction.CallbackContext obj)
    {
        breakdanceMode = !breakdanceMode;
        rb.automaticCenterOfMass = !rb.automaticCenterOfMass;
        if (breakdanceMode)
        {
            Debug.Log("Funk on");
        }
        else
        {
            Debug.Log("Funk off");
        }

        if (breakdanceMode)
        {
            moveSpeed += 10f;
            Time.timeScale *= 2f;
            rb.linearDamping = 0f;
            jumpHeight /= 4f;
        }
        else
        {
            moveSpeed -= 10f;
            Time.timeScale = 1f;
            rb.linearDamping = 3.5f;
            jumpHeight *= 4f;
        }
    }

    private void HowToJump(InputAction.CallbackContext obj)
    {
        jumpQueued = true;
    }

    private void HighJump(InputAction.CallbackContext obj)
    {
        highJumpQueued = true;
    }

    private void HoofIt(InputAction.CallbackContext obj)
    {
        hoofQueued = true;
    }

    private void Sneak(InputAction.CallbackContext obj)
    {
        sneaking = !sneaking;
        if (sneaking)
        {
            moveSpeed -= 13f;
            jumpHeight += 2f;
            fallForce += 2f;
        }
        else
        {
            moveSpeed += 13f;
            jumpHeight -= 2f;
            fallForce -= 2f;
        }
    }

    private void Drift(InputAction.CallbackContext obj)
    {
        Debug.Log("Spin me right round");
        drifting = !drifting;
        if (drifting)
        {
            rb.angularDamping = 8000f;
            moveSpeed -= 1.5f;
        }
        else
        {
            rb.angularDamping = 0.05f;
            moveSpeed += 1.5f;
        }
    }

    private void Accelerator(InputAction.CallbackContext obj)
    {
        Debug.Log("Acceleration toggled");
        accelerationEnabled = !accelerationEnabled;
        if (!accelerationEnabled)
        {
            accelSpeed = 1f;
        }
    }

    private void BeginPunch(InputAction.CallbackContext obj)
    {
        punchInProgress = true;
        startTime = Time.time;
    }

    private void ReleasePunch(InputAction.CallbackContext obj)
    {
        if (punchInProgress)
        {
            punchDuration = basePunchDuration;
            punchSize = basePunchSize;
            if (drifting)
            {
                punchDuration *= 0.5f;
                punchSize *= 0.75f;
            }
            if (sneaking)
            {
                punchDuration *= 1.5f;
                punchSize *= 1.25f;
            }
            if (accelerationEnabled)
            {
                punchDuration *= 0.85f;
                punchSize *= 1.15f;
            }

            currentTime = Time.time;
            holdTime = currentTime - startTime;

            playerAttack.BringTheHurtII(punchDuration, punchSize, false);
            punchInProgress = false;
        }
    }

    public int GetDamage()
    {
        return attackDamage;
    }

    public void ChangeThrottle(float degree)
    {
        throttle = degree;
    }

    private void ChargeCommand(InputAction.CallbackContext obj)
    {
        if (!Grounded())
        {
            StartCoroutine(PowerToGround());
        }
    }

    public bool Grounded()
    {
        if (cachedCollider == null)
        {
            cachedCollider = GetComponent<Collider>();
            if (cachedCollider == null)
            {
                boxHit = false;
                return false;
            }
        }

        Bounds b = cachedCollider.bounds;
        Vector3 center = b.center;
        Vector3 halfExtents = new Vector3(
            Mathf.Max(0.02f, b.extents.x * 0.92f),
            groundCheckHeight,
            Mathf.Max(0.02f, b.extents.z * 0.92f)
        );
        float castDistance = groundCheckDistance + Mathf.Abs(rb.linearVelocity.y) * Time.fixedDeltaTime;
        boxHit = Physics.BoxCast(center, halfExtents, Vector3.down, out objectHit, transform.rotation, castDistance, ~0, QueryTriggerInteraction.Ignore);
        return boxHit;
    }

    private void OnDrawGizmos()
    {
        if (boxHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * boxDistance);
            Gizmos.DrawWireCube(transform.position + Vector3.down * boxDistance, transform.localScale);
        }
        else
        {
            Gizmos.color = Color.orange;
            Gizmos.DrawRay(transform.position, Vector3.down * boxDistance);
            Gizmos.DrawWireCube(transform.position + Vector3.down * boxDistance, transform.localScale);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + gizmoCubeVector, 2f);
    }

    private IEnumerator GroundCheck()
    {
        yield return new WaitForSeconds(0.5f);
        if (!Grounded() && !sneaking)
        {
            airborneMovement *= 0.2f;
            yield return null;
        }
        else if (!Grounded() && sneaking)
        {
            airborneMovement *= 0.8f;
            yield return null;
        }

        while (!Grounded())
        {
            yield return new WaitForSeconds(0.2f);
        }
        airborneMovement = 1f;
    }

    private IEnumerator PowerToGround()
    {
        if (storedSpeed <= 1f)
        {
            storedSpeed = 1f;
        }
        yield return new WaitForSeconds(0.2f);
        while (!Grounded())
        {
            storedSpeed += 0.4f;
            chargeAura.gameObject.SetActive(true);
            chargeAura.gameObject.GetComponent<ParticleSystem>().Play(true);
            chargeAura.gameObject.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
            yield return new WaitForSeconds(0.1f);
        }

        while (storedSpeed > 1f)
        {
            storedSpeed -= 0.2f;
            yield return new WaitForSeconds(0.3f);
        }
        if (storedSpeed <= 1f)
        {
            storedSpeed = 1f;
            chargeAura.gameObject.GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
            chargeAura.gameObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            chargeAura.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator RechargeHoof()
    {
        hoofRecharged = false;
        while (!Grounded())
        {
            yield return new WaitForSeconds(0.1f);
        }
        hoofRecharged = true;
    }

    private void LookDirection()
    {
        Vector3 direction = rb.linearVelocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 GetCameraF(Camera sourceCamera)
    {
        if (sourceCamera == null)
        {
            return transform.forward;
        }

        Vector3 forward = sourceCamera.transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    private Vector3 GetCameraR(Camera sourceCamera)
    {
        if (sourceCamera == null)
        {
            return transform.right;
        }

        Vector3 right = sourceCamera.transform.right;
        right.y = 0f;
        return right.normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProjectVelocityOnContacts(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        ProjectVelocityOnContacts(collision);
    }

    private void ProjectVelocityOnContacts(Collision collision)
    {
        Vector3 velocity = rb.linearVelocity;

        foreach (ContactPoint contact in collision.contacts)
        {
            float intoSurface = Vector3.Dot(velocity, contact.normal);
            if (intoSurface < -wallProjectionThreshold)
            {
                velocity = Vector3.ProjectOnPlane(velocity, contact.normal);
            }
        }

        rb.linearVelocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            targetGet = true;
            Destroy(other.gameObject);
            Debug.Log("Target acquired");
            if (targetAura != null)
            {
                targetAura.gameObject.SetActive(true);
            }
        }

        if (other.CompareTag("Finish"))
        {
            if (targetGet)
            {
                Destroy(other.gameObject);
                Debug.Log("Win");
                if (targetAura != null)
                {
                    targetAura.gameObject.SetActive(false);
                }
                targetGet = false;
            }
        }
    }
}
