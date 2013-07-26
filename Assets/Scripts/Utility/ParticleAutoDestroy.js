function Start () 
{
	var pSys = this.GetComponent(ParticleSystem);
	Destroy(this.gameObject, pSys.duration + 1);
}