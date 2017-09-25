using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GizmosCircuitDebug))]
public class HandlesText : Editor
{
    /// <summary>
    /// Instance du script avec lequel les Handles doivent intéragir
    /// </summary>
    private GizmosCircuitDebug gizmoCircuit;

    void OnEnable()
    {
        gizmoCircuit = (GizmosCircuitDebug)target;
    }

    void OnSceneGUI()
    {
        // Récupere les sommets du circuit en world points
        Vector3[] points = gizmoCircuit.PointsWorlds;
        var xconv = gizmoCircuit.ScreenSize.x / 1920;
        var yconv = gizmoCircuit.ScreenSize.y / 1080;

        // Le but de ce Handles va être d'afficher dans l'éditeur le numéro du sommet en dessous de chaque sphere dessinée en Gizmos

        if (gizmoCircuit.PointsWorlds != null && gizmoCircuit.PointsWorlds.Length > 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                string msg = "";

                if (i != points.Length - 1)
                {
                    var p1 = new Vector2(points[i + 1].x, points[i + 1].y);
                    var p1p = Camera.main.WorldToScreenPoint(new Vector3(p1.x, p1.y, 0f));
                    var p1pixel = new Vector2((p1p.x / gizmoCircuit.rectCam.width) * 1920f, (p1p.y / gizmoCircuit.rectCam.height) * 1080f);
                    var p1cm = new Vector2(p1pixel.x * xconv, p1pixel.y * yconv);

                    var p2 = new Vector2(points[i].x, points[i].y);
                    var p2p = Camera.main.WorldToScreenPoint(new Vector3(p2.x, p2.y, 0f));
                    var p2pixel = new Vector2((p2p.x / gizmoCircuit.rectCam.width) * 1920f, (p2p.y / gizmoCircuit.rectCam.height) * 1080f);
                    var p2cm = new Vector2(p2pixel.x * xconv, p2pixel.y * yconv);

                    var distance = Vector2.Distance(p2cm, p1cm);
                    msg += i.ToString() + "\n" + distance.ToString();
                }
                else
                {
                    var p1 = new Vector2(points[0].x, points[0].y);
                    var p1p = Camera.main.WorldToScreenPoint(new Vector3(p1.x, p1.y, 0f));
                    var p1pixel = new Vector2((p1p.x / gizmoCircuit.rectCam.width) * 1920f, (p1p.y / gizmoCircuit.rectCam.height) * 1080f);
                    var p1cm = new Vector2(p1pixel.x * xconv, p1pixel.y * yconv);

                    var p2 = new Vector2(points[i].x, points[i].y);
                    var p2p = Camera.main.WorldToScreenPoint(new Vector3(p2.x, p2.y, 0f));
                    var p2pixel = new Vector2((p2p.x / gizmoCircuit.rectCam.width) * 1920f, (p2p.y / gizmoCircuit.rectCam.height) * 1080f);
                    var p2cm = new Vector2(p2pixel.x * xconv, p2pixel.y * yconv);

                    var distance = Vector2.Distance(p1cm, p2cm);
                    msg += i.ToString() + "\n" + distance.ToString();
                }

                Handles.Label(points[i], msg);
            } 
        }
    }
}
