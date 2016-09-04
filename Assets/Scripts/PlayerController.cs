﻿using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof(Custom2dPhysics))]
public class PlayerController : MonoBehaviour {
	CameraController cameraController;
	AudioSource audioSource;
	ParticleSystem particleSystem;
	SpriteRenderer spriteRenderer;
	Custom2dPhysics movement;

	public AudioClip[] soundEffects; // land, shoot, explosion
	public GameObject gunPrefab;
	Sprite[] sprites; //alive, dead, flying

	public Action destroyEvent;
	public Action hitEvent;
	public Action getPointEvent;
	public Action respawnEvent;

	public bool facingLeft = false;

	//rules
	public int score;
	public bool hasControl, isAlive, canMove;

	//Based on player number / settings
	public Color m_playerColor;
	
	string horizontalAxis, verticalAxis; // controls
	const float moveForce = 0.02f;	//how much you can move on the x

	public void Setup (string tag, string m_horizontalAxis, string m_verticalAxis, string fireButton, Sprite[] m_sprites, Color m_playerColor) {
		//Componant / GameObject references
		movement = GetComponent<Custom2dPhysics>();
		cameraController = Camera.main.GetComponent<CameraController>();
		audioSource = GetComponent<AudioSource>();
		particleSystem = GetComponent<ParticleSystem>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		FindObjectOfType<GameManagerScript>().startGameEvent += Respawn;
		destroyEvent += Destroy;
		//setup
		this.tag = tag;
		this.horizontalAxis = m_horizontalAxis;
		this.verticalAxis = m_verticalAxis;
		this.sprites = m_sprites;
		this.m_playerColor = m_playerColor;
		particleSystem.startColor = m_playerColor;
		(Instantiate(gunPrefab) as GameObject).GetComponent<GunController>().Setup(gameObject, fireButton, m_playerColor);//Gun Setup 
		score = 0;
	}

	void FixedUpdate () {
		X();
		Y();
		if (Input.GetAxisRaw(horizontalAxis) != 0) facingLeft = Input.GetAxisRaw(horizontalAxis) < 0;
		spriteRenderer.flipX = facingLeft;
		GetComponent<SpriteRenderer>().sprite = sprites[0]; // keep without method
	}

	void X () {
		float x = Input.GetAxisRaw(horizontalAxis);

		if (x != 0) {
			movement.AddAcceleration(new Vector3(Mathf.Sin(x) * moveForce,0,0));
		}
	}

	void Y () {
		bool y = Input.GetButtonDown(verticalAxis);

		if (y && movement.IsOnGround())
			movement.AddVelocity(Vector3.up * 0.25f);
	}

	public void Respawn () {
		hasControl = true;
		isAlive = true;
		canMove = true;
		movement.Stop();
		movement.SetPosition (GameObject.FindWithTag ("GameController").GetComponent<GameManagerScript> ().GetNextRespawnPos ());
		if (respawnEvent != null) respawnEvent();
		//TODO maybe respawn effects?
	}

	public void Hit () {
		if (hitEvent != null) hitEvent();
		StartCoroutine("HitEffects");
	}

	IEnumerator HitEffects () {
		hasControl = false; // no control
		isAlive = false;
		SetSprite(1);

		yield return null; // wait 1 - move a bit

		canMove = false; // freeze
		cameraController.StartScreenShake(1.4f,3); // shake

		yield return new WaitForSeconds(0.12f); // wait a bit

		canMove = true; // unfreeze
		particleSystem.Play(); // pretty

		yield return new WaitForSeconds(0.33f);

		SetSprite(2);
		Invoke("Respawn", 1f);
	}

	public void OnHitOtherPlayer () {
		if (getPointEvent != null) getPointEvent();
		score ++;
		PlaySound(2);
	}

	public void PlaySound (int id) {
		audioSource.PlayOneShot(soundEffects[id]);
	}

	public void SetSprite (int id) {
		spriteRenderer.sprite = sprites[id];
	}

	public void Destroy () {
		Destroy(gameObject);
	}
}
