using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(RawImage))]
public class PlayVideo : MonoBehaviour                                   //commentaire
{
    #region public variable
    /// <summary>
    /// variable public
    /// </summary>
    public MovieTexture movie;
    public enum TypeOfCinematic                                                        //type de pouvoir
    {
        Intro = 0,
        Chargement = 1,
        Cinematic1 = 2,
        Cinematic2 = 3,
        Ending = 4,
    }
    public TypeOfCinematic TP = TypeOfCinematic.Intro;
    public bool isPlaying = false;

    /// <summary>
    /// variable public HideInInspector
    /// </summary>
    //[HideInInspector] public bool tmp;

    #endregion

    #region private variable
    /// <summary>
    /// variable privé
    /// </summary>
    private AudioSource audio_movie;

    #endregion

    #region  initialisation

    /// <summary>
    /// Initialisation
    /// </summary>
    private void Start()                                                    //initialsiation
    {
        Debug.Log("ici?");

        if (!movie)
            return;
        Debug.Log("ici fuck ?");
        gameObject.GetComponent<RawImage>().texture = movie as MovieTexture;
        audio_movie = gameObject.GetComponent<AudioSource>();
        audio_movie.clip = movie.audioClip;
        stopVideo();
        movie.Play();
        audio_movie.Play();
        isPlaying = true;
        //if (TP == TypeOfCinematic.Intro && QOC)
        //    QOC.StartLoading("1_MainMenu", false);             //commence à charger le jeu
    }
    #endregion

    #region core script
    /// <summary>
    /// Initialisation
    /// </summary>
    public void stopVideo()                                             //test
    {
        if (!movie || !audio_movie)
            return;
        movie.Stop();
        audio_movie.Stop();
    }
    #endregion

    #region unity fonction and ending
    /// <summary>
    /// Exécuté chaque frame
    /// </summary>
    private void Update()                                                   //update
    {
        if (!movie)
        {
            this.enabled = false;
            return;
        }
            
        if (!movie.isPlaying && TP == TypeOfCinematic.Intro/* && QOC */&& isPlaying) //si on est dans l'introduction et que la vidéo est fini, activer la scène du menu précédemment chargé
        {
            isPlaying = false;
            Debug.Log("ici fini");
            GameManager.Instance.SceneManagerLocal.PlayNext();
            //QOC.ActivateScene();
        }

        /*if (Input.GetKeyDown(KeyCode.Space) && movie.isPlaying)
        {
            movie.Pause();
        }
        if (Input.GetKeyDown(KeyCode.Space) && !movie.isPlaying)
        {
            movie.Play();
        }*/

    }

    private void OnGUI()
    {
        if (!movie.isPlaying && isPlaying && TP != TypeOfCinematic.Intro)
        {
            isPlaying = false;
            Debug.Log("ici not playing !");
        }
    }
    #endregion
}
