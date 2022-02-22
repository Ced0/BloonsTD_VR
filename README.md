#Test Unitaire

Chaque scène contient une fonctionnalités. J'ai tous mis sur la gachette pour aller plus vite, et seul la scène du levier à une main droite.

##CatchThrow

Pointé la manette dans la direction de la balle avec une approximation de moins de 30 degrée, appuyer et maintenir la gachette appuyé jusqu'à ce que la balle soit dans votre main. Relacher la gachette pour lacher/lancer la balle.

Vous pouvez continuer a appelé une balle pendant que vous vous téléporter.

Il faudrait définir des limites de zones pour que la balle ne sorte pas de la map.

Il faut probalement que je fasse une médiane pour stabiliser le résultat et peut être augmenté la vélocité au lancer.

##HUD

Simple démonstrationde comment on peut utiliser les caméras layer pour créer un HUD. En gros un canvas spatial est toujours afficher par dessus les autre éléments et suis la tête du joueur.

##Lever

Vous pouvez utiliser n'importe quel mains pour celui-ci.

Metter votre main en contact avec le levier -la manette devrait ce mettre à vibrer- appuyer sur la gachette à partir d'un certains angle le levier devrait ce bloquer dans une position -il faudrait rajouter un bruit genre clac-, pour pouvoir à nouveau déplacer le levier avec cette main là -il peut toujours le déplacer avec l'autre-, le joueur doit relacher la gachette et saisir à nouvreau le levier.

Le levier devrait fonctionner pour n'importe quel taille et position en théorie.

##PlacingTurret

Un pointeur laser au bout de la main et un fantôme de la tour à poser indique ou la tour sera posée. Appuyer puis relacher la gachette pour placer la tour.

Il y a une correction des collision pour le fantôme, répresentant donc vraiment ou sera poser la tour. La tour sera poser sur un éléments du layer Floor sauf si la correction de collision la plus proche et de mettre la tour sur un autre éléments -à décider ce qu'on veut faire dans ce cas là-.

##Teleport

Appuyer sur la gachette pour faire apparaitre le pointeur laser et le fantôme du joueur ou il sera "téléporté". Relacher la gachette pour être "téléporté".

Correction de collision comme pour la tour voir au dessus.

J'utilise une interpolation pour créer un dash, pendant le dash le joueur ne rentre pas en collision, comme s'il était vraiment téléporté.