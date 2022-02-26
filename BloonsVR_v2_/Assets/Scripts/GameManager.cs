using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Static class to get GameManager everywhere
/// </summary>
internal static class GetGameManager
{
    /// <summary>
    /// Current game manager
    /// </summary>
    internal static GameManager gameManager;
}

/// <summary>
/// All states possible for the game
/// </summary>
public enum GameStatus { Loading, Menu, Pause, InWave, EndWave, EndGame };

public class GameManager : MonoBehaviour
{
    [Tooltip("Current gameStatus")]
    private GameStatus gameStatus;

    [Tooltip("All ennemies spawned in game")]
    private List<GameObject> EnnemiesInWave = new List<GameObject>();

    [Tooltip("All turrets spawned in game")]
    private List<GameObject> TurretsInGame = new List<GameObject>();

    [Tooltip("Prefab of a turret")]
    [SerializeField] 
    private GameObject TurretPrefab;

    [Tooltip("Prefab of an ennemy")]
    [SerializeField] 
    private GameObject EnnemyPrefab;

    [Tooltip("Where an ennemy will spawn")]
    [SerializeField] 
    private Transform EnnemySpawnPosition;

    [Tooltip("Where an ennemy will go")]
    [SerializeField] 
    private Transform EnnemyDestinationPosition;

    [Tooltip("Player Money")]
    private float TotalMoney = 0;

    [Tooltip("Amount of hitpoints before GameOver")]
    private int HitPoints = 100;
    private int MaxHitPoints = 100;

    [Tooltip("Barre de vie")]
    private Slider slider;

    [Tooltip("Current wave")]
    private int Wave;

    //ennemies infos
    private int BaseHP;
    private float BaseSpeed;
    private int BaseDamage;
    private int BaseMoney;


    private void Awake()
    {
        gameStatus = GameStatus.Loading;
        slider = EnnemyDestinationPosition.gameObject.GetComponentInChildren<Transform>().GetComponentInChildren<Slider>();
        if (slider == null) throw new Exception("Put a canvas and a slider as children of base.");
    }

    void Start()
    {
        GetGameManager.gameManager = this;
        InitValues();
        gameStatus = GameStatus.Menu;
    }

    /// <summary>
    /// Function called when an ennemy die
    /// </summary>
    /// <param name="ennemy">Ennemy who die</param>
    /// <param name="cashGiven">Cash given by the ennemy</param>
    internal void OnEnnemyDeath(GameObject ennemy, int cashGiven)
    {
        TotalMoney += cashGiven;
        print("Get cash : " + TotalMoney);
        EnnemiesInWave.Remove(ennemy);
        Destroy(ennemy);
    }

    /// <summary>
    /// Function used to spawn an ennemy on their location
    /// </summary>
    private void SpawnEnnemy(int hitpoints = 100, int damageOnBase = 10, float speed = 5f, int cash = 25)
    {
        print("Spawn "+ EnnemyDestinationPosition.position);
        var Ennemy = Instantiate<GameObject>(EnnemyPrefab, EnnemySpawnPosition.position, Quaternion.identity);
        EnnemiesInWave.Add(Ennemy);
        var ennemyScript = Ennemy.GetComponent<EnnemyScript>();
        ennemyScript.InitValues(EnnemyDestinationPosition.position, hitpoints, damageOnBase, speed, cash);
    }

    /// <summary>
    /// Function called when an ennemy reached the base
    /// </summary>
    /// <param name="ennemy">Ennemy who reached the base</param>
    /// <param name="damage">Amount of damage of the ennemy</param>
    internal void EnnemyReachedDestination(GameObject ennemy, int damage)
    {
        EnnemiesInWave.Remove(ennemy);
        Destroy(ennemy);
        HitPoints -= damage;
        if (HitPoints <= 0)
        {
            HitPoints = 0;
            if (gameStatus != GameStatus.EndGame)
            {
                gameStatus = GameStatus.EndGame;
                ClearMap();
            }
        }
        slider.value = Math.Abs(HitPoints);
        print("Damaged  base  !! HP restants : " + HitPoints);
        //Check End WAve
    }

    /// <summary>
    /// Remove all items from the map
    /// </summary>
    private void ClearMap()
    {
        foreach (var item in EnnemiesInWave)
        {
            Destroy(item);
        }
        EnnemiesInWave.Clear();

        foreach (var item in TurretsInGame)
        {
            Destroy(item);
        }
        TurretsInGame.Clear();
    }

    /// <summary>
    /// Prepare une nouvelle party si une n'est pas deja en cours
    /// </summary>
    /// <param name="difficulty"></param>
    public void BeginGame(int difficulty = 1)
    {
        if (gameStatus != GameStatus.Menu) return;
        ClearMap();
        InitValues(); 
        BaseSpeed = 5.0f;
        BaseHP = 100;
        BaseDamage = 10;
        BaseMoney = 10;
        TotalMoney = 100; //pour acheter 1 tourelle
        Wave = 0;
        StartCoroutine(LetsPlay());
    }

    /// <summary>
    /// Lance le processus de gameplay
    /// </summary>
    /// <returns></returns>
    private IEnumerator LetsPlay()
    {
        while (gameStatus != GameStatus.EndGame) //tant qu on est en jeu 
        {
            gameStatus = GameStatus.EndWave; //chargement vague suivante
            Wave++;
            if (Wave % 4 == 0) // toutes les 4 waves augmenter un peu la puissance des ballons
            {
                BaseSpeed += 1.0f;
                BaseHP += 20;
                BaseDamage += 2;
                BaseMoney += 5;
            }
            for (int i = 10; i > 0; i--)
            {
                if (GameStatus.Pause == gameStatus) i++;
                //peut ajouter possibilite de quitter le jeu quand le jeu est en pause
                print("Next wave in " + i);
                yield return new WaitForSeconds(1);
            }
            gameStatus = GameStatus.InWave;
            print("Wave " + Wave);
            yield return new WaitForSeconds(1);

            //Debut vague suivante
            var nBennemies = Wave * 2 + 3;
            for (int i = 0; i < nBennemies; i++)
            {
                 if (Wave < 3 ) yield return new WaitForSeconds(UnityEngine.Random.Range(2, 4) );
                 else yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 2));
                SpawnEnnemy(BaseHP, BaseDamage, BaseSpeed, BaseMoney);
            }

            while (gameStatus != GameStatus.EndGame && EnnemiesInWave.Count > 0)
            {
                yield return new WaitForSeconds(2.0f);
            }
            print("End of wave " + Wave);
            yield return new WaitForSeconds(2.0f);
        }
        print("Game Over !\nScore :" + Wave);
        gameStatus = GameStatus.Menu;
    }

    /// <summary>
    /// Mettre en pause le jeu au moment du chargement de la prochaine vague ? A voir si on garde 
    /// </summary>
    public void PauseGame()
    {
        if (gameStatus == GameStatus.EndWave) gameStatus = GameStatus.Pause;
        else if (gameStatus == GameStatus.Pause) gameStatus = GameStatus.EndWave;
    }

    /// <summary>
    /// Reset les valeurs de vie de la base
    /// </summary>
    private void InitValues()
    {
        HitPoints = 100;
        MaxHitPoints = 100;
        slider.maxValue = MaxHitPoints;
        slider.value = MaxHitPoints;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) PauseGame(); 
        if (Input.GetKeyDown(KeyCode.P)) BeginGame(); 
        if (Input.GetKeyDown(KeyCode.T)) SpawnTurret(); 
    }


    /// <summary>
    /// Fais apparaitre une tourelle si on a assez d'argent
    /// </summary>
    private void SpawnTurret()
    {
        if (TotalMoney < 100)
        {
            print("Need more cash");
            return;
        }
        TotalMoney -= 100;
        print("Buy turet");
        var turret = Instantiate<GameObject>(TurretPrefab, GameObject.Find("Player").transform.position, Quaternion.identity);
        TurretsInGame.Add(turret);
        var turretScript = turret.GetComponent<TurretScript>();
        turretScript.InitValues();
    }
}
