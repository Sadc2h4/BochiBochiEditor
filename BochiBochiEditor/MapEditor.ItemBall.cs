using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// アイテム（モンスターボールの形の落とし物）の追加
	// 落とし物は、人のイベントに「アイテムを渡すスクリプト」と「拾った後に消すためのフラグ」を付けたもの。
	// 「ここにアイテム（落とし物）を追加…」でアイテムとフラグを決めると、見た目・向き・階層を今の ROM の落とし物に合わせた人を追加する。
	// スクリプトは 13 バイトの決まった形（アイテムの番号と個数 1 を入れて、アイテムを見つけたときの共通の処理を呼ぶ）で、
	// 「編集中のMAPを確定」のときに用意する。同じアイテムのスクリプトが ROM にあればそれを使い、無ければ空き領域に作る
	//-------------------------------------------------------------------------------
	public partial class MapEditor
	{
		// 今の ROM の落とし物から調べた、置くときの見本
		private sealed class ItemBallStyle
		{
			public ushort SpriteNo;
			public byte Action;
			public byte Layer = 3;
			public int FlagMin;
			public int FlagMax;
			// ROM の落とし物から調べられたか（false なら、ゲームごとの決まった値）
			public bool Found;
		}

		//-------------------------------------------------------------------------------
		// アイテムを渡すスクリプトの中身を作る処理
		// setorcopyvar 0x8000, アイテム / setorcopyvar 0x8001, 1 / callstd 1（アイテムを見つけた）/ end
		//-------------------------------------------------------------------------------
		private static byte[] BuildItemBallScript(int item)
		{
			return new byte[] { 0x1A, 0x00, 0x80, (byte)(item & 0xFF), (byte)((item >> 8) & 0xFF), 0x1A, 0x01, 0x80, 0x01, 0x00, 0x09, 0x01, 0x02 };
		}

		//-------------------------------------------------------------------------------
		// その場所のスクリプトが落とし物の形なら、アイテムの番号を返す処理（違えば -1）
		//-------------------------------------------------------------------------------
		private int ItemBallItemAt(uint scriptAddress)
		{
			if (scriptAddress == 0U || !this.IsRomRange(scriptAddress, 13))
			{
				return -1;
			}
			int item = this.romData[scriptAddress + 3] | (this.romData[scriptAddress + 4] << 8);
			byte[] expected = BuildItemBallScript(item);
			for (int i = 0; i < expected.Length; i++)
			{
				if (this.romData[scriptAddress + i] != expected[i])
				{
					return -1;
				}
			}
			return item;
		}

		//-------------------------------------------------------------------------------
		// 今の ROM の全マップの落とし物から、見た目・向き・階層のいちばん多い値と、フラグの範囲を調べる処理
		// 1 つも無いときは、ゲームごとの決まった値（ファイアレッド系 = 見た目 92・下向き、エメラルド系 = 見た目 59・全方向を見る）
		//-------------------------------------------------------------------------------
		private ItemBallStyle DetectItemBallStyle()
		{
			Dictionary<int, int> sprites = new Dictionary<int, int>();
			Dictionary<int, int> actions = new Dictionary<int, int>();
			Dictionary<int, int> layers = new Dictionary<int, int>();
			int flagMin = int.MaxValue;
			int flagMax = -1;
			Action<Dictionary<int, int>, int> count = (table, key) =>
			{
				int old;
				table.TryGetValue(key, out old);
				table[key] = old + 1;
			};
			foreach (MapHeader header in this.mapHeaders)
			{
				if (header == null || header.Persons == null)
				{
					continue;
				}
				foreach (PersonEvent person in header.Persons)
				{
					if (this.ItemBallItemAt(person.ScriptAddress) < 0)
					{
						continue;
					}
					count(sprites, person.SpriteNo);
					count(actions, person.Action);
					count(layers, person.Layer);
					if (person.Flag != 0)
					{
						flagMin = Math.Min(flagMin, person.Flag);
						flagMax = Math.Max(flagMax, person.Flag);
					}
				}
			}
			bool emerald = (GameProfile.Current.Code ?? string.Empty).StartsWith("BPE", StringComparison.Ordinal);
			ItemBallStyle style = new ItemBallStyle
			{
				SpriteNo = (ushort)(emerald ? 59 : 92),
				Action = (byte)(emerald ? 1 : 8),
				Layer = 3,
				FlagMin = emerald ? 1000 : 340,
				FlagMax = emerald ? 1170 : 510,
			};
			if (sprites.Count > 0)
			{
				style.Found = true;
				style.SpriteNo = (ushort)sprites.OrderByDescending(p => p.Value).First().Key;
				style.Action = (byte)actions.OrderByDescending(p => p.Value).First().Key;
				style.Layer = (byte)layers.OrderByDescending(p => p.Value).First().Key;
				if (flagMax >= 0)
				{
					style.FlagMin = flagMin;
					style.FlagMax = flagMax;
				}
			}
			return style;
		}

		//-------------------------------------------------------------------------------
		// 落とし物に使えそうなフラグ（今の ROM の落とし物のフラグの範囲で、どの人のイベントも使っていないいちばん小さい番号）を返す処理
		// 範囲の中に空きが無ければ -1。スクリプトの中だけで使われているフラグは調べていない
		//-------------------------------------------------------------------------------
		private int SuggestItemBallFlag(ItemBallStyle style)
		{
			HashSet<int> used = new HashSet<int>();
			foreach (MapHeader header in this.mapHeaders)
			{
				// 今のマップは、編集中の内容のほうを見る
				if (header == null || header.Persons == null || (this.tempHeader != null && header.Bank == this.tempHeader.Bank && header.Number == this.tempHeader.Number))
				{
					continue;
				}
				foreach (PersonEvent person in header.Persons)
				{
					used.Add(person.Flag);
				}
			}
			if (this.tempHeader != null && this.tempHeader.Persons != null)
			{
				foreach (PersonEvent person in this.tempHeader.Persons)
				{
					used.Add(person.Flag);
				}
			}
			for (int flag = style.FlagMin; flag <= style.FlagMax; flag++)
			{
				if (!used.Contains(flag))
				{
					return flag;
				}
			}
			return -1;
		}

		//-------------------------------------------------------------------------------
		// マスに落とし物を追加する処理（足した人の番号を返す。足せなければ -1 で、理由を message に入れる）
		// スクリプトはまだ作らず、確定のときに用意する印（PendingItem）を付けておく
		//-------------------------------------------------------------------------------
		internal int AddItemBallAt(int x, int y, int item, int flag, out string message)
		{
			int index = this.AddEventAt(EventKindPerson, x, y, out message);
			if (index < 0)
			{
				return -1;
			}
			ItemBallStyle style = this.DetectItemBallStyle();
			PersonEvent person = this.tempHeader.Persons[index];
			person.SpriteNo = style.SpriteNo;
			person.Action = style.Action;
			person.Layer = style.Layer;
			person.Flag = (ushort)flag;
			person.PendingItem = item;
			this.SelectEvent(EventKindPerson, index);
			this.AfterEventListChanged();
			return index;
		}

		//-------------------------------------------------------------------------------
		// 「ここにアイテム（落とし物）を追加…」: アイテムとフラグを決める画面を出して、落とし物を追加する処理
		//-------------------------------------------------------------------------------
		private void AddItemBallWithDialog(int x, int y)
		{
			ItemNameTable table = this.GetItemNameTable();
			List<KeyValuePair<int, string>> items = new List<KeyValuePair<int, string>>();
			if (table != null)
			{
				for (int id = 1; id < 4096; id++)
				{
					string name = table.GetName(id);
					if (!string.IsNullOrWhiteSpace(name) && name.Trim('?', ' ').Length > 0)
					{
						items.Add(new KeyValuePair<int, string>(id, name));
					}
				}
			}
			if (items.Count == 0)
			{
				MessageBox.Show(this, Localizer.T("この ROM のアイテムの名前を読めなかったので、アイテムを選べません。人を追加してから、スクリプトを自分で指定してください。"), Localizer.T("イベントの追加"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			ItemBallStyle style = this.DetectItemBallStyle();
			int suggested = this.SuggestItemBallFlag(style);
			string note = string.Format(Localizer.T("フラグは、拾った後にこの落とし物を消すための番号です。ほかの落とし物や人と同じ番号にすると、片方を拾ったときにもう片方も消えます。\r\n初めに入っている番号は、落とし物用の範囲（{0:X4}〜{1:X4}）のうち、どのマップの人のイベントも使っていない番号です（スクリプトの中だけで使われている番号までは調べていません）。"),
				style.FlagMin, style.FlagMax);
			using (ItemBallForm form = new ItemBallForm(items, suggested, note))
			{
				AppIconHelper.Apply(form);
				UiTheme.Apply(form);
				if (form.ShowDialog(this) != DialogResult.OK || form.ItemId < 0 || form.Flag < 0)
				{
					return;
				}
				string message;
				if (this.AddItemBallAt(x, y, form.ItemId, form.Flag, out message) < 0 && message != null)
				{
					MessageBox.Show(this, message, Localizer.T("イベントの追加"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 確定の前に、追加した落とし物のスクリプトを用意する処理（用意できれば true）
		// 同じアイテムのスクリプトが ROM にあればそれを指し、無ければ空き領域に 13 バイトを書く。
		// 空き領域が無いときは、案内を出して false（その落とし物の印は残る）
		//-------------------------------------------------------------------------------
		private bool WritePendingItemBallScripts()
		{
			if (this.tempHeader == null || this.tempHeader.Persons == null)
			{
				return true;
			}
			foreach (PersonEvent person in this.tempHeader.Persons)
			{
				if (person.PendingItem < 0)
				{
					continue;
				}
				// 追加の後でスクリプトを自分で指定していたら、そちらを使う
				if (person.ScriptAddress != 0U)
				{
					person.PendingItem = -1;
					continue;
				}
				byte[] script = BuildItemBallScript(person.PendingItem);
				uint address = this.FindBytesInRom(script);
				if (address == 0U)
				{
					if (!this.TryFindFreeSpaceForNewEvent(script.Length, ref address) || !this.IsRomRange(address, script.Length))
					{
						MessageBox.Show(this, Localizer.T("アイテムを渡すスクリプトを置く空き領域が ROM に見つかりませんでした。確定を中止しました。"), Localizer.T("イベントの追加"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return false;
					}
					Array.Copy(script, 0, this.romData, address, script.Length);
				}
				person.ScriptAddress = address;
				person.RawScriptValue = address + 0x08000000U;
				person.PendingItem = -1;
			}
			return true;
		}

		//-------------------------------------------------------------------------------
		// ROM の中から、同じ並びのバイト列を探して場所を返す処理（無ければ 0）
		//-------------------------------------------------------------------------------
		private uint FindBytesInRom(byte[] pattern)
		{
			if (this.romData == null || pattern == null || pattern.Length == 0)
			{
				return 0U;
			}
			int index = this.romData.AsSpan().IndexOf(pattern);
			return index > 0 ? (uint)index : 0U;
		}
	}
}
