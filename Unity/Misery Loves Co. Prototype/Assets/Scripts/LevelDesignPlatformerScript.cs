using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeel{
    public class LevelDesignPlatformerScript : AudioPlatformerScriptV2
    {
        [Header("Level Settings")]
        public AudioSource audioSourceMusic; // audio source for background music
        public AudioClip backgroundMusic; // clip for background music

        protected override void Start(){
            base.Start();
            // if we have background music, start it!
            if (audioSourceMusic != null && backgroundMusic != null){
                audioSourceMusic.clip = backgroundMusic;
                audioSourceMusic.Play();
            }
        }

        protected override void OnGrounded_Hook(){
            base.OnGrounded_Hook();
            // check if we've landed on a platform, and send a signal if so!
            Vector2 extents = _playerCollider.bounds.extents;
            Vector2 rayPosition = new Vector2(transform.position.x, transform.position.y - extents.y); // reminder to unhack once yalmaz makes collider protected
            RaycastHit2D checkValid = Physics2D.Raycast(rayPosition, Vector2.down);

            Debug.Log(checkValid.collider);
            if (checkValid.collider){
                checkValid.collider.gameObject.SendMessage("onContact");
            }

        }
    }


    
}