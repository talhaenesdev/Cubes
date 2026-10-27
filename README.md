<p align="center">
  <img src="docs/images/banner.png" alt="CUBES — a noir stealth-shooter" width="100%">
</p>

<p align="center">
  <b>A noir, side-on stealth-shooter about a charismatic rectangle, a city of shape families, and one shop at a time.</b>
</p>

<p align="center">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-6000.3-black?logo=unity">
  <img alt="Platforms" src="https://img.shields.io/badge/platforms-Windows%20%7C%20Android-8c161c">
  <img alt="Languages" src="https://img.shields.io/badge/languages-English%20%7C%20T%C3%BCrk%C3%A7e-c4a05c">
  <img alt="Status" src="https://img.shields.io/badge/status-prototype-555">
</p>

<p align="center">
  <a href="https://horseman7.itch.io/cubes"><img alt="Play on itch.io" src="https://img.shields.io/badge/PLAY%20ON-itch.io-fa5c5c?style=for-the-badge&logo=itchdotio&logoColor=white"></a>
</p>

<p align="center">
  <a href="#english">English</a> · <a href="#türkçe">Türkçe</a>
</p>

---

## English

The city sleeps; your work begins. Every shop in town pays one of the shape families - the Triangles, the Pentagons,
the Hexagons, the Diamonds. You are the Square family's man in the trench coat, and your job is to make every one
of them pay **you** instead.

Walk in, put their armed men on the floor, leave the civilians alone, and have a word with the owner. Once they
agree, the shop works for you - in real time, even while the game is closed. Then the other families come to take it
back.

<p align="center">
  <img src="docs/images/city_map.png" width="100%" alt="The city map: every shop, its family, its income and its raid risk">
</p>

<p align="center">
  <img src="docs/images/persuasion.png" width="49%" alt="The owner, once his men are down">
  <img src="docs/images/firefight.png" width="49%" alt="A firefight in the docks">
</p>

### Features

- **Take the city shop by shop.** Clear a shop's guards, then talk its owner round in speech bubbles - and it pays you from then on.
- **A living city map.** Every shop, the family it works for, what it earns, how often it gets raided. Post your own men to guard it.
- **Real-time income.** Shops pay on the clock, open or closed. Stay away too long and you lose a shop for every day you were gone.
- **Raids while you play.** Enemy families hit your shops at random - the richer the shop, the harder they hit. Go and help, and fight beside your men; stay home, and they hold it or they don't.
- **Guards that fight smart.** They take cover behind crates and pop up to shoot; lookouts run to warn the others first.
- **Deadbolt-style stealth.** Kill the lights, hide in vents, kick doors, take the stairwells. Bullets are real and can be dodged; barrels explode and lamps fall.
- **Twelve locations,** from a quiet laundromat to the Don's penthouse.
- **A hideout to come home to** - a wardrobe, a weapon locker, an upgrade bench, a tape deck with a volume knob, and a cat.
- **Cash from the fallen** buys upgrades. Weapons come home with you.
- **Saves everything,** down to a contract in progress.
- **English and Turkish**, Windows and Android (touch controls included).

<p align="center">
  <img src="docs/images/hideout.png" width="32%" alt="The hideout">
  <img src="docs/images/phone_call.png" width="32%" alt="The broker calls">
  <img src="docs/images/tape_deck.png" width="32%" alt="The tape deck">
</p>
<p align="center">
  <img src="docs/images/mobile.png" width="32%" alt="Touch controls">
  <img src="docs/images/persuasion_2.png" width="32%" alt="Talking the owner round">
  <img src="docs/images/title.png" width="32%" alt="The title screen">
</p>

### Controls

| Action | PC | Android |
|---|---|---|
| Move / crouch | `A` `D` / `Ctrl` | Left stick (pull down to crouch) |
| Jump | `Space` | ▲ |
| Aim and fire | Mouse, left click | Touch the game anywhere |
| Melee | Right click / `F` | F |
| Use, talk, doors, stairs | `E` | E |
| Reload / retry | `R` | R |
| Swap weapon | `Q` | Q |
| Next song | `T` | ♪ |
| Zoom | Mouse wheel | Pinch |
| Pause menu | `Esc` | MENU |

### Playing it

Builds for Windows and Android are on **[itch.io](https://horseman7.itch.io/cubes)**. To run it from source, open the project in
**Unity 6000.3** (6000.3.26f1) and press Play in `Assets/Modules/MainModule/Scenes/MainScene.unity`.

### Under the hood

CUBES is built on **[FlowIoC](https://github.com/FlowArc/FlowIoC)**, a signal-driven IoC framework for Unity. The game is
split into modules that never reference each other - they are wired together by Connectors:

| Module | What it does |
|---|---|
| `GameplayModule` | The run: player, guards, bullets, props, the hideout, the city and its raids (`TerritoryModel`) |
| `GameplayScreenModule` | The HUD, the city map, the tape deck, phone calls, terminals |
| `MainModule` | Boot, the title screen and pause panel, settings and credits |
| `LoadingModule` | Loading sets and the loading screen |
| `LocalizationModule` | Every on-screen text, in English and Turkish |
| `AudioSettingsModule` | Master and music volume, saved |
| `ConnectorModule` | The wiring between all of the above |

Almost everything is tuned from colour-coded config assets in the inspector: `CD_Weapons`, `CD_Characters`,
`CD_Gameplay`, `CD_Territory`, `CD_Missions`, `CD_Families`, `CD_Sounds`. Level prefabs, characters, sounds and
effects are generated by an in-editor asset builder.

### Credits

Made by **Talha Enes İslamoğlu**.

Music by **Kevin MacLeod** ([incompetech.com](https://incompetech.com)), licensed under
[Creative Commons: By Attribution 4.0](http://creativecommons.org/licenses/by/4.0/):
*Burn the World Waltz, Waltz Primordial, Boogie Party, El Magicia, Exhilarate, Feelin Good, Metalmania, Motherlode,
Noise Attack, Sax Rock and Roll, Surf Shimmy, Twisted, Welcome to the Show.*

Sound effects were made for the game. Built with Unity.

---

## Türkçe

Şehir uyuyor; senin işin başlıyor. Şehirdeki her dükkân şekil ailelerinden birine çalışıyor: Üçgenler, Beşgenler,
Altıgenler, Elmaslar. Sen trençkotlu, Kare ailesinin adamısın. İşin, hepsinin bundan sonra **sana** ödemesini sağlamak.

İçeri gir, silahlı adamlarını yere ser, sivillere dokunma ve patronla konuş. İkna olduğunda dükkân senin için
çalışmaya başlar; gerçek zamanlı, oyun kapalıyken bile. Sonra diğer aileler onu geri almaya gelir.

### Özellikler

- **Şehri dükkân dükkân ele geçir.** Korumaları temizle, patronu konuşma balonlarıyla ikna et; dükkân artık sana öder.
- **Yaşayan bir şehir haritası.** Her dükkânın hangi aileye çalıştığı, getirisi, ne sıklıkla baskın yediği. Kendi adamlarını koruma olarak koy.
- **Gerçek zamanlı gelir.** Dükkânlar oyun açık da olsa kapalı da olsa öder. Çok uzun süre uzak kalırsan her gün için bir dükkân kaybedersin.
- **Oynarken baskınlar.** Düşman aileler dükkânlarına rastgele saldırır; dükkân ne kadar zenginse saldırı o kadar sert olur. Yardıma gidip adamlarınla birlikte savaş, ya da onlar tek başına dayansın.
- **Akıllı düşmanlar.** Kasaların arkasına siper alıp ateş ederler; gözcüler önce diğerlerine haber vermeye koşar.
- **Deadbolt tarzı gizlilik.** Işıkları söndür, havalandırmada saklan, kapıları tekmele. Mermiler gerçektir ve kaçılabilir; variller patlar, lambalar düşer.
- **On iki mekân,** sakin bir çamaşırhaneden Don'un çatı katına.
- **Eve dönünce bir sığınak:** gardırop, silah dolabı, geliştirme tezgâhı, ses düğmeli kasetçalar ve bir kedi.
- **Düşenlerden düşen para** ile kendini geliştir; bulduğun silahlar seninle eve gelir.
- **Her şey kaydedilir,** yarım kalan görev dahil.
- **Türkçe ve İngilizce**, Windows ve Android (dokunmatik kontrollerle).

Windows ve Android sürümleri **[itch.io](https://horseman7.itch.io/cubes)**'da. Kaynaktan çalıştırmak için projeyi **Unity 6000.3** (6000.3.26f1) ile açıp
`Assets/Modules/MainModule/Scenes/MainScene.unity` sahnesinde Play'e bas.

Müzikler **Kevin MacLeod** ([incompetech.com](https://incompetech.com)) - CC BY 4.0. Oyunu yapan **Talha Enes İslamoğlu**.
