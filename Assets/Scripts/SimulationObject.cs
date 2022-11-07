using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SimulationObject : MonoBehaviour
{
    public int Attack, Defense, MaxHP, CurrentHP;
    [HideInInspector]
    public int Team;
    public SpriteRenderer HPBar;
    public TMP_Text HPDisplay;
    [HideInInspector]
    public List<SimulationObject> Attackers = new List<SimulationObject>();
    [HideInInspector]
    public List<SimulationObject> Defenders = new List<SimulationObject>();
    [HideInInspector]
    public Transform CarriedMushroom;

    protected virtual void Start()
    {
        CurrentHP = MaxHP;
        HPDisplay.text = CurrentHP.ToString();
    }

    public void MakeAnAttack(SimulationObject enemy)
    {
        CurrentHP -= calculateDamage(enemy.Attack, Defense);
        enemy.CurrentHP -= calculateDamage(Attack, enemy.Defense);

        if (enemy.CurrentHP < 1)
        {
            Defenders.Remove(enemy);
            enemy.DeathBehaviour();

            if (enemy.gameObject.layer == LayerMask.NameToLayer("Mushroom"))
            {
                CarriedMushroom = enemy.transform;
                SimulationController.Instance.UnpickedMushrooms--;
                enemy.CurrentHP = 0;
                enemy.updateHPDisplay();
                enemy.transform.parent = transform;
                enemy.GetComponent<Collider2D>().enabled = false;
            }
        }
        else
        {
            enemy.updateHPDisplay();
        }

        if (CurrentHP < 1)
        {
            enemy.Attackers.Remove(this);
            DeathBehaviour();
        }
        else
        {
            updateHPDisplay();
        }
    }

    void updateHPDisplay()
    {
        HPBar.size = new Vector2(CurrentHP / MaxHP, HPBar.size.y);
        HPDisplay.text = CurrentHP.ToString();
    }

    int calculateDamage(int attack, int defense)
    {
        int defensePower = Random.Range(0, defense);
        return Mathf.Max(0, attack - defensePower);
    }

    public virtual void DeathBehaviour()
    {
        Destroy(gameObject);
    }
}
