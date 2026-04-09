using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class Player : MonoBehaviour
{
    private Destroyer destroyer;
    private InputAction move;

    public Rigidbody rb;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpHeight = 30f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float fallForce = 8f;
    [SerializeField] private float storedSpeed = 1f;
    [SerializeField] private float accelSpeed = 1f;
    [SerializeField] private float airborneMovement = 1f;
    [SerializeField] private float yourSpeed = 1f;
    private float hoofSpeed = 24f;
    //private bool hoofCooldown = false;
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
    [SerializeField] private float throttle = 1;




    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        destroyer = new Destroyer();
        Collider collider = GetComponent<Collider>();
    }

    void Start()
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
        //destroyer.Player.High_Jump.AddCompositeBinding("OneModifier").With("Binding", "<Keyboard>/Space").With("Modifier", "<Keyboard>/Shift");

        destroyer.Player.Hoof_It.started += HoofIt;
        destroyer.Player.Hoof_It.Enable();
        destroyer.Player.Hoof_It.AddCompositeBinding("OneModifier").With("Binding", "<Keyboard>/W").With("Binding", "<Keyboard>/A").With("Binding", "<Keyboard>/S").With("Binding", "<Keyboard>/D").With("Modifier", "<Keyboard>/H");


        destroyer.Player.Escape.performed += GetOut;
        destroyer.Player.Escape.Enable();

        destroyer.Player.Enable();

        targetAura.gameObject.SetActive(false);

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
        destroyer.Player.Disable();
    }


    private void FixedUpdate()
    {
        yourSpeed = moveSpeed * storedSpeed * airborneMovement * accelSpeed;
        moveDirection += move.ReadValue<Vector2>().x * GetCameraR(playerCamera) * yourSpeed;
        moveDirection += move.ReadValue<Vector2>().y * GetCameraF(playerCamera) * yourSpeed;
        movingDirection = moveDirection;
        
        /*if (movingDirection != Vector3.zero && !sneaking && accelerationEnabled) {
            accelerating = true;
            InvokeRepeating("AccelerationII", 2f, 1f); //To-do: Replace with coroutine
        }
        else {
            accelerating = false;
        }*/
        

        if (drifting)
        {
            transform.rotation *= Quaternion.AngleAxis(15, Vector3.up);
        }

        rb.AddForce(moveDirection, ForceMode.Impulse);
        rb.AddForce(moveDirection, ForceMode.Acceleration);
        moveDirection = Vector3.zero; 

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * fallForce * Time.fixedDeltaTime;
        }

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }

        if (!breakdanceMode)
        {
            LookDirection();
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
            else if(accelSpeed > 10f)
            {
                accelSpeed -= 0.02f;
            }
        }

        if (accelSpeed > maxSpeed / 4f + (storedSpeed*0.1f))
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
        if (Grounded())
        {
            moveDirection += Vector3.up * jumpHeight;
            StartCoroutine(GroundCheck());
        }
        //It's time you learned how
        else
        {
            moveDirection += Vector3.up * jumpHeight * 0.055f;
            //Air "hover"
        }
    }

    private void HighJump(InputAction.CallbackContext obj)
    {
        if (Grounded() && !breakdanceMode)
        {
            moveDirection += Vector3.up * jumpHeight * 1.75f;
            moveDirection += transform.forward * jumpHeight * 2.5f;
            StartCoroutine(GroundCheck());
            Debug.Log("Go long");
        }
    }

    private void HoofIt(InputAction.CallbackContext obj)
    {
        //if (!Grounded() && !hoofCooldown)
        if(!Grounded() && hoofRecharged)
        {
            hoofRecharged = false;
            
            moveDirection += move.ReadValue<Vector2>().x * GetCameraR(playerCamera) * moveSpeed * hoofSpeed;
            moveDirection += move.ReadValue<Vector2>().y * GetCameraF(playerCamera) * moveSpeed * hoofSpeed;
            moveDirection += Vector3.down;
            rb.AddForce(moveDirection, ForceMode.Impulse);
            Debug.Log("Hoofing it: Strong");
            StartCoroutine(RechargeHoof());
            //hoofCooldown = true;
            //StartCoroutine(HoofWait(1.5f));
        }
        /*else if (Grounded() && !hoofCooldown)
        {
            moveDirection += move.ReadValue<Vector2>().x * GetCameraR(playerCamera) * moveSpeed * hoofSpeed * 0.25f;
            moveDirection += move.ReadValue<Vector2>().y * GetCameraF(playerCamera) * moveSpeed * hoofSpeed * 0.25f;
            rb.AddForce(moveDirection, ForceMode.Impulse);
            Debug.Log("Hoofing it: Weak");
            hoofCooldown = true;
            StartCoroutine(HoofWait(1f));
        } */
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
        if(punchInProgress) {
            //playerAttack.BringTheHurt();
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
            if(accelerationEnabled)
            {
                punchDuration *= 0.85f;
                punchSize *= 1.15f;
            }

            currentTime = Time.time;
            holdTime = currentTime - startTime;

            //Debug.Log("Punch held: " + holdTime);
            //Long press: Long punch
            //Short press: Short punch
            /*
            if(holdTime >= 0.4f) {    
                playerAttack.BringTheHurtII(punchDuration*1.25f, punchSize, true);
            }
            else
            {
                playerAttack.BringTheHurtII(punchDuration, punchSize, false);
            }*/
            playerAttack.BringTheHurtII(punchDuration, punchSize, false);
            
            /*
            ChangeThrottle(200*(2+holdTime));
            moveDirection += transform.forward * throttle;
            rb.AddForce(moveDirection, ForceMode.Impulse);
            rb.AddForce(moveDirection, ForceMode.Acceleration);
            */

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
        bool boxHit = Physics.BoxCast(GetComponent<Collider>().bounds.center, transform.localScale * 0.75f, Vector3.down, out objectHit, transform.rotation, boxDistance);

        if (boxHit)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void OnDrawGizmos()
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
        Gizmos.DrawWireSphere(transform.position+gizmoCubeVector, 2f);
    }

    IEnumerator GroundCheck()
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

    IEnumerator PowerToGround()
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

    /*
    IEnumerator HoofWait(float coolTime)
    {
        yield return new WaitForSeconds(0.1f);
        hoofCooldown = true;
        yield return new WaitForSeconds(coolTime);
        hoofCooldown = false;
    }
    */

    IEnumerator RechargeHoof()
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
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 GetCameraF(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraR(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            targetGet = true;
            Destroy(other.gameObject);
            Debug.Log("Target acquired");
            targetAura.gameObject.SetActive(true);
        }

        if (other.CompareTag("Finish"))
        {
            if (targetGet)
            {
                Destroy(other.gameObject);
                Debug.Log("Win");
                targetAura.gameObject.SetActive(false);
                targetGet = false;
            }
        }
    }

    

}
