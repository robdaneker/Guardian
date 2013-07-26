using UnityEngine;
using System.Collections;

public class ClickNavAgent_bot : MonoBehaviour 
{
	private Vector3 dir;
	private Vector3 k;
	private RaycastHit hit;
	private Ray ray;
	private float rot = 0f;
	private float maxSpeed = 5.0f;
	private float walkSpeed = 2.0f;
	private float speed = 0.0f;
	private float speedDamper = 0.2f;
	private float angle;
	private float dist;
	private GameObject pmp;
	[SerializeField]
	private GameObject pMovePoint;
	private int groundMask = (1 << 9);
	private int unitMask = (1 << 11); 
	public NavMeshAgent nAgent;
	private CamControllerClick gameCam;
	private Camera cam;
	private Animator animator;
	private GameController gameController;		// This script will not turn run until the gameController is set to true in the GameController
	private Vector3 destinationPosition;		// The destination Point
	private float destinationDistance;			// The distance between myTransform and destinationPosition
	private float directionDamper = 0.25f;
	private bool isActive = false;				//controller status
	private NetworkRigidbody networkSync;		//used for Networking players
	public Transform player;					//used to reference Player controller transform
	private bool inCombat;						//used to check Combat state
	[SerializeField]
	private AbilityType[] abilityList;			//.js Class which stores Attack info and parameters
	private AbilityType curAbility;


	public Camera Cam {
		get {
			return this.cam;
		}
		set {
			cam = value;
		}
	}	
	public CamControllerClick GameCam {
		get {
			return this.gameCam;
		}
		set {
			gameCam = value;
		}
	}	
	public GameController GameController {
		get {
			return this.gameController;
		}
		set {
			gameController = value;
		}
	}
	public bool IsActive {
		get {
			return this.isActive;
		}
		set {
			isActive = value;
		}
	}
	public float Speed {
		get {
			return this.speed;
		}
	}
	public Vector3 DestinationPosition {
		get {
			return this.destinationPosition;
		}
		set {
			destinationPosition = value;
		}
	}
	public AbilityType CurAbility {
		get {
			return this.curAbility;
		}
		set {
			curAbility = value;
		}
	}	
	
	
	void Awake () 
	{
		if(!networkView.isMine)
		{
			enabled = false;	
		}else{
			destinationPosition = transform.position;
			animator = transform.GetComponent<Animator>();
			nAgent = transform.GetComponent<NavMeshAgent>();
			if(animator.layerCount >= 2)
				animator.SetLayerWeight(1,1);
			if(this.gameObject.tag == "Player") //set the Player to the default controller at game start
				isActive = true;
			networkSync = transform.GetComponent<NetworkRigidbody>();
			//curAbility = abilityList[0];
		}
	}
	
	
	void Update ()
	{
		if(isActive && gameController)
		{
			nAgent.stoppingDistance = 0.1f;
			// Particle Art for Move Click
			if(Input.GetMouseButtonDown(1))
			{
				Ray pRay = cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit pHit;
		        Physics.Raycast(cam.transform.position,pRay.direction,out pHit,1000f,groundMask);
				Vector3 iPoint = pHit.point;
				iPoint.y = iPoint.y + 0.1f;
				if(pmp)
					DestroyImmediate(pmp);
				pmp = Instantiate(pMovePoint,iPoint,Quaternion.identity) as GameObject;
			}
			
			if (Input.GetMouseButton(1))
		    {
		        //Move Click
		        ray = cam.ScreenPointToRay(Input.mousePosition);
		        Physics.Raycast(cam.transform.position,ray.direction,out hit,1000f,groundMask);
				destinationPosition = hit.point;
		    }
		}else if(gameController && !isActive) /*&& put !inCombat condition here*/
		{
			destinationPosition = player.position; //Follow the bot controller
		}

		//Move to Destination:
		if(isActive)
		{
			nAgent.stoppingDistance = 0.1f;//Stop if active controller
		}else{
			nAgent.stoppingDistance = 2.3f;//Stop if inactive controller
		}
        dir = nAgent.pathEndPosition - transform.position;
		dist = nAgent.remainingDistance;
        // Get rotation smoothly & Animate:
        angle = Vector3.Angle(dir, transform.forward);
        k = Vector3.Cross(transform.forward, dir);
        k.Normalize();
        rot = k[1] * (angle);
		if(dist > 2)//Define walk & run parameters
			{nAgent.speed = maxSpeed;
		}else{
			nAgent.speed = walkSpeed;}
		nAgent.destination = destinationPosition;
		speed = dist;
		animator.SetFloat("Speed", speed);
		animator.SetFloat("Angle",rot);			
		animator.SetFloat("Direction",rot,directionDamper, Time.deltaTime);
		networkSync.setCharacter(ref speed,ref rot, ref rot);//Send animations over the network;
		
		//Stop Moving Function:
		float curDistance = nAgent.remainingDistance;
		if((isActive && curDistance<=0.1f) || (!isActive && curDistance<=2.1f))
		{
			speed = 0.1f;
			speed = Mathf.Lerp(speed,0f,speedDamper*Time.deltaTime);
			animator.SetFloat("Speed", speed);
			nAgent.speed = 0f;
		}
		
		//Ability init:
		if(curAbility!=null)
		{
			//Instant Ability call
			if(isActive && curAbility.type==Type.instant && (Input.GetButtonDown("Q")||Input.GetButtonDown("W")||Input.GetButtonDown("E")||Input.GetButtonDown("R")))
			{
				instantAbility(curAbility);
			}else
			//Point Ability call
			if(isActive && curAbility.type==Type.point && Input.GetMouseButtonUp(0))
			{
				Ray pRay = cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit pHit;
				Physics.Raycast(cam.transform.position,pRay.direction,out pHit,1000f,groundMask);
				pointAbility(curAbility,pHit.point,curAbility.effectRange,curAbility.baseDamage);
			}else
			//Unit Ability Call
			if(isActive && curAbility.type==Type.unit && Input.GetMouseButtonUp(0))
			{
				Ray uRay = cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit uHit;
				if(Physics.Raycast(cam.transform.position,uRay.direction,out uHit,1000f,unitMask))
					{unitAbility(curAbility,uHit.collider.gameObject,curAbility.baseDamage);
					}else{Debug.Log ("Must Target a unit!");}
			}
		}
	}
	

	//Instant Ability Functionality
	private void instantAbility(AbilityType ability)
	{
		curAbility = null;
		Debug.Log("Successful Instant Attack!");
	}
	
	//Point Ability Functionality
	private void pointAbility(AbilityType ability,Vector3 target,float radius,float baseDamage)
	{
		Collider[] effectedC= Physics.OverlapSphere(target,radius);
		int gO = 0;
		while(gO<effectedC.Length)
		{
			if(!ability.friendlyFire)
			{
				if(effectedC[gO].gameObject.tag=="Enemy")
					effectedC[gO].gameObject.GetComponent<UnitData>().networkView.RPC("ApplyDamage",RPCMode.AllBuffered,baseDamage);
			}else{
				if(effectedC[gO].gameObject.CompareTag("Enemy") || effectedC[gO].gameObject.CompareTag("Bot"))
					effectedC[gO].gameObject.GetComponent<UnitData>().networkView.RPC("ApplyDamage",RPCMode.AllBuffered,baseDamage);
			}
			gO++;
		}
		DestroyImmediate(gameController.curCursorArt);
		curAbility = null;
		Debug.Log("Successful Point Attack!");
	}
	
	//Unit Ability Functionality
	private void unitAbility(AbilityType ability,GameObject target,float baseDamage)
	{
		if(!ability.friendlyFire)
		{
			if(target.tag=="Enemy")
					target.GetComponent<UnitData>().networkView.RPC("ApplyDamage",RPCMode.AllBuffered,baseDamage);
		}else{
			if(target.CompareTag("Enemy") || target.CompareTag("Bot"))
					target.GetComponent<UnitData>().networkView.RPC("ApplyDamage",RPCMode.AllBuffered,baseDamage);
		}
		curAbility = null;
		Debug.Log("Successful Unit Attack!");
	}


	void OnGUI()
	{
		if(isActive)
		{
			//Get which Ability key was pressed
			Event keyPress = Event.current;
			if(keyPress.keyCode == KeyCode.Q){
				curAbility = abilityList[0];
			}else if(keyPress.keyCode == KeyCode.W){
				curAbility = abilityList[1];
			}else if(keyPress.keyCode == KeyCode.E){
				curAbility = abilityList[2];
			}else if(keyPress.keyCode == KeyCode.R){
				curAbility = abilityList[3];
			}
			if(curAbility!=null && curAbility.type == Type.point && gameController.curCursorArt==null)
				{gameController.curCursorArt = Instantiate(gameController.cursorArtList[0],Vector3.zero,Quaternion.identity) as GameObject;
				//}else if   ...Put other cursor art events here.
				}
		}
	}
}