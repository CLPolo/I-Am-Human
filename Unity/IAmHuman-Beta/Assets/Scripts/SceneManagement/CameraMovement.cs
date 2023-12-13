using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;  // https://learn.unity.com/tutorial/starting-timeline-through-a-c-script-2019-3#5ff8d183edbc2a0020996601

public class CameraMovement : MonoBehaviour
{
    private LogicScript logic;
    public Transform player;
    public Vector3 offset;
    public Player playerAccess = null;

    [Header("Screen Shake")]
    public AnimationCurve monsterFootstepsCurve;
    public AnimationCurve stuckCurve;
    private bool shaking = false;

    [Header("Level Bounds")]
    // These are the edges of the level where the camera should stop following the player
    public float leftBound = -100;
    public float rightBound = 100;
    public float cameraYOffset = 0;

    public static bool checkBoundsAgain = false;

    [Header("Camera Control")]
    private int pans = 0;  // The number of camera pans completed

    private float StartSize;
    private float noSmallerThan;
    private bool currentlyShrinking = false;

    // Start is called before the first frame update
    void Start()
    {
        logic = LogicScript.Instance;
        player = GameObject.FindWithTag("Player").transform;
     
        offset.Set(0f, cameraYOffset, -10f);
        CheckBounds();

        StartSize = this.GetComponent<Camera>().orthographicSize;
        noSmallerThan = StartSize / 1.25f;
    }

    private void CheckBounds()
    {
        if (player.transform.position.x <= leftBound)
        {
            transform.position = new Vector3(leftBound, player.position.y, player.position.z) + offset;
        }
        else if (player.transform.position.x >= rightBound)
        {
            transform.position = new Vector3(rightBound, player.position.y, player.position.z) + offset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerAccess == null)
        {
            playerAccess = Player.Instance;
        }
        if (checkBoundsAgain)
        {
            CheckBounds();
            checkBoundsAgain = false;
        }
        if (playerAccess.shrinkCamera)
        {
            StartCoroutine(ShrinkCamera());
            playerAccess.shrinkCamera = false;  // Set to false so it does not repeat (only need to call it once)
        }
        if (!playerAccess.cameraCutscene)
        {
            if (player.transform.position.x > leftBound && player.transform.position.x < rightBound)
            {
                transform.position = player.position + offset;  // Only change position if within bounds
            }
            if (playerAccess.monsterEmergesCutscene == true && PlayerPrefs.GetInt("MonsterEmerges") == 0)
            {
                // This is the cutscene! The player is frozen already
                StartCoroutine(threeCrashes(0.3f, monsterFootstepsCurve, playerAccess.monsterFootsteps[0]));
                PlayerPrefs.SetInt("MonsterEmerges", 1);
            }
            if (!currentlyShrinking)
            {
                HandleHiding();
                HandleStuck();
            }
        }
        else
        {
            // We are in a cutscene involving camera movement, so disable the other camera movement
            if (playerAccess.finalCutscene)
            {
                leftBound = -100;  // Avoid weird camera snapping issue
                // After the finalCutscene is started and camera is finished Shrinking we can Animate
                // We are in the final cutscene and the camera has shrunk enough
                switch (pans)
                {
                    case 0:
                        // We want a slow pan for a slow reveal of the car
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(1, -0.66f, 5, 4));  
                        break;
                    case 1:
                        // Move to the right to make space for dialogue box
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(2, 3.56f, 2, 0.2f)); // the 0.2 second wait is needed to display the text... don't ask me why
                        break;
                    case 2:
                        // Move to show monster again and flip the character
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(3, 9f, 2));
                        break;
                    case 3:
                        // Move to og position
                        playerAccess.Flip();  // To face the monsert
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(4, 13.5f, 2, 0.2f));
                        // playerAccess.SpriteRenderer.sprite = playerAccess.idleHide;  // Character "Falls to knees" FIX IF WE HAVE TIME
                        break;
                    case 4:
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(5, 17.6f, 2, 0.2f));  // Center on the monster "I..."
                        break;
                    case 5:
                        gameObject.GetComponent<Camera>().orthographicSize = 3.5f;  // Zoom in on monster
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(6, 17.6f, 0.2f, 0.2f));  // Play the next text "am..."
                        break;
                    case 6:
                        gameObject.GetComponent<Camera>().orthographicSize = 3f;
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(7, 17.6f, 0.2f, 0.2f));  // Play the final text "HUMAN"
                        break;
                    case 7:
                        // We are done! Move to end credits
                        logic.playMonsterRoar();
                        gameObject.GetComponent<Camera>().orthographicSize = 2.5f;  // One final zoom
                        pans = 8;  // Final time so this line doesnt replay
                        break;
                }
            }
        }
    }

    private void HandleHiding()
    {
        // if player is hiding, mildly zooms cam and once at 'full' zoom (noSmallerThan % of the canvas has been zoomed in on) it has a mild camera shake (as dictated by hideCurve on camera's inspector)
        CheckBounds();
        if (playerAccess.GetState() == PlayerState.Hiding)  // if the player is currently hiding
        {
            if (this.GetComponent<Camera>().orthographicSize <= noSmallerThan)  // if it's zoomed to max
            {
                this.GetComponent<Camera>().orthographicSize = this.GetComponent<Camera>().orthographicSize;  // keep it at this size
            }
            else this.GetComponent<Camera>().orthographicSize = this.GetComponent<Camera>().orthographicSize / 1.0002f;  // if not full zoom, slowly zoom (smaller num = slower, must be above 1 tho or else zooms out not in)
        }
        else  // not hiding
        {
            if (this.GetComponent<Camera>().orthographicSize < StartSize)  // if not back to normal
            {
                this.GetComponent<Camera>().orthographicSize = this.GetComponent<Camera>().orthographicSize * 1.004f;  // zooms out faster than zoom in (but not immediately snap to normal)
            }
            else if (this.GetComponent<Camera>().orthographicSize >= StartSize)  // if back to normal
            {
                this.GetComponent<Camera>().orthographicSize = StartSize;  // resets size to EXACT start size (must do this to prevent slight dif in cam size cause of multiplication / division)
            }
        }
    }

    private void HandleStuck()
    {
        // if player is stuck, does the same camera shake as the hiding. Most of the code is copied from HandleHiding() with the camera effects removed
        if (playerAccess.GetState() == PlayerState.Trapped)  // if the player is currently trapped
        {
            if (!shaking) { shaking = true; StartCoroutine(Shaking(0.2f, stuckCurve)); }  // start the shake process as long as it's not already shaking
        }
    }

    IEnumerator Shaking(float shakeDuration, AnimationCurve curve)
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / shakeDuration);
            transform.position = player.position + offset + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = player.position + offset;
        shaking = false;
    }

    IEnumerator threeCrashes(float shakeDuration, AnimationCurve curve, AudioClip soundEffect)
    {
        // https://www.youtube.com/watch?v=BQGTdRhGmE4
        for (int i=0; i<3; i++)
        {
            playerAccess.AudioSource.PlayOneShot(playerAccess.monsterFootsteps[0], 0.25f*(i+1));  // Sound louder each time
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                elapsedTime += Time.deltaTime;
                float strength = curve.Evaluate(elapsedTime / shakeDuration);
                transform.position = player.position + offset + Random.insideUnitSphere * strength;
                yield return null;
            }

            transform.position = player.position + offset;
            yield return new WaitForSeconds(0.6f);
        }
        // The shaking is done, now the monster EMERGES !!!
        playerAccess.AudioSource.PlayOneShot(playerAccess.monsterFootsteps[0], 0.75f);  // Another for good measure
        PlayerPrefs.SetInt("MonsterEmerges", 2);  // THIS ANIMATION CONTINUES IN LogicScript.cs
    }

    IEnumerator ShrinkCamera()
    {
        currentlyShrinking = true;
        // Shrinks the camera to prepare for the final cutscene
        while (gameObject.GetComponent<Camera>().orthographicSize > 4)
        {
            gameObject.GetComponent<Camera>().orthographicSize -= 0.1f;
            yield return new WaitForSeconds(0.05f);
        }
        if (gameObject.GetComponent<Camera>().orthographicSize <= 4)
        {
            playerAccess.finalCutscene = true;  // We are ready to begin the final cutscene
        }
    }

    IEnumerator PanCamera(int panNum, float xCoord, float panTime, float waitAfterPan = 0)
    {
        /// ARGUMENTS:
        /// panNum: Which camera pan this is (i.e. 1st, 2nd, 3rd pan)
        /// xCoord: The final x coordinate of the camera position
        /// panTime: float representing how long the pan should take (in seconds)
        /// waitAfterPan: time (in seconds) to wait after panning. Can be left blank for a 0 second wait.

        // Debug.Log(GameObject.Find("Cutscene Triggers/EndingCutscene").GetComponent<PuzzleTargetScript>().GetTextPlayed());
        yield return new WaitUntil( () => GameObject.Find("Cutscene Triggers/EndingCutscene").GetComponent<PuzzleTargetScript>().GetTextPlayed());
        Vector3 originalLoc = gameObject.transform.position;
        if (xCoord > originalLoc.x)
        {
            while (gameObject.transform.position.x < xCoord)
            {
                gameObject.transform.position = new Vector3(
                    gameObject.transform.position.x - ((originalLoc.x - xCoord) * Time.deltaTime / panTime),  // Takes <panTime> seconds
                    originalLoc.y,
                    originalLoc.z);
                yield return null;
            }
        }
        else
        {
            while (gameObject.transform.position.x > xCoord)
            {
                gameObject.transform.position = new Vector3(
                    gameObject.transform.position.x - ((originalLoc.x - xCoord) * Time.deltaTime / panTime),  // Takes <panTime> seconds
                    originalLoc.y,
                    originalLoc.z);
                yield return null;
            }
        }
        PlayerPrefs.SetInt("FinalDialogue", panNum);  // This allows for text to properly show
        // Now that we have panned, we must wait:
        if (waitAfterPan != 0)
        {
            yield return new WaitForSeconds(waitAfterPan);
        }
        // After waiting, we can set the pans variable to the next value to tell the code we are ready to do the next camera pan
        pans = panNum;
    }
}
