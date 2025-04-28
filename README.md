<a name="readme-top"></a>

<div align="center">
  <img src="https://avatars.githubusercontent.com/u/53418547" alt="logo" width="140"  height="auto" />
  <br/>

  <h3><b>YGO OMEGA BOT API</b></h3>

</div>

# 📖 YGO OMEGA BOT API (Duel Bot Wrapper) <a name="about-project"></a>

Duel Bot Wrapper is Omega Custom Duel BOT API of [Yu-Gi-Oh!](https://www.yugioh-card.com/en/) game written in [C#](https://dotnet.microsoft.com/en-us/languages/csharp). Compatible with [YGO Omega - The Ultimate Yu-Gi-Oh! Simulator.](https://omega.duelistsunite.org/)

## ✨ Features

- **Create your BOT**
  - It allows you to create and customize your own bot to interact with the game, automating actions like playing cards and responding to effects. "Create your bot" means building a personalized bot to suit your needs in the game. 
  - You can create it in any format (Master Duel, Rush Duel, Speed Duel...)
- **Soon...**
  - Soon....

## 💻 Getting Started

  - [🎮 **YGO Omega** ]
    - Run YGO Omega
    - Create a Duel
    - Select AI_CUSTOM

  - [🛠 **Bot Actions** ]
    - Run Bot (Visual Studio or Bot.exe)
    - Select the Duel Format
    - Select the Deck
    - It's time to duel!!


## 🔭 Future Features <a name="future-features"></a>

- [ ] **[Insert the Deck Hash directly from the console and the bot will execute its basic actions.]**
- [ ] **[Create default actions for the most common cards.]**
- [ ] **[Soon...]**


## 🤝 Contributing <a name="contributing"></a>

Contributions, issues, and feature requests are welcome!

Feel free to check the [issues page](../../issues/).


## 🔗Community
YGO Omega is developed in [Duelist Unite](https://duelistsunite.org/) community, join the [https://discord.gg/duelistsunite](https://gitlab.com/uploads/-/system/group/avatar/5775229/53418547.png?width=48), where you can ask questions and make discussions.

## 👥 Authors <a name="authors"></a>

- Kunogi ([@Kunogi](https://gitlab.com/Kunogi)) - Creating Duelists Unite Omega Custom Duel Bot API.
- BBeretta ([@BPBCoding](https://gitlab.com/BPBCoding)) - Creation of templates and bot actions.
- IceYGO/windbot ([IceYGO/windbot](https://github.com/IceYGO/windbot)) - Bots and flow ideas are extracted from this project. Credit goes to their contributions.


## 🐛 Known Issues <a name="faq"></a>

- [ ] `AttackableCards[index].DeclareAttack()`-> When choosing the attacker (e.g., index = 2) [index is the monster that will attack, in this case, my field with 3 monsters], the index is reset to zero when it enters DuelBotWrapper.cs.

- [ ] OnSelectDisfield -> It's not operating correctly, and the game crashes when it's called. When the Bot summons Ojama King, OnSelectDisfield is called, and when it reaches the line of code `pkt.Write(selectDisfieldResponse);`, the duel ends unexpectedly.

## ❓ FAQ <a name="faq"></a>

- **[Question_1]**

  - [Answer_1]

- **[Question_2]**

  - [Answer_2]
