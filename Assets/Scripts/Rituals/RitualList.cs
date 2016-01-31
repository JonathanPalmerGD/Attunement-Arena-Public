using System;
using System.Collections.Generic;

[System.Flags]
public enum RitualID
{
	AnIceGuy = 1
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

		if ((rID & RitualID.AnIceGuy) > 0) rtn.Add(new AnIceGuy());

		return rtn.ToArray();
	}
}

public class AnIceGuy : Ritual
{
	public override RitualID rID
	{
		get
		{
			return RitualID.AnIceGuy;
		}
	}

	public override void ApplyToPlayer(Player plyr)
	{
		throw new NotImplementedException();
	}
}