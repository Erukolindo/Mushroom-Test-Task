using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : SimulationObject
{
    [HideInInspector]
    public Color TribeColor;
    public float SpawnInterval;
    public GameObject PickerPrefab;
    int mushroomCounter = 0;

    Picker.TribeModifiers tribeModifiers = new Picker.TribeModifiers();

    private new void Start()
    {
        base.Start();
        GetComponent<SpriteRenderer>().color = TribeColor;
        StartCoroutine(pickerSpawning());
    }

    IEnumerator pickerSpawning()
    {
        while (CurrentHP > 0)
        {
            yield return new WaitForSeconds(SpawnInterval);
            spawnAPicker();
        }
    }

    void spawnAPicker()
    {
        Vector2 pickerPosition = Random.insideUnitCircle.normalized * 4;
        int freezePrevention = 0;
        while (SimulationController.Instance.spawnBlockedByObject(pickerPosition + (Vector2)transform.position, new Vector2(2, 2)))
        {
            pickerPosition = Random.insideUnitCircle.normalized * 4;
            freezePrevention++;
            if (freezePrevention > 200)
                break;
        }

        GameObject newPicker = Instantiate(PickerPrefab, pickerPosition + (Vector2)transform.position, new Quaternion(0, 0, 0, 0), transform.parent);
        Picker newPickerValues = newPicker.GetComponent<Picker>();
        newPickerValues.Team = Team;
        newPickerValues.Modifiers = tribeModifiers;
        newPickerValues.BasePosition = transform.position;
        newPicker.GetComponent<SpriteRenderer>().color = TribeColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Picker returningPicker = collision.gameObject.GetComponent<Picker>();
        checkForNewMushroom(returningPicker);
    }

    void checkForNewMushroom(Picker pickerChecked)
    {
        if (pickerChecked == null)
            return;
        if (pickerChecked.Team != Team | pickerChecked.CarriedMushroom == null)
            return;

        mushroomCounter++;
        pickerChecked.MushroomDelivered();

        if (mushroomCounter == 5)
        {
            mushroomCounter = 0;
            MutateStatistics();
        }
    }

    void MutateStatistics()
    {
        Attack = Mathf.Max(1, Attack + Random.Range(-1, 3));
        Defense = Mathf.Max(1, Defense + Random.Range(-1, 3));
        MaxHP = Mathf.Max(1, MaxHP + Random.Range(-1, 3));
        CurrentHP = Mathf.Max(1, CurrentHP + Random.Range(-1, 3));
        SpawnInterval = Mathf.Max(1, SpawnInterval + Random.Range(-1, 3));


        tribeModifiers.Attack = Mathf.Max(1, tribeModifiers.Attack + Random.Range(-1, 3));
        tribeModifiers.Defense = Mathf.Max(1, tribeModifiers.Defense + Random.Range(-1, 3));
        tribeModifiers.MaxHP = Mathf.Max(1, tribeModifiers.MaxHP + Random.Range(-1, 3));
        tribeModifiers.MovementSpeed = Mathf.Max(1, tribeModifiers.MovementSpeed + Random.Range(-1, 3));
        tribeModifiers.VisionRange = Mathf.Max(1, tribeModifiers.VisionRange + Random.Range(-1, 3));
    }
}
