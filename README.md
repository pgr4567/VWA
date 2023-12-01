# VWA
Das ist ein Spiel, dass ich für meine vorwissenschaftliche Arbeit schreibe. Es soll Multiplayer-Spieleentwicklung anhand eines Beispiels demonstrieren.

## Architektur
Das Spiel ist simpel aufgebaut, es gibt eine Lobby, von der aus die Spieler verschiedene Minigames spielen können. Zusätzlich zum Gameserver, kommuniziert dieser auch mit einem Authentifizierungsserver. Über den öffentlichen Dataserver kann der Spielclient Informationen wie die aktuelle Version abfragen.
Für die Kommunikation habe ich [Mirror Networking](https://mirror-networking.com/) verwendet.

Um möglichst viele Aspekte zu demonstrieren, gibt es Grundgerüste von den häufigsten Features in derartigen Multiplayerspielen. Ein simples Chatsystem, Freundesystem, Rangsystem, sowie Shop- und Inventarsystem ist implementiert. Es gibt insgesamt 2 Minispiele.
