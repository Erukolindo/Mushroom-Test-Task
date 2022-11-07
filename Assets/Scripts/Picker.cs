using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Picker : SimulationObject
{
    public float MovementSpeed;
    public float VisionRadius;
    [HideInInspector]
    public TribeModifiers Modifiers = new TribeModifiers();
    [HideInInspector]
    public Vector2 BasePosition;
    Vector2 wanderingLocation = new Vector2();
    Transform pursuedEnemy;
    Coroutine currentBehaviour;
    int currentMode = 0; //0 - wandering, 1 - pursuit, 2 - combat, 3 - returning
    int newMode = 0;
    Rigidbody2D pickerBody;
    int bounds;

    public class TribeModifiers
    {
        public int Attack, Defense, MaxHP, MovementSpeed, VisionRange;
    }

    private new void Start()
    {
        initialValues();
        applyModifiers();
        base.Start();
        StartCoroutine(targetPickingLoop());
        currentBehaviour = StartCoroutine(WanderingBehaviour());
    }

    void initialValues()
    {
        pickerBody = GetComponent<Rigidbody2D>();
        bounds = SimulationController.Instance.MapSize;
        Modifiers.Attack = 0;
        Modifiers.Defense = 0;
        Modifiers.MaxHP = 0;
        Modifiers.MovementSpeed = 0;
        Modifiers.VisionRange = 0;
    }

    void applyModifiers()
    {
        Attack = Mathf.Max(1, Attack + Modifiers.Attack);
        Defense = Mathf.Max(1, Defense + Modifiers.Defense);
        MaxHP = Mathf.Max(1, MaxHP + Modifiers.MaxHP);
        MovementSpeed = Mathf.Max(1, MovementSpeed + Modifiers.MovementSpeed);
        VisionRadius = Mathf.Max(1, VisionRadius + Modifiers.VisionRange);
    }

    IEnumerator targetPickingLoop()
    {
        while (CurrentHP > 0)
        {
            wanderingLocation = randomWithinBounds(bounds);
            yield return new WaitForSeconds(10);
        }
    }

    Vector2 randomWithinBounds(int bounds)
    {
        float halfBound = bounds * .5f;
        return new Vector2(Random.Range(-halfBound, halfBound), Random.Range(-halfBound, halfBound));
    }

    IEnumerator WanderingBehaviour()
    {
        currentMode = 0;
        while (CurrentHP > 0)
        {
            SearchForEnemies();
            Move(wanderingLocation);
            if (Vector2.Distance(transform.position, wanderingLocation) < 1)
            {
                wanderingLocation = randomWithinBounds(bounds);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    void SearchForEnemies()
    {
        Collider2D[] objectsInVision = Physics2D.OverlapCircleAll(transform.position, VisionRadius, LayerMask.GetMask("Mushroom", "Tribes"));
        List<Transform> enemiesInVision = new List<Transform>();
        foreach (var objectInVision in objectsInVision)
        {
            if (isObjectAnEnemy(objectInVision))
            {
                enemiesInVision.Add(objectInVision.transform);
            }
        }
        if (enemiesInVision.Count == 0)
            return;

        float shortestDistance = VisionRadius + 5;
        Transform nearestEnemy = null;
        foreach (var enemy in enemiesInVision)
        {
            float enemyDistance = Vector2.Distance(transform.position, enemy.position);
            if (enemyDistance < shortestDistance)
            {
                shortestDistance = enemyDistance;
                nearestEnemy = enemy;
            }
        }

        pursuedEnemy = nearestEnemy;
        newMode = 1;
    }

    bool isObjectAnEnemy(Collider2D objectTested)
    {
        if (objectTested.gameObject.layer == LayerMask.NameToLayer("Mushroom"))
        {
            return true;
        }
        if (objectTested.GetComponent<SimulationObject>().Team != Team)
        {
            return true;
        }
        return false;
    }

    IEnumerator PursuitBehaviour()
    {
        currentMode = 1;
        while (CurrentHP > 0)
        {
            if (pursuedEnemy != null)
            {
                Move(pursuedEnemy.position);
            }
            else
            {
                newMode = 0;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        pickerBody.velocity = Vector2.zero;
        testForEnemy(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        TestForDisengage(collision);
    }

    void TestForDisengage(Collision2D collision)
    {
        SimulationObject objectLeft = collision.gameObject.GetComponent<SimulationObject>();
        if (objectLeft)
        {
            if (Defenders.Contains(objectLeft))
            {
                Defenders.Remove(objectLeft);
                objectLeft.Attackers.Remove(this);
            }
        }
    }

    void testForEnemy(Collider2D objectHit)
    {
        SimulationObject simulationObject = objectHit.gameObject.GetComponent<SimulationObject>();
        if (simulationObject)
        {
            if (isObjectAnEnemy(objectHit))
            {
                beginCombat(simulationObject);
            }
        }
    }

    void beginCombat(SimulationObject target)
    {
        if (!Attackers.Contains(target))
        {
            Defenders.Add(target);
            target.Attackers.Add(this);
        }

        if (currentMode != 2)
        {
            newMode = 2;
        }
    }

    IEnumerator CombatBehaviour()
    {
        currentMode = 2;
        while (CurrentHP > 0)
        {
            for (int i = Defenders.Count - 1; i > -1; i--)
            {
                if (i < Defenders.Count)
                {
                    MakeAnAttack(Defenders[i]);
                }
            }
            testForCombatEnd();
            yield return new WaitForSeconds(5);
        }
    }

    void testForCombatEnd()
    {
        if (CarriedMushroom != null)
        {
            newMode = 3;
            return;
        }

        if (Attackers.Count == 0 & Defenders.Count == 0)
        {
            newMode = 0;
        }
    }

    IEnumerator ReturningBehaviour()
    {
        currentMode = 3;
        while (CurrentHP > 0)
        {
            Move(BasePosition);
            yield return new WaitForFixedUpdate();
        }
    }

    public void MushroomDelivered()
    {
        Destroy(CarriedMushroom.gameObject);
        CarriedMushroom = null;

        newMode = 0;
    }

    void Move(Vector2 CurrentTarget)
    {
        Vector2 movementDirection = (CurrentTarget - (Vector2)transform.position).normalized;
        //pickerBody.MovePosition((Vector2)transform.position + (movementDirection * MovementSpeed * .02f));
        transform.position += (Vector3)movementDirection * MovementSpeed * .02f;
    }

    public override void DeathBehaviour()
    {
        if (CarriedMushroom != null)
            Destroy(CarriedMushroom.gameObject);
        base.DeathBehaviour();
    }

    private void FixedUpdate()
    {
        if (newMode != currentMode)
        {
            changeBehaviour();
        }
    }

    void changeBehaviour()
    {
        StopCoroutine(currentBehaviour);
        switch (newMode)
        {
            case 0:
                currentBehaviour = StartCoroutine(WanderingBehaviour());
                break;
            case 1:
                currentBehaviour = StartCoroutine(PursuitBehaviour());
                break;
            case 2:
                currentBehaviour = StartCoroutine(CombatBehaviour());
                break;
            case 3:
                currentBehaviour = StartCoroutine(ReturningBehaviour());
                break;
            default:
                break;
        }
    }
}
