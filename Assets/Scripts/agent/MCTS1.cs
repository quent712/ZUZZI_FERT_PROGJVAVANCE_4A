using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCTS1
{
    private Node tree;
    public static float FREQUENCY = 0.4f; // Fréquence des actions du MCTS
    private int[] a; // matrice des touches directionnelles
    private float born;
    private CharacterRender render;
    private Model model;
    private int playerID = 1;
    private float simtime = 0.0f;

    public MCTS1(Model model) 
    {
        tree = new Node(new Register(0, 0));
        born = 0.0f;
        a = new int[4];
        this.model = model;
    }

    public bool trust()
    {
        foreach (Node n in tree.getPossibleAction())
        {
            if (n.data.b > 20)
            {
                // au moins un des noeuds doit être fiable (>20)
                return true;
            }
        }

        return false;
    }

    public Action interact() //SELECT BEST ACTION IN THREE
    {
        simtime = 0.0f;
        Player[] listplayer = model.getGameState()["PlayersInfo"] as Player[];

        if (listplayer.Length != 0)
        {
            // initialise les données du simulateur PROB HERE
            
            listplayer[1].health = 1;
            listplayer[0].health = 1;
            
            for (int i =0;i<50;i++)
            {
                compute(tree); //compute(tree,pokemonMe, pokemonAdv);
            }
        }


        // Appel horloge
        if (trust())
        {
            float max = float.MinValue;
            Action currentAction = Action.Undertermined;
            Node n = null;
            
            // Cherche la meilleure action conduisant à une victoire
            foreach (Node child in tree.getPossibleAction())
            {
                if (child.state != Action.Undertermined)
                {
                    if ((float) child.data.a / (float) child.data.b > max)
                    {
                        currentAction = child.state;
                        max = (float) child.data.a / (float) child.data.b;
                        n = child;
                    }
                }
            }
            if (n != null)
                tree = n;

            return currentAction; //On retourne l'action select

                // IMPORTANT ! On définie le nouveau noeud de base sur le noeud choisi
            
        }

        return Action.Undertermined;
    }




    void compute(Node action) //Simulation
    {
        //Debug.Log("In COMPUTE");
        Model simumodel = new Model(model);  //On copie le model actuel
        simumodel.inGameDeltaTime = 0.02f; //Les déplacements seront similaire à la réalité dans la simu
        GameSimul.copymodel = simumodel;
        
        
        //Tant que la simulation n'est pas achevée
        while (!GameSimul.isFinished)
        {
            simtime = simtime + Time.deltaTime;
            
            System.Array actions = GameSimul.GetNextPossibleAction(action);

            // Choisi une action au piff
            Action choice = (Action) GameSimul.GetRandomAction(actions);

            // Crée un node (donc une action) si elle n'existe pas encore
            // ou sinon prend celle trouvée
            Node exitanteNode = action.Exist(choice);
            if (exitanteNode == null)
            {
                Node selectedAction = action.AddChild(new Register(0, 0));
                selectedAction.parent = action;
                selectedAction.setState(choice);

                action = selectedAction; //La nouvelle action devient la current action
            }
            else
            {
                action = exitanteNode;   //la current action est l'action
            }

            // Lance l'action
            GameSimul.PlayAction(action);
           
            
        }

        // Applique des valeurs sur la feuille finale
        action.data.b = 1;
        if (GameSimul.finalSituation == 0) //gameover
            action.data.a = 0;
        else //win
            action.data.a = 1;

        // Retroprograpagation de l'action
        Node.Retropropagation(action);
        // Prépare le simulateur à une prochaine simulation
        GameSimul.Reset();

    }
}
