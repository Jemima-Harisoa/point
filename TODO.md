# TODO List : reprise et adaptation du projet de jeu de point (rattrapage S4)

## Logique Metier :   
- Les joueurs vont avoir chacun une barre pour lancer des missiles 
    - Les joueurs peuvent choisir d'utiliser leur tour soit pour lancer un missile, soit pour placer un point sur la grille.
    - Le missile disparaît après avoir pris contact avec le premier point enemis dans sa trajectoire, et détruit ce point.
    - Le lancement d'un missile est limité à une fois par tour, 
    - Pour lancer un missile, le joueur  choisit une ligne de l'axe des ordonne y et une puissance de lancement (une valeur entière entre 1 et 9 : 1 etat la prmiere ligne de la grille, 9 etant la derniere ligne de la grille)
    - Le missile se deplace horizontalement vers le cote de l'adversaire, et detruit le premier point ennemi qu'il rencontre sur sa trajectoire. 
    - Pour determiner sur quelle ligne / interserction le missile va tomber (ex : taille de grille 18x18, puissance de lancement 5, le missile va tomber sur l'intersection 18 * 5 / 9 = 10 donc 10 eme ligne de la grille, le choix colone sera fait avec les touche gauche droite du clavier ou avec la souris, on prend que les entier pour determiner la ligne d'impact du missile, si le missile tombe sur une ligne ou une colone deja occupé par un point ennemi, le missile detruit ce point et disparait, sinon le missile disparait sans rien detruire.) 

## TO DO :

### [] Refactirisation de code :
- Restructurer le code pour améliorer sa lisibilité et sa maintenabilité.
    - [X] Ajout de commentaires pour expliquer les différentes parties du code.
    - [X] Alleger le contenu du code que ce soint moins lourd et plus facile à faire tourner. 
- [X] Commenter les logiques de jeu sur les suggestion de point et le condition de victoire dans le cas ou un jour alignerait 5 points en L (On ne supprime rien on empeche les regle qui ) 


### [X] Classe missile :
- Créer une classe `Missile` pour gérer les propriétés et les comportements des missiles.
    - [X] Propriétés : position, puissance de lancement, état (en vol, détruit, etc.).
    - [X] Méthodes : déplacement du missile, détection de collision avec les points ennemis, destruction du point ennemi.
    - [X] Affichage : gérer l'affichage du missile sur la grille. (the class draw itself cf la classe point, ligne etc)
- Integerer la logique de lancement de missile dans le tour de jeu, en s'assurant que les joueurs ne peuvent lancer qu'un missile par tour ou placer un point :
    - [X] Faire aparaitre la barre de lancement de missile pour chaque joueur, et permettre aux joueurs de choisir la ligne de lancement et la puissance du missile.
    - [X] Gérer les interactions entre les missiles et les points ennemis, en mettant à jour l'état du jeu en conséquence (ex : destruction du point ennemi, mise à jour de la grille, etc.).
- [X] Ajouter un input pour parametrer le nombre de colone de et ligne de la grille avec le showdialog de la classe window.designer.cs

### [X] Persistance de données : PostgresQl
- [X] Modeliser le schema de donnees pour le jeu de point, en identifiant les entités principales (joueurs, parties, scores, etc.) et leurs relations.
- [X] Intégrer une base de données PostgreSQL pour stocker les informations sur les joueurs, les parties, les scores, etc.
    - [X] Créer une classe `DatabaseManager` pour gérer les interactions avec la base de données (connexion, requêtes, etc.).
    - [X] Créer les modèles de données (PlayerModel, GameModel, GamePointModel, MissileActionModel)
    - [X] Implémenter l'historisation complète (soft delete) des points supprimés
    - [X] Enregistrer les missiles avec point d'impact et joueur lanceur
    - [X] Stocker les informations sur les joueurs (nom, score, etc.) et les parties (date, durée, résultat, etc.) dans la base de données.
    - [X] Créer GameSaveManager pour gérer sauvegarde hybride (local + PostgreSQL)
    - [ ] Permettre aux joueurs de consulter leur historique de parties et leurs scores via l'interface utilisateur.
    - [X] Continuer la sauvegarde local du jeu.
    - [ ] Mettre un modal pour determiner si on migre la partie en cours en base de donnees ou si on continue la partie en cours localement.

- [X] Ajouter un environnemt de dev sur docker pour faciliter le développement et la gestion de la base de données PostgreSQL.
    - [X] Créer docker-compose.yml pour PostgreSQL
    - [X] Créer le schéma SQL complet avec tables, index, vues et fonctions
