#pragma strict

public class AbilityType
{
	public var name : String;
	public var description : String;
	public var baseDamage : float;
	public var attackSpeed : float;
	public var effectRange : float;
	
	public var type : Type;
	public var friendlyFire : boolean;//Only affect enemies if set to FALSE
	
	public var effectArt_1 : GameObject;
	public var effectArt_1_point : EffectArtPoint;
	public var effectArt_1_dur : float;
}

enum Type
{
	point,
	unit,
	instant
}

enum EffectArtPoint
{
	point,
	user,
	target
}