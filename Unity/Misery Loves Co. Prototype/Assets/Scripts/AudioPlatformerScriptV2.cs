using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeel{
    public class AudioPlatformerScriptV2 : AnimationPlatformerScript
    {
        // Audio Stuff
        [Header("Audio Settings")]
        public AudioSource audioSourceAction; // controls singly played audio events: jump and impact
        public AudioSource audioSourceMove; // controls looping audio events: ground and air movement, falling, rising, etc

        [Header("Action Clips")]
        public AudioClip jumpClip; // plays when player jumps
        public AudioClip doubleJumpClip; // plays when player double jumps
        public AudioClip impactClip; // plays when player impacts ground

        [Header("Looping Clips")]
        public AudioClip groundMoveClip; // plays on loop when moving on ground
        public AudioClip airRisingClip; // plays on loop when rising in air
        public AudioClip airFallingClip; // plays on loop when falling in air

        private float _loopThreshold = 0.01f;
        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            if (audioSourceMove == null) { return; }

            // if we are moving, handle the looping sound logic
            if (groundLoop() || _currentVelocity.y != 0){
                bool play = false;
                // switch based on state & status of each clip
                if (currState == STATE.Grounded && groundMoveClip != null){
                    audioSourceMove.clip = groundMoveClip;
                    play = true;
                }
                else if (currState == STATE.Rising && airRisingClip != null){
                    audioSourceMove.clip = airRisingClip;
                    play = true;
                }
                else if (currState == STATE.Falling && airFallingClip != null){
                    audioSourceMove.clip = airFallingClip;
                    play = true;
                }
                
                // if there's a sound we should be playing, play it!
                if (play){
                    if (!audioSourceMove.isPlaying){
                        audioSourceMove.Play();
                    }
                }
                else{
                    audioSourceMove.Stop();
                }
            }
            else{
                audioSourceMove.Stop();
            }

        }

        protected override void OnGrounded_Hook(){
            base.OnGrounded_Hook();
            if (audioSourceAction == null) { return; }

            // we have hit the ground, play the impact sound
            if (impactClip != null){
                audioSourceAction.clip = impactClip;
                if (!audioSourceAction.isPlaying){
                    audioSourceAction.Play();
                }
            }


        }

        protected override void OnJump_Hook(){
            base.OnJump_Hook();
            if (audioSourceAction == null){ return; }

            bool play = false;
            // we just jumped for the first time, jump sound
            if (jumpCounter == 1 && jumpClip != null){
                audioSourceAction.clip = jumpClip;
                play = true;
            }
            // this is a double jump, double jump sound
            else if (doubleJumpClip != null){
                audioSourceAction.clip = doubleJumpClip;
                play = true;
            }

            if (!audioSourceAction.isPlaying && play){
                audioSourceAction.Play();
            }
        }

        protected bool groundLoop(){
            return Input.GetKey(left) || Input.GetKey(right);
        }
    }

    
}