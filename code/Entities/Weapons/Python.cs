﻿using System.ComponentModel.DataAnnotations;

[Library( "dm_357", Title = ".357 Magnum Revolver" )]
[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
[Display( Name = ".357 Magnum Revolver" )]
partial class Python : DeathmatchWeapon
{
	public static readonly Model WorldModel = Model.Load( "weapons/rust_pistol/rust_pistol.vmdl" );
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 7.0f;
	public override int ClipSize => 6;
	public override AmmoType AmmoType => AmmoType.Python;

	public override int Bucket => 1;
	public override int BucketWeight => 200;

	[Net, Predicted]
	public bool Zoomed { get; set; }

	private float? LastFov;
	private float? LastViewmodelFov;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		AmmoClip = 6;
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}
			return;
		}

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "rust_pistol.shoot" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.01f, 1.5f, 40.0f, 2.0f );

		if ( IsClient && IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 2, 2, 3, 0 );
		}

	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Zoomed = Input.Down( InputButton.Attack2 );
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		float targetFov = camSetup.FieldOfView;
		float targetViewmodelFov = camSetup.ViewModel.FieldOfView;
		LastFov = LastFov ?? camSetup.FieldOfView;
		LastViewmodelFov = LastViewmodelFov ?? camSetup.ViewModel.FieldOfView;

		if ( Zoomed )
		{
			targetFov = 40.0f;
			targetViewmodelFov = 40.0f;
		}

		float lerpedFov = LastFov.Value.LerpTo( targetFov, Time.Delta * 24.0f );
		float lerpedViewmodelFov = LastViewmodelFov.Value.LerpTo( targetViewmodelFov, Time.Delta * 24.0f );

		camSetup.FieldOfView = lerpedFov;
		camSetup.ViewModel.FieldOfView = lerpedViewmodelFov;

		LastFov = lerpedFov;
		LastViewmodelFov = lerpedViewmodelFov;
	}

}
