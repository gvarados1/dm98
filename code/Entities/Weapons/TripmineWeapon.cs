﻿[Library( "dm_tripmine", Title = "Tripmine" )]
[Hammer.EditorModel( "models/dm_tripmine.vmdl" )]
partial class TripmineWeapon : DeathmatchWeapon
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 1.0f;
	public override AmmoType AmmoType => AmmoType.Grenade;
	public override int ClipSize => 1;
	public override int Bucket => 5;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/dm_tripmine.vmdl" );
		AmmoClip = 1;
	}

	public override bool CanPrimaryAttack()
	{
		return Input.Released( InputButton.Attack1 );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( Owner is not DeathmatchPlayer player ) return;

		// woosh sound
		// screen shake

		Rand.SetSeed( Time.Tick );

		var tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * 150 )
				.Ignore( Owner )
				.Run();

		if ( !tr.Hit )
			return;

		if ( !tr.Entity.IsWorld )
			return;

		if ( IsServer )
		{
			var grenade = new Tripmine
			{
				Position = tr.EndPosition,
				Rotation = Rotation.LookAt( tr.Normal, Vector3.Up ),
				Owner = Owner
			};

			_ = grenade.Arm( 0.5f );
		}

		TakeAmmo( 1 );
		Reload();


		if ( IsServer && AmmoClip == 0 && player.AmmoCount( AmmoType.Grenade ) == 0 )
		{
			Delete();
			player.SwitchToBestWeapon();
		}
	}
}
