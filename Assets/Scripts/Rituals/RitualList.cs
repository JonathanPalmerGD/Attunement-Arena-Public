using System;
using System.Collections.Generic;

[System.Flags]
public enum RitualID
{
	Cyclone = 1,
	AspectBlizzard = 2,
	HarnessElements = 4,
	LightningHelix = 8,
	TrickleCharge = 16,
	TideCeremony = 32,
	RiteOfQuartz = 64
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
		if ((rID & RitualID.HarnessElements) > 0) rtn.Add(new HarnessElements());
		if ((rID & RitualID.LightningHelix) > 0) rtn.Add(new LightningHelix());
		if ((rID & RitualID.TrickleCharge) > 0) rtn.Add(new TrickleCharge());

		return rtn.ToArray();
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
		Gust gust = plyr.GetAbility<Gust>(false);
		if (gust)
		{
			gust.earthAligned = true;
			gust.MaxCharges -= 2;
			gust.JumpForce += 150;
		}

		plyr.transform.localScale = new UnityEngine.Vector3(plyr.transform.localScale.x + 1.2f, plyr.transform.localScale.y + 1.0f, plyr.transform.localScale.z + 1.2f);
		plyr.Health += 20;
		plyr.KnockbackMultiplier -= .2f;
		
		
		
		
		
		Extract extr = plyr.GetAbility<Extract>(false);
		if (extr)
		{
			extr.Automated = true;
			extr.AccretionSpeed += .5f;
		}

		plyr.Health += 25;
	}
}

public class LightningHelix : Ritual
{
	public override string Description
	{
		get { return "<color=green>Larger Gusts\n+2 Gust Charges</color>"; }
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
		for (int i = 0; i < plyr.abilities.Count; i++)
		{
			if (plyr.abilities[i].GetType() == typeof(Bolt))
			{
				Bolt bolt = (Bolt)plyr.abilities[i];
				bolt.GeneralDamage += .5f;
			}
		}
		plyr.Health -= 20;
	}
}

public class TrickleCharge : Ritual
{
	public override string Description
	{
		get { return "<color=green>+50 Max Mana\n+75% Wider Bolt</color>\n<color=red>-50% Bolt Damage</color>"; }
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
		get { return "<color=blue>Gust gives water shield\n</color>"; }
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
		for (int i = 0; i < plyr.abilities.Count; i++)
		{
			if (plyr.abilities[i].GetType() == typeof(Bolt))
			{
				WaterShield shield = (WaterShield)plyr.abilities[i];
				shield.Duration += 4;
				//shield.MaxAngle += 6;
			}
		}
		plyr.Mana += 50;
	}
}

public class RiteOfQuartz : Ritual
{
	public override string Description
	{
		get { return "<color=green>Airborne gusts dive downward\nReduced knockback\n+20 Health</color>"; }
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
		}

		plyr.transform.localScale = new UnityEngine.Vector3(plyr.transform.localScale.x + 1.2f, plyr.transform.localScale.y + 1.0f, plyr.transform.localScale.z + 1.2f);
		plyr.Health += 20;
		plyr.KnockbackMultiplier -= .2f;
	}
}