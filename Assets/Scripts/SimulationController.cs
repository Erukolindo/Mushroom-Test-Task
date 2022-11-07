using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public int NumberOfTribes;
    public int MapSize;
    public GameObject BasePrefab, MushroomPrefab;
    public Transform SimulationSpace;
    List<Color> usedColors = new List<Color>();
    bool simulating;

    [HideInInspector]
    public int UnpickedMushrooms;

    public static SimulationController Instance;

    private void Awake()
    {
        Instance = this;
        initiateSimulation();
    }

    void initiateSimulation() //could change the camera size based on mapsize
    {
        UnpickedMushrooms = 0;
        simulating = true;
        for (int i = 0; i < NumberOfTribes; i++)
        {
            spawnABase(i);
        }

        StartCoroutine(mushroomSpawnCycle());
    }

    void spawnABase(int team)
    {
        Vector2 basePosition = Random.insideUnitCircle.normalized * (.69f * .5f * MapSize);
        int freezePrevention = 0;
        while (spawnBlockedByObject(basePosition, new Vector2(2, 2)))
        {
            basePosition = Random.insideUnitCircle.normalized * (.69f * .5f * MapSize);
            freezePrevention++;
            if (freezePrevention > 200)
                break;
        }

        GameObject newBase = Instantiate(BasePrefab, basePosition, new Quaternion(0, 0, 0, 0), SimulationSpace);
        Base newBaseValues = newBase.GetComponent<Base>();
        newBaseValues.TribeColor = randomUnique();
        newBaseValues.Team = team;
    }

    void spawnAMushroom()
    {
        Vector2 mushroomPosition = new Vector2(Random.Range(-MapSize, MapSize), Random.Range(-MapSize, MapSize)) * .5f;
        int freezePrevention = 0;
        while (spawnBlockedByObject(mushroomPosition, new Vector2(1, 1)))
        {
            mushroomPosition = new Vector2(Random.Range(-MapSize, MapSize), Random.Range(-MapSize, MapSize)) * .5f;
            freezePrevention++;
            if (freezePrevention > 200)
                break;
        }

        Instantiate(MushroomPrefab, mushroomPosition, new Quaternion(0, 0, 0, 0), SimulationSpace);
        UnpickedMushrooms++;
    }

    IEnumerator mushroomSpawnCycle()
    {
        while (simulating)
        {
            yield return new WaitForSeconds(3);
            if (UnpickedMushrooms < 50)
            {
                spawnAMushroom();
            }
        }
    }

    public bool spawnBlockedByObject(Vector2 SpawnObjectPosition, Vector2 SpawnObjectSize)
    {
        return Physics2D.OverlapBox(SpawnObjectPosition, SpawnObjectSize, 0);
    }

    Color randomUnique()
    {
        Color NewColor = Random.ColorHSV(0, 1, .5f, 1, 1, 1, 1, 1);
        while (usedColors.Contains(NewColor))
        {
            NewColor = Random.ColorHSV(0, 1, .5f, 1, 1, 1, 1, 1);
        }
        usedColors.Add(NewColor);
        return NewColor;
    }
}
