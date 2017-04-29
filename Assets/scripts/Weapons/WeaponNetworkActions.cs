﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Weapons;
using PlayGroup;

public class WeaponNetworkActions : NetworkBehaviour {

	private GameObject spritesObj;
	private PlayerSprites playerSprites;
	private PlayerMove playerMove;
	private SoundNetworkActions soundNetworkActions;

	//Lerp parameters
	private float lerpProgress = 0f;
	private bool lerping = false;
	private Vector3 lerpFrom;
	private Vector3 lerpTo;
	private float speed = 7f;
	private bool isForLerpBack = false;

	void Start(){
		spritesObj = transform.Find("Sprites").gameObject;
		playerSprites = GetComponent<PlayerSprites>();
		playerMove = GetComponent<PlayerMove>();
		soundNetworkActions = GetComponent<SoundNetworkActions>();
	}

	[Command]
	public void CmdLoadMagazine(GameObject weapon, GameObject magazine){
		if (!playerMove.allowInput)
			return;
		
		Weapon_Ballistic w = weapon.GetComponent<Weapon_Ballistic>();
		NetworkInstanceId nID = magazine.GetComponent<NetworkIdentity>().netId;
		w.magNetID = nID;
	}

	[Command]
	public void CmdUnloadWeapon(GameObject weapon){
		if (!playerMove.allowInput)
			return;
		
		Weapon_Ballistic w = weapon.GetComponent<Weapon_Ballistic>();
		NetworkInstanceId newID = NetworkInstanceId.Invalid;
		w.magNetID = newID;
	}

	[Command]
	public void CmdShootBullet(Vector2 direction, string bulletName){
		if (!playerMove.allowInput)
			return;
		
		GameObject bullet = GameObject.Instantiate(Resources.Load(bulletName) as GameObject,transform.position, Quaternion.identity);
		NetworkServer.Spawn(bullet);
		var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		BulletBehaviour b = bullet.GetComponent<BulletBehaviour>();
		b.Shoot(direction, angle);

	}
		
	[Command]
	public void CmdKnifeAttackMob(GameObject npcObj, Vector2 stabDirection)
	{
		if (!playerMove.allowInput)
			return;
		
		Living attackTarget = npcObj.GetComponent<Living>();
		RpcKnifeAttackLerp(stabDirection);
		attackTarget.RpcReceiveDamage();
		soundNetworkActions.RpcPlayNetworkSound("BladeSlice", transform.position);
	}

	[Command]
	public void CmdKnifeHarvestMob(GameObject npcObj, Vector2 stabDirection)
	{
		if (!playerMove.allowInput)
			return;
		
		Living attackTarget = npcObj.GetComponent<Living>();
		RpcKnifeAttackLerp(stabDirection);
		attackTarget.HarvestIt();
		soundNetworkActions.RpcPlayNetworkSound("BladeSlice", transform.position);
	}

	[ClientRpc]
	void RpcKnifeAttackLerp(Vector2 stabDir){
		if (lerping)
			return;

		PlayerManager.LocalPlayerScript.hitIcon.ShowHitIcon(stabDir);

		lerpFrom = transform.position;
		Vector3 newDir = stabDir * 0.5f;
		newDir.z = lerpFrom.z;
		lerpTo = lerpFrom + newDir;
		lerpProgress = 0f;
		isForLerpBack = true;
		lerping = true;
	}

	//Server lerps
	void Update(){
		if (lerping) {
			lerpProgress += Time.deltaTime;
			spritesObj.transform.position = Vector3.Lerp(lerpFrom, lerpTo, lerpProgress * speed);
			if (spritesObj.transform.position == lerpTo || lerpProgress > 2f) {
				if (!isForLerpBack) {
					ResetLerp();
					spritesObj.transform.localPosition = Vector3.zero;
				} else {
					//To lerp back from knife attack
					ResetLerp();
					lerpTo = lerpFrom;
					lerpFrom = spritesObj.transform.position;
					lerping = true;
				}
			}
		}
	}
	void ResetLerp(){
		lerpProgress = 0f;
		lerping = false;
		isForLerpBack = false;
	}
}
