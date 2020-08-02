using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 6f;
    [SerializeField] private float dashDistance = 5f; 
    [SerializeField] private bool rangeWeaponIsActive = false;
    [SerializeField] private float attackCooldown = 3f;
    private float cooldownTimer = attackCooldown;
    private Vector2 movePlayer = Vector2.zero;

    public Animator PlayerAnimation;
    public Transform Player;
    public GameObject MeleeProjectile; 
    public GameObject RangeProjectile;
    public GameObject MeleeAttackButon;
    public GameObject RangeAttackButon;   
    public GameObject PauseMenu;
    public Text PauseText;

    public AudioSource MeleeAttackSound;
    public AudioSource RangeAttackSound;
    public AudioSource WalkSound;
    public AudioSource SelectWeapon;
    public AudioSource DeathSound;


    private void Awake()
    {
        MeleeAttackButon.SetActive(true);
        RangeAttackButon.SetActive(false);      
        PlayerAnimation = GetComponent<Animator>();
        UnpauseTheGame();
    }


    private void Update()
    {
        movePlayer = Joystick.GetJoystickInput();

        if (movePlayer != Vector2.zero)
        {
            PlayerAnimation.SetBool("IsWalking", true);
            Player.position = Vector2.MoveTowards(Player.position, new Vector2(Player.position.x + movePlayer.x,
                Player.position.y + movePlayer.y), movementSpeed * Time.deltaTime);

            if (!WalkSound.isPlaying)
            {
                WalkSound.Play();
            }
        }
        else
        {
            PlayerAnimation.SetBool("IsWalking", false);
        }

        if (cooldownTimer <= attackCooldown)
        {
            cooldownTimer += Time.deltaTime;
        }
    }


    public void PauseTheGame()
    {       
        Time.timeScale = 0;
        PauseText.text = "Pause";
        PauseMenu.SetActive(true);
        SaveHandler.Save();
    }


    public void UnpauseTheGame()
    {      
        Time.timeScale = 1f;        
        PauseMenu.SetActive(false);        
    }


    private void ShowDeathMenu()
    {
        Time.timeScale = 0;
        PauseText.text = "You Are Dead";
        DeathSound.Play();
        PauseMenu.SetActive(true);      
    }


    [SerializeField]
    public void PerformAttack(string direction) // input string format: "x,y"
    {                                           // where valid x/y values are: -1, 0, 1
        if (cooldownTimer >= attackCooldown)
        {
            cooldownTimer = 0;
            string[] splitDirections = direction.Split(',');
            int directionX = int.Parse(splitDirections[0]);
            int directionY = int.Parse(splitDirections[1]);

            if (RangeWeaponIsActive == false)
            {               
                Player.position = new Vector2(Player.position.x + dashDistance * directionX, Player.position.y + dashDistance * directionY);             
                SpawnMeleeProjectile(directionX, directionY);
            }
            else
            {             
                SpawnRangeProjectile();
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "EnemyProjectile")
        {
            Data.ChangePlayerHP();
            int currentHp = Data.GetCurrentHP();
            SaveHandler.Save();

            if (currentHp <= 0)
            {
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(obj);
                foreach (GameObject obj in GameObject.FindGameObjectsWithTag("EnemyProjectile")) Destroy(obj);
                Player.position = Vector2.zero;
                SaveHandler.SaveDefaultValues();
                SaveHandler.LoadAll();               
                ShowDeathMenu();
            }
        }
    }


    private void SpawnMeleeProjectile(int directionX, int directionY) //spawn a melee 'attack'
    {       
        MeleeAttackSound.Play();
        Instantiate(MeleeProjectile, new Vector2(Player.position.x - directionX * 0.5f, Player.position.y - directionY * 0.5f), Quaternion.identity);
        PlayerAnimation.SetTrigger("MelleAttack");        
    }


    private void SpawnRangeProjectile() //spawn a melee 'attack'
    {
        int ammunition = Data.GetAmmunition();

        if (ammunition > 0)
        {          
            RangeAttackSound.Play();
            Instantiate(RangeProjectile, Player.position, Quaternion.identity);
            PlayerAnimation.SetTrigger("RangeAttack");
            Data.ChangeCurrentAmmunition(-1);
            saveHandler.save();
        }
        else
        {
            cooldownTimer = attackCooldown;
        }
    }
  

    public void ChangeWeapon()
    {
        if (RangeWeaponIsActive == false) 
        {
            SelectWeapon.Play();
            rangeWeaponIsActive = true; 
            MeleeAttackButon.SetActive(false);
            RangeAttackButon.SetActive(true);
        }
        else 
        {
            SelectWeapon.Play();
            rangeWeaponIsActive = false;
            MeleeAttackButon.SetActive(true);
            RangeAttackButon.SetActive(false);
        }      
    }
}
