using System;
using System.Collections.Generic;

[System.Flags]
public enum RitualID
{
	Cyclone = 1,
	AspectBlizzard = 2,
	EarthenHeart = 4,
	//HarnessElements = 4,
	LightningHelix = 8,
	TrickleCharge = 16,
	TideCeremony = 32,
	RiteOfQuartz = 64,
	//IceShelfMeditation = 128
}

public abstract class Ritual
{
	public abstract RitualID rID
	{
		get;
	}

	//This will be the description (with color tags) for the main menu
	public abstract string Description
	{
		get;
	}

	public abstract int DisplayIconID
	{
		get;
	}

	public abstract string DisplayName
	{ get; }

	public abstract void ApplyToPlayer(Player plyr);

	public static Ritual[] GetRitualsForIDs(RitualID rID)
	{
		List<Ritual> rtn = new List<Ritual>();

		if ((rID & RitualID.Cyclone) > 0) rtn.Add(new Cyclone());
		if ((rID & RitualID.AspectBlizzard) > 0) rtn.Add(new AspectBlizzard());
		//if ((rID & RitualID.HarnessElements) > 0) rtn.Add(new HarnessElements());
		if ((rID & RitualID.LightningHelix) > 0) rtn.Add(new LightningHelix());
		if ((rID & RitualID.TrickleCharge) > 0) rtn.Add(new TrickleCharge());
		if ((rID & RitualID.TideCeremony) > 0) rtn.Add(new TideCeremony());
		if ((rID & RitualID.RiteOfQuartz) > 0) rtn.Add(new RiteOfQuartz());
		if ((rID & RitualID.EarthenHeart) > 0) rtn.Add(new EarthenHeart());

		return rtn.ToArray();
	}

	public static Ritual GetRitualForID(RitualID rID)
	{
		Ritual rit = null;

		if ((rID & RitualID.Cyclone) > 0) rit = new Cyclone();
		if ((rID & RitualID.AspectBlizzard) > 0) rit = new AspectBlizzard();
		//if ((rID & RitualID.HarnessElements) > 0) rit = new HarnessElements();
		if ((rID & RitualID.LightningHelix) > 0) rit = new LightningHelix();
		if ((rID & RitualID.TrickleCharge) > 0) rit = new TrickleCharge();
		if ((rID & RitualID.TideCeremony) > 0) rit = new TideCeremony();
		if ((rID & RitualID.RiteOfQuartz) > 0) rit = new RiteOfQuartz();
		if ((rID & RitualID.EarthenHeart) > 0) rit = new EarthenHeart();

		return rit;
	}
}

public class Cyclone : Ritual
{
	public override string Description
	{
		get { return "<color=green>Larger Gusts\n+2 Gust Charges</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 6; }
	}

	public override string DisplayName
	{
		get { return "Center of the Cyclone"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.Cyclone;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		for (int i = 0; i < plyr.abilities.Count; i++)
		{
			if (plyr.abilities[i].GetType() == typeof(Gust))
			{
				Gust gust = (Gust)plyr.abilities[i];
				gust.MaxCharges += 2;
				gust.MaxAngle += 15f;
				gust.Range += 10f;
				gust.CooldownReduction -= .5f;
			}
		}
	}
}

public class AspectBlizzard : Ritual
{
	public override string Description
	{
		get { return "<color=green>Slick Deals Area Damage\n+120% Mana Regen</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 9; }
	}

	public override string DisplayName
	{
		get { return "Aspect of the Blizzard"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.AspectBlizzard;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		for (int i = 0; i < plyr.abilities.Count; i++)
		{
			if (plyr.abilities[i].GetType() == typeof(Skate))
			{
				Skate skate = (Skate)plyr.abilities[i];
				skate.GeneralDamage += 2;
			}
		}
		plyr.ManaRegenRate += 3*1.2f;
	}
}
/*
public class HarnessElements : Ritual
{
	public override string Description
	{
		get { return "<color=green>Improved Elemental Extraction\n+25 Health</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 13; }
	}

	public override string DisplayName
	{
		get { return "Harness the Elements"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.HarnessElements;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		Extract extr = plyr.GetAbility<Extract>(false);
		if (extr)
		{
			extr.Automated = true;
			extr.AccretionSpeed += .5f;
		}

		plyr.Health += 25;
	}
}
*/
public class LightningHelix : Ritual
{
	public override string Description
	{
		get { return "<color=green><color=green>+33% Bolt Damage\n+33% faster move speed</color>\n<color=red>-20 Max Health</color></color>"; }
	}

	public override int DisplayIconID
	{
		get { return 3; }
	}

	public override string DisplayName
	{
		get { return "Lightning Helix"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.LightningHelix;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		Bolt bolt = plyr.GetAbility<Bolt>(false);
		if (bolt)
		{
			bolt.GeneralDamage += .5f;
		}
		plyr.controller.movementSettings.ForwardSpeed += 4;
		plyr.controller.movementSettings.BackwardSpeed += 4;
		plyr.controller.movementSettings.StrafeSpeed += 4;
		plyr.Health -= 20;
	}
}

public class TrickleCharge : Ritual
{
	public override string Description
	{
		get { return "<color=green>+30 Max Mana\n+75% Wider Bolt</color>\n<color=red>-50% Bolt Damage</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 11; }
	}

	public override string DisplayName
	{
		get { return "Trickle Charge"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.TrickleCharge;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		for (int i = 0; i < plyr.abilities.Count; i++)
		{
			if (plyr.abilities[i].GetType() == typeof(Bolt))
			{
				Bolt bolt = (Bolt)plyr.abilities[i];
				bolt.GeneralDamage -= .75f;
				bolt.MaxAngle += 6;
			}
		}
		plyr.Mana += 50;
	}
}

public class TideCeremony : Ritual
{
	public override string Description
	{
		get { return "<color=blue>Stronger & longer water shield.\nGust reduces shield cooldown</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 8; }
	}

	public override string DisplayName
	{
		get { return "Ceremony of the Tides"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.TideCeremony;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		Gust gust = plyr.GetAbility<Gust>(false);
		if (gust)
		{
			gust.JumpForce -= 20;
			gust.CooldownReduction += 1.0f;
			gust.waterAligned = true;
		}
		WaterShield shield = plyr.GetAbility<WaterShield>(false);
		if (shield)
		{
			shield.Duration += 4;
			shield.damageReduction += .25f;
			shield.KnockbackReduction += .25f;
		}

	}
}

public class RiteOfQuartz : Ritual
{
	public override string Description
	{
		get { return "<color=green>Airborne gusts dive downward\nStronger but fewer gusts\n+20% knockback resistance</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 12; }
	}

	public override string DisplayName
	{
		get { return "Rite Of Quartz"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.RiteOfQuartz;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		Gust gust = plyr.GetAbility<Gust>(false);
		if (gust)
		{
			gust.earthAligned = true;
			gust.MaxCharges -= 2;
			gust.JumpForce += 150;
			gust.SpecialDamage += 10;
		}

		plyr.transform.localScale = new UnityEngine.Vector3(plyr.transform.localScale.x + 0.75f, plyr.transform.localScale.y + 0.5f, plyr.transform.localScale.z + 0.75f);
		plyr.KnockbackMultiplier -= .2f;
	}
}

public class EarthenHeart : Ritual
{
	public override string Description
	{
		get { return "<color=green>Faster and <color=red>weaker</color> Rock Smash\n+3 Rock Smash charges</color>\n<color=red>-40% knockback resistance</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 10; }
	}

	public override string DisplayName
	{
		get { return "Earthen Heart"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.EarthenHeart;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{

		Smash smash = plyr.GetAbility<Smash>(false);
		if (smash)
		{
			smash.RecoilForce += 50;
			smash.PoundRange += 3;
			smash.MaxCharges += 3;
			smash.SwingDuration -= 1.25f;
			smash.MaxCooldown -= 3;
			smash.GeneralDamage -= 6;
		}

		plyr.KnockbackMultiplier += .4f;
	}
}

public class IceShelfMeditation : Ritual
{
	public override string Description
	{
		get { return "<color=green>Arctic Winds adds Water Shield duration\n</color>\n<color=blue>Water Shield costs 0 mana.</color>\n</color>\n<color=red>+6 seconds Water Shield Cooldown.</color>"; }
	}

	public override int DisplayIconID
	{
		get { return 0; }
	}

	public override string DisplayName
	{
		get { return "Ice Shelf Meditation"; }
	}

	public override RitualID rID
	{
		get
		{
			return RitualID.TideCeremony;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		Skate skate = plyr.GetAbility<Skate>(false);
		if (skate)
		{
			skate.waterAligned = true;
			skate.CooldownReduction += skate.MaxCooldown * 2;
			skate.AmplifiedForce += 8;
		}

		WaterShield shield = plyr.GetAbility<WaterShield>(false);
		if (shield)
		{
			shield.MaxCooldown += 6;
			shield.Cost = 0;
		}

	}
}
