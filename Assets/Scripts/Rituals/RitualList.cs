using System;
using System.Collections.Generic;

[System.Flags]
public enum RitualID
{
	Cyclone = 1,
	AspectBlizzard = 2,
	HarnessElements = 4,
	LightningHelix = 8,
	TrickleCharge = 16
}

public abstract class Ritual
{
	public abstract RitualID rID
	{
		get;
	}

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
	public override RitualID rID
	{
		get
		{
			return RitualID.AspectBlizzard;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		throw new NotImplementedException();
	}
}

public class HarnessElements : Ritual
{
	public override RitualID rID
	{
		get
		{
			return RitualID.HarnessElements;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		throw new NotImplementedException();
	}
}

public class LightningHelix : Ritual
{
	public override RitualID rID
	{
		get
		{
			return RitualID.LightningHelix;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		throw new NotImplementedException();
	}
}

public class TrickleCharge : Ritual
{
	public override RitualID rID
	{
		get
		{
			return RitualID.TrickleCharge;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		throw new NotImplementedException();
	}
}