using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;  // https://learn.unity.com/tutorial/starting-timeline-through-a-c-script-2019-3#5ff8d183edbc2a0020996601

public class CameraMovement : MonoBehaviour
{

    public Transform player;
    public Vector3 offset;
    public Player playerAccess = null;

    [Header("Screen Shake")]
    public AnimationCurve hideCurve;
    public AnimationCurve monsterFootstepsCurve;
    private bool shaking = false;

    [Header("Level Bounds")]
    // These are the edges of the level where the camera should stop following the player
    public float leftBound = -100;
    public float rightBound = 100;
    public float cameraYOffset = 0;

    public static bool checkBoundsAgain = false;

    [Header("Camera Control")]
    private int pans = 0;  // The number of camera pans completed

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
     
        offset.Set(0f, cameraYOffset, -10f);
        CheckBounds();
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
            //if (playerAccess != null && playerAccess.GetState() == PlayerState.Hiding && !shaking)
            //{
            //    shaking = true;
            //    StartCoroutine(Shaking());
            //}  ?? for some reason the player.instance isn't working ??
            if (Input.GetKeyDown(KeyCode.K))  // just for test purposes until the above is figured out
            {
                shaking = true;
                StartCoroutine(Shaking(1f, hideCurve));
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
                        StartCoroutine(PanCamera(1, -0.66f, 5, 2));  
                        break;
                    case 1:
                        // Move to the right to make space for dialogue box
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(2, 3.56f, 3, 0.2f));
                        break;
                    case 2:
                        // Move to show monster again and flip the character
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(3, 9f, 2));
                        // flip character here
                        break;
                    case 3:
                        // Move to og position
                        pans = -1;  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(4, 13.5f, 2, 2));
                        // drop to knees here
                        break;
                        //case 3:
                        //    PlayerPrefs.SetInt("NumPans", -1);  // This way we cannot advance until coroutine finishes
                        //    StartCoroutine(PanCamera(3, 13.5f));
                        //    break;
                        //case 4:
                        //    PlayerPrefs.SetInt("NumPans", -1);  // This way we cannot advance until coroutine finishes
                        //    StartCoroutine(PanCamera(4, 17.6f));
                        //    break;
                }
            }
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

        Debug.Log(GameObject.Find("Cutscene Triggers/EndingCutscene").GetComponent<PuzzleTargetScript>().GetTextPlayed());
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
        //switch (panNum)
        //{
        //    case 1:
        //        while (gameObject.transform.position.x > xCoord)
        //        {
        //            // We want a slow pan for a slow reveal of the car
        //            gameObject.transform.position = new Vector3(
        //                gameObject.transform.position.x - ((originalLoc.x - xCoord)*Time.deltaTime/5f),  // Takes 5 seconds
        //                originalLoc.y,
        //                originalLoc.z);
        //            yield return null;
        //        }
        //        yield return new WaitForSeconds(2);
        //        break;
        //    case 2:
        //        // Move to the right to make space for dialogue box
        //        while (gameObject.transform.position.x < xCoord)
        //        {
        //            gameObject.transform.position = new Vector3(
        //                gameObject.transform.position.x - ((originalLoc.x - xCoord) * Time.deltaTime / 3f),  // Takes 3 seconds
        //                originalLoc.y,
        //                originalLoc.z);
        //            yield return null;
        //        }
        //        PlayerPrefs.SetInt("FinalDialogue", 1);
        //        //yield return new WaitUntil(()=>PlayerPrefs.GetInt("FinalDialogue") == 0);
        //        break;
        // DROP TO KNEES HERE FOR FUTURE REFERENCE
        //case 3:
        //    while (gameObject.transform.position.x < xCoord)
        //    {
        //        gameObject.transform.position = new Vector3(
        //            gameObject.transform.position.x - ((originalLoc.x - xCoord) * Time.deltaTime / 3f),  // Takes 2 seconds
        //            originalLoc.y,
        //            originalLoc.z);
        //        yield return null;
        //    }
        //    PlayerPrefs.SetInt("FinalDialogue", 2);
        //    yield return new WaitUntil(() => PlayerPrefs.GetInt("FinalDialogue") == 0);
        //    break;
        //case 4:
        //    while (gameObject.transform.position.x < xCoord)
        //    {
        //        gameObject.transform.position = new Vector3(
        //            gameObject.transform.position.x - ((originalLoc.x - xCoord) * Time.deltaTime / 3f),  // Takes 2 seconds
        //            originalLoc.y,
        //            originalLoc.z);
        //        yield return null;
        //    }
        //    while (gameObject.GetComponent<Camera>().orthographicSize > 3)
        //    {
        //        gameObject.GetComponent<Camera>().orthographicSize -= 0.1f;
        //        yield return new WaitForSeconds(0.05f);
        //    }
        //    PlayerPrefs.SetInt("FinalDialogue", 3);
        //    yield return new WaitUntil(() => PlayerPrefs.GetInt("FinalDialogue") == 0);
        //    break;
        //}
        //PlayerPrefs.SetInt("NumPans", panNum);
    }
}
