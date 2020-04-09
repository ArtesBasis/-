using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]

public class MyControll : Playable

{
    
    public Rigidbody MyBody;  
    //блок для оружия и стрельбы
    public GameObject Bullet, StartBullet; // шаблон пули
    public bool CanAttack = true, Reload = false; // состояния для проверки стреьбы
    public const int Magazine = 30; // константа максимально заполненого магазина                              
    public int CurMagazine = 30; // переменная хранения значения текущего магазина

   
    public float JumpForce; // сила прыжка,почему я его здесь прилепил? потом перетащить в контролмув

    public Text DisHealth, DisMagazine, DisAmmo; // ссылки на отобображения канваса

    float GroundDis; // расстояние до земли
    Collider Col;    
     // блок камеры и переменных ждя подгибания оружия
    public GameObject Cam;        // ссылка на камеру
    public GameObject Weapon;      // ссылка на объект оружия
    public float RotSpeedWeapon;   // скорость Вращения оружия
    Ray ray;       // переменная для отрисовки рейкаста, чтобы получить точку куда будет направляться пушка
    RaycastHit hit; // точка удара рейкаста

    
    void Start()
    {
        // отображение показателей канваса
        DisHealth.text = "Health: " + Health;
        DisAmmo.text = "Ammo: " + Ammo;
        DisMagazine.text = "Magazine: " + CurMagazine;

        Col = GetComponent<Collider>(); // Получение ссылки на собственный коллайдер

        Cursor.lockState = CursorLockMode.Locked; // Блокировка курсора

        MyBody = GetComponent<Rigidbody>();       // ссылка на собственное физическое тело

        GroundDis = Col.bounds.extents.y; // высчитывание половинного размера  нашей капсульколайдера от пивот
        
    }

    
    void Update()
    {
        if (IsAlive) // если пешка жива
        {
            if (Input.GetMouseButton(0)) // при нажатии ЛКМ
                StartCoroutine(Fire()); // начинаем короутину атаки

            if (Input.GetKeyDown(KeyCode.R)) // нажатие клавиши R
                StartCoroutine(StartReload()); // вызов короутины перезарядки

            ray = new Ray(Cam.transform.position, Cam.transform.forward); // чертим луч из центра камеры ровно вперёд

            Physics.Raycast(ray, out hit); // получаем точку удара

            Vector3 rot; 

            if (hit.collider == null) // если рейкаст ни во что не врезался, то просто чертим вектор вперёд
                rot = Weapon.transform.forward;

            else // в ином случае присваиваем точку удара
                rot = hit.point - Weapon.transform.position;


            // поворт(подгибание) пушки в сторону точки удара рейкаста
            Weapon.transform.rotation = Quaternion.Slerp(Weapon.transform.rotation, Quaternion.LookRotation(rot), RotSpeedWeapon * Time.deltaTime);

        }
    }

    
    void FixedUpdate()
    {
        if (IsAlive) // Проверка жив ли игрок
        {
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded()) // проверка нажат ли пробел и находится ли пешка в приземлённом состоянии
                //убрав && IsGrounded() уберем проверку условие на землю-получим множественные прыжки
                MyBody.AddForce(Vector3.up * JumpForce * 20, ForceMode.Impulse); // реализация прыжка
        }
    }
    // Короутин Стрельбы
    IEnumerator Fire()
    {
        if (CanAttack && CurMagazine > 0 && !Reload) // Проверка есть ли патроны в магазине, можем ли атаковать и не перезаряжаемся ли?
        {
            CanAttack = false; // больше не можем стрелять

            CurMagazine--; // отнимаем патрон из магазина

            DisMagazine.text = "Magazine: " + CurMagazine; // отображаем кол-ва патронов в магазине 

            GameObject MyBullet = Instantiate(Bullet) as GameObject; // ссылаемся на пулу и создаём её на сцене

            // Поворачиваем пулю
            MyBullet.transform.position = StartBullet.transform.position;
            MyBullet.transform.rotation = StartBullet.transform.rotation;

            // Если магазин пуст, перезаряжаемся
            if (CurMagazine <= 0)
            {
                StartCoroutine(StartReload()); // запуск короутина перезарядки
                Reload = true; // говорим что перезаряжаемся
            }

            yield return new WaitForSeconds(0.05f);  // ждём

            CanAttack = true; // можем снова выстрелить
        }
    }

    // Перезарядка
    IEnumerator StartReload()
    {
        yield return new WaitForSeconds(1f); // ждём

        if (Ammo > Magazine) // если патронов больше, чем в макс магазине
        {
            int Num = Magazine;      // доп локальная переменная
            Num = Num - CurMagazine; // отнимаем от макс магазина текущий магаз
            Ammo -= Num;             // отнимаем остаток у общего боезапаса
            CurMagazine = Magazine; // берём полный магазин
        }

        else if (Ammo > Magazine - CurMagazine)
        {
            int Num = Magazine;
            Num = Num - CurMagazine;
            Ammo -= Num;
            CurMagazine = Magazine;
        }

        else // если общий боезапас меньше текщуего магазина
        {
            CurMagazine = CurMagazine + Ammo; //  остатки в магазин
            Ammo = 0;
        }

        // канвас
        DisMagazine.text = "Magazine: " + CurMagazine;
        DisAmmo.text = "Ammo: " + Ammo;

        Reload = false; // конец перезарядки
    }

    
    public override void TakeDamage(int Damage)
    {
        Health -= Damage; // отнимаем жизни

        DisHealth.text = "Health: " + Health; // отображаем их

        if (Health <= 0) // если жизней меньше 0
        {
            Die(); // то вызываем метод смерти
        }
    }

    void Die() // смерть
    {
        IsAlive = false; // меняем на смерть

   
        Time.timeScale = 0; // Остановка игры
    }

    // проверка через рейкаст приземлены ли? прыжки
    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, GroundDis + 0.1f);
    }
}
