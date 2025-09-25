document.addEventListener('DOMContentLoaded', () => {
    // définir la base de l'URL de l'API
    const urlBaseApi = 'http://yams.iutrs.unistra.fr:3000'; 
    // cette URL est utilisée pour faire des requêtes à l'API du jeu, qui récupère les données de la partie.

    // sélectionner les éléments HTML nécessaires
    const champIdPartie = document.getElementById('game-id'); 
    // l'élément où l'utilisateur entre l'id de la partie.
    const boutonRécupérer = document.getElementById('fetch-game'); 
    // le bouton sur lequel on clique pour récupérer les données.
    const boutonTourPrécédent = document.getElementById('prev-round'); 
    // le bouton pour aller au tour précédent.
    const boutonTourSuivant = document.getElementById('next-round'); 
    // le bouton pour aller au tour suivant.
    const sectionJeu = document.querySelector('section.jeu'); 
    // la section HTML où les résultats du jeu sont affichés.
    const sectionRésultats = document.querySelector('section.résultats'); 
    // la section HTML où les résultats finaux seront affichés.

    let tourActuel = 1; // tour actuellement affiché
    let toursMax = 13; // nombre maximum de tours
    let idPartie = ''; // identifiant de la partie
    let nomsJoueurs = {}; // stocke les pseudos des joueurs

    // fonction pour afficher une image pour chaque dé
    function générerImagesDés(tableauDés) {
        const nomsDés = {
            1: "un",
            2: "deux",
            3: "trois",
            4: "quatre",
            5: "cinq",
            6: "six"
        };
        // un objet qui associe chaque valeur du dé à un nom d'image.

        return tableauDés.map(dé => {
            const nomDé = nomsDés[dé]; 
            // on récupère le nom de l'image pour la valeur du dé.
            if (!nomDé) return ''; // ignore les valeurs invalides
            return `<img src="./img/${nomDé}.png" alt="Dé ${nomDé}" style="width:30px; height:30px; margin-right:5px;">`; 
            // si le nom existe, on crée une balise <img> pour afficher l'image du dé.
        }).join('');
    }

    // fonction pour récupérer et afficher les joueurs
    function récupérerJoueurs(idPartie) {
        return fetch(`${urlBaseApi}/api/games/${idPartie}/players`) 
            // on fait une requête à l'API pour obtenir les joueurs de la partie avec l'id fourni.
            .then(réponse => réponse.json()) 
            // on transforme la réponse en JSON.
            .then(données => {
                nomsJoueurs = Object.fromEntries(données.map(joueur => [joueur.id, joueur.pseudo])); 
                // on crée un objet associant les id des joueurs à leurs pseudos.
            });
    }

    // fonction pour récupérer et afficher un tour
    function récupérerEtAfficherTour(numTour) {
        fetch(`${urlBaseApi}/api/games/${idPartie}/rounds/${numTour}`)
            // on demande à l'API les informations du tour en cours.
            .then(réponse => {
                return réponse.json(); 
                // sinon, on transforme la réponse en JSON.
            })
            .then(données => {
                // débogage des données du tour
                console.log("Données du tour:", données); 
                // affiche les données du tour dans la console pour le débogage.

                const résultatsHTML = données.results.map(resultat => {
                    const pseudo = nomsJoueurs[resultat.id_player];  
                    // on cherche le pseudo du joueur à partir de son id
                    const défi = resultat.challenge; 
                    // si le défi est spécifié
                    const score = resultat.score || 0; 
                    // si le score est spécifié, on l'affiche, sinon on met 0.
                    const imagesDés = générerImagesDés(resultat.dice || []); 
                    // on génère les images des dés en utilisant la fonction `générerImagesDés`.

                    return `
                        <div class="resultat-joueur">
                            <h4 style="text-decoration: underline;">${pseudo}:</h4>
                            <p>Défis: ${défi}</p>
                            <p>Dés:</p>
                            <div>${imagesDés}</div>
                            <p>Score: ${score}</p>
                        </div>
                    `;
                }).join('<hr>'); 
                // on crée un bloc HTML pour chaque joueur et ses résultats, séparé par des lignes de séparation.

                sectionJeu.innerHTML = `
                    <h3>Tour ${données.id}</h3>
                    ${résultatsHTML}
                `;
                // on affiche les résultats du tour dans la section "jeu".
            });
    }

    // fonction pour récupérer et afficher les résultats finaux
    function récupérerEtAfficherRésultatsFinaux() {
        fetch(`${urlBaseApi}/api/games/${idPartie}/final-result`)
            // on demande à l'API les résultats finaux de la partie.
            .then(réponse => réponse.json()) 
            // on transforme la réponse en JSON.
            .then(données => {
                // débogage des résultats finaux
                console.log("Données des résultats finaux:", données); 
                // affiche les résultats finaux dans la console pour le débogage.

                const résultatsFinauxHTML = données.map(resultat => {
                    const pseudo = nomsJoueurs[resultat.id_player]  
                    // on récupère le pseudo du joueur
                    const bonus = resultat.bonus || 0; 
                    // on récupère le bonus du joueur, ou 0 si ce n'est pas spécifié.
                    const score = resultat.score || 0; 
                    // on récupère le score final du joueur, ou 0 si ce n'est pas spécifié.

                    return `
                        <div class="résultat-final">
                            <h4 style="text-decoration: underline;">${pseudo}:</h4>
                            <p>Bonus: ${bonus}</p>
                            <p>Score Total: ${score}</p>
                        </div>
                    `;
                }).join('<hr>'); 
                // on crée un bloc HTML pour chaque joueur avec son bonus et son score final.

                sectionRésultats.innerHTML = `
                    <h3>Résultats Finaux</h3>
                    ${résultatsFinauxHTML}
                `;
                // on affiche les résultats finaux dans la section "résultats".
            });
    }

    // gérer le clic sur le bouton "valider"
    boutonRécupérer.addEventListener('click', () => {
        idPartie = champIdPartie.value.trim(); 
        // on récupère l'id de la partie que l'utilisateur a saisi.
        if (idPartie) {
            tourActuel = 1; // réinitialiser au premier tour
            récupérerJoueurs(idPartie).then(() => { 
                // si l'id est valide, on récupère les joueurs.
                récupérerEtAfficherTour(tourActuel); 
                // on affiche les résultats du premier tour.
                récupérerEtAfficherRésultatsFinaux(); 
                // on affiche les résultats finaux.
            });
        } else {
            alert('Veuillez entrer un identifiant de partie valide.'); 
            // si l'id est vide ou invalide, on affiche une alerte.
        }
    });

    // gérer le clic sur la flèche gauche pour afficher le tour précédent
    boutonTourPrécédent.addEventListener('click', () => {
        if (idPartie && tourActuel > 1) { 
            // si on est dans une partie et qu'on n'est pas au premier tour, on va au tour précédent.
            tourActuel--;
            récupérerEtAfficherTour(tourActuel); 
            // on affiche les résultats du tour précédent.
        }
    });

    // gérer le clic sur la flèche droite pour afficher le tour suivant
    boutonTourSuivant.addEventListener('click', () => {
        if (idPartie && tourActuel < toursMax) { 
            // si on est dans une partie et qu'on n'est pas au dernier tour, on va au tour suivant.
            tourActuel++;
            récupérerEtAfficherTour(tourActuel); 
            // on affiche les résultats du tour suivant.
        }
    });
});
