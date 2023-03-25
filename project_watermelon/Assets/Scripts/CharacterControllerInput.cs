using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class CharacterControllerInput : MonoBehaviour
{
    private CharacterController _characterController;
    private Vector3 movementInput;
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private float gravity = -10f;
    private float currentGravity = 0f;
    private Vector3 _velocity;
    private bool isGrounded = false;
    [SerializeField]
    private Transform groundChecker;
    [SerializeField]
    private float groundCheckDistance;
    [SerializeField]
    private LayerMask groundLayer;

    [Header("SFX Stuff")]
    [SerializeField] EventReference walkingSFX;
    EventInstance walkingI;
    bool walkingStarted;
    [SerializeField] EventReference breathingSFX;
    EventInstance breathingI;
    bool breathingStarted;
    private float breathingTimer;
    [SerializeField] float breathingNeed = 10;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        //Start Sounds
        walkingI = RuntimeManager.CreateInstance(walkingSFX);
        breathingI = RuntimeManager.CreateInstance(breathingSFX);
    }

    void Update()
    {
        Gravity();

        WalkingMovement();
        

        movementSFX();
    }

    private void FixedUpdate()
    {
        
    }

    private void WalkingMovement()
    {
        movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        movementInput = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * movementInput;
        _characterController.Move(movementInput * movementSpeed * Time.deltaTime);

        if (movementInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movementInput);
            targetRotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotateSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
    }

    private void Gravity()
    {
        _velocity.y = _characterController.velocity.y;

        if (_velocity.y > 0)
            _velocity.y = 0; //cancels velocity of running upstairs to avoid hops

        Debug.Log(_velocity.y);

        isGrounded = Physics.CheckSphere(groundChecker.position, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);

        _velocity.y += gravity * Time.deltaTime;

        if (isGrounded && _velocity.y < 0f)
        {
            _velocity.y = 0f;
        }

        _characterController.Move(new Vector3(0, _velocity.y, 0));      
    }

    void movementSFX()
    {
        Debug.Log(breathingTimer);
        if (movementInput.magnitude > 0 )
        {
            if (!walkingStarted)
            {
                walkingI.start();
                walkingStarted = true;
            }
            if (!breathingStarted)
            {



                breathingI.start();
                breathingStarted = true;
            }
            if(breathingTimer < 100)
            {
                breathingTimer += Time.deltaTime * breathingNeed;
                breathingI.setParameterByName("TimeWalking", breathingTimer);
            }
        } else
        {
            if (walkingStarted)
            {
                walkingI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                walkingStarted = false;
            }
            if (breathingStarted) { 
            breathingI.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            breathingStarted=false;
            }
            if (breathingTimer >= 0)
            {
                breathingTimer -= Time.deltaTime * breathingNeed * 5;
                breathingI.setParameterByName("TimeWalking", breathingTimer);
            }
        }
    }
    private void OnDisable()
    {
        walkingI.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        walkingI.release();
        breathingI.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        breathingI.release();
    }
}
