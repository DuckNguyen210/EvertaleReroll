#pragma warning disable CA2211

using Alluseri.EvertalePonoserV2.API;
using Alluseri.EvertalePonoserV2.Reroll;
using System;
using System.Threading;

namespace Alluseri.EvertalePonoserV2;

public static class Program {
	public static void Reroll(string Device, string OS, int Shard, string Language, string Timezone, EvertaleAPI.ProfileData? Data) {
		EvertaleUser? Reu = EvertaleUser.RegisterNew(Device, OS, Shard, Language, Timezone, null);
		if (Reu == null) {
			Console.WriteLine("Could not register an account to reroll.");
			return;
		}
		Console.WriteLine("[1/3] [1/3] Running offline act scenario...");
		OfflineActScenario Oas = new(Reu.SessionID, Data);
		Oas.Run();
		Console.WriteLine("[1/3] [2/3] Hacking sidequests...");
		Oas.HackSidequests();
		Console.WriteLine("[1/3] [3/3] Completing missing quests...");
		Oas.CompleteMissing();
		Console.WriteLine("[2/3] Running one time nuisance scenario...");
		OneTimeNuisanceScenario Otns = new(Reu.SessionID, true);
		Otns.Run();
		Console.WriteLine("[3/3] Running daily nuisance scenario...");
		DailyNuisanceScenario Dns = new(Reu.SessionID, false, false);
		Dns.Run();
		Console.WriteLine("Done! To get into your account, use the restore code: " + Reu.RCode);
		Console.WriteLine("lunahook.dev on top!");
	}

	public static void Main(string[] Args) {
		Console.WriteLine("Evertale Assistant");
		Console.WriteLine("~Поносим Evertale по L7~");
		switch (Args.Length == 0 ? "help" : Args[0].ToLowerInvariant()) {
			default:
			Console.WriteLine($"Unknown subcommand: {Args[0]}");
			Console.WriteLine("Run with 'help' to list all available subcommands.");
			break;
			case "help":
			Console.WriteLine("Available subcommands:");
			Console.WriteLine("help - Displays this list of subcommands.");
			Console.WriteLine("reroll - Creates and rerolls a new account in interactive mode.");
			Console.WriteLine("restore [rcode] [device] [os] [shard] [lang] [region] - Logs into an existing account using a restore code; prompts for omitted values.");
			Console.WriteLine("daily [rcode] [device] [os] [shard] [lang] [region] - Logs in and runs the daily routine.");
			Console.WriteLine("arena [sessid] [enemy id] - Fights an enemy in the arena. Ignores rank checks.");
			Console.WriteLine("test-api - Tests the API connectivity.");
			break;
			case "restore": {
				string ReadValue(int ArgumentIndex, string Prompt) {
					if (Args.Length > ArgumentIndex)
						return Args[ArgumentIndex];
					Console.Write(Prompt);
					return Console.ReadLine() ?? "";
				}

				string Code = ReadValue(1, "Please enter your restore code: ").Trim();
				string Dev = ReadValue(2, "Please enter the device name: ");
				if (string.IsNullOrWhiteSpace(Dev))
					Dev = "Genuine Lunahook Branded Phone";
				string Os = ReadValue(3, "Please enter the OS: ");
				if (string.IsNullOrWhiteSpace(Os))
					Os = "Android OS 11 / API-30 (RP1A.200720.012/A225FXXU2AUH1)";
				string ShardInput = ReadValue(4, "Please enter the shard*: ");
				if (!int.TryParse(ShardInput, out int Sh)) {
					Console.WriteLine("Shard must be a valid integer.");
					return;
				}
				string Alp = ReadValue(5, "Please enter the Alpha-2 language: ");
				if (string.IsNullOrWhiteSpace(Alp))
					Alp = "jp";
				string Rtz = ReadValue(6, "Please enter the region timezone(e.g. JST): ");
				if (string.IsNullOrWhiteSpace(Rtz))
					Rtz = "JST";

				if (string.IsNullOrWhiteSpace(Code)) {
					Console.WriteLine("Restore code cannot be empty.");
					return;
				}
				EvertaleUser? RestoredUser = EvertaleUser.LoginWithRestoreCode(Code, Dev, Os, Sh, Alp, Rtz);
				if (RestoredUser == null)
					return;
				Console.WriteLine("Successfully restored account.");
				Console.WriteLine("User ID: " + RestoredUser.UserID);
				Console.WriteLine("Session ID: " + RestoredUser.SessionID);
				Console.WriteLine("Language: " + RestoredUser.Language);
				Console.WriteLine("Region: " + RestoredUser.Region);
				Console.WriteLine("Shard: " + (RestoredUser.Shard?.ToString() ?? "N/A"));
				Console.WriteLine("CLID: " + (RestoredUser.CLID ?? "N/A"));
				Console.WriteLine("Device: " + (RestoredUser.Device ?? "N/A"));
				Console.WriteLine("OS: " + (RestoredUser.OS ?? "N/A"));
				Console.WriteLine("Restore code: " + (RestoredUser.RCode ?? "N/A"));
			}
			break;
			
			case "reroll": {
				Console.WriteLine("Interactive mode. Fields not marked with * may be left empty.");
				Console.Write("Please enter the device name: ");
				string Devn = Console.ReadLine()!;
				if (string.IsNullOrEmpty(Devn))
					Devn = "Genuine Lunahook Branded Phone";
				Console.Write("Please enter the OS: ");
				string Osn = Console.ReadLine()!;
				if (string.IsNullOrEmpty(Osn))
					Osn = "Android OS 11 / API-30 (RP1A.200720.012/A225FXXU2AUH1)";
				Console.Write("Please enter the shard*: ");
				if (!int.TryParse(Console.ReadLine(), out int Shard)) {
					Console.WriteLine("Shard must be a valid integer.");
					return;
				}
				Console.Write("Please enter the Alpha-2 language: ");
				string Alp2 = Console.ReadLine()!;
				if (string.IsNullOrEmpty(Alp2))
					Alp2 = "jp";
				Console.Write("Please enter the region timezone(e.g. JST): ");
				string Rtz = Console.ReadLine()!;
				if (string.IsNullOrEmpty(Rtz))
					Rtz = "JST";
				Console.Write("Please enter your nickname(or leave empty to choose later in game, THAT MIGHT BE BANNABLE THOUGH): ");
				string Nick = Console.ReadLine()!;
				Console.WriteLine("Bravo 6, going dark.");
				Reroll(Devn, Osn, Shard, Alp2, Rtz, string.IsNullOrEmpty(Nick) ? null : new EvertaleAPI.ProfileData(Nick));
			}
			break;
			case "daily": {
				if (Args.Length < 2) {
					Console.WriteLine("Usage: daily [rcode] [device] [os] [shard] [lang] [region]");
					return;
				}
				string Devn = Args.Length > 2 ? Args[2] : "Genuine Lunahook Branded Phone";
				string Osn = Args.Length > 3 ? Args[3] : "Android OS 11 / API-30 (RP1A.200720.012/A225FXXU2AUH1)";
				int Shard = 1;
				if (Args.Length > 4 && !int.TryParse(Args[4], out Shard)) {
					Console.WriteLine("Shard must be a valid integer.");
					return;
				}
				string Alp2 = Args.Length > 5 ? Args[5] : "jp";
				string RestoreRegion = Args.Length > 6 ? Args[6] : "JST";
				EvertaleUser? User = EvertaleUser.LoginWithRestoreCode(Args[1], Devn, Osn, Shard, Alp2, RestoreRegion);
				if (User == null) {
					Console.WriteLine("Daily routine was not started because login failed. Check the restore code and login settings, especially the shard.");
					return;
				}
				Console.WriteLine("Successfully logged in. Starting daily routine...");
				Daily DailyRoutine = new(User.SessionID, true, true);
				DailyRoutine.Run();
			}
			break;
			case "arena":
			if (Args.Length < 3) {
				Console.WriteLine("Usage: arena [sessid] [enemy id]");
				return;
			}
			Console.WriteLine("Trying to fight on SESSID " + Args[1] + " against " + Args[2]);
			bool? V = EvertaleAPI.ArenaFight(Args[1], Args[2]);
			Console.WriteLine("Result: " + (V == null ? "Blocked by server" : V.Value ? "Victory!" : "Loss!"));
			break;
			case "test-api": {
				EvertaleUser? Eu = EvertaleUser.RegisterNew("Lunahook", "Lunahook", 1, "ru", "GMT", new EvertaleAPI.ProfileData("lunahook.dev"));
				if (Eu == null) {
					Console.WriteLine("[API Test] Account failed to register.");
					return;
				}
				EvertaleChat Ec;
				try {
					Ec = new(Eu);
				} catch {
					Console.WriteLine("[API Test] Failed to connect to chat.");
					throw;
				}
				if (Ec.Send("X" + Random.Shared.Next(9999) + "X lunahook.dev on top! Free rerolls for everyone! github.com/Alluseri/EvertalePonoserV2")) {
					Thread.Sleep(500);
				} else {
					Console.WriteLine("[API Test] Failed to send local message.");
					return;
				}
				if (Ec.Swap(true)) {
					Thread.Sleep(500);
				} else {
					Console.WriteLine("[API Test] Failed to swap to global.");
					return;
				}
				if (Ec.Send("X" + Random.Shared.Next(9999) + "X lunahook.dev on top! Free rerolls for everyone! github.com/Alluseri/EvertalePonoserV2")) {
					Thread.Sleep(500);
				} else {
					Console.WriteLine("[API Test] Failed to send global message.");
					return;
				}
				Console.WriteLine("API Test successful!");
			}
			break;
		}
	}
}