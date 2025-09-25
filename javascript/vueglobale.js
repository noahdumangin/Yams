document.addEventListener('DOMContentLoaded', () => { 
    // Quand tout est prêt sur la page, on commence
    const urlBaseApi = 'http://yams.iutrs.unistra.fr:3000'; 
    // Voici l'adresse de l'API qu'on va utiliser pour récupérer les infos
    const sectionAffichage = document.querySelector('section'); 
    // On choisit l'endroit sur la page où on va afficher les infos
    const champIdPartie = document.getElementById('game-id'); 
    // On prend la case où on met l'ID de la partie
    const boutonRecuperer = document.getElementById('fetch-game'); 
    // On prend le bouton sur lequel on clique pour récupérer les infos

    // Cette fonction va récupérer les paramètres de la partie
    const recupererParametres = (idPartie) => {
        return fetch(`${urlBaseApi}/api/games/${idPartie}/parameters`) 
            // On demande à l'API les paramètres de la partie avec l'ID qu'on a
            .then(reponse => reponse.json()) 
            // Quand on reçoit la réponse, on la transforme en format compréhensible
            .then(donnees => {
                return `
                `; 
                // On prépare les informations qu'on va afficher : le code et la date
            });
    };

    // Cette fonction va récupérer la liste des joueurs
    const recupererJoueurs = (idPartie) => {
        return fetch(`${urlBaseApi}/api/games/${idPartie}/players`) 
            // On demande à l'API de nous dire qui joue dans cette partie
            .then(reponse => reponse.json()) 
            // Quand on a la réponse, on la transforme pour pouvoir l'utiliser
            .then(donnees => {
                const nomsJoueurs = Object.fromEntries(
                    donnees.map(joueur => [joueur.id, joueur.pseudo]) 
                    // On transforme la liste de joueurs en un objet avec leurs noms
                );

                const listeJoueurs = donnees.map(joueur => `<li>${joueur.pseudo}</li>`).join(''); 
                // On fait une liste HTML des pseudos des joueurs

                return {
                    html: `
                        <h3>Joueurs</h3>
                        <ul>${listeJoueurs}</ul>
                    `, 
                    // On retourne la liste des joueurs prête à être affichée
                    nomsJoueurs 
                    // On retourne aussi les noms des joueurs sous forme d'objet
                };
            });
    };

    // Cette fonction va récupérer les résultats des différents tours du jeu
    const recupererTours = (idPartie, nomsJoueurs) => {
        const nombreMaxTours = 13; 
        // On sait qu'il y a 13 tours dans le jeu
        const promessesTours = []; 
        // On prépare un tableau pour garder les informations de tous les tours

        // On commence une boucle pour chaque tour
        for (let i = 1; i <= nombreMaxTours; i++) { 
            promessesTours.push(
                fetch(`${urlBaseApi}/api/games/${idPartie}/rounds/${i}`) 
                // On demande à l'API les résultats de chaque tour
                    .then(reponse => {
                        if (!reponse.ok) throw new Error(`Tour ${i} non trouvé`); 
                        // Si on n'a pas trouvé ce tour, on crée une erreur
                        return reponse.json(); 
                        // Sinon, on transforme la réponse en un format utilisable
                    })
                    .then(donnees => {
                        const resultats = donnees.results.map((resultat, index) => `
                            ${index > 0 ? '<br>' : ''} 
                            <li>
                                <u>${nomsJoueurs[resultat.id_player]}</u>:<br>
                                Défis: ${resultat.challenge}<br>
                                Dés: ${resultat.dice.map(de => {
                                    // Ici, on décide quelle image de dé montrer en fonction de la valeur du dé
                                    let nomImageDe;
                                    switch (de) {
                                        case 1:
                                            nomImageDe = 'un.png';
                                            break;
                                        case 2:
                                            nomImageDe = 'deux.png';
                                            break;
                                        case 3:
                                            nomImageDe = 'trois.png';
                                            break;
                                        case 4:
                                            nomImageDe = 'quatre.png';
                                            break;
                                        case 5:
                                            nomImageDe = 'cinq.png';
                                            break;
                                        case 6:
                                            nomImageDe = 'six.png';
                                            break;
                                        default:
                                            nomImageDe = 'default.png'; // Si ça ne marche pas, on utilise une image par défaut
                                    }
                                    return `<img src="./img/${nomImageDe}" alt="Dé ${de}" style="width:30px; height:30px; margin-right:5px;">`; 
                                    // On affiche l'image du dé
                                }).join('')}<br>
                                Score: ${resultat.score} 
                                <!-- On affiche le score du joueur pour ce tour -->
                            </li>
                        `).join('');
                        return `
                            <h4>Tour ${donnees.id}</h4>
                            <ul>${resultats}</ul>
                        `; 
                        // On prépare tout le HTML des résultats pour ce tour
                    })
            );
        }

        return Promise.all(promessesTours).then(tours => `
            <h3>Tours</h3>
            ${tours.filter(tour => tour).join('')} 
        `);
    };

    // Cette fonction va récupérer le résultat final du jeu
    const recupererResultatFinal = (idPartie, nomsJoueurs) => {
        return fetch(`${urlBaseApi}/api/games/${idPartie}/final-result`) 
        // On demande à l'API le résultat final de la partie
            .then(reponse => reponse.json()) 
            // On transforme la réponse en format JSON
            .then(donnees => {
                const resultatsFinaux = donnees.map((resultat, index) => `
                    ${index > 0 ? '<br>' : ''} 
                    <li>
                        <u>${nomsJoueurs[resultat.id_player]}</u>:<br>
                        Bonus: ${resultat.bonus}<br>
                        Score total: ${resultat.score} 
                    </li>
                `).join('');
                return `
                    <h3>Résultat final</h3>
                    <ul>${resultatsFinaux}</ul> 
                `;
            });
    };

    // Quand on clique sur le bouton, on récupère l'ID de la partie et on cherche les infos
    boutonRecuperer.addEventListener('click', () => { 
        const idPartie = champIdPartie.value.trim(); 
        // On prend l'ID de la partie que l'utilisateur a écrit

        if (idPartie) { 
            // Si l'ID est valide, on continue

            recupererJoueurs(idPartie).then(({ html: joueursHtml, nomsJoueurs }) => {
                sectionAffichage.innerHTML = joueursHtml; 
                // On affiche la liste des joueurs

                Promise.all([
                    recupererParametres(idPartie), // On récupère les paramètres
                    recupererTours(idPartie, nomsJoueurs), // On récupère les résultats des tours
                    recupererResultatFinal(idPartie, nomsJoueurs) // On récupère le résultat final
                ]).then(resultats => {
                    sectionAffichage.innerHTML += resultats.join(''); 
                    // On affiche tous les résultats à la suite
                });
            });
        } else {
            alert('Veuillez entrer un identifiant de partie valide.'); 
            // Si l'ID est vide, on demande à l'utilisateur de le remplir
        }
    });
});