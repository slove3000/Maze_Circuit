using UnityEngine;
using System.Collections;

public class GizmosCircuitDebug : MonoBehaviour {

    /// <summary>
    /// Taille des spheres qui vont représenter les sommets du circuit
    /// </summary>
    public float SphereSize;

    /// <summary>
    /// Tableau des sommets du circuit en world point
    /// </summary>
    [HideInInspector]
    public Vector3[] PointsWorlds;

    /// <summary>
    /// Taille en pixel de la zone visible par la caméra
    /// </summary>
    [HideInInspector]
    public Rect rectCam;

    private LevelManager levelManager;

    public Vector2 ScreenSize;

    void OnDrawGizmos()
    {
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        var pointPixels = levelManager.PointsPixels;

        this.rectCam = Camera.main.pixelRect;
        if (pointPixels != null && pointPixels.Length > 0)
        {
            // Init du tableau des posistions en world point
            PointsWorlds = new Vector3[pointPixels.Length];

            // Dessine dans l'éditeur les points representant les sommets du circuit
            for (int i = 0; i < pointPixels.Length; i++)
            {
                // La conversion des pixel en world point dépend de la zone visible par la caméra
                // Cette zone est l'écran de jeu dont la taille peut varier d'un éditeur à l'autre
                // Pour que la conversion se fasse correctement indépendament de la taille de la zone de jeu 
                // Il faut mettre à l'echelle de la caméra les point en pixel utilisé. La résolution de base est 1920*1080

                Vector3 worldPoint = GameUtils.PixelToWorldPoint(pointPixels[i], Camera.main);

                // Ajout du worldPoint dans le tableau
                PointsWorlds[i] = worldPoint;

                // Dessine dans l'éditeur une sphere qui represente le point qui vient d'être calculé
                Gizmos.DrawSphere(worldPoint, SphereSize);
            }

            // Dessine dans l'éditeur une ligne blanche qui relie les sommet et représente la courbe de reference
            for (int j = 0; j < this.PointsWorlds.Length; j++)
            {
                // pour dessiner une ligne Unity à besoin d'un point de départ (le point actuel) et d'un point d'arrivé (le point suivant)
                int indiceSuivant;
                // Si le point actuel est le dernier point de la courbe le suivant est le premier pour que le circuit soit fermé
                if (j == this.PointsWorlds.Length - 1)
                {
                    indiceSuivant = 0;
                }
                else
                {
                    indiceSuivant = j + 1;
                }

                // Dessine la ligne blanche du point de départ au point d'arrivé
                Gizmos.DrawLine(this.PointsWorlds[j], this.PointsWorlds[indiceSuivant]);
            }
        }
        else
        {
            this.PointsWorlds = null;
        }
    }
}
