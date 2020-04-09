using UnityEngine;
using System.Collections;

public class PickUp : MonoBehaviour
{
        
    public int Health, Ammo, Speed; // Кол-во восстанавливаемых жизней и патронов, а так же скорость вращения

		
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(0, Speed,0);	// Вращение пикапика
	}

    /// <summary>
    /// Срабатывает при пересечении коллайдера
    /// </summary>
    /// <param name="Other"></param>
    void OnTriggerEnter(Collider Other)
    {
        if(Other.gameObject.layer == 8 || Other.gameObject.layer == 9) // Если объект находится на слою Ally или Enemy
        {
            Playable Pl = Other.gameObject.GetComponent<Playable>(); // Ссылка на пересекаемый объект
            Pl.Ammo += Ammo;   // Добавляем патронов
            Pl.Health += Health; // Добавляем жизней

            // Если пересёк игрок то отображаем его дисплей
            if (Other.gameObject.tag == "Player")
            {
                MyControll MPL = Other.gameObject.GetComponent<MyControll>(); // Ссылку на игрока
                MPL.DisAmmo.text = "Ammo: " + MPL.Ammo;        // Изменение текста на дисплее
                MPL.DisHealth.text = "Health: " + MPL.Health;
            }

            Destroy(gameObject); // Удалить пикапик
        }
    }
}
