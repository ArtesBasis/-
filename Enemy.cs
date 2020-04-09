using UnityEngine;
using System.Collections;

public class Enemy : Playable
{
    public Transform Target; // Цель
    public Playable MyEnemy; // Ссылка на объкт противника для оптимизации

    public float Speed, TimeCast; // Скорость передвижения и время для выстрела
    public float MinDistance, AttackDistance; // Минмальная дистанция для приближения к цели, дистанция начала атаки 

  //  public int Health;
   // public int Damage;
    public GameObject StartBullet, Bullet; // Точка создания пули и ссылка на префаб пули
    Rigidbody MyBody;      // мой физ тело
    Transform MyTransform; // моя точка и поврот в пространстве 

    public bool Agressive = false; // Сагрился ли
    public bool Couldown = false;  // Откат после выстрела
    //public bool IsAlive = true;

    public UnityEngine.AI.NavMeshAgent MyAgent;      // Ссылка на NavMeshAgent
    public GameObject[] PathsMassive; // Массив на точки перемещения
    public GameObject MyWay;          // Текущая точка к которой двигаемся

    // Use this for initialization
    void Start()
    {
        MyAgent.speed = Speed; // Указываем скорость агента
        MyBody = GetComponent<Rigidbody>(); // создание ссылка на тело
        MyTransform = transform;              // получение своих координат
        PathsMassive = GameObject.FindGameObjectsWithTag("Paths"); // нахождение точек перемещения

        //Target = GameObject.FindGameObjectWithTag("Player").transform;
        // if (IsAlive && MyAgent.enabled == true)
        //    MyAgent.SetDestination(Target.transform.position);
    }

    void Update()
    {
        if (IsAlive) // Если пешка жива
        {
            ChekEnemies(); // Поиск противников
        }        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsAlive) // Если пешка жива
        {
            //Если есть какая-то цель в радиуса обзора
            if (Target != null)
            {
                if (!MyEnemy.IsAlive) // И если эта цель не мертва
                {
                    // Если цель мертва то мы её удаляем из актуальной цели
                    Target = null; 
                    MyEnemy = null;
                    return; // Прекращаем выполенине метода
                }

                if (Vector3.Distance(MyTransform.position, Target.position) <= ViewRadius) // Если расстояние между пешкой и противником меньше или равна ViewRadius
                {
                    MyAgent.SetDestination(Target.transform.position); // Направляемся в сторону противника               

                    if (Vector3.Distance(MyTransform.position, Target.position) <= AttackDistance) // Если мы достигаем расстояния стрельбы
                    {
                        if (!Couldown) // Если у нас не откат
                        {
                            Couldown = true; // У нас откат
                            
                            StartCoroutine(Attack()); // И начинаем стрельбу
                        }
                    }

                    if (Vector3.Distance(MyTransform.position, Target.position) < MinDistance) // если дистанция меньше минмальной
                        MyAgent.SetDestination(transform.position); // останавливаем наш агент в текущей точке
                }

             
            }

            //Если у нас нет цели и текщуая точка пуста
            else if (MyWay == null)
            {
                NewWay(); // назначаем новую точку
                MyAgent.SetDestination(MyWay.transform.position); // и двигаемся к ней
            }

            else if(Vector3.Distance(MyTransform.position, MyWay.transform.position) < 2) // Если расстояние до пункта меньше двух юнитов
            {
                NewWay(); // назначаем новую точку
                MyAgent.SetDestination(MyWay.transform.position); // и двигаемся к ней
            }
        }
    }

    public void NewWay() // Поиск новой точки пути
    {       
           MyWay = PathsMassive[Random.Range(0, PathsMassive.Length - 2)]; // Получаем сдучайную точку перемещения
    }

    // короутин стрельбы
    IEnumerator Attack()
    {
        yield return new WaitForSeconds(TimeCast); // ждём

        for (int i = 0; i < 2; i++) // цикл стрельбой очередью из 3 патронов
        {
            yield return new WaitForSeconds(0.1f); // ждём

            if (Target != null) // если цель ещё актуальна
            {
                if (Vector3.Distance(MyTransform.position, Target.position) < AttackDistance) // не убежала ли цель
                {
                   GameObject MyBullet = Instantiate(Bullet, StartBullet.transform.position, StartBullet.transform.rotation) as GameObject; // выплёвываем пулю
                    MyBullet.transform.LookAt(Target); // и поворачиваем в строну противника
                }
            }
        }

        Couldown = false; // закончили откат
    }

    /*
    public void TakeDamage(int Damage, GameObject Agressor)
    {
        if (IsAlive)
        {
            Health -= Damage;

            // Debug.Log(Agressor);

            if (Health <= 0)
            {
                Die();
            }

            StartCoroutine(BeAgressive(Agressor));
        }
    }
    */

    //Получение урона
    public override void TakeDamage(int Damage)
    {
        if (IsAlive) // Если мы живы
        {
            Health -= Damage; // отнимаем жизни

            // Debug.Log(Agressor);

            if (Health <= 0) // если жизней меньше 1
            {
                Die(); // умираем
            }

           // StartCoroutine(BeAgressive(Agressor));
        }
    }

    public void Die() // метод смерти
    {
        IsAlive = false;    // Не живой
        Agressive = false;  // не агрессивный
        Target = null;       // не имеющий цели
        MyAgent.SetDestination(transform.position); // Стоящий на месте
        MyAgent.enabled = false; // сы выключенным агентом
        MyBody.constraints = RigidbodyConstraints.None; // С физикой падения 
        GetComponent<MeshRenderer>().material.color = Color.red; // И покрашеный в красный цвет

        gameObject.layer = 0; // Перемещаем на слой мертвеца
    }

    /*
    IEnumerator BeAgressive(GameObject Agressor)
    {
        Agressive = true;
        Target = Agressor.transform;

        yield return new WaitForSeconds(20);

        Agressive = false;
    }
    */

    public void ChekEnemies() // Поиск противников
    {
        TargetsInRadius = Physics.OverlapSphere(transform.position, ViewRadius, EnemyLayer); // Получаем все коолайдер на наших масках

        if (TargetsInRadius.Length != 0) // Если в диапазоне больше чем 0 целей
        {
            Target = TargetsInRadius[0].transform; // Хватаем первого попавшегося противника
            MyEnemy = Target.GetComponent<Playable>(); // Делаем ссылку на его Playable
        }

       
    }
}
