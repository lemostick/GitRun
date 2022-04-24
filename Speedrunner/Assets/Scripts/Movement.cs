using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Movement : MonoBehaviour
{
    [Header("Camera reference")]
    [SerializeField] private Camera cam;
    [SerializeField] private Look cameraScript;
    [SerializeField] private float maxWallRunTilt;
    [SerializeField] private ParticleSystem speedParticle;
    //[SerializeField] private Animator camAnim;
    float fov;
    float targetFov;
    float startFov;


    [Header("Velocity")]
    [SerializeField] private TMP_Text velocityText;
    [SerializeField] private TMP_Text dashCDText;
    private Vector3 currentPos;
    private Vector3 lastPos;
    private float overallSpeed;

    [Header("Char controller and speed")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform player;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float dashCoolDown;
    [SerializeField] private float dashPower;
    public bool canHook;
    private Vector3 move;
    private float currentSpeed;
    private float x;
    private float z;
    private bool accelerating;
    private float charHeight;



    //State Control, you may ignore this part
    [HideInInspector] public bool canWalk;
    [HideInInspector] public bool hasDied;
    bool isCrouching;
    bool isStanding;
    bool isDucking;
    bool isSliding;
    bool isDashing;
    bool allowDashCD;
    float timeBeforeDash;
    Vector3 duckPos;
    Vector3 standPos;
    bool touchingCeiling;
    bool wallOnLeft;
    bool wallOnRight;
    bool shouldSlide;
    bool speedHasBeenReset;
    bool allowDoubleJump;
    Vector3 hitNormal;
    //Vector3 hitPoint;
    //Vector3 slideDir;
    float slideSpeed;



    [Header("Jumping and gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private LayerMask ceilingLayer;
    [SerializeField] private float ceilingCheckDistance;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpHeight = 3f;
    public Vector3 velocity;
    bool isGrounded;

    [Header("Wall Running")]
    [SerializeField] private Transform leftWallCheck;
    [SerializeField] private Transform rightWallCheck;
    [SerializeField] private float wallCheckRadius;
    [SerializeField] private float allowWallRunAfterJump;
    [SerializeField] private LayerMask WallLayer;
    bool allowWallRun;
    bool isWallRunning;

    [Header("Grappling Hook")]
    [SerializeField] private float hookReelingSpeed;
    [SerializeField] private float hookDistance;
    [SerializeField] private LayerMask hookLayer;
    [SerializeField] private Transform hookOrigin;
    [SerializeField] private LineRenderer lineRenderer;
    Vector3 hookPosition;
    bool isReeling;
    Vector3 hookDir;

    private void Awake()
    {
        //reseting values on start
        timeBeforeDash = 0;
        speedHasBeenReset = false;
        startFov = cam.fieldOfView;
        targetFov = startFov;
        speedParticle.Stop();
        hookPosition = Vector3.zero;
        lineRenderer.positionCount = 0;

        currentPos = transform.position;
        lastPos = transform.position;

        standPos = new Vector3(0, 0, 0);
        duckPos = new Vector3(0, -0.5f, 0);
        charHeight = controller.height;

        allowWallRun = true;

        //bug fixing
        StandUp();
    }

    // Update is called once per frame
    void Update()
    {
        //state controls
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);
        touchingCeiling = Physics.CheckSphere(ceilingCheck.position, ceilingCheckDistance, ceilingLayer);
        wallOnLeft = Physics.CheckSphere(leftWallCheck.position, wallCheckRadius, WallLayer);
        wallOnRight = Physics.CheckSphere(rightWallCheck.position, wallCheckRadius, WallLayer);

        //the same function, which didn't make it into the game. Left for the sake of study.
        //shouldSlide = Vector3.Angle(Vector3.up, hitNormal) > controller.slopeLimit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, hookDistance, hookLayer))
            canHook = true;
        else
            canHook = false;

        VelocityControl();
        AccelerationControl();
        CheckResetSpeed();
        Running();
        WallRun();
        Crouch();
        Hookshot();
        HookMovement();
        ManageFOV();

        //dash cooldown mechanism
        if (timeBeforeDash > 0 && allowDashCD)
            timeBeforeDash -= Time.deltaTime;

        //dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isCrouching)
        {
            if (timeBeforeDash <= 0)
            {
                StartCoroutine(Dash());
            }

        }

        //getting input from player to code
        if (canWalk)
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
        }

        //jump from ground input
        if (Input.GetButtonDown("Jump") && isGrounded && isStanding)
        {
            Jump();
        }

        //jump from wall input
        if (Input.GetButtonDown("Jump") && isWallRunning && isStanding)
        {
            StartCoroutine(WallRunCD());
            Jump();
        }

        //jump in mid-air input
        if (Input.GetButtonDown("Jump") && !isGrounded && isStanding)
        {
            if (allowDoubleJump)
            {
                Jump();
                isReeling = false;
                allowDoubleJump = false;
            }
        }

        
    }

    private void LateUpdate()
    {
        CheckVelocity();

        DrawLine();
    }

    //controlling movement vector and moving
    private void Running()
    {
        if (canWalk)
        {
            move = Vector3.Normalize(transform.right * x + transform.forward * z);

            if (accelerating && !isSliding)
            {
                if (currentSpeed < sprintSpeed)
                {
                    if (Mathf.RoundToInt(overallSpeed) >= 10)
                    {
                        currentSpeed += Time.deltaTime;
                    }
                    else
                    {
                        currentSpeed += 6f * Time.deltaTime;
                    }
                }
            }
            //if (!isReeling)
            
                if (isSliding)
                    controller.Move(transform.forward * currentSpeed * Time.deltaTime);
                else if (!isSliding)
                    controller.Move(move * currentSpeed * Time.deltaTime);
                else if (isWallRunning)
                    controller.Move(transform.forward * currentSpeed * Time.deltaTime);
                else if (isDashing)
                    controller.Move(transform.forward * currentSpeed);
            
        }
    }

    //checking for player wallrunning, tilting the camera, depending on state
    private void WallRun()
    {
        if (wallOnLeft && x <= -0.1f && !isGrounded && allowWallRun && overallSpeed > 3)
        {
            isWallRunning = true;
            if (Mathf.Abs(cameraScript.wallRunRotation) < maxWallRunTilt)
            {
                cameraScript.wallRunRotation -= Time.deltaTime * maxWallRunTilt * 6;
            }
        }
        else if (wallOnRight && x >= 0.1f && !isGrounded && allowWallRun && overallSpeed > 3)
        {
            isWallRunning = true;
            if (Mathf.Abs(cameraScript.wallRunRotation) < maxWallRunTilt)
            {
                cameraScript.wallRunRotation += Time.deltaTime * maxWallRunTilt * 6;
            }
        }
        else
        {
            isWallRunning = false;
            if (cameraScript.wallRunRotation > 0.1)
            {
                cameraScript.wallRunRotation -= Time.deltaTime * maxWallRunTilt * 6;
            }
            else if (cameraScript.wallRunRotation < -0.1)
            {
                cameraScript.wallRunRotation += Time.deltaTime * maxWallRunTilt * 6;
            }
            
        }
    }

    //controlling whether or not player should increase speed
    private void AccelerationControl()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            accelerating = false;
        }
        else
        {
            accelerating = true;
        }

        if (isDucking && !isSliding)
        {
            accelerating = false;
        }
    }

    //velocity controlling, and some states.
    //probably should have done everything in enum 
    //i mean the state control, but... yeah, i forgot about it
    //while i was writing it. It works though, even if its messy.
    private void VelocityControl()
    {
        controller.Move(velocity * Time.deltaTime);

        //applying bit of force on ground
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        //manipulating gravity on wall run
        if (isWallRunning)
        {
            velocity.y += (gravity / 4) * Time.deltaTime;
            allowDoubleJump = true;
        }

        //stopping the jump force if hit ceiling
        if (touchingCeiling && !isGrounded)
        {
            velocity.y = -2f;
        }

        //manipulating player via gravity
        if (!isWallRunning)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        //allowing double jump when grounded and fixing wallrun state
        if (isGrounded)
        {
            allowDoubleJump = true;
            isWallRunning = false;
        }



        //allowing Dash to cooldown
        if (isGrounded || isWallRunning)
        {
            if (allowDashCD == false)
            {
                allowDashCD = true;
            }
        }

        //velocity control apart from moving the controller
        if (velocity.z > 0.1)
        {
            velocity.z -= Time.deltaTime * 15;
        }
        else if (velocity.z < -0.1)
        {
            velocity.z += Time.deltaTime * 15;
        }
        else
        {
            velocity.z = 0;
        }

        if (velocity.x > 0.1)
        {
            velocity.x -= Time.deltaTime * 15;
        }
        else if (velocity.x < -0.1)
        {
            velocity.x += Time.deltaTime * 15;
        }
        else
        {
            velocity.x = 0;
        }

    }

    //jump function
    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        controller.slopeLimit = 100f;
    }

    //dadsh function. Made in coroutine for the sake of smoothness
    private IEnumerator Dash()
    {
        float chase = 0;
        chase = (dashPower * 10) - currentSpeed;
        currentSpeed += chase;
        isDashing = true;
        SetTargetFOV(140f);
        speedParticle.Play();

        yield return new WaitForSeconds(0.1f);

        isDashing = false;
        currentSpeed -= chase;
        timeBeforeDash = dashCoolDown;
        allowDashCD = false;
        SetTargetFOV(startFov);
        speedParticle.Stop();
    }

    //checking for slide and crouching, crouching, sliding
    private void Crouch()
    {
        if (isGrounded)
        {
            if (Input.GetKey(KeyCode.C) && overallSpeed >= 3)
            {
                if (z == 1)
                {
                    Slide();
                }
                else
                {
                    StartCrouch();
                }
            }
            else if (Input.GetKey(KeyCode.C) && overallSpeed < 3)
            {
                StartCrouch();
            }
            else if (Input.GetKeyUp(KeyCode.C) && isSliding && touchingCeiling)
            {
                StartCrouch();
            }
            else
            {
                StandUp();
            }
        }
    }

    //starting to crouch
    private void StartCrouch()
    {
        isCrouching = true;
        isSliding = false;

        if (isCrouching && isGrounded)
        {
            accelerating = false;
            //speedHasBeenReset = false;
            Duck();
        }
    }

    //sliding mechanic.
    private void Slide()
    {
        isSliding = true;

        if(isSliding && isGrounded)
        {
            Duck();
            currentSpeed -= 5 * Time.deltaTime;
        }
        else if(isSliding && isGrounded && shouldSlide)
        {
            Duck();
            currentSpeed = currentSpeed + slideSpeed;
        }
    }

    //set player state to stadinding, raising height and collider
    private void StandUp()
    {
        canWalk = true;

        if (!isStanding && !touchingCeiling)
        {
            float timeElapsed = 0;
            timeElapsed += Time.deltaTime;

            accelerating = true;
            controller.height = charHeight;
            controller.center = new Vector3(0, 0, 0);
            player.localPosition = Vector3.Lerp(standPos, duckPos, timeElapsed * 5);
            timeElapsed = 0;            
            isSliding = false;
            isStanding = true;
            isDucking = false;
            isCrouching = false;
        }
    }

    //set player state to ducking, reducing height and collider
    private void Duck()
    {
        if (!isDucking)
        {
            float timeElapsed = 0;
            timeElapsed += Time.deltaTime;

            accelerating = false;
            controller.height = charHeight / 2;
            controller.center = new Vector3(0, -0.3f, 0);
            player.localPosition = Vector3.Lerp(duckPos, standPos, timeElapsed * 5);
            timeElapsed = 0;
            isDucking = true;
            isStanding = false;
        }
    }

    //check if speed needs to be reset to 2 or 0
    private void CheckResetSpeed()
    {
        if (overallSpeed == 0 || overallSpeed <= 2)
        {
            currentSpeed = walkSpeed;
        }

        if (!speedHasBeenReset)
        {
            currentSpeed = walkSpeed;
            speedHasBeenReset = true;
            accelerating = true;
        }

        if (currentSpeed > walkSpeed && isCrouching)
        {
            currentSpeed -= 14 * Time.deltaTime;
        }
    }

    //checking current player velocity, displaying it
    private void CheckVelocity()
    {
        currentPos = transform.position;
        overallSpeed = new Vector3((currentPos.x - lastPos.x), 0, + (currentPos.z - lastPos.z)).magnitude / Time.deltaTime;
        lastPos = currentPos;
        velocityText.text = Mathf.RoundToInt(overallSpeed).ToString();
        dashCDText.text = Mathf.RoundToInt(timeBeforeDash).ToString();
    }

    private IEnumerator WallRunCD()
    {
        allowWallRun = false;

        yield return new WaitForSeconds(allowWallRunAfterJump);

        allowWallRun = true;
    }

    //a test feature which didn't make it to the game.
    //it fits, but works janky and i didn't want to waste
    //my time on this feature since i thought it won't fit
    //the actual concept of the game.
/*    private void ShouldSlideDown()
    {
        if (shouldSlide)
        {
            if (slideSpeed < 5)
                slideSpeed += Time.deltaTime;

            Vector3 c = Vector3.Cross(hitNormal, Vector3.up);
            slideDir = -Vector3.Cross(c, hitNormal);
        }
        else
        {
            slideDir = Vector3.zero;
            slideSpeed = 1;
        }    
    }*/

    //controlling the shooting of out "hook"
    private void Hookshot()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit raycastHit, hookDistance, hookLayer))
            {
                isReeling = true;
                hookPosition = raycastHit.point;
            }
            else
            {
                hookPosition = Vector3.zero;
                lineRenderer.positionCount = 0;
            }
            lineRenderer.positionCount = 2;
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            hookPosition = Vector3.zero;
            lineRenderer.positionCount = 0;
            speedParticle.Stop();
            isReeling = false;
        }
    }

    //controlling the movement during the hookshot
    private void HookMovement()
    {
        if  (isReeling)
        {
            hookDir = (hookPosition - transform.position).normalized;
            controller.Move(hookDir * hookReelingSpeed * Time.deltaTime);
            velocity = hookDir * hookReelingSpeed;
            speedParticle.Play();
            allowDoubleJump = true;

            if (Vector3.Distance(transform.position, hookPosition) <= 1.5f || Input.GetButtonDown("Jump"))
            {
                //reached hook
                speedParticle.Stop();
                hookPosition = Vector3.zero;
                lineRenderer.positionCount = 0;
                isReeling = false;
            }
        }
    }

    //Drawing hook line
    private void DrawLine()
    {
        if (hookPosition == Vector3.zero) return;

        lineRenderer.SetPosition(0, hookOrigin.position);
        lineRenderer.SetPosition(1, hookPosition);
    }

    //camera FOV control
    private void ManageFOV()
    {
        float fovspeed = 4f;
        fov = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * fovspeed);
        cam.fieldOfView = fov;
    }

    //set camera target fov
    private void SetTargetFOV(float target)
    {
        targetFov = target;
    }

    //checking of controller hit something, and if so - doing something.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
        //hitPoint = hit.point;

        if (hit.gameObject.tag == "BounceUp") // boost player upwards when stepped on jump pad
        {
            float power = hit.gameObject.GetComponent<JumpPad>().bouncePower;
            velocity.y = Mathf.Sqrt(power * -2f * gravity);
        }
        else if (hit.gameObject.tag == "DirBoost") //boost player in a direction, boost pad is facing
        {
            float power = hit.gameObject.GetComponent<JumpPad>().bouncePower;
            velocity = hit.normal * Mathf.Sqrt(power * -2f * gravity);
        }
        else if (hit.gameObject.tag == "Trap") //if player touched a trap
        {
            hasDied = true;
        }

    }

    //gizmos for spheres checking for states
    private void OnDrawGizmos()
    {
        //ground checking sphere
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckDistance);

        //ceiling check sphere
        Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckDistance);

        //right wall check sphere
        Gizmos.DrawWireSphere(rightWallCheck.position, wallCheckRadius);

        //left wall check sphere
        Gizmos.DrawWireSphere(leftWallCheck.position, wallCheckRadius);

        //hook position radius in which player drops from hook
        Gizmos.DrawWireSphere(hookPosition, 1.5f);

        //Sphere that projects range of max hook distance
        Gizmos.DrawWireSphere(transform.position, hookDistance);
    }
}