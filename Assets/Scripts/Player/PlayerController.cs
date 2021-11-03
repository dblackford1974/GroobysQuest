// PlayerController.cs
// Controls movement, actions, and animations for player character
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    enum SoundId
    {
        Walk = 3,
        Run = 4,
        Jump = 5,   
        Fall = 6,
        Fly = 7,
        Climb = 8,
        Kick = 9,    
        Attack = 10,  
        Roll = 11,  
        Duck = 12,  
        Idle = 13   
    };

    enum ActionMode
    {
        Idle = 0,
        Walk = 1,
        Fly = 2,
        Climb = 3,
        Attack = 4,
        Duck = 5,
        Roll = 6
    };

    //Player controller configuration
    public float jumpImpulse = 220.0f;  //Impulse force on Jump
    public float kickImpulse = 500.0f;  //Impulse force on Kick
    public float walkSpeed = 6.0f;      //Max velocity considered walk not run
    public float rollSpeed = 20.0f;     //Rolling rock velocity
    public float climbSpeed = 10.0f;    //Fixed velocity for climbing
    public float runForce = 500.0f;     //Applied force to running    
    public float runDrag = 20.0f;       //Drag proportional to velocity
    public float stopForce = 50.0f;         //Applied force to stopping
    public float flyForce = 150.0f;         //Applied force to guided falling (fly)
    public float maxFlySpeed = 30.0f;       //Max fall / fly adjustment velocity
    public float maxAttackTime = 0.7f;      //Attack action time
    public float maxDuckTime = 0.5f;        //Duck action time
    public float maxRollTime = 0.5f;        //Roll animation time
    public float maxRecoverTime = 0.25f;    //Action recovery time
    public float maxDropTime = 1.0f;        //'Drop to ladder' action time
    public float maxIdleTime = 5.0f;            //Idle time before showing Idle animation
    public float maxFallCollideSpeed = 30.0f;   //Maximum collide speed with no damage
    public float fallCollideDamage = 5.0f;      //Damage per velocity over maximum collide speed
    public float groundDistance = 3.0f;         //Distance to ground for 'groundMode' detection
    public float climbDownDistance = 6.0f;      //Distance to ground for 'drop to ladder' detection
    public float netSideDistance = 2.0f;        //Horizontal distance for 'climb sideways' detection
    public float walkToRunTime = 0.5f;          //Transition time from walk to run with held direction
    public float walkToReverseTime = 0.75f;     //Transition time to reverse facing with held direction
    public float doubleTapTime = 0.2f;          //Maximum time between presses to count as 'double tap'
    public float facingOffset = 10.0f;          //Ideal camera offset from player in directon of facing
    public float climbOffset = 5.0f;            //Ideal camera vertical offset when climbing
    public float gravityScale = 2.0f;           //Scale of gravity (when not climbing)
    public float slowOnLandKick = 0.3f;         //Velocity slow factor when landing from a kick

    public Collider2D standCollider;
    public Collider2D rollCollider;
    public WeaponTrigger swingTrigger;
    public WeaponTrigger rollTrigger;
    public WeaponTrigger kickTrigger;
    public PlayerHitBox hitBox;
    public ClimbBox climbBox;
    
    //Internal state
    private bool faceRight = true;
    private bool runMode = false;
    private bool groundMode = false;
    private bool kickMode = false;
    private bool dropMode = false;
    private bool recoverDuck = false;
    private bool fixClimbX = false;
    private bool fixClimbEnd = false;
    private bool forceDuck = false;

    private ActionMode actionMode = ActionMode.Walk;
    private ActionMode recoverMode = ActionMode.Idle;

    private float walkToRunTotal = 0;
    private float lastDx;
    private float lastDy;
    private float lastVx;

    //Timing
    private bool lastTapRight = false;
    private float lastTapTime = 0;
    private float actionStartTime = 0;
    private float recoverStartTime = 0;
    private float dropStartTime = 0;
    private float idleStartTime = 0;

    //Buttons pressed
    private bool buttonAttack = false;
    private bool buttonRoll = false;
    private bool buttonFlip = false;

    //Members
    private Vector2 axis;
    private Rigidbody2D body;
    private Animator animator;
    private SoundPalette sounds;
    private PlayerVigor vigor;
    private new CameraController camera;
    private int groundLayer;
    private int groundToClimbLayer;
    private int groundLayerMask;
    private int climbLayer;
    private int climbLayerMask;
    private int playerLayer;
    private int playerToClimbLayer;

    //Velocity wiggle room to be considered as 'stopped'
    private const float stoppedVelocityY = 0.05f;
    private const float stoppedVelocityX = 0.01f;

    public void HealFull()
    {
        vigor.HealFull();
    }

    public void Initialize(float damage, float resistance)
    {
        //Initialize Vigor
        vigor = GetComponent<PlayerVigor>();
        vigor.resistDamage = resistance;
        vigor.resistKnock = resistance;

        //Initialize weapon damage bonuses
        swingTrigger.damageBonus = damage;
        rollTrigger.damageBonus = damage;
        kickTrigger.damageBonus = damage;
    }

    //Cancel all actions, called when player is hit
    public void Interrupt()
    {
        //Linecast to ground mode
        groundMode = Physics2D.Linecast(transform.position, transform.position + (Vector3.up * -groundDistance), layerMask:groundLayerMask);

        if (groundMode)
        {
            SetActionMode(ActionMode.Walk);
        }
        else
        {
            SetActionMode(ActionMode.Fly);
        }

        gameObject.layer = playerLayer;
        body.gravityScale = gravityScale;
        climbBox.SetNetFix(false);

        actionStartTime = 0;
        buttonAttack = false;
        buttonRoll = false;

        OnEndAttack();
        OnEndRoll();
        OnEndKick();
        OnEndDuck();
    }

    //Call to change climb mode and 'drop to ladder' mode
    public void SetClimbMode(bool climb, bool drop)
    {
        if (climb)
        {
            SetActionMode(ActionMode.Climb);
            gameObject.layer = playerToClimbLayer;
            
            if (drop)
            {
                //Zero horizontal motion and set drop mode
                body.velocity = new Vector2(0, body.velocity.y);
                dropMode = true;
                dropStartTime = Time.time;
            }
            else
            {
                //Suspend gravity and velocity while climbing
                body.gravityScale = 0;
                fixClimbX = true;
                body.velocity = Vector2.zero;
                dropMode = false;
            }
        }
        else
        {
            if (actionMode == ActionMode.Climb)
            {
                //Restore gravity and set to 'fly' mode
                //  'ground' mode will be restored when detected
                SetActionMode(ActionMode.Fly);
                body.gravityScale = gravityScale;
                fixClimbEnd = true;
                axis.x = 0;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D hit)
    {
        if (kickMode)
        {
            //Retract kick upon collision
            animator.SetFloat("ExtendKick", -1.0f);

            body.velocity = new Vector2(body.velocity.x * slowOnLandKick, body.velocity.y);
            kickMode = false;
            OnEndKick();
        }
        else if ((hit.gameObject.layer == groundLayer) || (hit.gameObject.layer == groundToClimbLayer))
        {
            //Process fall damage
            float hitGround = (hit.relativeVelocity.y - maxFallCollideSpeed);

            if (hitGround > 0)
            {
                vigor.OnHit(hitGround * fallCollideDamage, 0, Vigor.AttackType.None, 0);
                forceDuck = true;
            }
        }
    }

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sounds = GetComponent<SoundPalette>();
        groundLayer = LayerMask.NameToLayer("Floor");
        groundToClimbLayer = LayerMask.NameToLayer("ClimbToStructure");
        groundLayerMask = (1 << groundLayer) | (1 << groundToClimbLayer);
        climbLayer = LayerMask.NameToLayer("Climb");
        climbLayerMask = 1 << climbLayer;
        playerLayer = LayerMask.NameToLayer("Player");
        playerToClimbLayer = LayerMask.NameToLayer("PlayerDropToClimb");
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        camera = mainCamera.GetComponent<CameraController>();
        camera.SetTargetX(facingOffset);
        vigor.faceRight = faceRight;
        rollCollider.enabled = false;
        body.gravityScale = gravityScale;

        camera.test++;
        

        camera.OnStart(this);
    }

    void Update()
    {
        //Quit on escape
        bool quit = Input.GetKey(KeyCode.Escape);

        if (quit)
        {
            GameManager.instance.QuitLevel();
        }

        //Get movement keys
        float dy = Input.GetAxisRaw("Vertical");
        float dx = Input.GetAxisRaw("Horizontal");
        
        axis = new Vector2(dx, dy);

        //Actions must be separated by recovery time
        if (recoverStartTime != 0)
        {
            if ((Time.time - recoverStartTime) > maxRecoverTime)
            {
                recoverStartTime = 0;
            }
        }

        //Get action buttons
        bool recoverAttack = ((recoverMode == ActionMode.Attack) && (recoverStartTime != 0));
        bool recoverRoll = ((recoverMode == ActionMode.Roll) && (recoverStartTime != 0));
        recoverDuck = ((recoverMode == ActionMode.Duck) && (recoverStartTime != 0));

        if (!buttonAttack && !recoverAttack) buttonAttack = Input.GetButton("Fire1");
        if (!buttonRoll && !recoverRoll) buttonRoll = Input.GetButton("Fire2");
        if (!buttonFlip) buttonFlip = Input.GetButtonDown("Fire3");

        if (buttonAttack)
        {
            //Can't attack while falling to damage
            if (Mathf.Abs(body.velocity.y) > maxFallCollideSpeed)
            {
                buttonAttack = false;
            }
        }
    }

    void FixedUpdate()
    {
        float dx = axis.x;
        float dy = axis.y;

        float vx = body.velocity.x;
        float vy = body.velocity.y;

        bool climbMode = (actionMode == ActionMode.Climb);
        bool isAction = (actionMode >= ActionMode.Attack);
        bool isIdle = true;

        if (dropMode)
        {
            if ((Time.time - dropStartTime) > maxDropTime)
            {
                //Abort drop
                Interrupt();
                dropMode = false;
            }
            else
            {
                dx = dy = 0;
            }
        }

        //Linecast to ground mode
        groundMode = Physics2D.Linecast(transform.position, transform.position + (Vector3.up * -groundDistance), layerMask:groundLayerMask);

        if (groundMode && !climbMode)
        {
            if (isAction)
            {
                //Update action in progress
                FixedUpdateAction(body, dx, dy, ref vx, vy);
                isIdle = false;
            }

            if (!isAction)
            {
                if (dy < 0)
                {
                    //Linecast to climb mode (if push down)
                    bool canClimbDown = Physics2D.Linecast(transform.position, transform.position + (Vector3.up * -climbDownDistance), layerMask:climbLayerMask);

                    if (canClimbDown)
                    {
                        SetClimbMode(true, true);
                        climbMode = true;
                    }
                }

                if (!climbMode)
                {
                    //Not climbing, update ground motion
                    UpdateTap(dx);
                    FixedUpdateGround(body, dx, dy, vx, vy, ref isIdle);
                }
            }
        }
        else if (climbMode)
        {
            //Update for climbing
            FixedUpdateClimb(body, dx, dy, vx, vy);
            isIdle = false;
        }
        else
        {
            //Update for falling / flying
            FixedUpdateFly(body, dx, dy, vx, vy);
            isIdle = false;
        }

        lastDx = dx;
        lastDy = dy;
        lastVx = vx;

        if (actionMode != ActionMode.Idle)
        {
            //Update player facing
            bool lastRight = (transform.localScale.x > 0);
            
            if (lastRight != faceRight)
            {
                float scaleFlip = (faceRight) ? 1.0f : -1.0f;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * scaleFlip, transform.localScale.y, transform.localScale.z);
                vigor.faceRight = faceRight;
                isIdle = false;
            }
        }
        else
        {
            //On idle, force facing to the right to prevent mirror image of front facing idle animation
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (isIdle)
        {
            //Switch to Idle animation if Idle for too long
            float now = Time.time;

            if (idleStartTime == 0)
            {
                idleStartTime = now;
            }
            else if ((now - idleStartTime) > maxIdleTime)
            {
                SetActionMode(ActionMode.Idle);
                sounds.Play((int)SoundId.Idle);
                camera.SetTargetX(0);
            }
        }
        else
        {
            idleStartTime = 0;
        }

        UpdateAnimator();
        UpdateLoopedAudio();

        buttonAttack = false;
        buttonRoll = false;
        buttonFlip = false;
    }

    //Centers player on the ladder or net segment when starting a climb
    private void FixClimbX()
    {
        fixClimbX = false;
        Vector2 pos = body.position;

        //Snap to grid location for center ladder
        int x = (int)((pos.x + 0) / 4);
        pos.x = x * 4 + 2;
        body.MovePosition(pos);
        climbBox.SetNetFix(true);
    }

    //Restores player layer with required fix ups upon ending a climb
    private void FixClimbEnd()
    {
        gameObject.layer = playerLayer;
        climbBox.SetNetFix(false);
        body.velocity = new Vector2(0, 0);
    }

    //Update animator parameters for horizontal facing and velocity
    private void UpdateAnimator()
    {
        float facingX = ((faceRight) ? 1.0f : -1.0f);
        float velocityX = ((lastDx == 0) ? -1.0f : (runMode) ? 1.0f : 0.0f);

        animator.SetFloat("FacingX", facingX);
        animator.SetFloat("VelocityX", velocityX);
    }

    //Update looped audio from player state
    private void UpdateLoopedAudio()
    {
        if (actionMode == ActionMode.Walk)
        {
            if (lastDx == 0f)
            {
                sounds.Play(-1);
            }
            else if (!runMode)
            {
                sounds.Play((int)SoundId.Walk);
            }
            else
            {
                sounds.Play((int)SoundId.Run);
            }            
        }
        else if ((actionMode == ActionMode.Fly) || (actionMode == ActionMode.Roll))
        {
            if (lastDy <= 0f)
            {
                sounds.Play((int)SoundId.Fall);
            }
            else
            {
                sounds.Play((int)SoundId.Fly);
            }
        }
        else if (actionMode == ActionMode.Climb)
        {
            if ((lastDx != 0) || (lastDy != 0f))
            {
                sounds.Play((int)SoundId.Climb);
            }
            else
            {
                sounds.Play((int)-1);
            }
        }
    }

    //Fixed Update while Climbing
    private void FixedUpdateClimb(Rigidbody2D body, float dx, float dy, float vx, float vy)
    {
        Vector2 dp = Vector2.zero;
        bool climbing = false;

        //Vertical motion
        if (dy != 0)
        {
            dp.y = dy;
            climbing = true;
            camera.SetTargetY(dy * climbOffset);
        }

        //Horizontal motion
        if (dx != 0)
        {
            Vector2 testDelta = Vector2.right * ((dx > 0) ? netSideDistance : -netSideDistance);
            
            //Look for adjoined net section
            bool canMove = Physics2D.Linecast(body.position + testDelta, body.position + testDelta + testDelta, layerMask:climbLayerMask);

            if (canMove) 
            {
                dp.x = dx;
                climbing = true;
            }
        }

        if (dropMode)
        {
            //Drop to ladder
            animator.SetFloat("VelocityY", -1.0f);
            camera.SetTargetY(-climbOffset);
        }
        else if (!fixClimbX)
        {
            //Manually move based on climb velocity
            body.MovePosition(body.position + (dp * climbSpeed * Time.fixedDeltaTime));

            float animateY = (climbing) ? 1.0f : 0;
            animator.SetFloat("VelocityY", animateY);
        }
        else
        {
            //Implement fixup when first grabbing ladder or net
            FixClimbX();
            animator.SetFloat("VelocityY", 0);
        }
    }

    //Detect 'double tap' to roll, or direct button push
    private void UpdateTap(float dx)
    {
        bool setRollMode = false;

        if (buttonRoll)
        {
            //Direct button push
            buttonRoll = false;
            setRollMode = true;
        }
        else if ((lastDx == 0) && (dx != 0))
        {
            //Detect double tap
            float tapTime = Time.time;
            bool tapRight = (dx > 0);

            if ((tapRight == lastTapRight) && ((tapTime - lastTapTime) < doubleTapTime))
            {
                if (tapRight == faceRight)
                {
                    setRollMode = true;
                }

                lastTapTime = 0;
            }
            else
            {
                lastTapRight = tapRight;
                lastTapTime = Time.time;
            }
        }

        if (setRollMode)
        {
            //Start rolling rock maneuver
            SetActionMode(ActionMode.Roll);
            OnStartRoll();
        }
    }

    //Update the action mode
    private void SetActionMode(ActionMode setMode)
    {
        if (actionMode != setMode)
        {
            actionMode = setMode;

            //Initialize action timer
            if (actionMode >= ActionMode.Attack)
            {
                actionStartTime = Time.time;
            }

            animator.SetInteger("ActionMode", (int)actionMode);

            bool rollMode = ((actionMode == ActionMode.Roll) || (actionMode == ActionMode.Duck));

            //Update colliders
            standCollider.enabled = !rollMode;
            rollCollider.enabled = rollMode;            
        }
    }

    //Fixed Update when falling / flying
    private void FixedUpdateFly(Rigidbody2D body, float dx, float dy, float vx, float vy)
    {
        float avx = Mathf.Abs(vx);
        float avy = Mathf.Abs(vy);

        buttonRoll = false;

        if (fixClimbEnd)
        {
            //Fixup when exiting ladder
            FixClimbEnd();
            fixClimbEnd = false;
        }

        //Camera updates for fast falling
        if (avy > 15.0f)
        {
            float vv = avy - 15.0f;
            float lv = vv / 10.0f;
            
            float xv = Mathf.Lerp((faceRight) ? facingOffset : -facingOffset, 0, lv);

            camera.SetTargetX(xv);
            camera.SetTargetY((vy - 10.0f) * 0.15f);
        }
        else
        {
            camera.SetTargetX((faceRight) ? facingOffset : -facingOffset);
            camera.SetTargetY(0);
        }

        //Initialize fly / fall action
        if ((actionMode != ActionMode.Fly) && (actionMode != ActionMode.Roll))
        {
            SetActionMode(ActionMode.Fly);
            animator.SetFloat("ExtendKick", -1.0f);
            kickMode = false;
        }

        //Retract kick if free falling
        bool freeFall = (Mathf.Abs(body.velocity.y) > maxFallCollideSpeed);

        if (freeFall && kickMode)
        {
            animator.SetFloat("ExtendKick", -1.0f);
            kickMode = false;
        }

        //Process flying kick request
        if (buttonAttack && !kickMode && !freeFall)
        {
            buttonAttack = false;
            kickMode = true;
            animator.SetFloat("ExtendKick", 1.0f);
            OnStartKick();

            Vector3 kickDir;

            if (faceRight)
            {
                kickDir = new Vector3(2.0f, -1.0f, 0);
            }
            else
            {
                kickDir = new Vector3(-2.0f, -1.0f, 0);
            }
            
            kickDir.Normalize();
            body.AddForce(kickDir * kickImpulse, ForceMode2D.Impulse);
        }
        else
        {
            //Allow vertical and horizontal maneuvering while falling
            float forceFlyX = dx * flyForce;
            float forceFlyY = dy * flyForce;

            if (dx != 0)
            {
                faceRight = (dx > 0);
            }

            if ((avx < maxFlySpeed) || ((vx > 0) != (dx > 0)))
                body.AddForce(transform.right * forceFlyX);
            
            if ((avy < maxFlySpeed) || ((vy > 0) != (dy > 0)))
                body.AddForce(transform.up * forceFlyY);
        }
    }

    //Update player action in progress
    private void FixedUpdateAction(Rigidbody2D body, float dx, float dy, ref float vx, float vy)
    {
        float now = Time.time;
        float elapse = now - actionStartTime;
        float maxActionTime = 0;

        if (actionMode == ActionMode.Roll)
        {        
            float dir = (faceRight) ? 1.0f : -1.0f;
            vx = dir * rollSpeed;
            body.velocity = new Vector3(vx, vy, 0);
            maxActionTime = maxRollTime;
        }
        else if (actionMode == ActionMode.Attack)
        {
            maxActionTime = maxAttackTime;
        }
        else if (actionMode == ActionMode.Duck)
        {
            maxActionTime = maxDuckTime;
        }

        if (elapse > maxActionTime)
        {
            if (actionMode == ActionMode.Roll)
            {
                OnEndRoll();        
            }
            else if (actionMode == ActionMode.Attack)
            {
                OnEndAttack();
            }
            else if (actionMode == ActionMode.Duck)
            {
                OnEndDuck();
            }

            recoverMode = actionMode;
            recoverStartTime = now;

            if (groundMode)
            {
                SetActionMode(ActionMode.Walk);
            }
            else
            {
                SetActionMode(ActionMode.Fly);
            }
        }
    }

    //Fixed update while on ground
    private void FixedUpdateGround(Rigidbody2D body, float dx, float dy, float vx, float vy, ref bool isIdle)
    {
        float avx = Mathf.Abs(vx);
        float avy = Mathf.Abs(vy);

        //Update camera target for walking
        camera.SetTargetX((faceRight) ? facingOffset : -facingOffset);
        camera.SetTargetY(0);

        if ((actionMode != ActionMode.Walk) && (actionMode != ActionMode.Roll))
        {
            if ((actionMode == ActionMode.Idle) && (dx != 0))
            {
                //Break out of Idle mode
                faceRight = (dx > 0);
                SetActionMode(ActionMode.Walk);
            }
            else if (actionMode != ActionMode.Idle)
            {
                //Transition fron non-Idle to Walk action
                SetActionMode(ActionMode.Walk);
            }
        }
        else if (buttonFlip)
        {
            //Process flip facing
            buttonFlip = false;
            faceRight = !faceRight;
        }

        //'forceDuck' forces duck after taking damage from long fall
        if (forceDuck)
        {
            //If force duck, set dy to 'down' direction
            dy = -1.0f;
        }

        //Jump and duck
        if (dy != 0)
        {
            //Jump not allowed if significant vertical velocity
            if ((avy < stoppedVelocityY) || forceDuck)
            {
                if (dy > 0)
                {
                    //Add force for upward jump
                    body.AddForce(Vector3.up * dy * jumpImpulse, ForceMode2D.Impulse);
                    sounds.Play((int)SoundId.Jump);
                }
                else if (!recoverDuck)
                {
                    //Set Duck mode
                    SetActionMode(ActionMode.Duck);
                    OnStartDuck();
                    dx = 0;
                }

                forceDuck = false;
            }

            isIdle = false;
        }

        //Attack
        if (buttonAttack)
        {
            SetActionMode(ActionMode.Attack);
            OnStartAttack();
            isIdle = false;
        }

        if (dx != 0)
        {
            isIdle = false;

            if (!runMode)
            {
                //Update timer to start running when a direction is held
                if ((lastDx > 0) == (dx > 0))
                {
                    walkToRunTotal += Time.fixedDeltaTime;
                }
                else
                {
                    walkToRunTotal = 0;
                } 

                if (walkToRunTotal > (((dx > 0) == faceRight) ? walkToRunTime : walkToReverseTime))
                {
                    //Transition from walk to run
                    runMode = true;
                    faceRight = (dx > 0);
                    walkToRunTotal = 0;
                }
            }
            else if ((avx > stoppedVelocityX) && ((vx > 0) != faceRight) && ((vx > 0) == (lastVx > 0)))
            {
                //If running then changed direction, now slowed enough to reverse
                faceRight = !faceRight;
            }

            float forceRun = dx * runForce;

            if (runMode && (avx >= 0))
            {
                if ((dx > 0) == (vx > 0))
                {
                    //Running in direction of velocity, apply proportional drag
                    float forceDrag = -vx * runDrag;

                    if (Mathf.Abs(forceDrag) < Mathf.Abs(forceRun))
                    {
                        body.AddForce(transform.right * (forceRun + forceDrag), ForceMode2D.Force);
                    }
                    else
                    {
                        Debug.Log("Max run");
                    }
                }
                else
                {
                    //Apply slowing force in reverse direction
                    body.AddForce(transform.right * forceRun, ForceMode2D.Force);
                }
            }
            else
            {
                //Walk at constant speed
                body.velocity = new Vector3(walkSpeed * dx, body.velocity.y, 0);
            }
        }
        else
        {
            //Player is not moving, slide to stop if running
            walkToRunTotal = 0;

            if (avx > walkSpeed)
            {
                //Apply stopping force while velocity greater than walk speed
                float forceStop = -vx * stopForce;

                body.AddForce(transform.right * forceStop);
            }
            else
            {
                //If at walking speed, immediately stop
                runMode = false;
                body.velocity = new Vector3(0, body.velocity.y, 0);
            }
        }
    }

    private void OnStartAttack()
    {
        swingTrigger.Enable(true, maxAttackTime);
        sounds.Play((int)SoundId.Attack);
    }

    private void OnEndAttack()
    {
        swingTrigger.Enable(false, 0);
    }

    private void OnStartRoll()
    {
        rollTrigger.Enable(true, maxRollTime);
        vigor.isRolling = true;
        hitBox.SetDuck(true);
        sounds.Play((int)SoundId.Roll);
    }

    private void OnEndRoll()
    {
        rollTrigger.Enable(false, 0);
        hitBox.SetDuck(false);
        vigor.isRolling = false;
    }

    private void OnStartKick()
    {
        kickTrigger.Enable(true, 0);
        sounds.Play((int)SoundId.Kick);
    }

    private void OnEndKick()
    {
        kickTrigger.Enable(false, 0);
    }

    private void OnStartDuck()
    {
        sounds.Play((int)SoundId.Duck);
        hitBox.SetDuck(true);
        vigor.isDucking = true;
    }

    private void OnEndDuck()
    {
        hitBox.SetDuck(false);
        vigor.isDucking = false;
    }
}
