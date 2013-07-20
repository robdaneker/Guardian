using UnityEngine;
using System.Collections;

struct CameraPosition
{
	private Vector3 position;
	private Transform xForm;
	public Vector3 Position { get { return position; } set { position = value; } }
	public Transform XForm { get { return xForm; } set { xForm = value; } }

	public void Init(string camName, Vector3 pos, Transform transform, Transform parent)
	{
		position = pos;
		xForm = transform;
		xForm.name = camName;
		xForm.parent = parent;
		xForm.localPosition = Vector3.zero;
		xForm.localPosition = position;
	}
}


public class CameraController : MonoBehaviour {
	
	[SerializeField]
	private CharacterController controller;
	[SerializeField]
	private Transform parentRig;
	[SerializeField]
	private float distance;
	[SerializeField]
	private float height;
	[SerializeField]
	private float smooth;
	[SerializeField]
	private float camSmoothDamper = 0.1f;
	[SerializeField]
	private float widescreen = 0.2f;
	[SerializeField]
	private float targetTime= 0.5f;
	[SerializeField]
	private float distanceMulitplier = 1.5f;
	[SerializeField]
	private float heightMultiplier = 5f;
	[SerializeField]
	private Vector2 distanceMin = new Vector2(1f,-0.5f);
	[SerializeField]
	private float lookSpeed = 3.0f;
	[SerializeField]
	private float mouseThreshold = 0.1f;
	[SerializeField]
	private const float freeRotationDegreePerSecond = -5f;
	//Private Globals
	private Vector3 lookDir;
	private Transform cameraOrigin;
	private Vector3 targetPosition;
	private Vector3 velocityCamSmooth = Vector3.zero;
	private BarsEffect letterBox;
	private CameraModes camState = CameraModes.Behind;
	private float lookWeight;
	private Vector3 savedRig;
	private float distanceFree;
	private float heightFree;
	private Vector2 inputOld = Vector2.zero;
	
	

	public CameraModes CamState {
		get {
			return this.camState;
		}
	}
	public Transform ParentRig
	{
		get
		{
			return this.parentRig;
		}
	}
	
	public enum CameraModes
	{
		Behind,
		Target,
		Free
	}
	
	
	void Start () 
	{
		parentRig = this.transform.parent;
		cameraOrigin = GameObject.FindGameObjectWithTag("Player").transform;
		lookDir = cameraOrigin.forward;
		letterBox = GetComponent<BarsEffect>();	
	}
	
	
	void LateUpdate () 
	{
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");
		Vector3 characterOffset = cameraOrigin.position + new Vector3(0f,height,0f);
		Vector3 lookAt = characterOffset;
		Vector3 targetPosition = Vector3.zero;
		//Determine Camera Mode
		if(Input.GetAxis("Target") > 0.01f)
		{
			letterBox.coverage = Mathf.SmoothStep(letterBox.coverage, widescreen, targetTime);
			camState = CameraModes.Target;
		}else{
			letterBox.coverage = Mathf.SmoothStep(letterBox.coverage, 0f, targetTime);
			if(Input.GetButton("Fire1") && System.Math.Round (controller.Speed, 2) == 0)
				camState = CameraModes.Free;
			if((camState==CameraModes.Free && !Input.GetButton("Fire1")) || (camState == CameraModes.Target && (Input.GetAxis ("Target")<=0.01)))
				camState = CameraModes.Behind;
		}

		//Execute Camera State
		switch (camState)
		{
			case CameraModes.Behind:
				//Calculate direction from camera to Player
				lookDir = characterOffset - this.transform.position;
				lookDir.y = 0;
				lookDir.Normalize();
				//Set Camera Position based on Camera Origin transform
				targetPosition = characterOffset + (cameraOrigin.up * height) - (lookDir * distance);
				break;
			case CameraModes.Target:
				lookDir = cameraOrigin.forward;
				targetPosition = characterOffset + (cameraOrigin.up * height) - (lookDir * distance);
				break;
			case CameraModes.Free:
				lookWeight = Mathf.Lerp (lookWeight,0.0f,Time.deltaTime*lookSpeed);
				Vector3 rigToGoalDirection = Vector3.Normalize(characterOffset - this.transform.position);
				rigToGoalDirection.y = 0f;
				Vector3 rigToGoal = characterOffset - parentRig.position;
				rigToGoal.y = 0f;
				//Pan Camera on Mouse Axis
				if((mouseY < -1f * mouseThreshold) && (mouseY<=inputOld.y) && (Mathf.Abs(mouseX) < mouseThreshold))
				{
					heightFree = Mathf.Lerp(height,height*heightMultiplier,Mathf.Abs(mouseY));
					distanceFree = Mathf.Lerp(distance,distance*distanceMulitplier,Mathf.Abs(mouseY));
					targetPosition = characterOffset + (cameraOrigin.up * heightFree) - (rigToGoalDirection * distanceFree);
				}else if((mouseY > mouseThreshold) && (mouseY>=inputOld.y) && (Mathf.Abs(mouseX)<mouseThreshold)){
					heightFree = Mathf.Lerp(Mathf.Abs(transform.position.y - characterOffset.y),distanceMin.y,mouseY);
					distanceFree = Mathf.Lerp(rigToGoal.magnitude,distanceMin.x,mouseY);
					targetPosition = characterOffset + (cameraOrigin.up * heightFree) - (rigToGoalDirection * distanceFree);
				}
				if(mouseX != 0 || mouseY != 0)
					savedRig = rigToGoalDirection;
				//Rotate Camera around Player
				parentRig.RotateAround(characterOffset,cameraOrigin.up,(freeRotationDegreePerSecond*(Mathf.Abs(mouseX)>mouseThreshold ? mouseX : 0f)));
				if(targetPosition == Vector3.zero)
					targetPosition = characterOffset + (cameraOrigin.up * heightFree) - (savedRig * distanceFree);
				break;
		}
		//Smooth its Position & Look At the Camera Origin after checking for Colliders
		cameraCollision(characterOffset, ref targetPosition);
		smoothPosition(this.transform.position,targetPosition);
		transform.LookAt(cameraOrigin);
		inputOld = new Vector2(mouseX,mouseY);
	}
	
	
	private void smoothPosition(Vector3 fromPos, Vector3 toPos)
	{
		this.transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDamper);
	}
	
	
	private void cameraCollision(Vector3 fromObject, ref Vector3 toTarget)
	{
		RaycastHit hit = new RaycastHit();
		LayerMask mask = (1<<8);
		if(Physics.Linecast(fromObject,toTarget, out hit, mask))
		{
			Vector3 offset = new Vector3(0,0,1);
			Vector3 worldOffset = this.transform.rotation * offset;
			toTarget = new Vector3(hit.point.x + worldOffset.x, toTarget.y, hit.point.z + worldOffset.z);	
		}
	}
	
	 public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}