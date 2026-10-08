using System;

namespace Alluseri.EvertalePonoserV2.API;

public class EvertaleUser {
	public readonly string UserID;
	public readonly string SessionID;
	public readonly string Language;
	public readonly string Region;
	public readonly int? Shard;
	public readonly string? CLID;
	public readonly string? Device;
	public readonly string? OS;
	public readonly string? RCode;

	public EvertaleUser(string UserID, string? CLID, string SessionID, string? Device, string? OS, int? Shard, string Language, string Region, string? RCode = null) {
		this.UserID = UserID;
		this.SessionID = SessionID;
		this.Language = Language;
		this.Region = Region;
		this.Shard = Shard;
		this.CLID = CLID;
		this.Device = Device;
		this.OS = OS;
		this.RCode = RCode;
	}

	public EvertaleAPI.LoginData? Relog()
		=> EvertaleAPI.CreateSession(UserID, CLID!, Language, Region, Shard!.Value, Device!, OS!);

	public static EvertaleUser? RegisterNew(string Device, string OS, int Shard, string Language, string Region, EvertaleAPI.ProfileData? Data) {
		EvertaleAPI.NewAccountData? NAD = EvertaleAPI.RegisterAccount(Device, OS, Shard, Language, Region);
		if (NAD == null)
			return null;
		EvertaleAPI.LoginData? LD = EvertaleAPI.CreateSession(NAD.UID, NAD.CLID, Language, Region, Shard, Device, OS);
		if (LD == null)
			return null;
		if (Data.HasValue)
			EvertaleAPI.UpdateAccountSettings(LD.SessionID, Data.Value);
		return new EvertaleUser(NAD.UID, NAD.CLID, LD.SessionID, Device, OS, Shard, Language, Region, NAD.RCode);
	}

	public static EvertaleUser? LoginWithRestoreCode(string RestoreCode, string Device, string OS, int Shard, string Language, string Region) {
		EvertaleAPI.RestoreData? RD = EvertaleAPI.RestoreAccount(RestoreCode.Trim(), Language.Trim(), Region.Trim());
		if (RD == null) {
			Console.WriteLine("Could not restore the account. Verify the restore code, language, and region.");
			return null;
		}
		EvertaleAPI.LoginData? LD = EvertaleAPI.CreateSession(RD.UID, RD.CLID, Language.Trim(), Region.Trim(), Shard, Device.Trim(), OS.Trim());
		if (LD == null) {
			Console.WriteLine("The account was restored, but session login failed. Verify the shard, device, OS, language, and region.");
			return null;
		}
		return new EvertaleUser(RD.UID, RD.CLID, LD.SessionID, Device.Trim(), OS.Trim(), Shard, Language.Trim(), Region.Trim(), RestoreCode.Trim());
	}
}