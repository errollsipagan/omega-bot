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

## 💻 Getting Started <a name="getting-started"></a>
Soon...

## 🔭 Future Features <a name="future-features"></a>

- [ ] **[new_feature_1]**
- [ ] **[new_feature_2]**
- [ ] **[new_feature_3]**


## 🤝 Contributing <a name="contributing"></a>

Contributions, issues, and feature requests are welcome!

Feel free to check the [issues page](../../issues/).


## 🔗Community
YGO Omega is developed in [Duelist Unite](https://duelistsunite.org/) community, join the [https://discord.gg/duelistsunite](https://gitlab.com/uploads/-/system/group/avatar/5775229/53418547.png?width=48), where you can ask questions and make discussions.

## 👥 Authors <a name="authors"></a>

- Kunogi ([@Kunogi](https://gitlab.com/Kunogi)) - Creating Duelists Unite Omega Custom Duel Bot API.
- BBeretta ([@BPBCoding](https://gitlab.com/BPBCoding))

## 🐛 Known Issues <a name="faq"></a>

- [ ] `AttackableCards[index].DeclareAttack()`-> When choosing the attacker (e.g., index = 2) [index is the monster that will attack, in this case, my field with 3 monsters], the index is reset to zero when it enters DuelBotWrapper.cs.

- [ ] OnSelectDisfield -> It's not operating correctly, and the game crashes when it's called. When the Bot summons Ojama King, OnSelectDisfield is called, and when it reaches the line of code `pkt.Write(selectDisfieldResponse);`, the duel ends unexpectedly.

## ❓ FAQ <a name="faq"></a>

> Add at least 2 questions new developers would ask when they decide to use your project.

- **[Question_1]**

  - [Answer_1]

- **[Question_2]**

  - [Answer_2]
