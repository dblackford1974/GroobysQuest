// KoboldController.cs
// Controls movement, actions, and animations for Kobold enemy
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoboldController : MonoBehaviour
{
    private enum SoundId 
    {
        Hop = 3,
        Kick = 4,
        Swing = 5,
        Block = 6,
        Counter = 7,
        Taunt = 8        
    }

    public float jumpImpulseHigh = 1500.0f; //Kick impulse
    public float jumpImpulseLow = 1000.0f;  //Hop impulse
    public float jumpTimeHigh = 0.72f;      //Kick total time
    public float jumpTimeLow = 0.5f;        //Hop total time
    public float swingTime = 1.0f;          //Swing club total time
    public float blockTimeHigh = 1.0f;      //Block total time
    public float blockTimeLow = 1.0f;       //Counter attack total time
    public float maxIdleTime = 5.0f;        //Idle time before taunting
    public float recoverTime = 0.5f;        //Recovery time for action
    public float swingRecoverTime = 0.3f;   //Recovery time for swing
    public float minPassTime = 0.1f;        //Minimum time for pass decision
    public float maxPassTime = 0.3f;        //Maximum time for pass decision
    public float groundDistance = 4.5f;     //Distance to ground for grounded detection
    public bool directControlMode = false;  //Set to true for direct controlled testing

    public float maxFlipSpeed = 2.0f;       //Delay between changing facing from left to right

    public WeaponTrigger swingTrigger;
    public WeaponTrigger kickTrigger;
    public WeaponTrigger counterTrigger;

    public enum ActionMode
    {
        None = -1,
        Idle = 0,
        Jump = 1,
        Kick = 2,
        Swing = 3,
        Block = 4,
        Counter = 5
    };

    public enum Command
    {
        None = 0,
        Hop = 1,
        Back = 2,
        Kick = 3,
        Swing = 4,
        Block = 5,
        Counter = 6
    }

    //Internal state
    private bool faceRight = false;
    private bool groundMode = false;
    private ActionMode actionMode = ActionMode.None;
    private Command command = Command.None;
    
    private float swingTimer = 0;
    private float swingStart = 0;
    private float actionTimer = 0;
    private float actionStart = 0;
    private float holdStart = 0;
    private float holdTime = 0;
    private float idleStartTime = 0;
    private float lastFlipTime = 0;

    private Rigidbody2D body;
    private Animator animator;
    private KoboldVigor vigor;
    private SoundPalette sounds;
    private int groundLayerMask;
    private int ladderLayerMask;

    public void Initialize(float damage, float resistance)
    {
        //Initialize Vigor
        vigor = GetComponent<KoboldVigor>();
        vigor.resistDamage = resistance;
        vigor.resistKnock = resistance;

        //Initialize weapon damage bonuses
        swingTrigger.damageBonus = damage;
        kickTrigger.damageBonus = damage;
        counterTrigger.damageBonus = damage;
    }

    //Cancel all actions, called when Kobold is hit
    public void Interrupt()
    {
        OnEndBlock();
        OnEndAttack();
        OnEndCounter();
        OnEndKick();
    }

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vigor = GetComponent<KoboldVigor>();
        sounds = GetComponent<SoundPalette>();
        groundLayerMask = 1 << LayerMask.NameToLayer("Floor");
        groundLayerMask |= 1 << LayerMask.NameToLayer("ClimbToStructure");
        ladderLayerMask = LayerMask.NameToLayer("ClimbToStructure");
        ladderLayerMask |= 1 << LayerMask.NameToLayer("Climb");
        vigor.faceRight = faceRight;
        animator.SetFloat("FacingX", -1.0f);
    }

    //Pass zone trigger to Brain
    public void OnStayTrigger(int id)
    {
        if (!directControlMode)
        {
            Brain(id);
        }
    }

     void FixedUpdate()
    {
        //Linecast to ground mode
        groundMode = Physics2D.Linecast(transform.position, transform.position + (Vector3.up * -groundDistance), layerMask:groundLayerMask);

        if (!directControlMode)
        {
            //Calculate next action
            Brain((groundMode) ? 1 : 0);
        }

        //Update current action
        UpdateActions(groundMode);
    }

    void Update()
    {
        if (directControlMode)
        {
            GetDirectControls();
        }
    }

    private void Brain(int mode)
    {
        if (holdTime != 0)
        {
            //Check if holding
            if ((Time.time - holdStart) > holdTime)
            {
                holdStart = 0;
            }
            else
            {
                //No actions while holding
                return;
            }
        }

        if (mode == 1)
        {
            //Update facing to face player
            if (GameManager.instance.player)
            {
                bool playerRight = (GameManager.instance.player.transform.position.x > transform.position.x);

                faceRight = playerRight;
            }
        }
        
        if (mode == 2)
        {
            //Hop closer to player
            if (command == Command.None)
            {
                command = Command.Hop;
            }
        }
        else if (mode == 3)
        {
            //Process within kicking distance
            float r = Random.value;

            if (r > 0.5f)
            {
                command = Command.Kick;
            }
            else if (r > 0.3f)
            {
                command = Command.Back;
            }
            else
            {
                command = Command.Counter;
            }
        }
        else if (mode == 4)
        {
            //Process for melee combat distance
            float r = Random.value;

            if (r > 0.8f)
            {
                command = Command.Swing;
            }
            else if (r > 0.64f)
            {
                command = Command.Block;
            }
            else if (r > 0.48f)
            {
                command = Command.Counter;
            }
            else if (r > 0.32f)
            {
                command = Command.Kick;
            }
            else if (r > 0.16f)
            {
                command = Command.Back;
            }
            else
            {
                holdStart = Time.time;
                holdTime = Random.Range(minPassTime, maxPassTime);
                command = Command.None;
            }
        }
        else if (mode == 5)
        {
            //Process to defend or counter flying kick attack
            float r = Random.value;

            if (r > 0.5f)
            {
                command = Command.Swing;
            }
            else if (r > 0.25f)
            {
                command = Command.Block;
            }
            else
            {
                command = Command.Back;
            }
        }
        else if (groundMode)
        {
            //If not triggered by player and standing on ladder / net exit zone, must remain in motion
            bool onLadder = Physics2D.Linecast(transform.position, transform.position + (Vector3.up * -groundDistance), layerMask:ladderLayerMask);

            if (onLadder)
            {
                float r = Random.value;

                if (r > 0.66f)
                {
                    command = Command.Hop;
                }
                else if (r > 0.33f)
                {
                    command = Command.Kick;
                }
                else
                {
                    command = Command.Back;
                }
            }
        }
    }

    //Process commands and motion
    private void SetAction(bool grounded)
    {
        bool move = false;
        bool moveRight = false;
        bool jumpHigh = false;
        bool isIdle = true;
        bool ready = ((actionMode == ActionMode.None) || (actionMode == ActionMode.Jump) || (actionMode == ActionMode.Idle));
        bool setRecovery = false;

        if (!ready)
        {
            return;
        }

        //Update states and animation parameters for current command
        if (command == Command.Hop)
        {
            move = true;
            moveRight = faceRight;
            jumpHigh = false;
        }
        else if (command == Command.Back)
        {
            move = true;
            moveRight = !faceRight;
            jumpHigh = false;
        }
        else if (command == Command.Kick)
        {
            move = true;
            moveRight = faceRight;
            jumpHigh = true;
            setRecovery = true;
        }
        else if (command == Command.Swing)
        {
            if (swingTimer == 0)
            {
                actionMode = ActionMode.Swing;
                animator.SetBool("SwingMode", true);
                swingTimer = swingTime;
                swingStart = Time.time;
                setRecovery = true;
                OnStartAttack();
            }
            isIdle = false;
        }
        else if (command == Command.Block)
        {
            if (grounded)
            {
                actionMode = ActionMode.Block;
                animator.SetBool("BlockMode", true);
                animator.SetFloat("BlockHeight", 1.0f);
                actionTimer = blockTimeHigh;
                actionStart = Time.time;
                OnStartBlock();
            }
            isIdle = false;
        }
        else if (command == Command.Counter)
        {
            if (grounded)
            {
                actionMode = ActionMode.Counter;
                animator.SetBool("BlockMode", true);
                animator.SetFloat("BlockHeight", -1.0f);
                actionTimer = blockTimeLow;
                actionStart = Time.time;
                setRecovery = true;
                OnStartCounter();
            }
            isIdle = false;
        }

        //Process ground motion
        if (move && grounded)
        {
            animator.SetBool("JumpMode", true);
            animator.SetFloat("JumpHeight", (jumpHigh) ? 1.0f : -1.0f);
            actionTimer = (jumpHigh) ? jumpTimeHigh : jumpTimeLow;
            actionStart = Time.time;
            actionMode = ActionMode.Jump;
            isIdle = false;

            Vector2 hopDir;

            //Set hop direction vectors
            if (moveRight)
            {
                hopDir = (jumpHigh) ? new Vector3(1.0f, 3.0f) : new Vector3(1.0f, 1.0f);
            }
            else
            {
                hopDir = (jumpHigh) ? new Vector3(-1.0f, 3.0f) : new Vector3(-1.0f, 1.0f);
            }

            float jumpImpulse = (jumpHigh) ? jumpImpulseHigh : jumpImpulseLow;
            
            hopDir.Normalize();
            body.AddForce(hopDir * jumpImpulse, ForceMode2D.Impulse);

            if (jumpHigh)
            {
                OnStartKick();
            }
            else
            {
                sounds.Play((int)SoundId.Hop);
            }

            isIdle = false;
        }

        //Process Taunt counter on idle
        if (isIdle)
        {
            float now = Time.time;

            if (idleStartTime == 0)
            {
                idleStartTime = now;
            }
            else if ((now - idleStartTime) > maxIdleTime)
            {
                actionMode = ActionMode.Idle;
                animator.SetBool("TauntMode", true);
                sounds.Play((int)SoundId.Taunt);
            }
        }
        else
        {
            //Set recovery time from last action
            if (setRecovery)
            {
                holdStart = Time.time;
                holdTime = (command == Command.Swing) ? swingRecoverTime : recoverTime;
            }

            idleStartTime = 0;
            animator.SetBool("TauntMode", false);
        }

        command = Command.None;
    }

    private void UpdateActions(bool groundMode)
    {
        bool lastRight = (transform.localScale.x < 0);
        float now = Time.time;

        //Handle change of facing
        if ((lastRight != faceRight) && ((now - lastFlipTime) > maxFlipSpeed))
        {
            float scaleFlip = (faceRight) ? -1.0f : 1.0f;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * scaleFlip, transform.localScale.y, transform.localScale.z);
            vigor.faceRight = faceRight;
            lastFlipTime = now;
            animator.SetFloat("FacingX", -scaleFlip);
        }

        if (actionTimer > 0.0f)
        {   
            //Handle end of action
            if ((Time.time - actionStart) > actionTimer)
            {
                if (actionMode == ActionMode.Jump)
                {
                }
                else if (actionMode == ActionMode.Kick)
                {
                    OnEndKick();
                }
                else if (actionMode == ActionMode.Block)
                {
                    OnEndBlock();
                }
                else if (actionMode == ActionMode.Counter)
                {
                    OnEndCounter();
                }

                animator.SetBool("JumpMode", false);
                animator.SetBool("BlockMode", false);

                actionMode = ActionMode.None;
                actionTimer = 0;
                actionStart = 0;
            }
        }

        if (swingTimer > 0.0f)
        {
            //Handle end of swing (can be concurrent with Jump action)
            if ((Time.time - swingStart) > swingTimer)
            {
                animator.SetBool("SwingMode", false);
                animator.SetBool("JumpMode", false);
                OnEndAttack();
                actionMode = ActionMode.None;
                swingTimer = 0;
                swingStart = 0;
            }
        }

        bool grounded = (groundMode && (actionMode != ActionMode.Jump));

        //Process commands and motion
        SetAction(grounded);
    }

    private void OnStartAttack()
    {
        swingTrigger.Enable(true, swingTime);
        sounds.Play((int)SoundId.Swing);
    }

    private void OnEndAttack()
    {
        swingTrigger.Enable(false, 0);
    }

    private void OnStartKick()
    {
        kickTrigger.Enable(true, jumpTimeHigh);
        sounds.Play((int)SoundId.Kick);
    }

    private void OnEndKick()
    {
        kickTrigger.Enable(false, 0);
    }

    private void OnStartCounter()
    {
        counterTrigger.Enable(true, blockTimeLow);
        vigor.isCounter = true;
        vigor.isBlocking = false;
        sounds.Play((int)SoundId.Counter);
    }

    private void OnEndCounter()
    {
        counterTrigger.Enable(false, 0);
        vigor.isCounter = false;
        vigor.isBlocking = false;
    }

    private void OnStartBlock()
    {
        vigor.isBlocking = true;
        vigor.isCounter = false;
        sounds.Play((int)SoundId.Block);
    }

    private void OnEndBlock()
    {
        vigor.isBlocking = false;
        vigor.isCounter = false;
    }

    //Update this method to allow manual control for testing
    void GetDirectControls()
    {
        /*
        if (!directControlMode)
        {

        }
        else if (directControlMode)
        {
            bool reverse = (Input.GetKeyDown("r"));

            if (reverse)
            {
                faceRight = !faceRight;
                Debug.Log(faceRight);
                animator.SetFloat("FacingX", (faceRight) ? 1.0f : -1.0f);
            }

            bool attack = (Input.GetKeyDown("f"));

            if ((swingTimer == 0) && (attack))
            {
                attackMode = true;
                animator.SetBool("SwingMode", true);
                swingTimer = swingTime;
                swingStart = Time.time;
            }

            if ((actionTimer == 0) && (swingTimer == 0))
            {
                float dx = Input.GetAxisRaw("Horizontal");
                float dy = Input.GetAxisRaw("Vertical");
                
                bool lastTauntMode = tauntMode;
                tauntMode = (Input.GetKey("t"));

                if (tauntMode != lastTauntMode)
                {
                    animator.SetBool("TauntMode", tauntMode);
                }

                if (!tauntMode)
                {
                    bool blockClub = (Input.GetKeyDown("e"));
                    bool blockLow = (Input.GetKeyDown("c"));

                    if (blockClub)
                    {
                        blockMode = true;
                        blockClub = true;
                        animator.SetBool("BlockMode", true);
                        animator.SetFloat("BlockHeight", 1.0f);
                        actionTimer = blockTimeHigh;
                        actionStart = Time.time;
                    }
                    else if (blockLow)
                    {
                        blockMode = true;
                        blockClub = false;
                        animator.SetBool("BlockMode", true);
                        animator.SetFloat("BlockHeight", -1.0f);
                        actionTimer = blockTimeLow;
                        actionStart = Time.time;
                    }
                    else if (dx != 0)
                    {
                        axisX = dx;
                        jumpHigh = false;
                        animator.SetFloat("JumpHeight", -1.0f);
                    }
                    else if (dy != 0)
                    {
                        axisX = dy;
                        jumpHigh = true;
                        animator.SetFloat("JumpHeight", 1.0f);
                    }
                }
            }
        }
        */
    }
}
