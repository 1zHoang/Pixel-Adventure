using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet_Bee : MonoBehaviour
{
    private Transform target;
    private List<Vector3> wayPoints = new List<Vector3>();
    private int wayIndex;

    [SerializeField] private GameObject pickupVfx;
    [SerializeField] private float wayPointUpdateCooldown;
    private float speed;

    public void SetupBullet(Transform newTarget, float newSpeed, float lifeDuration)
    {
        speed = newSpeed;
        target = newTarget;

        if (target != null)
        {
            transform.up = transform.position - target.position;
            StartCoroutine(AddWayPointCo());
            Destroy(gameObject, lifeDuration);
        }
        else
        {
            Debug.LogWarning("Target is null. Bullet setup failed.");
        }
    }

    private void Update()
    {
        if (wayPoints.Count <= 0)
            return;

        if (wayIndex >= wayPoints.Count)
        {
            wayIndex = wayPoints.Count - 1;
        }

        transform.position = Vector2.MoveTowards(transform.position, wayPoints[wayIndex], speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, wayPoints[wayIndex]) < .1f)
        {
            wayIndex++;
            
            if (wayIndex >= wayPoints.Count)
            {
                wayIndex = wayPoints.Count - 1;
            }
            else
            {
                transform.up = transform.position - wayPoints[wayIndex];
            }
        }
    }

    private IEnumerator AddWayPointCo()
    {
        while (true)
        {
            AddWayPoint();
            yield return new WaitForSeconds(wayPointUpdateCooldown);
        }
    }

    private void AddWayPoint()
    {
        if (target == null)
            return;

        foreach (Vector3 wayPoint in wayPoints)
        {
            if (wayPoint == target.position)
                return;
        }

        wayPoints.Add(target.position);
    }

    private void OnDestroy()
    {
        GameObject newFx = Instantiate(pickupVfx, transform.position, Quaternion.identity);
        newFx.transform.localScale = new Vector3(.6f, .6f, .6f);

        DestroyImmediate(newFx);
    }
}
