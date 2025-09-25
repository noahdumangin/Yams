using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

class YamsGame
{
    // Constantes pour définir le nombre de dés, le nombre maximum de lancers, et le nombre total de manches
    const int NUMBER_OF_DICE = 5;
    const int MAX_ROLLS = 3;
    const int TOTAL_ROUNDS = 13;
    
    static void Main(string[] args)
    {
        // Définition des catégories de score disponibles (une case suppl. pour le bonus de la somme des challenges mineurs)
        string[] challenges = { "Un (la somme de tous les uns)", "Deux (la somme de tous les deux)", 
                                     "Trois (la somme de tous les trois)", "Quatre (la somme de tous les quatre)", 
                                     "Cinq (la somme de tous les cinq)", "Six (la somme de tous les six)", 
                                     "Brelan (trois dés de même valeur)", "Carré (quatre dés de même valeur)", 
                                     "Full (un triplet et une paire)", "Petite Suite (quatre valeurs consécutives)", 
                                     "Grande Suite (cinq valeurs consécutives)", "Yams (cinq dés de même valeur)", 
                                     "Chance (le total des dés obtenus)", "Bonus" };

        // Initialisation des scores et des catégories utilisées pour chaque joueur
        int[] player1Scores = new int[challenges.Length]; // tableau des scores du joueur 1 pour chaque défi
        int[] player2Scores = new int[challenges.Length]; // tableau des scores du joueur 2 pour chaque défi
        bool[] player1ChallengesUsed = new bool[challenges.Length];
        bool[] player2ChallengesUsed = new bool[challenges.Length];

        // Stockage des détails de la partie
        List<Dictionary<int, string[]>> gameDetails = new List<Dictionary<int, string[]>>();

        Console.WriteLine($"\u001b[0;33mau jeu de Yams !");
        Console.WriteLine("\nLe jeu se joue à deux. Il consiste à lancer 2 dés à 6 faces chacun, jusqu'à 3 fois par manche, net à remplir une grille de défis en fonction des résultats.");

        Console.Write("Veuillez entrer le nom du joueur 1 : ");
        string nom1 = Console.ReadLine();
        Console.Write("Veuillez entrer le nom du joueur 2 : ");
        string nom2 = Console.ReadLine();
        string[] noms = new string[2];
        noms[0] = nom1;
        noms[1] = nom2;

        // Boucle principale pour jouer toutes les manches
        for (int round = 1; round <= TOTAL_ROUNDS; round++)
        {
            Console.WriteLine($"\u001b[0;0m\n--- Manche {{round}} ---\n");

            // Stockage des détails de la manche
            Dictionary<int, string[]> roundDetails = new Dictionary<int, string[]>();

            // Tour du joueur 1
            Console.WriteLine("C'est au tour de " + nom1 + " !");
            string[] player1Turn = PlayTurn(player1Scores, player1ChallengesUsed, challenges);
            roundDetails.Add(1,player1Turn);

            // Tour du joueur 2
            Console.WriteLine("\nC'est au tour de " + nom2 + " !");
            string[] player2Turn = PlayTurn(player2Scores, player2ChallengesUsed, challenges);
            roundDetails.Add(2,player2Turn);

            // Ajout des détails de la manche
            gameDetails.Add(roundDetails);
        }

        // Vérification du bonus des challenges mineurs pour chaque joueur
        int[] bonusMineurs = new int[2];
        bonusMineurs[0] = ApplyMinorChallengesBonus(player1Scores, player1ChallengesUsed) ? 35 : 0;
        bonusMineurs[1] = ApplyMinorChallengesBonus(player2Scores, player2ChallengesUsed) ? 35 : 0;

        // Fin de la partie, affichage des scores finaux
        Console.WriteLine("\n--- Partie terminée ! ---\n");
        DisplayFinalScores(player1Scores, player2Scores);

        // Calcul et affichage du gagnant
        if (player1Scores.Sum() > player2Scores.Sum())
        {
            Console.WriteLine(nom1 + " gagne la partie ! Félicitations !");
        }
        else if (player2Scores.Sum() > player1Scores.Sum())
        {
            Console.WriteLine(nom2 + " gagne la partie ! Félicitations !");
        }
        else
        {
            Console.WriteLine("Égalité parfaite !");
        }
        
        // Exportation des détails de la partie en fichier JSON
        int[] totaux = new int[2];
        totaux[0] = player1Scores.Sum();
        totaux[1] = player2Scores.Sum();
        ExportGameDetailsToJson(noms, gameDetails, totaux, bonusMineurs);
    }

    // fonction qui manipule les tableaux du joueur, et retourne les infos du tour sous forme de dictionnaire
    static string[] PlayTurn(int[] playerScores, bool[] challengesUsed, string[] challenges)
    {
        // Initialisation des codes Json correspondants aux défis
        string[] challengesJson = { "nombre1", "nombre2", "nombre3", "nombre4", 
            "nombre5", "nombre6", "brelan", "carre", "full", "petite", "grande", "yams", "chance"};
        // Initialisation des dés et du nombre de lancers
        int[] dice = new int[NUMBER_OF_DICE];
        Random random = new Random();
        int rolls = 0;
        string rollsHistory = ""; // NB: on ne sauvegarde que le dernier lancer

        // Phase de lancer des dés
        while (rolls < MAX_ROLLS)
        {
            // indique le numéro de lancer
            Console.WriteLine($"\nLancer {rolls + 1}/{MAX_ROLLS}");
            for (int i = 0; i < NUMBER_OF_DICE; i++)
            {
                // Au premier lancer, tous les dés sont jetés
                // Lors des lancers suivants, l'utilisateur peut choisir de relancer certains dés
                if (rolls == 0 || !AskKeepDice(i))
                {
                    dice[i] = random.Next(1, 7);
                }
            }

            // Enregistrement du résultat des dés pour ce lancer
            rollsHistory = string.Join(',', dice);

            // Affichage du résultat des dés
            Console.WriteLine("Résultat des dés : " + rollsHistory);

            // Proposition de relancer les dés si le maximum de lancers n'est pas encore atteint
            // Proposition de relancer les dés si le maximum de lancers n'est pas encore atteint
            if (rolls < MAX_ROLLS - 1)
            {
                string input = "";

                // Boucle while pour demander une réponse tant qu'elle est invalide
                while (input != "o" && input != "n")
                {
                    Console.Write("Voulez-vous relancer certains dés ? (o/n) : ");
                    input = Console.ReadLine()?.ToLower();
                }

                if (input == "n")
                {
                    break;
                }
            }

            rolls++;
        }

        // Affichage des catégories disponibles pour le joueur
        Console.WriteLine("\nCatégories disponibles :");
        for (int i = 0; i < challenges.Length-1; i++)
        {
            if (challengesUsed[i]) 
                Console.WriteLine($"\u001b[2m{i + 1}. {challenges[i]}\u001b[0;0m");
            else 
                Console.WriteLine($"{i + 1}. {challenges[i]}");
        }

        // Sélection de la catégorie par le joueur
        int categoryIndex;
        do
        {
            Console.Write("Choisissez une catégorie par son numéro : ");
        } while (!int.TryParse(Console.ReadLine(), out categoryIndex) || 
            categoryIndex < 1 || categoryIndex > 13 || 
            challengesUsed[categoryIndex - 1]);

        // Marquage de la catégorie comme utilisée
        challengesUsed[categoryIndex - 1] = true;

        // Calcul du score pour la catégorie choisie
        int score = CalculateScore(dice, challenges[categoryIndex - 1]);
        playerScores[categoryIndex - 1] = score;

        Console.WriteLine($"Vous avez marqué {score} points dans la catégorie {challenges[categoryIndex - 1]}.");

        // Retour des informations du tour sous forme de dictionnaire
        return new string[3] {rollsHistory, challengesJson[categoryIndex - 1], score+""};
    }

    // Demande au joueur s'il souhaite conserver un dé spécifique
    static bool AskKeepDice(int diceIndex)
    {
        Console.Write($"Voulez-vous relancer le dé {diceIndex + 1} ? (o/n) : ");
        return Console.ReadLine()?.ToLower() == "n";
    }

    // Calcul du score en fonction de la catégorie choisie
    static int CalculateScore(int[] dice, string category)
    {
    // Tri des dés pour faciliter les vérifications (comme les suites)
        Array.Sort(dice);
        switch (category)
        {
            case "Un (la somme de tous les uns)":
                return dice.Count(d => d == 1) * 1; // Somme des dés de valeur 1
            case "Deux (la somme de tous les deux)":
                return dice.Count(d => d == 2) * 2; // Somme des dés de valeur 2
            case "Trois (la somme de tous les trois)":
                return dice.Count(d => d == 3) * 3; // Somme des dés de valeur 3
            case "Quatre (la somme de tous les quatre)":
                return dice.Count(d => d == 4) * 4; // Somme des dés de valeur 4
            case "Cinq (la somme de tous les cinq)":
                return dice.Count(d => d == 5) * 5; // Somme des dés de valeur 5
            case "Six (la somme de tous les six)":
                return dice.Count(d => d == 6) * 6; // Somme des dés de valeur 6
            // Les autres catégories restent inchangées...
            case "Brelan (trois dés de même valeur)":
                return dice.GroupBy(d => d).Any(g => g.Count() >= 3) ? dice.Sum() : 0; // Trois dés identiques
            case "Carré (quatre dés de même valeur)":
                return dice.GroupBy(d => d).Where(g => g.Count() >= 4).Select(g => g.Key * 4).FirstOrDefault(); // Quatre dés identiques
            case "Full (un triplet et une paire)":
                return dice.GroupBy(d => d).Count() == 2 &&
                    dice.GroupBy(d => d).Any(g => g.Count() == 3) ? 25 : 0; // Un triplet et une paire
            case "Petite Suite (quatre valeurs consécutives)":
                return dice.Distinct().OrderBy(d => d).SequenceEqual(new[] { 1, 2, 3, 4 }) ||
                    dice.Distinct().OrderBy(d => d).SequenceEqual(new[] { 2, 3, 4, 5 }) ||
                    dice.Distinct().OrderBy(d => d).SequenceEqual(new[] { 3, 4, 5, 6 }) ? 30 : 0; // Quatre valeurs consécutives
            case "Grande Suite (cinq valeurs consécutives)":
                return dice.Distinct().SequenceEqual(new[] { 1, 2, 3, 4, 5 }) ||
                    dice.Distinct().SequenceEqual(new[] { 2, 3, 4, 5, 6 }) ? 40 : 0; // Cinq valeurs consécutives
            case "Yams (cinq dés de même valeur)":
                return dice.GroupBy(d => d).Any(g => g.Count() == 5) ? 50 : 0; // Cinq dés identiques
            case "Chance (le total des dés obtenus)":
                return dice.Sum(); // Somme des dés, quelle que soit la combinaison
            default:
                return 0;
        }
    }


    // Affichage des scores finaux de chaque joueur
    static void DisplayFinalScores(int[] player1Scores, int[] player2Scores)
    {
        Console.WriteLine("Scores finaux :");
        Console.WriteLine("-------------------------------");
        Console.WriteLine("| Catégorie       | Joueur 1 | Joueur 2 |");
        Console.WriteLine("-------------------------------");

        Console.WriteLine("-------------------------------");
        Console.WriteLine($"| Total           | {player1Scores.Sum(),8} | {player2Scores.Sum(),8} |");
        Console.WriteLine("-------------------------------");
    }

    // Exportation des détails de la partie dans un fichier JSON (manuellement)
    static void ExportGameDetailsToJson(
    string[] names, List<Dictionary<int, string[]>> gameDetails, int[] totals, int[] bonuses)
    {
        try
        {
            // Chemin du fichier JSON
            string filePath = "game_details.json";

            // Création du contenu JSON manuellement
            List<string> jsonLines = new List<string>();
            jsonLines.Add("{");

            // Section parameters
            jsonLines.Add("  \"parameters\": {");
            jsonLines.Add($"    \"code\": \"groupe4-004\",");
            jsonLines.Add($"    \"date\": \"{DateTime.Today.ToString("yyyy-MM-dd")}\"");
            jsonLines.Add("  },");

            // Section players
            int idjoueur=1;
            jsonLines.Add("  \"players\": [");
            for (int i = 0; i <= 1; i++)
            {
                jsonLines.Add("    {");
                jsonLines.Add($"      \"id\": {idjoueur},");
                jsonLines.Add($"      \"pseudo\": \"{names[i]}\"");
                jsonLines.Add(i < 1 ? "    }," : "    }");
                idjoueur= idjoueur+1;
            }
            jsonLines.Add("  ],");

            // Section rounds
            jsonLines.Add("  \"rounds\": [");
            for (int rID = 0; rID < TOTAL_ROUNDS; rID++)
            {
                jsonLines.Add("    {");
                jsonLines.Add($"      \"id\": {rID + 1},");
                jsonLines.Add("      \"results\": [");

                Dictionary<int, string[]> roundResults = gameDetails[rID];
                for (int pID = 1; pID < 3; pID++) // (avec 2 joueurs : j < 2)
                {
                    string[] resultPlayer = roundResults[pID];
                    jsonLines.Add("        {");
                    jsonLines.Add($"          \"id_player\": {pID},");
                    jsonLines.Add($"          \"dice\": [{resultPlayer[0]}],");
                    jsonLines.Add($"          \"challenge\": \"{resultPlayer[1]}\",");
                    jsonLines.Add($"          \"score\": {resultPlayer[2]}");
                    jsonLines.Add(pID < 2 ? "        }," : "        }");
                }

                jsonLines.Add("      ]");
                jsonLines.Add(rID < TOTAL_ROUNDS - 1 ? "    }," : "    }");
            }
            jsonLines.Add("  ],");


            // Section final_result
            jsonLines.Add("  \"final_result\": [");
            for (int i = 0; i < 2; i++)
            {
                jsonLines.Add("    {");
                jsonLines.Add($"      \"id_player\": {i+1},");
                jsonLines.Add($"      \"bonus\": {bonuses[i]},");
                jsonLines.Add($"      \"score\": {totals[i]}");
                jsonLines.Add(i < 1 ? "    }," : "    }");
            }
            jsonLines.Add("  ]");

            jsonLines.Add("}");

            // Écriture dans le fichier
            File.WriteAllText(filePath, string.Join("\n", jsonLines));

            Console.WriteLine($"\nLe déroulement de la partie a été exporté avec succès dans le fichier : {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nErreur lors de l'exportation des détails de la partie : {ex.Message}");
        }
    }

    static bool ApplyMinorChallengesBonus(int[] playerScores, bool[] challengesUsed)
    {  
        // Calcul de la somme des scores des catégories mineures (Un à Six)
        int minorChallengesScore = playerScores[0] + playerScores[1] + playerScores[2] + playerScores[3] + playerScores[4] + playerScores[5];

        // Vérification si la somme atteint ou dépasse 63
        if (minorChallengesScore >= 63)
        {
            // Attribution du bonus de 35 points
            playerScores[playerScores.Length - 1] = 35; // On ajoute au dernier score ("Bonus")
            challengesUsed[challengesUsed.Length-1] = true;
            Console.WriteLine("Félicitations ! Vous recevez un bonus de 35 points pour avoir atteint 63 points dans les challenges mineurs.");
            return true;
        }
        return false;
    }
}