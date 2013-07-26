using UnityEngine;
using System.Collections;

public class ClickNavAgent : MonoBehaviour 
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
	private int layerMask = 1 << 9; 
	public NavMeshAgent nAgent;
	private CamControllerClick gameCam;
	private Camera cam;
	private Animator animator;
	private GameController gameController;		// This script will not turn run until the gameController is set to true in the GameController
	private Vector3 destinationPosition;		// The destination Point
	private float destinationDistance;			// The distance between myTransform and destinationPosition
	private float curHealth;
	[SerializeField]
	private float maxHealth;
	private float directionDamper = 0.25f;
	private bool isActive = false;				//controller status
	private NetworkRigidbody networkSync;		//used for Networking players
	public Transform bot;

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
		}
	}
	
	
	void Update ()
	{
		if(isActive && gameController)
		{
			nAgent.stoppingDistance = 0.1f;
			// Particle Art
			if(Input.GetMouseButtonDown(1))
			{
				Ray pRay = cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit pHit;
		        Physics.Raycast(cam.transform.position,pRay.direction,out pHit,1000f,layerMask);
				Vector3 iPoint = pHit.point;
				iPoint.y = iPoint.y + 0.1f;
				if(pmp)
					DestroyImmediate(pmp);
				pmp = Instantiate(pMovePoint,iPoint,Quaternion.identity) as GameObject;
			}
			
			if (Input.GetMouseButton(1))
		    {
		        // Cast ray from mouse to point on terrain and get location
		        ray = cam.ScreenPointToRay(Input.mousePosition);
		        Physics.Raycast(cam.transform.position,ray.direction,out hit,1000f,layerMask);
				destinationPosition = hit.point;
		    }
		}else if(gameController && !isActive) /*&& put !inCombat condition here*/
		{
			destinationPosition = bot.position; //Follow the bot controller
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
		if(dist > 2)//Dedfine walk & run parameters
			{nAgent.speed = maxSpeed;
		}else{
			nAgent.speed = walkSpeed;}
		nAgent.destination = destinationPosition;
		speed = dist;
		animator.SetFloat("Speed", speed);
		animator.SetFloat("Angle",rot);			
		if(speed > 0.05f && (rot>=135f || rot<= -135f) && !animator.GetBool("Pivot"))
			{animator.SetBool("Pivot",true);
		}else{
			animator.SetBool("Pivot",false);}
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
	}	
}