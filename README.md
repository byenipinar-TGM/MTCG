# MTCG
SWEN Semesterproject von Enes Yeinpinar

#### Protocol about the technical steps you made (designs, failures and selected solutions):

*Failures*
Hierbei habe ich mir sehr schwergetan, die ersten Schritte des Projekts zu bewältigen. Denn zuvor, beispielsweise bei der Arbeit, war es immer so, dass ich ein vorhandenes Projekt übernommen habe, mich damit auseinandergesetzt und es dann weiterentwickelt habe. Deswegen fiel es mir schwer, einen Anfang zu finden. Das war der Grund dafür, dass ich mehrere Kolleginnen und Kollegen gefragt habe und letztendlich zu einem Entschluss gekommen bin. Das zweite Problem war dann, die passende Datenbankstruktur zu finden, wofür ich erneut Hilfe in Anspruch genommen habe. Nachdem die ersten Schritte getan waren, kam ich gut zurecht und habe das Projekt erfolgreich abgeschlossen.

*Designs*
Ich habe die Klassen so einfach wie möglich gestaltet, weil ich persönlich der Meinung bin, dass es am einfachsten ist, sich so zurechtzufinden (wenige Klassen und wenige Ordner). Das Wesentliche wurde in mehrere Klassen unterteilt, nämlich Data.cs, Request.cs, Response.cs und die Objekte. Während des Programmierens habe ich entschieden, welche weiteren Klassen relevant sein könnten, und diese dann erstellt. Persönlich würde ich das nächste Mal eine andere Herangehensweise wählen.

*Selected Solutions*
Wie auch in anderen Bereichen war der erste Ansatz, zunächst zu googeln und selbstverständlich eine Lösung zu finden. Besonders hilfreich erwies sich das gemeinsame Programmieren, da dadurch Unklarheiten schnell geklärt und zu einer Lösung gefunden werden konnten. Zusammengefasst: Google + Kollegen:innen.

#### Explain why these unit tests are chosen and why the tested code is critical:

Ich habe diese speziellen Unit-Tests für die Data- und DataBattleTrade-Klasse ausgewählt, weil sie alle Funktionen beinhalten. Die Tests berücksichtigen verschiedene Fälle, prüfen die Interaktion mit der Datenbank, und stellen sicher, dass die Klasse zuverlässig arbeitet. Durch die Auswahl verschiedener Szenarien und Randfälle stellen diese Tests sicher, dass die Data-Klassen gut funktioniert und leicht gewartet werden kann. Es wurden explizit Methoden gewählt bei denen ich mir persönlich unsicher war, die aber einen erheblichen Einfluss aber das Project haben.

#### Track the time spent with the project + lessons learned: 

Aufgrund dessen, dass ich erst in diesem Jahr intensiveren Kontakt mit C# hatte, habe ich länger gebraucht als gedacht. In den Jahren zuvor habe ich hauptsächlich mit Java gearbeitet, was sowohl Vor- als auch Nachteile hatte. Anfangs hielt ich zu stark daran fest, dass Java und C# sehr ähnlich sind, was der Grund dafür war, dass ich erst später mit dem Projekt begonnen habe. Doch nach einer gewissen Zeit, die ich dem Projekt gewidmet habe, habe ich mich schnell an C# gewöhnt und bin anschließend zügig vorangekommen. Die Gesamtzeit für das Projekt betrug etwa **90 Stunden**. Was ich aus dem Ganzen mitgenommen habe, ist die Server-Client-Verbindung. Des Weiteren habe ich auch erstmals mit Threads in C# gearbeitet. Viele Technologien waren für mich in C# aufgrund meiner begrenzten Erfahrung neu, aber aufgrund meiner vorherigen Erfahrung waren das immer wieder Themen, die mir nicht fremd waren. Dadurch konnte ich Vieles einfacher verstehen.

#### Unique Feature:

Eine zusätzliche Spalte namens "first_login" wurde erstellt. Diese speichert ab, ob der Benutzer sich zum ersten Mal eingeloggt hat oder nicht. Zu Beginn ist sie auf "true" gesetzt, und wenn sich der Benutzer zum ersten Mal einloggt, wird sie dann auf "false" gesetzt. Darüber hinaus gibt es eine weitere Spalte für einen Verifizierungsbonus. Nach der Registrierung muss der Benutzer sich einloggen und eine Stunde abwarten. Nachdem eine Stunde vergangen ist, wird diese Spalte mit "verified" gefüllt, und der Benutzer erhält nach einigen Stunden zusätzlich 20 Coins. Eine weitere Spalte namens "first_login_time" speichert einen Zeitstempel, wann er sich eingeloggt hat, damit das Programm weiß, ab wann es zu zählen beginnen soll (um die echte Verifizierung zu simulieren). Ich denke jedoch, es wäre besser, wenn ich das weiter erkläre.

#### Link to Git: https://github.com/byenipinar-TGM/MTCG



