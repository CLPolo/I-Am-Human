using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeel
{
    public class AnimationPlatformerScript : VisualPlatformerScriptV2
    {
        [Header("Sprite Settings")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private List<Sprite> walkSprites = new List<Sprite>(4);
        [SerializeField] private Sprite jumpSprite;
        [SerializeField] private Sprite fallSprite;

        [Header("Animation Settings")]
        [SerializeField, Range(1f, 60f), Tooltip("Reccommended values are 7.5, 15 or 30")] 
        protected float walkFramesPerSecond = 7.5f;   //How fast the walking animation will play


        private Animator animator;              //Reference to the animator component
        private SpriteRenderer spriteRenderer;  //Reference to the sprite renderer component

        private float currentFrame = 0f;

        protected override void Start()
        {
            base.Start();
            //Obtain references to the needed components
            GameObject spriteObject = transform.GetChild(0).gameObject;
            animator = spriteObject.GetComponent<Animator>();
            spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        }


        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            //Update the animation state
            UpdateAnimation();
            //animator.SetBool("isGrounded", currState == STATE.Grounded);
            //if (currState == STATE.Grounded && _horizontalInput != 0)
            //{
            //    animator.SetBool("isWalking", true);
            //}
            //else
            //{
            //    animator.SetBool("isWalking", false);
            //}
            //animator.SetFloat("yVelocity", _currentVelocity.y);

            //Update animation speed multipliers
            //animator.speed = walkAnimationSpeedMultiplier;
            //note: currently only the walk has more than one frame, will require change in logic if that change

            //Update the sprite facing direction
            if (_horizontalInput > 0)
            {
                spriteRenderer.flipX = false;
            }
            else if (_horizontalInput < 0)
            {
                spriteRenderer.flipX = true;
            }
        }

        private void UpdateAnimation()
        {
            bool isGrounded = (currState == STATE.Grounded);
            bool isWalking = (isGrounded && _horizontalInput != 0);

            currentFrame = Mathf.Repeat(currentFrame+Time.deltaTime*walkFramesPerSecond, (float)walkSprites.Count);

            if (isGrounded)
            {
                if (isWalking)
                {
                    spriteRenderer.sprite = walkSprites[Mathf.FloorToInt(currentFrame)];
                }
                else
                {
                    spriteRenderer.sprite = idleSprite;
                    currentFrame = 0.0f;
                }
            }
            else
            {
                currentFrame = 0.0f;
                if (_currentVelocity.y > 0.1)
                {
                    spriteRenderer.sprite = jumpSprite;
                }
                else if(_currentVelocity.y < -0.1)
                {
                    spriteRenderer.sprite = fallSprite;
                }
            }
        }
    }
}