using System;
using System.Collections.Generic;

namespace BochiBochiEditor
{
	//-------------------------------------------------------------------------------
	// 元のゲーム（ファイアレッド・エメラルド）の曲番号と曲名の対応表（マップの BGM を名前で選ぶために使う）
	// 番号は解析済みソース（pret/pokefirered・pokeemerald の songs.h）に合わせたもの。曲名はどの場面の曲か分かる言い方にしている
	// 改造 ROM で曲を足している場合、表に無い番号は「一覧にない曲」として番号だけで扱う
	// どちらのゲームの表を使うかは、読み込んでいる ROM（GameProfile.Current）で決まる
	//-------------------------------------------------------------------------------
	internal static class BgmCatalog
	{
		// ファイアレッドの曲: 番号、日本語名、英語名、元のファイアレッドでの曲の「指紋」（SongTable.Fingerprint。曲が差し替えられていないかの判定に使う）
		private static readonly (int Id, string Ja, string En, string Print)[] songs =
		{
			(256, "ポケモン回復", "Pokémon healed", "49b4e5c16f05"),
			(257, "レベルアップ", "Level up", "7432da332015"),
			(258, "アイテム入手", "Obtained an item", "618f58a09417"),
			(259, "進化おめでとう", "Evolved", "cdba975495a9"),
			(260, "バッジ入手", "Obtained a badge", "58c94fd4692e"),
			(261, "わざマシン入手", "Obtained a TM/HM", "df1f4f7ed48c"),
			(262, "きのみ入手", "Obtained a berry", "93556eca4d72"),
			(263, "進化（前奏）", "Evolution (intro)", "112365d4efb8"),
			(264, "進化", "Evolution", "f1dac3fff22c"),
			(265, "戦闘！ジムリーダー（ルビー・サファイア）", "Battle! Gym Leader (Ruby/Sapphire)", "7410816a0f89"),
			(266, "戦闘！トレーナー（ルビー・サファイア）", "Battle! Trainer (Ruby/Sapphire)", "eeb1c4af8106"),
			(267, "トレーナーズスクール", "Trainer school", "1ad8f689d37e"),
			(268, "スロット大当たり", "Slots jackpot", "0d9a71a03e24"),
			(269, "スロット当たり", "Slots win", "553c9ad5609a"),
			(270, "わざを忘れた", "Move deleted", "5d7c6114441b"),
			(271, "残念", "Too bad", "ff5434e68367"),
			(272, "ついてきて", "Follow me", "8bbb726a556d"),
			(273, "ゲームコーナー", "Game Corner", "6e06e3315b60"),
			(274, "ロケット団のアジト", "Rocket Hideout", "9fa90ad9e0cd"),
			(275, "ポケモンジム", "Pokémon Gym", "dd60948e9ba2"),
			(276, "プリンのうた", "Jigglypuff's song", "7bb779f252f3"),
			(277, "オープニング（戦い）", "Opening (fight)", "1b92a90339cd"),
			(278, "タイトル", "Title screen", "0523463c7201"),
			(279, "グレンタウン", "Cinnabar Island", "03e07e6e27b4"),
			(280, "シオンタウン", "Lavender Town", "7bcf3c431970"),
			(281, "回復（未使用）", "Heal (unused)", "6fe9c5f7213e"),
			(282, "サイクリング", "Cycling", "0bf98bab9935"),
			(283, "視線！ロケット団", "Encounter! Team Rocket", "5551fa36e667"),
			(284, "視線！女の子", "Encounter! Girl", "d02d4fa57e63"),
			(285, "視線！男の子", "Encounter! Boy", "5193f77f14b2"),
			(286, "殿堂入り", "Hall of Fame", "8d6c68246b1c"),
			(287, "トキワのもり", "Viridian Forest", "49c11267d9a1"),
			(288, "おつきみやま", "Mt. Moon", "b1295d6df1e0"),
			(289, "ポケモンやしき", "Pokémon Mansion", "ffb74f9b43d6"),
			(290, "エンディング", "Credits", "294885b689c7"),
			(291, "1ばんどうろ", "Route 1", "23121a1e1da2"),
			(292, "24ばんどうろ", "Route 24", "2e9f13565cf9"),
			(293, "3ばんどうろ", "Route 3", "ea1a7d40294c"),
			(294, "11ばんどうろ", "Route 11", "34cdf85b6f8a"),
			(295, "チャンピオンロード", "Victory Road", "a7004bb212e7"),
			(296, "戦闘！ジムリーダー", "Battle! Gym Leader", "ab0183eacf49"),
			(297, "戦闘！トレーナー", "Battle! Trainer", "8900dd2fe7e2"),
			(298, "戦闘！野生ポケモン", "Battle! Wild Pokémon", "c62d979061e0"),
			(299, "戦闘！チャンピオン", "Battle! Champion", "08da21096eee"),
			(300, "マサラタウン", "Pallet Town", "000051ac16ed"),
			(301, "オーキド研究所", "Oak's Lab", "cb8456f506c3"),
			(302, "オーキド博士", "Professor Oak", "bf66f3ec84d1"),
			(303, "ポケモンセンター", "Pokémon Center", "928fb145c05a"),
			(304, "サント・アンヌ号", "S.S. Anne", "42e5de09f914"),
			(305, "なみのり", "Surf", "6f8122ec1982"),
			(306, "ポケモンタワー", "Pokémon Tower", "ca1697380475"),
			(307, "シルフカンパニー", "Silph Co.", "3f1e583d00d0"),
			(308, "セキチクシティ", "Fuchsia City", "45007587a6b8"),
			(309, "タマムシシティ", "Celadon City", "7545ac658253"),
			(310, "勝利！トレーナー", "Victory! Trainer", "ca092f1a9194"),
			(311, "勝利！野生ポケモン", "Victory! Wild Pokémon", "8c8498a9fa78"),
			(312, "勝利！ジムリーダー", "Victory! Gym Leader", "b76548e8d58b"),
			(313, "クチバシティ", "Vermilion City", "a1b55fea772a"),
			(314, "ニビシティ", "Pewter City", "c3ec80a7c8c3"),
			(315, "ライバル登場", "Rival appears", "554e3d17feef"),
			(316, "ライバル退場", "Rival leaves", "7cfbf45d1b0a"),
			(317, "図鑑評価", "Pokédex rating", "95c3195f8a5d"),
			(318, "だいじなもの入手", "Obtained a key item", "3bca4c120174"),
			(319, "捕獲（前奏）", "Caught (intro)", "18deb9449f71"),
			(320, "記念撮影", "Photo", "c5589cde63cb"),
			(321, "ゲームフリークのロゴ", "Game Freak logo", "16084c38ae1f"),
			(322, "捕獲成功", "Caught", "a66dbf357a07"),
			(323, "冒険の説明", "New game instructions", "fbc188c8f461"),
			(324, "冒険のはじまり", "New game intro", "3f9213f7c1d2"),
			(325, "冒険へ出発", "New game exit", "fecd76143a73"),
			(326, "ポケモンジャンプ", "Pokémon Jump", "859b0f988e99"),
			(327, "ユニオンルーム", "Union Room", "fe6e774c4119"),
			(328, "ポケモンネットセンター", "Network Center", "2a7de7d5ba12"),
			(329, "ふしぎなおくりもの", "Mystery Gift", "bbff9659112a"),
			(330, "きのみどり", "Berry picking", "7ca05bc2eae4"),
			(331, "ナナシマの洞窟", "Sevii Islands cave", "5d866c2c90f2"),
			(332, "おしえてテレビ", "Teachy TV show", "8bbb726a556d"),
			(333, "ナナシマの道路", "Sevii Islands route", "8935467a5f14"),
			(334, "ナナシマのダンジョン", "Sevii Islands dungeon", "49c11267d9a1"),
			(335, "1・2・3のしま", "Islands 1-3", "c3ec80a7c8c3"),
			(336, "4・5のしま", "Islands 4-5", "69d4bde1e9f5"),
			(337, "6・7のしま", "Islands 6-7", "e698404578fb"),
			(338, "ポケモンのふえ", "Poké Flute", "0d6fc1ff26fa"),
			(339, "戦闘！デオキシス", "Battle! Deoxys", "fc98dc5c3238"),
			(340, "戦闘！ミュウツー", "Battle! Mewtwo", "25e2b4a58ee0"),
			(341, "戦闘！伝説のポケモン", "Battle! Legendary Pokémon", "428b87839e67"),
			(342, "視線！ジムリーダー", "Encounter! Gym Leader", "effae629ba87"),
			(343, "デオキシス登場", "Deoxys appears", "fd306aeedb0e"),
			(344, "トレーナータワー", "Trainer Tower", "dd60948e9ba2"),
			(345, "マサラタウン（ゆっくり）", "Pallet Town (slow)", "e395486286ef"),
			(346, "おしえてテレビ（メニュー）", "Teachy TV menu", "80ce6d02fb11"),
			(0xFFFF, "なし（音楽を止める）", "None (stop the music)", null),
		};

		// エメラルドの曲（350 番から。それより前は効果音）。指紋は日本語版のもの。英語版で中身が少し違う曲だけ、英語版の指紋を 5 つ目に持つ
		// 484〜558 番はファイアレッド・リーフグリーンの曲（ファイアレッドの 272〜346 番と同じ並び）
		private static readonly (int Id, string Ja, string En, string Print, string PrintEn)[] emeraldSongs =
		{
			(350, "ミシロタウン（テスト用）", "Littleroot Town (test)", "7f71ac5a3c41", null),
			(351, "38ばんどうろ（金・銀）", "Route 38 (Gold/Silver)", "8d9cbe416c42", null),
			(352, "捕獲成功", "Caught", "93212cd26ad8", null),
			(353, "勝利！野生ポケモン", "Victory! Wild Pokémon", "e1ec42636ef7", null),
			(354, "勝利！ジムリーダー", "Victory! Gym Leader", "6c0d3f429d9a", null),
			(355, "勝利！ポケモンリーグ", "Victory! Pokémon League", "2481f4d91aea", null),
			(356, "ポケモンコミュニケーションセンター（クリスタル）", "Pokémon Communication Center (Crystal)", "fb70d6c6bcb4", null),
			(357, "ニビシティ（金・銀）", "Pewter City (Gold/Silver)", "1d6d17f6fc7c", null),
			(358, "戦闘！伝説のポケモン（クリスタル）", "Battle! Legendary beasts (Crystal)", "235e973a90ff", null),
			(359, "101ばんどうろ", "Route 101", "369b60642c42", null),
			(360, "110ばんどうろ", "Route 110", "ae20e580bf2d", null),
			(361, "120ばんどうろ", "Route 120", "46687bc4afa1", null),
			(362, "トウカシティ", "Petalburg City", "fc2e2b81d976", null),
			(363, "コトキタウン", "Oldale Town", "6b6126184df2", null),
			(364, "ポケモンジム", "Pokémon Gym", "edf597857540", null),
			(365, "なみのり", "Surf", "7d3e718f34f4", null),
			(366, "トウカのもり", "Petalburg Woods", "1e0d376208b0", null),
			(367, "レベルアップ", "Level up", "4ed9521cc1fa", null),
			(368, "ポケモン回復", "Pokémon healed", "49b4e5c16f05", null),
			(369, "バッジ入手", "Obtained a badge", "58c94fd4692e", null),
			(370, "アイテム入手", "Obtained an item", "085e82e008fe", null),
			(371, "進化おめでとう", "Evolved", "eb4df8da464a", null),
			(372, "わざマシン入手", "Obtained a TM/HM", "df1f4f7ed48c", null),
			(373, "ミナモ美術館", "Lilycove Museum", "437d9bf5c0b4", null),
			(374, "122ばんすいどう", "Route 122", "934fdd527baf", null),
			(375, "うみのかがくはくぶつかん", "Oceanic Museum", "ffadf99d55e8", null),
			(376, "進化（前奏）", "Evolution (intro)", "e4240d3900e6", "5867f15f57fe"),
			(377, "進化", "Evolution", "1029d168b6ec", null),
			(378, "わざを忘れた", "Move deleted", "6a9df497e483", "bb4cb46f8fa1"),
			(379, "視線！少女", "Encounter! Girl", "3c36d0951495", null),
			(380, "視線！男の子", "Encounter! Boy", "52636d27fdf5", null),
			(381, "すてられぶね", "Abandoned Ship", "27124b4ec1f9", null),
			(382, "ヒワマキシティ", "Fortree City", "180ad361952e", null),
			(383, "オダマキ研究所", "Birch's Lab", "b79bb631ac38", null),
			(384, "バトルタワー（ルビー・サファイア）", "Battle Tower (Ruby/Sapphire)", "3ce35b6509ac", null),
			(385, "視線！かいパンやろう", "Encounter! Swimmer", "e4083ad1f7e7", null),
			(386, "めざめのほこら", "Cave of Origin", "989c897b4193", null),
			(387, "きのみ入手", "Obtained a berry", "93556eca4d72", null),
			(388, "伝説のポケモンの目覚め", "Legendary Pokémon awakens", "92ce63b18578", null),
			(389, "スロット大当たり", "Slots jackpot", "fa5e4723c27a", null),
			(390, "スロット当たり", "Slots win", "08e0df8488ea", null),
			(391, "残念", "Too bad", "bfe2027d899a", "3224bda9dd7b"),
			(392, "ルーレット", "Roulette", "ce6b82e8414d", null),
			(393, "通信コンテスト（1 人目）", "Link contest (player 1)", "997a7c0eb676", null),
			(394, "通信コンテスト（2 人目）", "Link contest (player 2)", "71121cedc800", null),
			(395, "通信コンテスト（3 人目）", "Link contest (player 3)", "8c943cd56156", null),
			(396, "通信コンテスト（4 人目）", "Link contest (player 4)", "d2d7f56d2612", null),
			(397, "視線！おぼっちゃま", "Encounter! Rich Boy", "af449f72e5f4", null),
			(398, "シダケタウン", "Verdanturf Town", "c29a06a41b42", null),
			(399, "カナズミシティ", "Rustboro City", "4aa1c28d1444", null),
			(400, "ポケモンセンター", "Pokémon Center", "299f0bd8dd79", null),
			(401, "104ばんどうろ", "Route 104", "74f3ce750560", null),
			(402, "119ばんどうろ", "Route 119", "d2126bc2a8c7", null),
			(403, "サイクリング", "Cycling", "1e892d0e3ff9", null),
			(404, "フレンドリィショップ", "Poké Mart", "97afdfbb2671", null),
			(405, "ミシロタウン", "Littleroot Town", "391eb988d360", null),
			(406, "えんとつやま", "Mt. Chimney", "19c2d211ac92", null),
			(407, "視線！女の子", "Encounter! Lass", "d430515f72fc", null),
			(408, "ミナモシティ", "Lilycove City", "3341469cf745", null),
			(409, "111ばんどうろ", "Route 111", "e6604e233a75", null),
			(410, "たすけて！", "Help!", "129ae7083802", null),
			(411, "すいちゅう", "Underwater", "ce9573c6c6a0", null),
			(412, "勝利！トレーナー", "Victory! Trainer", "e131f29e5c9a", null),
			(413, "タイトル", "Title screen", "bf6406e6b7af", null),
			(414, "オープニング", "Opening", "3053911bad89", null),
			(415, "ハルカ登場", "May appears", "9bfcaef87c87", null),
			(416, "視線！（はげしい曲）", "Encounter! (intense)", "ab442a22dc4a", null),
			(417, "視線！（かっこいい曲）", "Encounter! (cool)", "c1200f500f86", null),
			(418, "113ばんどうろ", "Route 113", "2142473db1d7", null),
			(419, "視線！アクア団", "Encounter! Team Aqua", "a080d879b63e", null),
			(420, "ついてきて", "Follow me", "13ef983fd46a", null),
			(421, "ユウキ登場", "Brendan appears", "c73b946e8081", null),
			(422, "サイユウシティ", "Ever Grande City", "5400329eb019", null),
			(423, "視線！（あやしい曲）", "Encounter! (suspicious)", "8ecb25b9e432", null),
			(424, "勝利！アクア団・マグマ団", "Victory! Team Aqua/Magma", "1232fa8338fd", null),
			(425, "ロープウェイ", "Cable Car", "7749edf6d7c5", null),
			(426, "ゲームコーナー", "Game Corner", "f5d8f393c228", null),
			(427, "ムロタウン", "Dewford Town", "b181b120972f", null),
			(428, "サファリゾーン", "Safari Zone", "bd33e4293f01", null),
			(429, "チャンピオンロード", "Victory Road", "c77664861416", null),
			(430, "アクア団・マグマ団のアジト", "Team Aqua/Magma Hideout", "5af3f52d322d", null),
			(431, "連絡船", "Sailing", "fc1f6a562d9c", null),
			(432, "おくりびやま", "Mt. Pyre", "f8cff78a5992", null),
			(433, "カイナシティ", "Slateport City", "24535f33f492", null),
			(434, "おくりびやま（外）", "Mt. Pyre exterior", "4bfe6d3c1336", null),
			(435, "トレーナーズスクール", "Trainer school", "6e263eaf0392", null),
			(436, "殿堂入り", "Hall of Fame", "cc8c9037de3b", null),
			(437, "ハジツゲタウン", "Fallarbor Town", "9c685f59baa3", null),
			(438, "おふれのせきしつ", "Sealed Chamber", "3e6794dff649", null),
			(439, "コンテスト優勝", "Contest winner", "e3898e33f12d", null),
			(440, "コンテスト", "Contest", "d9dba291026c", null),
			(441, "視線！マグマ団", "Encounter! Team Magma", "90bd8c86da46", null),
			(442, "オープニング（戦い）", "Opening (battle)", "23096554ea4f", null),
			(443, "異常気象（大雨）", "Abnormal weather (heavy rain)", "6f764b77208c", null),
			(444, "異常気象（ひでり）", "Abnormal weather (drought)", "dfeb88954f2a", null),
			(445, "ルネシティ", "Sootopolis City", "8bac4452eea8", null),
			(446, "コンテスト結果発表", "Contest results", "40d8448f6c83", null),
			(447, "殿堂入りの部屋", "Hall of Fame room", "38ae237cf255", null),
			(448, "カラクリやしき", "Trick House", "f2ea9873f1d9", null),
			(449, "視線！ふたごちゃん", "Encounter! Twins", "c7b9389b68d3", null),
			(450, "四天王登場", "Elite Four appears", "3a39f302f544", null),
			(451, "視線！やまおとこ", "Encounter! Hiker", "6d2da8334369", null),
			(452, "コンテスト会場（ロビー）", "Contest lobby", "219272ba4ece", null),
			(453, "視線！インタビュアー", "Encounter! Interviewer", "ca83c7cabc61", null),
			(454, "チャンピオン登場", "Champion appears", "ce3f73f61c8c", null),
			(455, "エンディング", "Credits", "4bcaf86d3a64", null),
			(456, "THE END", "The End", "55eb6f93906a", null),
			(457, "バトルフロンティア", "Battle Frontier", "4f3efcb0bbd4", null),
			(458, "バトルアリーナ", "Battle Arena", "ff28cbd760ed", null),
			(459, "バトルポイント入手", "Obtained Battle Points", "be28a9c09e0c", null),
			(460, "エントリーコール登録", "Registered in Match Call", "7f15b098f309", "41e8d9310485"),
			(461, "バトルピラミッド", "Battle Pyramid", "b8b7c36df7c5", null),
			(462, "バトルピラミッド（頂上）", "Battle Pyramid summit", "66e33de33e82", null),
			(463, "バトルパレス", "Battle Palace", "337a7e07e9b6", null),
			(464, "レックウザ登場", "Rayquaza appears", "77383496f6c2", null),
			(465, "バトルタワー", "Battle Tower", "c9af5262f76b", null),
			(466, "シンボル入手", "Obtained a Symbol", "fe71e9afe584", null),
			(467, "バトルドーム", "Battle Dome", "18b998d51958", null),
			(468, "バトルチューブ", "Battle Pike", "92336eac881e", null),
			(469, "バトルファクトリー", "Battle Factory", "1cbe63fce60c", null),
			(470, "戦闘！レックウザ", "Battle! Rayquaza", "0f3757ee6321", null),
			(471, "戦闘！フロンティアブレーン", "Battle! Frontier Brain", "ee48e0a3d0e9", null),
			(472, "戦闘！ミュウ", "Battle! Mew", "00060a9d11bc", null),
			(473, "バトルドーム（ロビー）", "Battle Dome lobby", "84ce779fc238", null),
			(474, "戦闘！野生ポケモン", "Battle! Wild Pokémon", "688a0d0650a0", null),
			(475, "戦闘！アクア団・マグマ団", "Battle! Team Aqua/Magma", "a9e576a84141", null),
			(476, "戦闘！トレーナー", "Battle! Trainer", "eeb1c4af8106", null),
			(477, "戦闘！ジムリーダー", "Battle! Gym Leader", "ff534d332395", null),
			(478, "戦闘！チャンピオン", "Battle! Champion", "e3767a43230d", null),
			(479, "戦闘！レジロック・レジアイス・レジスチル", "Battle! Regirock/Regice/Registeel", "8581b8e7d1b4", null),
			(480, "戦闘！カイオーガ・グラードン", "Battle! Kyogre/Groudon", "0f3757ee6321", null),
			(481, "戦闘！ライバル", "Battle! Rival", "2049cd7a4d6d", null),
			(482, "戦闘！四天王", "Battle! Elite Four", "2aaf6cc5ff06", null),
			(483, "戦闘！アクア団・マグマ団のリーダー", "Battle! Team Aqua/Magma Leader", "fc7591fff48e", null),
			(484, "ついてきて（FR・LG）", "Follow me (FR/LG)", "8bbb726a556d", null),
			(485, "ゲームコーナー（FR・LG）", "Game Corner (FR/LG)", "6e06e3315b60", null),
			(486, "ロケット団のアジト（FR・LG）", "Rocket Hideout (FR/LG)", "9fa90ad9e0cd", null),
			(487, "ポケモンジム（FR・LG）", "Pokémon Gym (FR/LG)", "dd60948e9ba2", null),
			(488, "プリンのうた（FR・LG）", "Jigglypuff's song (FR/LG)", "7bb779f252f3", null),
			(489, "オープニング（戦い）（FR・LG）", "Opening (fight) (FR/LG)", "1b92a90339cd", null),
			(490, "タイトル（FR・LG）", "Title screen (FR/LG)", "0523463c7201", null),
			(491, "グレンタウン（FR・LG）", "Cinnabar Island (FR/LG)", "03e07e6e27b4", null),
			(492, "シオンタウン（FR・LG）", "Lavender Town (FR/LG)", "7bcf3c431970", null),
			(493, "回復（未使用）（FR・LG）", "Heal (unused) (FR/LG)", "a307600d6f8b", "2fd032debad6"),
			(494, "サイクリング（FR・LG）", "Cycling (FR/LG)", "0bf98bab9935", null),
			(495, "視線！ロケット団（FR・LG）", "Encounter! Team Rocket (FR/LG)", "5551fa36e667", null),
			(496, "視線！女の子（FR・LG）", "Encounter! Girl (FR/LG)", "3649b7317350", "0f713275550d"),
			(497, "視線！男の子（FR・LG）", "Encounter! Boy (FR/LG)", "5193f77f14b2", null),
			(498, "殿堂入り（FR・LG）", "Hall of Fame (FR/LG)", "8d6c68246b1c", null),
			(499, "トキワのもり（FR・LG）", "Viridian Forest (FR/LG)", "49c11267d9a1", null),
			(500, "おつきみやま（FR・LG）", "Mt. Moon (FR/LG)", "b1295d6df1e0", null),
			(501, "ポケモンやしき（FR・LG）", "Pokémon Mansion (FR/LG)", "ffb74f9b43d6", null),
			(502, "エンディング（FR・LG）", "Credits (FR/LG)", "294885b689c7", null),
			(503, "1ばんどうろ（FR・LG）", "Route 1 (FR/LG)", "23121a1e1da2", null),
			(504, "24ばんどうろ（FR・LG）", "Route 24 (FR/LG)", "2e9f13565cf9", null),
			(505, "3ばんどうろ（FR・LG）", "Route 3 (FR/LG)", "ea1a7d40294c", null),
			(506, "11ばんどうろ（FR・LG）", "Route 11 (FR/LG)", "34cdf85b6f8a", null),
			(507, "チャンピオンロード（FR・LG）", "Victory Road (FR/LG)", "a7004bb212e7", null),
			(508, "戦闘！ジムリーダー（FR・LG）", "Battle! Gym Leader (FR/LG)", "ab0183eacf49", null),
			(509, "戦闘！トレーナー（FR・LG）", "Battle! Trainer (FR/LG)", "8900dd2fe7e2", null),
			(510, "戦闘！野生ポケモン（FR・LG）", "Battle! Wild Pokémon (FR/LG)", "c62d979061e0", null),
			(511, "戦闘！チャンピオン（FR・LG）", "Battle! Champion (FR/LG)", "08da21096eee", null),
			(512, "マサラタウン（FR・LG）", "Pallet Town (FR/LG)", "000051ac16ed", null),
			(513, "オーキド研究所（FR・LG）", "Oak's Lab (FR/LG)", "cb8456f506c3", null),
			(514, "オーキド博士（FR・LG）", "Professor Oak (FR/LG)", "bf66f3ec84d1", null),
			(515, "ポケモンセンター（FR・LG）", "Pokémon Center (FR/LG)", "928fb145c05a", null),
			(516, "サント・アンヌ号（FR・LG）", "S.S. Anne (FR/LG)", "42e5de09f914", null),
			(517, "なみのり（FR・LG）", "Surf (FR/LG)", "6f8122ec1982", null),
			(518, "ポケモンタワー（FR・LG）", "Pokémon Tower (FR/LG)", "ca1697380475", null),
			(519, "シルフカンパニー（FR・LG）", "Silph Co. (FR/LG)", "3f1e583d00d0", null),
			(520, "セキチクシティ（FR・LG）", "Fuchsia City (FR/LG)", "45007587a6b8", null),
			(521, "タマムシシティ（FR・LG）", "Celadon City (FR/LG)", "7545ac658253", null),
			(522, "勝利！トレーナー（FR・LG）", "Victory! Trainer (FR/LG)", "ca092f1a9194", null),
			(523, "勝利！野生ポケモン（FR・LG）", "Victory! Wild Pokémon (FR/LG)", "8c8498a9fa78", null),
			(524, "勝利！ジムリーダー（FR・LG）", "Victory! Gym Leader (FR/LG)", "b76548e8d58b", null),
			(525, "クチバシティ（FR・LG）", "Vermilion City (FR/LG)", "a1b55fea772a", null),
			(526, "ニビシティ（FR・LG）", "Pewter City (FR/LG)", "c3ec80a7c8c3", null),
			(527, "ライバル登場（FR・LG）", "Rival appears (FR/LG)", "554e3d17feef", null),
			(528, "ライバル退場（FR・LG）", "Rival leaves (FR/LG)", "7cfbf45d1b0a", null),
			(529, "図鑑評価（FR・LG）", "Pokédex rating (FR/LG)", "95c3195f8a5d", null),
			(530, "だいじなもの入手（FR・LG）", "Obtained a key item (FR/LG)", "3bca4c120174", null),
			(531, "捕獲（前奏）（FR・LG）", "Caught (intro) (FR/LG)", "35528119cecd", "9c94d7882f49"),
			(532, "記念撮影（FR・LG）", "Photo (FR/LG)", "c43a62563a64", "7bbe9065d8f3"),
			(533, "ゲームフリークのロゴ（FR・LG）", "Game Freak logo (FR/LG)", "16084c38ae1f", null),
			(534, "捕獲成功（FR・LG）", "Caught (FR/LG)", "a66dbf357a07", null),
			(535, "冒険の説明（FR・LG）", "New game instructions (FR/LG)", "fbc188c8f461", null),
			(536, "冒険のはじまり（FR・LG）", "New game intro (FR/LG)", "3f9213f7c1d2", null),
			(537, "冒険へ出発（FR・LG）", "New game exit (FR/LG)", "ba6ef8dea186", "fd34bd2b2f3a"),
			(538, "ポケモンジャンプ（FR・LG）", "Pokémon Jump (FR/LG)", "859b0f988e99", null),
			(539, "ユニオンルーム（FR・LG）", "Union Room (FR/LG)", "fe6e774c4119", null),
			(540, "ポケモンネットセンター（FR・LG）", "Network Center (FR/LG)", "2a7de7d5ba12", null),
			(541, "ふしぎなおくりもの（FR・LG）", "Mystery Gift (FR/LG)", "bbff9659112a", null),
			(542, "きのみどり（FR・LG）", "Berry picking (FR/LG)", "7ca05bc2eae4", null),
			(543, "ナナシマの洞窟（FR・LG）", "Sevii Islands cave (FR/LG)", "5d866c2c90f2", null),
			(544, "おしえてテレビ（FR・LG）", "Teachy TV show (FR/LG)", "8bbb726a556d", null),
			(545, "ナナシマの道路（FR・LG）", "Sevii Islands route (FR/LG)", "8935467a5f14", null),
			(546, "ナナシマのダンジョン（FR・LG）", "Sevii Islands dungeon (FR/LG)", "49c11267d9a1", null),
			(547, "1・2・3のしま（FR・LG）", "Islands 1-3 (FR/LG)", "c3ec80a7c8c3", null),
			(548, "4・5のしま（FR・LG）", "Islands 4-5 (FR/LG)", "69d4bde1e9f5", null),
			(549, "6・7のしま（FR・LG）", "Islands 6-7 (FR/LG)", "e698404578fb", null),
			(550, "ポケモンのふえ（FR・LG）", "Poké Flute (FR/LG)", "0d6fc1ff26fa", null),
			(551, "戦闘！デオキシス（FR・LG）", "Battle! Deoxys (FR/LG)", "fc98dc5c3238", null),
			(552, "戦闘！ミュウツー（FR・LG）", "Battle! Mewtwo (FR/LG)", "25e2b4a58ee0", null),
			(553, "戦闘！伝説のポケモン（FR・LG）", "Battle! Legendary Pokémon (FR/LG)", "428b87839e67", null),
			(554, "視線！ジムリーダー（FR・LG）", "Encounter! Gym Leader (FR/LG)", "effae629ba87", null),
			(555, "デオキシス登場（FR・LG）", "Deoxys appears (FR/LG)", "fd306aeedb0e", null),
			(556, "トレーナータワー（FR・LG）", "Trainer Tower (FR/LG)", "dd60948e9ba2", null),
			(557, "マサラタウン（ゆっくり）（FR・LG）", "Pallet Town (slow) (FR/LG)", "e395486286ef", null),
			(558, "おしえてテレビ（メニュー）（FR・LG）", "Teachy TV menu (FR/LG)", "80ce6d02fb11", null),
			(0x7FFF, "118ばんどうろ（西は 110ばん、東は 119ばんどうろの曲）", "Route 118 (west: Route 110, east: Route 119)", null, null),
			(0xFFFF, "なし（音楽を止める）", "None (stop the music)", null, null),
		};

		// 曲の表に入っていない特別な番号（これ以上の番号は、曲の表の外の値として扱う）
		private const int FirstSpecialId = 0x7000;

		//-------------------------------------------------------------------------------
		// 読み込んでいる ROM がエメラルド系か
		//-------------------------------------------------------------------------------
		private static bool IsEmerald
		{
			get { return GameProfile.Current != null && GameProfile.Current.EmeraldHeaderLayout; }
		}

		//-------------------------------------------------------------------------------
		// 読み込んでいるゲームの曲の一覧を、同じ形（番号・日本語名・英語名・指紋・もう 1 つの指紋）で返す処理
		//-------------------------------------------------------------------------------
		private static IEnumerable<(int Id, string Ja, string En, string Print, string PrintEn)> Songs()
		{
			if (IsEmerald)
			{
				foreach (var song in emeraldSongs)
				{
					yield return song;
				}
			}
			else
			{
				foreach (var song in songs)
				{
					yield return (song.Id, song.Ja, song.En, song.Print, null);
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 曲（BGM）が始まる番号（それより前は効果音。ファイアレッド 256、エメラルド 350）
		//-------------------------------------------------------------------------------
		public static int FirstMusicId
		{
			get { return IsEmerald ? 350 : 256; }
		}

		//-------------------------------------------------------------------------------
		// 元のゲームの曲の表の件数（効果音を含む。ファイアレッド 347、エメラルド日本語版 559、英語版 610）
		// 英語版エメラルドの 559〜609 番は、曲ではなく声の効果音
		//-------------------------------------------------------------------------------
		public static int OriginalSongCount
		{
			get
			{
				if (!IsEmerald)
				{
					return 347;
				}
				return GameProfile.Current.Code == "BPEJ" ? 559 : 610;
			}
		}

		//-------------------------------------------------------------------------------
		// 元のゲームで、曲ではない音（声の効果音）が入っている番号か（英語版エメラルドの 559〜609 番）
		//-------------------------------------------------------------------------------
		public static bool IsOriginalSoundEffectSlot(int id)
		{
			return IsEmerald && id >= 559 && id <= 609;
		}

		//-------------------------------------------------------------------------------
		// 曲の一覧（番号順）を返す処理
		//-------------------------------------------------------------------------------
		public static IEnumerable<int> Ids
		{
			get
			{
				foreach (var song in Songs())
				{
					yield return song.Id;
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 曲の表に入っていない特別な番号（「なし」など）の一覧を返す処理
		//-------------------------------------------------------------------------------
		public static IEnumerable<int> SpecialIds
		{
			get
			{
				foreach (var song in Songs())
				{
					if (song.Id >= FirstSpecialId)
					{
						yield return song.Id;
					}
				}
			}
		}

		//-------------------------------------------------------------------------------
		// 元のゲームにある曲の番号かを返す処理
		//-------------------------------------------------------------------------------
		public static bool Contains(int id)
		{
			foreach (var song in Songs())
			{
				if (song.Id == id)
				{
					return song.Print != null;
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// その番号の曲の指紋が、元のゲームのものと同じかを返す処理（日本語版・英語版のどちらかと同じなら true）
		//-------------------------------------------------------------------------------
		public static bool IsOriginalPrint(int id, string print)
		{
			if (print == null)
			{
				return false;
			}
			foreach (var song in Songs())
			{
				if (song.Id == id)
				{
					return song.Print == print || song.PrintEn == print;
				}
			}
			return false;
		}

		//-------------------------------------------------------------------------------
		// 指紋が同じ元の曲の番号を返す処理（別の番号へ移された元の曲を見つけるのに使う。無ければ -1）
		//-------------------------------------------------------------------------------
		public static int FindByPrint(string print)
		{
			if (print == null)
			{
				return -1;
			}
			foreach (var song in Songs())
			{
				if (song.Print == print || song.PrintEn == print)
				{
					return song.Id;
				}
			}
			return -1;
		}

		//-------------------------------------------------------------------------------
		// 番号から、今の言語での曲名を返す処理（表に無い番号は null）
		//-------------------------------------------------------------------------------
		public static string GetName(int id)
		{
			foreach (var song in Songs())
			{
				if (song.Id == id)
				{
					return Localizer.Language == Localizer.Japanese ? song.Ja : song.En;
				}
			}
			return null;
		}
	}
}
