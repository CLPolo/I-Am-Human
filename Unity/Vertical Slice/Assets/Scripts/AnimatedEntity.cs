using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedEntity : MonoBehaviour
{
    public List<Sprite> DefaultAnimationCycle;
    public float Framerate = 12f;//frames per second
    public SpriteRenderer SpriteRenderer;//spriteRenderer

    //private animation stuff
    private float animationTimer;//current number of seconds since last animation frame update
    private float animationTimerMax;//max number of seconds for each frame, defined by Framerate
    private int index;//current index in the DefaultAnimationCycle


    //interrupt animation info
    private bool interruptFlag;
    private bool persistFlag;
    private List<Sprite> interruptAnimation;


    //Set up logic for animation stuff
    protected void AnimationSetup()
    {
        animationTimerMax = 2f/(float)Framerate;
        index = 0;
    }

    //Default animation update
    protected void AnimationUpdate()
    {
        animationTimer += Time.deltaTime;

        if (animationTimer > animationTimerMax)
        {
            animationTimer = 0;
            index++;

            if (!interruptFlag)
            {
                if (DefaultAnimationCycle.Count == 0 || index >= DefaultAnimationCycle.Count)
                {
                    index = 0;
                }
                if (DefaultAnimationCycle.Count > 0)
                {
                    SpriteRenderer.sprite = DefaultAnimationCycle[index];
                }
            }
            else
            {
                if (interruptAnimation == null || index >= interruptAnimation.Count)
                {
                    index = 0;
                    if (!persistFlag || interruptAnimation == null)
                    {
                        // don't loop the interrupt animation (end after current cycle)
                        interruptFlag = false;
                        interruptAnimation = null;//clear interrupt animation
                    }
                }
                else
                {
                    SpriteRenderer.sprite = interruptAnimation[index];
                }
            }
        }
    }

    //Interrupt animation
    // This will play the interrupt animation then return to the default animation cycle once it is complete
    // If the interrupt animation cycle needs to be looped until a trigger then pass persistInterrupt = true
    protected void InterruptAnimation(List<Sprite> _interruptAnimation, bool persistInterrupt = false)
    {
        persistFlag = persistInterrupt;
        interruptFlag = true;
        animationTimer = 0;
        index = 0;
        interruptAnimation = _interruptAnimation;
        SpriteRenderer.sprite = interruptAnimation[index];
    }

    // currently the only way of ending persisted interrupt animations
    protected void ResetAnimationCycle()
    {
        persistFlag = false;
        interruptFlag = false;
        animationTimer = 0;
        index = 0;
        interruptAnimation = null;
        SpriteRenderer.sprite = DefaultAnimationCycle[index];
    }
    public int GetAnimationIndex(){
        return index;
    }
}
