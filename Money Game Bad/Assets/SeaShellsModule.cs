using UnityEngine;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

public class SeaShellsModule : MonoBehaviour
{
	public KMSelectable[] buttons;
	public Transform[] LEDs;
	public TextMesh Display;

	string[,] phrase = new string[3,4] {{"she sells ", "she shells ", "sea shells ", "sea sells "}, {"sea\nshells ", "she\nshells ", "sea\nsells ", "she\nsells "}, {"sea shore", "she sore", "she sure", "seesaw"}};
	string[,] buttonText = new string[4,5] {{"she", "sit", "sushi", "shoe", "shih tzu"}, {"tutu", "can", "cancan", "2", "toucan"}, {"stitch", "twitch", "itch", "witch", "switch"}, {"burger", "llama", "armour", "bulgaria", "burglar alarm"}};

	int[,] length = new int[4, 4] {{7,6,7,7}, {6,6,6,5}, {5,6,5,6}, {7,5,7,5}};
	int[,,] table = new int[4, 4, 7] {{{1,3,0,1,3,0,1},{0,2,4,4,0,2,0},{4,0,2,4,0,2,4},{3,0,0,1,3,0,1}},
									  {{1,4,4,1,1,4,0},{2,3,2,2,3,1,0},{4,0,4,0,4,0,0},{1,4,4,3,0,0,0}},
									  {{0,1,0,1,0,0,0},{4,0,0,4,4,0,0},{3,1,4,0,2,0,0},{0,1,3,1,0,0,0}},
									  {{0,2,0,2,4,0,2},{3,1,0,4,2,0,0},{4,1,3,0,3,0,1},{2,4,2,4,2,0,0}}};
	int[,] key = new int[4,5] {{3,4,0,1,2}, {1,4,0,3,2}, {3,4,2,1,0}, {4,3,2,0,1}};
	int[] swap = new int[5];

	int row;
	int col;
	int keynum;
	int step;
	int stage = 0;
	bool isActivated = false;
	bool isPassed = false;
	string displaytext;

	void Start()
	{
		Init();

		GetComponent<KMBombModule>().OnActivate += ActivateModule;
	}

	void Init()
	{
		Display.text = " ";

		for(int i = 0; i < buttons.Length; i++)
		{
			TextMesh buttonTextMesh = buttons[i].GetComponentInChildren<TextMesh>();
			buttonTextMesh.text = " ";
			int j = i;
			buttons[i].OnInteract += delegate () {buttons[j].AddInteractionPunch(0.3f); OnPress(swap[j] == key[keynum, table [row, col, step]]); return false; };
		}

		for (int i = 0; i < LEDs.Length; i++)
		{
			foreach (MeshRenderer Render in LEDs[i].GetComponentsInChildren<MeshRenderer>())
			{
				if (Render.name == "Off")
				{
					Render.enabled = true;
				}
				else
				{
					Render.enabled = false;
				}
			}
		}
	}

	void ActivateModule()
	{
		isActivated = true;
		StartCoroutine(NewStage(false));
	}

	void OnPress(bool correctButton)
	{
		GetComponent<KMAudio> ().PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, transform);

		if (!isPassed)
		{
			if (!isActivated)
			{
				GetComponent<KMBombModule> ().HandleStrike ();
			}
			else
			{
				if (correctButton)
				{
					step++;
					if (step == length [row, col])
					{
						foreach(MeshRenderer Render in LEDs[stage].GetComponentsInChildren<MeshRenderer>())
						{
							if (Render.name == "On")
							{
								Render.enabled = true;
							}
							else
							{
								Render.enabled = false;
							}
						}
						stage++;
						if (stage == 3)
						{
							GetComponent<KMBombModule> ().HandlePass ();
							isPassed = true;
							step = 0;
							stage = 0;
							Display.text = " ";
						}
						else
						{
							StartCoroutine(NewStage(true));
						}
					} 
				}
				else
				{
					GetComponent<KMBombModule> ().HandleStrike ();
					StartCoroutine(NewStage(true));
				}
			}
		}
	}

	protected IEnumerator NewStage(bool delay)
	{
		Display.text = " ";
		yield return new WaitForSeconds (0.2f);
		for(int i = 0; i < buttons.Length; i++)
		{
			TextMesh buttonTextMesh = buttons[i].GetComponentInChildren<TextMesh>();
			buttonTextMesh.text = " ";
			yield return new WaitForSeconds (0.1f);
		}

		if (delay)
		{
			yield return new WaitForSeconds (0.6f);
		}

		row = Random.Range (0, 4);
		col = Random.Range (0, 4);
		keynum = Random.Range (0, 4);
		swap[0] = Random.Range (0,4);
		swap[1] = Random.Range (0,3);
		if (swap[0] <= swap[1])
		{
		swap[1]++;
		}
		swap[2] = Random.Range(0, 2);
		if (swap[0] <= swap[2] || swap[1] <= swap[2])
		{
		swap[2]++;
		}
		if (swap[0] <= swap[2] && swap[1] <= swap[2])
		{
		swap[2]++;
		}
		swap[3] = 6 - swap[0] - swap[1] - swap[2];
		swap[4] = 4;
		step = 0;

		displaytext = phrase[0,row];
		displaytext += phrase[1,col];
		displaytext += "on the\n";
		displaytext += phrase[2,keynum];

		Display.text = displaytext;
		yield return new WaitForSeconds (0.2f);
		for(int i = 0; i < buttons.Length; i++)
		{
			TextMesh buttonTextMesh = buttons[i].GetComponentInChildren<TextMesh>();
			buttonTextMesh.text = buttonText[keynum,swap[i]];
			yield return new WaitForSeconds (0.1f);
		}
	}
}
