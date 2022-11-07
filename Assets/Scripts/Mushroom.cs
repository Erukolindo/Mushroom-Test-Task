using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mushroom : SimulationObject
{
    private new void Start()
    {
        base.Start();
        StartCoroutine(mushroomSpawning());
    }

    IEnumerator mushroomSpawning()
    {
        int SpawnCheck = 0;
        while (CurrentHP > 0)
        {
            yield return new WaitForSeconds(1);
            SpawnCheck = Random.Range(0, 100);
            if (SpawnCheck == 0)
            {
                spawnNewMushroom();
            }
        }
    }

    void spawnNewMushroom()
    {
        Vector2 mushroomPosition = Random.insideUnitCircle.normalized * 3;
        int freezePrevention = 0;
        while (SimulationController.Instance.spawnBlockedByObject(mushroomPosition + (Vector2)transform.position, new Vector2(1, 1)))
        {
            mushroomPosition = Random.insideUnitCircle.normalized * 3;
            freezePrevention++;
            if (freezePrevention > 200)
                break;
        }

        if (!IsInBounds(mushroomPosition + (Vector2)transform.position))
        {
            return;
        }

        Instantiate(gameObject, mushroomPosition + (Vector2)transform.position, new Quaternion(0, 0, 0, 0), transform.parent);
        SimulationController.Instance.UnpickedMushrooms++;
    }

    bool IsInBounds(Vector2 TestedValue)
    {
        float BoundsToCenter = SimulationController.Instance.MapSize * .5f;
        if (Mathf.Abs(TestedValue.x) > BoundsToCenter | Mathf.Abs(TestedValue.y) > BoundsToCenter)
        {
            return false;
        }
        return true;
    }

    public override void DeathBehaviour()
    {
        SimulationController.Instance.UnpickedMushrooms--;
    }
}
