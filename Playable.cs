using UnityEngine;
using System.Collections;


public class Playable : MonoBehaviour
{
    public int Health;   // Жизни
    public int Ammo = 90;   // Кол-во боеприпасов
    public float ViewRadius;    // Радиус обзора
    public LayerMask EnemyLayer;     // Маска слоёв    
    public Collider[] TargetsInRadius; // Цели в нашем радиусе
    public bool IsAlive;    // Проверка живой ли персонаж?

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public virtual void TakeDamage(int Damage)
    {
        Health -= Damage;

       // DisHealth.text = "Health: " + Health;

        if (Health <= 0)
        {
            //Условно смерть
        }
    }
}
