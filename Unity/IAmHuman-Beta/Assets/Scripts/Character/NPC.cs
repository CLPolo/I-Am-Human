using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : AnimatedEntity
{
    protected bool following = false;
    private float speed = 1f;
    // enemies will start following if distance to player is less than or equal to this
    protected float? detectionRange = null;
    // sister will only start following if distance is larger than proximity
    protected float? proximity = null;
    // both enemy and sister will follow until distance is equal to untilDistance
    protected float untilDistance = 0f;
    protected Player player;

    private bool inLatency = false;

    protected void FollowPlayer()
    {
        Vector3 playerPosition = player.transform.position;
        float distance = Vector2.Distance(transform.position, playerPosition);
        if ((proximity == null || distance > proximity) && (detectionRange == null || distance <= detectionRange))
        {
            following = true;
        } else if (following && distance <= untilDistance) {
            following = false;
        }

        if (following && distance > untilDistance && !inLatency)
        {
            Vector3 direction = playerPosition - transform.position;
            direction.y = 0;
            direction.z = 0;
            direction = direction.normalized;
            transform.position += direction * Time.deltaTime * speed;
            Flip(direction.x);
        }
        else
        {
            following = false;
        }
    }

    protected void HidingLatency()
    {
        StartCoroutine(Timer());
    }

    protected IEnumerator Timer()
    {
        inLatency = true;
        yield return new WaitForSeconds(1f);
        inLatency = false;
        PlayerPrefs.SetInt("finishedLatency", 1);
    }

    public void Flip(float x)
    {
        GetComponent<SpriteRenderer>().flipX = (x < 0);
    }

    protected void SetupNPC(float speed, float untilDistance = 0f, float? proximity = null, float? detectionRange = null)
    {
        this.speed = speed;
        this.untilDistance = untilDistance;
        this.proximity = proximity;
        this.detectionRange = detectionRange;
    }

    public bool GetFollowing()
    {
        return following;
    }
}
