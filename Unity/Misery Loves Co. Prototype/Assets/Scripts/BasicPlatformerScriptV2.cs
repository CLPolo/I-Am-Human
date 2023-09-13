using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFeel
{
    public class BasicPlatformerScriptV2 : MonoBehaviour
    {
        private const float VELOCITY_SCALING = 100;
        private float GROUND_CHECK_DEPTH = 0.7f;
        
        [Header("Input Bindings")]
        [SerializeField] protected KeyCode left = KeyCode.LeftArrow;
        [SerializeField] protected KeyCode right = KeyCode.RightArrow;
        [SerializeField] protected KeyCode jump = KeyCode.UpArrow;
        [SerializeField] protected KeyCode reset = KeyCode.R;

        [Header("Physics Settings")]
        [SerializeField] protected float gravity = 9.8f;
        [SerializeField] protected float mass = 10;
        [SerializeField] protected float groundBuffer = 0.08f;
        [SerializeField,Range(0,1)] protected float groundFriction = 0.139f;
        [SerializeField,Range(0,1)] protected float airFriction = 0.279f;
        [SerializeField] protected float terminalVelocity = 20;

        [Header("Vertical Movement Settings")]
        [SerializeField] protected float jumpStrength = 16.21f;
        [SerializeField] protected float jumpTime = 0.5f;
        [SerializeField] protected AnimationCurve jumpCurve;
        [SerializeField] protected float hangTime = 0.2f;
        [SerializeField,Range(1,2)] protected int jumpCount = 1;
        
        [Header("Horizontal Movement Settings")]
        [SerializeField] protected float horizontalAcceleration = 10;
        [SerializeField] protected float maxHorizontalVelocity = 20;

        public enum STATE {Falling, Rising, Hanging, Grounded};
        private STATE _currState = STATE.Falling;
        [HideInInspector] public STATE currState
        {
            get => _currState;
            protected set
            {
                if (_currState == value)
                {
                    return;
                }
                _currState = value;
                switch (_currState)
                {
                    case STATE.Grounded: OnGrounded_Hook();
                        break;
                    case STATE.Rising: OnRising_Hook();
                        break;
                    case STATE.Hanging: OnHanging_Hook();
                        break;
                    case STATE.Falling: OnFalling_Hook();
                        break;
                }
            }
        }
        //Jump hooks
        protected virtual void OnGrounded_Hook(){}
        protected virtual void OnRising_Hook(){}
        protected virtual void OnHanging_Hook(){}
        protected virtual void OnFalling_Hook(){}
        protected virtual void OnJump_Hook(){}
        
        protected BoxCollider2D _playerCollider;
        private Rigidbody2D _rigidbody2D;
        protected float _horizontalInput = 0;
        protected Vector2 _currentVelocity = Vector2.zero;
        private Coroutine _jumpRoutine;
        protected int jumpCounter = 0;

        #region Unity LifeCycle
            protected virtual void Start()
            {
                _playerCollider = GetComponent<BoxCollider2D>();
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }
            
            protected virtual void Update()
            {
                //Input parsing
                _horizontalInput = 0;
                if (Input.GetKeyDown(jump) && jumpCounter < jumpCount)
                {
                    if (_jumpRoutine == null)
                    {
                        jumpCounter += 1;
                        _jumpRoutine = StartCoroutine(_Jump());
                    }
                    else if (jumpCounter < jumpCount)
                    {
                        jumpCounter += 1;
                        StopCoroutine(_jumpRoutine);
                        _jumpRoutine = StartCoroutine(_Jump());
                    }
                }
                if (Input.GetKey(right))
                {
                    _horizontalInput += 1;
                }
                if (Input.GetKey(left))
                {
                    _horizontalInput -= 1;
                }
                
                //all the physics calc short of actually moving happen in here.
                _CheckGrounded();
                _CalculateHorizontalMovement();
                _ApplyFriction();
                _ApplyGravity();
                _ClampVelocities();

                if (Input.GetKey(reset)){
                    resetLevel();
                }
            }

            protected virtual  void FixedUpdate()
            {
                //actually move in fixed update to avoid kinematic body glitches
                _TryMove();
            }
        #endregion
        
        
        /// <summary>
        /// the only time we need collision checks is while going down so we use a raycast to check if the displacement
        /// of our movement is going to put us inside an obstacle, if yest it adjusts the displacement to only go so far
        /// that it dosnt clip.
        /// </summary>
        private void _TryMove()
        {
            Vector2 position = transform.position;
            Vector2 newPosition = (Vector2)position + _currentVelocity;
            Vector2 extents = _playerCollider.bounds.extents;
            
            if (_currentVelocity.y < 0)
            {
                //only check if going down.
                Vector2 rayPosition = new Vector2(position.x, position.y - extents.y);
                RaycastHit2D checkValid = Physics2D.Raycast(rayPosition, Vector2.down);
                if (checkValid.collider)
                {
                    //we only need to do adjust if were actually about to hit something
                    float distanceFromBase = checkValid.distance;
                    if ( distanceFromBase <= groundBuffer && distanceFromBase>0)
                    {
                        //if the distance from our position to the obstacle is less than the ground buffer then we've
                        //landed.
                        currState = STATE.Grounded;
                        jumpCounter = 0;
                        _currentVelocity = new Vector2(_currentVelocity.x, 0);
                        newPosition = (Vector2)position;
                    }
                    else if (distanceFromBase < Mathf.Abs(_currentVelocity.y)+groundBuffer && distanceFromBase>0 )
                    {
                        //otherwise adjust the velocity; dont kill it so we can get as close as possible
                        newPosition = new Vector2(
                            newPosition.x,
                            position.y - distanceFromBase + groundBuffer
                            );
                    }
                }
            }
            //now we can safely update
            _rigidbody2D.MovePosition(newPosition);
        }
        
        
        /// <summary>
        /// Use a ray cast to check if we walked off a ledge or if the floor got deleted. Seperated from the other raycast
        /// to avoid glitches because we only need this when we are grounded. Jump is handled with no physics
        /// </summary>
        private void _CheckGrounded()
        {
            Vector2 position = transform.position;
            Vector2 extents = _playerCollider.bounds.extents;
            Vector2 rayPosition = new Vector2(position.x, position.y - extents.y);
            
            if (currState == STATE.Grounded)
            {
                Debug.DrawRay(rayPosition,Vector2.down*(GROUND_CHECK_DEPTH));
                RaycastHit2D checkValid = Physics2D.Raycast(rayPosition, Vector2.down,GROUND_CHECK_DEPTH);
                if (!checkValid.collider)
                {
                    currState = STATE.Falling;
                }
                else if(checkValid.distance == 0f){
                    currState = STATE.Falling;
                }
            }
        }
        
        
        /// <summary>
        /// Our physics free routine for jump. Handling the upward motion in a physical manner caused way too many inconsistencies
        /// so I just gave up and move the stuff based on a max height which is the jump strength. This also handles hang time.
        /// </summary>
        private IEnumerator _Jump()
        {
            float timer = 0;
            float oldHeight = 0;
            currState = STATE.Rising;
            OnJump_Hook();
            while (timer <= jumpTime)
            {
                timer += Time.deltaTime;
                float curveValue = jumpCurve.Evaluate(Mathf.Clamp(timer / jumpTime, 0, 1));
                float newHeight = Mathf.Lerp(0,jumpStrength,curveValue);
                float velocity = (newHeight - oldHeight);
                _currentVelocity = new Vector2(_currentVelocity.x, velocity);
                oldHeight = newHeight; 
                yield return new WaitForFixedUpdate();
            }
            timer = 0;
            _currentVelocity = new Vector2(_currentVelocity.x, 0);
            currState = STATE.Hanging;
            
            while (timer<=hangTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _jumpRoutine = null;
            currState = STATE.Falling;
        }
        
        
        /// <summary>
        /// Simple horizontal movement calculation.
        /// </summary>
        private void _CalculateHorizontalMovement()
        {
            _currentVelocity += new Vector2(_horizontalInput, 0) * ((horizontalAcceleration) * 0.5f * Time.deltaTime);
        }
        
        
        /// <summary>
        /// These two functions calculate friction and gravity based on current velocity. There both not physically accurate
        /// but i think they look believable enough
        /// </summary>
        private void _ApplyFriction()
        {
            if (Mathf.Abs(_currentVelocity.x) < 0.001f) _currentVelocity = new Vector2(0, _currentVelocity.y);
            _currentVelocity -= new Vector2(
                _currentVelocity.x * ((currState == STATE.Grounded) ? groundFriction : airFriction),
                0);
        }
        private void _ApplyGravity()
        {
            if (currState == STATE.Falling)
            {
                _currentVelocity += new Vector2(0, -gravity*mass) * (0.5f * Mathf.Pow(Time.deltaTime,2));
            }
        }
        
        
        /// <summary>
        /// Clamp velocities to abide by terminal and max horizontal velocity. Jump dosnt care about clamping since its
        /// not physics based.
        /// </summary>
        private void _ClampVelocities()
        {
            float adjustedMaxHorizontal = maxHorizontalVelocity / VELOCITY_SCALING;
            float adjustedTerminal = terminalVelocity / VELOCITY_SCALING;
            float clampedHorizontal = Mathf.Clamp(_currentVelocity.x, -adjustedMaxHorizontal, adjustedMaxHorizontal);
            float clampedVertical = Mathf.Clamp(_currentVelocity.y, -adjustedTerminal, adjustedTerminal);
            _currentVelocity = new Vector2(
                clampedHorizontal,
                (currState == STATE.Falling)?clampedVertical:_currentVelocity.y
                );
        }
        
        
        //made this but ended up not using it ill get rid of it after im done with visual script or maybe move it there
        public static int SignFloat(float val)
        {
            if (val > 0) return 1;
            if (val == 0) return 0;
            else return -1;
        }

        // helper for resetting the level easily so I can call this in the level design lab
        public void resetLevel(){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}