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
        if (playerAccess.shrinkCamera && PlayerPrefs.GetInt("ShrinkCamera") != 1)
        {
            PlayerPrefs.SetInt("ShrinkCamera", 1);
            StartCoroutine(ShrinkCamera());
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
            if (playerAccess.finalCutscene && PlayerPrefs.GetInt("ShrinkCamera") == 2  && PlayerPrefs.GetInt("NumPans") != -1)
            {
                leftBound = -100;
                // After the finalCutscene is started and camera is finished Shrinking we can Animate
                // We are in the final cutscene and the camera has shrunk enough
                switch (PlayerPrefs.GetInt("NumPans"))
                {
                    case 0:
                        Debug.Log("CASE 1");
                        PlayerPrefs.SetInt("NumPans", -1);  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(1, -0.66f));
                        break;
                    case 1:
                        Debug.Log("CASE 2");
                        PlayerPrefs.SetInt("NumPans", -1);  // This way we cannot advance until coroutine finishes
                        StartCoroutine(PanCamera(2, 3.56f));
                        break;
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
        PlayerPrefs.SetInt("ShrinkCamera", 2);  // We are ready to begin camera pans
    }

    IEnumerator PanCamera(int panNum, float xCoord)
    {
        // Pans the camera in a specific direction
        Vector3 originalLoc = gameObject.transform.position;
        Debug.Log("THE PANNUM IS " + panNum.ToString());
        switch (panNum)
        {
            case 1:
                Debug.Log("HEY WE GOT HERE YASSSSS");
                while (gameObject.transform.position.x > xCoord)
                {
                    // We want a slow pan for a slow reveal of the car
                    gameObject.transform.position = new Vector3(
                        gameObject.transform.position.x - ((originalLoc.x - xCoord)*Time.deltaTime/5f),  // Takes 5 seconds
                        originalLoc.y,
                        originalLoc.z);
                    yield return null;
                }
                yield return new WaitForSeconds(2);
                break;
            case 2:
                // Move to the right to make space for dialogue box
                while (gameObject.transform.position.x < xCoord)
                {
                    gameObject.transform.position = new Vector3(
                        gameObject.transform.position.x - ((originalLoc.x - xCoord) * Time.deltaTime / 3f),  // Takes 3 seconds
                        originalLoc.y,
                        originalLoc.z);
                    yield return null;
                }
                PlayerPrefs.SetInt("FinalDialogue", 1);
                break;
        }
        PlayerPrefs.SetInt("NumPans", panNum);
        Debug.Log("THIS IS THE NUMPANS NOW: " + PlayerPrefs.GetInt("NumPans").ToString());
    }
}
