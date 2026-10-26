using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class bulkyProductScript : MonoBehaviour
{

	public List<KMSelectable> buttons;
	public TextMesh topText, middleText, bottomText;

	private List<int> top, middle, answer;
	private static int ModuleIdCounter;
	private int ModuleId;
	private bool TwitchShouldStop;
	private string inputtedAnswer, expectedAnswer;
	private bool ModuleSolved;
	void log(string message) {Debug.Log(message.Split('\n').Select(x => $"[Bulky Product #{ModuleId}] {x}").Aggregate((a,b) => a+ "\n" + b));}

	void Awake() {ModuleId = ++ModuleIdCounter;}
	string toString(List<int> number)
	{
		return number.ToList().Select(x => "0123456789abcdef"[x].ToString()).Reverse().Aggregate((a,b)=>a+b);
	}

	string toString(int x) => "0123456789abcdef"[x].ToString();

	void generate()
	{
		top = Enumerable.Range(0, 32).Select(_=>_==31?Rnd.Range(1,16):Rnd.Range(0,16)).ToList();
		middle = Enumerable.Range(0, 32).Select(_=>_==31?Rnd.Range(1,16):Rnd.Range(0,16)).ToList();
		
		topText.text = top.AsEnumerable().Reverse().Select((c, i) =>
		{
			string c0 = toString(c);
			if (i == 31) return c0;
			if (i == 15) return c0 + "\n";
			if (i % 4 == 3)
			{
				return c0 + " ";
			}
			return c0;
		}).Aggregate((a,b)=>a+b);
		middleText.text = middle.AsEnumerable().Reverse().Select((c, i) =>
		{
			string c0 = toString(c);
			if (i == 31) return c0;
			if (i == 15) return c0 + "\n";
			if (i % 4 == 3)
			{
				return c0 + " ";
			}
			return c0;
		}).Aggregate((a,b)=>a+b);
		bottomText.text = "";
		
		
		answer = Enumerable.Repeat(0, 64).ToList();
		for (int i = 0; i < 32; i++)
		{
			for (int j = 0; j < 32; j++)
			{
				answer[i + j] += top[i] * middle[j];
			}
		}
		for (int i = 0; i < 63; i++)
		{
			int t = answer[i] / 16;
			answer[i] %= 16;
			answer[i + 1] += t;
		}

		answer = answer.AsEnumerable().Reverse().SkipWhile(x => x == 0).Reverse().ToList();
		expectedAnswer = toString(answer);
		log($"Top number: {toString(top)}\nMiddle number: {toString(middle)}\nExpected answer: {toString(answer)}");
	}

	void solve()
	{
		topText.text = "<color=#0f0>solved!</color>";
		bottomText.text = "<color=#0f0>solved!</color>";
		middleText.text = "go solve other mods!";
		ModuleSolved = true;
		GetComponent<KMBombModule>().HandlePass();
	}

	void updateScreen()
	{
		bottomText.text = inputtedAnswer.TakeLast(32).Select((c, i) =>
		{
			string c0 = expectedAnswer[Math.Max(32,inputtedAnswer.Length)-32+i] == c? c.ToString(): $"<color=#f00>{c.ToString()}</color>";
			if (i == 31) return c0;
			if (i == 15) return c0 + "\n";
			if (i % 4 == 3)
			{
				return c0 + " ";
			}
			return c0;
		}).Aggregate((a,b)=>a+b);
	}

	void press(int index)
	{
		if (ModuleSolved) return;
		inputtedAnswer += "0123456789abcdef"[index];
		if (inputtedAnswer[inputtedAnswer.Length - 1] != expectedAnswer[inputtedAnswer.Length - 1])
		{
			GetComponent<KMBombModule>().HandleStrike();
			TwitchShouldStop = true;
		}

		if (inputtedAnswer.Length == 64) solve();
		else updateScreen();
	}
	
	void Start () {
		for (int i = 0; i < 16; i++)
		{
			int i1 = i;
			buttons[i1].OnInteract += delegate { press(i1); return false; };
		}
		generate();
	}

	private string TwitchHelpMessage = "Use !{0} <digits> to input digits.";

	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (!Regex.Match(command, "^[0-9a-f ]$").Success)
		{
			yield return "sendtochaterror Invalid command.";
			yield break;
		}

		foreach (char c in command)
		{
			if (c == ' '){}
			else if ("0123456789abcdef".IndexOf(c) != -1) buttons["0123456789abcdef".IndexOf(c)].OnInteract();
			else
			{
				yield return "sendtochaterror Invalid command.";
				yield break;
			}
			if (TwitchShouldStop)
			{
				TwitchShouldStop = false;
				yield break;
			}
			yield return new WaitForSeconds(0.1f);
		}
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		yield return null;
		while (inputtedAnswer.Length < 64)
		{
			buttons["0123456789abcdef".IndexOf(expectedAnswer[inputtedAnswer.Length])].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
	}
}
